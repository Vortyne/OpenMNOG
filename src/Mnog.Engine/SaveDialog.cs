using System.Runtime.InteropServices;

namespace Mnog.Engine;

/// <summary>
/// Where the player saves files out of the game (the chronicle's wallpapers): on Windows, its "Save As" dialog
/// (comdlg32's GetSaveFileName); on macOS, AppleScript's "choose file name" (osascript, part of every Mac); on Linux,
/// straight into the Pictures folder, under a name not already taken. Asked on its own thread, as it waits for the
/// player; null when cancelled or unavailable.
/// </summary>
public static class SaveDialog
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner, hInstance;
        public string lpstrFilter;
        public string? lpstrCustomFilter;
        public int nMaxCustFilter, nFilterIndex;
        public IntPtr lpstrFile;
        public int nMaxFile;
        public string? lpstrFileTitle;
        public int nMaxFileTitle;
        public string? lpstrInitialDir, lpstrTitle;
        public int Flags;
        public short nFileOffset, nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData, lpfnHook;
        public string? lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved, FlagsEx;
    }

    [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool GetSaveFileNameW(ref OpenFileName ofn);

    const int OFN_OVERWRITEPROMPT = 0x2, OFN_NOCHANGEDIR = 0x8, OFN_PATHMUSTEXIST = 0x800, OFN_EXPLORER = 0x80000;

    /// <summary>Asks where to save (starting in <paramref name="folder"/>, named <paramref name="name"/>); calls back with the path.</summary>
    public static void Ask(string title, string folder, string name, string filterName, string extension, Action<string?> done)
    {
        var thread = new Thread(() =>
        {
            string? path = null;
            if (OperatingSystem.IsWindows())
            {
                const int max = 1024;
                var buffer = Marshal.AllocHGlobal(max * 2);
                try
                {
                    var initial = (name + "\0").ToCharArray();
                    Marshal.Copy(initial, 0, buffer, Math.Min(initial.Length, max));
                    var ofn = new OpenFileName
                    {
                        lStructSize = Marshal.SizeOf<OpenFileName>(),
                        lpstrFilter = $"{filterName} (*.{extension})\0*.{extension}\0",
                        lpstrFile = buffer,
                        nMaxFile = max,
                        lpstrInitialDir = folder,
                        lpstrTitle = title,
                        lpstrDefExt = extension,
                        Flags = OFN_OVERWRITEPROMPT | OFN_NOCHANGEDIR | OFN_PATHMUSTEXIST | OFN_EXPLORER,
                    };
                    if (GetSaveFileNameW(ref ofn)) path = Marshal.PtrToStringUni(buffer);
                }
                finally { Marshal.FreeHGlobal(buffer); }
            }
            else if (OperatingSystem.IsMacOS()) path = AskMac(title, folder, name, extension);
            else path = Unused(folder, name);
            done(path);
        }) { IsBackground = true };
        if (OperatingSystem.IsWindows()) thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    /// <summary>macOS: AppleScript's save dialog (it asks before replacing a file itself); null when cancelled.</summary>
    static string? AskMac(string title, string folder, string name, string extension)
    {
        static string Quote(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        string script = $"POSIX path of (choose file name with prompt {Quote(title)} default name {Quote(name)}" +
                        (Directory.Exists(folder) ? $" default location (POSIX file {Quote(folder)})" : "") + ")";
        try
        {
            var info = new System.Diagnostics.ProcessStartInfo("/usr/bin/osascript") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            info.ArgumentList.Add("-e");
            info.ArgumentList.Add(script);
            using var process = System.Diagnostics.Process.Start(info);
            if (process == null) return null;
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            // (Cancelled: it exits with an error.)
            if (process.ExitCode != 0 || output.Length == 0) return null;
            return Path.HasExtension(output) ? output : output + "." + extension;
        }
        catch (Exception e) when (e is System.ComponentModel.Win32Exception or InvalidOperationException) { return null; }
    }

    /// <summary>Linux: the file's path in the Pictures folder (made if missing), numbered if the name's taken.</summary>
    static string? Unused(string folder, string name)
    {
        if (string.IsNullOrEmpty(folder))
            folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Pictures");
        try { Directory.CreateDirectory(folder); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { return null; }
        string stem = Path.GetFileNameWithoutExtension(name), ext = Path.GetExtension(name);
        string path = Path.Combine(folder, name);
        for (int n = 2; File.Exists(path); n++) path = Path.Combine(folder, $"{stem} ({n}){ext}");
        return path;
    }
}
