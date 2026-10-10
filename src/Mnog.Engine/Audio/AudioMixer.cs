using Mnog.Engine.Assets;
using Mnog.Format;

namespace Mnog.Engine.Audio;

/// <summary>
/// Mixes event and stream sounds into 44.1 kHz stereo float samples, applying Flash sound
/// semantics: in/out points, loop counts, volume envelopes, no-multiple and stop-by-id.
/// </summary>
public sealed class AudioMixer
{
    public const int OutputRate = 44100;

    sealed class Instance
    {
        public required SoundAsset Sound;
        public required float[] Samples;
        public int Channels;
        public double Step;          // source frames per output frame
        public double Pos;           // current source frame
        public double Start, End;    // source frame range of one loop
        public int LoopsLeft;
        public long Played;          // output frames played (for envelopes)
        public List<long[]>? Envelope;
        public object? Owner;        // stream sounds: the clip playing them
        public object? Source;       // the loaded movie that started it; unloading that movie stops it
        public float Gain = 1, FadeStep; // fading out: gain falls by FadeStep per output frame
        public int Group;            // Music or Sfx (volume settings)
    }

    public const int Music = 0, Sfx = 1;

    /// <summary>Sounds sorted by hand or analysis (the game's sound-groups.json: cutscenes, minigames), "Movie#id".</summary>
    readonly Dictionary<string, int> _listed = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Reads sound-groups.json ({"music": [...], "sfx": [...]}).</summary>
    public void LoadGroups(string file)
    {
        if (!File.Exists(file)) return;
        try
        {
            if (System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(file)) is not System.Text.Json.Nodes.JsonObject o) return;
            foreach (var (list, group) in new[] { ("music", Music), ("sfx", Sfx) })
                if (o[list] is System.Text.Json.Nodes.JsonArray a)
                    foreach (var k in a) if (k?.GetValue<string>() is { } key) _listed[key] = group;
        }
        catch (Exception e) when (e is System.Text.Json.JsonException or IOException) { Console.WriteLine($"[audio] sound-groups.json: {e.Message}"); }
    }

    /// <summary>The volume of each group of sounds (the Audio settings), 0..1; Volume is the master volume.</summary>
    public readonly float[] GroupVolume = [1, 1];

    /// <summary>
    /// Which group a sound is in: the scenes' music (Music_* movies, and the musical pieces the game plays as
    /// scene sounds: Ga-Koro's sting, the Bionicle head's theme, Ko-Koro's monk horns, two of the Ga-Koro water
    /// pieces), or everything else (ambience, the game's own sounds, cutscenes).
    /// </summary>
    int GroupOf(SoundAsset sound)
    {
        var name = sound.Movie.Name;
        if (_listed.TryGetValue($"{name}#{sound.Id}", out int listed)) return listed;
        bool Is(string movie) => name.Equals(movie, StringComparison.OrdinalIgnoreCase);
        if (name.StartsWith("Music_", StringComparison.OrdinalIgnoreCase) || name.StartsWith("SFX_Monkhorn", StringComparison.OrdinalIgnoreCase) ||
            Is("SFX_Gakorosting") || Is("SFX_bionihead") || Is("SFX_WaterAmbiant") && sound.Id is 2 or 3) return Music;
        return Sfx;
    }

    readonly List<Instance> _playing = [];
    public float Volume = 1f;

    /// <summary>The audio thread mixes while the game thread starts and stops sounds: both hold this.</summary>
    readonly object _lock = new();

    public void Start(MovieAsset movie, SoundRefDoc r, object? source = null)
    {
        if (movie.Symbol(r.Id) is not SoundAsset sound) return;
        if (r.Stop == true)
        {
            lock (_lock) _playing.RemoveAll(i => i.Sound == sound);
            return;
        }
        if (r.NoMultiple == true) lock (_lock) if (_playing.Any(i => i.Sound == sound)) return;
        // Decoded outside the lock (a long sound takes a while; the audio thread mustn't wait for it).
        var inst = Create(sound);
        if (inst == null) return;
        double toSource = sound.Rate / (double)OutputRate;
        if (r.In is long inPt) inst.Start = Math.Min(inst.End, inPt * toSource);
        if (r.Out is long outPt) inst.End = Math.Min(inst.End, outPt * toSource);
        inst.Pos = inst.Start;
        inst.LoopsLeft = Math.Max(1, r.Loops ?? 1);
        inst.Envelope = r.Envelope;
        inst.Source = source;
        lock (_lock)
        {
            if (r.NoMultiple == true && _playing.Any(i => i.Sound == sound)) return;
            _playing.Add(inst);
        }
    }

    Instance? Create(SoundAsset sound)
    {
        var samples = sound.Samples;
        if (samples.Length == 0)
        {
            foreach (var e in sound.Movie.Errors) Console.WriteLine($"[audio] {sound.Movie.Name}: {e}");
            return null;
        }
        int ch = Math.Max(1, sound.Channels);
        return new Instance
        {
            Sound = sound,
            Samples = samples,
            Channels = ch,
            Step = sound.Rate / (double)OutputRate,
            End = samples.Length / ch,
            LoopsLeft = 1,
            Group = GroupOf(sound),
        };
    }

    public void StartStream(object owner, SoundAsset sound)
    {
        var inst = Create(sound);
        lock (_lock)
        {
            _playing.RemoveAll(i => i.Owner == owner);
            if (inst == null) return;
            inst.Owner = owner;
            _playing.Add(inst);
        }
    }

    public void StopStream(object owner) { lock (_lock) _playing.RemoveAll(i => i.Owner == owner); }

    /// <summary>Stops every sound (stopAllSounds), or those whose source matches.</summary>
    public void StopAll(Func<object?, bool>? which = null)
    {
        lock (_lock)
        {
            if (which == null) _playing.Clear();
            else _playing.RemoveAll(i => which(i.Source));
        }
    }

    /// <summary>Fades out (then stops) the sounds whose source matches, over the given time.</summary>
    public void FadeOut(Func<object?, bool> source, double seconds)
    {
        lock (_lock)
            foreach (var i in _playing)
                if (source(i.Source)) i.FadeStep = (float)(1 / Math.Max(0.01, seconds * OutputRate));
    }

    /// <summary>Stops the sounds started by a loaded movie (Flash stops them when the movie is unloaded).</summary>
    public void StopSource(object source) { lock (_lock) _playing.RemoveAll(i => i.Source == source); }

    /// <summary>The sounds playing now, as "Movie#id" (for the debug overlay).</summary>
    public List<string> Describe() { lock (_lock) return _playing.Select(i => $"{i.Sound.Movie.Name}#{i.Sound.Id}{(i.Owner != null ? " (stream)" : "")} [{i.Sound.Samples.Length} samples, {i.Sound.Channels} ch, {i.Sound.Rate} Hz]").ToList(); }

    /// <summary>Fills <paramref name="buffer"/> (interleaved stereo) with the next frames.</summary>
    public void Mix(Span<float> buffer)
    {
        lock (_lock) MixLocked(buffer);
    }

    void MixLocked(Span<float> buffer)
    {
        buffer.Clear();
        int frames = buffer.Length / 2;
        for (int n = _playing.Count - 1; n >= 0; n--)
        {
            var s = _playing[n];
            float group = GroupVolume[s.Group];
            bool done = false;
            for (int i = 0; i < frames; i++)
            {
                if (s.Pos >= s.End)
                {
                    if (--s.LoopsLeft <= 0) { done = true; break; }
                    s.Pos = s.Start + (s.Pos - s.End);
                }
                int i0 = (int)s.Pos;
                float t = (float)(s.Pos - i0);
                int i1 = Math.Min(i0 + 1, (int)s.End - 1);
                float l, r;
                if (s.Channels == 1)
                {
                    l = r = Lerp(s.Samples[i0], s.Samples[Math.Max(i0, i1)], t);
                }
                else
                {
                    l = Lerp(s.Samples[i0 * 2], s.Samples[Math.Max(i0, i1) * 2], t);
                    r = Lerp(s.Samples[i0 * 2 + 1], s.Samples[Math.Max(i0, i1) * 2 + 1], t);
                }
                if (s.Envelope is { Count: > 0 } env)
                {
                    var (el, er) = EnvelopeAt(env, s.Played);
                    l *= el;
                    r *= er;
                }
                if (s.FadeStep > 0)
                {
                    s.Gain -= s.FadeStep;
                    if (s.Gain <= 0) { done = true; break; }
                    l *= s.Gain;
                    r *= s.Gain;
                }
                buffer[i * 2] += l * group;
                buffer[i * 2 + 1] += r * group;
                s.Pos += s.Step;
                s.Played++;
            }
            if (done) _playing.RemoveAt(n);
        }
        for (int i = 0; i < buffer.Length; i++) buffer[i] = Math.Clamp(buffer[i] * Volume, -1f, 1f);
    }

    static float Lerp(float a, float b, float t) => a + (b - a) * t;

    /// <summary>Envelope levels (0..32768 → 0..1) interpolated at a position in 44.1 kHz frames.</summary>
    static (float, float) EnvelopeAt(List<long[]> env, long pos)
    {
        if (pos <= env[0][0]) return (env[0][1] / 32768f, env[0][2] / 32768f);
        for (int k = 1; k < env.Count; k++)
        {
            if (pos > env[k][0]) continue;
            var a = env[k - 1];
            var b = env[k];
            float t = b[0] == a[0] ? 1 : (pos - a[0]) / (float)(b[0] - a[0]);
            return ((a[1] + (b[1] - a[1]) * t) / 32768f, (a[2] + (b[2] - a[2]) * t) / 32768f);
        }
        var last = env[^1];
        return (last[1] / 32768f, last[2] / 32768f);
    }
}
