namespace WindowsLauncher;

// Until 21 Nov 2026 the app ran in an Edge app window with the profile %LOCALAPPDATA%\MyFathersWork\Browser. Its saved
// games, undo history, Endings Gallery and settings are localStorage of http://localhost:51660, the same origin the
// WebView2 window uses. On the first start (no WebView2 profile yet) the Local Storage folder is copied across, so
// everything carries over. Both are Chromium profiles with the same layout for this folder.
internal static class SaveMigration
{
    public static void CopyFromEdgeProfile(string edgeProfile, string webView2UserData)
    {
        try
        {
            var source = Path.Combine(edgeProfile, "Default", "Local Storage");
            var target = Path.Combine(webView2UserData, "EBWebView", "Default", "Local Storage");
            if (!Directory.Exists(source) || Directory.Exists(target))
                return;
            // An Edge app window of the old version still open: try again on the next start
            if (IsLocked(Path.Combine(edgeProfile, "lockfile")))
                return;

            var staging = target + ".copying";
            if (Directory.Exists(staging))
                Directory.Delete(staging, recursive: true);
            CopyDirectory(source, staging);
            Directory.Move(staging, target);
        }
        catch
        {
            // Nothing lost: the old profile stays where it is, and saves can still be moved with a save code
        }
    }

    private static void CopyDirectory(string from, string to)
    {
        Directory.CreateDirectory(to);
        foreach (var file in Directory.GetFiles(from))
        {
            var name = Path.GetFileName(file);
            if (name.Equals("LOCK", StringComparison.OrdinalIgnoreCase))
                continue; // LevelDB creates its own
            File.Copy(file, Path.Combine(to, name));
        }
        foreach (var dir in Directory.GetDirectories(from))
            CopyDirectory(dir, Path.Combine(to, Path.GetFileName(dir)));
    }

    private static bool IsLocked(string path)
    {
        if (!File.Exists(path))
            return false;
        try
        {
            using var _ = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
    }
}
