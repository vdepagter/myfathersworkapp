namespace MyFathersWorkApp;

// Generation III "You Cant Stop Progress" (port map 2.7): reached from TimeEnd1 (fewer Paradox tokens than dox) or
// TimeEnd2 (Time Machine repaired), via Paradoxical. Time Paradox cards, intro, Utopian Experiments, the {figure} token
// on the Creepy Track, Giant Masterworks, the secret vote to publish the Time Machine, the random paradox of the Late
// Years and the Giant Masterwork check before Scoring. The Technician / Utopian / University sections and UniGen3 are in
// file 07.
public static partial class ATimeOfWar
{
    // Originally $timemistake (ParadoxTimeRandom): the Late hub changes for these (Paradox3b)
    private const int _MISTAKE_SUSPICION   = 6;
    private const int _MISTAKE_CATASTROPHE = 8;

    public static void Paradox(GlobalData globalData)
    {
        // Round 1 -> Paradox1
        // Round 2 -> Paradox2
        // Round 3 -> Paradox3 / Paradox3b (after "A Return to Suspicion" or "Utter Catastrophe")
        ATimeOfWarVars vars        = Vars(globalData);
        Years          years       = ParadoxYears(globalData);
        bool           early       = years == Years.Early;
        bool           late        = years == Years.Late;
        bool           suspicion   = vars.TimeMistake == _MISTAKE_SUSPICION;
        bool           catastrophe = vars.TimeMistake == _MISTAKE_CATASTROPHE;
        bool           monarchists = vars.WarWinner == WarFaction.Monarchists;

        vars.HubId           = ATimeOfWarHubId.Paradox;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(years);

        // The {figure} token on the Creepy Track (port map 7, B22: still shown after "Utter Catastrophe" returned it, as
        // shipped)
        const string       scienceSection = "ScienceAtAnyCost";
        GameplayHubSection science        = globalData.ActiveHub.AddSection(scienceSection);
        science.ReplaceShouldShow(() => !suspicion);
        science.AddDefaultContent(scienceSection, content => content
                                                             .FormatWithReplacement(0, Figure(globalData))
                                                             .FormatWithCondition(1, () => monarchists));

        const string       mobSection = "AngryMobReturn";
        GameplayHubSection mob        = globalData.ActiveHub.AddSection(mobSection);
        mob.ReplaceShouldShow(() => suspicion);
        mob.AddDefaultContent(mobSection);

        AddUtopianSections(globalData, () => !catastrophe, () => true, () => !catastrophe, early ? Paradox_0 : null);

        // Middle / Late: who gains the In Memoriam upgrade (none after "Utter Catastrophe"). The end-of-round link is part
        // of this section (Paradox3b: an untitled section after it).
        const string       memoriamSection        = "InMemoriam";
        const string       memoriamEndSection     = "InMemoriamEnd";
        const string       memoriamEndLateSection = "InMemoriamEndLate";
        GameplayHubSection memoriam               = globalData.ActiveHub.AddSection(memoriamSection);
        memoriam.ReplaceShouldShow(() => !early);
        if (catastrophe) memoriam.AddNextContent(1, memoriamSection, false, content => content.FormatWithReplacement(0, globalData.TownName));
        else
        {
            memoriam.AddDefaultContent(memoriamSection, content => content
                                                                   .FormatWithCondition(0, () => vars.Releases >= 1)
                                                                   .FormatWithCondition(1, () => late));
        }

        if (late) memoriam.AddSpecialClickHere(memoriamEndLateSection, Paradox_0, true, endOfRound: true);
        else memoriam.AddSpecialClickHere(memoriamEndSection, Paradox_0, true, endOfRound: true);
    }

    private static void Paradox_0(GlobalData globalData)
    {
        globalData.ShowEndOfRoundPopUp(ParadoxYears(globalData) switch
        {
            Years.Early  => ParadoxFirst,
            Years.Middle => Vars(globalData).Releases >= 1 ? ParadoxRandoEvent : Paradox,
            _            => ParadoxGiantCheck
        });
    }

    // Round of Generation III. After the Late end-of-round popup Generation is 3 (outside the enum): the Late Years.
    private static Years ParadoxYears(GlobalData globalData)
    {
        return globalData.Generation == Generation.Third ? globalData.Years : Years.Late;
    }

    // Player deciding about the Time Machine schematics (originally MonuA ... MonuD)
    private static string MonuPlayer(GlobalData globalData)
    {
        return globalData.GetPlayerNameByIndex(Vars(globalData).MonuIndex);
    }

    #region Intro

    // Originally Paradoxical (after TimeEnd1 / TimeEnd2): the Time Paradox cards
    private static void Paradoxical(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Paradoxical_0);
    }

    private static void Paradoxical_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_TimeParadox, PopUpButton.Confirm, ParadoxIntro);
    }

    // Originally ParadoxIntro. The source draws $accolade and $timemistake here. $accolade is drawn on the popup that shows
    // it; source bug B19 fixed: $timemistake is only drawn in ParadoxTimeRandom (the source kept this first draw when
    // nobody released the schematics, so ParadoxEvent could show "Rebuilding" without a catastrophe). "debug" town name
    // "v1.0" dropped (port map 7, B29).
    private static void ParadoxIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.TimeMistake = 0;
        vars.Releases    = 0;
        vars.Giants      = 0;
        vars.MonuIndex   = 0;
        vars.Ending      = 0;
        foreach (string player in globalData.GetActivePlayers()) vars.Released[player] = false;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("WarningIntro"); // Unity plays the WarningIntro clip for ParadoxIntro
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_III);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TimeTravelEvent);
    }

    private static void TimeTravelEvent(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, globalData.LocalizedPlayerNumberIndex()));
        globalData.ActiveWindow.AddClickHereToContinue(TimeTravelEvent_0);
    }

    private static void TimeTravelEvent_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_ModernExperiment, PopUpButton.Confirm, TimeTravelEventb);
    }

    private static void TimeTravelEventb(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TimeTravelEventb_0);
    }

    // A player met the gentleman in Generation I (originally $rumor2) and holds the {figure} token
    private static void TimeTravelEventb_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_CircuitryToken, PopUpButton.Confirm,
            Vars(globalData).MetGentleman ? FigureontheTrackIn : NobodyFigures);
    }

    private static void FigureontheTrackIn(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Figure(globalData)));
        globalData.ActiveWindow.AddOnceYouAreReady(FigureontheTrackInRevealed);
    }

    private static void FigureontheTrackInRevealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Figure(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(FigureontheTrackInRevealed_0);
    }

    private static void FigureontheTrackInRevealed_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, FigureontheTrackInb);
    }

    private static void FigureontheTrackInb(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Figure(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(FigureontheTrackInb_0);
    }

    // Originally $accolade (space 12 / 13 of the Creepy Track)
    private static void FigureontheTrackInb_0(GlobalData globalData)
    {
        int accolade = Accolade(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, FigureIcon(globalData), PopUpButton.Confirm, Late2ResACheck,
            content => AccoladeText(globalData, content, accolade));
    }

    private static void NobodyFigures(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(NobodyFigures_0);
    }

    private static void NobodyFigures_0(GlobalData globalData)
    {
        int accolade = Accolade(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMob_Icon, PopUpButton.Confirm, Late2ResACheck,
            content => AccoladeText(globalData, content, accolade));
    }

    private static int Accolade(GlobalData globalData) => Vars(globalData).RandomElement([12, 13], _RND_ACCOLADE);

    // The reward for reaching the {figure} token depends on the faction the town supported in Generation I
    private static string AccoladeText(GlobalData globalData, string content, int accolade)
    {
        return content
              .FormatWithReplacement(0, Figure(globalData))
              .FormatWithReplacement(1, accolade.ToString())
              .FormatWithCondition(2, () => Vars(globalData).WarWinner == WarFaction.Monarchists);
    }

    // Everybody stayed silent in the Blame game of Generation II (originally $late == 2)
    private static void Late2ResACheck(GlobalData globalData)
    {
        if (Vars(globalData).AllSilent) Late2ResA(globalData);
        else ParadoxSetup(globalData);
    }

    private static void Late2ResA(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Late2ResA_0);
    }

    private static void Late2ResA_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.MFWlogo, PopUpButton.Confirm, ParadoxSetup);
    }

    // Originally the setupStyle block of Paradox1 (guarded by $pgGen3; shown once at the start of the round)
    private static void ParadoxSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, Paradox,
            content => content.FormatWithCondition(0, () => globalData.PlayersNum == 3));
    }

    #endregion Intro

    #region End of the Early Years

    // Runs after the Early end-of-round popup (Years == Middle): the Giant Masterworks
    private static void ParadoxFirst(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ParadoxFirst_0);
    }

    // The Wooden Figures of Generation I (originally $crazy): which story follows (the once-per-game
    // $ParadoxFirstnextPsg, one occurrence) is drawn when the popup is closed
    private static void ParadoxFirst_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_GiantMW, PopUpButton.Confirm, data =>
        {
            if (Vars(data).WoodenFigures > 0) Vars(data).RandomElement([FutureStick, CostStick1], _RND_PARADOX_FIGURE).Invoke(data);
            else MonumentVote(data);
        });
    }

    private static void CostStick1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CostStick1_0);
    }

    private static void CostStick1_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_WoodenFigureToken, PopUpButton.Confirm, MonumentVote);
    }

    private static void FutureStick(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(FutureStick_0);
    }

    private static void FutureStick_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_WoodenFigureToken, PopUpButton.Confirm, MonumentVote);
    }

    // Publish the Time Machine schematics? Each player decides in secret, in turn order.
    private static void MonumentVote(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.MonuIndex = 0;
        vars.Releases  = 0;
        foreach (string player in globalData.GetActivePlayers()) vars.Released[player] = false;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Monu);
    }

    // Originally MonuA ... MonuD (MonuE: 5 players, dropped)
    private static void Monu(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDoNotOtherPlayersToSee(MonuPlayer(globalData), MonuRevealed);
    }

    private static void MonuRevealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => MonuChosen(data, false), data => MonuChosen(data, true)], true);
    }

    // Originally $platm ... $pldtm and $release
    private static void MonuChosen(GlobalData globalData, bool release)
    {
        ATimeOfWarVars vars = Vars(globalData);
        vars.Released[MonuPlayer(globalData)] =  release;
        if (release) vars.Releases            += 1;
        vars.MonuIndex                        += 1;

        if (vars.MonuIndex < globalData.PlayersNum) Monu(globalData);
        else MonuRes(globalData);
    }

    private static void MonuRes(GlobalData globalData)
    {
        bool chaos = Vars(globalData).Releases >= 1;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(title => title.FormatWithCondition(0, () => chaos));
        globalData.ActiveWindow.AddNextContent(chaos ? 1 : 2);
        globalData.ActiveWindow.AddNextContent(3, true);
        globalData.ActiveWindow.AddClickHereToContinue(MonuRes_0);
    }

    // Exactly one release: the releaser gains 3VP; more: each releaser gains a Compulsion. The others draw a Utopian
    // Experiment and gain 1 Creepy (players in turn order, releasers first, as in the source).
    private static void MonuRes_0(GlobalData globalData)
    {
        ATimeOfWarVars vars     = Vars(globalData);
        string[]       players  = globalData.GetActivePlayers();
        string         released = globalData.GetScenarioLocalizedTag("MonuRes_0_Released");
        string         hidden   = globalData.GetScenarioLocalizedTag("MonuRes_0_Hidden");

        string lines = string.Concat(players.Where(player => vars.Released[player])
                                            .Select(player => released.FormatWithReplacement(0, player).FormatWithCondition(1, () => vars.Releases == 1)))
                     + string.Concat(players.Where(player => !vars.Released[player]).Select(player => hidden.FormatWithReplacement(0, player)));

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_EstateUpgradeBack, PopUpButton.Confirm, Paradox,
            content => content + lines.TrimTrailingBreaks());
    }

    #endregion End of the Early Years

    #region End of the Middle Years

    // Runs after the Middle end-of-round popup (Years == Late) when any player released the schematics. The source draws
    // $timemistake here; it is drawn on ParadoxTimeRandom, which shows it.
    private static void ParadoxRandoEvent(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("ParadoxRandoEvent_Continue"), ParadoxTimeRandom);
    }

    // The new paradox (originally $timemistake 1 - 8; RandomElement, not RandomInRange: mirror mode then lists the texts)
    private static void ParadoxTimeRandom(GlobalData globalData)
    {
        ATimeOfWarVars vars    = Vars(globalData);
        int            mistake = vars.RandomElement(Enumerable.Range(1, 8).ToList(), _RND_TIME_MISTAKE);

        globalData.SaveToUndo();
        vars.TimeMistake = mistake;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(title => title.FormatWithIndex(0, mistake - 1));
        globalData.ActiveWindow.AddNextContent(mistake);
        globalData.ActiveWindow.AddClickHereToContinue(ParadoxTimeRandom_0);
    }

    // Outcome 7 lists the players who released the schematics (at least one: the event only runs then)
    private static void ParadoxTimeRandom_0(GlobalData globalData)
    {
        ATimeOfWarVars vars    = Vars(globalData);
        int            mistake = vars.TimeMistake;
        string         flip    = globalData.GetScenarioLocalizedTag("ParadoxTimeRandom_0_Flip");
        string         flips   = string.Concat(globalData.GetActivePlayers().Where(player => vars.Released[player]).Select(player => flip.FormatWithReplacement(0, player)));

        string icon = mistake switch
        {
            1 => PopUpIcon.ExperimentCFront,
            2 => PopUpIcon.CompulsionBack,
            3 => PopUpIcon.ScoreTrackMarker,
            5 => PopUpIcon.LoseCreepy,
            6 => PopUpIcon.AngryMob_Icon,
            7 => PopUpIcon.S3_CardBack,
            8 => PopUpIcon.VillageChronicleCover,
            _ => PopUpIcon.MFWlogo
        };

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, icon, PopUpButton.Confirm, Paradox, content => content
                                                                                                                              .FormatWithReplacement(0, Figure(globalData))
                                                                                                                              .FormatWithReplacement(1, flips)
                                                                                                                              .FormatWithIndex(2, mistake - 1));
    }

    #endregion End of the Middle Years

    #region End of Generation III

    // Runs after the Late end-of-round popup (Generation == 3). Paradox3b linked here without the end-of-round popup
    // (port map 7, B20); the web app always shows it (it also advances the Generation).
    private static void ParadoxGiantCheck(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();

        for (int giants = 0; giants <= globalData.PlayersNum; ++giants)
        {
            int count = giants;
            globalData.ActiveWindow.AddCustomLink(count.ToString(), data =>
            {
                Vars(data).Giants = count;
                ParadoxEvent(data);
            });
        }
    }

    // Ending (port map 8): the Giant Masterworks completed, one fewer if any player released the schematics (not after
    // "Utter Catastrophe", as in the source), against 1 / 2 / 3 for 2 / 3 / 4 players -> 1 "A Giant Success", else 2
    // "Behind the Curve". Giants itself keeps the players' answer (the source subtracted from $giants).
    private static void ParadoxEvent(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars        = Vars(globalData);
        bool           catastrophe = vars.TimeMistake == _MISTAKE_CATASTROPHE;
        int            giants      = vars.Giants - (!catastrophe && vars.Releases >= 1 ? 1 : 0);
        vars.Ending = giants >= globalData.PlayersNum - 1 ? 1 : 2;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(title => title.FormatWithCondition(0, () => catastrophe));
        globalData.ActiveWindow.AddNextContent(catastrophe ? 2 : 1);
        globalData.ActiveWindow.AddClickHereToContinue(catastrophe ? Scoring : ParadoxEvent_0);
    }

    // The In Memoriam Estate Upgrade
    private static void ParadoxEvent_0(GlobalData globalData)
    {
        bool released = Vars(globalData).Releases >= 1;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_EstateUpgradeBack, PopUpButton.Confirm, Scoring,
            content => content.FormatWithCondition(0, () => released));
    }

    #endregion End of Generation III
}
