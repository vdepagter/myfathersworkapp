using System.Text.RegularExpressions;

namespace MyFathersWorkApp;

// Guided mode (Settings / pause menu, Shared/GameOptions.cs): hints at the choices that steer the story to a branch or an
// ending, for players chasing every ending. Text lives in wwwroot/localization/<Scenario>_Guide.csv (English); the rules
// that pick and fill it are in Shared/Guide/<Scenario>Guide.cs, keyed by the screen on display (GlobalData.ResumeMethod)
// or the round screen (hub). Shown by Shared/GuideBox.razor, never by scenario code: guide code only reads state and must
// never draw from RandomArray (that would change the game and ask questions in mirror mode). Hints explain odds and
// rules, never a pre-rolled outcome, and their "Now:" lines show only what the table already knows (no secret choices).
// Not narrated: guide text is no story tag, and js/audio.js skips .guide-note when it highlights.
public sealed class GuideService(HttpClient http)
{
    private readonly Dictionary<ScenarioId, Dictionary<string, string>> _Texts = new();

    public async Task LoadAsync(ScenarioId scenario)
    {
        if (_Texts.ContainsKey(scenario)) return;

        string file = scenario switch
        {
            ScenarioId.CostOfDisease => "localization/TheCostOfDisease_Guide.csv",
            ScenarioId.FearOfUnknown => "localization/FearOfTheUnknown_Guide.csv",
            ScenarioId.TimeOfWar     => "localization/ATimeOfWar_Guide.csv",
            _                        => string.Empty
        };

        Dictionary<string, string> texts = new();
        _Texts[scenario] = texts;
        if (file == string.Empty) return;

        try
        {
            Dictionary<string, Dictionary<string, string>> languages = new();
            (await http.GetStringAsync(file)).ProcessLocalizationFile(languages);
            if (languages.TryGetValue("English", out Dictionary<string, string>? english)) _Texts[scenario] = english;
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Guide: no hints for {scenario} ({exception.Message})");
        }
    }

    // The hint for what is on display, or null. key: the screen method of a window or popup, or null for the hub.
    public GuideNote? Note(GlobalData globalData, string? key, GuideFound found)
    {
        if (!_Texts.TryGetValue(globalData.ScenarioId, out Dictionary<string, string>? texts) || texts.Count == 0) return null;

        GuideContext context = new(globalData, texts, found);

        try
        {
            return globalData.ScenarioId switch
            {
                ScenarioId.CostOfDisease => key == null ? CostOfDiseaseGuide.Hub(context) : CostOfDiseaseGuide.Screen(context, key),
                ScenarioId.FearOfUnknown => key == null ? FearOfTheUnknownGuide.Hub(context) : FearOfTheUnknownGuide.Screen(context, key),
                ScenarioId.TimeOfWar     => key == null ? ATimeOfWarGuide.Hub(context) : ATimeOfWarGuide.Screen(context, key),
                _                        => null
            };
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Guide: {key ?? "hub"} failed: {exception.Message}");
            return null;
        }
    }
}

// Branches (hub ids) and endings the Endings Gallery has recorded for this scenario (✓ found, ☆ not yet)
public sealed record GuideFound(HashSet<int> Hubs, HashSet<int> Endings)
{
    public static readonly GuideFound None = new([], []);
}

public sealed record GuideNote(string Title, string Body, string? Now);

// What a scenario guide uses to build a note: the game state, the guide texts and the gallery
public sealed partial class GuideContext(GlobalData globalData, Dictionary<string, string> texts, GuideFound found)
{
    public GlobalData GlobalData => globalData;

    public bool Has(string tag) => texts.ContainsKey(tag);

    // Guide text with [[hub:Name]] and [[end:n]] replaced by the gallery names and their found marks
    public string Text(string tag, Func<string, string>? format = null)
    {
        if (!texts.TryGetValue(tag, out string? text)) return $"Missing guide text -> {tag}";
        if (format != null) text = format(text);
        return _Token().Replace(text, match => Branch(match.Groups[1].Value, match.Groups[2].Value));
    }

    // A note from Guide_<key> (body), Guide_<key>_Title (heading, else the default) and an optional "Now:" line
    public GuideNote Note(string key, Func<string, string>? format = null, string? now = null, string? title = null) =>
        new(title ?? (Has($"Guide_{key}_Title") ? Text($"Guide_{key}_Title") : Text("Guide_Title")), Text($"Guide_{key}", format), now);

    // The "Now:" line from Guide_<key>_Now, or null when there is nothing to say
    public string? Now(string key, Func<string, string>? format = null) => Has($"Guide_{key}_Now") ? Text($"Guide_{key}_Now", format) : null;

    private string Branch(string kind, string id)
    {
        ScenarioId     scenario = globalData.ScenarioId;
        GalleryNode[]  tree     = GalleryTrees.Tree(scenario);
        GalleryNode?   node;
        bool           isFound;

        if (kind == "end" && int.TryParse(id, out int ending))
        {
            node    = tree.FirstOrDefault(n => n.IsEnding && n.Ending == ending);
            isFound = found.Endings.Contains(ending);
        }
        else
        {
            int hubId = HubId(scenario, id);
            node    = tree.FirstOrDefault(n => !n.IsEnding && n.HubId == hubId && hubId != 0);
            isFound = found.Hubs.Contains(hubId);
        }

        if (node == null) return id;

        string label = node.Label.Replace("[your village]", globalData.TownName);
        return isFound
            ? $"<span class=\"guide-branch found\" title=\"{globalData.GetLocalizedUITag(GlobalTags.Guide_Found)}\">✓ {label}</span>"
            : $"<span class=\"guide-branch new\" title=\"{globalData.GetLocalizedUITag(GlobalTags.Guide_NotFound)}\">☆ {label}</span>";
    }

    private static int HubId(ScenarioId scenario, string name)
    {
        Type? type = scenario switch
        {
            ScenarioId.CostOfDisease => typeof(CostOfDiseaseHubId),
            ScenarioId.FearOfUnknown => typeof(FearOfTheUnknownHubId),
            ScenarioId.TimeOfWar     => typeof(ATimeOfWarHubId),
            _                        => null
        };

        return type != null && Enum.TryParse(type, name, out object? value) ? Convert.ToInt32(value) : 0;
    }

    [GeneratedRegex(@"\[\[(hub|end):(\w+)\]\]")]
    private static partial Regex _Token();
}
