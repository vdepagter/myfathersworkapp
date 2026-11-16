#:sdk Microsoft.NET.Sdk.Web

// Serves the web app on the local network, to test on a phone before pushing (needs the .NET 10 SDK):
//
//   dotnet build MyFathersWorkApp/MyFathersWorkApp.sln -p:DebugType=none -p:DebugSymbols=false
//   dotnet run tools/lan-serve.cs [port]          (default 5199; open http://<laptop LAN IP>:<port> on the phone)
//
// Why not `dotnet run --urls http://0.0.0.0:...`: the Blazor WebAssembly dev server (DevServer 9.0.5) answers a
// byte-range request with "206 Partial Content" but sends the whole file from byte 0. Audio elements ask for ranges
// while they play, so on the dev server a narration could jump back to its start while the highlight kept going.
// This server uses plain UseStaticFiles, which serves ranges correctly (as GitHub Pages and the Windows launcher do).
//
// Files come from the project's wwwroot first (JS, CSS, CSV and audio changes show on a reload, no build needed),
// then the Debug build output (_framework: C# and Razor changes need the build again), then the scoped CSS bundle.
// The build leaves out debug symbols: the boot list of a normal Debug build names a .pdb that only the dev server finds.
// Windows Firewall must allow the port on the private network once (admin PowerShell):
//   New-NetFirewallRule -DisplayName "MFW dev 5199" -Direction Inbound -Protocol TCP -LocalPort 5199 -Profile Private -Action Allow

using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

int port = args.Length > 0 && int.TryParse(args[0], out int p) ? p : 5199;
string tools   = Path.GetDirectoryName(Path.GetFullPath(AppContext.GetData("EntryPointFilePath") as string ?? "tools/lan-serve.cs"))!;
string project = Path.GetFullPath(Path.Combine(tools, "..", "MyFathersWorkApp", "MyFathersWorkApp"));
string[] roots =
[
    Path.Combine(project, "wwwroot"),
    Path.Combine(project, "bin", "Debug", "net9.0", "wwwroot"),
    Path.Combine(project, "obj", "Debug", "net9.0", "scopedcss", "bundle")
];
if (!File.Exists(Path.Combine(roots[1], "_framework", "blazor.webassembly.js")))
{
    Console.Error.WriteLine("Build first: dotnet build MyFathersWorkApp/MyFathersWorkApp.sln -p:DebugType=none -p:DebugSymbols=false");
    return 1;
}

var files = new CompositeFileProvider(roots.Where(Directory.Exists).Select(r => (IFileProvider)new PhysicalFileProvider(r)));
var types = new FileExtensionContentTypeProvider();
types.Mappings[".wasm"] = "application/wasm";
types.Mappings[".dat"] = "application/octet-stream";
types.Mappings[".blat"] = "application/octet-stream";
types.Mappings[".webmanifest"] = "application/manifest+json";
types.Mappings[".csv"] = "text/csv; charset=utf-8";
types.Mappings[".webp"] = "image/webp";

var builder = WebApplication.CreateSlimBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.ConfigureKestrel(o => o.Listen(IPAddress.Any, port));
var app = builder.Build();
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.CacheControl = "no-cache";   // every reload picks up changed files
    await next();
});
app.UseStaticFiles(new StaticFileOptions { FileProvider = files, ContentTypeProvider = types });
app.MapFallback(async ctx =>                       // deep links such as /Gameplay go to the Blazor router
{
    ctx.Response.ContentType = "text/html; charset=utf-8";
    await ctx.Response.SendFileAsync(files.GetFileInfo("index.html"));
});

Console.WriteLine($"Serving on port {port}:");
foreach (var address in Dns.GetHostAddresses(Dns.GetHostName()).Where(a => a.AddressFamily == AddressFamily.InterNetwork))
    Console.WriteLine($"  http://{address}:{port}/");
Console.WriteLine("Ctrl+C stops it.");
await app.RunAsync();
return 0;
