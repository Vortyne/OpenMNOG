using System.Globalization;
using System.Text;

namespace Mnog.Script;

public sealed class ParseException(string message, int line) : Exception($"line {line}: {message}")
{
    public int Line { get; } = line;
}

enum Tok { Ident, Number, String, Punct, Eof }

readonly record struct Token(Tok Kind, string Text, double Number, int Line);

/// <summary>Parses MNOG script source (the language the converter prints) back into a syntax tree.</summary>
public sealed class Parser
{
    readonly List<Token> _toks;
    int _p;

    Parser(List<Token> toks) => _toks = toks;

    public static ScriptFile Parse(string source)
    {
        int version = 4;
        int? operators = null;
        var trimmed = source.TrimStart('﻿');
        // Header directives ("#version N", "#operators flashN") become blank lines to keep line numbers.
        var lines = trimmed.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var l = lines[i].Trim();
            if (l.StartsWith("#version"))
                version = int.Parse(l["#version".Length..].Trim(), CultureInfo.InvariantCulture);
            else if (l.StartsWith("#operators"))
                operators = l["#operators".Length..].Trim() switch
                {
                    "flash4" => 4,
                    "flash5" => 5,
                    var o => throw new ParseException($"unknown operator set '{o}'", i + 1),
                };
            else continue;
            lines[i] = "";
        }
        trimmed = string.Join('\n', lines);
        var p = new Parser(Lex(trimmed));
        var body = new List<Stmt>();
        while (!p.At(Tok.Eof)) p.Statement(body);
        return new ScriptFile { Version = version, Operators = operators ?? ScriptFile.DefaultOperators(version), Body = body };
    }

    // ---- Lexer ----

    static readonly string[] Puncts =
    [
        ">>>", "===", "!==", "<<", ">>", "<=", ">=", "==", "!=", "&&", "||", "++", "--",
        "{", "}", "(", ")", "[", "]", ";", ",", ".", ":", "?", "=", "+", "-", "*", "/", "%", "<", ">",
        "!", "&", "|", "^",
    ];

    static List<Token> Lex(string s)
    {
        var list = new List<Token>();
        int i = 0, line = 1;
        while (i < s.Length)
        {
            char c = s[i];
            if (c == '\n') { line++; i++; continue; }
            if (char.IsWhiteSpace(c)) { i++; continue; }
            if (c == '/' && i + 1 < s.Length && s[i + 1] == '/')
            {
                while (i < s.Length && s[i] != '\n') i++;
                continue;
            }
            if (c == '/' && i + 1 < s.Length && s[i + 1] == '*')
            {
                i += 2;
                while (i + 1 < s.Length && !(s[i] == '*' && s[i + 1] == '/')) { if (s[i] == '\n') line++; i++; }
                i += 2;
                continue;
            }
            if (char.IsAsciiLetter(c) || c == '_' || c == '$')
            {
                int st = i;
                while (i < s.Length && (char.IsAsciiLetterOrDigit(s[i]) || s[i] == '_' || s[i] == '$')) i++;
                list.Add(new Token(Tok.Ident, s[st..i], 0, line));
                continue;
            }
            if (char.IsAsciiDigit(c) || (c == '.' && i + 1 < s.Length && char.IsAsciiDigit(s[i + 1])))
            {
                int st = i;
                while (i < s.Length && (char.IsAsciiDigit(s[i]) || s[i] == '.')) i++;
                if (i < s.Length && (s[i] == 'e' || s[i] == 'E'))
                {
                    i++;
                    if (i < s.Length && (s[i] == '+' || s[i] == '-')) i++;
                    while (i < s.Length && char.IsAsciiDigit(s[i])) i++;
                }
                var text = s[st..i];
                list.Add(new Token(Tok.Number, text, double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture), line));
                continue;
            }
            if (c == '"' || c == '\'')
            {
                char q = c;
                i++;
                var sb = new StringBuilder();
                while (i < s.Length && s[i] != q)
                {
                    char ch = s[i++];
                    if (ch == '\\' && i < s.Length)
                    {
                        char e = s[i++];
                        switch (e)
                        {
                            case 'n': sb.Append('\n'); break;
                            case 'r': sb.Append('\r'); break;
                            case 't': sb.Append('\t'); break;
                            case 'x': sb.Append((char)Convert.ToInt32(s.Substring(i, 2), 16)); i += 2; break;
                            case 'u': sb.Append((char)Convert.ToInt32(s.Substring(i, 4), 16)); i += 4; break;
                            default: sb.Append(e); break;
                        }
                    }
                    else
                    {
                        if (ch == '\n') line++;
                        sb.Append(ch);
                    }
                }
                if (i >= s.Length) throw new ParseException("unterminated string", line);
                i++;
                list.Add(new Token(Tok.String, sb.ToString(), 0, line));
                continue;
            }
            bool matched = false;
            foreach (var p in Puncts)
            {
                if (string.CompareOrdinal(s, i, p, 0, p.Length) == 0)
                {
                    list.Add(new Token(Tok.Punct, p, 0, line));
                    i += p.Length;
                    matched = true;
                    break;
                }
            }
            if (!matched) throw new ParseException($"unexpected character '{c}'", line);
        }
        list.Add(new Token(Tok.Eof, "", 0, line));
        return list;
    }

    // ---- Helpers ----

    Token Peek(int k = 0) => _toks[Math.Min(_p + k, _toks.Count - 1)];
    Token Next() => _toks[_p++];
    bool At(Tok kind) => Peek().Kind == kind;
    bool AtPunct(string p, int k = 0) => Peek(k).Kind == Tok.Punct && Peek(k).Text == p;
    bool AtWord(string w, int k = 0) => Peek(k).Kind == Tok.Ident && Peek(k).Text == w;

    bool Accept(string p)
    {
        if (!AtPunct(p)) return false;
        _p++;
        return true;
    }

    void Expect(string p)
    {
        if (!Accept(p)) throw Error($"expected '{p}' but found '{Peek().Text}'");
    }

    void ExpectWord(string w)
    {
        if (!AtWord(w)) throw Error($"expected '{w}' but found '{Peek().Text}'");
        _p++;
    }

    string Ident()
    {
        var t = Next();
        if (t.Kind != Tok.Ident) throw new ParseException($"expected identifier but found '{t.Text}'", t.Line);
        return t.Text;
    }

    ParseException Error(string msg) => new(msg, Peek().Line);

    // ---- Statements ----

    List<Stmt> Block()
    {
        Expect("{");
        var list = new List<Stmt>();
        while (!AtPunct("}"))
        {
            if (At(Tok.Eof)) throw Error("unexpected end of file");
            Statement(list);
        }
        Expect("}");
        return list;
    }

    void Statement(List<Stmt> outp)
    {
        if (AtPunct("{")) { outp.AddRange(Block()); return; }
        if (Accept(";")) return;
        var t = Peek();
        if (t.Kind == Tok.Ident)
        {
            switch (t.Text)
            {
                case "var":
                    {
                        _p++;
                        var name = Ident();
                        Expr? init = Accept("=") ? Expression() : null;
                        Expect(";");
                        outp.Add(new VarDecl(name, init));
                        return;
                    }
                case "if":
                    outp.Add(IfStatement());
                    return;
                case "while":
                    {
                        _p++;
                        Expect("(");
                        var c = Expression();
                        Expect(")");
                        outp.Add(new While(c, Block()));
                        return;
                    }
                case "do":
                    {
                        _p++;
                        var body = Block();
                        ExpectWord("while");
                        Expect("(");
                        var c = Expression();
                        Expect(")");
                        Expect(";");
                        outp.Add(new DoWhile(body, c));
                        return;
                    }
                case "for":
                    {
                        _p++;
                        Expect("(");
                        bool declare = AtWord("var");
                        if (declare) _p++;
                        var v = Unary();
                        ExpectWord("in");
                        var obj = Expression();
                        Expect(")");
                        var body = Block();
                        if (declare && v is Name n) outp.Add(new VarDecl(n.Id, null));
                        outp.Add(new ForIn(v, obj, body));
                        return;
                    }
                case "break": _p++; Expect(";"); outp.Add(new Break()); return;
                case "continue": _p++; Expect(";"); outp.Add(new Continue()); return;
                case "return":
                    {
                        _p++;
                        Expr? v = AtPunct(";") ? null : Expression();
                        Expect(";");
                        outp.Add(new Return(v));
                        return;
                    }
                case "goto":
                    {
                        _p++;
                        var l = Ident();
                        Expect(";");
                        outp.Add(new Goto(l));
                        return;
                    }
                case "function" when Peek(1).Kind == Tok.Ident:
                    outp.Add(new FunctionDecl(FunctionLiteral()));
                    return;
                case "tellTarget":
                    {
                        _p++;
                        Expect("(");
                        var target = Expression();
                        Expect(")");
                        outp.Add(new TellTarget(target, Block()));
                        return;
                    }
                case "set" when AtPunct("(", 1):
                    {
                        _p += 2;
                        var name = Expression();
                        Expect(",");
                        var value = Expression();
                        Expect(")");
                        Expect(";");
                        outp.Add(new Assign(new Eval(name), value));
                        return;
                    }
            }
            if (AtPunct(":", 1) && !Builtins.Keywords.Contains(t.Text))
            {
                _p += 2;
                outp.Add(new Label(t.Text));
                return;
            }
        }

        var e = Expression();
        if (Accept("="))
        {
            var v = Expression();
            Expect(";");
            outp.Add(new Assign(CheckTarget(e), v));
            return;
        }
        if (Accept("++"))
        {
            Expect(";");
            outp.Add(new Assign(CheckTarget(e), new Binary("+", new Call("Number", [e]), new Lit(1.0))));
            return;
        }
        if (Accept("--"))
        {
            Expect(";");
            outp.Add(new Assign(CheckTarget(e), new Binary("-", e, new Lit(1.0))));
            return;
        }
        Expect(";");
        outp.Add(new ExprStmt(e));
    }

    Expr CheckTarget(Expr e) =>
        e is Name or Reg or Member or Eval ? e : throw Error("invalid assignment target");

    If IfStatement()
    {
        ExpectWord("if");
        Expect("(");
        var c = Expression();
        Expect(")");
        var then = Block();
        List<Stmt>? els = null;
        if (AtWord("else"))
        {
            _p++;
            els = AtWord("if") ? [IfStatement()] : Block();
        }
        return new If(c, then, els);
    }

    FunctionExpr FunctionLiteral()
    {
        ExpectWord("function");
        string name = At(Tok.Ident) ? Ident() : "";
        Expect("(");
        var ps = new List<string>();
        if (!AtPunct(")"))
        {
            do ps.Add(Ident()); while (Accept(","));
        }
        Expect(")");
        return new FunctionExpr(name, ps, Block());
    }

    // ---- Expressions ----

    public Expr Expression() => Ternary();

    Expr Ternary()
    {
        var c = BinaryExpr(3);
        if (!Accept("?")) return c;
        var a = Ternary();
        Expect(":");
        var b = Ternary();
        return new Ternary(c, a, b);
    }

    static readonly string[] WordOps = ["or", "and", "eq", "ne", "lt", "gt", "le", "ge", "add", "instanceof"];

    string? BinaryOpAt()
    {
        var t = Peek();
        if (t.Kind == Tok.Punct && t.Text is "||" or "&&" or "|" or "^" or "&" or "==" or "!=" or "===" or "!==" or
            "<" or ">" or "<=" or ">=" or "<<" or ">>" or ">>>" or "+" or "-" or "*" or "/" or "%")
            return t.Text;
        if (t.Kind == Tok.Ident && WordOps.Contains(t.Text)) return t.Text;
        return null;
    }

    Expr BinaryExpr(int minPrec)
    {
        var left = Unary();
        while (true)
        {
            var op = BinaryOpAt();
            if (op == null) break;
            int prec = Printer.Precedence(op);
            if (prec < minPrec) break;
            _p++;
            var right = BinaryExpr(prec + 1);
            left = new Binary(op, left, right);
        }
        return left;
    }

    Expr Unary()
    {
        if (Accept("!")) return new Unary("!", Unary());
        if (Accept("-")) return new Unary("-", Unary());
        if (AtWord("typeof")) { _p++; return new Unary("typeof", Unary()); }
        if (AtWord("not")) { _p++; return new Unary("!", Unary()); }
        return Postfix(Primary());
    }

    Expr Postfix(Expr e)
    {
        while (true)
        {
            if (Accept("."))
            {
                var name = Ident();
                if (AtPunct("(")) e = new MethodCall(e, new Lit(name), Args());
                else e = new Member(e, new Lit(name));
            }
            else if (Accept("["))
            {
                var idx = Expression();
                Expect("]");
                if (AtPunct("(")) e = new MethodCall(e, idx, Args());
                else e = new Member(e, idx);
            }
            else if (AtPunct("(") && e is not Name)
            {
                // Calling a value: obj(args) is printed for CallMethod with an empty method name.
                e = new MethodCall(e, new Lit(""), Args());
            }
            else return e;
        }
    }

    List<Expr> Args()
    {
        Expect("(");
        var list = new List<Expr>();
        if (!AtPunct(")"))
        {
            do list.Add(Ternary()); while (Accept(","));
        }
        Expect(")");
        return list;
    }

    Expr Primary()
    {
        var t = Peek();
        switch (t.Kind)
        {
            case Tok.Number: _p++; return new Lit(t.Number);
            case Tok.String: _p++; return new Lit(t.Text);
            case Tok.Punct:
                if (Accept("("))
                {
                    var e = Expression();
                    Expect(")");
                    return e;
                }
                if (Accept("["))
                {
                    var items = new List<Expr>();
                    if (!AtPunct("]"))
                    {
                        do items.Add(Ternary()); while (Accept(","));
                    }
                    Expect("]");
                    return new ArrayLit(items);
                }
                if (Accept("{"))
                {
                    var items = new List<(Expr, Expr)>();
                    if (!AtPunct("}"))
                    {
                        do
                        {
                            var kt = Next();
                            Expr key = kt.Kind switch
                            {
                                Tok.Ident or Tok.String => new Lit(kt.Text),
                                Tok.Number => new Lit(kt.Number),
                                _ => throw new ParseException("bad object key", kt.Line),
                            };
                            Expect(":");
                            items.Add((key, Ternary()));
                        } while (Accept(","));
                    }
                    Expect("}");
                    return new ObjectLit(items);
                }
                throw Error($"unexpected '{t.Text}'");
            case Tok.Ident:
                switch (t.Text)
                {
                    case "true": _p++; return new Lit(true);
                    case "false": _p++; return new Lit(false);
                    case "null": _p++; return Lit.Null;
                    case "undefined": _p++; return Lit.Undef;
                    case "NaN": _p++; return new Lit(double.NaN);
                    case "Infinity": _p++; return new Lit(double.PositiveInfinity);
                    case "function": return FunctionLiteral();
                    case "new":
                        {
                            _p++;
                            Expr ctor = new Name(Ident());
                            while (Accept(".")) ctor = new Member(ctor, new Lit(Ident()));
                            return new New(ctor, Args());
                        }
                    case "eval" when AtPunct("(", 1):
                        {
                            _p += 2;
                            var n = Expression();
                            Expect(")");
                            return new Eval(n);
                        }
                }
                _p++;
                if (t.Text.Length > 2 && t.Text.StartsWith("$r") && int.TryParse(t.Text.AsSpan(2), out int reg))
                    return new Reg(reg);
                if (AtPunct("(")) return new Call(t.Text, Args());
                return new Name(t.Text);
        }
        throw Error("unexpected end of file");
    }
}
