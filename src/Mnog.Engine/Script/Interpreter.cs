using System.Text;
using Mnog.Engine.Display;

namespace Mnog.Engine.Script;

public sealed class ScriptError(string message) : Exception(message);

/// <summary>Executes compiled chunks with ActionScript 1 semantics.</summary>
public sealed partial class Interpreter
{
    readonly Player _player;
    public readonly AsObject Global;
    public readonly AsObject ObjectPrototype;
    public readonly AsObject FunctionPrototype;
    public readonly AsObject ArrayPrototype;
    public readonly AsObject StringPrototype;
    public readonly AsObject NumberPrototype;
    public readonly AsObject BooleanPrototype;
    public readonly AsObject MovieClipPrototype;

    /// <summary>SWF version of the code currently running (drives value conversions).</summary>
    public int Version { get; private set; } = 6;
    int _depth;
    long _budget;
    const long InstructionBudget = 5_000_000;

    public Interpreter(Player player)
    {
        _player = player;
        ObjectPrototype = new AsObject();
        FunctionPrototype = new AsObject(ObjectPrototype);
        ArrayPrototype = new AsObject(ObjectPrototype);
        StringPrototype = new AsObject(ObjectPrototype);
        NumberPrototype = new AsObject(ObjectPrototype);
        BooleanPrototype = new AsObject(ObjectPrototype);
        MovieClipPrototype = new AsObject(ObjectPrototype);
        Global = new AsObject(ObjectPrototype);
        InstallNatives();
    }

    /// <summary>Execution state of one script or function call.</summary>
    sealed class Frame
    {
        public required Chunk Chunk;
        public required DisplayObject Base;
        public DisplayObject? Target;
        public Scope Scope = null!;
        public AsObject? Activation;
        public Value This;
        public readonly List<Value> Stack = new(16);
        public Value[]? Registers;

        public void Push(Value v) => Stack.Add(v);

        public Value Pop()
        {
            if (Stack.Count == 0) return Value.Undefined;
            var v = Stack[^1];
            Stack.RemoveAt(Stack.Count - 1);
            return v;
        }

        public Value Peek() => Stack.Count == 0 ? Value.Undefined : Stack[^1];
    }

    // ---- Entry points ----

    /// <summary>Runs a frame, button or clip-event script on a timeline.</summary>
    public void Run(Chunk chunk, MovieClip clip)
    {
        var f = new Frame { Chunk = chunk, Base = clip, Target = clip, This = Value.From(clip) };
        f.Scope = new Scope(clip, null);
        _budget = InstructionBudget;
        try
        {
            Execute(f);
        }
        catch (ScriptError e)
        {
            _player.Log($"script error in {chunk.Name}: {e.Message}");
        }
    }

    public Value Invoke(ScriptFunction fn, Value thisValue, Value[] args)
    {
        if (_depth > 200) throw new ScriptError("stack overflow (recursion too deep)");
        var activation = new AsObject(ObjectPrototype);
        var chunk = fn.Chunk;
        for (int i = 0; i < chunk.Params.Length; i++)
            activation.SetOwn(chunk.Params[i], i < args.Length ? args[i] : Value.Undefined);
        var argsArray = new AsArray(ArrayPrototype);
        argsArray.Items.AddRange(args);
        argsArray.SetOwn("callee", Value.From(fn));
        activation.SetOwn("arguments", Value.From(argsArray));

        DisplayObject baseClip = fn.Timeline != null && !fn.Timeline.Removed ? fn.Timeline : _player.Level0Root;
        var f = new Frame
        {
            Chunk = chunk,
            Base = baseClip,
            Target = baseClip,
            Activation = activation,
            This = thisValue,
            Scope = new Scope(activation, fn.Closure),
        };
        int savedVersion = Version;
        _depth++;
        try
        {
            return Execute(f);
        }
        finally
        {
            _depth--;
            Version = savedVersion;
        }
    }

    public Value CallFunction(Value fn, Value thisValue, Value[] args)
    {
        if (fn.Object is AsFunction af) return af.Call(this, thisValue, args);
        return Value.Undefined;
    }

    // ---- Main loop ----

    Value Execute(Frame f)
    {
        var code = f.Chunk.Code;
        int ver = f.Chunk.Version;
        Version = ver;
        int pc = 0;
        while (pc < code.Length)
        {
            if (--_budget < 0) throw new ScriptError("script ran too long (infinite loop?)");
            var ins = code[pc++];
            switch (ins.Op)
            {
                case Op.PushConst: f.Push((Value)ins.K!); break;
                case Op.PushReg: f.Push(f.Registers?[ins.A] ?? Value.Undefined); break;
                case Op.StoreReg: (f.Registers ??= new Value[256])[ins.A] = f.Pop(); break;
                case Op.Pop: f.Pop(); break;
                case Op.Dup: f.Push(f.Peek()); break;
                case Op.Swap: { var a = f.Pop(); var b = f.Pop(); f.Push(a); f.Push(b); break; }

                case Op.GetVar: f.Push(GetVariable(f, (string)ins.K!)); break;
                case Op.EvalVar: f.Push(GetVariable(f, f.Pop().ToStr(ver))); break;
                case Op.SetVar: SetVariable(f, (string)ins.K!, f.Pop()); break;
                case Op.SetVarDyn: { var v = f.Pop(); var n = f.Pop().ToStr(ver); SetVariable(f, n, v); break; }
                case Op.DefLocal: DefineLocal(f, (string)ins.K!, f.Pop(), true); break;
                case Op.DefLocalUndef: DefineLocal(f, (string)ins.K!, Value.Undefined, false); break;

                case Op.GetMember: { var name = f.Pop(); var obj = f.Pop(); f.Push(GetMember(obj, name.ToStr(ver))); break; }
                case Op.SetMember:
                    {
                        var v = f.Pop(); var name = f.Pop().ToStr(ver); var obj = f.Pop();
                        obj.Object?.Set(name, v, this);
                        break;
                    }

                case Op.CallBuiltin:
                    {
                        var args = PopN(f, ins.B);
                        var r = CallBuiltin(f, (Builtin)ins.A, args);
                        f.Push(r);
                        Version = ver;
                        break;
                    }
                case Op.CallFunc:
                    {
                        var args = PopArgs(f, ins.A);
                        var name = (string)ins.K!;
                        var fn = GetVariable(f, name);
                        if (fn.Object is not AsFunction)
                        {
                            _player.LogOnce($"call to undefined function {name}()");
                            f.Push(Value.Undefined);
                            break;
                        }
                        var thisV = f.Target != null ? Value.From(f.Target) : Value.Undefined;
                        f.Push(CallFunction(fn, thisV, args));
                        Version = ver;
                        break;
                    }
                case Op.CallMethod:
                    {
                        var method = f.Pop(); var obj = f.Pop(); var args = PopArgs(f, ins.A);
                        f.Push(CallMethod(obj, method, args, ver));
                        Version = ver;
                        break;
                    }
                case Op.New:
                    {
                        var ctor = f.Pop(); var args = PopArgs(f, ins.A);
                        f.Push(ctor.Object is AsFunction af ? af.Construct(this, args) : Value.Undefined);
                        Version = ver;
                        break;
                    }

                case Op.Add4: { var b = f.Pop().ToNumber(ver); var a = f.Pop().ToNumber(ver); f.Push(a + b); break; }
                case Op.Add5: { var b = f.Pop(); var a = f.Pop(); f.Push(Add5(a, b, ver)); break; }
                case Op.Sub: { var b = f.Pop().ToNumber(ver); var a = f.Pop().ToNumber(ver); f.Push(a - b); break; }
                case Op.Mul: { var b = f.Pop().ToNumber(ver); var a = f.Pop().ToNumber(ver); f.Push(a * b); break; }
                case Op.Div:
                    {
                        var b = f.Pop().ToNumber(ver); var a = f.Pop().ToNumber(ver);
                        f.Push(b == 0 && ver < 5 ? Value.From("#ERROR#") : Value.From(a / b));
                        break;
                    }
                case Op.Mod: { var b = f.Pop().ToNumber(ver); var a = f.Pop().ToNumber(ver); f.Push(a % b); break; }
                case Op.Eq4: { var b = f.Pop().ToNumber(ver); var a = f.Pop().ToNumber(ver); f.Push(Value.FromBool(a == b, ver)); break; }
                case Op.Eq5: { var b = f.Pop(); var a = f.Pop(); f.Push(Value.FromBool(LooseEquals(a, b, ver), ver)); break; }
                case Op.Lt4: { var b = f.Pop().ToNumber(ver); var a = f.Pop().ToNumber(ver); f.Push(Value.FromBool(a < b, ver)); break; }
                case Op.Lt5: { var b = f.Pop(); var a = f.Pop(); f.Push(Less5(a, b, ver)); break; }
                case Op.StrictEq: { var b = f.Pop(); var a = f.Pop(); f.Push(a.Equals(b)); break; }
                case Op.StrEq: { var b = f.Pop().ToStr(ver); var a = f.Pop().ToStr(ver); f.Push(Value.FromBool(a == b, ver)); break; }
                case Op.StrLt:
                    {
                        var b = f.Pop().ToStr(ver); var a = f.Pop().ToStr(ver);
                        f.Push(Value.FromBool(string.CompareOrdinal(a, b) < 0, ver));
                        break;
                    }
                case Op.StrAdd: { var b = f.Pop().ToStr(ver); var a = f.Pop().ToStr(ver); f.Push(a + b); break; }
                case Op.And4: { var b = f.Pop().ToBool(ver); var a = f.Pop().ToBool(ver); f.Push(Value.FromBool(a && b, ver)); break; }
                case Op.Or4: { var b = f.Pop().ToBool(ver); var a = f.Pop().ToBool(ver); f.Push(Value.FromBool(a || b, ver)); break; }
                case Op.Not: f.Push(Value.FromBool(!f.Pop().ToBool(ver), ver)); break;
                case Op.Neg: f.Push(-f.Pop().ToNumber(ver)); break;
                case Op.TypeOf: f.Push(f.Pop().TypeOf()); break;
                case Op.InstanceOf:
                    {
                        var ctor = f.Pop(); var obj = f.Pop();
                        var proto = ctor.Object?.Get("prototype", this).Object;
                        bool r = false;
                        for (var o = obj.Object?.Proto; o != null && proto != null; o = o.Proto)
                            if (o == proto) { r = true; break; }
                        f.Push(r);
                        break;
                    }
                case Op.BitAnd: { var b = f.Pop().ToInt32(ver); var a = f.Pop().ToInt32(ver); f.Push(a & b); break; }
                case Op.BitOr: { var b = f.Pop().ToInt32(ver); var a = f.Pop().ToInt32(ver); f.Push(a | b); break; }
                case Op.BitXor: { var b = f.Pop().ToInt32(ver); var a = f.Pop().ToInt32(ver); f.Push(a ^ b); break; }
                case Op.Shl: { var b = f.Pop().ToInt32(ver); var a = f.Pop().ToInt32(ver); f.Push(a << (b & 31)); break; }
                case Op.Shr: { var b = f.Pop().ToInt32(ver); var a = f.Pop().ToInt32(ver); f.Push(a >> (b & 31)); break; }
                case Op.UShr: { var b = f.Pop().ToInt32(ver); var a = f.Pop().ToInt32(ver); f.Push((double)((uint)a >> (b & 31))); break; }

                case Op.Jump: pc = ins.A; break;
                case Op.JumpIfFalse: if (!f.Pop().ToBool(ver)) pc = ins.A; break;
                case Op.JumpIfTrue: if (f.Pop().ToBool(ver)) pc = ins.A; break;

                case Op.Return: return f.Pop();
                case Op.MakeFunc:
                    f.Push(Value.From(new ScriptFunction(FunctionPrototype, f.Chunk.Functions[ins.A], f.Scope,
                        f.Base as MovieClip, ObjectPrototype)));
                    break;
                case Op.DefFunc:
                    {
                        var fn = f.Pop();
                        if (f.Activation != null) f.Activation.SetOwn((string)ins.K!, fn);
                        else (f.Target ?? f.Base).Set((string)ins.K!, fn, this);
                        break;
                    }
                case Op.InitArray:
                    {
                        var arr = new AsArray(ArrayPrototype);
                        arr.Items.AddRange(PopArgs(f, ins.A));
                        f.Push(Value.From(arr));
                        break;
                    }
                case Op.InitObject:
                    {
                        var obj = new AsObject(ObjectPrototype);
                        var pairs = new (string, Value)[ins.A];
                        for (int i = ins.A - 1; i >= 0; i--) { var v = f.Pop(); var k = f.Pop().ToStr(ver); pairs[i] = (k, v); }
                        foreach (var (k, v) in pairs) obj.SetOwn(k, v);
                        f.Push(Value.From(obj));
                        break;
                    }
                case Op.EnumStart:
                    {
                        var obj = f.Pop();
                        var keys = obj.Object?.Keys().ToList() ?? [];
                        if (obj.Object is MovieClip mc)
                            foreach (var c in mc.Children.Values.Reverse())
                                if (c.Name.Length > 0 && !keys.Contains(c.Name, AsObject.NameComparer)) keys.Add(c.Name);
                        f.Push(Value.From(new Enumerator(keys)));
                        break;
                    }
                case Op.EnumNext:
                    {
                        var it = (Enumerator)f.Peek().Object!;
                        if (it.Index < it.Keys.Count) f.Push(it.Keys[it.Index++]);
                        else { f.Pop(); pc = ins.A; }
                        break;
                    }
                case Op.SetTarget: SetTarget(f, f.Pop()); break;
                case Op.Delete:
                    {
                        var name = f.Pop().ToStr(ver); var obj = f.Pop();
                        f.Push(obj.Object?.DeleteOwn(name) ?? false);
                        break;
                    }
                case Op.Delete2:
                    {
                        var name = f.Pop().ToStr(ver);
                        bool r = false;
                        for (var s = f.Scope; s != null && !r; s = s.Parent) r = s.Object.DeleteOwn(name);
                        f.Push(r);
                        break;
                    }
                default: throw new ScriptError($"bad opcode {ins.Op}");
            }
        }
        return Value.Undefined;
    }

    sealed class Enumerator(List<string> keys) : AsObject
    {
        public readonly List<string> Keys = keys;
        public int Index;
    }

    static Value[] PopN(Frame f, int n)
    {
        var args = new Value[n];
        for (int i = n - 1; i >= 0; i--) args[i] = f.Pop();
        return args;
    }

    /// <summary>Arguments pushed last-to-first: the first argument is on top.</summary>
    static Value[] PopArgs(Frame f, int n)
    {
        var args = new Value[n];
        for (int i = 0; i < n; i++) args[i] = f.Pop();
        return args;
    }

    // ---- Operators ----

    static Value Add5(Value a, Value b, int ver)
    {
        var pa = a.ToPrimitive(ver);
        var pb = b.ToPrimitive(ver);
        if (pa.IsString || pb.IsString) return pa.ToStr(ver) + pb.ToStr(ver);
        return pa.ToNumber(ver) + pb.ToNumber(ver);
    }

    public static bool LooseEquals(Value a, Value b, int ver)
    {
        if (a.Kind == b.Kind) return a.Equals(b) && !(a.IsNumber && double.IsNaN(a.RawNumber));
        if (a.IsNullish && b.IsNullish) return true;
        if (a.IsNullish || b.IsNullish) return false;
        if (a.Kind == ValueKind.Bool) return LooseEquals(a.ToNumber(ver), b, ver);
        if (b.Kind == ValueKind.Bool) return LooseEquals(a, b.ToNumber(ver), ver);
        if (a.IsNumber && b.IsString) return a.RawNumber == b.ToNumber(ver);
        if (a.IsString && b.IsNumber) return a.ToNumber(ver) == b.RawNumber;
        if (a.IsObject && !b.IsObject) return LooseEquals(a.ToPrimitive(ver), b, ver);
        if (b.IsObject && !a.IsObject) return LooseEquals(a, b.ToPrimitive(ver), ver);
        return false;
    }

    static Value Less5(Value a, Value b, int ver)
    {
        var pa = a.ToPrimitive(ver, Hint.Number);
        var pb = b.ToPrimitive(ver, Hint.Number);
        if (pa.IsString && pb.IsString) return string.CompareOrdinal(pa.RawString, pb.RawString) < 0;
        double x = pa.ToNumber(ver), y = pb.ToNumber(ver);
        if (double.IsNaN(x) || double.IsNaN(y)) return Value.Undefined;
        return x < y;
    }

    // ---- Members and methods ----

    public Value GetMember(Value obj, string name)
    {
        switch (obj.Kind)
        {
            case ValueKind.Object: return obj.Object!.Get(name, this);
            case ValueKind.String:
                if (name.Equals("length", StringComparison.OrdinalIgnoreCase)) return obj.RawString.Length;
                return StringPrototype.Get(name, this);
            case ValueKind.Number: return NumberPrototype.Get(name, this);
            case ValueKind.Bool: return BooleanPrototype.Get(name, this);
            default: return Value.Undefined;
        }
    }

    Value CallMethod(Value obj, Value method, Value[] args, int ver)
    {
        if (method.IsUndefined || (method.IsString && method.RawString.Length == 0))
            return CallFunction(obj, Value.Undefined, args);
        var name = method.ToStr(ver);
        var fn = GetMember(obj, name);
        if (fn.Object is not AsFunction af)
        {
            _player.LogOnce($"call to undefined method {name}()");
            return Value.Undefined;
        }
        return af.Call(this, obj, args);
    }

    // ---- Variables, paths and targets ----

    /// <summary>Plain-name lookup along the scope chain, then globals.</summary>
    Value Lookup(Frame f, string name)
    {
        switch (name.ToLowerInvariant())
        {
            case "this": return f.This;
            case "_root": return Value.From(RootOf(f));
            case "_parent": return Value.From((f.Target ?? f.Base).Parent);
            case "_global": return Value.From(Global);
        }
        if (name.StartsWith("_level", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(name.AsSpan(6), out int lv))
            return _player.GetLevel(lv) is { } level ? Value.From(level) : Value.Undefined;

        for (var s = f.Scope; s != null; s = s.Parent)
            if (Has(s.Object, name)) return s.Object.Get(name, this);
        if (Has(Global, name)) return Global.Get(name, this);
        return Value.Undefined;
    }

    static bool Has(AsObject o, string name)
    {
        for (var p = o; p != null; p = p.Proto)
            if (p.HasOwn(name)) return true;
        return false;
    }

    MovieClip RootOf(Frame f) => (f.Target ?? f.Base).Root;

    Value GetVariable(Frame f, string path)
    {
        int colon = path.LastIndexOf(':');
        if (colon >= 0)
        {
            var target = ResolveTarget(f, path[..colon], f.Target ?? f.Base.Root);
            return target == null ? Value.Undefined : target.Get(path[(colon + 1)..], this);
        }
        if (path.Contains('/'))
            return ResolveTarget(f, path, f.Target ?? f.Base.Root) is { } clip ? Value.From(clip) : Value.Undefined;
        int dot = path.IndexOf('.');
        if (dot > 0)
        {
            var v = Lookup(f, path[..dot]);
            foreach (var part in path[(dot + 1)..].Split('.'))
            {
                if (v.IsNullish) return Value.Undefined;
                v = GetMember(v, part);
            }
            return v;
        }
        return Lookup(f, path);
    }

    void SetVariable(Frame f, string path, Value v)
    {
        int colon = path.LastIndexOf(':');
        if (colon >= 0)
        {
            var target = ResolveTarget(f, path[..colon], f.Target ?? f.Base.Root);
            target?.Set(path[(colon + 1)..], v, this);
            return;
        }
        int dot = path.IndexOf('.');
        if (dot > 0 && !path.Contains('/'))
        {
            var holder = GetVariable(f, path[..path.LastIndexOf('.')]);
            holder.Object?.Set(path[(path.LastIndexOf('.') + 1)..], v, this);
            return;
        }
        // Locals win; otherwise the variable lives on the current target timeline.
        for (var s = f.Scope; s != null; s = s.Parent)
        {
            if (s.Parent == null) break;
            if (s.Object.HasOwn(path)) { s.Object.Set(path, v, this); return; }
        }
        (f.Target ?? RootOf(f)).Set(path, v, this);
    }

    void DefineLocal(Frame f, string name, Value v, bool overwrite)
    {
        var holder = f.Activation ?? (AsObject)(f.Target ?? RootOf(f));
        if (overwrite || !holder.HasOwn(name)) holder.Set(name, v, this);
    }

    void SetTarget(Frame f, Value target)
    {
        if (target.IsString && target.RawString.Length == 0)
        {
            f.Target = f.Base;
        }
        else
        {
            var t = ResolveTargetValue(f, target, f.Base);
            if (t == null) _player.LogOnce($"tellTarget: target not found: {target.ToStr(Version)}");
            // Unknown targets swallow timeline actions; variables then go to _root (as Flash does).
            f.Target = t;
        }
        if (f.Activation == null) f.Scope = new Scope((AsObject?)f.Target ?? RootOf(f), null);
    }

    DisplayObject? ResolveTargetValue(Frame f, Value v, DisplayObject start) =>
        v.Object is DisplayObject d ? d : ResolveTarget(f, v.ToStr(Version), start);

    /// <summary>
    /// Resolves a target path in slash ("/a/b", "../c", "_level1/x") or dot ("_root.a", "this.b")
    /// syntax relative to <paramref name="start"/>.
    /// </summary>
    DisplayObject? ResolveTarget(Frame? f, string path, DisplayObject start)
    {
        if (path.Length == 0) return start;
        DisplayObject? cur = start;
        int i = 0;
        if (path[0] == '/')
        {
            cur = start.Root;
            i = 1;
        }
        var segments = new List<string>();
        foreach (var piece in path[i..].Split('/'))
        {
            if (piece == ".." || piece.Length == 0) { segments.Add(piece); continue; }
            segments.AddRange(piece.Split('.'));
        }
        bool first = true;
        foreach (var seg in segments)
        {
            if (cur == null) return null;
            if (seg.Length == 0) { first = false; continue; }
            if (seg == "..") cur = cur.Parent;
            else if (seg.Equals("_root", StringComparison.OrdinalIgnoreCase)) cur = cur.Root;
            else if (seg.Equals("_parent", StringComparison.OrdinalIgnoreCase)) cur = cur.Parent;
            else if (seg.Equals("this", StringComparison.OrdinalIgnoreCase)) cur = first && f != null ? f.This.Object as DisplayObject ?? cur : cur;
            else if (seg.StartsWith("_level", StringComparison.OrdinalIgnoreCase) && int.TryParse(seg.AsSpan(6), out int lv))
                cur = _player.GetLevel(lv);
            else if (cur is MovieClip mc && mc.ChildByName(seg) is { } child) cur = child;
            else if (cur.Get(seg, this).Object is DisplayObject viaVar) cur = viaVar;
            else if (first && f != null && Lookup(f, seg).Object is DisplayObject scoped) cur = scoped;
            else return null;
            first = false;
        }
        return cur;
    }

    /// <summary>Variable access for text fields bound to "name" or "path:name" relative to a clip.</summary>
    public Value GetVariableAt(MovieClip clip, string path)
    {
        var f = new Frame { Chunk = EmptyChunk, Base = clip, Target = clip, This = Value.From(clip) };
        f.Scope = new Scope(clip, null);
        return GetVariable(f, path);
    }

    public void SetVariableAt(MovieClip clip, string path, Value v)
    {
        var f = new Frame { Chunk = EmptyChunk, Base = clip, Target = clip, This = Value.From(clip) };
        f.Scope = new Scope(clip, null);
        SetVariable(f, path, v);
    }

    static readonly Chunk EmptyChunk = new() { Version = 6 };

    /// <summary>Resolves a target given as a string path or clip reference, relative to a clip.</summary>
    public DisplayObject? ResolveTargetFrom(DisplayObject start, Value target) =>
        target.Object is DisplayObject d ? d : ResolveTarget(null, target.ToStr(Version), start);

    // ---- Built-in actions ----

    MovieClip? TargetClip(Frame f) => f.Target as MovieClip;

    /// <summary>Splits "path:frame" (used by gotoAndPlay and call) into a clip and a frame value.</summary>
    (MovieClip?, Value) FrameTarget(Frame f, Value frame)
    {
        if (frame.IsString)
        {
            var s = frame.RawString;
            int colon = s.LastIndexOf(':');
            if (colon >= 0)
            {
                var clip = ResolveTarget(f, s[..colon], f.Target ?? f.Base) as MovieClip;
                return (clip, Value.From(s[(colon + 1)..]));
            }
        }
        return (TargetClip(f), frame);
    }

    Value CallBuiltin(Frame f, Builtin b, Value[] a)
    {
        int ver = f.Chunk.Version;
        switch (b)
        {
            case Builtin.Play: TargetClip(f)?.Play(); break;
            case Builtin.Stop: TargetClip(f)?.Stop(); break;
            case Builtin.NextFrame: TargetClip(f)?.NextFrame(); break;
            case Builtin.PrevFrame: TargetClip(f)?.PrevFrame(); break;
            // gotoFrame/gotoLabel: Flash 4 "Go To" — moves and stops.
            case Builtin.GotoFrame:
                if (TargetClip(f) is { } gf) { gf.GotoFrame((int)a[0].ToNumber(ver)); gf.Stop(); }
                break;
            case Builtin.GotoLabel:
                {
                    var clip = TargetClip(f);
                    if (clip == null) break;
                    if (clip.Timeline.Labels.TryGetValue(a[0].ToStr(ver), out int fr)) clip.GotoFrame(fr);
                    clip.Stop();
                    break;
                }
            case Builtin.GotoAndPlay:
            case Builtin.GotoAndStop:
                {
                    var (clip, frame) = FrameTarget(f, a[0]);
                    if (clip == null) break;
                    if (clip.FrameOf(frame, ver) is int fr) clip.GotoFrame(fr);
                    if (b == Builtin.GotoAndPlay) clip.Play(); else clip.Stop();
                    break;
                }
            case Builtin.Call:
                {
                    var (clip, frame) = FrameTarget(f, a[0]);
                    if (clip != null && clip.FrameOf(frame, ver) is int fr && fr >= 1 && fr <= clip.FrameCount)
                        foreach (var chunk in clip.Timeline.Frame(fr).Scripts) Run(chunk, clip);
                    _budget = Math.Max(_budget, 1);
                    break;
                }
            case Builtin.FrameLoaded:
                {
                    // Movies are loaded whole, and Flash counts every frame of a fully loaded movie as loaded, even
                    // past its end (the crab rides wait for frame 68 of 67).
                    var clip = TargetClip(f);
                    return Value.FromBool(clip != null && clip.FrameOf(a[0], ver) is int, ver);
                }
            case Builtin.StopAllSounds: _player.StopAllSounds(); break;
            case Builtin.ToggleHighQuality: break;
            case Builtin.SetTarget: SetTarget(f, a[0]); break;
            case Builtin.GetUrl:
                _player.GetUrl(a[0].ToStr(ver), a.Length > 1 ? a[1].ToStr(ver) : "", f.Target ?? f.Base);
                break;
            case Builtin.LoadMovie:
                _player.LoadMovie(a[0].ToStr(ver), a[1], f.Target ?? f.Base);
                break;
            case Builtin.LoadVariables:
                _player.LoadVariables(a[0].ToStr(ver), a[1], f.Target ?? f.Base);
                break;
            case Builtin.GetProperty:
                {
                    var t = PropertyTarget(f, a[0]);
                    return t == null ? Value.Undefined : t.GetProperty((int)a[1].ToNumber(ver), ver);
                }
            case Builtin.SetProperty:
                PropertyTarget(f, a[0])?.SetProperty((int)a[1].ToNumber(ver), a[2], ver);
                break;
            case Builtin.DuplicateMovieClip:
                if (ResolveTargetValue(f, a[0], f.Target ?? f.Base) is MovieClip src)
                    src.Duplicate(a[1].ToStr(ver), (int)a[2].ToNumber(ver));
                break;
            case Builtin.RemoveMovieClip:
                if (ResolveTargetValue(f, a[0], f.Target ?? f.Base) is MovieClip rm) RemoveMovieClip(rm);
                break;
            case Builtin.Trace: _player.Log("trace: " + a[0].ToStr(ver)); break;
            case Builtin.StartDrag:
                if (ResolveTargetValue(f, a[0], f.Target ?? f.Base) is MovieClip drag)
                {
                    SkiaSharp.SKRect? bounds = a.Length >= 6
                        ? new SkiaSharp.SKRect((float)a[2].ToNumber(ver), (float)a[3].ToNumber(ver), (float)a[4].ToNumber(ver), (float)a[5].ToNumber(ver))
                        : null;
                    _player.StartDrag(drag, a[1].ToBool(ver), bounds);
                }
                break;
            case Builtin.StopDrag: _player.StopDrag(); break;
            case Builtin.Random:
                {
                    int n = Value.DoubleToInt32(a[0].ToNumber(ver));
                    return n <= 0 ? 0 : _player.Random.Next(n);
                }
            case Builtin.GetTimer: return (double)_player.ElapsedMs;
            case Builtin.Int: return Value.DoubleToInt32(a[0].ToNumber(ver));
            case Builtin.Length: case Builtin.MbLength: return a[0].ToStr(ver).Length;
            case Builtin.Substring:
            case Builtin.MbSubstring:
                {
                    var s = a[0].ToStr(ver);
                    int index = Value.DoubleToInt32(a[1].ToNumber(ver)) - 1;
                    int count = Value.DoubleToInt32(a[2].ToNumber(ver));
                    if (index < 0) index = 0;
                    if (index >= s.Length) return "";
                    if (count < 0 || index + count > s.Length) count = s.Length - index;
                    return s.Substring(index, count);
                }
            case Builtin.Ord:
            case Builtin.MbOrd:
                {
                    var s = a[0].ToStr(ver);
                    if (s.Length == 0) return 0;
                    if (ver < 6 && b == Builtin.Ord)
                    {
                        var bytes = Ansi.Encoding.GetBytes(s[..1]);
                        return bytes.Length > 0 ? bytes[0] : 0;
                    }
                    return (double)s[0];
                }
            case Builtin.Chr:
            case Builtin.MbChr:
                {
                    int n = Value.DoubleToInt32(a[0].ToNumber(ver));
                    if (n == 0) return "";
                    if (ver < 6 && b == Builtin.Chr && n is > 0 and < 256)
                        return Ansi.Encoding.GetString([(byte)n]);
                    return ((char)n).ToString();
                }
            case Builtin.TargetPath: return a[0].Object is DisplayObject d ? d.DotPath : Value.Undefined;
            case Builtin.Number: return a[0].ToNumber(ver);
            case Builtin.String: return a[0].ToStr(ver);
        }
        return Value.Undefined;
    }

    DisplayObject? PropertyTarget(Frame f, Value target)
    {
        if (target.IsString && target.RawString.Length == 0) return f.Target;
        return ResolveTargetValue(f, target, f.Target ?? f.Base);
    }

    /// <summary>Only clips created by script (duplicate/attach) can be removed.</summary>
    public static void RemoveMovieClip(MovieClip clip)
    {
        if (clip.PlaceFrame == 0 && clip.Parent != null && clip.Level == null) clip.Parent.RemoveChild(clip);
    }

    public static string Escape(string s)
    {
        var sb = new StringBuilder();
        foreach (var c in s)
        {
            if (char.IsAsciiLetterOrDigit(c)) sb.Append(c);
            else if (c < 256) sb.Append('%').Append(((int)c).ToString("X2"));
            else foreach (var b in Encoding.UTF8.GetBytes(c.ToString())) sb.Append('%').Append(b.ToString("X2"));
        }
        return sb.ToString();
    }
}
