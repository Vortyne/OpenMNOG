using Mnog.Script;

namespace Mnog.Engine.Script;

public enum Op : byte
{
    PushConst, PushReg, StoreReg, Pop, Dup, Swap,
    GetVar, SetVar, EvalVar, SetVarDyn, DefLocal, DefLocalUndef,
    GetMember, SetMember,
    CallBuiltin, CallFunc, CallMethod, New,
    Add4, Add5, Sub, Mul, Div, Mod, Eq4, Eq5, Lt4, Lt5, StrictEq, StrEq, StrLt, StrAdd,
    And4, Or4, Not, Neg, TypeOf, InstanceOf,
    BitAnd, BitOr, BitXor, Shl, Shr, UShr,
    Jump, JumpIfFalse, JumpIfTrue,
    Return, MakeFunc, DefFunc, InitArray, InitObject,
    EnumStart, EnumNext,
    SetTarget, Delete, Delete2,
}

public enum Builtin : byte
{
    Play, Stop, NextFrame, PrevFrame, GotoFrame, GotoAndPlay, GotoAndStop, GotoLabel, StopAllSounds,
    ToggleHighQuality, SetTarget, GetUrl, LoadMovie, LoadVariables, GetProperty, SetProperty,
    DuplicateMovieClip, RemoveMovieClip, Trace, StartDrag, StopDrag, FrameLoaded, Call, Random, GetTimer,
    Int, Length, Substring, Ord, Chr, MbLength, MbSubstring, MbOrd, MbChr, TargetPath, Number, String,
}

public readonly record struct Instr(Op Op, int A = 0, int B = 0, object? K = null);

/// <summary>Compiled script: a frame/button/event script or a function body.</summary>
public sealed class Chunk
{
    public string Name = "";
    public int Version;
    public int Operators;
    public string[] Params = [];
    public Instr[] Code = [];
    public List<Chunk> Functions = [];
    public bool IsFunction;
}

public sealed class CompileException(string message) : Exception(message);

public sealed class Compiler
{
    static readonly Dictionary<string, Builtin> Builtins = new(StringComparer.Ordinal)
    {
        ["play"] = Builtin.Play, ["stop"] = Builtin.Stop, ["nextFrame"] = Builtin.NextFrame,
        ["prevFrame"] = Builtin.PrevFrame, ["gotoFrame"] = Builtin.GotoFrame, ["gotoAndPlay"] = Builtin.GotoAndPlay,
        ["gotoAndStop"] = Builtin.GotoAndStop, ["gotoLabel"] = Builtin.GotoLabel, ["stopAllSounds"] = Builtin.StopAllSounds,
        ["toggleHighQuality"] = Builtin.ToggleHighQuality, ["setTarget"] = Builtin.SetTarget, ["getURL"] = Builtin.GetUrl,
        ["loadMovie"] = Builtin.LoadMovie, ["loadVariables"] = Builtin.LoadVariables, ["getProperty"] = Builtin.GetProperty,
        ["setProperty"] = Builtin.SetProperty, ["duplicateMovieClip"] = Builtin.DuplicateMovieClip,
        ["removeMovieClip"] = Builtin.RemoveMovieClip, ["trace"] = Builtin.Trace, ["startDrag"] = Builtin.StartDrag,
        ["stopDrag"] = Builtin.StopDrag, ["frameLoaded"] = Builtin.FrameLoaded, ["call"] = Builtin.Call,
        ["random"] = Builtin.Random, ["getTimer"] = Builtin.GetTimer, ["int"] = Builtin.Int, ["length"] = Builtin.Length,
        ["substring"] = Builtin.Substring, ["ord"] = Builtin.Ord, ["chr"] = Builtin.Chr, ["mblength"] = Builtin.MbLength,
        ["mbsubstring"] = Builtin.MbSubstring, ["mbord"] = Builtin.MbOrd, ["mbchr"] = Builtin.MbChr,
        ["targetPath"] = Builtin.TargetPath, ["Number"] = Builtin.Number, ["String"] = Builtin.String,
    };

    readonly Chunk _chunk;
    readonly List<Instr> _code = [];
    readonly Dictionary<string, int> _labels = [];
    readonly List<(int At, string Label)> _gotos = [];
    readonly Stack<(List<int> Breaks, List<int> Continues, bool PopOnBreak)> _loops = new();

    Compiler(Chunk chunk) => _chunk = chunk;

    public static Chunk Compile(ScriptFile file, string name)
    {
        var chunk = new Chunk { Name = name, Version = file.Version, Operators = file.Operators };
        new Compiler(chunk).Body(file.Body);
        return chunk;
    }

    void Body(List<Stmt> body)
    {
        foreach (var s in body) Stmt(s);
        foreach (var (at, label) in _gotos)
        {
            if (!_labels.TryGetValue(label, out int target)) throw new CompileException($"undefined label {label}");
            _code[at] = _code[at] with { A = target };
        }
        _chunk.Code = _code.ToArray();
    }

    int Emit(Op op, int a = 0, int b = 0, object? k = null)
    {
        _code.Add(new Instr(op, a, b, k));
        return _code.Count - 1;
    }

    int Here => _code.Count;

    void Patch(int at) => _code[at] = _code[at] with { A = Here };

    // ---- Statements ----

    void Stmt(Stmt s)
    {
        switch (s)
        {
            case ExprStmt e:
                Expr(e.Expr);
                Emit(Op.Pop);
                break;
            case Assign a:
                Assign(a.Target, a.Value);
                break;
            case VarDecl v:
                if (v.Init != null) { Expr(v.Init); Emit(Op.DefLocal, k: v.Name); }
                else Emit(Op.DefLocalUndef, k: v.Name);
                break;
            case If i:
                {
                    Expr(i.Condition);
                    int jf = Emit(Op.JumpIfFalse);
                    foreach (var x in i.Then) Stmt(x);
                    if (i.Else is { Count: > 0 })
                    {
                        int je = Emit(Op.Jump);
                        Patch(jf);
                        foreach (var x in i.Else) Stmt(x);
                        Patch(je);
                    }
                    else Patch(jf);
                    break;
                }
            case While w:
                {
                    int top = Here;
                    Expr(w.Condition);
                    int jf = Emit(Op.JumpIfFalse);
                    var loop = PushLoop(false);
                    foreach (var x in w.Body) Stmt(x);
                    Emit(Op.Jump, top);
                    Patch(jf);
                    PopLoop(loop, Here, top);
                    break;
                }
            case DoWhile d:
                {
                    int top = Here;
                    var loop = PushLoop(false);
                    foreach (var x in d.Body) Stmt(x);
                    int cond = Here;
                    Expr(d.Condition);
                    Emit(Op.JumpIfTrue, top);
                    PopLoop(loop, Here, cond);
                    break;
                }
            case ForIn f:
                {
                    Expr(f.Object);
                    Emit(Op.EnumStart);
                    int next = Here;
                    int en = Emit(Op.EnumNext);
                    AssignFromStack(f.Variable);
                    var loop = PushLoop(true);
                    foreach (var x in f.Body) Stmt(x);
                    Emit(Op.Jump, next);
                    int breakPop = Here;
                    Emit(Op.Pop); // the iterator, when leaving via break
                    Patch(en);
                    PopLoop(loop, breakPop, next);
                    break;
                }
            case Break:
                if (_loops.Count == 0) throw new CompileException("break outside loop");
                _loops.Peek().Breaks.Add(Emit(Op.Jump));
                break;
            case Continue:
                if (_loops.Count == 0) throw new CompileException("continue outside loop");
                _loops.Peek().Continues.Add(Emit(Op.Jump));
                break;
            case Return r:
                if (r.Value != null) Expr(r.Value);
                else Emit(Op.PushConst, k: Value.Undefined);
                Emit(Op.Return);
                break;
            case Goto g:
                _gotos.Add((Emit(Op.Jump), g.Label));
                break;
            case Label l:
                _labels[l.Name] = Here;
                break;
            case FunctionDecl fd:
                Function(fd.Function);
                Emit(Op.DefFunc, k: fd.Function.Name);
                break;
            case TellTarget t:
                Expr(t.Target);
                Emit(Op.SetTarget);
                foreach (var x in t.Body) Stmt(x);
                Emit(Op.PushConst, k: Value.EmptyString);
                Emit(Op.SetTarget);
                break;
            default:
                throw new CompileException($"unsupported statement {s.GetType().Name}");
        }
    }

    (List<int>, List<int>, bool) PushLoop(bool popOnBreak)
    {
        var l = (new List<int>(), new List<int>(), popOnBreak);
        _loops.Push(l);
        return l;
    }

    void PopLoop((List<int> Breaks, List<int> Continues, bool) loop, int breakTarget, int continueTarget)
    {
        _loops.Pop();
        foreach (var b in loop.Breaks) _code[b] = _code[b] with { A = breakTarget };
        foreach (var c in loop.Continues) _code[c] = _code[c] with { A = continueTarget };
    }

    void Assign(Expr target, Expr value)
    {
        switch (target)
        {
            case Name n: Expr(value); Emit(Op.SetVar, k: n.Id); break;
            case Reg r: Expr(value); Emit(Op.StoreReg, r.N); break;
            case Eval ev: Expr(ev.NameExpr); Expr(value); Emit(Op.SetVarDyn); break;
            case Member m: Expr(m.Object); Expr(m.Property); Expr(value); Emit(Op.SetMember); break;
            default: throw new CompileException("invalid assignment target");
        }
    }

    /// <summary>Assigns the value on top of the stack (used by for-in).</summary>
    void AssignFromStack(Expr target)
    {
        switch (target)
        {
            case Name n: Emit(Op.SetVar, k: n.Id); break;
            case Reg r: Emit(Op.StoreReg, r.N); break;
            default: throw new CompileException("unsupported for-in variable");
        }
    }

    void Function(FunctionExpr f)
    {
        var chunk = new Chunk
        {
            Name = f.Name,
            Version = _chunk.Version,
            Operators = _chunk.Operators,
            Params = [.. f.Params],
            IsFunction = true,
        };
        new Compiler(chunk).Body(f.Body);
        _chunk.Functions.Add(chunk);
        Emit(Op.MakeFunc, _chunk.Functions.Count - 1);
    }

    // ---- Expressions ----

    void Expr(Expr e)
    {
        switch (e)
        {
            case Lit l: Emit(Op.PushConst, k: Const(l.Value)); break;
            case Name n: Emit(Op.GetVar, k: n.Id); break;
            case Reg r: Emit(Op.PushReg, r.N); break;
            case Eval ev: Expr(ev.NameExpr); Emit(Op.EvalVar); break;
            case Dup d: Expr(d.Original); break;
            case Unary u:
                if (u.Op == "-" && u.Operand is Lit { Value: double d0 }) { Emit(Op.PushConst, k: Value.From(-d0)); break; }
                Expr(u.Operand);
                Emit(u.Op switch { "!" => Op.Not, "-" => Op.Neg, "typeof" => Op.TypeOf, _ => throw new CompileException(u.Op) });
                break;
            case Binary b: Binary(b); break;
            case Ternary t:
                {
                    Expr(t.Condition);
                    int jf = Emit(Op.JumpIfFalse);
                    Expr(t.Then);
                    int je = Emit(Op.Jump);
                    Patch(jf);
                    Expr(t.Else);
                    Patch(je);
                    break;
                }
            case Call c: Call(c); break;
            case MethodCall m:
                PushArgsReversed(m.Args);
                Expr(m.Object);
                Expr(m.Method);
                Emit(Op.CallMethod, m.Args.Count);
                break;
            case Member m:
                Expr(m.Object);
                Expr(m.Property);
                Emit(Op.GetMember);
                break;
            case New n:
                PushArgsReversed(n.Args);
                Expr(n.Constructor);
                Emit(Op.New, n.Args.Count);
                break;
            case ArrayLit a:
                PushArgsReversed(a.Items);
                Emit(Op.InitArray, a.Items.Count);
                break;
            case ObjectLit o:
                foreach (var (k, v) in o.Items) { Expr(k); Expr(v); }
                Emit(Op.InitObject, o.Items.Count);
                break;
            case FunctionExpr f: Function(f); break;
            default: throw new CompileException($"unsupported expression {e.GetType().Name}");
        }
    }

    static Value Const(object? v) => v switch
    {
        null => Value.Null,
        Undefined => Value.Undefined,
        bool b => Value.From(b),
        double d => Value.From(d),
        string s => Value.From(s),
        _ => throw new CompileException("bad literal"),
    };

    /// <summary>AVM1 evaluates function, method and constructor arguments last-to-first.</summary>
    void PushArgsReversed(List<Expr> args)
    {
        for (int i = args.Count - 1; i >= 0; i--) Expr(args[i]);
    }

    void Binary(Binary b)
    {
        bool f4 = _chunk.Operators == 4;
        switch (b.Op)
        {
            case "&&":
            case "||":
                {
                    Expr(b.Left);
                    Emit(Op.Dup);
                    int j = Emit(b.Op == "&&" ? Op.JumpIfFalse : Op.JumpIfTrue);
                    Emit(Op.Pop);
                    Expr(b.Right);
                    Patch(j);
                    return;
                }
        }
        Expr(b.Left);
        Expr(b.Right);
        switch (b.Op)
        {
            case "+": Emit(f4 ? Op.Add4 : Op.Add5); break;
            case "-": Emit(Op.Sub); break;
            case "*": Emit(Op.Mul); break;
            case "/": Emit(Op.Div); break;
            case "%": Emit(Op.Mod); break;
            case "==": Emit(f4 ? Op.Eq4 : Op.Eq5); break;
            case "!=": Emit(f4 ? Op.Eq4 : Op.Eq5); Emit(Op.Not); break;
            case "<": Emit(f4 ? Op.Lt4 : Op.Lt5); break;
            case ">=": Emit(f4 ? Op.Lt4 : Op.Lt5); Emit(Op.Not); break;
            case ">": Emit(Op.Swap); Emit(f4 ? Op.Lt4 : Op.Lt5); break;
            case "<=": Emit(Op.Swap); Emit(f4 ? Op.Lt4 : Op.Lt5); Emit(Op.Not); break;
            case "===": Emit(Op.StrictEq); break;
            case "!==": Emit(Op.StrictEq); Emit(Op.Not); break;
            case "eq": Emit(Op.StrEq); break;
            case "ne": Emit(Op.StrEq); Emit(Op.Not); break;
            case "lt": Emit(Op.StrLt); break;
            case "ge": Emit(Op.StrLt); Emit(Op.Not); break;
            case "gt": Emit(Op.Swap); Emit(Op.StrLt); break;
            case "le": Emit(Op.Swap); Emit(Op.StrLt); Emit(Op.Not); break;
            case "add": Emit(Op.StrAdd); break;
            case "and": Emit(Op.And4); break;
            case "or": Emit(Op.Or4); break;
            case "&": Emit(Op.BitAnd); break;
            case "|": Emit(Op.BitOr); break;
            case "^": Emit(Op.BitXor); break;
            case "<<": Emit(Op.Shl); break;
            case ">>": Emit(Op.Shr); break;
            case ">>>": Emit(Op.UShr); break;
            case "instanceof": Emit(Op.InstanceOf); break;
            default: throw new CompileException($"unknown operator {b.Op}");
        }
    }

    void Call(Call c)
    {
        switch (c.Function)
        {
            // Operators from the other Flash generation, spelled as intrinsics.
            case "__add4": Expr(c.Args[0]); Expr(c.Args[1]); Emit(Op.Add4); return;
            case "__add2": Expr(c.Args[0]); Expr(c.Args[1]); Emit(Op.Add5); return;
            case "__eq4": Expr(c.Args[0]); Expr(c.Args[1]); Emit(Op.Eq4); return;
            case "__eq2": Expr(c.Args[0]); Expr(c.Args[1]); Emit(Op.Eq5); return;
            case "__lt4": Expr(c.Args[0]); Expr(c.Args[1]); Emit(Op.Lt4); return;
            case "__lt2": Expr(c.Args[0]); Expr(c.Args[1]); Emit(Op.Lt5); return;
            case "__gt2": Expr(c.Args[0]); Expr(c.Args[1]); Emit(Op.Swap); Emit(Op.Lt5); return;
            case "__delete": Expr(c.Args[0]); Expr(c.Args[1]); Emit(Op.Delete); return;
            case "__delete2": Expr(c.Args[0]); Emit(Op.Delete2); return;
        }
        if (Builtins.TryGetValue(c.Function, out var bi))
        {
            for (int i = 0; i < c.Args.Count; i++)
            {
                // getProperty(target, _x) / setProperty(target, _x, v): the property is a name, not a variable.
                if (i == 1 && bi is Builtin.GetProperty or Builtin.SetProperty && c.Args[1] is Name pn)
                {
                    int idx = Array.IndexOf(Mnog.Script.Builtins.Properties, pn.Id);
                    if (idx < 0) throw new CompileException($"unknown property {pn.Id}");
                    Emit(Op.PushConst, k: Value.From((double)idx));
                    continue;
                }
                Expr(c.Args[i]);
            }
            Emit(Op.CallBuiltin, (int)bi, c.Args.Count);
            return;
        }
        PushArgsReversed(c.Args);
        Emit(Op.CallFunc, c.Args.Count, k: c.Function);
    }
}
