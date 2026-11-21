using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyFathersWorkApp;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<AudioService>();
builder.Services.AddScoped<GalleryService>();
builder.Services.AddScoped<LogBookService>();
builder.Services.AddScoped<SavedGamesService>();
builder.Services.AddScoped<GameOptions>();
builder.Services.AddScoped<GuideService>();
builder.Services.AddScoped<OfflineService>();

await builder.Build().RunAsync();