// Offline Windows launcher for the companion app.
// Serves the published Blazor WebAssembly files (the wwwroot folder next to this exe) on http://localhost:<Port>
// and opens them in an Edge app window with its own browser profile. The server stops when the last app window
// closes (Chromium holds <profile>\lockfile while it runs), so a window never outlives its server.
// Without Edge it opens the default browser and shows a console window to stop the server.
//
// The port is fixed on purpose: saved games, the undo history and the Endings Gallery live in the browser's
// localStorage, which belongs to the origin (host + port). A different port would show an empty app.

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Win32;

const int Port = 51660;
const string PingPath = "/__mfw-launcher";
const string PingReply = "my-fathers-work";
const string Title = "My Father's Work - Companion App";

var url = $"http://localhost:{Port}/";
var baseDir = AppContext.BaseDirectory;
var webRoot = Path.Combine(baseDir, "wwwroot");
// Edge profile of the app window: holds the saves (localStorage), separate from the user's own browsing
var profileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyFathersWork", "Browser");
var edge = FindEdge();

if (!File.Exists(Path.Combine(webRoot, "index.html")))
{
    Native.Message($"The app files were not found in\n{webRoot}\n\nKeep MyFathersWork.exe next to its wwwroot folder (unzip the whole release).", Title, error: true);
    return 1;
}

// Already running (second double-click)? Just open another window on the running instance.
if (!PortIsFree(Port))
{
    if (await IsOurServerAsync())
    {
        OpenApp();
        return 0;
    }
    Native.Message($"Port {Port} is used by another program, so the app cannot start.\nClose that program and try again.", Title, error: true);
    return 1;
}

var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = baseDir,
    WebRootPath = webRoot,
});
builder.Logging.ClearProviders();
builder.WebHost.ConfigureKestrel(o => o.Listen(IPAddress.Loopback, Port));

var app = builder.Build();

var types = new FileExtensionContentTypeProvider();
types.Mappings[".wasm"] = "application/wasm";
types.Mappings[".dat"] = "application/octet-stream";
types.Mappings[".blat"] = "application/octet-stream";
types.Mappings[".webmanifest"] = "application/manifest+json";
types.Mappings[".csv"] = "text/csv; charset=utf-8";
types.Mappings[".webp"] = "image/webp";

// Files change when a new release is unzipped over the old one: always revalidate.
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.CacheControl = "no-cache";
    await next();
});
app.MapGet(PingPath, () => PingReply);
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = types });
app.MapFallbackToFile("index.html"); // deep links such as /Gameplay go to the Blazor router

await app.StartAsync();

if (edge != null)
{
    OpenApp();
    await WaitForEdgeProfileClosedAsync();
    await app.StopAsync();
    return 0;
}

// No Edge: default browser, and a console window to stop the server
Native.ShowConsole(Title);
Console.WriteLine("My Father's Work - Fan Made Companion App");
Console.WriteLine();
Console.WriteLine($"  Running at {url}");
Console.WriteLine("  The app opens in your browser. If it does not, open the address above.");
Console.WriteLine();
Console.WriteLine("  Keep this window open while you play. Close it (or press Ctrl+C) to stop.");
OpenApp();
await app.WaitForShutdownAsync();
return 0;

void OpenApp()
{
    if (edge != null)
    {
        try
        {
            var psi = new ProcessStartInfo(edge) { UseShellExecute = false };
            psi.ArgumentList.Add($"--user-data-dir={profileDir}");
            psi.ArgumentList.Add($"--app={url}");
            psi.ArgumentList.Add("--no-first-run");
            psi.ArgumentList.Add("--no-default-browser-check");
            Process.Start(psi);
            return;
        }
        catch
        {
            // fall through to the default browser
        }
    }
    try
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
    catch
    {
        Native.Message($"Could not open a browser. Open {url} yourself.", Title, error: false);
    }
}

// Chromium keeps <user-data-dir>\lockfile open (not shared) while any window of that profile is open.
async Task WaitForEdgeProfileClosedAsync()
{
    var lockFile = Path.Combine(profileDir, "lockfile");
    var started = Stopwatch.StartNew();
    while (!IsLocked(lockFile))
    {
        if (started.Elapsed > TimeSpan.FromSeconds(60))
            return; // Edge never came up; nothing to wait for
        await Task.Delay(500);
    }
    while (IsLocked(lockFile))
        await Task.Delay(2000);
}

static bool IsLocked(string path)
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

static bool PortIsFree(int port)
{
    try
    {
        var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        listener.Stop();
        return true;
    }
    catch (SocketException)
    {
        return false;
    }
}

async Task<bool> IsOurServerAsync()
{
    try
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        return await http.GetStringAsync($"http://127.0.0.1:{Port}{PingPath}") == PingReply;
    }
    catch
    {
        return false;
    }
}

static string? FindEdge()
{
    if (!OperatingSystem.IsWindows())
        return null;
    foreach (var hive in new[] { Registry.CurrentUser, Registry.LocalMachine })
    {
        using var key = hive.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe");
        if (key?.GetValue(null) is string path && File.Exists(path))
            return path;
    }
    foreach (var folder in new[] { Environment.SpecialFolder.ProgramFilesX86, Environment.SpecialFolder.ProgramFiles })
    {
        var path = Path.Combine(Environment.GetFolderPath(folder), @"Microsoft\Edge\Application\msedge.exe");
        if (File.Exists(path))
            return path;
    }
    return null;
}

static partial class Native
{
    [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AllocConsole();

    public static void Message(string text, string caption, bool error) =>
        MessageBox(IntPtr.Zero, text, caption, error ? 0x10u : 0x40u); // MB_ICONERROR / MB_ICONINFORMATION

    public static void ShowConsole(string title)
    {
        AllocConsole();
        Console.Title = title;
    }
}
