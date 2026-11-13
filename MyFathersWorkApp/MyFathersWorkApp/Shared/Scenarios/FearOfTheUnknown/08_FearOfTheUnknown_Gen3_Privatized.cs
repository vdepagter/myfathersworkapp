namespace MyFathersWorkApp;

public static partial class FearOfTheUnknown
{
    public static void Privatized(GlobalData globalData)
    {
        // Round 16 -> Privatized
        // Round 17 -> Privatized2
        // Round 18 -> Privatized3
        FearOfTheUnknownVars vars = Vars(globalData);

        vars.HubId           = FearOfTheUnknownHubId.Privatized;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(globalData.Years);

        const string       investmentSection = "Investment";
        GameplayHubSection investment        = globalData.ActiveHub.AddSection(investmentSection, true);
        investment.ReplaceShouldShow(() => globalData.Years == Years.Early);
        investment.AddDefaultContent(investmentSection);

        const string       jailSection = "Jail";
        GameplayHubSection jail        = globalData.ActiveHub.AddSection(jailSection, true);
        jail.ReplaceShouldShow(() => globalData.Years == Years.Early);
        jail.AddDefaultContent(jailSection);

        const string       huntSection = "Hunt";
        GameplayHubSection hunt        = globalData.ActiveHub.AddSection(huntSection);
        hunt.ReplaceShouldShow(() => globalData.Years == Years.Early && HuntActive(globalData));
        hunt.AddDefaultContent(huntSection);
        hunt.AddSpecialClickHere(huntSection, HuntExplanation);

        const string       everythingWrongSection = "EverythingWrong";
        GameplayHubSection everythingWrong        = globalData.ActiveHub.AddSection(everythingWrongSection, true);
        everythingWrong.ReplaceShouldShow(() => globalData.Years != Years.Early);
        everythingWrong.AddDefaultContent(everythingWrongSection);

        const string       wardenSection = "Warden";
        GameplayHubSection warden        = globalData.ActiveHub.AddSection(wardenSection, true);
        // Each player is rewarded only the first time they reach the end of the track
        warden.ReplaceShouldShow(() => globalData.Years != Years.Early && globalData.GetActivePlayers().Any(player => !vars.CreepReward[player]));
        warden.FormatTitle(title => title.FormatWithReplacement(0, Warden(globalData)));
        warden.AddDefaultContent(wardenSection, content => content.FormatWithReplacement(0, Warden(globalData)));
        warden.AddSpecialClickHere(wardenSection, HorrorOfExistence);

        const string       junkPalaceSection = "JunkPalace";
        GameplayHubSection junkPalace        = globalData.ActiveHub.AddSection(junkPalaceSection);
        junkPalace.ReplaceShouldShow(() => globalData.Years != Years.Early);

        if (vars.Jail == 1)
        {
            junkPalace.AddNextContent(1, junkPalaceSection, false, content => content.FormatWithReplacement(0, AsylumUpgradeName(globalData)));
        }
        else
        {
            junkPalace.AddDefaultContent(junkPalaceSection);
            junkPalace.AddSpecialClickHere(junkPalaceSection, PalaceOfScraps, false, content => content.FormatWithReplacement(0, Warden(globalData)));
        }

        const string       battledomeSection = "Battledome";
        GameplayHubSection battledome        = globalData.ActiveHub.AddSection(battledomeSection);
        battledome.ReplaceShouldShow(() => globalData.Years != Years.Early);
        battledome.AddDefaultContent(battledomeSection);
        battledome.AddSpecialClickHere(battledomeSection, BattleWarrior);

        const string       huntAbandonedSection = "HuntAbandoned";
        GameplayHubSection huntAbandoned        = globalData.ActiveHub.AddSection(huntAbandonedSection);
        huntAbandoned.ReplaceShouldShow(() => globalData.Years != Years.Early && HuntActive(globalData));
        huntAbandoned.AddDefaultContent(huntAbandonedSection);

        GameplayHubSection nextRound = globalData.ActiveHub.AddSection(string.Empty);
        nextRound.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        nextRound.AddClickHereContinueNextRound(Privatized_0);

        globalData.ActiveHub.AddEndOfGenerationSection(Privatized_0);
    }

    private static void Privatized_0(GlobalData globalData)
    {
        globalData.ShowEndOfRoundPopUp(globalData.Years switch
        {
            Years.Early  => HuntActive(globalData) ? Expedition : HostileTakeover,
            Years.Middle => CreatureIntervenes,
            Years.Late   => PenaltyForDefiance,
            _            => _ => { }
        });
    }

    // The Butcher or the Blacksmith spared in Generation I rules the ruined town
    private static string Warden(GlobalData globalData) => Tag(globalData, "Warden").FormatWithCondition(0, () => Vars(globalData).Victim == _VICTIM_BUTCHER);

    private static string JunkPenalty(GlobalData globalData) => Tag(globalData, "JunkPenalty").FormatWithCondition(0, () => Vars(globalData).RandomBool(_RND_JUNK_PENALTY));

    #region Intro

    private static void PrivatizedIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Path       = FearOfTheUnknownHubId.Privatized;
        Vars(globalData).Expedition = false;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("PrivatizedIntro");
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_III);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(AsylumDeteriorated);
    }

    // Originally PrivatetoJail
    private static void AsylumDeteriorated(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName).FormatWithCondition(1, () => Vars(globalData).Jail == 2));
        globalData.ActiveWindow.AddClickHereToContinue(AsylumDeteriorated_0);
    }

    private static void AsylumDeteriorated_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, InvestmentDividends);
    }

    // Originally PrivateInvestors
    private static void InvestmentDividends(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(InvestmentDividends_0);
    }

    private static void InvestmentDividends_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_ColoredMoneyToken, PopUpButton.Confirm, DebtRepaid);
    }

    // Originally PrivateCountYesNo: the Count invests when he received more support than the Asylum
    private static void CountInvestment(GlobalData globalData)
    {
        if (Vars(globalData).Jail != 2)
        {
            ObsessionsResolve(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CountInvestment_0);
    }

    private static void CountInvestment_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.GainServantFromLost, PopUpButton.Confirm, ObsessionsResolve);
    }

    private static void PrivatizedSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();

        FearOfTheUnknownVars vars = Vars(globalData);
        vars.Tracker -= globalData.PlayersNum >= 4 ? 2 : 1;

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMobSetup1, PopUpButton.Confirm, Privatized, content => content
                                                                                                                                                         .FormatWithCondition(0, () => vars.CreatureMade == _CREATURE_NOT_CREATED)
                                                                                                                                                         .FormatWithReplacement(1, vars.Tracker.ToString())
                                                                                                                                                         .FormatWithCondition(2, () => globalData.PlayersNum == 3));
    }

    #endregion Intro

    #region End of the Early Years - The Junk Palace

    // Originally JailEventA
    private static void HostileTakeover(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(LetterHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(HostileTakeover_0);
    }

    private static void HostileTakeover_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_ColoredMoneyToken, PopUpButton.Confirm, TheEscape);
    }

    // Originally JailEvent1
    private static void TheEscape(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(HostileTakeover))));
        globalData.ActiveWindow.AddNextContent(Vars(globalData).Victim == _VICTIM_BLACKSMITH ? 1 : 2, true);
        globalData.ActiveWindow.AddNextContent(3, true, content => content.FormatWithReplacement(0, Warden(globalData)));
        if (HuntActive(globalData)) globalData.ActiveWindow.AddNextContent(4);
        globalData.ActiveWindow.AddNextContent(5, true, content => content.FormatWithReplacement(0, Warden(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(BuildMeAWarrior);
    }

    // Originally JailEvent1A
    private static void BuildMeAWarrior(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(HostileTakeover))).FormatWithReplacement(1, Warden(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(BuildMeAWarrior_0);
    }

    private static void BuildMeAWarrior_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S2_BattleStartTile, PopUpButton.Confirm, WorldIsNotTheSame);
    }

    // Originally JailEvent1b
    private static void WorldIsNotTheSame(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(HostileTakeover))).FormatWithReplacement(1, Warden(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(MercifulHeritage);
    }

    // Originally JailEvent1c
    private static void MercifulHeritage(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        if (vars.Jail != 1 && vars.AsylumCount < 1)
        {
            AbruptEnd(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithCondition(0, () => vars.Jail == 1));
        globalData.ActiveWindow.AddNextContent(vars.Jail == 1 ? 1 : 2, false, content => content
                                                                                        .FormatWithReplacement(0, RandomPlayer(globalData, nameof(HostileTakeover)))
                                                                                        .FormatWithReplacement(1, Warden(globalData))
                                                                                        .FormatWithReplacement(2, AsylumUpgradeName(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(vars.Jail == 1 ? AbruptEnd : MercifulHeritage_0);
    }

    private static void MercifulHeritage_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_EstateUpgradeBACK, PopUpButton.Confirm, AbruptEnd,
            content => content.FormatWithReplacement(0, AsylumUpgradeName(globalData)));
    }

    // Originally EndtotheCount
    private static void AbruptEnd(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(AbruptEnd_0);
    }

    private static void AbruptEnd_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_ScenarioIcon, PopUpButton.Confirm, PrivatizedMiddleSetup,
            content => content.FormatWithCondition(0, () => Vars(globalData).RandomBool(_RND_COUNT_FAREWELL)));
    }

    private static void PrivatizedMiddleSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_HuntTokenFRONT, PopUpButton.Confirm, Privatized);
    }

    #endregion End of the Early Years - The Junk Palace

    #region Hub Events

    // Originally PrivateRewardSignIn: a player reached the end of the Creepy track
    private static void HorrorOfExistence(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Warden(globalData)));
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(player =>
        {
            Vars(globalData).SelectedPlayer = player;
            JunkyardScrap(globalData);
        }, showPlayer: player => !Vars(globalData).CreepReward[player]);
    }

    // Originally PrivateReward1
    private static void JunkyardScrap(GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               player = vars.SelectedPlayer;

        globalData.SaveToUndo();
        vars.CreepReward[player] = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, player)
                                                             .FormatWithReplacement(1, RandomDate(globalData, "JunkyardScrap" + player))
                                                             .FormatWithReplacement(2, Warden(globalData))
                                                             .FormatWithCondition(3, () => vars.Victim == _VICTIM_BUTCHER));
        globalData.ActiveWindow.AddClickHereToContinue(JunkyardScrap_0);
    }

    private static void JunkyardScrap_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.EstateUpgradeBack3, PopUpButton.Confirm, Privatized);
    }

    // Originally JunkPalaceSignIn
    private static void PalaceOfScraps(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 2, 3, "PalaceOfScraps" + globalData.Years)(content
                                                                                                                                  .FormatWithReplacement(0, globalData.TownName)
                                                                                                                                  .FormatWithReplacement(1, Warden(globalData))));
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(player =>
        {
            Vars(globalData).SelectedPlayer = player;
            PlanRevealed(globalData);
        });
    }

    // Originally JunkPalaceMeet
    private static void PlanRevealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDoNotOtherPlayersToSee(Vars(globalData).SelectedPlayer, data =>
        {
            // The penalty stays hidden while the Creature still lives
            if (HuntActive(data)) MeetingAtTheJunkPalace(data);
            else HideousFeast(data);
        });
    }

    private static void HideousFeast(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 1, 2, "HideousFeast" + Vars(globalData).SelectedPlayer)(content)
                                                             .FormatWithReplacement(0, Warden(globalData))
                                                             .FormatWithReplacement(2, JunkPenalty(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(MeetingAtTheJunkPalace);
    }

    // Originally JunkPalaceMeet1
    private static void MeetingAtTheJunkPalace(GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               player = vars.SelectedPlayer;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, player)
                                                             .FormatWithReplacement(1, RandomDate(globalData, "MeetingAtTheJunkPalace" + player))
                                                             .FormatWithReplacement(2, Warden(globalData)));
        globalData.ActiveWindow.AddNextContent(vars.Victim == _VICTIM_BUTCHER ? 1 : 2, true, content => content.FormatWithReplacement(0, Warden(globalData)));
        globalData.ActiveWindow.AddNextContent(3, true, content => content.FormatWithReplacement(0, Warden(globalData)));
        globalData.ActiveWindow.AddNextContentWithLinks(4, [data => JunkOffer(0, data), data => JunkOffer(1, data), data => JunkOffer(2, data), data => JunkOffer(3, data)], true);
    }

    private static void JunkOffer(int money, GlobalData globalData)
    {
        Vars(globalData).JunkOffer = money;
        JunkResolution(globalData);
    }

    // Originally JunkPalaceMeetb
    private static void JunkResolution(GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               player = vars.SelectedPlayer;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, player)
                                                             .FormatWithReplacement(1, RandomDate(globalData, "JunkResolution" + player))
                                                             .FormatWithReplacement(2, Warden(globalData)));
        if (HuntActive(globalData)) globalData.ActiveWindow.AddNextContent(1, false, content => content.FormatWithReplacement(0, Warden(globalData)));
        globalData.ActiveWindow.AddNextContent(2, false, content => content.FormatWithReplacement(0, Warden(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(JunkResolution_0);
    }

    private static void JunkResolution_0(GlobalData globalData)
    {
        int money = Vars(globalData).JunkOffer;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, Privatized,
            content => content.FormatWithReplacement(1, money.ToString()).FormatWithCondition(0, () => money == 0));
    }

    // Originally BattleDomeExplanation
    private static void BattleWarrior(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, globalData.LocalizedPlayerNumberIndex()));
        globalData.ActiveWindow.AddClickHereToContinue(Privatized);
    }

    #endregion Hub Events

    #region End of the Middle Years

    // Originally CreaturePleased: the Creature pays the family's debts to the Count
    private static void CreatureIntervenes(GlobalData globalData)
    {
        if (Vars(globalData).CreatureMade != _CREATURE_CREATED)
        {
            StartDeathCollection(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(CreatureIntervenes_0);
    }

    private static void CreatureIntervenes_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.MFWlogo, PopUpButton.Confirm, data =>
        {
            foreach (string player in data.GetActivePlayers()) Vars(data).Debt[player] = 0;
            Privatized(data);
        });
    }

    #endregion End of the Middle Years

    #region End of Generation III - The Battledome

    // Originally PrivateCreepPenalty
    private static void PenaltyForDefiance(GlobalData globalData)
    {
        FearOfTheUnknownVars vars     = Vars(globalData);
        string[]             defiants = globalData.GetActivePlayers().Where(player => !vars.CreepReward[player]).ToArray();

        if (defiants.Length == 0)
        {
            EnterTheBattledome(globalData);
            return;
        }

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Warden(globalData)));

        foreach (string player in defiants)
        {
            globalData.ActiveWindow.AddNextContent(1, false, content => content.FormatWithReplacement(0, player));
        }

        globalData.ActiveWindow.AddNextContent(2, true, content => content.FormatWithReplacement(0, JunkPenalty(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(EnterTheBattledome);
    }

    // Originally ENTERTHEBATTLEDOME
    private static void EnterTheBattledome(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Warden(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(BattlePreparation);
    }

    private static void BattlePreparation(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(data =>
        {
            Vars(data).StatIndex = 0;
            WarriorEntry(data);
        });
    }

    private static string StatPlayer(GlobalData globalData) => globalData.GetPlayerNameByIndex(Vars(globalData).StatIndex);

    private static bool IsValidStat(string value) => int.TryParse(value, out int stat) && stat is >= 0 and <= 99;

    // Originally Player1StatEntry - Player5StatEntry
    private static void WarriorEntry(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithReplacement(0, StatPlayer(globalData)));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, StatPlayer(globalData)));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [WarriorAgility], true);
    }

    private static void WarriorAgility(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveInputPopup = new GameplayInputPopup(globalData, "0", PopUpButton.Confirm, IsValidStat, value =>
        {
            Vars(globalData).Agility[StatPlayer(globalData)] = int.Parse(value);
            AgilityConfirmed(globalData);
        });
    }

    private static void AgilityConfirmed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Vars(globalData).Agility[StatPlayer(globalData)].ToString()));
        globalData.ActiveWindow.AddClickHereToContinue(WarriorStrength);
    }

    private static void WarriorStrength(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveInputPopup = new GameplayInputPopup(globalData, "0", PopUpButton.Confirm, IsValidStat, value =>
        {
            Vars(globalData).Strength[StatPlayer(globalData)] = int.Parse(value);
            StrengthConfirmed(globalData);
        });
    }

    private static void StrengthConfirmed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Vars(globalData).Strength[StatPlayer(globalData)].ToString()));
        globalData.ActiveWindow.AddClickHereToContinue(WarriorLife);
    }

    private static void WarriorLife(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveInputPopup = new GameplayInputPopup(globalData, "0", PopUpButton.Confirm, IsValidStat, value =>
        {
            Vars(globalData).Life[StatPlayer(globalData)] = int.Parse(value);
            LifeConfirmed(globalData);
        });
    }

    private static void LifeConfirmed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, Vars(globalData).Life[StatPlayer(globalData)].ToString())
                                                             .FormatWithReplacement(1, StatPlayer(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(WarriorName);
    }

    private static void WarriorName(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveInputPopup = new GameplayInputPopup(globalData, string.Empty, PopUpButton.Confirm, value => value.Trim().Length is > 0 and <= 40, value =>
        {
            Vars(globalData).WarriorName[StatPlayer(globalData)] = value.Trim();
            WarriorComplete(globalData);
        });
    }

    private static void WarriorComplete(GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               player = StatPlayer(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithReplacement(0, vars.WarriorName[player]));
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, vars.Agility[player].ToString())
                                                             .FormatWithReplacement(1, vars.Strength[player].ToString())
                                                             .FormatWithReplacement(2, vars.Life[player].ToString()));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data =>
        {
            Vars(data).StatIndex += 1;

            if (Vars(data).StatIndex < data.PlayersNum) WarriorEntry(data);
            else MainEvent(data);
        }, WarriorEntry], true);
    }

    // Originally BattleIntro
    private static void MainEvent(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();

        int rewarded = globalData.GetActivePlayers().Count(player => vars.CreepReward[player]);
        vars.Ending       = rewarded >= (globalData.PlayersNum <= 3 ? 2 : 3) ? 5 : 6;
        vars.BattleStep   = 0;
        vars.BattleWinner = string.Empty;

        foreach (string player in globalData.GetActivePlayers()) vars.Hp[player] = vars.Life[player];

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddCustomContent(JournalHeader(globalData));
        globalData.ActiveWindow.AddDefaultContent();

        foreach (string player in globalData.GetActivePlayers())
        {
            globalData.ActiveWindow.AddNextContent(1, false, content => content.FormatWithReplacement(0, player).FormatWithReplacement(1, vars.WarriorName[player]));
        }

        globalData.ActiveWindow.AddNextContent(2, true);

        // Rules line (not narrated): Warriors entered with 0 Life never fight
        string[] lifeless = globalData.GetActivePlayers().Where(player => vars.Life[player] <= 0).ToArray();
        if (lifeless.Length > 0)
        {
            globalData.ActiveWindow.AddCustomContent(globalData.GetScenarioLocalizedTag("MainEvent_NoLife")
                                                               .FormatWithReplacement(0, string.Join(", ", lifeless.Select(player => vars.WarriorName[player]))), true);
        }

        globalData.ActiveWindow.AddClickHereToContinue(BattleBegins);
    }

    // With fewer than two Warriors able to fight there is no battle: the only one left wins, or nobody when none is
    private static void BattleBegins(GlobalData globalData)
    {
        FearOfTheUnknownVars vars  = Vars(globalData);
        string[]             alive = globalData.GetActivePlayers().Where(player => vars.Hp[player] > 0).ToArray();

        if (alive.Length > 1) Battle(globalData);
        else BattleDecided(globalData, alive);
    }

    // Originally BattleChecks / BattleStart. The source drew attacker and target among all Warriors, also the eliminated ones,
    // and counted repeated eliminations - a Warrior already destroyed could still attack and win. Only Warriors still
    // standing fight here, and the last one standing wins. Warriors entered with 0 Life never fight (BattleBegins).
    private static void Battle(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);
        int                  step = vars.BattleStep;

        globalData.SaveToUndo();

        string[]       alive = globalData.GetActivePlayers().Where(player => vars.Hp[player] > 0).ToArray();
        List<string[]> pairs = alive.SelectMany(attacker => alive.Where(target => target != attacker).Select(target => new[] { attacker, target })).ToList();

        string[] duel     = pairs[vars.RandomElement(Enumerable.Range(0, pairs.Count).ToList(), _RND_BATTLE_PAIR + step)];
        string   attacker = duel[0];
        string   target   = duel[1];
        bool     hit      = vars.RandomElement(Enumerable.Range(1, 40).ToList(), _RND_BATTLE_HIT + step) > vars.Agility[target];

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 2, 13, "Battle" + step)(content)
                                                             .FormatWithReplacement(0, vars.WarriorName[attacker])
                                                             .FormatWithReplacement(1, vars.WarriorName[target]));

        if (hit)
        {
            vars.Hp[target] -= vars.Strength[attacker];
            globalData.ActiveWindow.AddNextContent(1, false, content => Either(globalData, 2, 10, "Battle" + step)(content)
                                                                       .FormatWithReplacement(0, vars.WarriorName[target])
                                                                       .FormatWithReplacement(1, vars.Strength[attacker].ToString()));

            if (vars.Hp[target] <= 0)
            {
                globalData.ActiveWindow.AddNextContent(3, false, content => Either(globalData, 2, 3, "BattleEliminated" + step)(Either(globalData, 1, 6, "Battle" + step)(content))
                                                                           .FormatWithReplacement(0, vars.WarriorName[target]));
            }
        }
        else
        {
            globalData.ActiveWindow.AddNextContent(2, false, content => Either(globalData, 1, 6, "Battle" + step)(content).FormatWithReplacement(0, vars.WarriorName[target]));
        }

        globalData.ActiveWindow.AddNextContentWithLinks(4, [BattleContinues], true);
    }

    private static void BattleContinues(GlobalData globalData)
    {
        FearOfTheUnknownVars vars  = Vars(globalData);
        string[]             alive = globalData.GetActivePlayers().Where(player => vars.Hp[player] > 0).ToArray();

        vars.BattleStep += 1;

        // A fight nobody can win (no damage left) or an endless one ends with the Warrior holding the most life
        bool stalemate = alive.All(player => vars.Strength[player] == 0) || vars.BattleStep >= _BATTLE_MAX_STEPS;

        if (alive.Length > 1 && !stalemate)
        {
            Battle(globalData);
            return;
        }

        BattleDecided(globalData, alive);
    }

    private static void BattleDecided(GlobalData globalData, string[] alive)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        vars.BattleWinner = alive.Length == 0 ? string.Empty : alive.MaxBy(player => vars.Hp[player])!;

        if (vars.BattleWinner == string.Empty) BattleOver_NoWinner(globalData);
        else BattleOver(globalData);
    }

    // Every Warrior was entered with 0 Life: nobody fights, nobody gains the 9VP (not narrated)
    private static void BattleOver_NoWinner(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultBaseTitle();
        globalData.ActiveWindow.AddCustomContent(globalData.GetScenarioLocalizedTag("BattleNoWinner_Content"));
        globalData.ActiveWindow.AddClickHereToContinue(ObsessionResolution);
    }

    // Originally BattleEnd
    private static void BattleOver(GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               winner = vars.BattleWinner;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithReplacement(0, winner)
                                                             .FormatWithReplacement(1, vars.WarriorName[winner])
                                                             .FormatWithReplacement(2, Warden(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(BattleOver_0);
    }

    private static void BattleOver_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, ObsessionResolution,
            content => content.FormatWithReplacement(0, Vars(globalData).BattleWinner));
    }

    #endregion End of Generation III - The Battledome
}
