using System.Text.Json.Nodes;
using Mnog.Engine.Assets;

namespace Mnog.Engine.Audio;

/// <summary>
/// High-quality replacements for the game's sounds (the Audio menu's High quality option): game/audio/hq holds the
/// files, and hq.json says which sound ("Movie#id") each replaces: {"file"} plays that file instead, {"silent": true}
/// plays nothing (the sound is part of another file, as the Le-Koro band's layers are of its recreation), and "group"
/// keeps a file from starting again while one of its group is playing (the Underwater song stands in for both flute
/// melodies the game plays at random), and "gain" (dB) sets the file's volume.
/// Each file is read once, when first needed (the game waits for it: WAV, as all of game/audio/hq is, takes
/// milliseconds), and shared by the sounds that use it. If a file can't be read, the game's own sound plays, and a
/// movie with silent sounds keeps all its own sounds (never half replaced). The longest files are read as soon as
/// High quality is turned on (WarmUp).
/// </summary>
public sealed class HqSounds
{
    /// <summary>File: what plays instead; Silent: nothing plays; Group: see the class; Gain: a constant volume change
    /// for the file (dB), so a whole song supplied as it is can sit at the game's level without being edited;
    /// NoEnvelope: the game's volume envelope for the sound it replaces isn't applied (the file plays at its own level
    /// and Gain, not faded as the game faded its short original).</summary>
    public sealed record Entry(string? File, bool Silent, string? Group, double Gain = 0, bool NoEnvelope = false);

    readonly string _dir;
    readonly Dictionary<string, Entry> _map = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, SoundAsset> _files = new(StringComparer.OrdinalIgnoreCase);
    readonly HashSet<string> _decoding = new(StringComparer.OrdinalIgnoreCase);
    // Movies with silent sounds, and the files their sounds play: all or nothing.
    readonly Dictionary<string, List<string>> _together = new(StringComparer.OrdinalIgnoreCase);
    readonly object _lock = new();

    public HqSounds(string gameDir)
    {
        _dir = Path.Combine(Path.GetFullPath(gameDir), "audio", "hq");
        var file = Path.Combine(_dir, "hq.json");
        if (!File.Exists(file)) return;
        try
        {
            if (JsonNode.Parse(File.ReadAllText(file))?["sounds"] is not JsonObject sounds) return;
            foreach (var (key, v) in sounds)
                if (v is JsonObject o)
                    _map[key] = new Entry(o["file"]?.GetValue<string>(), o["silent"]?.GetValue<bool>() == true,
                        o["group"]?.GetValue<string>(), o["gain"]?.GetValue<double>() ?? 0,
                        o["noEnvelope"]?.GetValue<bool>() == true);
        }
        catch (Exception e) when (e is System.Text.Json.JsonException or IOException)
        {
            Console.WriteLine($"[audio] hq.json: {e.Message}");
        }
        foreach (var movie in _map.Where(kv => kv.Value.Silent).Select(kv => MovieOf(kv.Key)).Distinct())
            _together[movie] = _map.Where(kv => MovieOf(kv.Key).Equals(movie, StringComparison.OrdinalIgnoreCase) &&
                                                kv.Value.File != null).Select(kv => kv.Value.File!).Distinct().ToList();
    }

    static string MovieOf(string key) => key[..key.LastIndexOf('#')];

    public int Count => _map.Count;

    /// <summary>The replacement for a game sound, if it has one and it can be used now.</summary>
    public Entry? Find(SoundAsset sound)
    {
        if (_map.GetValueOrDefault($"{sound.Movie.Name}#{sound.Id}") is not { } e) return null;
        // (A movie whose sounds are replaced together: only once every file it plays is ready.)
        if (_together.TryGetValue(sound.Movie.Name, out var files) && !files.All(f => Ready(f, sound.Movie)))
        {
            foreach (var f in files) Decode(f, sound.Movie);
            return null;
        }
        return e;
    }

    /// <summary>The replacement's sound for a game sound whose entry has a file: if it's decoded (or ready: wait for
    /// it), else null, its decoding started in the background.</summary>
    public SoundAsset? Asset(SoundAsset sound, bool ready = false)
    {
        if (_map.GetValueOrDefault($"{sound.Movie.Name}#{sound.Id}") is not { File: { } name }) return null;
        var asset = FileAsset(name, sound.Movie);
        if (ready || Ready(name, sound.Movie)) return asset;
        Decode(name, sound.Movie);
        return null;
    }

    /// <summary>Starts decoding, in the background, the files of the movies replaced together (long songs).</summary>
    public void WarmUp(MovieAsset owner)
    {
        if (Environment.GetEnvironmentVariable("MNOG_HQDBG") != null) Console.WriteLine($"[hq] warm-up: {string.Join(", ", _together.Values.SelectMany(x => x).Distinct())}");
        foreach (var f in _together.Values.SelectMany(x => x).Distinct()) Decode(f, owner);
    }

    /// <summary>Makes a file ready to play: read (and decoded, if it isn't WAV) there and then, the game waiting for
    /// it, so the high-quality version always plays, the first time too. (A WAV, which all of game/audio/hq is, needs
    /// no decoding: even a 4-minute song is read in well under 0.1 s.) False only if it can't be read.</summary>
    bool Ready(string name, MovieAsset owner) => FileAsset(name, owner).Samples.Length > 0;

    SoundAsset FileAsset(string name, MovieAsset owner)
    {
        lock (_lock)
        {
            if (!_files.TryGetValue(name, out var asset))
                _files[name] = asset = SoundAsset.FromFile(owner, Path.Combine(_dir, name), 0);
            return asset;
        }
    }

    void Decode(string name, MovieAsset owner)
    {
        var asset = FileAsset(name, owner);
        lock (_lock)
            if (asset.Decoded || !_decoding.Add(name)) return;
        Task.Run(() =>
        {
            var t = System.Diagnostics.Stopwatch.StartNew();
            _ = asset.Samples;
            if (Environment.GetEnvironmentVariable("MNOG_HQDBG") != null)
                Console.WriteLine($"[hq] decoded {name}: {asset.Samples.Length} samples in {t.ElapsedMilliseconds} ms");
        });
    }
}
