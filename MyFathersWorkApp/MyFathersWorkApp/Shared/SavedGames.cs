using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyFathersWorkApp;

// One game kept in Saved Games, described from its newest screen when it was put away
public class SavedGameInfo
{
    public string     Id         { get; set; } = string.Empty; // "<ScenarioId>-<FlavorSeed>", one per playthrough
    public ScenarioId Scenario   { get; set; }
    public string[]   Players    { get; set; } = [];
    public string     Town       { get; set; } = string.Empty;
    public Generation Generation { get; set; }
    public Years      Years      { get; set; }
    public string     Ending     { get; set; } = string.Empty; // the ending title from the Endings Gallery, empty while unfinished
    public DateTime   LastPlayed { get; set; }                 // UTC
    public bool       History    { get; set; }                 // false: kept without its undo history (browser storage was full)
}

// Saved Games: every game is kept on this device. The game being played stays in the keys of GlobalData.Persistence.cs;
// New Game and loading another game first pack it into "savedGame/<id>" (wwwroot/js/saves.js) and list it in "savedGames".
// Opening a saved game packs the current one away and unpacks the chosen one in its place, history included, so Back works
// through the whole game again. A game is keyed by scenario + FlavorSeed, so putting the same game away again replaces it.
public class SavedGamesService(IJSRuntime js, GalleryService gallery)
{
    private const string _INDEX_KEY  = "savedGames";
    private const string _GAME_PREFIX = "savedGame/";

    public async Task<List<SavedGameInfo>> ListAsync()
    {
        try
        {
            string? json = await js.InvokeAsync<string?>("localStorage.getItem", _INDEX_KEY);
            return string.IsNullOrEmpty(json) ? [] : JsonConvert.DeserializeObject<List<SavedGameInfo>>(json) ?? [];
        }
        catch (Exception e)
        {
            Console.WriteLine($"Saved games: could not read the list ({e.Message})");
            return [];
        }
    }

    private async Task WriteListAsync(List<SavedGameInfo> games)
    {
        await js.InvokeVoidAsync("localStorage.setItem", _INDEX_KEY, JsonConvert.SerializeObject(games));
    }

    // The game being played, described as it would be listed; null when there is none
    public async Task<SavedGameInfo?> CurrentAsync()
    {
        string? base64 = await js.InvokeAsync<string?>("localStorage.getItem", "continue");
        if (string.IsNullOrEmpty(base64)) return null;

        string? time = await js.InvokeAsync<string?>("localStorage.getItem", "continueTime");
        return await DescribeAsync(base64, time);
    }

    private async Task<SavedGameInfo?> DescribeAsync(string base64, string? time)
    {
        try
        {
            string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            JObject jo  = JObject.Parse(json);

            GlobalData saved = new GlobalData();
            JsonConvert.PopulateObject(json, saved, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Reuse, NullValueHandling = NullValueHandling.Ignore });
            if (saved.ScenarioId == ScenarioId.None) return null;

            // The scenario's Vars are not created by a bare GlobalData, so the seed is read from the JSON
            long seed = jo.Descendants().OfType<JProperty>().FirstOrDefault(p => p.Name == nameof(ScenarioVars.FlavorSeed))?.Value.Value<long>() ?? 0;

            GalleryData  galleryData = await gallery.GetAsync();
            GalleryGame? finished    = galleryData.Games.FirstOrDefault(game => game.Id == $"{saved.ScenarioId}:{seed}");

            return new SavedGameInfo
            {
                Id         = $"{saved.ScenarioId}-{seed}",
                Scenario   = saved.ScenarioId,
                Players    = saved.PlayersNum >= 2 ? saved.GetActivePlayers() : [],
                Town       = saved.TownName,
                Generation = saved.Generation,
                Years      = saved.Years,
                Ending     = finished?.Title ?? string.Empty,
                LastPlayed = DateTime.TryParse(time, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime played) ? played.ToUniversalTime() : DateTime.UtcNow,
                History    = true
            };
        }
        catch (Exception e)
        {
            Console.WriteLine($"Saved games: could not read the current game ({e.Message})");
            return null;
        }
    }

    // Puts the game being played away in Saved Games (before New Game or loading another game). Keeps it without its undo
    // history when the browser storage is too full for the whole game.
    public async Task ArchiveCurrentAsync()
    {
        SavedGameInfo? current = await CurrentAsync();
        if (current == null) return;

        string key = _GAME_PREFIX + current.Id;
        if (!await js.InvokeAsync<bool>("mfwSaves.archive", key, true))
        {
            current.History = false;
            if (!await js.InvokeAsync<bool>("mfwSaves.archive", key, false))
            {
                Console.WriteLine("Saved games: browser storage is full, the previous game could not be kept");
                return;
            }
        }

        List<SavedGameInfo> games = await ListAsync();
        games.RemoveAll(game => game.Id == current.Id);
        games.Add(current);
        await WriteListAsync(games);
    }

    // Makes a saved game the game being played (the caller then loads it like Continue). False when it could not be read.
    public async Task<bool> OpenAsync(string id)
    {
        await ArchiveCurrentAsync();
        if (!await js.InvokeAsync<bool>("mfwSaves.restore", _GAME_PREFIX + id)) return false;

        await DeleteAsync(id);
        return true;
    }

    public async Task DeleteAsync(string id)
    {
        await js.InvokeVoidAsync("localStorage.removeItem", _GAME_PREFIX + id);
        List<SavedGameInfo> games = await ListAsync();
        if (games.RemoveAll(game => game.Id == id) > 0) await WriteListAsync(games);
    }

    // Removes the game being played (continue, continueTime, undo history); the Endings Gallery is not touched
    public async Task DeleteCurrentAsync() => await js.InvokeVoidAsync("mfwSaves.clearCurrent");

    // The save code of a saved game, as the pause menu shows it for the game being played
    public async Task<string> SaveCodeAsync(string id) => await js.InvokeAsync<string?>("mfwSaves.saveCode", _GAME_PREFIX + id) ?? string.Empty;
}
