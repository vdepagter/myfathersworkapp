namespace MyFathersWorkApp;

// Generation II "Time Travel" / "A Return to Devastation" (port map 2.3): intro, Paradox tokens, hub, Time Machine
// repairs, the military token, the Barracks vote in the past, the Blame game and the end of the Generation.
public static partial class ATimeOfWar
{
    public static void TimeTravel(GlobalData globalData)
    {
        // Round 1 -> TimeTravel
        // Round 2 -> TimeTravel2
        // Round 3 -> TimeTravel3
        ATimeOfWarVars vars  = Vars(globalData);
        Years          years = TimeTravelYears(globalData);

        vars.HubId           = ATimeOfWarHubId.TimeTravel;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(years);

        // The Barracks was not built in the original timeline, so it is built now (TownHallOpposite)
        const string       militarySection = "MilitaryDemands";
        GameplayHubSection military        = globalData.ActiveHub.AddSection(militarySection);
        military.ReplaceShouldShow(() => years == Years.Late && vars.Barracks == ExtendedBool.False);
        military.AddDefaultContent(militarySection);

        const string       repairSection = "Repair";
        GameplayHubSection repair        = globalData.ActiveHub.AddSection(repairSection);
        repair.ReplaceShouldShow(() => years != Years.Early && !vars.TimeMachineRepaired);
        repair.AddDefaultContent(repairSection);
        repair.AddSpecialClickHere(repairSection, ATimeMachineRepaired);

        const string       paradoxSection = "ParadoxExplanation";
        GameplayHubSection paradox        = globalData.ActiveHub.AddSection(paradoxSection, true);
        paradox.AddDefaultContent(paradoxSection);
        paradox.AddSpecialClickHere(paradoxSection, ParadoxExReminder);

        const string       mendingSection = "Mending";
        GameplayHubSection mending        = globalData.ActiveHub.AddSection(mendingSection);
        mending.ReplaceShouldShow(() => !vars.MendingClaimed);
        mending.AddDefaultContent(mendingSection);
        mending.AddSpecialClickHere(mendingSection, TriggerParadox);
        mending.AddNextContent(1, mendingSection);

        const string       endSection = "End";
        GameplayHubSection end        = globalData.ActiveHub.AddSection(endSection);
        end.ReplaceShouldShow(() => years != Years.Late);
        end.AddSpecialClickHere(endSection, TimeTravel_0, endOfRound: true);

        const string       endLateSection = "EndLate";
        GameplayHubSection endLate        = globalData.ActiveHub.AddSection(endLateSection);
        endLate.ReplaceShouldShow(() => years == Years.Late);
        endLate.AddSpecialClickHere(endLateSection, TimeTravel_0, endOfRound: true);
    }

    private static void TimeTravel_0(GlobalData globalData)
    {
        globalData.ShowEndOfRoundPopUp(TimeTravelYears(globalData) switch
        {
            Years.Early  => FixIt,
            Years.Middle => TownHallOpposite,
            _            => Vars(globalData).AllSilent ? BlameOut3 : SecretParadoxBonus
        });
    }

    // Round of Generation II. After the Late end-of-round popup Generation is already Third: the Late Years.
    private static Years TimeTravelYears(GlobalData globalData)
    {
        return globalData.Generation == Generation.Second ? globalData.Years : Years.Late;
    }

    #region Intro

    // Originally TimeTravelintro (after TowardsDev)
    private static void TimeTravelIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TimeTravelIntro_0);
    }

    private static void TimeTravelIntro_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, TimeTravelIntro1);
    }

    // Originally TimeTravelintro1. The $takereact faction swap is dead in the source (port map 7, B2). $late = 0 was set
    // on every render of the Early hub; it is set here once.
    private static void TimeTravelIntro1(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.TimeMachineRepaired = false;
        vars.MendingClaimed      = false;
        vars.FlagsBought         = 0;
        vars.AllSilent           = false;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("TimeTravelintro1");
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_II);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TimeTravelIntro2);
    }

    private static void TimeTravelIntro2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("TimeTravelIntro2");
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, globalData.LocalizedPlayerNumberIndex()));
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("TimeTravelIntro2_Continue"), TimeMachineExperiment);
    }

    private static void TimeMachineExperiment(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [
            data =>
            {
                Vars(data).TimeMachineMasterwork = true;
                TMReplace(data);
            },
            TTIntro3
        ], true);
    }

    private static void TMReplace(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TMReplace_0);
    }

    private static void TMReplace_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_MWUpgradeTimeMaching, PopUpButton.Confirm, TTIntro3);
    }

    private static void TTIntro3(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("TTIntro3");
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TimeTokens);
    }

    private static void TimeTokens(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TimeTokens_0);
    }

    private static void TimeTokens_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_ParadoxToken, PopUpButton.Confirm, TimeTravelEarlySetup);
    }

    #endregion Intro

    #region Setup

    // Originally the setupStyle block of TimeTravel (guarded by $pg13; shown once at the start of the round)
    private static void TimeTravelEarlySetup(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, TimeTravel,
            content => content.FormatWithCondition(0, () => globalData.PlayersNum == 3));
    }

    // Originally the setupStyle block of TimeTravel3 (guarded by $pg14): the Barracks is built in the new timeline
    private static void TimeTravelLateSetup(GlobalData globalData)
    {
        if (Vars(globalData).Barracks == ExtendedBool.False) TimeTravelLateSetup_0(globalData);
        else TimeTravel(globalData);
    }

    private static void TimeTravelLateSetup_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, TimeTravel);
    }

    #endregion Setup

    #region Hub events

    private static void ParadoxExReminder(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TimeTravel);
    }

    // Paradoxical Mending Reward: the first player with zero Paradox tokens
    private static void TriggerParadox(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TriggerParadox_0);
    }

    private static void TriggerParadox_0(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);
        int            vp   = vars.RandomElement([3, 4], _RND_MENDING_VP);

        globalData.SaveToUndo();
        vars.MendingClaimed = true;

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, TimeTravel,
            content => content.FormatWithReplacement(0, vp.ToString()));
    }

    // All three Time Machine Repairs completed
    private static void ATimeMachineRepaired(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).TimeMachineRepaired = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TimeTravel);
    }

    #endregion Hub events

    #region End of the Early Years

    // Runs after the Early end-of-round popup (Years == Middle)
    private static void FixIt(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(FixIt_0);
    }

    // A player met the guard in Generation I (originally $rumor1) and still holds the {crest} token
    private static void FixIt_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_TimeMachineRepairs, PopUpButton.Confirm,
            Vars(globalData).MetGuard ? Militarytoken : TimeTravel);
    }

    private static void Militarytoken(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Crest(globalData)));
        globalData.ActiveWindow.AddOnceYouAreReady(MilitarytokenRevealed);
    }

    private static void MilitarytokenRevealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [MilitaryYes, MilitaryNo], true);
    }

    private static void MilitaryNo(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(MilitaryNo_0);
    }

    private static void MilitaryNo_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_ParadoxToken, PopUpButton.Confirm, TimeTravel,
            content => content.FormatWithReplacement(0, Crest(globalData)));
    }

    private static void MilitaryYes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Crest(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(MilitaryYes_0);
    }

    private static void MilitaryYes_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_ParadoxToken, PopUpButton.Confirm, TimeTravel,
            content => content.FormatWithReplacement(0, Crest(globalData)));
    }

    #endregion End of the Early Years

    #region End of the Middle Years

    // Runs after the Middle end-of-round popup (Years == Late): the Barracks vote of Generation I is reversed
    private static void TownHallOpposite(GlobalData globalData)
    {
        bool barracks = Vars(globalData).Barracks == ExtendedBool.True;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => barracks));
        globalData.ActiveWindow.AddClickHereToContinue(SeedResolution);
    }

    // Showcase of Generation I (1 Knowledge, 2 Ingredients, else money - also without a showcase, as shipped: port map 7, B11)
    private static int SeedResolutionIndex(GlobalData globalData)
    {
        return Vars(globalData).Showcase switch
        {
            _SHOWCASE_KNOWLEDGE   => 1,
            _SHOWCASE_INGREDIENTS => 2,
            _                     => 3
        };
    }

    private static void SeedResolution(GlobalData globalData)
    {
        int index = SeedResolutionIndex(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(title => title.FormatWithIndex(0, index));
        globalData.ActiveWindow.AddNextContent(index, false, content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(SeedResolution_0);
    }

    // The source block has no SETUP header: shown as a Setup popup
    private static void SeedResolution_0(GlobalData globalData)
    {
        int index = SeedResolutionIndex(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.CompulsionBack, PopUpButton.Confirm, BlameGameEvent,
            content => content.FormatWithIndex(0, index));
    }

    #endregion End of the Middle Years

    #region Blame game

    private static void BlameGameEvent(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).BlameIndex = 0;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(globalData.PlayersNum == 2 ? BlameGameEvent2P : Blame);
    }

    // Originally 2PBlameGameEvent: the rules of the dilemma for two players
    private static void BlameGameEvent2P(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Blame);
    }

    // Player answering the interrogation (originally Blame1 ... Blame4, $counter)
    private static string BlamePlayer(GlobalData globalData)
    {
        return globalData.GetPlayerNameByIndex(Vars(globalData).BlameIndex);
    }

    // Originally Blame1 ... Blame4: one secret interrogation per player in turn order
    private static void Blame(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDoNotOtherPlayersToSee(BlamePlayer(globalData), BlameRevealed);
    }

    private static void BlameRevealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Winner(globalData)));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [Blame1c, Blame1b], true);
    }

    // The offer to buy a {warwinner} Flag token for $2
    private static void Blame1b(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Winner(globalData)));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [
            data =>
            {
                Vars(data).FlagsBought += 1;
                Blame1c(data);
            },
            Blame1c
        ], true);
    }

    // Stay silent or blame another player; then the next player, after the last one the resolution
    private static void Blame1c(GlobalData globalData)
    {
        string player = BlamePlayer(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Winner(globalData)));
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("Blame1c_Silent"), data =>
        {
            Vars(data).Silent[player] = true;
            BlameNext(data);
        });

        string blame = globalData.GetScenarioLocalizedTag("Blame1c_Blame");

        foreach (string other in globalData.GetActivePlayers().Where(other => other != player))
        {
            globalData.ActiveWindow.AddCustomLink(blame.FormatWithReplacement(0, other), data =>
            {
                Vars(data).BlameVotes[other] += 1;
                BlameNext(data);
            });
        }
    }

    private static void BlameNext(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);
        vars.BlameIndex += 1;

        if (vars.BlameIndex < globalData.PlayersNum) Blame(globalData);
        else BlameResolve(globalData);
    }

    // The player with more blame votes than every other player is blamed; nobody blamed at all -> everybody stayed
    // silent (originally $late == 2); otherwise a tie.
    private static void BlameResolve(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars    = Vars(globalData);
        string[]       players = globalData.GetActivePlayers();
        int            most    = players.Max(player => vars.BlameVotes[player]);
        string[]       blamed  = players.Where(player => vars.BlameVotes[player] == most).ToArray();

        vars.AllSilent = most == 0;
        vars.Blamed    = most > 0 && blamed.Length == 1 ? blamed[0] : string.Empty;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(vars.FlagsBought > 0 ? BlameResolve_0 : BlameResolveNext);
    }

    // Flag tokens bought during the interrogation (singular / plural text as in the source)
    private static void BlameResolve_0(GlobalData globalData)
    {
        int bought = Vars(globalData).FlagsBought;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_SeparatistToken, PopUpButton.Confirm, BlameResolveNext,
            content => content.FormatWithCondition(0, () => bought == 1).FormatWithReplacement(1, Winner(globalData)));
    }

    private static void BlameResolveNext(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        if (vars.Blamed != string.Empty) BlameOut1(globalData);
        else if (vars.AllSilent) TimeTravelLateSetup(globalData);
        else BlameOut2(globalData);
    }

    // One player blamed
    private static void BlameOut1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Vars(globalData).Blamed));
        globalData.ActiveWindow.AddClickHereToContinue(BlameOut1_0);
    }

    private static void BlameOut1_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.DiscardEstateUpgrade_Icon, PopUpButton.Confirm, TimeTravelLateSetup,
            content => content.FormatWithReplacement(0, Vars(globalData).Blamed));
    }

    // Tie: the silent players pay 1, all others 2
    private static void BlameOut2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(BlameOut2_0);
    }

    private static void BlameOut2_0(GlobalData globalData)
    {
        ATimeOfWarVars vars   = Vars(globalData);
        string         line   = globalData.GetScenarioLocalizedTag("BlameOut2_0_Silent");
        string         silent = string.Concat(globalData.GetActivePlayers().Where(player => vars.Silent[player]).Select(player => line.FormatWithReplacement(0, player)));

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Servant, PopUpButton.Confirm, TimeTravelLateSetup,
            content => silent == string.Empty ? content : silent + "<br />" + content);
    }

    #endregion Blame game

    #region End of Generation II

    // Runs after the Late end-of-round popup (Generation == Third): everybody stayed silent
    private static void BlameOut3(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("BlameOut3_Continue"), SecretParadoxBonus);
    }

    // The source draws $parasecret here and never reads it (port map 7, B13)
    private static void SecretParadoxBonus(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(SecretParadoxBonus_0);
    }

    private static void SecretParadoxBonus_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_ParadoxToken, PopUpButton.Confirm, TTBarracksPenaltyCheck);
    }

    // Originally TTBarracksPenalty: the penalty only when the Barracks was built in the new timeline
    private static void TTBarracksPenaltyCheck(GlobalData globalData)
    {
        if (Vars(globalData).Barracks == ExtendedBool.False) TTBarracksPenalty(globalData);
        else TimeTravelGenerationEnd(globalData);
    }

    private static void TTBarracksPenalty(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TimeTravelGenerationEnd);
    }

    // End of Generation text of TimeEnd1 / TimeEnd2 (identical in both; Unity S_OnEndOfGeneration overlay, shown before
    // the passage). Which passage follows (originally the once-per-game $TTBarracksPenaltynextPsg) is drawn when the
    // popup is closed, so mirror mode asks once the official app shows it.
    private static void TimeTravelGenerationEnd(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.EndOfGeneration, PopUpIcon.MFWlogo, PopUpButton.Confirm,
            data => Vars(data).RandomElement([TimeEnd1, TimeEnd2], _RND_TIME_END).Invoke(data));
    }

    // Paradox tokens left on all Experiments against $dox (2p 1-3, 3p 2-4, 4p 3-5)
    private static void TimeEnd1(GlobalData globalData)
    {
        int dox = Vars(globalData).RandomInRange(globalData.PlayersNum - 1, globalData.PlayersNum + 1, _RND_DOX);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [WarningIntro, Paradoxical], true, content => content.FormatWithReplacement(0, dox.ToString()));
    }

    // Time Machine repaired -> You Cant Stop Progress, else Future Consequences
    private static void TimeEnd2(GlobalData globalData)
    {
        bool repaired = Vars(globalData).TimeMachineRepaired;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(title => title.FormatWithCondition(0, () => repaired));

        if (repaired)
        {
            globalData.ActiveWindow.AddNextContent(1);
            globalData.ActiveWindow.AddClickHereToContinue(Paradoxical);
            return;
        }

        globalData.ActiveWindow.AddNextContent(2);
        globalData.ActiveWindow.AddNextContentWithLinks(3, [
            data => TimeEnd2Repairs(data, 0),
            data => TimeEnd2Repairs(data, 1),
            data => TimeEnd2Repairs(data, 2)
        ], true);
    }

    // Originally $tmnumber ("zero pieces" / "one piece" / "two pieces")
    private static void TimeEnd2Repairs(GlobalData globalData, int repairs)
    {
        Vars(globalData).RepairsDone = repairs;
        TimeEnd2b(globalData);
    }

    private static void TimeEnd2b(GlobalData globalData)
    {
        int repairs = Vars(globalData).RepairsDone;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddNextContent(repairs == 0 ? 1 : 2, false, content => content.FormatWithIndex(0, repairs));
        globalData.ActiveWindow.AddClickHereToContinue(WarningIntro);
    }

    #endregion End of Generation II
}
