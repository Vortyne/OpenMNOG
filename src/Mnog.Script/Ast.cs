namespace Mnog.Script;

// Syntax tree for MNOG script (.as files): an ActionScript 1 dialect whose operators have
// exact Flash semantics. See docs/SCRIPT.md.

public abstract record Expr;

/// <summary>Literal: string, double, bool, null (Value = null) or undefined (Value = Undefined.Value).</summary>
public sealed record Lit(object? Value) : Expr
{
    public static readonly Lit Null = new((object?)null);
    public static readonly Lit Undef = new(Undefined.Value);
}

public sealed class Undefined
{
    public static readonly Undefined Value = new();
    Undefined() { }
    public override string ToString() => "undefined";
}

/// <summary>Variable reference by plain identifier.</summary>
public sealed record Name(string Id) : Expr;

/// <summary>Register reference ($r0..$r255).</summary>
public sealed record Reg(int N) : Expr;

/// <summary>eval(nameExpr): variable lookup by computed or path name ("/:x", "../:y", "_root.z").</summary>
public sealed record Eval(Expr NameExpr) : Expr;

public sealed record Binary(string Op, Expr Left, Expr Right) : Expr;

/// <summary>Unary: "!", "-", "typeof".</summary>
public sealed record Unary(string Op, Expr Operand) : Expr;

/// <summary>Call of a built-in or user-defined function by name.</summary>
public sealed record Call(string Function, List<Expr> Args) : Expr;

/// <summary>obj.method(args) or obj[expr](args). Method may be an empty-string literal meaning "call obj itself".</summary>
public sealed record MethodCall(Expr Object, Expr Method, List<Expr> Args) : Expr;

/// <summary>obj.prop or obj[expr].</summary>
public sealed record Member(Expr Object, Expr Property) : Expr;

public sealed record Ternary(Expr Condition, Expr Then, Expr Else) : Expr;

public sealed record New(Expr Constructor, List<Expr> Args) : Expr;

public sealed record ArrayLit(List<Expr> Items) : Expr;

public sealed record ObjectLit(List<(Expr Key, Expr Value)> Items) : Expr;

public sealed record FunctionExpr(string Name, List<string> Params, List<Stmt> Body) : Expr;

/// <summary>Decompiler-internal: the iterator values an Enumerate action leaves on the stack.</summary>
public sealed record EnumerationMarker(Expr Object) : Expr;

/// <summary>Decompiler-internal: second reference to a duplicated stack value.</summary>
public sealed record Dup(Expr Original) : Expr;

public abstract record Stmt;

public sealed record ExprStmt(Expr Expr) : Stmt;

/// <summary>Assignment. Target is Name, Reg, Eval (printed as set(name, value)) or Member.</summary>
public sealed record Assign(Expr Target, Expr Value) : Stmt;

public sealed record VarDecl(string Name, Expr? Init) : Stmt;

public sealed record If(Expr Condition, List<Stmt> Then, List<Stmt>? Else) : Stmt;

public sealed record While(Expr Condition, List<Stmt> Body) : Stmt;

public sealed record DoWhile(List<Stmt> Body, Expr Condition) : Stmt;

/// <summary>for (variable in object) { body }: iterates the object's enumerable property names.</summary>
public sealed record ForIn(Expr Variable, Expr Object, List<Stmt> Body) : Stmt;

public sealed record Break : Stmt;

public sealed record Continue : Stmt;

public sealed record Return(Expr? Value) : Stmt;

public sealed record Goto(string Label) : Stmt;

public sealed record Label(string Name) : Stmt;

public sealed record FunctionDecl(FunctionExpr Function) : Stmt;

/// <summary>tellTarget(t) { body } ≡ setTarget(t); body; setTarget("").</summary>
public sealed record TellTarget(Expr Target, List<Stmt> Body) : Stmt;

/// <summary>A whole script file.</summary>
public sealed class ScriptFile
{
    /// <summary>SWF version of the source movie; governs value conversions (Flash 4 vs 5+ rules).</summary>
    public int Version = 4;
    /// <summary>
    /// Which opcodes the plain operators +, ==, &lt; (and !=, &gt;=) mean: 4 = Flash 4 numeric ops,
    /// 5 = Flash 5 typed ops. The other generation is reachable via __add4/__add2-style intrinsics.
    /// </summary>
    public int Operators = 4;

    public static int DefaultOperators(int version) => version >= 5 ? 5 : 4;
    public List<Stmt> Body = [];
}

public static class Builtins
{
    /// <summary>Statement-like built-ins the decompiler emits. Anything else is a user-defined function.</summary>
    public static readonly HashSet<string> Functions =
    [
        "play", "stop", "nextFrame", "prevFrame", "gotoFrame", "gotoAndPlay", "gotoAndStop", "gotoLabel",
        "stopAllSounds", "toggleHighQuality", "setTarget", "getURL", "loadMovie", "loadVariables",
        "getProperty", "setProperty", "duplicateMovieClip", "removeMovieClip", "trace", "startDrag", "stopDrag",
        "frameLoaded", "call", "random", "getTimer", "int", "length", "substring", "ord", "chr",
        "mblength", "mbsubstring", "mbord", "mbchr", "targetPath", "Number", "String",
        "__add4", "__eq4", "__lt4", "__add2", "__eq2", "__lt2", "__enumerate", "__delete", "__delete2",
        "__typeof",
    ];

    /// <summary>Property names usable in getProperty/setProperty, by Flash property index.</summary>
    public static readonly string[] Properties =
    [
        "_x", "_y", "_xscale", "_yscale", "_currentframe", "_totalframes", "_alpha", "_visible", "_width",
        "_height", "_rotation", "_target", "_framesloaded", "_name", "_droptarget", "_url", "_highquality",
        "_focusrect", "_soundbuftime", "_quality", "_xmouse", "_ymouse",
    ];

    public static readonly HashSet<string> Keywords =
    [
        "if", "else", "while", "do", "for", "break", "continue", "return", "function", "var", "new",
        "typeof", "delete", "true", "false", "null", "undefined", "and", "or", "not", "add", "eq", "ne",
        "lt", "gt", "le", "ge", "tellTarget", "goto", "in", "instanceof", "set", "eval", "NaN", "Infinity",
    ];

    /// <summary>Names usable after "." or as object literal keys: identifier syntax, keywords allowed.</summary>
    public static bool IsPropertyName(string s)
    {
        if (s.Length == 0 || !(char.IsAsciiLetter(s[0]) || s[0] == '_' || s[0] == '$')) return false;
        foreach (var c in s)
            if (!(char.IsAsciiLetterOrDigit(c) || c == '_' || c == '$')) return false;
        return true;
    }

    public static bool IsIdentifier(string s)
    {
        if (s.Length == 0 || Keywords.Contains(s)) return false;
        if (!(char.IsAsciiLetter(s[0]) || s[0] == '_' || s[0] == '$')) return false;
        foreach (var c in s)
            if (!(char.IsAsciiLetterOrDigit(c) || c == '_' || c == '$')) return false;
        // $rN is reserved for registers.
        if (s.Length > 2 && s[0] == '$' && s[1] == 'r' && s[2..].All(char.IsAsciiDigit)) return false;
        return true;
    }
}
