namespace Mnog.Engine.Script;

/// <summary>An ActionScript object. Property names are case-insensitive (SWF ≤ 6 semantics).</summary>
public class AsObject
{
    public static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;

    readonly OrderedDictionary<string, Value> _props = new(NameComparer);
    public AsObject? Proto;

    public AsObject(AsObject? proto = null) => Proto = proto;

    public virtual string TypeOf => "object";

    public virtual bool TryGetOwn(string name, out Value v) => _props.TryGetValue(name, out v);

    public virtual void SetOwn(string name, Value v) => _props[name] = v;

    public virtual bool DeleteOwn(string name) => _props.Remove(name);

    public virtual bool HasOwn(string name) => _props.ContainsKey(name);

    /// <summary>Property names for for-in, most recently added first (as Flash enumerates).</summary>
    public virtual IEnumerable<string> Keys() => _props.Keys.Reverse();

    /// <summary>Its own string properties rewritten (the settings' Tohunga or Matoran, switched with text loaded).</summary>
    public void MapStrings(Func<string, string> f)
    {
        foreach (var k in _props.Keys.ToList())
            if (_props[k] is { IsString: true } v && f(v.RawString) is var s && s != v.RawString) _props[k] = Value.From(s);
    }

    /// <summary>Looks up a property along the prototype chain.</summary>
    public virtual Value Get(string name, Interpreter vm)
    {
        if (name.Equals("__proto__", StringComparison.OrdinalIgnoreCase)) return Value.From(Proto);
        for (var o = this; o != null; o = o.Proto)
            if (o.TryGetOwn(name, out var v)) return v;
        return Value.Undefined;
    }

    public virtual void Set(string name, Value v, Interpreter vm)
    {
        if (name.Equals("__proto__", StringComparison.OrdinalIgnoreCase)) { Proto = v.Object; return; }
        SetOwn(name, v);
    }

    public virtual Value DefaultValue(Hint hint, int version) => Value.From("[object Object]");
}

public sealed class AsArray : AsObject
{
    public readonly List<Value> Items = [];

    public AsArray(AsObject? proto) : base(proto) { }

    static bool Index(string name, out int i) =>
        int.TryParse(name, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out i);

    public override bool TryGetOwn(string name, out Value v)
    {
        if (name.Equals("length", StringComparison.OrdinalIgnoreCase)) { v = Items.Count; return true; }
        if (Index(name, out int i))
        {
            v = i < Items.Count ? Items[i] : Value.Undefined;
            return i < Items.Count;
        }
        return base.TryGetOwn(name, out v);
    }

    public override void SetOwn(string name, Value v)
    {
        if (name.Equals("length", StringComparison.OrdinalIgnoreCase))
        {
            int n = Math.Max(0, Value.DoubleToInt32(v.ToNumber(6)));
            if (n < Items.Count) Items.RemoveRange(n, Items.Count - n);
            while (Items.Count < n) Items.Add(Value.Undefined);
            return;
        }
        if (Index(name, out int i))
        {
            while (Items.Count <= i) Items.Add(Value.Undefined);
            Items[i] = v;
            return;
        }
        base.SetOwn(name, v);
    }

    public override IEnumerable<string> Keys()
    {
        foreach (var k in base.Keys()) yield return k;
        for (int i = Items.Count - 1; i >= 0; i--) yield return i.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public override Value DefaultValue(Hint hint, int version) =>
        string.Join(",", Items.Select(v => v.ToStr(version)));
}

public abstract class AsFunction : AsObject
{
    protected AsFunction(AsObject? proto) : base(proto) { }

    public override string TypeOf => "function";

    public abstract Value Call(Interpreter vm, Value thisValue, Value[] args);

    /// <summary>new F(args): creates an object whose prototype is F.prototype, then calls F on it.</summary>
    public virtual Value Construct(Interpreter vm, Value[] args)
    {
        var proto = Get("prototype", vm).Object ?? vm.ObjectPrototype;
        var obj = new AsObject(proto);
        obj.SetOwn("constructor", Value.From(this));
        var r = Call(vm, Value.From(obj), args);
        return r.IsObject ? r : Value.From(obj);
    }

    public override Value DefaultValue(Hint hint, int version) => Value.From("[type Function]");
}

public delegate Value NativeFn(Interpreter vm, Value thisValue, Value[] args);

public sealed class NativeFunction(AsObject? proto, NativeFn fn, NativeFn? construct = null) : AsFunction(proto)
{
    public override Value Call(Interpreter vm, Value thisValue, Value[] args) => fn(vm, thisValue, args);

    public override Value Construct(Interpreter vm, Value[] args) =>
        construct != null ? construct(vm, Value.Undefined, args) : base.Construct(vm, args);
}

/// <summary>A function written in script; closes over the scope chain where it was defined.</summary>
public sealed class ScriptFunction : AsFunction
{
    public readonly Chunk Chunk;
    public readonly Scope Closure;
    public readonly Display.MovieClip? Timeline;

    public ScriptFunction(AsObject? proto, Chunk chunk, Scope closure, Display.MovieClip? timeline, AsObject objectProto)
        : base(proto)
    {
        Chunk = chunk;
        Closure = closure;
        Timeline = timeline;
        var prototype = new AsObject(objectProto);
        prototype.SetOwn("constructor", Value.From(this));
        SetOwn("prototype", Value.From(prototype));
    }

    public override Value Call(Interpreter vm, Value thisValue, Value[] args) => vm.Invoke(this, thisValue, args);
}

/// <summary>A link in the scope chain (activation objects, timelines, globals).</summary>
public sealed class Scope(AsObject obj, Scope? parent)
{
    public readonly AsObject Object = obj;
    public readonly Scope? Parent = parent;
}
