namespace Mnog.Engine;

/// <summary>
/// Where the game keeps the player's things: the save (sharedobjects/), slot pictures (pictures/), backups of the save
/// (backups/) and the window's state (window.json), all in one folder. In order:
/// MNOG_SAVEDIR (the sharedobjects folder itself, for test runs on a copy), then the "Folder" of OpenMNOG.ini's [Save]
/// section (beside the game folder), then Documents\My Games\OpenMNOG on Windows (elsewhere, OpenMNOG in the user's
/// application data folder).
/// </summary>
public static class SaveLocation
{
    public const string IniName = "OpenMNOG.ini";

    /// <summary>The folder (until <see cref="Resolve"/>: the default one).</summary>
    public static string Folder { get; private set; } = Default();

    /// <summary>The save's own folder (the Flash-style shared objects).</summary>
    public static string SharedObjects { get; private set; } = Path.Combine(Folder, "sharedobjects");

    /// <summary>Where the folder came from, for the log.</summary>
    public static string Source { get; private set; } = "default";

    /// <summary>Settles the folder for a game in <paramref name="gameDir"/> (its ini is beside it).</summary>
    public static void Resolve(string gameDir)
    {
        if (Environment.GetEnvironmentVariable("MNOG_SAVEDIR") is { Length: > 0 } saveDir)
        {
            SharedObjects = Path.GetFullPath(saveDir);
            Folder = Path.GetDirectoryName(SharedObjects) ?? SharedObjects;
            Source = "MNOG_SAVEDIR";
            return;
        }
        var root = Path.GetDirectoryName(Path.GetFullPath(gameDir)) ?? gameDir;
        var ini = Path.Combine(root, IniName);
        if (ReadIni(ini, "Save", "Folder") is { Length: > 0 } configured)
        {
            Folder = Path.GetFullPath(Path.Combine(root, Expand(configured)));
            Source = IniName;
        }
        else
        {
            Folder = Default();
            Source = "default";
            MoveFromOldDefault();
        }
        SharedObjects = Path.Combine(Folder, "sharedobjects");
    }

    static string Default()
    {
        if (OperatingSystem.IsWindows() && Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) is { Length: > 0 } documents)
            return Path.Combine(documents, "My Games", "OpenMNOG");
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenMNOG");
    }

    /// <summary>The save from before it moved to the default folder (%APPDATA%\MataNuiOnlineGame), copied there once
    /// when the default folder has none yet. The old folder is left as it was.</summary>
    static void MoveFromOldDefault()
    {
        var old = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MataNuiOnlineGame");
        if (Directory.Exists(Path.Combine(Folder, "sharedobjects")) || !Directory.Exists(Path.Combine(old, "sharedobjects"))) return;
        try
        {
            foreach (var file in Directory.GetFiles(old, "*", SearchOption.AllDirectories))
            {
                var to = Path.Combine(Folder, Path.GetRelativePath(old, file));
                Directory.CreateDirectory(Path.GetDirectoryName(to)!);
                File.Copy(file, to, overwrite: false);
            }
            Source = $"default (save copied from {old})";
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Source = $"default (couldn't copy the save from {old}: {e.Message})";
        }
    }

    /// <summary>Environment variables (%USERPROFILE%) and a leading ~ (the home folder) filled in.</summary>
    static string Expand(string path)
    {
        path = Environment.ExpandEnvironmentVariables(path.Trim().Trim('"'));
        if (path == "~" || path.StartsWith("~/") || path.StartsWith("~\\"))
            path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + path[1..];
        return path;
    }

    /// <summary>A key's value in an ini file's section (null without the file, section or key). Lines starting with ;
    /// or # are comments; names are case-insensitive.</summary>
    static string? ReadIni(string file, string section, string key)
    {
        if (!File.Exists(file)) return null;
        string? current = null;
        foreach (var raw in File.ReadLines(file))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line[0] is ';' or '#') continue;
            if (line[0] == '[' && line[^1] == ']') { current = line[1..^1].Trim(); continue; }
            int eq = line.IndexOf('=');
            if (eq > 0 && string.Equals(current, section, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(line[..eq].Trim(), key, StringComparison.OrdinalIgnoreCase))
                return line[(eq + 1)..].Trim();
        }
        return null;
    }
}
