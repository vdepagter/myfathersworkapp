using System.Text.Json;
using Microsoft.JSInterop;

namespace MyFathersWorkApp;

// The installable offline app (PWA), wrapping wwwroot/js/offline.js (window.mfwOffline): whether the app is cached,
// "a new version is ready", "Install as an app" and the narration downloads (wwwroot/js/audio-cache.js). UI code only.
// Layout/MainLayout.razor starts it; Shared/OfflinePanel.razor (Offline play), Shared/OfflineEntry.razor and
// Shared/UpdateBanner.razor show it.
public sealed class OfflineService(IJSRuntime js) : IDisposable
{
    private const string UpdatedFromKey = "mfwUpdatedFrom";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private DotNetObjectReference<OfflineService>? _Ref;

    public OfflineState State { get; private set; } = new();

    // The version the app ran before the player pressed Update (shown once as "Updated to ..."), else null
    public string? UpdatedFrom { get; private set; }

    public event Action? Changed;

    public async Task StartAsync()
    {
        if (_Ref != null) return;
        _Ref = DotNetObjectReference.Create(this);

        try
        {
            UpdatedFrom = await js.InvokeAsync<string?>("localStorage.getItem", UpdatedFromKey);
            if (UpdatedFrom != null) await js.InvokeVoidAsync("localStorage.removeItem", UpdatedFromKey);
            if (UpdatedFrom == AppInfo.Version) UpdatedFrom = null;
        }
        catch (JSException) { /* storage blocked */ }

        await js.InvokeVoidAsync("mfwOffline.subscribe", _Ref);
    }

    [JSInvokable]
    public void OnOfflineState(string json)
    {
        State = JsonSerializer.Deserialize<OfflineState>(json, JsonOptions) ?? new OfflineState();
        Changed?.Invoke();
    }

    public void ClearUpdatedFrom()
    {
        UpdatedFrom = null;
        Changed?.Invoke();
    }

    public async Task ApplyUpdateAsync()
    {
        try { await js.InvokeVoidAsync("localStorage.setItem", UpdatedFromKey, AppInfo.Version); }
        catch (JSException) { /* storage blocked */ }
        await js.InvokeVoidAsync("mfwOffline.applyUpdate");
    }

    public ValueTask DownloadAsync(string id) => js.InvokeVoidAsync("mfwOffline.download", id);
    public ValueTask DownloadManyAsync(IEnumerable<string> ids) => js.InvokeVoidAsync("mfwOffline.downloadMany", (object)ids.ToArray()); // one argument, not spread as params
    public ValueTask AnswerNarrationAsync(IEnumerable<string> ids) => js.InvokeVoidAsync("mfwOffline.answerNarration", (object)ids.ToArray());
    public ValueTask ApplyAsync(IEnumerable<string> adds, IEnumerable<string> removes) =>
        js.InvokeVoidAsync("mfwOffline.apply", adds.ToArray(), removes.ToArray());
    public ValueTask RemoveAllAsync()         => js.InvokeVoidAsync("mfwOffline.removeAll");
    public ValueTask CancelAsync(string id)   => js.InvokeVoidAsync("mfwOffline.cancel", id);
    public ValueTask RemoveAsync(string id)   => js.InvokeVoidAsync("mfwOffline.remove", id);
    public ValueTask InstallAsync()           => js.InvokeVoidAsync("mfwOffline.install");
    public ValueTask DismissErrorAsync()      => js.InvokeVoidAsync("mfwOffline.dismissError");
    public ValueTask RefreshAsync()           => js.InvokeVoidAsync("mfwOffline.refresh");

    public void Dispose() => _Ref?.Dispose();
}

public sealed class OfflineState
{
    public bool              Supported     { get; set; }
    public bool              Available     { get; set; }
    public int?              ShellProgress { get; set; }
    public bool              Update        { get; set; }
    public bool              Installable   { get; set; }
    public bool              IosHint       { get; set; }
    public bool              FirefoxHint   { get; set; }
    public bool              Blocked       { get; set; }
    public bool              AskNarration  { get; set; }
    public bool              Asked         { get; set; }
    public bool              Standalone    { get; set; }
    public bool              Online        { get; set; } = true;
    public bool              IndexMissing  { get; set; }
    public long              Usage         { get; set; }
    public long              Quota         { get; set; }
    public string            Error         { get; set; } = "";
    public List<OfflinePack> Packs         { get; set; } = new();
}

public sealed class OfflinePack
{
    public string Id          { get; set; } = "";
    public string Voice       { get; set; } = "";
    public string Scenario    { get; set; } = "";
    public int    Files       { get; set; }
    public long   Bytes       { get; set; }
    public int    StoredFiles { get; set; }
    public long   StoredBytes { get; set; }
    public bool   Wanted      { get; set; }
    public bool   Downloading { get; set; }
    public bool   Queued      { get; set; } // waiting for the download before it (Download all, the narration choice)

    public bool Complete => Files > 0 && StoredFiles >= Files;

    // Oliver first, by the scenario order of the menu, then the original recordings
    public int Order => Scenario switch
    {
        "TheCostOfDisease" => 0,
        "FearOfTheUnknown" => 1,
        "ATimeOfWar"       => 2,
        _                  => Voice == "male" ? 10 : 11
    };

    public string Name(GlobalData globalData) => Voice switch
    {
        "oliver" => globalData.GetLocalizedUITag(GlobalTags.Offline_PackOliver).FormatWithReplacement(0, globalData.GetLocalizedUITag("Scenario_" + Scenario)),
        "male"   => globalData.GetLocalizedUITag(GlobalTags.Offline_PackMale),
        "female" => globalData.GetLocalizedUITag(GlobalTags.Offline_PackFemale),
        _        => Id
    };

    public static string Size(long bytes) => bytes >= 1_000_000_000
        ? (bytes / 1_000_000_000.0).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " GB"
        : bytes <= 0 ? "0 MB" : $"{Math.Max(1, (int)Math.Round(bytes / 1_000_000.0))} MB";
}
