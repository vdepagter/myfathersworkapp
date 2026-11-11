using System.Runtime.CompilerServices;

namespace MyFathersWorkApp;

public static partial class FearOfTheUnknown
{
    private const int _VICTIM_BLACKSMITH = 1;
    private const int _VICTIM_BUTCHER    = 2;

    public static void WitchHunt(GlobalData globalData)
    {
        // Round 1 -> Mania
        // Round 2 -> Mania2
        // Round 3 -> Mania3
        FearOfTheUnknownVars vars = Vars(globalData);

        vars.HubId           = FearOfTheUnknownHubId.WitchHunt;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(globalData.Years);

        const string       townHallSection = "TownHall";
        GameplayHubSection townHall        = globalData.ActiveHub.AddSection(townHallSection, true);
        townHall.ReplaceShouldShow(() => globalData.Years == Years.Late);
        townHall.AddDefaultContent(townHallSection);

        const string       familyPlotSection = "FamilyPlot";
        GameplayHubSection familyPlot        = globalData.ActiveHub.AddSection(familyPlotSection, true);
        familyPlot.AddDefaultContent(familyPlotSection);
        familyPlot.AddSpecialClickHere(familyPlotSection, PlotPlayerCheckin);

        const string       tenVpSection = "TenVp";
        GameplayHubSection tenVp        = globalData.ActiveHub.AddSection(tenVpSection);
        tenVp.ReplaceShouldShow(() => !vars.Milestones[0]);
        tenVp.AddDefaultContent(tenVpSection);
        tenVp.AddSpecialClickHere(tenVpSection, data => MilestoneReached(0, data));

        const string       researchSection = "Research";
        GameplayHubSection research        = globalData.ActiveHub.AddSection(researchSection);
        research.ReplaceShouldShow(() => !vars.Milestones[1]);
        research.AddDefaultContent(researchSection);
        research.AddSpecialClickHere(researchSection, data => MilestoneReached(1, data));

        const string       insanitySection = "Insanity";
        GameplayHubSection insanity        = globalData.ActiveHub.AddSection(insanitySection);
        insanity.ReplaceShouldShow(() => !vars.Milestones[2]);
        insanity.AddDefaultContent(insanitySection);
        insanity.AddSpecialClickHere(insanitySection, data => MilestoneReached(2, data));

        const string       creepySection = "Creepy";
        GameplayHubSection creepy        = globalData.ActiveHub.AddSection(creepySection);
        creepy.ReplaceShouldShow(() => !vars.Milestones[3]);
        creepy.AddDefaultContent(creepySection);
        creepy.AddSpecialClickHere(creepySection, data => MilestoneReached(3, data));

        const string       secondRoundSection = "SecondRound";
        GameplayHubSection secondRound        = globalData.ActiveHub.AddSection(secondRoundSection);
        secondRound.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        secondRound.AddDefaultContent(secondRoundSection);
        secondRound.AddClickHereContinueNextRound(WitchHunt_0, true);

        globalData.ActiveHub.AddEndOfGenerationSection(WitchHunt_0);
    }

    private static void WitchHunt_0(GlobalData globalData)
    {
        globalData.ShowEndOfRoundPopUp(globalData.Years switch
        {
            Years.Early  => WitchHunt,
            Years.Middle => PublicExecution,
            Years.Late   => FateOfTheTown,
            _            => _ => { }
        });
    }

    #region Setup

    private static void WitchHuntSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();

        // Set here, not in the hub method: undo and load rebuild the last shown hub (still WitchHunt during the Generation II intro
        // and setup), which reset Path and sent the Foreigners branch into the Tension hub and an ending-less final screen
        FearOfTheUnknownVars vars = Vars(globalData);
        vars.Path = FearOfTheUnknownHubId.WitchHunt;
        vars.Tracker = globalData.PlayersNum switch
        {
            2 => 6,
            3 => vars.RandomElement([6, 7], _RND_TRACKER),
            _ => vars.RandomElement([7, 8, 9], _RND_TRACKER) // 4
        };

        string firstPlayer = vars.RandomElement(globalData.GetActivePlayers().ToList(), _RND_FIRST_PLAYER);

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AngryMobSetup1, PopUpButton.Confirm, WitchHunt,
            content => content.FormatWithReplacement(0, vars.Tracker.ToString()).FormatWithReplacement(1, firstPlayer));
    }

    private static void WitchHuntLateSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, WitchHunt,
            content => content.FormatWithCondition(0, () => Vars(globalData).Victim == _VICTIM_BUTCHER));
    }

    #endregion Setup

    #region Milestones

    // The first player to reach a milestone claims the matching Obsession for the next Generation (one per player)
    private static void MilestoneReached(int milestone, GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);
        vars.Milestone = milestone;

        if (vars.ClaimCount >= globalData.PlayersNum)
        {
            vars.Milestones[milestone] = true;
            OverAchievement(globalData);
            return;
        }

        Milestone(globalData);
    }

    private static void Milestone(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithIndex(0, vars.Milestone));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => vars.Milestone == 0));
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(player =>
        {
            vars.MilestonePlayer = player;

            if (vars.Fate[player] != 0)
            {
                Overambitious(globalData);
                return;
            }

            vars.Milestones[vars.Milestone] = true;
            vars.Fate[player]               = vars.Milestone + 1;
            MilestoneSignIn(globalData);
        });
    }

    private static void MilestoneSignIn(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithIndex(0, vars.Milestone));
        globalData.ActiveWindow.AddNextContent(vars.Milestone, false, content => content
                                                                                .FormatWithReplacement(0, Newspaper(globalData))
                                                                                .FormatWithReplacement(1, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(MilestoneSignIn_0);
    }

    private static void MilestoneSignIn_0(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, vars.Milestone == 1 ? PopUpIcon.StorybookToken : PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm,
            data =>
            {
                Vars(data).ClaimCount += 1;
                WitchHunt(data);
            }, content => content.FormatWithCondition(0, () => vars.Milestone == 1));
    }

    private static void OverAchievement(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(Either(globalData, 0, 2, "OverAchievement" + Vars(globalData).Milestone));
        globalData.ActiveWindow.AddClickHereToContinue(OverAchievement_0);
    }

    private static void OverAchievement_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, data =>
        {
            Vars(data).ClaimCount += 1;
            WitchHunt(data);
        });
    }

    private static void Overambitious(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Vars(globalData).MilestonePlayer));
        globalData.ActiveWindow.AddClickHereToContinue(Overambitious_0);
    }

    private static void Overambitious_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, WitchHunt);
    }

    private static string Newspaper(GlobalData globalData)
    {
        return Tag(globalData, "Newspaper").FormatWithReplacement(0, globalData.TownName).FormatWithIndex(1, Vars(globalData).Flavor("Newspaper", 5));
    }

    #endregion Milestones

    #region Family Plot

    private static void PlotPlayerCheckin(GlobalData globalData)
    {
        FearOfTheUnknownVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(Either(globalData, 0, 2, "PlotPlayerCheckin" + vars.FamilyPlot.Count(pair => pair.Value)));
        globalData.ActiveWindow.AddAllPlayersNamesAsOptions(player =>
        {
            vars.MilestonePlayer = player;
            GainFamilyPlot(globalData);
        });
    }

    private static void GainFamilyPlot(GlobalData globalData)
    {
        FearOfTheUnknownVars vars   = Vars(globalData);
        string               player = vars.MilestonePlayer;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => Either(globalData, 2, 2, "GainFamilyPlot" + player)(content)
                                                             .FormatWithReplacement(0, player)
                                                             .FormatWithReplacement(1, RandomDate(globalData, "GainFamilyPlot" + player)));
        globalData.ActiveWindow.AddClickHereToContinue(data =>
        {
            Vars(data).Mobbed             = true;
            Vars(data).FamilyPlot[player] = true;
            WitchHunt(data);
        });
    }

    #endregion Family Plot

    #region Public Execution

    private static void PublicExecution(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(globalData.PlayersNum == 2 ? ExecutionBid : ExecutionVote);
    }

    private static void ExecutionVote(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereAfterVote(ExecutionVoteResolved);
    }

    private static void ExecutionVoteResolved(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [BlacksmithDead, ButcherDead], true);
    }

    private static void ExecutionBid(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereAfterBid(ExecutionBidResolved);
    }

    private static void ExecutionBidResolved(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [BlacksmithDead, ButcherDead], true);
    }

    private static void BlacksmithDead(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Victim = _VICTIM_BLACKSMITH;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(PublicExecution))));
        globalData.ActiveWindow.AddClickHereToContinue(globalData.PlayersNum >= 3 ? BlacksmithDead_0 : WitchHuntLateSetup);
    }

    private static void BlacksmithDead_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.VotingTokenNay, PopUpButton.Confirm, WitchHuntLateSetup);
    }

    private static void ButcherDead(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).Victim = _VICTIM_BUTCHER;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData, nameof(PublicExecution))));
        globalData.ActiveWindow.AddClickHereToContinue(globalData.PlayersNum >= 3 ? ButcherDead_0 : WitchHuntLateSetup);
    }

    private static void ButcherDead_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.VotingTokenYay, PopUpButton.Confirm, WitchHuntLateSetup);
    }

    #endregion Public Execution

    #region End of Generation I

    // Originally FPFateHub / FPFateAssignHub. The Obsessions of players without a milestone are drawn when shown (Obsession)
    private static void FateOfTheTown(GlobalData globalData)
    {
        if (Vars(globalData).Mobbed) FearOfOurFamily(globalData);
        else Gen1End(globalData);
    }

    private static void FearOfOurFamily(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, RandomPlayer(globalData)));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [FearOfOurFamily_0]);
    }

    private static void FearOfOurFamily_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S2_EstateUpgradeBACK, PopUpButton.Confirm, Gen1End, string.Empty);

        string content = string.Empty;

        foreach (string player in globalData.GetActivePlayers().Where(player => Vars(globalData).FamilyPlot[player]))
        {
            content += Tag(globalData, "FearOfOurFamily_0_Content").FormatWithReplacement(0, player);
        }

        globalData.ActivePopup.ReplaceDescription(content.TrimTrailingBreaks());
    }

    private static void Gen1End(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.EndOfGeneration, PopUpIcon.MFWlogo, PopUpButton.Confirm,
            data => (ForcedFate(data) ?? Vars(data).RandomElement([Gen1FateVictim, Gen1FateMob], _RND_GEN1_FATE)).Invoke(data),
            content => content.FormatWithCondition(0, () => Vars(globalData).Milestones.Any(claimed => !claimed)));
    }

    // Village-name secret (Help page): true forces Foreigners, false Creature, null draws as usual.
    private static bool? ForcedForeigners(GlobalData globalData) => globalData.TownName switch
    {
        FearOfTheUnknownVars.FOREIGNERS_TOWN_NAME => true,
        FearOfTheUnknownVars.CREATURE_TOWN_NAME   => false,
        _                                         => null,
    };

    // With a village-name secret the fate screen is not drawn: the one whose text agrees with play is shown. When neither
    // agrees, the Victim screen is shown with a link to the forced branch (its text is then slightly off, as the
    // designer accepted for the original codes).
    private static Action<GlobalData>? ForcedFate(GlobalData globalData)
    {
        if (ForcedForeigners(globalData) is not bool foreigners) return null;
        if (VictimLeadsToForeigners(globalData) == foreigners) return Gen1FateVictim;
        if (MobLeadsToForeigners(globalData) == foreigners) return Gen1FateMob;
        return Gen1FateVictim;
    }

    private static bool VictimLeadsToForeigners(GlobalData globalData) => Vars(globalData).Victim == _VICTIM_BUTCHER;

    private static bool MobLeadsToForeigners(GlobalData globalData) => !Vars(globalData).Mobbed;

    // Originally S4Witch1: the executed Butcher was innocent -> Foreigners, the Blacksmith was guilty -> Creature
    private static void Gen1FateVictim(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Gen1Fate(globalData, VictimLeadsToForeigners(globalData));
    }

    // Originally S4Witch2: nobody overtaken by the mob -> Foreigners, the mob was appeased -> Creature
    private static void Gen1FateMob(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Gen1Fate(globalData, MobLeadsToForeigners(globalData));
    }

    // Body of both fate screens (tags of the calling screen): title condition and content 1 when the text leads to
    // Foreigners, content 2 when to Creature. The link follows a village-name secret when there is one.
    private static void Gen1Fate(GlobalData globalData, bool textForeigners, [CallerMemberName] string caller = "")
    {
        bool foreigners = ForcedForeigners(globalData) ?? textForeigners;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithCondition(0, () => textForeigners), caller);
        globalData.ActiveWindow.AddNextContentWithLinks(textForeigners ? 1 : 2, [foreigners ? ForeignersIntro : CreatureIntro], false, content => content
                                                                                                                                       .FormatWithReplacement(0, RandomPlayer(globalData, caller))
                                                                                                                                       .FormatWithReplacement(1, globalData.TownName), caller);
    }

    #endregion End of Generation I
}
