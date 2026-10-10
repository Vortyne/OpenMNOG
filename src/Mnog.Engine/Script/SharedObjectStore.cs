using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mnog.Engine.Script;

/// <summary>
/// SharedObject.getLocal(): persistent per-name data objects saved as JSON files.
/// The patched game uses one to remember settings and progress.
/// </summary>
public sealed class SharedObjectStore(string dir)
{
    /// <summary>The folder the data files are saved in.</summary>
    public string Dir => dir;

    readonly Dictionary<string, (AsObject So, AsObject Data)> _objects = new(StringComparer.OrdinalIgnoreCase);

    string FileFor(string name)
    {
        var safe = string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        return Path.Combine(dir, safe + ".json");
    }

    public AsObject GetLocal(string name, Interpreter vm)
    {
        if (_objects.TryGetValue(name, out var existing)) return existing.So;
        var data = new AsObject(vm.ObjectPrototype);
        var file = FileFor(name);
        var stored = Read(file);
        if (stored != null)
            foreach (var (k, v) in stored) data.SetOwn(k, FromJson(v, vm));
        _baseline[name] = Snapshot(data, vm);
        var so = new AsObject(vm.ObjectPrototype);
        so.SetOwn("data", Value.From(data));
        so.SetOwn("flush", Value.From(new NativeFunction(vm.FunctionPrototype, (v, t, a) =>
        {
            Save(name, data, v);
            return true;
        })));
        so.SetOwn("clear", Value.From(new NativeFunction(vm.FunctionPrototype, (v, t, a) =>
        {
            foreach (var k in data.Keys().ToList()) data.DeleteOwn(k);
            Save(name, data, v);
            return Value.Undefined;
        })));
        _objects[name] = (so, data);
        return so;
    }

    /// <summary>Each data object's values as last read or written (JSON text), to tell what this game changed.</summary>
    readonly Dictionary<string, Dictionary<string, string>> _baseline = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, DateTime> _lastBackup = new(StringComparer.OrdinalIgnoreCase);

    const int BackupsKept = 60;
    /// <summary>The patched game's value for a save slot with nothing saved.</summary>
    const string EmptySave = "&nostate=1&";
    static readonly TimeSpan BackupEvery = TimeSpan.FromMinutes(10);

    /// <summary>Where backups of the data files are kept: "backups" beside the data folder.</summary>
    public string BackupDir => Path.Combine(Path.GetDirectoryName(Path.GetFullPath(dir))!, "backups");

    static JsonObject? Read(string file)
    {
        if (!File.Exists(file)) return null;
        try { return JsonNode.Parse(File.ReadAllText(file)) as JsonObject; }
        catch (Exception e) when (e is JsonException or IOException) { return null; }
    }

    static Dictionary<string, string> Snapshot(AsObject data, Interpreter vm)
    {
        var r = new Dictionary<string, string>(AsObject.NameComparer);
        foreach (var k in data.Keys())
            if (data.TryGetOwn(k, out var v)) r[k] = ToJson(v, vm, 0)?.ToJsonString() ?? "null";
        return r;
    }

    /// <summary>
    /// Writes only what this game changed since the file was read or last written, over what the file holds now:
    /// another copy of the game running, or anything else that wrote the file meanwhile, keeps its changes (each
    /// save slot is its own value). Values others changed are taken in. The file is replaced whole only once the
    /// new one is written, and backed up first now and then (see Backup).
    /// </summary>
    void Save(string name, AsObject data, Interpreter vm)
    {
        Directory.CreateDirectory(dir);
        var file = FileFor(name);
        var baseline = _baseline.GetValueOrDefault(name) ?? [];
        var now = Snapshot(data, vm);
        var obj = Read(file) ?? [];
        var changed = now.Where(kv => baseline.GetValueOrDefault(kv.Key) != kv.Value).Select(kv => kv.Key).ToList();
        var removed = baseline.Keys.Where(k => !now.ContainsKey(k)).ToList();
        if (changed.Count == 0 && removed.Count == 0 && File.Exists(file)) return;
        // A save slot being emptied (a new game, a restart, a delete) is always backed up first.
        if (removed.Count > 0 || changed.Any(k => now[k].Contains(EmptySave) && obj[k] is { } o && !o.ToJsonString().Contains(EmptySave)))
            _lastBackup.Remove(name);
        Backup(name, file, obj, changed.Where(k => now[k] != obj[k]?.ToJsonString()).Any(k => obj[k] != null) || removed.Count > 0);
        foreach (var k in changed) obj[k] = JsonNode.Parse(now[k]);
        foreach (var k in removed) obj.Remove(k);
        // What others changed comes in to this game too (its scripts keep their own copies of what they use).
        foreach (var (k, v) in obj)
            if (!changed.Contains(k, AsObject.NameComparer)) data.SetOwn(k, FromJson(v, vm));
        var temp = file + ".tmp";
        // Something else can hold the file for a moment (the indexer, a virus scan, the network share's own
        // caching, a save made just before): tried again shortly, and if it still can't be written, the game plays
        // on and the next save writes these changes too (the baseline stays as it was).
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                File.WriteAllText(temp, obj.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
                File.Move(temp, file, overwrite: true);
                break;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                if (attempt < 6) { Thread.Sleep(25 * attempt); continue; }
                Console.WriteLine($"save: couldn't write {file} ({e.Message}); kept for the next save");
                return;
            }
        }
        _baseline[name] = Snapshot(data, vm);
    }

    /// <summary>
    /// A copy of the data file before it's written: always when a value already saved is about to be replaced
    /// by a different one for the first time this run, then at most every ten minutes; the newest are kept.
    /// </summary>
    void Backup(string name, string file, JsonObject current, bool replacing)
    {
        if (!replacing || !File.Exists(file)) return;
        if (_lastBackup.TryGetValue(name, out var last) && DateTime.Now - last < BackupEvery) return;
        try
        {
            Directory.CreateDirectory(BackupDir);
            var stem = Path.GetFileNameWithoutExtension(file);
            File.WriteAllText(Path.Combine(BackupDir, $"{stem}-{DateTime.Now:yyyyMMdd-HHmmss}.json"), current.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            _lastBackup[name] = DateTime.Now;
            foreach (var old in Directory.GetFiles(BackupDir, stem + "-*.json").OrderByDescending(f => f, StringComparer.Ordinal).Skip(BackupsKept))
                File.Delete(old);
        }
        catch (IOException) { }
    }

    /// <summary>Saves a data object now (values the engine set in it itself).</summary>
    public void Flush(string name, Interpreter vm)
    {
        if (_objects.TryGetValue(name, out var o)) Save(name, o.Data, vm);
    }

    /// <summary>Backs up the data file now, whatever the last backup's time (before a save slot is cleared).</summary>
    public void BackupNow(string name)
    {
        var file = FileFor(name);
        _lastBackup.Remove(name);
        if (Read(file) is { } current) Backup(name, file, current, true);
    }

    static JsonNode? ToJson(Value v, Interpreter vm, int depth)
    {
        switch (v.Kind)
        {
            case ValueKind.Bool: return v.RawBool;
            case ValueKind.Number: return double.IsFinite(v.RawNumber) ? v.RawNumber : null;
            case ValueKind.String: return v.RawString;
            case ValueKind.Object when depth < 16 && v.Object is AsArray arr:
                return new JsonArray(arr.Items.Select(i => ToJson(i, vm, depth + 1)).ToArray());
            case ValueKind.Object when depth < 16 && v.Object is not AsFunction and not Display.DisplayObject:
                {
                    var o = new JsonObject();
                    foreach (var k in v.Object!.Keys().Reverse())
                        if (v.Object.TryGetOwn(k, out var x)) o[k] = ToJson(x, vm, depth + 1);
                    return o;
                }
            default: return null;
        }
    }

    static Value FromJson(JsonNode? n, Interpreter vm)
    {
        switch (n)
        {
            case null: return Value.Null;
            case JsonArray a:
                {
                    var arr = vm.NewArray([]);
                    foreach (var x in a) arr.Items.Add(FromJson(x, vm));
                    return Value.From(arr);
                }
            case JsonObject o:
                {
                    var obj = new AsObject(vm.ObjectPrototype);
                    foreach (var (k, x) in o) obj.SetOwn(k, FromJson(x, vm));
                    return Value.From(obj);
                }
            case JsonValue jv:
                if (jv.TryGetValue(out bool b)) return b;
                if (jv.TryGetValue(out double d)) return d;
                if (jv.TryGetValue(out string? s)) return s;
                return Value.Undefined;
        }
        return Value.Undefined;
    }
}
