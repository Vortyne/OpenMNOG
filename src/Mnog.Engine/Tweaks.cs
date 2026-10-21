using System.Text.RegularExpressions;
using Mnog.Format;

namespace Mnog.Engine;

/// <summary>
/// The settings' Tweaks: changes from fans' patches for the original game (MNOGIM, by Ontocron), each off unless
/// chosen (the save's "matoran", "colors", "clones"; the Player sets IsOn). "matoran" says Matoran in place of Tohunga
/// in the game's text. "colors" and "clones" are the Color Consistency and Clone Replacement patches' changes to the
/// pictures: a movie's tweaks/KEY.json, laid over its movie.json as it loads (docs/FORMAT.md).
/// </summary>
public static partial class Tweaks
{
    /// <summary>The tweaks that change movies' pictures, in the order they're laid on.</summary>
    public static readonly string[] Pictures = ["colors", "clones", "pakari", "ruru-accurate", "ruru-classic"];

    public static Func<string, bool> IsOn = _ => false;

    /// <summary>A movie's tweak files: the changed and added symbols, and main-timeline frames' placed objects.</summary>
    internal sealed class TweakDoc
    {
        public List<SymbolDoc> Symbols { get; set; } = [];
        /// <summary>Main-timeline frames (1-based) whose ops are replaced.</summary>
        public Dictionary<string, List<OpDoc>>? Frames { get; set; }
        /// <summary>Symbols used in place of its own while another tweak is on too (its own version of what both
        /// change: the Pakari redrawn with Color consistency's eye colours).</summary>
        public Dictionary<string, List<SymbolDoc>>? With { get; set; }
    }

    /// <summary>The picture tweaks that are on, laid over a movie's document as it loads.</summary>
    public static void Apply(MovieDoc doc, string dir)
    {
        foreach (var key in Pictures)
        {
            var file = Path.Combine(dir, "tweaks", key + ".json");
            if (!IsOn(key) || !File.Exists(file)) continue;
            var tweak = Json.Load<TweakDoc>(file);
            var index = new Dictionary<int, int>();
            for (int i = 0; i < doc.Symbols.Count; i++) index[doc.Symbols[i].Id] = i;
            foreach (var s in tweak.Symbols)
                if (index.TryGetValue(s.Id, out int i)) doc.Symbols[i] = s;
                else doc.Symbols.Add(s);
            foreach (var (other, symbols) in tweak.With ?? [])
                if (IsOn(other))
                    foreach (var s in symbols)
                        if (index.TryGetValue(s.Id, out int i)) doc.Symbols[i] = s;
            foreach (var (frame, ops) in tweak.Frames ?? [])
                if (int.TryParse(frame, out int n) && n >= 1 && n <= doc.Timeline.Frames.Count)
                    doc.Timeline.Frames[n - 1].Ops = ops;
        }
    }

    /// <summary>Whether a movie has picture tweaks (it's loaded again when they're switched).</summary>
    public static bool HasPictures(string dir) => Directory.Exists(Path.Combine(dir, "tweaks"));

    /// <summary>The text with Matoran for Tohunga, in the same case, when that tweak is on.</summary>
    public static string Matoran(string text) => IsOn("matoran") ? Swap(text, true) : text;

    /// <summary>The text with Matoran for Tohunga (toMatoran) or back, each in the same case.</summary>
    public static string Swap(string text, bool toMatoran)
    {
        var (from, to) = toMatoran ? ("tohunga", "matoran") : ("matoran", "tohunga");
        if (text.IndexOf(from, StringComparison.OrdinalIgnoreCase) < 0) return text;
        return (toMatoran ? TohungaWord() : MatoranWord()).Replace(text, m =>
            m.Value == m.Value.ToUpperInvariant() ? to.ToUpperInvariant() : char.IsUpper(m.Value[0]) ? char.ToUpperInvariant(to[0]) + to[1..] : to);
    }

    [GeneratedRegex("tohunga", RegexOptions.IgnoreCase)]
    private static partial Regex TohungaWord();

    [GeneratedRegex("matoran", RegexOptions.IgnoreCase)]
    private static partial Regex MatoranWord();
}
