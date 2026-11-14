namespace MyFathersWorkApp;

// Generation III "Future Consequences" (port map 2.6): reached from TimeEnd1 (Paradox tokens >= dox) or TimeEnd2 (Time
// Machine not repaired). Intro, Wooden Figure fortune, Utopian Experiments, the time travelers, the changing worlds
// (Village Chronicle pages 17 - 22) with their time shifts, the old man's letter, the stable timeline and the Masterwork
// that pacifies the timestream, before Scoring. Also the hub sections and the University explanation (UniGen3) shared
// with "You Cant Stop Progress" (file 08).
public static partial class ATimeOfWar
{
    // Village Chronicle pages of the worlds (originally $cont / $twcode)
    private static readonly int[] _WORLDS = [17, 18, 19, 20, 21, 22];

    private const int _WORLD_BANK   = 18;
    private const int _WORLD_CHURCH = 19;

    public static void Warning(GlobalData globalData)
    {
        // Round 1 -> Warning1
        // Round 2 -> Warning2 / Warning2time (timeline stable)
        // Round 3 -> Warning3 / Warning3time (timeline stable)
        ATimeOfWarVars vars   = Vars(globalData);
        Years          years  = WarningYears(globalData);
        bool           early  = years == Years.Early;
        bool           late   = years == Years.Late;
        bool           stable = vars.TimelineStable;
        int            world  = vars.World;

        vars.HubId           = ATimeOfWarHubId.Warning;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle(title => title.FormatWithCondition(0, () => early));
        globalData.ActiveHub.SetSubtitle(years);

        // Early Years: Technician, Utopian Experiments, University (with the end-of-round link)
        AddUtopianSections(globalData, () => early, () => early, () => early, Warning_0);

        // Middle / Late Years in a shifting timeline: the current world (drawn on WarningWorldSetup; nothing is shown
        // before it is drawn, so the hub never draws)
        if (!early && !stable && world != 0)
        {
            int index = world - _WORLDS[0];

            const string       worldSection = "World";
            GameplayHubSection worldPanel   = globalData.ActiveHub.AddSection(worldSection);
            worldPanel.FormatTitle(title => title.FormatWithIndex(0, index));
            worldPanel.AddNextContent(world, worldSection, false, content => content
                                                                         .FormatWithCondition(0, () => late)
                                                                         .FormatWithIndex(1, vars.Flavor("Warning#" + _WORLD_BANK + "#" + vars.WorldShifts, 2)));

            const string       stressfulSection = "Stressful";
            GameplayHubSection stressful        = globalData.ActiveHub.AddSection(stressfulSection);
            stressful.ReplaceShouldShow(() => world == _WORLD_CHURCH);
            stressful.AddDefaultContent(stressfulSection, content => content.FormatWithCondition(0, () => late));

            // Originally the "change the world" links of Warning2 / Warning3 ($search + 1, then TimeWarp)
            const string       visitSection = "Visit";
            GameplayHubSection visit        = globalData.ActiveHub.AddSection(visitSection, true);
            visit.FormatTitle(title => title.FormatWithIndex(0, index));
            visit.AddDefaultContent(visitSection, content => content.FormatWithIndex(0, index));
            visit.AddSpecialClickHere(visitSection, data =>
            {
                Vars(data).TimeShifts += 1;
                TimeWarp(data);
            }, true);
        }

        const string       utopianWorldSection        = "UtopianWorld";
        const string       utopianWorldEndSection     = "UtopianWorldEnd";
        const string       utopianWorldEndLateSection = "UtopianWorldEndLate";
        GameplayHubSection utopianWorld               = globalData.ActiveHub.AddSection(utopianWorldSection);
        utopianWorld.ReplaceShouldShow(() => !early && !stable);
        utopianWorld.AddDefaultContent(utopianWorldSection);
        if (late) utopianWorld.AddSpecialClickHere(utopianWorldEndLateSection, Warning_0, true, endOfRound: true);
        else
        {
            utopianWorld.AddNextContent(1, utopianWorldSection, true);
            utopianWorld.AddSpecialClickHere(utopianWorldEndSection, Warning_0, endOfRound: true);
        }

        // Stable timeline (originally Warning2time / Warning3time, $stasis == "yes"); the Masterwork section until it is
        // completed (originally $master)
        const string       masterworkSection = "Masterwork";
        GameplayHubSection masterwork        = globalData.ActiveHub.AddSection(masterworkSection, true);
        masterwork.ReplaceShouldShow(() => !early && stable && !vars.MasterworkDone);
        masterwork.AddDefaultContent(masterworkSection, content => content.FormatWithCondition(0, () => late));
        masterwork.AddSpecialClickHere(masterworkSection, Stasis);

        const string       perseveranceSection        = "Perseverance";
        const string       perseveranceEndSection     = "PerseveranceEnd";
        const string       perseveranceEndLateSection = "PerseveranceEndLate";
        GameplayHubSection perseverance               = globalData.ActiveHub.AddSection(perseveranceSection);
        perseverance.ReplaceShouldShow(() => !early && stable);
        perseverance.AddDefaultContent(perseveranceSection);
        if (late) perseverance.AddSpecialClickHere(perseveranceEndLateSection, Warning_0, true, endOfRound: true);
        else perseverance.AddSpecialClickHere(perseveranceEndSection, Warning_0, true, endOfRound: true);
    }

    private static void Warning_0(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        globalData.ShowEndOfRoundPopUp(WarningYears(globalData) switch
        {
            Years.Early                         => WarningEvent,
            Years.Middle when !vars.TimelineStable => TimeWarp2,
            Years.Middle                        => vars.MetGentleman ? OldManToken : WarningStableSetup,
            _                                   => Scoring
        });
    }

    // Round of Generation III. After the Late end-of-round popup Generation is 3 (outside the enum): the Late Years.
    private static Years WarningYears(GlobalData globalData)
    {
        return globalData.Generation == Generation.Third ? globalData.Years : Years.Late;
    }

    #region Shared with You Cant Stop Progress

    // Technician, Utopian Experiments and University sections of Warning1 and Paradox1 / 2 / 3 / 3b (tags Gen3_*). The
    // end-of-round link of the Early Years is part of the University section, as in the source.
    private static void AddUtopianSections(GlobalData globalData, Func<bool> technician, Func<bool> utopian, Func<bool> university, Action<GlobalData>? endOfRound)
    {
        const string caller = "Gen3";

        const string       technicianSection = "Technician";
        GameplayHubSection technicianPanel   = globalData.ActiveHub!.AddSection(technicianSection, false, caller);
        technicianPanel.ReplaceShouldShow(technician);
        technicianPanel.AddDefaultContent(technicianSection, null, caller);

        const string       utopianSection = "Utopian";
        GameplayHubSection utopianPanel   = globalData.ActiveHub.AddSection(utopianSection, false, caller);
        utopianPanel.ReplaceShouldShow(utopian);
        utopianPanel.AddDefaultContent(utopianSection, null, caller);

        const string       universitySection = "University";
        GameplayHubSection universityPanel   = globalData.ActiveHub.AddSection(universitySection, true, caller);
        universityPanel.ReplaceShouldShow(university);
        universityPanel.AddDefaultContent(universitySection, null, caller);
        universityPanel.AddSpecialClickHere(universitySection, UniGen3, false, null, caller);
        if (endOfRound != null) universityPanel.AddSpecialClickHere("UniversityEnd", endOfRound, true, null, caller, true);
    }

    // University explanation. Source bug B21 fixed: "pay $1 to" was only printed in the Paradox rounds, although the
    // Warning hub says "pay $1 to gain a Caretaker" as well. ($pgGen3 only stopped the hub setup popup from reappearing.)
    private static void UniGen3(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("ClickToReturn"),
            data => (Vars(data).HubId == ATimeOfWarHubId.Paradox ? Paradox : (Action<GlobalData>)Warning).Invoke(data));
    }

    #endregion Shared with You Cant Stop Progress

    #region Intro

    // Originally WarningIntro. The source draws $total here; it is drawn when a time shift could reach it (TimeWarp_0).
    // "debug" town name "v1.0" dropped (port map 7, B29).
    private static void WarningIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.TimeShifts     = 0;
        vars.World          = 0;
        vars.PreviousWorld  = 0;
        vars.WorldShifts    = 0;
        vars.TimelineStable = false;
        vars.MasterworkDone = false;
        vars.Ending         = 4;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("WarningIntro");
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_III);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent();

        // The Wooden Figures of Generation I (originally $crazy): good or ill fortune (the once-per-game
        // $WarningIntronextPsg, one occurrence)
        if (vars.WoodenFigures >= 1) globalData.ActiveWindow.AddClickHereToContinue(data => Vars(data).RandomElement([CostStick, CostStick2], _RND_FIGURE_FORTUNE).Invoke(data));
        else globalData.ActiveWindow.AddClickHereToContinue(Late2ResBCheck);
    }

    private static void CostStick(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CostStick_0);
    }

    private static void CostStick_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_WoodenFigureToken, PopUpButton.Confirm, Late2ResBCheck);
    }

    private static void CostStick2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CostStick2_0);
    }

    private static void CostStick2_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_WoodenFigureToken, PopUpButton.Confirm, Late2ResBCheck);
    }

    // Everybody stayed silent in the Blame game of Generation II (originally $late == 2)
    private static void Late2ResBCheck(GlobalData globalData)
    {
        if (Vars(globalData).AllSilent) Late2ResB(globalData);
        else WarningWarEnding(globalData);
    }

    private static void Late2ResB(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Late2ResB_0);
    }

    private static void Late2ResB_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.MFWlogo, PopUpButton.Confirm, WarningWarEnding);
    }

    // The civil war of Generation I in this timeline (the faction the town supported)
    private static void WarningWarEnding(GlobalData globalData)
    {
        bool separatists = Vars(globalData).WarWinner == WarFaction.Separatists;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContent(separatists ? 1 : 2, false, content => content.FormatWithReplacement(0, Winner(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(WhoopsUtopian);
    }

    private static void WhoopsUtopian(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(WhoopsUtopian_0);
    }

    private static void WhoopsUtopian_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_ModernExperiment, PopUpButton.Confirm, WhoopsUtopianb);
    }

    private static void WhoopsUtopianb(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(WhoopsUtopianb_0);
    }

    private static void WhoopsUtopianb_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_CircuitryToken, PopUpButton.Confirm, WarningEarlySetup);
    }

    #endregion Intro

    #region Setup

    // Originally the setupStyle block of Warning1 (guarded by $pgGen3; shown once at the start of the round)
    private static void WarningEarlySetup(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, Warning,
            content => content.FormatWithCondition(0, () => globalData.PlayersNum == 3));
    }

    // Originally the setupStyle block of Warning2 / Warning3 ("Turn to page {$cont}"), shown whenever a new world appears.
    // The world is drawn here, where it becomes visible, from the pages other than the previous world.
    private static void WarningWorldSetup(GlobalData globalData)
    {
        ATimeOfWarVars vars  = Vars(globalData);
        int            world = vars.World != 0 ? vars.World : vars.RandomElement(_WORLDS.Where(page => page != vars.PreviousWorld).ToList(), _RND_WORLD + Math.Min(vars.WorldShifts, 9));

        globalData.SaveToUndo();
        vars.World = world;

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, Warning,
            content => content.FormatWithReplacement(0, world.ToString()));
    }

    // Originally the setupStyle block of Warning2time / Warning3time: shown while the Masterwork is not completed
    private static void WarningStableSetup(GlobalData globalData)
    {
        if (Vars(globalData).MasterworkDone) Warning(globalData);
        else WarningStableSetup_0(globalData);
    }

    private static void WarningStableSetup_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, Warning);
    }

    // The previous world disappears; the next one is drawn on WarningWorldSetup
    private static void ShiftWorld(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);
        vars.PreviousWorld =  vars.World;
        vars.World         =  0;
        vars.WorldShifts   += 1;
    }

    #endregion Setup

    #region End of the Early Years

    // Runs after the Early end-of-round popup (Years == Middle)
    private static void WarningEvent(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("WarningEvent");
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, globalData.LocalizedPlayerNumberIndex()));
        globalData.ActiveWindow.AddClickHereToContinue(TimeWarp1st);
    }

    // The source draws the first world ($cont) here; it is drawn on WarningWorldSetup, where the page is shown
    private static void TimeWarp1st(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(WarningMaybe);
    }

    private static void WarningMaybe(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(WarningMaybe_0);
    }

    private static void WarningMaybe_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_ModernExperiment, PopUpButton.Confirm, WarningWorldSetup);
    }

    #endregion End of the Early Years

    #region Time shifts

    // A player completed an action at the world's location ($search + 1 in the hub link)
    private static void TimeWarp(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, vars.Flavor("TimeWarp#" + vars.TimeShifts, 4)));
        globalData.ActiveWindow.AddClickHereToContinue(TimeWarp_0);
    }

    // Lost in Time: 1 or 2VP for the triggering player; then a new world, or the timeline stabilizes once the time shifts
    // reach $total
    private static void TimeWarp_0(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);
        int            vp   = vars.RandomInRange(1, 2, _RND_SHIFT_VP + Math.Min(vars.TimeShifts, 9));

        globalData.SaveToUndo();
        ShiftWorld(globalData);

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, TimeWarpNext,
            content => content.FormatWithReplacement(0, vp.ToString()));
    }

    private static void TimeWarpNext(GlobalData globalData)
    {
        if (TimeShiftLimitReached(globalData)) WarningEvent2(globalData);
        else WarningWorldSetup(globalData);
    }

    // Originally $search >= $total ($total: 6 - 10 minus 3 / 2 / 1 for 2 / 3 / 4 players). The limit is only drawn when
    // the number of time shifts does not decide it already; in mirror mode the question shows the two next screens.
    private static bool TimeShiftLimitReached(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);
        int            min  = globalData.PlayersNum + 1;
        int            max  = min + 4;

        return vars.TimeShifts >= max || (vars.TimeShifts >= min && vars.TimeShifts >= vars.RandomInRange(min, max, _RND_SHIFT_LIMIT));
    }

    // Runs after the Middle end-of-round popup (Years == Late) in a shifting timeline. The source adds 1 to $search in the
    // end-of-round link of Warning2; it is added here.
    private static void TimeWarp2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).TimeShifts += 1;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TimeWarp2_0);
    }

    private static void TimeWarp2_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        ShiftWorld(globalData);

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.GainServantFromLost, PopUpButton.Confirm, data =>
        {
            ATimeOfWarVars vars = Vars(data);

            if (vars.MetGentleman) OldManToken(data);
            else if (vars.TimelineStable) WarningStableSetup(data);
            else WarningWorldSetup(data);
        });
    }

    // The time shifts reached $total: the timeline is stable from now on (originally $stasis = "yes")
    private static void WarningEvent2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).TimelineStable = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(WarningStableSetup);
    }

    #endregion Time shifts

    #region Old man's letter

    // A player met the gentleman in Generation I (originally $rumor2) and holds the {figure} token. Runs after the Middle
    // end-of-round popup (Years == Late).
    private static void OldManToken(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Figure(globalData)));
        globalData.ActiveWindow.AddOnceYouAreReady(OldManTokenRevealed);
    }

    // The Generation I loser's flag -> OldManYes
    private static void OldManTokenRevealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [OldManYes, OldManNo], true, content => content
                                                                                                  .FormatWithReplacement(0, Loser(globalData))
                                                                                                  .FormatWithReplacement(1, Winner(globalData)));
    }

    private static void OldManNo(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(OldManNo_0);
    }

    private static void OldManNo_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ExperimentABack, PopUpButton.Confirm, OldManNext);
    }

    private static void OldManYes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(OldManYes_0);
    }

    private static void OldManYes_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, FigureIcon(globalData), PopUpButton.Confirm, OldManNext,
            content => content.FormatWithReplacement(0, Figure(globalData)));
    }

    private static void OldManNext(GlobalData globalData)
    {
        if (Vars(globalData).TimelineStable) WarningStableSetup(globalData);
        else WarningWorldSetup(globalData);
    }

    #endregion Old man's letter

    #region Stasis

    // A Masterwork completed in the stable timeline (originally $master = "yes"). Ending (port map 8): 3 "A Wrinkle Within
    // Time"; without it the ending stays 4 "A Timeline Unbound" (set in WarningIntro).
    private static void Stasis(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.MasterworkDone = true;
        vars.Ending         = 3;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, globalData.LocalizedPlayerNumberIndex()));
        globalData.ActiveWindow.AddClickHereToContinue(Stasis_0);
    }

    private static void Stasis_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Bodies_Icon, PopUpButton.Confirm, Warning);
    }

    #endregion Stasis
}
