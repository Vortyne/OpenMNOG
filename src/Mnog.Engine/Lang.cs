using System.Globalization;
using System.Text.Json.Nodes;

namespace Mnog.Engine;

/// <summary>
/// The menus in the chosen language (Settings > Language: "eng", "deu", "fre"): game/files/menu-text.json gives, for
/// each language, its text for an English string (the settings dialogs' labels, the engine's menus, the settings' hints).
/// A string it has no translation for stays English. Strings with "{0}", "{1}" are formats, filled in after translating.
/// </summary>
public static class Lang
{
    static Dictionary<string, Dictionary<string, string>> _table = [];

    /// <summary>The language now chosen (the game's save "language").</summary>
    public static Func<string> Current = () => "eng";

    /// <summary>Reads game/files/menu-text.json (if it's there).</summary>
    public static void Load(string gameDir)
    {
        var file = Path.Combine(gameDir, "files", "menu-text.json");
        if (!File.Exists(file)) return;
        try
        {
            var table = new Dictionary<string, Dictionary<string, string>>();
            if (JsonNode.Parse(File.ReadAllText(file)) is JsonObject root)
                foreach (var (lang, strings) in root)
                    if (strings is JsonObject o)
                        table[lang] = o.Where(kv => kv.Value is JsonValue).ToDictionary(kv => kv.Key, kv => kv.Value!.GetValue<string>());
            _table = table;
        }
        catch (Exception e) when (e is System.Text.Json.JsonException or IOException or InvalidOperationException) { }
    }

    /// <summary>The string in the chosen language (or as it is).</summary>
    public static string T(string english) =>
        Current() is var lang && lang != "eng" && _table.TryGetValue(lang, out var t) && t.TryGetValue(english, out var s) ? s : english;

    /// <summary>A format ("Slot {0}") in the chosen language, filled in.</summary>
    public static string F(string english, params object[] args) => string.Format(CultureInfo.InvariantCulture, T(english), args);

    /// <summary>Whether the string has a translation in the chosen language.</summary>
    public static bool Has(string english, out string translated)
    {
        translated = T(english);
        return !ReferenceEquals(translated, english) && translated != english;
    }

    /// <summary>Dates (a save slot's) in the chosen language's way.</summary>
    public static CultureInfo Culture => Current() switch
    {
        "deu" => CultureInfo.GetCultureInfo("de-DE"),
        "fre" => CultureInfo.GetCultureInfo("fr-FR"),
        _ => CultureInfo.GetCultureInfo("en-US"),
    };
}
