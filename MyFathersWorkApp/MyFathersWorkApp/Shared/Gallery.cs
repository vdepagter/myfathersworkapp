using System.Text.RegularExpressions;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyFathersWorkApp;

// Endings Gallery data: remembered players, finished games (winner, ending) and the branches reached, kept on this device
// in its own localStorage key. Never part of GlobalData, so New Game, load and undo do not touch it. A game is keyed by
// scenario + FlavorSeed (unique per playthrough, kept by undo and load), so showing its ending again replaces the record.
public class GalleryData
{
    public int                           Version   { get; set; } = 1;
    public List<GalleryPlayer>           Players   { get; set; } = [];
    public List<GalleryGame>             Games     { get; set; } = [];
    public Dictionary<string, List<int>> Reached   { get; set; } = new(); // scenario -> hub ids reached in play
    public Dictionary<string, List<int>> Ticked    { get; set; } = new(); // scenario -> endings ticked off by hand
    public List<string>                  LastTable { get; set; } = [];    // the players of the last game, in seat order
    public List<string>                  Villages  { get; set; } = [];    // village names played, most recent first
}

public class GalleryPlayer
{
    public string   Name       { get; set; } = string.Empty;
    public DateTime LastPlayed { get; set; }
}

public class GalleryGame
{
    public string                  Id           { get; set; } = string.Empty;
    public ScenarioId              Scenario     { get; set; }
    public int                     Ending       { get; set; }
    public string                  Title        { get; set; } = string.Empty; // the ending title as shown (village / warden names filled in)
    public DateTime                Date         { get; set; }
    public string[]                Players      { get; set; } = [];
    public Dictionary<string, int> Scores       { get; set; } = new();
    public string                  Winner       { get; set; } = string.Empty;
    public bool                    FamilyWinner { get; set; }
}

// One row of a scenario's endings tree: a branch (hub) or an ending. SameAs: the other branch that leads to the same
// ending (shown twice, ticked and counted once)
public record GalleryNode(int Depth, string Label, int HubId = 0, int Ending = 0, string SameAs = "")
{
    public bool IsEnding => Ending > 0;
}

public static class GalleryTrees
{
    public static readonly ScenarioId[] Scenarios = [ScenarioId.CostOfDisease, ScenarioId.FearOfUnknown, ScenarioId.TimeOfWar];

    public static string Name(ScenarioId scenario) => scenario switch
    {
        ScenarioId.CostOfDisease => "The Cost of Disease",
        ScenarioId.FearOfUnknown => "Fear of the Unknown",
        ScenarioId.TimeOfWar     => "A Time of War",
        _                        => string.Empty
    };

    // Branch names are the hub titles; endings and the ending numbers set by the scenario code (Vars.Ending)
    public static GalleryNode[] Tree(ScenarioId scenario) => scenario switch
    {
        ScenarioId.CostOfDisease =>
        [
            new(0, "Yellow Fever", HubId: (int)CostOfDiseaseHubId.Fever),
            new(1, "Devastation", HubId: (int)CostOfDiseaseHubId.Devastation),
            new(2, "A Taste of Evil", HubId: (int)CostOfDiseaseHubId.GloomyGothic),
            new(3, "Their Own Destiny", Ending: 1),
            new(3, "Overshadowed and Exploited", Ending: 2),
            new(3, "Local Legends", Ending: 3),
            new(3, "The Spread of Evil", Ending: 4, SameAs: "The Good Fight / Moving Forward"),
            new(2, "The Good Fight / Moving Forward", HubId: (int)CostOfDiseaseHubId.Prosperity),
            new(3, "To Be Rid of Evil", Ending: 5),
            new(3, "There Could Never Be Peace", Ending: 6),
            new(3, "The Spread of Evil", Ending: 4, SameAs: "A Taste of Evil"),
            new(1, "Thriving in Adversity", HubId: (int)CostOfDiseaseHubId.Hospital),
            new(2, "Search for the Cure", HubId: (int)CostOfDiseaseHubId.University),
            new(3, "Editor's note - August 1923", Ending: 8),
            new(2, "The Crawl", HubId: (int)CostOfDiseaseHubId.NoUniversity),
            new(3, "A Legacy Will Live On", Ending: 7)
        ],
        ScenarioId.FearOfUnknown =>
        [
            new(0, "Witch Hunt", HubId: (int)FearOfTheUnknownHubId.WitchHunt),
            new(1, "Blame it on the Foreigners", HubId: (int)FearOfTheUnknownHubId.Foreigners),
            new(2, "Walled City", HubId: (int)FearOfTheUnknownHubId.Isolation),
            new(3, "Faded Into Obscurity", Ending: 1),
            new(3, "A Model City", Ending: 2),
            new(2, "Tensions Increase", HubId: (int)FearOfTheUnknownHubId.Tension),
            new(3, "Springtime for [your village]", Ending: 3),
            new(3, "The Anger of the Mob", Ending: 4),
            new(1, "The Creature Re-emerges", HubId: (int)FearOfTheUnknownHubId.Creature),
            new(2, "Private Investors", HubId: (int)FearOfTheUnknownHubId.Privatized),
            new(3, "Peace in Defiance", Ending: 5),
            new(3, "[The warden] Forever", Ending: 6),
            new(2, "Acceptance", HubId: (int)FearOfTheUnknownHubId.Liberal),
            new(3, "Committed", Ending: 7),
            new(3, "Taking Charge", Ending: 8)
        ],
        ScenarioId.TimeOfWar =>
        [
            new(0, "Taking Sides", HubId: (int)ATimeOfWarHubId.TakingSides),
            new(1, "Martial Law", HubId: (int)ATimeOfWarHubId.Martial),
            new(2, "Monarchy Reigns", HubId: (int)ATimeOfWarHubId.MonarchReign),
            new(3, "Royal Praise", Ending: 5),
            new(3, "Tipping the Scales", Ending: 6),
            new(2, "World Conquest", HubId: (int)ATimeOfWarHubId.Peace),
            new(3, "History of Applied Sciences", Ending: 7),
            new(3, "Local Legend", Ending: 8),
            new(1, "Time Travel", HubId: (int)ATimeOfWarHubId.TimeTravel),
            new(2, "Peaceful Return", HubId: (int)ATimeOfWarHubId.Warning),
            new(3, "A Wrinkle Within Time", Ending: 3),
            new(3, "A Timeline Unbound", Ending: 4),
            new(2, "Forward to Progress", HubId: (int)ATimeOfWarHubId.Paradox),
            new(3, "A Giant Success", Ending: 1),
            new(3, "Behind the Curve", Ending: 2)
        ],
        _ => []
    };

    public static int HubId(GlobalData globalData) => globalData.ScenarioId switch
    {
        ScenarioId.CostOfDisease => (int)globalData.TheCostOfDiseaseVars.HubId,
        ScenarioId.FearOfUnknown => (int)globalData.FearOfTheUnknownVars.HubId,
        ScenarioId.TimeOfWar     => (int)globalData.ATimeOfWarVars.HubId,
        _                        => 0
    };

    public static int Ending(GlobalData globalData) => globalData.ScenarioId switch
    {
        ScenarioId.CostOfDisease => (int)globalData.TheCostOfDiseaseVars.Ending,
        ScenarioId.FearOfUnknown => globalData.FearOfTheUnknownVars.Ending,
        ScenarioId.TimeOfWar     => globalData.ATimeOfWarVars.Ending,
        _                        => 0
    };
}

// Loads the gallery once per page session and writes it back after every change. Called from the UI only (Gameplay,
// the menu pages and the gallery page), never from scenario code (mirror sandbox, undo, load).
public class GalleryService(IJSRuntime js)
{
    private const string _STORAGE_KEY = "gallery";

    private GalleryData? _Data;

    public async Task<GalleryData> GetAsync()
    {
        if (_Data != null) return _Data;

        try
        {
            string? json = await js.InvokeAsync<string?>("localStorage.getItem", _STORAGE_KEY);
            _Data = string.IsNullOrEmpty(json) ? new GalleryData() : JsonConvert.DeserializeObject<GalleryData>(json) ?? new GalleryData();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Gallery: could not read saved data ({e.Message})");
            _Data = new GalleryData();
        }

        return _Data;
    }

    private async Task SaveAsync()
    {
        if (_Data == null) return;

        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", _STORAGE_KEY, JsonConvert.SerializeObject(_Data));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Gallery: could not save ({e.Message})");
        }
    }

    // Remembered names, most recently played first
    public async Task<List<string>> PlayerNamesAsync()
    {
        GalleryData data = await GetAsync();
        return data.Players.OrderByDescending(player => player.LastPlayed).Select(player => player.Name).ToList();
    }

    // A hub was shown: remember the table and the branch. Writes only when something is new.
    public async Task NoteHubAsync(GlobalData globalData)
    {
        int hubId = GalleryTrees.HubId(globalData);
        if (globalData.ScenarioId == ScenarioId.None || hubId == 0) return;

        GalleryData data    = await GetAsync();
        bool        changed = NotePlayers(data, globalData.GetActivePlayers()) | NoteVillage(data, globalData.TownName);

        List<int> reached = Entry(data.Reached, globalData.ScenarioId);
        if (!reached.Contains(hubId))
        {
            reached.Add(hubId);
            changed = true;
        }

        if (changed) await SaveAsync();
    }

    // The final ending is shown: record (or replace, after undo and new scores) this playthrough. Returns the record and
    // whether its ending was new to the gallery.
    public async Task<(GalleryGame game, bool newEnding)> RecordGameAsync(GlobalData globalData, string shownTitle)
    {
        GalleryData  data   = await GetAsync();
        ScenarioVars vars   = globalData.ScenarioVars;
        string       id     = $"{globalData.ScenarioId}:{vars.FlavorSeed}";
        int          ending = GalleryTrees.Ending(globalData);

        GalleryGame? existing = data.Games.FirstOrDefault(game => game.Id == id);
        bool newEnding = !data.Games.Any(game => game.Id != id && game.Scenario == globalData.ScenarioId && game.Ending == ending)
                         && !Entry(data.Ticked, globalData.ScenarioId).Contains(ending);

        string[] players = globalData.GetActivePlayers();
        GalleryGame record = new GalleryGame
        {
            Id           = id,
            Scenario     = globalData.ScenarioId,
            Ending       = ending,
            Title        = Regex.Replace(shownTitle, "<[^>]+>", string.Empty).Trim(),
            Date         = existing?.Date ?? DateTime.Now,
            Players      = players,
            Scores       = players.ToDictionary(player => player, player => vars.Scores.GetValueOrDefault(player)),
            Winner       = vars.FamilyWinner ? string.Empty : vars.Winner,
            FamilyWinner = vars.FamilyWinner
        };

        bool changed = existing == null || JsonConvert.SerializeObject(existing) != JsonConvert.SerializeObject(record);
        if (existing != null) data.Games.Remove(existing);
        data.Games.Add(record);

        changed |= NotePlayers(data, players);
        if (changed) await SaveAsync();
        return (record, newEnding);
    }

    // Endings reached in play or ticked off by hand
    public static HashSet<int> EndingsDone(GalleryData data, ScenarioId scenario)
    {
        HashSet<int> done = data.Games.Where(game => game.Scenario == scenario).Select(game => game.Ending).ToHashSet();
        if (data.Ticked.TryGetValue(scenario.ToString(), out List<int>? ticked)) done.UnionWith(ticked);
        done.Remove(0);
        return done;
    }

    public async Task SetTickedAsync(ScenarioId scenario, int ending, bool ticked)
    {
        GalleryData data = await GetAsync();
        List<int>   list = Entry(data.Ticked, scenario);
        list.Remove(ending);
        if (ticked) list.Add(ending);
        await SaveAsync();
    }

    // Remembered village names, most recently played first
    public async Task<List<string>> VillagesAsync() => (await GetAsync()).Villages.ToList();

    public async Task NoteVillageAsync(string village)
    {
        if (NoteVillage(await GetAsync(), village)) await SaveAsync();
    }

    public async Task ForgetPlayerAsync(string name)
    {
        GalleryData data = await GetAsync();
        data.Players.RemoveAll(player => player.Name == name);
        data.LastTable.Clear();
        await SaveAsync();
    }

    public async Task<string> ExportAsync()
    {
        GalleryData data = await GetAsync();
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
    }

    // Merges an export into this device's gallery: players, games (by id), branches and ticks are combined
    public async Task<bool> ImportAsync(string base64)
    {
        GalleryData? incoming;

        try
        {
            string  json   = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64.Trim()));
            JObject parsed = JObject.Parse(json);
            if (!parsed.ContainsKey(nameof(GalleryData.Games))) return false; // e.g. a saved game
            incoming = parsed.ToObject<GalleryData>();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Gallery: import failed ({e.Message})");
            return false;
        }

        if (incoming == null) return false;

        GalleryData data = await GetAsync();

        foreach (GalleryPlayer player in incoming.Players)
        {
            GalleryPlayer? mine = data.Players.FirstOrDefault(p => string.Equals(p.Name, player.Name, StringComparison.CurrentCultureIgnoreCase));
            if (mine == null) data.Players.Add(player);
            else if (player.LastPlayed > mine.LastPlayed) mine.LastPlayed = player.LastPlayed;
        }

        foreach (GalleryGame game in incoming.Games.Where(game => data.Games.All(mine => mine.Id != game.Id))) data.Games.Add(game);

        foreach ((string scenario, List<int> hubs) in incoming.Reached)
        {
            List<int> mine = Entry(data.Reached, scenario);
            mine.AddRange(hubs.Where(hub => !mine.Contains(hub)));
        }

        foreach ((string scenario, List<int> endings) in incoming.Ticked)
        {
            List<int> mine = Entry(data.Ticked, scenario);
            mine.AddRange(endings.Where(ending => !mine.Contains(ending)));
        }

        if (data.LastTable.Count == 0) data.LastTable = incoming.LastTable;
        data.Villages.AddRange(incoming.Villages.Where(village => !data.Villages.Contains(village, StringComparer.CurrentCultureIgnoreCase)));

        await SaveAsync();
        return true;
    }

    public static List<int> Entry(Dictionary<string, List<int>> map, ScenarioId scenario) => Entry(map, scenario.ToString());

    private static List<int> Entry(Dictionary<string, List<int>> map, string key)
    {
        if (map.TryGetValue(key, out List<int>? list)) return list;
        list     = [];
        map[key] = list;
        return list;
    }

    private const int _MAX_VILLAGES = 8;

    private static bool NoteVillage(GalleryData data, string village)
    {
        village = village.Trim();
        if (village.Length == 0 || data.Villages.FirstOrDefault() == village) return false;

        data.Villages.RemoveAll(v => string.Equals(v, village, StringComparison.CurrentCultureIgnoreCase));
        data.Villages.Insert(0, village);
        if (data.Villages.Count > _MAX_VILLAGES) data.Villages.RemoveRange(_MAX_VILLAGES, data.Villages.Count - _MAX_VILLAGES);
        return true;
    }

    private static bool NotePlayers(GalleryData data, string[] players)
    {
        bool changed = false;

        foreach (string name in players)
        {
            GalleryPlayer? player = data.Players.FirstOrDefault(p => p.Name == name);
            if (player == null) data.Players.Add(player = new GalleryPlayer { Name = name });
            if (player.LastPlayed.Date == DateTime.Today) continue;
            player.LastPlayed = DateTime.Now;
            changed           = true;
        }

        if (!data.LastTable.SequenceEqual(players))
        {
            data.LastTable = players.ToList();
            changed        = true;
        }

        return changed;
    }
}
