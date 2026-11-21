// Offline Windows app (21 Nov 2026: WebView2; before, the exe opened an Edge app window).
// Serves the published Blazor WebAssembly files (the wwwroot folder next to this exe) inside this process on
// http://localhost:<Port> and shows them in its own window with WebView2, the Edge engine built into Windows 10 / 11.
// Closing the window stops the app. A second start brings the running window to the front.
//
// The port is fixed on purpose: saved games, the undo history and the Endings Gallery live in localStorage, which
// belongs to the origin (host + port). Keeping http://localhost:51660 also lets the first start copy the saves of the
// earlier Edge app window (its profile %LOCALAPPDATA%\MyFathersWork\Browser) into the WebView2 profile.
// For a headless test: set WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS="--remote-debugging-port=9333 --mute-audio" and attach
// with Playwright (connect_over_cdp).

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using WindowsLauncher;

const int Port = 51660;
const string Title = "My Father's Work (Unofficial)";

var url = $"http://localhost:{Port}/";
var baseDir = AppContext.BaseDirectory;
var webRoot = Path.Combine(baseDir, "wwwroot");
// MFW_DATA_DIR: another data folder, for tests
var dataDir = Environment.GetEnvironmentVariable("MFW_DATA_DIR") is { Length: > 0 } testDir ? testDir
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyFathersWork");

if (!File.Exists(Path.Combine(webRoot, "index.html")))
{
    MessageBox.Show($"The app files were not found in\n{webRoot}\n\nKeep MyFathersWork.exe next to its wwwroot folder (unzip the whole release).",
                    Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    return 1;
}

// Already running (second start)? Bring its window to the front.
if (!PortIsFree(Port))
{
    if (await Launcher.AskRunningToShowAsync(Port))
        return 0;
    MessageBox.Show($"Port {Port} is used by another program, so the app cannot start.\nClose that program and try again.",
                    Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    return 1;
}

string? runtime = null;
try { runtime = CoreWebView2Environment.GetAvailableBrowserVersionString(); }
catch (WebView2RuntimeNotFoundException) { }
if (runtime == null)
{
    if (MessageBox.Show("This app needs the Microsoft Edge WebView2 Runtime, which is part of Windows 11 and of an up-to-date Windows 10.\n\nOpen the download page now?",
                        Title, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
        Process.Start(new ProcessStartInfo("https://go.microsoft.com/fwlink/p/?LinkId=2124703") { UseShellExecute = true });
    return 1;
}

MainForm? form = null;

var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = baseDir,
    WebRootPath = webRoot,
});
builder.Logging.ClearProviders();
builder.WebHost.ConfigureKestrel(o => o.Listen(IPAddress.Loopback, Port));
var server = builder.Build();

var types = new FileExtensionContentTypeProvider();
types.Mappings[".wasm"] = "application/wasm";
types.Mappings[".dat"] = "application/octet-stream";
types.Mappings[".blat"] = "application/octet-stream";
types.Mappings[".webmanifest"] = "application/manifest+json";
types.Mappings[".csv"] = "text/csv; charset=utf-8";
types.Mappings[".webp"] = "image/webp";

// Files change when a new release replaces the old one: always revalidate.
server.Use(async (ctx, next) =>
{
    ctx.Response.Headers.CacheControl = "no-cache";
    await next();
});
server.MapGet(Launcher.PingPath, () => Launcher.PingReply);
server.MapPost(Launcher.ShowPath, () =>
{
    form?.BeginInvoke(form.BringToFrontNow);
    return Launcher.PingReply;
});
server.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = types });
server.MapFallbackToFile("index.html"); // deep links such as /Gameplay go to the Blazor router

await server.StartAsync();

var userData = Path.Combine(dataDir, "WebView2");
SaveMigration.CopyFromEdgeProfile(Path.Combine(dataDir, "Browser"), userData);

// WinForms and WebView2 need a single-threaded apartment; the async top-level code runs in the thread pool
var ui = new Thread(() =>
{
    ApplicationConfiguration.Initialize();
    form = new MainForm(Title, url, userData, dataDir);
    Application.Run(form);
});
ui.SetApartmentState(ApartmentState.STA);
ui.Start();
ui.Join();

await server.StopAsync();
return 0;

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
