namespace MyFathersWorkApp;

// Generation II "Martial" (port map 2.2): intros, hub, outpost, weapons bonus, Barracks penalties, quartering, bribe,
// afternoon tea and the end of the Generation. Sabotage, messengers and duels: 03_ATimeOfWar_Gen2_Sabotage.cs.
public static partial class ATimeOfWar
{
    // Target of the afternoon tea conspiracy (originally $teaopt)
    private const int _TEA_BARRACKS = 0;
    private const int _TEA_OUTPOST  = 1;

    public static void Martial(GlobalData globalData)
    {
        // Round 1 -> Martial1
        // Round 2 -> Martial2
        // Round 3 -> Martial3
        ATimeOfWarVars vars  = Vars(globalData);
        Years          years = MartialYears(globalData);

        vars.HubId           = ATimeOfWarHubId.Martial;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(years);

        bool monarchists    = vars.WarWinner == WarFaction.Monarchists;
        bool barracksStands = vars.SabotagedBuilding != MartialBuilding.Barracks;

        const string       turnTheTideSection = "TurnTheTide";
        GameplayHubSection turnTheTide        = globalData.ActiveHub.AddSection(turnTheTideSection, true);
        turnTheTide.AddDefaultContent(turnTheTideSection, content => content
                                                                    .FormatWithReplacement(0, Loser(globalData))
                                                                    .FormatWithCondition(1, () => LoserFaction(globalData) == WarFaction.Separatists)
                                                                    .FormatWithCondition(2, () => years == Years.Early));

        const string       weaponsBonusSection = "WeaponsBonus";
        GameplayHubSection weaponsBonus        = globalData.ActiveHub.AddSection(weaponsBonusSection);
        weaponsBonus.ReplaceShouldShow(() => !vars.WeaponsBonusClaimed);
        weaponsBonus.AddDefaultContent(weaponsBonusSection);
        weaponsBonus.AddSpecialClickHere(weaponsBonusSection, PackingHeat1);

        // Unified Monarchists: penalty each round while the Barracks stands (the Early section is shown regardless, as
        // in the source). The Late bonus (originally $moninc3) is drawn only when the section is shown.
        bool               demandsShown    = monarchists && (years == Years.Early || barracksStands);
        int                monarchistBonus = demandsShown && years == Years.Late ? MonarchistBonus(globalData) : 0;
        const string       demandsSection  = "MilitaryDemands";
        GameplayHubSection demands         = globalData.ActiveHub.AddSection(demandsSection);
        demands.ReplaceShouldShow(() => demandsShown);
        demands.FormatTitle(title => title.FormatWithCondition(0, () => monarchistBonus == 1));
        demands.AddDefaultContent(demandsSection, content => content.FormatWithIndex(0, monarchistBonus));

        // Separatists: a reward each round while the Barracks stands (originally $sepinc1 / 2 / 3, drawn when shown)
        bool               incentiveShown   = !monarchists && (years == Years.Early || barracksStands);
        int                incentive        = incentiveShown ? (int)years * 2 + SeparatistIncentive(globalData, years) : 0;
        const string       incentiveSection = "MilitaryIncentive";
        GameplayHubSection militaryIncentive = globalData.ActiveHub.AddSection(incentiveSection);
        militaryIncentive.ReplaceShouldShow(() => incentiveShown);
        militaryIncentive.FormatTitle(title => title.FormatWithCondition(0, () => years == Years.Middle));
        militaryIncentive.AddDefaultContent(incentiveSection, content => content.FormatWithIndex(0, incentive));

        const string       everythingSection = "EverythingIsAWeapon";
        GameplayHubSection everything        = globalData.ActiveHub.AddSection(everythingSection);
        everything.ReplaceShouldShow(() => years == Years.Late && vars.Bribe == ExtendedBool.True);
        everything.AddDefaultContent(everythingSection);

        // The Early / Middle end-of-round link is part of the Outpost section, as in the source
        const string       outpostSection    = "Outpost";
        const string       outpostEndSection = "OutpostEnd";
        GameplayHubSection outpost           = globalData.ActiveHub.AddSection(outpostSection);
        outpost.ReplaceShouldShow(() => years != Years.Late || vars.BuildingsDestroyed < 2);
        outpost.AddDefaultContent(outpostSection);
        outpost.AddSpecialClickHere(outpostSection, OutpostExplain);
        if (years != Years.Late) outpost.AddSpecialClickHere(outpostEndSection, Martial_0, true, endOfRound: true);

        const string       endLateSection = "EndLate";
        GameplayHubSection endLate        = globalData.ActiveHub.AddSection(endLateSection);
        endLate.ReplaceShouldShow(() => years == Years.Late);
        endLate.AddSpecialClickHere(endLateSection, Martial_0, endOfRound: true);
    }

    // Early / Middle: the events are played first and the End of Round only afterwards (port map 1.2)
    private static void Martial_0(GlobalData globalData)
    {
        if (MartialYears(globalData) == Years.Late) globalData.ShowEndOfRoundPopUp(BarracksPenaltyCheck);
        else MartialEndOfRound_0(globalData);
    }

    // Round of Generation II. After the Late end-of-round popup Generation is already Third: the Late Years.
    private static Years MartialYears(GlobalData globalData)
    {
        return globalData.Generation == Generation.Second ? globalData.Years : Years.Late;
    }

    // Originally $moninc3: 0 "no bonus.", 1 "$1 for your efforts."
    private static int MonarchistBonus(GlobalData globalData)
    {
        return Vars(globalData).RandomElement([0, 1], _RND_MON_INC3);
    }

    // Originally $sepinc1 / $sepinc2 / $sepinc3: one of two rewards per round
    private static int SeparatistIncentive(GlobalData globalData, Years years)
    {
        return Vars(globalData).RandomElement([0, 1], _RND_SEP_INC1 + (int)years);
    }

    // Originally $wardetermine: buildings that must be destroyed for the Gen I losers to win the war
    private static int WarDetermine(GlobalData globalData)
    {
        return Vars(globalData).RandomElement([1, 2], _RND_WAR_DETERMINE);
    }

    #region Intro

    // Generation I winner: Unified Monarchists
    private static void IntroMartialMonarchists(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Assessment = false;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("IntroMartial-Monarchists");
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_II);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(GUNSAward);
    }

    // Generation I winner: Separatists. The source draws $sepevent here; it is drawn on MartialSepEventYes_0.
    private static void IntroMartialSeparatists(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Assessment = false;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("IntroMartial-Separatists");
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_II);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(GUNSAward);
    }

    private static void GUNSAward(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.SabotagedBuilding  = MartialBuilding.None;
        vars.BuildingsDestroyed = 0;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(GUNSAward_0);
    }

    private static void GUNSAward_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_WeaponToken, PopUpButton.Confirm, AdvancedWeaponryIntro,
            content => content.FormatWithReplacement(0, Winner(globalData)).FormatWithReplacement(1, Loser(globalData)));
    }

    // The source draws $sepinc1-3, $moninc3, $wardetermine and $defense here; each is drawn on the screen that shows it
    private static void AdvancedWeaponryIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(AdvancedWeaponryIntro_0);
    }

    private static void AdvancedWeaponryIntro_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_AdvancedWeaponry, PopUpButton.Confirm, MartialEarlySetup);
    }

    #endregion Intro

    #region Setup

    // Originally the setupStyle block of Martial1 (the tracker change ran once, guarded by $Martial1)
    private static void MartialEarlySetup(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        if (vars.WarWinner == WarFaction.Monarchists) vars.Tracker += 1;
        else vars.Tracker -= vars.RandomElement([0, 1], _RND_MARTIAL_TRACKER);

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMob_Icon, PopUpButton.Confirm, Martial,
            content => content.FormatWithReplacement(0, vars.Tracker.ToString()).FormatWithCondition(1, () => globalData.PlayersNum == 3));
    }

    // Originally the setupStyle block of Martial2: a building was destroyed in the Early Years
    private static void MartialMiddleSetup(GlobalData globalData)
    {
        if (Vars(globalData).BuildingsDestroyed == 1) MartialChroniclePage(globalData);
        else Martial(globalData);
    }

    // Originally the setupStyle block of Martial3: a building was destroyed in the Middle Years (originally $warpage2)
    private static void MartialLateSetup(GlobalData globalData)
    {
        if (Vars(globalData).ChroniclePageMiddle) MartialChroniclePage(globalData);
        else Martial(globalData);
    }

    private static void MartialChroniclePage(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, Martial,
            content => content.FormatWithReplacement(0, Loser(globalData)).FormatWithReplacement(1, Vars(globalData).ChroniclePage.ToString()));
    }

    #endregion Setup

    #region Hub events

    private static void OutpostExplain(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("ClickToReturn"), Martial);
    }

    // Weapons Bonus: the first player to complete 2 Advanced Weaponry Experiments
    private static void PackingHeat1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(player =>
        {
            Vars(globalData).SelectedPlayer = player;
            PackingHeat1a(globalData);
        });
    }

    private static void PackingHeat1a(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Winner(globalData)).FormatWithReplacement(1, Loser(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(PackingHeat1a_0);
    }

    private static void PackingHeat1a_0(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars   = Vars(globalData);
        int            reward = vars.RandomElement([0, 1], _RND_HEAT_REWARD);
        vars.WeaponsBonusClaimed = true;

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, Martial,
            content => content
                      .FormatWithReplacement(0, vars.SelectedPlayer)
                      .FormatWithIndex(1, reward)
                      .FormatWithReplacement(2, Loser(globalData)));
    }

    #endregion Hub events

    #region End of round events

    // Unity S_OnSetSpecialSetup("Special Setup", ...) of Martial1 / Martial2: the events come before the End of Round
    private static void MartialEndOfRound_0(GlobalData globalData)
    {
        globalData.SaveToUndo();

        // Source: $sabotagee1 = "none" on entering Martial2 / Martial3 and in ATOWSabotageIntro1
        ATimeOfWarVars vars = Vars(globalData);
        vars.SabotageTarget = string.Empty;
        vars.Saboteur       = string.Empty;
        vars.DuelType       = 0;
        vars.DuelLoser      = string.Empty;

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.MFWlogo, PopUpButton.Confirm, MartialEvents);
    }

    private static void MartialEvents(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        if (globalData.Years == Years.Early) ATOWSabotageIntro1(globalData);
        else if (vars.WarWinner == WarFaction.Monarchists)
        {
            if (vars.MetGentleman) AMessenger2(globalData);
            else AfternoonTea(globalData);
        }
        else if (vars.BuildingsDestroyed == 1) ATOWSabotageIntro2Sep(globalData);
        else ATOWSabotageIntro2(globalData);
    }

    // Originally Martia1Pre2 / MartialPre3: the End of Round once the events are done
    private static void MartialRoundEnd(GlobalData globalData)
    {
        globalData.ShowEndOfRoundPopUp(globalData.Years == Years.Early ? MartialMiddleSetup : MartialLateSetup);
    }

    // Originally BarracksSimple1 / 2 / 3: Unified Monarchists only, while the Barracks stands
    private static void BarracksPenaltyCheck(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        if (vars.WarWinner == WarFaction.Monarchists && vars.SabotagedBuilding != MartialBuilding.Barracks) BarracksPenalty(globalData);
        else BarracksPenaltyNext(globalData);
    }

    private static void BarracksPenaltyNext(GlobalData globalData)
    {
        if (MartialYears(globalData) == Years.Late) ResistSides(globalData);
        else SabotageCheck(globalData);
    }

    private static void BarracksPenalty(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(BarracksPenalty_0);
    }

    // Penalty 1: gains 1-2 Creepy, 2: $1 less income, 3: loses 2-3VP, 4: flips the Flag and loses 1VP, 5: none
    private static void BarracksPenalty_0(GlobalData globalData)
    {
        ATimeOfWarVars vars    = Vars(globalData);
        int            round   = (int)MartialYears(globalData);
        int            penalty = vars.RandomElement([1, 2, 3, 4, 5], _RND_BARRACKS_PENALTY + round);
        int amount = penalty switch
        {
            1 => vars.RandomElement([1, 2], _RND_BARRACKS_AMOUNT + round),
            3 => vars.RandomElement([2, 3], _RND_BARRACKS_AMOUNT + round),
            _ => 0
        };

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.BuildingS3Barracks, PopUpButton.Confirm, BarracksPenaltyNext,
            content => content.FormatWithReplacement(1, amount.ToString()).FormatWithIndex(0, penalty - 1));
    }

    #endregion End of round events

    #region Quartering and bribe (Separatists, Middle Years)

    private static void ResistEvent(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Winner(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(ResistEvent_0);
    }

    // The next screen depends on $quarter, drawn (if still undrawn) when the popup is closed
    private static void ResistEvent_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Money_Icon, PopUpButton.Confirm, data =>
        {
            if (Quarter(data)) ResistEvent1(data);
            else ResistEvent2(data);
        });
    }

    private static void ResistEvent1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ResistEvent1_0);
    }

    private static void ResistEvent1_0(GlobalData globalData)
    {
        int vp = Vars(globalData).RandomInRange(1, 3, _RND_QUARTER_VP);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_EstateUpgradeBack, PopUpButton.Confirm, QuarteringNext,
            content => content.FormatWithReplacement(0, vp.ToString()));
    }

    private static void ResistEvent2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ResistEvent2_0);
    }

    private static void ResistEvent2_0(GlobalData globalData)
    {
        int vp = Vars(globalData).RandomInRange(1, 4, _RND_QUARTER_VP);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, QuarteringNext,
            content => content.FormatWithReplacement(0, vp.ToString()));
    }

    // Only the Separatists reach the quartering; the source's Unified Monarchists branch (BarracksSimple2) is kept
    private static void QuarteringNext(GlobalData globalData)
    {
        if (Vars(globalData).WarWinner == WarFaction.Separatists) MartialSepEvent(globalData);
        else BarracksPenaltyCheck(globalData);
    }

    private static void MartialSepEvent(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(MartialSepEventa);
    }

    // Originally $milit: 2p 1-3, 3p 2-6, 4p 3-6
    private static int BribeTarget(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        return globalData.PlayersNum switch
        {
            2 => vars.RandomInRange(1, 3, _RND_BRIBE_TARGET),
            3 => vars.RandomInRange(2, 6, _RND_BRIBE_TARGET),
            _ => vars.RandomInRange(3, 6, _RND_BRIBE_TARGET) // 4
        };
    }

    private static void MartialSepEventa(GlobalData globalData)
    {
        int target = BribeTarget(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, target.ToString()));
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("MartialSepEventa_Start"), MartialSepEventb);
    }

    private static void MartialSepEventb(GlobalData globalData)
    {
        int target = BribeTarget(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, target.ToString()));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [
            data =>
            {
                Vars(data).Bribe = ExtendedBool.True;
                MartialSepEventYes(data);
            },
            data =>
            {
                Vars(data).Bribe = ExtendedBool.False;
                MartialSepEventNo(data);
            }
        ], true);
    }

    private static void MartialSepEventYes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(MartialSepEventYes_0);
    }

    // Originally $sepevent (drawn in IntroMartial-Separatists): 2 = the biggest contributor also gains 3VP
    private static void MartialSepEventYes_0(GlobalData globalData)
    {
        bool bonus = Vars(globalData).RandomElement([1, 2], _RND_SEP_EVENT) == 2;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, SabotageCheck,
            content => content.FormatWithCondition(0, () => bonus));
    }

    private static void MartialSepEventNo(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(MartialSepEventNo_0);
    }

    private static void MartialSepEventNo_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.CompulsionBack, PopUpButton.Confirm, SabotageCheck);
    }

    #endregion Quartering and bribe (Separatists, Middle Years)

    #region Afternoon tea (Unified Monarchists, Middle Years)

    // Originally $teaopt: the Barracks while nothing was destroyed, otherwise the Outpost
    private static int TeaTarget(GlobalData globalData)
    {
        return Vars(globalData).SabotagedBuilding == MartialBuilding.None ? _TEA_BARRACKS : _TEA_OUTPOST;
    }

    private static string TeaTargetName(GlobalData globalData, int target)
    {
        return globalData.GetScenarioLocalizedTag("TeaTarget_Name").FormatWithIndex(0, target);
    }

    private static void AfternoonTea(GlobalData globalData)
    {
        string target = TeaTargetName(globalData, TeaTarget(globalData));

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, target));
        globalData.ActiveWindow.AddClickHereToContinue(globalData.PlayersNum == 2 ? AfternoonTeaBid2P : AfternoonTeaVote);
    }

    // Originally AfternoonTeab
    private static void AfternoonTeaVote(GlobalData globalData)
    {
        string target = TeaTargetName(globalData, TeaTarget(globalData));

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, target));
        globalData.ActiveWindow.AddClickHereAfterVote(AfternoonTeaRes);
    }

    private static void AfternoonTeaRes(GlobalData globalData)
    {
        string target = TeaTargetName(globalData, TeaTarget(globalData));

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, target));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [AfternoonTeaYay, AfternoonTeaNay], true, content => content.FormatWithReplacement(0, target));
    }

    // Originally 2PAfternoonBid
    private static void AfternoonTeaBid2P(GlobalData globalData)
    {
        string target = TeaTargetName(globalData, TeaTarget(globalData));

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, target));
        globalData.ActiveWindow.AddClickHereAfterBid(AfternoonTeaBidRes2P);
    }

    // Originally 2PAfternoonBidRes
    private static void AfternoonTeaBidRes2P(GlobalData globalData)
    {
        string target = TeaTargetName(globalData, TeaTarget(globalData));

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [AfternoonTeaYay, AfternoonTeaNay], true, content => content.FormatWithReplacement(0, target));
    }

    // Originally AfternoonTeaResolutionYAY. Source bugs fixed (port map 7): B5 page 7 when the Laborer's Union fell
    // before the Outpost, B6 the destroyed Barracks counts as the sabotaged building (no more Barracks penalty / section),
    // B7 the Late setup shows the Village Chronicle page. B8 (no Middle Barracks penalty after the tea) as shipped.
    private static void AfternoonTeaYay(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars   = Vars(globalData);
        int            target = TeaTarget(globalData);

        if (target == _TEA_OUTPOST)
        {
            vars.BuildingsDestroyed = 2;
            vars.OutpostDestroyed   = true;
            vars.ChroniclePage      = vars.SabotagedBuilding == MartialBuilding.LaborersUnion ? 7 : 8;
        }
        else
        {
            vars.BuildingsDestroyed = 1;
            vars.SabotagedBuilding  = MartialBuilding.Barracks;
            vars.ChroniclePage      = 5;
        }

        vars.ChroniclePageMiddle = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, TeaTargetName(globalData, target))
                                                             .FormatWithCondition(1, () => target == _TEA_BARRACKS));
        globalData.ActiveWindow.AddClickHereToContinue(AfternoonTeaYay_0);
    }

    private static void AfternoonTeaYay_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMob_Icon, PopUpButton.Confirm, MartialRoundEnd,
            content => content.FormatWithCondition(0, () => globalData.PlayersNum == 4).FormatWithCondition(1, () => globalData.PlayersNum > 2));
    }

    // Originally AfternoonTeaResolutionNAY
    private static void AfternoonTeaNay(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddNextContent(globalData.PlayersNum > 2 ? 1 : 2);
        globalData.ActiveWindow.AddClickHereToContinue(AfternoonTeaNay_0);
    }

    private static void AfternoonTeaNay_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_AdvancedWeaponry, PopUpButton.Confirm, BarracksPenaltyCheck,
            content => content.FormatWithCondition(0, () => globalData.PlayersNum > 2));
    }

    #endregion Afternoon tea (Unified Monarchists, Middle Years)

    #region End of Generation II

    private static void ResistSides(GlobalData globalData)
    {
        int experiments = Vars(globalData).RandomElement([0, 1], _RND_RESIST_FLAGS);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, experiments));

        // Source: once-per-game $ResistSidesnextPsg, drawn in the click (mirror mode); the assessment letter forces ResistEnd
        globalData.ActiveWindow.AddClickHereToContinue(data =>
        {
            if (Vars(data).Assessment) ResistEnd(data);
            else Vars(data).RandomElement([ResistEnd, ResistEnd2, ResistEnd3], _RND_RESIST_END).Invoke(data);
        });
    }

    // Enough buildings destroyed (originally $wardestroy >= $wardetermine): the Generation I losers win the war
    private static void ResistEnd(GlobalData globalData)
    {
        ATimeOfWarVars vars    = Vars(globalData);
        bool           victory = vars.BuildingsDestroyed >= WarDetermine(globalData);
        WarFaction     winner  = victory ? LoserFaction(globalData) : vars.WarWinner;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(title => title.FormatWithCondition(0, () => victory));
        globalData.ActiveWindow.AddNextContent(victory ? 1 : 2, false, content => content
                                                                                 .FormatWithReplacement(0, vars.BuildingsDestroyed.ToString())
                                                                                 .FormatWithReplacement(1, Loser(globalData))
                                                                                 .FormatWithReplacement(2, Winner(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(MartialEnd(winner));
    }

    // Face-up Flag tokens decide (players answer)
    private static void ResistEnd2(GlobalData globalData)
    {
        WarFaction winner = Vars(globalData).WarWinner;
        WarFaction loser  = LoserFaction(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Winner(globalData)).FormatWithReplacement(1, Loser(globalData)));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [MartialEnd(winner), MartialEnd(loser)], true, content => content
                                                                                                                 .FormatWithReplacement(0, Winner(globalData))
                                                                                                                 .FormatWithReplacement(1, Loser(globalData)));
    }

    // Completed Advanced Weaponry Experiments against the town's defenses (originally $defense: 2p 2-4, 3p 3-5, 4p 4-6)
    private static void ResistEnd3(GlobalData globalData)
    {
        int        defense = Vars(globalData).RandomInRange(globalData.PlayersNum, globalData.PlayersNum + 2, _RND_DEFENSE);
        WarFaction winner  = Vars(globalData).WarWinner;
        WarFaction loser   = LoserFaction(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, defense.ToString())
                                                             .FormatWithReplacement(1, Loser(globalData))
                                                             .FormatWithReplacement(2, Winner(globalData)));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [MartialEnd(loser), MartialEnd(winner)], true, content => content
                                                                                                                 .FormatWithReplacement(0, Loser(globalData))
                                                                                                                 .FormatWithReplacement(1, Winner(globalData)));
    }

    // The faction that won the war decides Generation III: Unified Monarchists -> Heavy is the Crown, Separatists ->
    // Peace Through War
    private static Action<GlobalData> MartialEnd(WarFaction warWinner)
    {
        return warWinner == WarFaction.Separatists ? MartialEndPeace : MartialEndReign;
    }

    // End of Generation text of ResistEnd / ResistEnd2 / ResistEnd3 (Unity S_OnSetSpecialSetup("End of Generation", ...),
    // shown after the click)
    private static void MartialEndReign(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.EndOfGeneration, PopUpIcon.MFWlogo, PopUpButton.Confirm, MonarchReignIntro,
            "MartialGenerationEnd_Content");
    }

    private static void MartialEndPeace(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.EndOfGeneration, PopUpIcon.MFWlogo, PopUpButton.Confirm, PeaceIntro,
            "MartialGenerationEnd_Content");
    }

    #endregion End of Generation II
}
