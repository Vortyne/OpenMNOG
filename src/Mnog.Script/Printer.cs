using System.Globalization;
using System.Text;

namespace Mnog.Script;

public sealed class Printer
{
    readonly StringBuilder _sb = new();
    int _ind;

    public static string Print(ScriptFile file)
    {
        var p = new Printer();
        p._sb.Append("#version ").Append(file.Version).Append('\n');
        if (file.Operators != ScriptFile.DefaultOperators(file.Version))
            p._sb.Append("#operators flash").Append(file.Operators).Append('\n');
        foreach (var s in file.Body) p.Stmt(s);
        return p._sb.ToString();
    }

    public static string Expr(Expr e)
    {
        var p = new Printer();
        p.Expr(e, 0);
        return p._sb.ToString();
    }

    void Indent(int n) => _sb.Append(' ', n * 4);

    void Block(List<Stmt> body)
    {
        _sb.Append("{\n");
        _ind++;
        foreach (var s in body) Stmt(s);
        _ind--;
        Indent(_ind);
        _sb.Append('}');
    }

    /// <summary>x = Number(x) + 1 prints as x++ and x = x - 1 as x-- (the language defines them that way).</summary>
    static string? IncDec(Assign a)
    {
        if (a.Target is not (Name or Reg)) return null;
        if (a.Value is Binary { Op: "+", Left: Call { Function: "Number", Args: [var x] }, Right: Lit { Value: 1.0 } } && x == a.Target)
            return "++";
        if (a.Value is Binary { Op: "-", Left: var y, Right: Lit { Value: 1.0 } } && y == a.Target)
            return "--";
        return null;
    }

    void Stmt(Stmt s)
    {
        if (s is Label lbl)
        {
            // Labels sit one level out so they stand out.
            Indent(Math.Max(0, _ind - 1));
            _sb.Append(lbl.Name).Append(":\n");
            return;
        }
        Indent(_ind);
        switch (s)
        {
            case ExprStmt e: Expr(e.Expr, 0); _sb.Append(";\n"); break;
            case Assign a:
                if (a.Target is Eval ev)
                {
                    _sb.Append("set(");
                    Expr(ev.NameExpr, 2);
                    _sb.Append(", ");
                    Expr(a.Value, 2);
                    _sb.Append(");\n");
                }
                else if (IncDec(a) is string op)
                {
                    Expr(a.Target, 14);
                    _sb.Append(op).Append(";\n");
                }
                else
                {
                    Expr(a.Target, 14);
                    _sb.Append(" = ");
                    Expr(a.Value, 0);
                    _sb.Append(";\n");
                }
                break;
            case VarDecl v:
                _sb.Append("var ").Append(v.Name);
                if (v.Init != null) { _sb.Append(" = "); Expr(v.Init, 0); }
                _sb.Append(";\n");
                break;
            case If i:
                _sb.Append("if (");
                Expr(i.Condition, 0);
                _sb.Append(") ");
                Block(i.Then);
                var els = i.Else;
                while (els is { Count: > 0 })
                {
                    if (els is [If nested])
                    {
                        _sb.Append(" else if (");
                        Expr(nested.Condition, 0);
                        _sb.Append(") ");
                        Block(nested.Then);
                        els = nested.Else;
                    }
                    else
                    {
                        _sb.Append(" else ");
                        Block(els);
                        break;
                    }
                }
                _sb.Append('\n');
                break;
            case While w:
                _sb.Append("while (");
                Expr(w.Condition, 0);
                _sb.Append(") ");
                Block(w.Body);
                _sb.Append('\n');
                break;
            case DoWhile d:
                _sb.Append("do ");
                Block(d.Body);
                _sb.Append(" while (");
                Expr(d.Condition, 0);
                _sb.Append(");\n");
                break;
            case ForIn f:
                _sb.Append("for (");
                Expr(f.Variable, 0);
                _sb.Append(" in ");
                Expr(f.Object, 0);
                _sb.Append(") ");
                Block(f.Body);
                _sb.Append('\n');
                break;
            case Break: _sb.Append("break;\n"); break;
            case Continue: _sb.Append("continue;\n"); break;
            case Return r:
                _sb.Append("return");
                if (r.Value is not null and not Lit { Value: Undefined }) { _sb.Append(' '); Expr(r.Value, 0); }
                _sb.Append(";\n");
                break;
            case Goto g: _sb.Append("goto ").Append(g.Label).Append(";\n"); break;
            case FunctionDecl f:
                Function(f.Function);
                _sb.Append('\n');
                break;
            case TellTarget t:
                _sb.Append("tellTarget (");
                Expr(t.Target, 0);
                _sb.Append(") ");
                Block(t.Body);
                _sb.Append('\n');
                break;
            default: throw new NotSupportedException(s.GetType().Name);
        }
    }

    void Function(FunctionExpr f)
    {
        _sb.Append("function");
        if (f.Name.Length > 0) _sb.Append(' ').Append(f.Name);
        _sb.Append('(').Append(string.Join(", ", f.Params)).Append(") ");
        Block(f.Body);
    }

    // Precedence levels (higher binds tighter).
    public static int Precedence(string op) => op switch
    {
        "||" or "or" => 3,
        "&&" or "and" => 4,
        "|" => 5,
        "^" => 6,
        "&" => 7,
        "==" or "!=" or "eq" or "ne" or "===" or "!==" => 8,
        "<" or ">" or "<=" or ">=" or "lt" or "gt" or "le" or "ge" or "instanceof" => 9,
        "<<" or ">>" or ">>>" => 10,
        "+" or "-" or "add" => 11,
        "*" or "/" or "%" => 12,
        _ => throw new ArgumentException(op),
    };

    void Open(int prec, int minPrec) { if (prec < minPrec) _sb.Append('('); }
    void Close(int prec, int minPrec) { if (prec < minPrec) _sb.Append(')'); }

    void Expr(Expr e, int minPrec)
    {
        switch (e)
        {
            case Lit l: _sb.Append(Literal(l.Value)); break;
            case Name n: _sb.Append(n.Id); break;
            case Reg r: _sb.Append("$r").Append(r.N); break;
            case Eval ev: _sb.Append("eval("); Expr(ev.NameExpr, 2); _sb.Append(')'); break;
            case Dup d: Expr(d.Original, minPrec); break;
            case Binary b:
                {
                    int p = Precedence(b.Op);
                    Open(p, minPrec);
                    // Left-associative: the right operand needs a strictly higher precedence.
                    Expr(b.Left, p);
                    _sb.Append(' ').Append(b.Op).Append(' ');
                    Expr(b.Right, p + 1);
                    Close(p, minPrec);
                    break;
                }
            case Unary u:
                Open(13, minPrec);
                _sb.Append(u.Op == "typeof" ? "typeof " : u.Op);
                Expr(u.Operand, 13);
                Close(13, minPrec);
                break;
            case Ternary t:
                Open(2, minPrec);
                Expr(t.Condition, 3);
                _sb.Append(" ? ");
                Expr(t.Then, 2);
                _sb.Append(" : ");
                Expr(t.Else, 2);
                Close(2, minPrec);
                break;
            case Call c:
                _sb.Append(c.Function);
                Args(c.Args);
                break;
            case MethodCall m:
                Expr(m.Object, 14);
                if (m.Method is Lit { Value: string ms })
                {
                    if (ms.Length > 0) Property(ms);
                }
                else { _sb.Append('['); Expr(m.Method, 0); _sb.Append(']'); }
                Args(m.Args);
                break;
            case Member m:
                Expr(m.Object, 14);
                if (m.Property is Lit { Value: string ps }) Property(ps);
                else { _sb.Append('['); Expr(m.Property, 0); _sb.Append(']'); }
                break;
            case New n:
                _sb.Append("new ");
                Expr(n.Constructor, 14);
                Args(n.Args);
                break;
            case ArrayLit a:
                _sb.Append('[');
                for (int i = 0; i < a.Items.Count; i++)
                {
                    if (i > 0) _sb.Append(", ");
                    Expr(a.Items[i], 2);
                }
                _sb.Append(']');
                break;
            case ObjectLit o:
                ObjectLiteral(o);
                break;
            case FunctionExpr f: Function(f); break;
            default: throw new NotSupportedException(e.GetType().Name);
        }
    }

    void Property(string name)
    {
        if (Builtins.IsPropertyName(name)) _sb.Append('.').Append(name);
        else _sb.Append('[').Append(Quote(name)).Append(']');
    }

    void ObjectLiteral(ObjectLit o)
    {
        if (o.Items.Count == 0) { _sb.Append("{}"); return; }
        // Objects holding functions go one entry per line.
        bool multiline = o.Items.Any(kv => kv.Value is FunctionExpr or ObjectLit { Items.Count: > 0 });
        _sb.Append('{');
        if (multiline) { _sb.Append('\n'); _ind++; }
        for (int i = 0; i < o.Items.Count; i++)
        {
            var (key, value) = o.Items[i];
            if (multiline) Indent(_ind);
            if (key is Lit { Value: string k } && Builtins.IsPropertyName(k)) _sb.Append(k);
            else Expr(key, 2);
            _sb.Append(": ");
            Expr(value, 2);
            if (i < o.Items.Count - 1) _sb.Append(',');
            if (multiline) _sb.Append('\n');
            else if (i < o.Items.Count - 1) _sb.Append(' ');
        }
        if (multiline) { _ind--; Indent(_ind); }
        _sb.Append('}');
    }

    void Args(List<Expr> args)
    {
        _sb.Append('(');
        for (int i = 0; i < args.Count; i++)
        {
            if (i > 0) _sb.Append(", ");
            Expr(args[i], 2);
        }
        _sb.Append(')');
    }

    public static string Literal(object? v) => v switch
    {
        null => "null",
        Undefined => "undefined",
        bool b => b ? "true" : "false",
        string s => Quote(s),
        double d => Number(d),
        _ => throw new NotSupportedException(v.GetType().Name),
    };

    public static string Number(double d)
    {
        if (double.IsNaN(d)) return "NaN";
        if (double.IsPositiveInfinity(d)) return "Infinity";
        if (double.IsNegativeInfinity(d)) return "-Infinity";
        if (d == 0 && double.IsNegative(d)) return "-0";
        return d.ToString("R", CultureInfo.InvariantCulture);
    }

    public static string Quote(string s)
    {
        var sb = new StringBuilder("\"");
        foreach (var c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20 || c == 0x7F) sb.Append("\\x").Append(((int)c).ToString("X2"));
                    else sb.Append(c);
                    break;
            }
        }
        return sb.Append('"').ToString();
    }
}
