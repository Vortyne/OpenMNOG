using System.Globalization;

namespace Mnog.Engine.Script;

public enum ValueKind : byte { Undefined, Null, Bool, Number, String, Object }

/// <summary>An ActionScript 1 value. Conversions follow Flash Player rules for the given SWF version.</summary>
public readonly struct Value : IEquatable<Value>
{
    public readonly ValueKind Kind;
    readonly double _num;
    readonly object? _ref;

    Value(ValueKind kind, double num, object? r) { Kind = kind; _num = num; _ref = r; }

    public static readonly Value Undefined = default;
    public static readonly Value Null = new(ValueKind.Null, 0, null);
    public static readonly Value True = new(ValueKind.Bool, 1, null);
    public static readonly Value False = new(ValueKind.Bool, 0, null);
    public static readonly Value EmptyString = new(ValueKind.String, 0, "");

    public static Value From(double d) => new(ValueKind.Number, d, null);
    public static Value From(string s) => new(ValueKind.String, 0, s);
    public static Value From(bool b) => b ? True : False;
    public static Value From(AsObject? o) => o == null ? Null : new(ValueKind.Object, 0, o);

    /// <summary>Comparison results: SWF 4 pushes 1/0, later versions push booleans.</summary>
    public static Value FromBool(bool b, int version) => version < 5 ? From(b ? 1.0 : 0.0) : From(b);

    public static implicit operator Value(double d) => From(d);
    public static implicit operator Value(string s) => From(s);
    public static implicit operator Value(bool b) => From(b);

    public bool IsUndefined => Kind == ValueKind.Undefined;
    public bool IsNullish => Kind is ValueKind.Undefined or ValueKind.Null;
    public bool IsString => Kind == ValueKind.String;
    public bool IsNumber => Kind == ValueKind.Number;
    public bool IsObject => Kind == ValueKind.Object;
    public double RawNumber => _num;
    public bool RawBool => _num != 0;
    public string RawString => (string)_ref!;
    public AsObject? Object => _ref as AsObject;

    public override string ToString() => ToStr(6);

    // ---- Conversions ----

    public double ToNumber(int version)
    {
        switch (Kind)
        {
            case ValueKind.Undefined: return version >= 7 ? double.NaN : 0;
            case ValueKind.Null: return version >= 7 ? double.NaN : 0;
            case ValueKind.Bool: return _num;
            case ValueKind.Number: return _num;
            case ValueKind.String: return ParseNumber(RawString, version);
            default:
                var prim = Object!.DefaultValue(Hint.Number, version);
                return prim.IsObject ? double.NaN : prim.ToNumber(version);
        }
    }

    public string ToStr(int version)
    {
        switch (Kind)
        {
            case ValueKind.Undefined: return version >= 7 ? "undefined" : "";
            case ValueKind.Null: return "null";
            case ValueKind.Bool: return version < 5 ? (_num != 0 ? "1" : "0") : (_num != 0 ? "true" : "false");
            case ValueKind.Number: return NumberToString(_num);
            case ValueKind.String: return RawString;
            default:
                var prim = Object!.DefaultValue(Hint.String, version);
                return prim.IsObject ? "[object]" : prim.ToStr(version);
        }
    }

    public bool ToBool(int version)
    {
        switch (Kind)
        {
            case ValueKind.Undefined:
            case ValueKind.Null: return false;
            case ValueKind.Bool: return _num != 0;
            case ValueKind.Number: return _num != 0 && !double.IsNaN(_num);
            case ValueKind.String:
                if (version >= 7) return RawString.Length > 0;
                var d = ParseNumber(RawString, version);
                return d != 0 && !double.IsNaN(d);
            default: return true;
        }
    }

    public Value ToPrimitive(int version, Hint hint = Hint.None) =>
        Kind == ValueKind.Object ? Object!.DefaultValue(hint, version) : this;

    public int ToInt32(int version) => DoubleToInt32(ToNumber(version));

    public static int DoubleToInt32(double d)
    {
        if (double.IsNaN(d) || double.IsInfinity(d)) return 0;
        d = Math.Truncate(d);
        double m = d % 4294967296.0;
        if (m < 0) m += 4294967296.0;
        return unchecked((int)(uint)m);
    }

    /// <summary>String to number as Flash Player does it: whitespace-trimmed decimal, invalid → NaN.</summary>
    public static double ParseNumber(string s, int version)
    {
        var t = s.AsSpan().Trim();
        if (t.Length == 0) return double.NaN;
        if (version >= 6 && t.Length > 2 && t[0] == '0' && (t[1] == 'x' || t[1] == 'X'))
        {
            return long.TryParse(t[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var h) ? h : double.NaN;
        }
        // Only plain decimal notation is accepted (no "Infinity", thousands separators, etc.).
        int i = 0;
        if (t[i] is '+' or '-') i++;
        bool digits = false;
        while (i < t.Length && char.IsAsciiDigit(t[i])) { i++; digits = true; }
        if (i < t.Length && t[i] == '.')
        {
            i++;
            while (i < t.Length && char.IsAsciiDigit(t[i])) { i++; digits = true; }
        }
        if (!digits) return double.NaN;
        if (i < t.Length && (t[i] is 'e' or 'E'))
        {
            int save = i;
            i++;
            if (i < t.Length && t[i] is '+' or '-') i++;
            bool expDigits = false;
            while (i < t.Length && char.IsAsciiDigit(t[i])) { i++; expDigits = true; }
            if (!expDigits) i = save;
        }
        if (i != t.Length) return double.NaN;
        return double.Parse(t, NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    /// <summary>Flash prints numbers with up to 15 significant digits.</summary>
    public static string NumberToString(double d)
    {
        if (double.IsNaN(d)) return "NaN";
        if (double.IsPositiveInfinity(d)) return "Infinity";
        if (double.IsNegativeInfinity(d)) return "-Infinity";
        if (d == 0) return "0";
        if (Math.Abs(d) < 1e15 && d == Math.Floor(d)) return ((long)d).ToString(CultureInfo.InvariantCulture);
        var s = d.ToString("G15", CultureInfo.InvariantCulture);
        int e = s.IndexOf('E');
        if (e < 0) return s;
        // "1.5E+20" → "1.5e+20", "1E-07" → "1e-7"
        string mant = s[..e];
        int exp = int.Parse(s[(e + 1)..], CultureInfo.InvariantCulture);
        return mant + "e" + (exp >= 0 ? "+" : "-") + Math.Abs(exp).ToString(CultureInfo.InvariantCulture);
    }

    public string TypeOf() => Kind switch
    {
        ValueKind.Undefined => "undefined",
        ValueKind.Null => "null",
        ValueKind.Bool => "boolean",
        ValueKind.Number => "number",
        ValueKind.String => "string",
        _ => Object!.TypeOf,
    };

    // ---- Identity (used by === and dictionary keys) ----

    public bool Equals(Value other) =>
        Kind == other.Kind && Kind switch
        {
            ValueKind.Undefined or ValueKind.Null => true,
            ValueKind.Bool or ValueKind.Number => _num.Equals(other._num),
            ValueKind.String => RawString == other.RawString,
            _ => ReferenceEquals(_ref, other._ref),
        };

    public override bool Equals(object? obj) => obj is Value v && Equals(v);
    public override int GetHashCode() => HashCode.Combine(Kind, _num, _ref);
}

public enum Hint { None, Number, String }
