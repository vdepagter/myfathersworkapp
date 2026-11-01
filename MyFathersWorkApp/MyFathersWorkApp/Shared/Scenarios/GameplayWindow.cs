using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace MyFathersWorkApp;

public class GameplayWindow(GlobalData globalData)
{
    public string                Title    { get; private set; } = string.Empty;
    public string                Subtitle { get; private set; } = string.Empty;
    public List<GameplayElement> Elements { get; }              = new();

    // Audio hints read by Pages/Gameplay.razor when this window is shown. Scenario code only marks the window, it never
    // plays audio itself (it also runs in mirror simulations, undo and load). Not saved: the window is rebuilt on load/undo.
    public SoundCue Cue           { get; private set; }
    public bool     IsFinalEnding { get; private set; }

    // Voice-over clip of a narrated window: the Unity passage name, file wwwroot/audio/vo/<voice>/<VoiceOver>.mp3
    // (docs/AUDIO_TRANSITIONS_MAP.md "Voice-over"). Null: no original recording.
    public string? VoiceOver { get; private set; }

    // Story text of this window for the Oliver narration (Shared/Narration.cs): every tag of the scenario story CSV
    // added with AddDefaultTitle / AddDefaultBaseTitle (narrated only where the scenario's clip list says so) and
    // AddDefaultContent / AddNextContent*, with the text as formatted (before the links are split off).
    public List<(string Tag, string Text)> StoryTexts { get; } = new();

    public void SetCue(SoundCue cue) => Cue = cue;

    public void SetVoiceOver(string clip) => VoiceOver = clip;

    private void AddStoryText(string tag, string text)
    {
        if (globalData.Scenario_Localization.ContainsKey(tag)) StoryTexts.Add((tag, text));
    }

    public void MarkFinalEnding() => IsFinalEnding = true;

    // The final ranking: Pages/Gameplay.razor shows it as the podium (Shared/RankingsView.razor) instead of the text
    public RankingsData? Rankings { get; private set; }

    public void SetRankings(RankingsData rankings) => Rankings = rankings;

    public void AddDefaultTitle(Func<string, string>? contentProcessor = null, [CallerMemberName] string callerName = "")
    {
        string title                        = globalData.GetScenarioLocalizedTag(callerName + "_Title");
        if (contentProcessor != null) title = contentProcessor(title);
        Title = title;
        AddStoryText(callerName + "_Title", title);
    }

    public void AddDefaultBaseTitle(Func<string, string>? contentProcessor = null, [CallerMemberName] string callerName = "")
    {
        string title                        = globalData.GetScenarioLocalizedTag(callerName.Split('_')[0] + "_Title");
        if (contentProcessor != null) title = contentProcessor(title);
        Title = title;
        AddStoryText(callerName.Split('_')[0] + "_Title", title);
    }

    public void AddGameplayTitle(string titleTag)
    {
        Title = globalData.GetLocalizedUITag(titleTag);
    }

    public void AddDefaultSubtitle([CallerMemberName] string callerName = "")
    {
        Subtitle = globalData.GetScenarioLocalizedTag(callerName + "_SubTitle");
    }

    public void AddDefaultContent(Func<string, string>? contentProcessor = null, [CallerMemberName] string callerName = "")
    {
        string content                        = globalData.GetScenarioLocalizedTag(callerName + "_Content");
        if (contentProcessor != null) content = contentProcessor(content);
        AddStoryText(callerName + "_Content", content);
        Elements.Add(new GameplayElement(content));
    }

    public void AddNextContent(int index, bool fromNewLine = false, Func<string, string>? contentFormatter = null, Action<GlobalData>? nextCallback = null, [CallerMemberName] string callerName = "")
    {
        string content                        = globalData.GetScenarioLocalizedTag(callerName + $"_Content{index}");
        if (contentFormatter != null) content = contentFormatter(content);
        AddStoryText(callerName + $"_Content{index}", content);
        Elements.Add(new GameplayElement(content, nextCallback, fromNewLine));
    }

    public void AddNextContentWithLinks(int index, List<Action<GlobalData>?> nextCallbacks, bool fromNewLine = false, Func<string, string>? contentFormatter = null, [CallerMemberName] string callerName = "")
    {
        string content                        = globalData.GetScenarioLocalizedTag(callerName + $"_Content{index}");
        if (contentFormatter != null) content = contentFormatter(content);
        AddStoryText(callerName + $"_Content{index}", content);

        // Regex pattern to find <link=X>Text</link>
        var linkPattern = new Regex(@"<link=(\d+)>(.*?)<\/link>", RegexOptions.Singleline);
        var matches     = linkPattern.Matches(content);

        List<string>            outputStrings = new List<string>();
        Dictionary<string, int> linkIndexMap  = new Dictionary<string, int>();

        int lastIndex = 0;

        foreach (Match match in matches)
        {
            int    linkIndex = int.Parse(match.Groups[1].Value);
            string linkText  = match.Groups[2].Value;

            if (match.Index > lastIndex)
            {
                string before = content.Substring(lastIndex, match.Index - lastIndex);
                outputStrings.Add(before);
            }

            outputStrings.Add(linkText);
            linkIndexMap[linkText] = linkIndex;

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < content.Length)
        {
            string remaining = content.Substring(lastIndex);
            outputStrings.Add(remaining);
        }

        for (int x = 0; x < outputStrings.Count; ++x)
        {
            Action<GlobalData>? callback = linkIndexMap.TryGetValue(outputStrings[x], out int callbackIndex) ? nextCallbacks[callbackIndex] : null;
            Elements.Add(new GameplayElement(outputStrings[x], callback, x == 0 && fromNewLine));
        }
    }

    // Story text of another method's tag (shared screens like "Secret_Content"): shown and narrated like AddDefaultContent
    public void AddStoryContent(string tag)
    {
        string content = globalData.GetScenarioLocalizedTag(tag);
        AddStoryText(tag, content);
        Elements.Add(new GameplayElement(content));
    }

    public void AddCustomContent(string content, bool fromNewLine = false)
    {
        Elements.Add(new GameplayElement(content, null, fromNewLine));
    }

    public void AddCustomLink(string text, Action<GlobalData> nextCallback, bool fromNewLine = true)
    {
        Elements.Add(new GameplayElement(text, nextCallback, fromNewLine));
    }

    public void AddClickHere(Action<GlobalData> nextCallback, bool fromNewLine = false)
    {
        Elements.Add(new GameplayElement(globalData.GetLocalizedUITag(GlobalTags.Gameplay_ClickHere), nextCallback, fromNewLine));
    }

    public void AddClickHereToContinue(Action<GlobalData> nextCallback)
    {
        Elements.Add(new GameplayElement(globalData.GetLocalizedUITag(GlobalTags.Gameplay_ClickHereToContinue), nextCallback, true));
    }

    public void AddClickHereAfterBid(Action<GlobalData> nextCallback)
    {
        Elements.Add(new GameplayElement(globalData.GetLocalizedUITag(GlobalTags.Gameplay_ClickHereAfterBid), nextCallback, true));
    }

    public void AddClickHereAfterVote(Action<GlobalData> nextCallback)
    {
        Elements.Add(new GameplayElement(globalData.GetLocalizedUITag(GlobalTags.Gameplay_ClickHereAfterVote), nextCallback, true));
    }

    public void AddChoose()
    {
        Elements.Add(new GameplayElement(globalData.GetLocalizedUITag(GlobalTags.Gameplay_Choose), null, true));
    }

    public void AddYesNo(Action<GlobalData> yesCallback, Action<GlobalData> noCallback)
    {
        Elements.Add(new GameplayElement(globalData.GetLocalizedUITag(GlobalTags.Gameplay_Yes), yesCallback, true));
        Elements.Add(new GameplayElement(globalData.GetLocalizedUITag(GlobalTags.Gameplay_No),  noCallback,  true));
    }

    public void AddHandStorybookTo(string who, Action<GlobalData> nextCallback)
    {
        Elements.Add(new GameplayElement(globalData.GetLocalizedUITag(GlobalTags.Gameplay_HandStorybookTo).FormatWithReplacement(0, who)));
        AddClickHereToContinue(nextCallback);
    }
    
    public void AddHandStorybookToNoJump(string who)
    {
        Elements.Add(new GameplayElement(globalData.GetLocalizedUITag(GlobalTags.Gameplay_HandStorybookTo).FormatWithReplacement(0, who)));
    }
    
    public void AddOnceYouAreReady(Action<GlobalData> nextCallback)
    {
        Elements.Add(new GameplayElement(globalData.GetLocalizedUITag(GlobalTags.Gameplay_OnceYouAreReady), nextCallback, true));
    }

    public void AddDoNotOtherPlayersToSee(string who, Action<GlobalData> nextCallback)
    {
        Elements.Add(new GameplayElement(globalData.GetLocalizedUITag(GlobalTags.Gameplay_DoNotOtherPlayersSee).FormatWithReplacement(0, who)));
        Elements.Add(new GameplayElement(globalData.GetLocalizedUITag(GlobalTags.Gameplay_OnceYouAreReady), nextCallback, true));
    }

    public void AddAllPlayersNamesAsOptions(Action<string> nextCallback, PlayerFormatterTag playerFormatterTag = PlayerFormatterTag.None, Func<string, bool>? showPlayer = null)
    {
        showPlayer ??= _ => true;

        string format = playerFormatterTag switch
        {
            PlayerFormatterTag.DrJr  => globalData.GetLocalizedUITag(GlobalTags.Gameplay_DrJr),
            PlayerFormatterTag.Dr    => globalData.GetLocalizedUITag(GlobalTags.Gameplay_Dr),
            PlayerFormatterTag.Third => globalData.GetLocalizedUITag(GlobalTags.Gameplay_Third),
            _                        => "{{0=...|Oliver|James|Olivia|...}}"
        };

        if (showPlayer(globalData.PlayerAName)) Elements.Add(new GameplayElement(format.FormatWithReplacement(0,                               globalData.PlayerAName), data => nextCallback(data.PlayerAName), true));
        if (showPlayer(globalData.PlayerBName)) Elements.Add(new GameplayElement(format.FormatWithReplacement(0,                               globalData.PlayerBName), data => nextCallback(data.PlayerBName), true));
        if (globalData.PlayersNum >= 3 && showPlayer(globalData.PlayerCName)) Elements.Add(new GameplayElement(format.FormatWithReplacement(0, globalData.PlayerCName), data => nextCallback(data.PlayerCName), true));
        if (globalData.PlayersNum >= 4 && showPlayer(globalData.PlayerDName)) Elements.Add(new GameplayElement(format.FormatWithReplacement(0, globalData.PlayerDName), data => nextCallback(data.PlayerDName), true));
    }
}