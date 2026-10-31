using System.IO.Compression;

namespace Mnog.Engine;

/// <summary>
/// The high-quality audio, a separate download (OpenMNOG-hq-audio-pack-v{version}.zip, built by release.sh): its one
/// folder, OpenMNOG-audio, holds hq/ and soundtrack/. Left in the game's folder, the game unpacks the zip into game/audio
/// (AudioPackMenu), where the files are in the repository too; unzipped by hand, the folder is found wherever that left
/// it. The lists of what there is (hq.json, soundtrack.json) are in game/audio in every download.
/// </summary>
public static class AudioPack
{
    /// <summary>The project page's page for it, with the download (site/audio).</summary>
    public const string Page = "https://vortyne.github.io/OpenMNOG/audio/";

    /// <summary>The zip's size in MB, as Windows shows it (what release.sh's audio target prints).</summary>
    public const int SizeMb = 513;

    const string Folder = "OpenMNOG-audio";
    const string ZipName = "OpenMNOG-hq-audio-pack";

    /// <summary>The folder the program and game/ are in.</summary>
    public static string GameFolder(string gameDir) => Path.GetFullPath(Path.Combine(gameDir, ".."));

    /// <summary>Where unzipping by hand leaves the OpenMNOG-audio folder in a folder: right there, or (Windows' Extract
    /// All) inside one named after the zip.</summary>
    static List<string> Unzipped(string root)
    {
        var folders = new List<string> { Path.Combine(root, Folder), Path.Combine(root, Folder, Folder) };
        try { folders.AddRange(Directory.EnumerateDirectories(root, ZipName + "*").Select(d => Path.Combine(d, Folder))); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
        return folders;
    }

    /// <summary>The path of a file of hq/ or soundtrack/: where it is, or else where it would be in game/audio.</summary>
    public static string File(string gameDir, string sub, string name)
    {
        var own = Path.GetFullPath(Path.Combine(gameDir, "audio", sub, name));
        if (System.IO.File.Exists(own)) return own;
        foreach (var root in new[] { GameFolder(gameDir), Path.GetFullPath(gameDir) })
            foreach (var folder in Unzipped(root))
                if (Path.Combine(folder, sub, name) is var file && System.IO.File.Exists(file)) return file;
        return own;
    }

    /// <summary>The downloaded zip, if it's in the game's folder (or game/).</summary>
    public static string? Zip(string gameDir)
    {
        foreach (var root in new[] { GameFolder(gameDir), Path.GetFullPath(gameDir) })
        {
            try
            {
                if (Directory.EnumerateFiles(root, ZipName + "*.zip").FirstOrDefault() is { } zip) return zip;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
        }
        return null;
    }

    /// <summary>Unpacks the zip into game/audio: first into a folder of its own, moved in when all of it is there, so a
    /// part of it is never taken for the audio. Throws if it can't be done.</summary>
    public static void Unpack(string zip, string gameDir, Action<float> progress, CancellationToken cancel)
    {
        var audio = Path.GetFullPath(Path.Combine(gameDir, "audio"));
        var part = Path.Combine(GameFolder(gameDir), Folder + ".partial");
        if (Directory.Exists(part)) Directory.Delete(part, true);
        try
        {
            using var archive = ZipFile.OpenRead(zip);
            long total = Math.Max(1, archive.Entries.Sum(e => e.Length)), done = 0;
            foreach (var entry in archive.Entries)
            {
                cancel.ThrowIfCancellationRequested();
                var path = Path.GetFullPath(Path.Combine(part, entry.FullName));
                if (!path.StartsWith(part + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("A file in the zip is outside its folder: " + entry.FullName);
                if (entry.Name == "") continue;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                entry.ExtractToFile(path);
                done += entry.Length;
                progress((float)done / total);
            }
            var unpacked = Path.Combine(part, Folder);
            if (!Directory.Exists(unpacked)) throw new InvalidDataException("No " + Folder + " folder in the zip");
            foreach (var file in Directory.EnumerateFiles(unpacked, "*", SearchOption.AllDirectories).ToList())
            {
                var path = Path.Combine(audio, Path.GetRelativePath(unpacked, file));
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                System.IO.File.Move(file, path, true);
            }
        }
        finally
        {
            try { if (Directory.Exists(part)) Directory.Delete(part, true); }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
        }
    }
}
