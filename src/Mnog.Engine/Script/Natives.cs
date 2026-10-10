using System.Globalization;
using System.Text;
using Mnog.Engine.Display;

namespace Mnog.Engine.Script;

public sealed partial class Interpreter
{
    NativeFunction Fn(NativeFn fn, NativeFn? construct = null) => new(FunctionPrototype, fn, construct);

    void Method(AsObject target, string name, NativeFn fn) => target.SetOwn(name, Value.From(Fn(fn)));

    static Value Arg(Value[] a, int i) => i < a.Length ? a[i] : Value.Undefined;

    void InstallNatives()
    {
        // ---- Object / Function ----
        var objectCtor = Fn((vm, t, a) => a.Length > 0 && a[0].IsObject ? a[0] : Value.From(new AsObject(ObjectPrototype)));
        objectCtor.SetOwn("prototype", Value.From(ObjectPrototype));
        Global.SetOwn("Object", Value.From(objectCtor));
        Method(ObjectPrototype, "toString", (vm, t, a) => "[object Object]");
        Method(ObjectPrototype, "valueOf", (vm, t, a) => t);
        Method(ObjectPrototype, "hasOwnProperty", (vm, t, a) => t.Object?.HasOwn(Arg(a, 0).ToStr(vm.Version)) ?? false);
        Method(FunctionPrototype, "call", (vm, t, a) => vm.CallFunction(t, Arg(a, 0), a.Skip(1).ToArray()));
        Method(FunctionPrototype, "apply", (vm, t, a) =>
            vm.CallFunction(t, Arg(a, 0), (Arg(a, 1).Object as AsArray)?.Items.ToArray() ?? []));

        // ---- Array ----
        var arrayCtor = Fn((vm, t, a) => Value.From(vm.NewArray(a)), (vm, t, a) => Value.From(vm.NewArray(a)));
        arrayCtor.SetOwn("prototype", Value.From(ArrayPrototype));
        Global.SetOwn("Array", Value.From(arrayCtor));
        Method(ArrayPrototype, "push", (vm, t, a) =>
        {
            if (t.Object is not AsArray arr) return Value.Undefined;
            arr.Items.AddRange(a);
            return arr.Items.Count;
        });
        Method(ArrayPrototype, "pop", (vm, t, a) =>
        {
            if (t.Object is not AsArray { Items.Count: > 0 } arr) return Value.Undefined;
            var v = arr.Items[^1];
            arr.Items.RemoveAt(arr.Items.Count - 1);
            return v;
        });
        Method(ArrayPrototype, "shift", (vm, t, a) =>
        {
            if (t.Object is not AsArray { Items.Count: > 0 } arr) return Value.Undefined;
            var v = arr.Items[0];
            arr.Items.RemoveAt(0);
            return v;
        });
        Method(ArrayPrototype, "unshift", (vm, t, a) =>
        {
            if (t.Object is not AsArray arr) return Value.Undefined;
            arr.Items.InsertRange(0, a);
            return arr.Items.Count;
        });
        Method(ArrayPrototype, "join", (vm, t, a) =>
        {
            if (t.Object is not AsArray arr) return Value.Undefined;
            var sep = a.Length > 0 && !a[0].IsUndefined ? a[0].ToStr(vm.Version) : ",";
            return string.Join(sep, arr.Items.Select(v => v.ToStr(vm.Version)));
        });
        Method(ArrayPrototype, "toString", (vm, t, a) => t.Object?.DefaultValue(Hint.String, vm.Version) ?? "");
        Method(ArrayPrototype, "reverse", (vm, t, a) => { (t.Object as AsArray)?.Items.Reverse(); return t; });
        Method(ArrayPrototype, "concat", (vm, t, a) =>
        {
            var r = vm.NewArray([]);
            if (t.Object is AsArray arr) r.Items.AddRange(arr.Items);
            foreach (var v in a)
                if (v.Object is AsArray other) r.Items.AddRange(other.Items); else r.Items.Add(v);
            return Value.From(r);
        });
        Method(ArrayPrototype, "slice", (vm, t, a) =>
        {
            var r = vm.NewArray([]);
            if (t.Object is not AsArray arr) return Value.From(r);
            var (s, e) = SliceRange(arr.Items.Count, a, vm.Version);
            if (e > s) r.Items.AddRange(arr.Items.GetRange(s, e - s));
            return Value.From(r);
        });
        Method(ArrayPrototype, "splice", (vm, t, a) =>
        {
            var removed = vm.NewArray([]);
            if (t.Object is not AsArray arr) return Value.From(removed);
            int n = arr.Items.Count;
            int start = (int)Arg(a, 0).ToNumber(vm.Version);
            if (start < 0) start = Math.Max(0, n + start);
            start = Math.Min(start, n);
            int count = a.Length > 1 ? Math.Clamp((int)a[1].ToNumber(vm.Version), 0, n - start) : n - start;
            removed.Items.AddRange(arr.Items.GetRange(start, count));
            arr.Items.RemoveRange(start, count);
            arr.Items.InsertRange(start, a.Skip(2));
            return Value.From(removed);
        });

        // ---- String ----
        var stringFn = Fn((vm, t, a) => a.Length > 0 ? a[0].ToStr(vm.Version) : "");
        stringFn.SetOwn("prototype", Value.From(StringPrototype));
        Method(stringFn, "fromCharCode", (vm, t, a) =>
            new string(a.Select(v => (char)Value.DoubleToInt32(v.ToNumber(vm.Version))).ToArray()));
        Global.SetOwn("String", Value.From(stringFn));
        string S(Value t) => t.ToStr(Version);
        Method(StringPrototype, "toString", (vm, t, a) => S(t));
        Method(StringPrototype, "valueOf", (vm, t, a) => S(t));
        Method(StringPrototype, "charAt", (vm, t, a) =>
        {
            var s = S(t); int i = (int)Arg(a, 0).ToNumber(vm.Version);
            return i >= 0 && i < s.Length ? s[i].ToString() : "";
        });
        Method(StringPrototype, "charCodeAt", (vm, t, a) =>
        {
            var s = S(t); int i = (int)Arg(a, 0).ToNumber(vm.Version);
            return i >= 0 && i < s.Length ? s[i] : double.NaN;
        });
        Method(StringPrototype, "indexOf", (vm, t, a) =>
        {
            var s = S(t);
            int from = Math.Clamp((int)Arg(a, 1).ToNumber(vm.Version), 0, s.Length);
            return s.IndexOf(Arg(a, 0).ToStr(vm.Version), from, StringComparison.Ordinal);
        });
        Method(StringPrototype, "lastIndexOf", (vm, t, a) => S(t).LastIndexOf(Arg(a, 0).ToStr(vm.Version), StringComparison.Ordinal));
        Method(StringPrototype, "toUpperCase", (vm, t, a) => S(t).ToUpperInvariant());
        Method(StringPrototype, "toLowerCase", (vm, t, a) => S(t).ToLowerInvariant());
        Method(StringPrototype, "substr", (vm, t, a) =>
        {
            var s = S(t);
            int start = (int)Arg(a, 0).ToNumber(vm.Version);
            if (start < 0) start = Math.Max(0, s.Length + start);
            start = Math.Min(start, s.Length);
            int len = a.Length > 1 && !a[1].IsUndefined ? (int)a[1].ToNumber(vm.Version) : s.Length - start;
            len = Math.Clamp(len, 0, s.Length - start);
            return s.Substring(start, len);
        });
        Method(StringPrototype, "substring", (vm, t, a) =>
        {
            var s = S(t);
            int x = Math.Clamp((int)Arg(a, 0).ToNumber(vm.Version), 0, s.Length);
            int y = a.Length > 1 && !a[1].IsUndefined ? Math.Clamp((int)a[1].ToNumber(vm.Version), 0, s.Length) : s.Length;
            if (x > y) (x, y) = (y, x);
            return s[x..y];
        });
        Method(StringPrototype, "slice", (vm, t, a) =>
        {
            var s = S(t);
            var (x, y) = SliceRange(s.Length, a, vm.Version);
            return y > x ? s[x..y] : "";
        });
        Method(StringPrototype, "split", (vm, t, a) =>
        {
            var s = S(t);
            var arr = vm.NewArray([]);
            if (a.Length == 0 || a[0].IsUndefined) { arr.Items.Add(s); return Value.From(arr); }
            var sep = a[0].ToStr(vm.Version);
            var parts = sep.Length == 0 ? s.Select(c => c.ToString()).ToArray() : s.Split(sep);
            foreach (var p in parts) arr.Items.Add(p);
            return Value.From(arr);
        });
        Method(StringPrototype, "concat", (vm, t, a) => S(t) + string.Concat(a.Select(v => v.ToStr(vm.Version))));

        // ---- Number / Boolean ----
        var numberFn = Fn((vm, t, a) => a.Length > 0 ? a[0].ToNumber(vm.Version) : 0);
        numberFn.SetOwn("prototype", Value.From(NumberPrototype));
        numberFn.SetOwn("MAX_VALUE", double.MaxValue);
        numberFn.SetOwn("MIN_VALUE", double.Epsilon);
        numberFn.SetOwn("NaN", double.NaN);
        numberFn.SetOwn("POSITIVE_INFINITY", double.PositiveInfinity);
        numberFn.SetOwn("NEGATIVE_INFINITY", double.NegativeInfinity);
        Global.SetOwn("Number", Value.From(numberFn));
        Method(NumberPrototype, "toString", (vm, t, a) =>
        {
            double d = t.ToNumber(vm.Version);
            int radix = a.Length > 0 && !a[0].IsUndefined ? (int)a[0].ToNumber(vm.Version) : 10;
            if (radix == 10 || radix < 2 || radix > 36 || double.IsNaN(d) || double.IsInfinity(d)) return Value.NumberToString(d);
            long n = (long)Math.Truncate(d);
            bool neg = n < 0;
            n = Math.Abs(n);
            var sb = new StringBuilder();
            do { sb.Insert(0, "0123456789abcdefghijklmnopqrstuvwxyz"[(int)(n % radix)]); n /= radix; } while (n > 0);
            return (neg ? "-" : "") + sb;
        });
        Method(NumberPrototype, "valueOf", (vm, t, a) => t.ToNumber(vm.Version));
        var boolFn = Fn((vm, t, a) => Arg(a, 0).ToBool(vm.Version));
        boolFn.SetOwn("prototype", Value.From(BooleanPrototype));
        Global.SetOwn("Boolean", Value.From(boolFn));
        Method(BooleanPrototype, "toString", (vm, t, a) => t.ToBool(vm.Version) ? "true" : "false");

        // ---- Global functions ----
        Global.SetOwn("NaN", double.NaN);
        Global.SetOwn("Infinity", double.PositiveInfinity);
        Method(Global, "isNaN", (vm, t, a) => double.IsNaN(Arg(a, 0).ToNumber(vm.Version)));
        Method(Global, "isFinite", (vm, t, a) => double.IsFinite(Arg(a, 0).ToNumber(vm.Version)));
        Method(Global, "parseFloat", (vm, t, a) => ParseFloatPrefix(Arg(a, 0).ToStr(vm.Version)));
        Method(Global, "parseInt", (vm, t, a) => ParseIntPrefix(Arg(a, 0).ToStr(vm.Version), a.Length > 1 ? (int)a[1].ToNumber(vm.Version) : 0));
        Method(Global, "escape", (vm, t, a) => Escape(Arg(a, 0).ToStr(vm.Version)));
        Method(Global, "unescape", (vm, t, a) => Uri.UnescapeDataString(Arg(a, 0).ToStr(vm.Version)));

        // ---- Math ----
        var math = new AsObject(ObjectPrototype);
        Global.SetOwn("Math", Value.From(math));
        math.SetOwn("PI", Math.PI);
        math.SetOwn("E", Math.E);
        math.SetOwn("SQRT2", Math.Sqrt(2));
        math.SetOwn("LN2", Math.Log(2));
        math.SetOwn("LN10", Math.Log(10));
        void M1(string n, Func<double, double> f) => Method(math, n, (vm, t, a) => f(Arg(a, 0).ToNumber(vm.Version)));
        M1("abs", Math.Abs); M1("floor", Math.Floor); M1("ceil", Math.Ceiling); M1("sqrt", Math.Sqrt);
        M1("sin", Math.Sin); M1("cos", Math.Cos); M1("tan", Math.Tan); M1("asin", Math.Asin); M1("acos", Math.Acos);
        M1("atan", Math.Atan); M1("exp", Math.Exp); M1("log", Math.Log);
        M1("round", d => Math.Floor(d + 0.5));
        Method(math, "atan2", (vm, t, a) => Math.Atan2(Arg(a, 0).ToNumber(vm.Version), Arg(a, 1).ToNumber(vm.Version)));
        Method(math, "pow", (vm, t, a) => Math.Pow(Arg(a, 0).ToNumber(vm.Version), Arg(a, 1).ToNumber(vm.Version)));
        Method(math, "min", (vm, t, a) => a.Length == 0 ? double.PositiveInfinity : a.Min(v => v.ToNumber(vm.Version)));
        Method(math, "max", (vm, t, a) => a.Length == 0 ? double.NegativeInfinity : a.Max(v => v.ToNumber(vm.Version)));
        Method(math, "random", (vm, t, a) => _player.Random.NextDouble());

        // ---- Player integration ----
        InstallMovieClip();
        var shared = new AsObject(ObjectPrototype);
        Method(shared, "getLocal", (vm, t, a) => Value.From(_player.Storage.GetLocal(Arg(a, 0).ToStr(vm.Version), vm)));
        Global.SetOwn("SharedObject", Value.From(shared));

        var system = new AsObject(ObjectPrototype);
        Method(system, "setClipboard", (vm, t, a) => { _player.Host?.SetClipboard(Arg(a, 0).ToStr(vm.Version)); return Value.Undefined; });
        // Engine extension: the project's version (version_number.txt beside game/, e.g. "1.0"; "" without it),
        // shown on the title screen.
        system.SetOwn("mnogVersion", Value.From(_player.ProjectVersion));
        // Engine extension: whether a menu covers the game (the Help, settings, save slots...): the scene doesn't pan.
        Method(system, "mnogMenuOpen", (vm, t, a) => Value.From(_player.MenuOpen));
        Global.SetOwn("System", Value.From(system));

        var stage = new StageObject(_player, ObjectPrototype);
        // Engine extension used by the widescreen patch: Stage.mnogClampScenery(layers, rates[, vertical]).
        Method(stage, "mnogClampScenery", (vm, t, a) =>
        {
            if (Arg(a, 0).Object is AsArray layers && Arg(a, 1).Object is AsArray rates)
                _player.ClampScenery(
                    layers.Items.Select(v => v.Object as DisplayObject).OfType<DisplayObject>().ToList(),
                    rates.Items.Select(v => v.ToNumber(vm.Version)).ToList(),
                    Arg(a, 2).ToBool(vm.Version));
            return Value.Undefined;
        });
        Global.SetOwn("Stage", Value.From(stage));

        var selection = new AsObject(ObjectPrototype);
        Method(selection, "setFocus", (vm, t, a) => { _player.SetFocus(Arg(a, 0)); return true; });
        Method(selection, "getFocus", (vm, t, a) => _player.Focus is { } fo ? fo.DotPath : Value.Null);
        Method(selection, "setSelection", (vm, t, a) => Value.Undefined);
        Global.SetOwn("Selection", Value.From(selection));

        var mouse = new AsObject(ObjectPrototype);
        Method(mouse, "hide", (vm, t, a) => { _player.Host?.ShowCursor(false); return Value.Undefined; });
        Method(mouse, "show", (vm, t, a) => { _player.Host?.ShowCursor(true); return Value.Undefined; });
        Global.SetOwn("Mouse", Value.From(mouse));

        var key = new AsObject(ObjectPrototype);
        Method(key, "isDown", (vm, t, a) => _player.KeysDown.Contains((int)Arg(a, 0).ToNumber(vm.Version)));
        Method(key, "getCode", (vm, t, a) => _player.LastKeyCode);
        Method(key, "getAscii", (vm, t, a) => _player.LastKeyAscii);
        Global.SetOwn("Key", Value.From(key));
    }

    public AsArray NewArray(Value[] items)
    {
        var arr = new AsArray(ArrayPrototype);
        if (items.Length == 1 && items[0].IsNumber)
            for (int i = 0; i < (int)items[0].RawNumber; i++) arr.Items.Add(Value.Undefined);
        else arr.Items.AddRange(items);
        return arr;
    }

    static (int, int) SliceRange(int n, Value[] a, int ver)
    {
        int s = a.Length > 0 ? (int)a[0].ToNumber(ver) : 0;
        int e = a.Length > 1 && !a[1].IsUndefined ? (int)a[1].ToNumber(ver) : n;
        if (s < 0) s = Math.Max(0, n + s);
        if (e < 0) e = Math.Max(0, n + e);
        return (Math.Min(s, n), Math.Min(e, n));
    }

    static double ParseFloatPrefix(string s)
    {
        s = s.TrimStart();
        int i = 0;
        if (i < s.Length && s[i] is '+' or '-') i++;
        while (i < s.Length && (char.IsAsciiDigit(s[i]) || s[i] == '.')) i++;
        if (i < s.Length && s[i] is 'e' or 'E')
        {
            int j = i + 1;
            if (j < s.Length && s[j] is '+' or '-') j++;
            if (j < s.Length && char.IsAsciiDigit(s[j])) { i = j; while (i < s.Length && char.IsAsciiDigit(s[i])) i++; }
        }
        while (i > 0 && !double.TryParse(s.AsSpan(0, i), NumberStyles.Float, CultureInfo.InvariantCulture, out _)) i--;
        return i == 0 ? double.NaN : double.Parse(s.AsSpan(0, i), NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    static double ParseIntPrefix(string s, int radix)
    {
        s = s.Trim();
        bool neg = false;
        if (s.StartsWith('-')) { neg = true; s = s[1..]; }
        else if (s.StartsWith('+')) s = s[1..];
        if ((radix == 0 || radix == 16) && s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) { radix = 16; s = s[2..]; }
        else if (radix == 0 && s.Length > 1 && s[0] == '0') radix = 8;
        if (radix == 0) radix = 10;
        if (radix < 2 || radix > 36) return double.NaN;
        double v = 0;
        int digits = 0;
        foreach (var c in s.ToLowerInvariant())
        {
            int d = char.IsAsciiDigit(c) ? c - '0' : char.IsAsciiLetterLower(c) ? c - 'a' + 10 : 99;
            if (d >= radix) break;
            v = v * radix + d;
            digits++;
        }
        return digits == 0 ? double.NaN : neg ? -v : v;
    }

    void InstallMovieClip()
    {
        var mc = MovieClipPrototype;
        MovieClip? Clip(Value t) => t.Object as MovieClip;
        int Depth(Value v) => (int)v.ToNumber(Version) + 16384;

        Method(mc, "play", (vm, t, a) => { Clip(t)?.Play(); return Value.Undefined; });
        Method(mc, "stop", (vm, t, a) => { Clip(t)?.Stop(); return Value.Undefined; });
        Method(mc, "nextFrame", (vm, t, a) => { Clip(t)?.NextFrame(); return Value.Undefined; });
        Method(mc, "prevFrame", (vm, t, a) => { Clip(t)?.PrevFrame(); return Value.Undefined; });
        Method(mc, "gotoAndPlay", (vm, t, a) =>
        {
            if (Clip(t) is { } c) { if (c.FrameOf(Arg(a, 0), vm.Version) is int f) c.GotoFrame(f); c.Play(); }
            return Value.Undefined;
        });
        Method(mc, "gotoAndStop", (vm, t, a) =>
        {
            if (Clip(t) is { } c) { if (c.FrameOf(Arg(a, 0), vm.Version) is int f) c.GotoFrame(f); c.Stop(); }
            return Value.Undefined;
        });
        Method(mc, "removeMovieClip", (vm, t, a) => { if (Clip(t) is { } c) RemoveMovieClip(c); return Value.Undefined; });
        Method(mc, "duplicateMovieClip", (vm, t, a) =>
            Value.From(Clip(t)?.Duplicate(Arg(a, 0).ToStr(vm.Version), Depth(Arg(a, 1)))));
        Method(mc, "attachMovie", (vm, t, a) =>
            Value.From(Clip(t)?.Attach(Arg(a, 0).ToStr(vm.Version), Arg(a, 1).ToStr(vm.Version), Depth(Arg(a, 2)))));
        Method(mc, "createEmptyMovieClip", (vm, t, a) =>
        {
            if (Clip(t) is not { } c) return Value.Undefined;
            var empty = new MovieClip(_player, c.Movie, new Assets.TimelineAsset(c.Movie, new Format.TimelineDoc { FrameCount = 1 }, ""))
            { Name = Arg(a, 0).ToStr(vm.Version) };
            c.AddChild(empty, Depth(Arg(a, 1)));
            empty.Construct();
            return Value.From(empty);
        });
        Method(mc, "loadMovie", (vm, t, a) =>
        {
            if (Clip(t) is { } c) _player.LoadMovie(Arg(a, 0).ToStr(vm.Version), Value.From(c), c);
            return Value.Undefined;
        });
        Method(mc, "unloadMovie", (vm, t, a) =>
        {
            if (Clip(t) is { } c) _player.LoadMovie("blank.swf", Value.From(c), c);
            return Value.Undefined;
        });
        Method(mc, "getBytesLoaded", (vm, t, a) => 1);
        Method(mc, "getBytesTotal", (vm, t, a) => 1);
        Method(mc, "swapDepths", (vm, t, a) =>
        {
            if (Clip(t) is not { Parent: { } p } c) return Value.Undefined;
            int other = Arg(a, 0).Object is DisplayObject d ? d.Depth : Depth(Arg(a, 0));
            p.Children.Remove(c.Depth);
            if (p.Children.TryGetValue(other, out var o)) { p.Children.Remove(other); o.Depth = c.Depth; p.Children[c.Depth] = o; }
            c.Depth = other;
            p.Children[other] = c;
            return Value.Undefined;
        });
        Method(mc, "hitTest", (vm, t, a) =>
        {
            if (Clip(t) is not { } c) return false;
            if (a.Length >= 2 && a[0].IsNumber || a.Length >= 2 && !a[0].IsObject)
            {
                var p = new SkiaSharp.SKPoint((float)Arg(a, 0).ToNumber(vm.Version), (float)Arg(a, 1).ToNumber(vm.Version));
                if (Arg(a, 2).ToBool(vm.Version)) return c.HitTest(p, c.WorldMatrix);
                return c.WorldMatrix.MapRect(c.LocalBounds).Contains(p);
            }
            if (vm.ResolveTargetFrom(c, Arg(a, 0)) is { } other)
                return c.WorldMatrix.MapRect(c.LocalBounds).IntersectsWith(other.WorldMatrix.MapRect(other.LocalBounds));
            return false;
        });
        Method(mc, "getBounds", (vm, t, a) =>
        {
            var o = new AsObject(ObjectPrototype);
            if (Clip(t) is not { } c) return Value.From(o);
            var r = c.WorldMatrix.MapRect(c.LocalBounds);
            if (vm.ResolveTargetFrom(c, Arg(a, 0)) is { } space && space.WorldMatrix.TryInvert(out var inv)) r = inv.MapRect(r);
            o.SetOwn("xMin", r.Left); o.SetOwn("xMax", r.Right); o.SetOwn("yMin", r.Top); o.SetOwn("yMax", r.Bottom);
            return Value.From(o);
        });
        Method(mc, "localToGlobal", (vm, t, a) => { MapPoint(Clip(t), Arg(a, 0), true); return Value.Undefined; });
        Method(mc, "globalToLocal", (vm, t, a) => { MapPoint(Clip(t), Arg(a, 0), false); return Value.Undefined; });
        Method(mc, "startDrag", (vm, t, a) =>
        {
            if (Clip(t) is not { } c) return Value.Undefined;
            SkiaSharp.SKRect? r = a.Length >= 5
                ? new SkiaSharp.SKRect((float)a[1].ToNumber(vm.Version), (float)a[2].ToNumber(vm.Version), (float)a[3].ToNumber(vm.Version), (float)a[4].ToNumber(vm.Version))
                : null;
            _player.StartDrag(c, Arg(a, 0).ToBool(vm.Version), r);
            return Value.Undefined;
        });
        Method(mc, "stopDrag", (vm, t, a) => { _player.StopDrag(); return Value.Undefined; });
    }

    void MapPoint(MovieClip? c, Value pt, bool toGlobal)
    {
        if (c == null || pt.Object is not { } o) return;
        var p = new SkiaSharp.SKPoint((float)o.Get("x", this).ToNumber(Version), (float)o.Get("y", this).ToNumber(Version));
        var m = c.WorldMatrix;
        if (!toGlobal && !m.TryInvert(out m)) return;
        p = m.MapPoint(p);
        o.Set("x", p.X, this);
        o.Set("y", p.Y, this);
    }

    /// <summary>Stage.displayState toggles fullscreen.</summary>
    sealed class StageObject(Player player, AsObject proto) : AsObject(proto)
    {
        public override bool TryGetOwn(string name, out Value v)
        {
            switch (name.ToLowerInvariant())
            {
                case "displaystate": v = player.Host?.Fullscreen == true ? "fullScreen" : "normal"; return true;
                case "width": v = player.StageWidth; return true;
                case "height": v = player.StageHeight; return true;
                // Engine extension: the stage range the window shows (wider than the stage in widescreen).
                case "mnogviewleft": v = player.ViewLeft; return true;
                case "mnogviewright": v = player.ViewRight; return true;
            }
            return base.TryGetOwn(name, out v);
        }

        public override void SetOwn(string name, Value v)
        {
            if (name.Equals("displayState", StringComparison.OrdinalIgnoreCase))
            {
                if (player.Host != null) player.Host.Fullscreen = v.ToStr(6).Equals("fullScreen", StringComparison.OrdinalIgnoreCase);
                return;
            }
            base.SetOwn(name, v);
        }
    }
}
