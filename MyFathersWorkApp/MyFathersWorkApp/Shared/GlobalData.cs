using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace MyFathersWorkApp;

public partial class GlobalData
{
    private class UndoData
    {
        public Action<GlobalData>? Callback   { get; set; }
        public string              Method     { get; set; } = string.Empty; // name of Callback, kept in the browser (GlobalData.Persistence.cs)
        public string              JsonData   { get; set; } = string.Empty;
        public string              RandomJson { get; set; } = string.Empty; // RandomArray as JSON, stored only when it changes
        public int                 Round      { get; set; } = -1; // hub saves only: (Generation, Years) of the shown hub
        public long                Id         { get; set; }
    }

    private int UndoRound => (int)Generation * 3 + (int)Years;

    [JsonIgnore] public Dictionary<string, Dictionary<string, string>> UI_Localization                { get; }      = new();
    [JsonIgnore] public Dictionary<string, string>                     Scenario_Localization          { get; set; } = new();
    [JsonIgnore] public Dictionary<string, string>                     Scenario_Gameplay_Localization { get; set; } = new();
    [JsonIgnore] public List<string>                                   OtherPossibleScenarioLanguages { get; set; } = new();

    // English story and rules text of the active scenario, also while it is shown in a translation: links are recognised
    // by their English text (Pages/Gameplay.razor LinkButtonTag)
    [JsonIgnore] public Dictionary<string, string> Scenario_Localization_English          { get; set; } = new();
    [JsonIgnore] public Dictionary<string, string> Scenario_Gameplay_Localization_English { get; set; } = new();

    // Menu language (a device setting, localStorage mfwLanguage: a loaded save or undo keeps the current choice) and the
    // language of the story, which follows it when the scenario is translated (AppLanguages.cs)
    public string Language         { get; set; } = AppLanguages.ENGLISH;
    public string ScenarioLanguage { get; set; } = AppLanguages.ENGLISH_STORY;

    // Grammatical gender per player for translations that need it (Czech): "m" or "f" by player index A-D, set at name
    // entry. Empty (older saves, English): male forms.
    public string PlayerGenders { get; set; } = string.Empty;

    public ScenarioId                 ScenarioId           { get; set; }
    public int                        PlayersNum           { get; set; }
    public string                     PlayerAName          { get; set; } = string.Empty;
    public string                     PlayerBName          { get; set; } = string.Empty;
    public string                     PlayerCName          { get; set; } = string.Empty;
    public string                     PlayerDName          { get; set; } = string.Empty;
    public string                     TownName             { get; set; } = string.Empty;
    public Years                      Years                { get; set; } = Years.Early;
    public Generation                 Generation           { get; set; } = Generation.First;
    public TheCostOfDiseaseVars       TheCostOfDiseaseVars { get; }      = new();
    public FearOfTheUnknownVars       FearOfTheUnknownVars { get; }      = new();
    public ATimeOfWarVars             ATimeOfWarVars       { get; }      = new();
    public Dictionary<string, object> TmpValues            { get; }      = new();


    // Screen this state is shown on (the method that called SaveToUndo), empty on a hub. Load reopens it.
    public string ResumeMethod { get; set; } = string.Empty;

    // The active scenario's random values and final scoring state
    [JsonIgnore] public ScenarioVars ScenarioVars => ScenarioId switch
    {
        ScenarioId.FearOfUnknown => FearOfTheUnknownVars,
        ScenarioId.TimeOfWar     => ATimeOfWarVars,
        _                        => TheCostOfDiseaseVars
    };

    // Only the active scenario's state is saved (keeps saves and undo snapshots small)
    public bool ShouldSerializeTheCostOfDiseaseVars() => ScenarioId is not (ScenarioId.FearOfUnknown or ScenarioId.TimeOfWar);
    public bool ShouldSerializeFearOfTheUnknownVars() => ScenarioId == ScenarioId.FearOfUnknown;
    public bool ShouldSerializeATimeOfWarVars()       => ScenarioId == ScenarioId.TimeOfWar;

    [JsonIgnore] public GameplayHub?        ActiveHub        { get; set; }
    [JsonIgnore] public GameplayWindow?     ActiveWindow     { get; set; }
    [JsonIgnore] public GameplayPopup?      ActivePopup      { get; set; }
    [JsonIgnore] public GameplayInputPopup? ActiveInputPopup { get; set; }

    [JsonIgnore] private Stack<UndoData> UndoStack { get; } = new();

    // Undo points taken back by Undo, newest on top. Redo puts them back; any new undo point clears them.
    [JsonIgnore] private Stack<UndoData> RedoStack { get; } = new();

    [JsonIgnore] public bool IsUndoDisabled => ActiveMirrorQuestion == null && UndoStack.Count <= 1;
    [JsonIgnore] public bool IsRedoDisabled => ActiveMirrorQuestion != null || RedoStack.Count == 0;

    [JsonIgnore] private bool _UndoInProgress;
    [JsonIgnore] private long _NextUndoId;

    public GlobalData()
    {
        ResetScenarioVars();
    }

    public void ResetScenarioVars()
    {
        TheCostOfDiseaseVars.Reset(this);
        FearOfTheUnknownVars.Reset(this);
        ATimeOfWarVars.Reset(this);
    }

    // Static class holding the active scenario's screens (SaveToUndo looks methods up by name)
    private Type? ScenarioType => ScenarioId switch
    {
        ScenarioId.CostOfDisease => typeof(TheCostOfDisease),
        ScenarioId.FearOfUnknown => typeof(FearOfTheUnknown),
        ScenarioId.TimeOfWar     => typeof(ATimeOfWar),
        _                        => null
    };

    public string GetLocalizedUITag(string tag) => TryGetLocalizedUITag(tag, out string? text) ? text : $"Missing Translation -> {Language}/{tag}";

    // A tag missing from the menu language (or a language that no longer exists, like the removed Polish) falls back to English
    public bool TryGetLocalizedUITag(string tag, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? text)
    {
        text = null;
        return (UI_Localization.TryGetValue(Language, out Dictionary<string, string>? translated) && translated.TryGetValue(tag, out text))
            || (UI_Localization.TryGetValue(AppLanguages.ENGLISH, out Dictionary<string, string>? english) && english.TryGetValue(tag, out text));
    }

    [JsonIgnore] public bool IsStoryInEnglish => ScenarioLanguage == AppLanguages.ENGLISH_STORY;

    // The menu language asks for each player's gender (Pages/PlayersNameEntry.razor)
    [JsonIgnore] public bool LanguageUsesGender => AppLanguages.Get(Language)?.Gender == true;

    public bool IsFemale(string name)
    {
        string[] players = GetActivePlayers();
        int index = Array.FindIndex(players, player => string.Equals(player, name, StringComparison.Ordinal));
        return index >= 0 && index < PlayerGenders.Length && PlayerGenders[index] == 'f';
    }

    public bool IsFemale(int index) => index < PlayerGenders.Length && PlayerGenders[index] == 'f';

    public void SetFemale(int index, bool female)
    {
        char[] genders = PlayerGenders.PadRight(4, 'm').ToCharArray();
        genders[index] = female ? 'f' : 'm';
        PlayerGenders  = new string(genders);
    }

    private const string _LANGUAGE_KEY = "mfwLanguage";

    // Loads the menu texts of English and every translation, then the language chosen on this device (or, the first
    // time, the browser's language when the app has it). Called once by the layout.
    public async Task LoadLanguagesAsync(HttpClient http, IJSRuntime js)
    {
        (await http.GetStringAsync("localization/UI_Localization.csv")).ProcessLocalizationFile(UI_Localization);

        foreach (AppLanguage translation in AppLanguages.Translations)
        {
            try
            {
                (await http.GetStringAsync(translation.File("UI_Localization"))).ProcessLocalizationFile(UI_Localization);
                UI_Localization.FillMissing(translation.Name, AppLanguages.ENGLISH);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{translation.Name} menus not loaded: {e.Message}");
                UI_Localization.Remove(translation.Name);
            }
        }

        try
        {
            string? stored = await js.InvokeAsync<string?>("localStorage.getItem", _LANGUAGE_KEY);
            string  chosen = stored ?? AppLanguages.FromBrowser(await js.InvokeAsync<string?>("eval", "navigator.language"));
            Language = UI_Localization.ContainsKey(chosen) ? chosen : AppLanguages.ENGLISH;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Language setting not read: {e.Message}");
        }

        GalleryTrees.Localize = label => TryGetLocalizedUITag(GalleryTrees.LabelTag(label), out string? text) ? text : label;
    }

    public async Task SetLanguageAsync(IJSRuntime js, string language)
    {
        Language = language;
        await js.InvokeVoidAsync("localStorage.setItem", _LANGUAGE_KEY, language);
    }

    // Dates in the menus, in the format of the menu language (UI tags Format_Date / Format_DateTime)
    public string FormatDate(DateTime date, bool time = false) =>
        date.ToString(GetLocalizedUITag(time ? GlobalTags.Format_DateTime : GlobalTags.Format_Date), System.Globalization.CultureInfo.InvariantCulture);

    // English text of a scenario tag, whatever language the story is shown in
    public string GetEnglishScenarioTag(string tag)
    {
        if (Scenario_Localization_English.TryGetValue(tag, out string? value)) return value;
        if (Scenario_Gameplay_Localization_English.TryGetValue(tag, out value)) return value;
        return string.Empty;
    }

    // Picks the story language for the loaded scenario texts (the menu language when the scenario is translated, else
    // English) and makes it active
    public void ApplyScenarioLanguage(Dictionary<string, Dictionary<string, string>> scenarioLocalizations,
                                      Dictionary<string, Dictionary<string, string>> scenarioGameplayLocalizations)
    {
        ScenarioLanguage = scenarioLocalizations.ContainsKey(Language) ? Language : AppLanguages.ENGLISH_STORY;
        if (!scenarioLocalizations.ContainsKey(ScenarioLanguage)) ScenarioLanguage = scenarioLocalizations.Keys.First();

        Scenario_Localization                  = scenarioLocalizations[ScenarioLanguage];
        Scenario_Gameplay_Localization         = scenarioGameplayLocalizations[Scenario_Localization["Gameplay_Lang"]];
        Scenario_Localization_English          = scenarioLocalizations.GetValueOrDefault(AppLanguages.ENGLISH_STORY) ?? Scenario_Localization;
        Scenario_Gameplay_Localization_English = scenarioGameplayLocalizations.GetValueOrDefault(AppLanguages.ENGLISH) ?? Scenario_Gameplay_Localization;
    }

    public string GetScenarioLocalizedTag(string tag)
    {
        if (tag == string.Empty) return string.Empty;
        if (Scenario_Localization.TryGetValue(tag, out string? value)) return value;
        if (Scenario_Gameplay_Localization.TryGetValue(tag, out value)) return value;
        return $"Missing Scenario Translation -> {tag}";
    }

    public T GetTmpValue<T>(string key)
    {
        object? value = TmpValues.GetValueOrDefault(key);

        // After undo or load the values come from JSON (an int comes back as a long)
        if (value is not T && value is IConvertible) return (T)Convert.ChangeType(value, typeof(T));
        return (T)value!;
    }

    public void SaveToUndo([CallerMemberName] string methodName = "")
    {
        if (_UndoInProgress) return;

        if (ScenarioType == null) return;

        ResumeMethod = methodName;
        UndoStack.Push(NewUndoData(ResolveScreen(methodName)!, methodName, JsonConvert.SerializeObject(this), -1));
        RedoStack.Clear();
    }

    // A screen method by name, see SaveToUndo
    private Action<GlobalData>? ResolveScreen(string methodName)
    {
        if (ScenarioType == null || methodName == string.Empty) return null;

        const BindingFlags flags  = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        MethodInfo?        method = ScenarioType.GetMethod(methodName, flags) ?? typeof(FinalScoring).GetMethod(methodName, flags);
        return method == null ? null : (Action<GlobalData>)Delegate.CreateDelegate(typeof(Action<GlobalData>), method);
    }

    private UndoData NewUndoData(Action<GlobalData>? callback, string method, string json, int round)
    {
        return new UndoData
        {
            Callback   = callback,
            Method     = callback == null ? string.Empty : method,
            JsonData   = json,
            RandomJson = JsonConvert.SerializeObject(ScenarioVars.RandomArray),
            Round      = round,
            Id         = ++_NextUndoId
        };
    }

    public int LocalizedPlayerNumberIndex()
    {
        return PlayersNum - 2;
    }

    public string GetPlayerNameByIndex(int index)
    {
        return index switch
        {
            0 => PlayerAName,
            1 => PlayerBName,
            2 => PlayerCName,
            3 => PlayerDName,
            _ => string.Empty
        };
    }

    public void ShowEndOfRoundPopUp(Action<GlobalData> nextCallback)
    {
        ActivePopup = new GameplayPopup(this, PopUpTitle.EndOfRound, PopUpIcon.MFWlogo, PopUpButton.Confirm, _ =>
            {
                Generation prevGeneration = Generation;

                if (Years == Years.Late)
                {
                    Years      =  Years.Early;
                    Generation += 1;
                }
                else
                {
                    Years += 1;
                }

                if (prevGeneration != Generation) ActiveHub = null;
                else ActiveHub!.SetSubtitle(Years);

                nextCallback(this);
            }, GlobalTags.Gameplay_EndOfRound_Content,
            content => content.FormatWithIndex(3, (int)Years + 1)
                              .FormatWithCondition(2, () => Years is Years.Early or Years.Middle)
                              .FormatWithIndex(1, (int)Generation)
                              .FormatWithIndex(0, (int)Years));
    }

    public string[] GetActivePlayers()
    {
        string[] result = new string[PlayersNum];
        result[0] = PlayerAName;
        result[1] = PlayerBName;
        if (PlayersNum >= 3) result[2] = PlayerCName;
        if (PlayersNum >= 4) result[3] = PlayerDName;
        return result;
    }

    // Player, village and typed-in names: wrapped in translate="no" so the browser's own translation leaves them alone
    public IEnumerable<string> ProtectedNames()
    {
        string[] setupNames = [PlayerAName, PlayerBName, PlayerCName, PlayerDName, TownName];
        return setupNames.Concat(ScenarioVars.TypedNames())
                         .Where(name => !string.IsNullOrWhiteSpace(name))
                         .Select(name => name.Trim())
                         .Distinct();
    }

    // Value for <html lang>: the story language on /Gameplay, the menu language elsewhere. Lets the browser offer
    // "Translate this page" from the right language.
    public string PageLanguageCode(bool gameplay)
    {
        string language = (gameplay ? ScenarioLanguage : Language).Replace("_Source", "");
        return _LanguageCodes.GetValueOrDefault(language, "en");
    }

    private static readonly Dictionary<string, string> _LanguageCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["English"] = "en", ["Dutch"] = "nl", ["German"] = "de", ["French"] = "fr", ["Spanish"] = "es",
        ["Italian"] = "it", ["Portuguese"] = "pt", ["Czech"] = "cs", ["Russian"] = "ru", ["Ukrainian"] = "uk",
        ["Chinese"] = "zh", ["Japanese"] = "ja", ["Korean"] = "ko"
    };

    public async Task<bool> IsContinueAvailableAsync(IJSRuntime js)
    {
        string? base64 = await js.InvokeAsync<string?>("localStorage.getItem", "continue");
        return !string.IsNullOrEmpty(base64);
    }

    public void Undo()
    {
        if (ActiveMirrorQuestion != null)
        {
            // Back from a mirror question returns to the screen it was asked from
            ActiveMirrorQuestion = null;
            if (ActiveHub != null || ActiveWindow != null || ActivePopup != null || ActiveInputPopup != null) return;
        }

        if (UndoStack.Count <= 1) return;
        RedoStack.Push(UndoStack.Pop());
        ReopenUndoPoint(UndoStack.Peek());
    }

    // Steps forward again to the undo point the last Undo took back
    public void Redo()
    {
        if (ActiveMirrorQuestion != null || RedoStack.Count == 0) return;
        UndoData next = RedoStack.Pop();
        UndoStack.Push(next);
        ReopenUndoPoint(next);
    }

    // Restores an undo point's state and shows its screen again (or its hub)
    private void ReopenUndoPoint(UndoData point)
    {
        _UndoInProgress  = true;
        ActiveHub        = null;
        ActiveWindow     = null;
        ActivePopup      = null;
        ActiveInputPopup = null;

        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Reuse,
            NullValueHandling      = NullValueHandling.Ignore
        };
        // The language picked in the pause menu stays, the snapshot may be older than that choice
        string language         = Language;
        string scenarioLanguage = ScenarioLanguage;
        JsonConvert.PopulateObject(point.JsonData, this, settings);
        Language         = language;
        ScenarioLanguage = scenarioLanguage;
        _UndoInProgress  = false;

        RunScenarioAction(() =>
        {
            _UndoInProgress = true;

            try
            {
                GetHubMethod()?.Invoke(this);
                point.Callback?.Invoke(this);
            }
            finally
            {
                _UndoInProgress = false;
            }
        });
    }

    // Makes the shown hub an undo point. Every undo point is written to the browser by PersistAsync (GlobalData.Persistence.cs).
    public void SaveGame()
    {
        // A hub shown again in the same round (no undoable screen in between) is not saved twice. A hub of the next round
        // reached straight from the end-of-round popup is a new undo point, otherwise undo would skip back a round.
        if (_UndoInProgress || (UndoStack.Count != 0 && UndoStack.Peek().Callback == null && UndoStack.Peek().Round == UndoRound)) return;
        ResumeMethod = string.Empty;
        UndoStack.Push(NewUndoData(null, string.Empty, JsonConvert.SerializeObject(this), UndoRound));
        RedoStack.Clear();
    }

    // data: a save code from the load popup; empty: the browser's save ("continue") and, when it belongs to it, the undo history
    public async Task LoadGameAsync(string data, NavigationManager nav, HttpClient http, IJSRuntime js)
    {
        UndoStack.Clear();
        RedoStack.Clear();

        bool    fromBrowser = string.IsNullOrEmpty(data);
        string? base64      = fromBrowser ? await js.InvokeAsync<string?>("localStorage.getItem", "continue") : data;

        if (string.IsNullOrEmpty(base64))
        {
            nav.NavigateTo("");
            return;
        }

        ResetScenarioVars();
        ActiveHub            = null;
        ActiveWindow         = null;
        ActivePopup          = null;
        ActiveInputPopup     = null;
        ActiveMirrorQuestion = null;
        ResumeMethod         = string.Empty; // saves from before ResumeMethod open on the hub

        string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Reuse,
            NullValueHandling      = NullValueHandling.Ignore
        };
        string language = Language; // the menu language is a device setting, not part of the save
        JsonConvert.PopulateObject(json, this, settings);
        Language = language;

        Dictionary<string, Dictionary<string, string>> scenarioLocalizations         = new();
        Dictionary<string, Dictionary<string, string>> scenarioGameplayLocalizations = new();

        await LoadScenarioLanguagesOptionsAsync(nav, http, scenarioLocalizations, scenarioGameplayLocalizations);
        if (scenarioLocalizations.Count == 0) return;
        ApplyScenarioLanguage(scenarioLocalizations, scenarioGameplayLocalizations);

        Action<GlobalData>? activeHub = GetHubMethod();
        Action<GlobalData>? screen    = ResolveScreen(ResumeMethod);

        if (activeHub == null && screen == null)
        {
            nav.NavigateTo("");
            return;
        }

        // The screen is rebuilt the way undo does it: from the state saved before its method ran
        RunScenarioAction(() =>
        {
            _UndoInProgress = true;

            try
            {
                activeHub?.Invoke(this);
                screen?.Invoke(this);
            }
            finally
            {
                _UndoInProgress = false;
            }
        });

        await RestoreUndoHistoryAsync(js, fromBrowser, json, screen);
    }

    public async Task LoadScenarioLanguagesOptionsAsync(NavigationManager nav, HttpClient http, Dictionary<string, Dictionary<string, string>> scenarioLocalizations,
                                                        Dictionary<string, Dictionary<string, string>> scenarioGameplayLocalizations)
    {
        await Narration.LoadAsync(http, ScenarioId);

        string scenarioLocalization = ScenarioId switch
        {
            ScenarioId.CostOfDisease => "localization/TheCostOfDisease_Localization.csv",
            ScenarioId.FearOfUnknown => "localization/FearOfTheUnknown_Localization.csv",
            ScenarioId.TimeOfWar     => "localization/ATimeOfWar_Localization.csv",
            _                        => string.Empty
        };

        string scenarioGameplayLocalization = ScenarioId switch
        {
            ScenarioId.CostOfDisease => "localization/TheCostOfDisease_Gameplay_Localization.csv",
            ScenarioId.FearOfUnknown => "localization/FearOfTheUnknown_Gameplay_Localization.csv",
            ScenarioId.TimeOfWar     => "localization/ATimeOfWar_Gameplay_Localization.csv",
            _                        => string.Empty
        };

        if (scenarioLocalization == string.Empty || scenarioGameplayLocalization == string.Empty)
        {
            nav.NavigateTo("");
            return;
        }

        (await http.GetStringAsync(scenarioLocalization)).ProcessLocalizationFile(scenarioLocalizations);
        (await http.GetStringAsync(scenarioGameplayLocalization)).ProcessLocalizationFile(scenarioGameplayLocalizations);

        // Translations of this scenario (localization/<code>/<same file name>), English for every tag they lack
        foreach (AppLanguage translation in AppLanguages.Translations.Where(translation => translation.Scenarios.Contains(ScenarioId)))
        {
            try
            {
                (await http.GetStringAsync(translation.File(Path.GetFileNameWithoutExtension(scenarioLocalization)))).ProcessLocalizationFile(scenarioLocalizations);
                (await http.GetStringAsync(translation.File(Path.GetFileNameWithoutExtension(scenarioGameplayLocalization)))).ProcessLocalizationFile(scenarioGameplayLocalizations);
                scenarioLocalizations.FillMissing(translation.Name, AppLanguages.ENGLISH_STORY);
                scenarioGameplayLocalizations.FillMissing(translation.Name, AppLanguages.ENGLISH);
                scenarioLocalizations[translation.Name]["Gameplay_Lang"] = translation.Name;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{translation.Name} story of {ScenarioId} not loaded: {e.Message}");
                scenarioLocalizations.Remove(translation.Name);
                scenarioGameplayLocalizations.Remove(translation.Name);
            }
        }

        OtherPossibleScenarioLanguages.Clear();
        OtherPossibleScenarioLanguages.AddRange(scenarioLocalizations.Keys);
    }

    // Starts the active scenario after the setup screens
    public bool StartScenario()
    {
        Action<GlobalData>? init = ScenarioId switch
        {
            ScenarioId.CostOfDisease => TheCostOfDisease.Init,
            ScenarioId.FearOfUnknown => FearOfTheUnknown.Init,
            ScenarioId.TimeOfWar     => ATimeOfWar.Init,
            _                        => null
        };

        if (init == null) return false;
        RunScenarioAction(() => init(this));
        return true;
    }

    public void ShowFinalEnding()
    {
        switch (ScenarioId)
        {
            case ScenarioId.CostOfDisease:
            {
                TheCostOfDisease.FinalEnding(this);
                break;
            }
            case ScenarioId.FearOfUnknown:
            {
                FearOfTheUnknown.FinalEnding(this);
                break;
            }
            case ScenarioId.TimeOfWar:
            {
                ATimeOfWar.FinalEnding(this);
                break;
            }
        }
    }

    private Action<GlobalData>? GetHubMethod()
    {
        if (ScenarioId == ScenarioId.FearOfUnknown)
        {
            return FearOfTheUnknownVars.HubId switch
            {
                FearOfTheUnknownHubId.WitchHunt  => FearOfTheUnknown.WitchHunt,
                FearOfTheUnknownHubId.Foreigners => FearOfTheUnknown.Foreigners,
                FearOfTheUnknownHubId.Creature   => FearOfTheUnknown.Creature,
                FearOfTheUnknownHubId.Isolation  => FearOfTheUnknown.Isolation,
                FearOfTheUnknownHubId.Tension    => FearOfTheUnknown.Tension,
                FearOfTheUnknownHubId.Privatized => FearOfTheUnknown.Privatized,
                FearOfTheUnknownHubId.Liberal    => FearOfTheUnknown.Liberal,
                _                                => null
            };
        }

        if (ScenarioId == ScenarioId.TimeOfWar)
        {
            return ATimeOfWarVars.HubId switch
            {
                ATimeOfWarHubId.TakingSides  => ATimeOfWar.TakingSides,
                ATimeOfWarHubId.Martial      => ATimeOfWar.Martial,
                ATimeOfWarHubId.TimeTravel   => ATimeOfWar.TimeTravel,
                ATimeOfWarHubId.MonarchReign => ATimeOfWar.MonarchReign,
                ATimeOfWarHubId.Peace        => ATimeOfWar.Peace,
                ATimeOfWarHubId.Warning      => ATimeOfWar.Warning,
                ATimeOfWarHubId.Paradox      => ATimeOfWar.Paradox,
                _                            => null
            };
        }

        if (ScenarioId == ScenarioId.CostOfDisease)
        {
            return TheCostOfDiseaseVars.HubId switch
            {
                CostOfDiseaseHubId.Fever        => TheCostOfDisease.Fever,
                CostOfDiseaseHubId.Devastation  => TheCostOfDisease.Devastation,
                CostOfDiseaseHubId.Hospital     => TheCostOfDisease.Hospital,
                CostOfDiseaseHubId.GloomyGothic => TheCostOfDisease.GloomyGothic,
                CostOfDiseaseHubId.Prosperity   => TheCostOfDisease.Prosperity,
                CostOfDiseaseHubId.NoUniversity => TheCostOfDisease.NoUniversity,
                CostOfDiseaseHubId.University   => TheCostOfDisease.University,
                _                               => null
            };
        }

        return null;
    }
}