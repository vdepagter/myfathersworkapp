namespace MyFathersWorkApp;

// Generation III "Peace Through War" (port map 2.5): reached when the Separatists win the war at the end of Generation II
// Martial. Intro, World Conquest setup, battles at the end of each round, the famine vote (new meat), the gas vote and
// the Final Battle before Scoring.
public static partial class ATimeOfWar
{
    public static void Peace(GlobalData globalData)
    {
        // Round 1 -> Peace1
        // Round 2 -> Peace2
        // Round 3 -> Peace3
        ATimeOfWarVars vars  = Vars(globalData);
        Years          years = PeaceYears(globalData);

        vars.HubId           = ATimeOfWarHubId.Peace;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(years);

        const string       allegianceSection = "Allegiance";
        GameplayHubSection allegiance        = globalData.ActiveHub.AddSection(allegianceSection);
        allegiance.AddDefaultContent(allegianceSection);

        // The explanation link comes before the text, as in the source
        const string       warSection = "WarOnTheWorld";
        GameplayHubSection war        = globalData.ActiveHub.AddSection(warSection, true);
        war.ReplaceShouldShow(() => years != Years.Late);
        war.AddSpecialClickHere(warSection, HowtoWar);
        war.AddDefaultContent(warSection);

        const string       everythingSection = "EverythingIsAWeapon";
        GameplayHubSection everything        = globalData.ActiveHub.AddSection(everythingSection);
        everything.ReplaceShouldShow(() => vars.Bribe == ExtendedBool.True);
        everything.AddDefaultContent(everythingSection);

        // The gas was released at the end of the Middle Years (originally $peac == 2)
        const string       relationsSection = "PublicRelations";
        GameplayHubSection relations        = globalData.ActiveHub.AddSection(relationsSection);
        relations.ReplaceShouldShow(() => years == Years.Late && vars.Gas == ExtendedBool.True);
        relations.AddDefaultContent(relationsSection);

        // Late: the Final Battle; the end-of-Generation link is part of this section, as in the source
        const string       finalSection    = "FinalConflict";
        const string       finalEndSection = "FinalConflictEnd";
        GameplayHubSection final           = globalData.ActiveHub.AddSection(finalSection, true);
        final.ReplaceShouldShow(() => years == Years.Late);
        final.AddSpecialClickHere(finalSection, HowtoWar);
        final.AddDefaultContent(finalSection, content => content.FormatWithCondition(0, () => vars.Gas == ExtendedBool.False));
        final.AddSpecialClickHere(finalEndSection, Peace_0, true, endOfRound: true);

        const string       endSection = "End";
        GameplayHubSection end        = globalData.ActiveHub.AddSection(endSection);
        end.ReplaceShouldShow(() => years != Years.Late);
        end.AddSpecialClickHere(endSection, Peace_0, false, click => click.FormatWithCondition(0, () => years == Years.Early), endOfRound: true);
    }

    private static void Peace_0(GlobalData globalData)
    {
        globalData.ShowEndOfRoundPopUp(PeaceYears(globalData) switch
        {
            Years.Early  => FamineEvent,
            Years.Middle => PeaceEvent,
            _            => FinalBattleTime
        });
    }

    // Round of Generation III. After the Late end-of-round popup Generation is 3 (outside the enum): the Late Years.
    private static Years PeaceYears(GlobalData globalData)
    {
        return globalData.Generation == Generation.Third ? globalData.Years : Years.Late;
    }

    // Originally $tempbs: Weapon tokens collected at the start of the Middle and the Late Years (one draw for both, port
    // map 7, B24)
    private static int WeaponIncome(GlobalData globalData)
    {
        return Vars(globalData).RandomElement([1, 2], _RND_WEAPON_INCOME);
    }

    #region Intro

    // Originally PeaceIntro. The source draws $wlimit here; it is drawn on PeaceDiscard_0. "debug" town name "v1.0"
    // dropped (port map 7, B29).
    private static void PeaceIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.NewMeat = string.Empty;
        vars.Meat    = false;
        vars.Gas     = ExtendedBool.None;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("PeaceIntro");
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_III);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(PeaceBarnCheck);
    }

    // The Suspicious Barn was built for the Unified Monarchists (the Generation I losers) in Generation II
    private static void PeaceBarnCheck(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        if (vars.WarWinner == WarFaction.Monarchists && vars.SuspiciousBarn) SeparatistBarnCheck(globalData);
        else PeaceWar(globalData);
    }

    private static void SeparatistBarnCheck(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(SeparatistBarnCheck_0);
    }

    private static void SeparatistBarnCheck_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_EstateUpgradeBack, PopUpButton.Confirm, PeaceWar);
    }

    private static void PeaceWar(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(PeaceWar_0);
    }

    private static void PeaceWar_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_WorldConquest, PopUpButton.Confirm, PeacePrep,
            content => content.FormatWithCondition(0, () => globalData.PlayersNum == 2).FormatWithCondition(1, () => globalData.PlayersNum == 3));
    }

    private static void PeacePrep(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(PeacePrep_0);
    }

    private static void PeacePrep_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_WeaponToken, PopUpButton.Confirm, PeacetoRebuild);
    }

    // Estate tax by the buildings destroyed in Generation II: none, one-time $1, $1 every round
    private static void PeacetoRebuild(GlobalData globalData)
    {
        int destroyed = Math.Min(Vars(globalData).BuildingsDestroyed, 2);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, destroyed));
        globalData.ActiveWindow.AddClickHereToContinue(PeaceDiscard);
    }

    private static void PeaceDiscard(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(PeaceDiscard_0);
    }

    // Originally $wlimit (4 / 5)
    private static void PeaceDiscard_0(GlobalData globalData)
    {
        int limit = Vars(globalData).RandomElement([4, 5], _RND_WEAPON_LIMIT);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_WeaponToken, PopUpButton.Confirm, PeaceEarlySetup,
            content => content.FormatWithReplacement(0, limit.ToString()));
    }

    #endregion Intro

    #region Setup

    // Originally the setupStyle block of Peace1 (guarded by $pea1; the tracker change ran once, guarded by $Peace1)
    private static void PeaceEarlySetup(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.Tracker += globalData.PlayersNum == 4 ? 2 : 1;

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMob_Icon, PopUpButton.Confirm, Peace,
            content => content
                      .FormatWithCondition(0, () => vars.BuildingsDestroyed >= 1)
                      .FormatWithReplacement(1, vars.Tracker.ToString())
                      .FormatWithCondition(2, () => globalData.PlayersNum == 3));
    }

    // Originally the setupStyle blocks of Peace2 / Peace3 (guarded by $pea2 / $pea3), after the battle of the previous round
    private static void PeaceRoundSetup(GlobalData globalData)
    {
        int income = WeaponIncome(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_WeaponToken, PopUpButton.Confirm, Peace,
            content => content.FormatWithCondition(0, () => income == 1).FormatWithCondition(1, () => Vars(globalData).BuildingsDestroyed >= 2));
    }

    #endregion Setup

    #region Hub events

    // Originally HowtoWar (the $pea1-3 writes only kept the hub setup popups from reappearing)
    private static void HowtoWar(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithCondition(0, () => globalData.PlayersNum <= 3)
                                                             .FormatWithIndex(1, globalData.PlayersNum - 2)
                                                             .FormatWithCondition(2, () => Vars(globalData).Gas == ExtendedBool.True));
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("ClickToReturn"), Peace);
    }

    #endregion Hub events

    #region Famine (end of the Early Years)

    // Originally ATOW-FamineEvent. Runs after the Early end-of-round popup (Years == Middle).
    private static void FamineEvent(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(globalData.PlayersNum == 2 ? FamineBid2P : FamineVote);
    }

    // Originally ATOW-FamineEventV
    private static void FamineVote(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereAfterVote(FamineRes);
    }

    // Originally ATOW-FamineEventRes
    private static void FamineRes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [FamineYes, FamineNo], true);
    }

    // Originally 2PFamineBid
    private static void FamineBid2P(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereAfterBid(FamineBidRes2P_0);
    }

    // Originally the setupStyle block at the top of 2pFamineBidRes: shown before the choice
    private static void FamineBidRes2P_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, FamineBidRes2P);
    }

    // Originally 2pFamineBidRes
    private static void FamineBidRes2P(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [FamineYes, FamineNo], true);
    }

    // Originally ATOW-FamineEventYes: the name of the new meat product (Unity input popup), then the passage
    private static void FamineYes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveInputPopup = new GameplayInputPopup(globalData, string.Empty, PopUpButton.Confirm, value => value.Trim().Length is > 0 and <= 40, value =>
        {
            ATimeOfWarVars vars = Vars(globalData);
            vars.NewMeat = value.Trim();
            vars.Meat    = true;
            FamineYesResult(globalData);
        }, false, content => content.FormatWithCondition(0, () => globalData.PlayersNum == 2));
    }

    private static void FamineYesResult(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Vars(globalData).NewMeat));
        globalData.ActiveWindow.AddClickHereToContinue(FamineYesResult_0);
    }

    private static void FamineYesResult_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Insanity_Icon, PopUpButton.Confirm, PeaceStickCheck);
    }

    // Originally ATOW-FamineEventNo
    private static void FamineNo(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(FamineNo_0);
    }

    private static void FamineNo_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMob_Icon, PopUpButton.Confirm, PeaceStickCheck);
    }

    // The Wooden Figures of Generation I (originally $crazy)
    private static void PeaceStickCheck(GlobalData globalData)
    {
        if (Vars(globalData).WoodenFigures >= 1) PeaceStick(globalData);
        else BattleTime(globalData);
    }

    private static void PeaceStick(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(PeaceStick_0);
    }

    private static void PeaceStick_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_WeaponToken, PopUpButton.Confirm, BattleTime,
            content => content.FormatWithCondition(0, () => Vars(globalData).WoodenFigures == 1));
    }

    #endregion Famine (end of the Early Years)

    #region Battles

    // The battle at the end of the Early and the Middle Years (Years is already the next round). One of three flavor
    // paragraphs; the first names a random player.
    private static void BattleTime(GlobalData globalData)
    {
        ATimeOfWarVars vars      = Vars(globalData);
        string         key       = "BattleTime#" + (int)globalData.Years;
        int            paragraph = vars.Flavor(key, 3);
        string         player    = vars.FlavorElement(globalData.GetActivePlayers().ToList(), key + "#Player");

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddNextContent(paragraph + 1, false, content => content.FormatWithReplacement(0, player));
        globalData.ActiveWindow.AddNextContent(4, true);
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("BattleTime_Begin"), BattleCompleteReturn);
    }

    private static void BattleCompleteReturn(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(title => title.FormatWithIndex(0, vars.Flavor("BattleCompleteReturn#" + (int)globalData.Years, 2)));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("BattleCompleteReturn_Next"), PeaceRoundSetup);
    }

    #endregion Battles

    #region Gas (end of the Middle Years)

    // Runs after the Middle end-of-round popup (Years == Late)
    private static void PeaceEvent(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(PeaceEventb);
    }

    // Thumbs vote: a plain link in the source (no voting screen)
    private static void PeaceEventb(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("PeaceEventb_Start"), PeaceEventc);
    }

    private static void PeaceEventc(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [PeaceEventGas, PeaceEventNo], true);
    }

    // The gas is released (originally $peac = 2 and $peacecount + 1)
    private static void PeaceEventGas(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Gas = ExtendedBool.True;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(PeaceEventGas_0);
    }

    private static void PeaceEventGas_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, BattleTime);
    }

    // Originally $peac = 1
    private static void PeaceEventNo(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Gas = ExtendedBool.False;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(PeaceEventNo_0);
    }

    private static void PeaceEventNo_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_ScenarioIcon, PopUpButton.Confirm, BattleTime);
    }

    #endregion Gas (end of the Middle Years)

    #region End of Generation III

    // Runs after the Late end-of-round popup (Generation == 3)
    private static void FinalBattleTime(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(FinalBattleTime_0);
    }

    // Without the gas the Battle card stays on its side (originally $peac == 1; a game without the event would flip it)
    private static void FinalBattleTime_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_WorldConquest, PopUpButton.Confirm, FinalBattleComplete,
            content => content.FormatWithCondition(0, () => Vars(globalData).Gas == ExtendedBool.False));
    }

    // Ending (port map 8): the new meat **and** the gas (originally $peacecount == 2) -> 7 "History of Applied Sciences",
    // else 8 "Local Legend"
    private static void FinalBattleComplete(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.Ending = vars.Meat && vars.Gas == ExtendedBool.True ? 7 : 8;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => vars.Gas == ExtendedBool.False));
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("FinalBattleComplete_Continue"), Scoring);
    }

    #endregion End of Generation III
}
