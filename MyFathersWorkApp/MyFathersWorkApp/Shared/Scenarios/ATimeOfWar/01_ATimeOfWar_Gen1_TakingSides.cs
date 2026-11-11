namespace MyFathersWorkApp;

public static partial class ATimeOfWar
{
    // Town Hall rumors (originally $rumor option numbers; the source swaps the numbers of RumorD3 and RumorD5, harmless)
    private const int _RUMOR_GUARD      = 1; // RumorD1, once
    private const int _RUMOR_GENTLEMAN  = 2; // RumorD2, once
    private const int _RUMOR_OLD_WOMAN  = 3; // RumorD3
    private const int _RUMOR_CITIZENS   = 4; // RumorD4
    private const int _RUMOR_HUNCHED    = 5; // RumorD5
    private const int _RUMOR_ASSISTANT  = 6; // RumorD6Guns, once

    // Showcase of the region (originally $gunsbonus)
    private const int _SHOWCASE_KNOWLEDGE   = 1;
    private const int _SHOWCASE_INGREDIENTS = 2;
    private const int _SHOWCASE_MONEY       = 3;

    public static void TakingSides(GlobalData globalData)
    {
        // Round 1 -> TakeSides
        // Round 2 -> TakeSides2
        // Round 3 -> TakeSides3
        ATimeOfWarVars vars = Vars(globalData);

        vars.HubId           = ATimeOfWarHubId.TakingSides;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(globalData.Years);

        const string       townHallSection = "TownHall";
        GameplayHubSection townHall        = globalData.ActiveHub.AddSection(townHallSection, true);
        townHall.ReplaceShouldShow(() => globalData.Years is Years.Early or Years.Middle);
        townHall.AddDefaultContent(townHallSection);
        townHall.AddSpecialClickHere(townHallSection, RumorD);

        const string       pledgeSection = "Pledge";
        GameplayHubSection pledge        = globalData.ActiveHub.AddSection(pledgeSection);
        pledge.ReplaceShouldShow(() => globalData.Years == Years.Early);
        pledge.AddDefaultContent(pledgeSection);
        pledge.AddSpecialClickHere(pledgeSection, TakingSides_0, true);

        const string       pledgeMiddleSection = "PledgeMiddle";
        GameplayHubSection pledgeMiddle        = globalData.ActiveHub.AddSection(pledgeMiddleSection);
        pledgeMiddle.ReplaceShouldShow(() => globalData.Years == Years.Middle);
        pledgeMiddle.AddDefaultContent(pledgeMiddleSection);

        // Knowledge / Ingredient / Wealth Bonus chosen with the mayor's assistant (not shown in the Early Years, as in the source)
        const string       bonusSection = "Bonus";
        GameplayHubSection bonus        = globalData.ActiveHub.AddSection(bonusSection);
        bonus.ReplaceShouldShow(() => (globalData.Years is Years.Middle or Years.Late) && vars.Showcase != 0);
        bonus.FormatTitle(title => title.FormatWithIndex(0, vars.Showcase));
        bonus.AddDefaultContent(bonusSection, content => content
                                                        .FormatWithIndex(0, vars.Showcase)
                                                        .FormatWithCondition(1, () => globalData.Years == Years.Late)
                                                        .FormatWithCondition(2, () => globalData.Years == Years.Late && vars.Showcase == _SHOWCASE_MONEY));

        // Barracks built: the source draws "mean" / "nice" at the start; drawn here, only when the section is shown
        bool               militaryShown   = globalData.Years == Years.Late && vars.Barracks == ExtendedBool.True;
        bool               harsh           = militaryShown && HarshMilitary(globalData);
        const string       militarySection = "MilitaryDemands";
        GameplayHubSection military        = globalData.ActiveHub.AddSection(militarySection);
        military.ReplaceShouldShow(() => militaryShown);
        military.FormatTitle(title => title.FormatWithCondition(0, () => harsh));
        military.AddDefaultContent(militarySection, content => content.FormatWithCondition(0, () => harsh).FormatWithCondition(1, () => !harsh));

        const string       endMiddleSection = "EndMiddle";
        GameplayHubSection endMiddle        = globalData.ActiveHub.AddSection(endMiddleSection);
        endMiddle.ReplaceShouldShow(() => globalData.Years == Years.Middle);
        endMiddle.AddSpecialClickHere(endMiddleSection, TakingSides_0);

        const string       endLateSection = "EndLate";
        GameplayHubSection endLate        = globalData.ActiveHub.AddSection(endLateSection);
        endLate.ReplaceShouldShow(() => globalData.Years == Years.Late);
        endLate.AddSpecialClickHere(endLateSection, TakingSides_0);
    }

    private static void TakingSides_0(GlobalData globalData)
    {
        globalData.ShowEndOfRoundPopUp(globalData.Years switch
        {
            Years.Early  => TownHallSpecial,
            Years.Middle => TakeEvent,
            Years.Late   => GenerationOneEnd,
            _            => TakingSides
        });
    }

    // Originally $barracksact: "mean" (Caretaker to Lost) or "nice" (Caretaker to your Quarters, -5VP if left on the Barracks)
    private static bool HarshMilitary(GlobalData globalData)
    {
        return Vars(globalData).RandomElement([true, false], _RND_BARRACKS_ACT);
    }

    #region Setup

    // Originally the setupStyle block of TakeSides (shown until the first Town Hall visit; the web app shows it once)
    private static void TakingSidesEarlySetup(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.Tracker = globalData.PlayersNum switch
        {
            2 => vars.RandomElement([4, 5, 6], _RND_TRACKER),
            3 => vars.RandomElement([6, 7, 8], _RND_TRACKER),
            _ => vars.RandomElement([7, 8, 9], _RND_TRACKER) // 4
        };

        string firstPlayer = vars.RandomElement(globalData.GetActivePlayers().ToList(), _RND_FIRST_PLAYER);

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_ScenarioIcon, PopUpButton.Confirm, TakingSides,
            content => content.FormatWithReplacement(0, vars.Tracker.ToString()).FormatWithReplacement(1, firstPlayer));
    }

    // Originally the setupStyle block of TakeSides3
    private static void TakingSidesLateSetup(GlobalData globalData)
    {
        bool barracks = Vars(globalData).Barracks == ExtendedBool.True;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, barracks ? PopUpIcon.Caretaker : PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, TakingSides,
            content => content.FormatWithCondition(0, () => barracks));
    }

    #endregion Setup

    #region Town Hall

    private static void RumorD(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.TownHallVisits += 1;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, vars.Flavor("RumorD#" + vars.TownHallVisits, 4)));
        AddRumorOptions(globalData);
    }

    private static void TownHallSpecial(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.TownHallMeeting = true;
        vars.TownHallIndex   = 0;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(TownHallS1);
    }

    // One conversation per player during the meeting
    private static void TownHallS1(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars   = Vars(globalData);
        string         player = globalData.GetPlayerNameByIndex(vars.TownHallIndex);
        vars.TownHallIndex  += 1;
        vars.TownHallVisits += 1;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, player));
        AddRumorOptions(globalData);
    }

    // Two of the remaining rumors (the guard, the gentleman and the mayor's assistant can only be met once)
    private static void AddRumorOptions(GlobalData globalData)
    {
        ATimeOfWarVars vars      = Vars(globalData);
        int            baseIndex = _RND_RUMOR_OPTIONS + Math.Min(vars.TownHallVisits - 1, 9) * 2;

        List<int> rumors = [_RUMOR_GUARD, _RUMOR_GENTLEMAN, _RUMOR_OLD_WOMAN, _RUMOR_CITIZENS, _RUMOR_HUNCHED, _RUMOR_ASSISTANT];
        if (vars.MetGuard) rumors.Remove(_RUMOR_GUARD);
        if (vars.MetGentleman) rumors.Remove(_RUMOR_GENTLEMAN);
        if (vars.ShowcaseChosen) rumors.Remove(_RUMOR_ASSISTANT);

        int first = vars.RandomElement(rumors, baseIndex);
        rumors.Remove(first);
        int second = vars.RandomElement(rumors, baseIndex + 1);

        string options = globalData.GetScenarioLocalizedTag("Rumor_Option");
        globalData.ActiveWindow!.AddCustomLink(options.FormatWithIndex(0, first - 1),  RumorScreen(first));
        globalData.ActiveWindow!.AddCustomLink(options.FormatWithIndex(0, second - 1), RumorScreen(second));
    }

    private static Action<GlobalData> RumorScreen(int rumor)
    {
        return rumor switch
        {
            _RUMOR_GUARD     => RumorD1,
            _RUMOR_GENTLEMAN => RumorD2,
            _RUMOR_OLD_WOMAN => RumorD3,
            _RUMOR_CITIZENS  => RumorD4,
            _RUMOR_HUNCHED   => RumorD5,
            _                => RumorD6Guns
        };
    }

    // Reward screen after a conversation (originally $RumorDxnextPsg: one fixed draw per rumor)
    private static void RumorReward(GlobalData globalData, int rumor)
    {
        Vars(globalData).RandomElement([RumorIngredient, RumorKnowledge, RumorPitch], _RND_RUMOR_REWARD + rumor - 1).Invoke(globalData);
    }

    // After a reward: the next player of the Town Hall meeting, else back to the hub
    private static void RumorReturn(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        if (vars.TownHallMeeting && vars.TownHallIndex < globalData.PlayersNum)
        {
            TownHallS1(globalData);
            return;
        }

        vars.TownHallMeeting = false;
        TakingSides(globalData);
    }

    #endregion Town Hall

    #region Rumors

    // The guard
    private static void RumorD1(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).MetGuard = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddStoryContent("Secret_Content");
        globalData.ActiveWindow.AddOnceYouAreReady(RumorD1Revealed);
    }

    private static void RumorD1Revealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddNextContent(War(globalData), false, content => content
                                                                                 .FormatWithReplacement(0, Crest(globalData))
                                                                                 .FormatWithReplacement(1, Winner(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(RumorD1_0);
    }

    private static void RumorD1_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, CrestIcon(globalData), PopUpButton.Confirm, data => RumorReward(data, _RUMOR_GUARD),
            content => content.FormatWithReplacement(0, Crest(globalData)));
    }

    // The gentleman
    private static void RumorD2(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddStoryContent("Secret_Content");
        globalData.ActiveWindow.AddOnceYouAreReady(RumorD2Revealed);
    }

    private static void RumorD2Revealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).MetGentleman = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Figure(globalData)));
        AddFinishedReading(globalData, RumorD2_0);
    }

    private static void RumorD2_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, FigureIcon(globalData), PopUpButton.Confirm, data => RumorReward(data, _RUMOR_GENTLEMAN),
            content => content.FormatWithReplacement(0, Figure(globalData)));
    }

    // The old woman: Unified Monarchists rumor
    private static void RumorD3(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).HeardMonarchists = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddStoryContent("Secret_Content");
        globalData.ActiveWindow.AddOnceYouAreReady(RumorD3Revealed);
    }

    private static void RumorD3Revealed(GlobalData globalData)
    {
        globalData.SaveToUndo();

        // Source: $war == 2 && $quarter == "yes" ($quarter only exists when the town supports the Separatists). The
        // sentence needs both values, so with War still undrawn mirror mode cannot preview the first draw: Quarter is
        // drawn first and asked as a hidden value (sentence shown = yes; no sentence = no, which is wrong only with
        // War 1, where Quarter never matters), then War is asked visibly. Once War is known, Quarter is only drawn
        // (and visible) with War 2.
        ATimeOfWarVars vars = Vars(globalData);
        bool quartering = vars.WarWinner == WarFaction.Separatists &&
                          (vars.War == 0 ? Quarter(globalData) && War(globalData) == 2 : War(globalData) == 2 && Quarter(globalData));

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => quartering));
        AddFinishedReading(globalData, data => RumorReward(data, _RUMOR_OLD_WOMAN));
    }

    // The concerned citizens: Separatists rumor
    private static void RumorD4(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).HeardSeparatists = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddStoryContent("Secret_Content");
        globalData.ActiveWindow.AddOnceYouAreReady(RumorD4Revealed);
    }

    private static void RumorD4Revealed(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithIndex(0, vars.Flavor("RumorD4#0#" + vars.TownHallVisits, 2))
                                                             .FormatWithIndex(1, vars.Flavor("RumorD4#1#" + vars.TownHallVisits, 2)));
        AddFinishedReading(globalData, data => RumorReward(data, _RUMOR_CITIZENS));
    }

    // The hunched figure: a Wooden Figure (repeatable)
    private static void RumorD5(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).WoodenFigures += 1;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddStoryContent("Secret_Content");
        globalData.ActiveWindow.AddOnceYouAreReady(RumorD5Revealed);
    }

    private static void RumorD5Revealed(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddNextContent(War(globalData));
        AddFinishedReading(globalData, RumorD5_0);
    }

    private static void RumorD5_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_WoodenFigureToken, PopUpButton.Confirm, RumorCreep);
    }

    // The mayor's assistant: showcase of the region
    private static void RumorD6Guns(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddStoryContent("Secret_Content");
        globalData.ActiveWindow.AddOnceYouAreReady(RumorD6GunsRevealed);
    }

    private static void RumorD6GunsRevealed(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        globalData.SaveToUndo();

        List<int> showcases = [_SHOWCASE_KNOWLEDGE, _SHOWCASE_INGREDIENTS, _SHOWCASE_MONEY];
        int       first     = vars.RandomElement(showcases, _RND_SHOWCASE_1);
        showcases.Remove(first);
        int second = vars.RandomElement(showcases, _RND_SHOWCASE_2);

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent();

        string choice = globalData.GetScenarioLocalizedTag("Showcase_Choice");
        string note   = globalData.GetScenarioLocalizedTag("Showcase_Note");

        foreach (int showcase in new[] { first, second })
        {
            globalData.ActiveWindow.AddCustomLink(choice.FormatWithIndex(0, showcase), data =>
            {
                Vars(data).Showcase = showcase;
                RumorD6Confirm(data);
            });
            globalData.ActiveWindow.AddCustomContent(note.FormatWithIndex(0, showcase));
        }
    }

    private static void RumorD6Confirm(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, Vars(globalData).Showcase));
        globalData.ActiveWindow.AddClickHereToContinue(data =>
        {
            Vars(data).ShowcaseChosen = true;
            RumorReward(data, _RUMOR_ASSISTANT);
        });
    }

    #endregion Rumors

    #region Rewards

    private static void RumorIngredient(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(RumorIngredient_0);
    }

    private static void RumorIngredient_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.MFWlogo, PopUpButton.Confirm, RumorReturn);
    }

    private static void RumorKnowledge(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(RumorKnowledge_0);
    }

    private static void RumorKnowledge_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.MFWlogo, PopUpButton.Confirm, RumorReturn);
    }

    private static void RumorPitch(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(RumorPitch_0);
    }

    private static void RumorPitch_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.AdvanceAngryMob, PopUpButton.Confirm, RumorReturn);
    }

    private static void RumorCreep(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(RumorCreep_0);
    }

    private static void RumorCreep_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, RumorReturn);
    }

    #endregion Rewards

    #region Military Protection

    // Barracks vote at the end of the Middle Years (runs with Years == Late)
    private static void TakeEvent(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Winner(globalData)).FormatWithReplacement(1, Loser(globalData)));
        globalData.ActiveWindow.AddClickHereAfterVote(TakeEventb);
    }

    private static void TakeEventb(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [
            data =>
            {
                Vars(data).Barracks = ExtendedBool.True;
                ResolutionofEvent(data);
            },
            data =>
            {
                Vars(data).Barracks = ExtendedBool.False;
                ResolutionofEvent(data);
            }
        ], true);
    }

    private static void ResolutionofEvent(GlobalData globalData)
    {
        bool barracks = Vars(globalData).Barracks == ExtendedBool.True;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddNextContent(barracks ? 1 : 2, false, content => content.FormatWithIndex(0, globalData.LocalizedPlayerNumberIndex()));
        globalData.ActiveWindow.AddClickHereToContinue(ResolutionofEvent_0);
    }

    // Flags for both sides; the Separatists rumor (RumorD4) pays 2VP and the Unified Monarchists rumor (RumorD3) 2
    // Resources to the players who voted for that faction (originally $sep / $mon "win" / "lose").
    // TakeEventFlag ("Change of Allegiance") is dead in the source ($takereact is never set) and not ported.
    private static void ResolutionofEvent_0(GlobalData globalData)
    {
        ATimeOfWarVars vars        = Vars(globalData);
        bool           separatists = vars.WarWinner == WarFaction.Separatists;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_SeparatistToken, PopUpButton.Confirm, TakingSidesLateSetup,
            content => content
                      .FormatWithReplacement(0, Winner(globalData))
                      .FormatWithReplacement(1, Loser(globalData))
                      .FormatWithCondition(2, () => separatists && vars.HeardSeparatists)
                      .FormatWithCondition(3, () => !separatists && vars.HeardMonarchists)
                      .FormatWithCondition(4, () => separatists && vars.HeardMonarchists)
                      .FormatWithCondition(5, () => !separatists && vars.HeardSeparatists));
    }

    #endregion Military Protection

    #region End of Generation I

    // Called by the Late end-of-round popup: Generation == Second, ActiveHub == null
    private static void GenerationOneEnd(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        if (vars.Barracks == ExtendedBool.True) TSBarracksPenalty(globalData);
        else if (vars.Showcase != 0) SeedGUNS(globalData);
        else GenerationOneWar(globalData);
    }

    private static void TSBarracksPenalty(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Vars(globalData).Showcase != 0 ? SeedGUNS : GenerationOneWar);
    }

    private static void SeedGUNS(GlobalData globalData)
    {
        int showcase = Vars(globalData).Showcase;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(content => content.FormatWithIndex(0, showcase));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, showcase));
        globalData.ActiveWindow.AddClickHereToContinue(SeedGUNS_0);
    }

    private static void SeedGUNS_0(GlobalData globalData)
    {
        int vp = Vars(globalData).RandomElement([2, 3], _RND_SHOWCASE_VP);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, GenerationOneWar,
            content => content.FormatWithReplacement(0, vp.ToString()));
    }

    // Generation II branch. The village name "Paradox" forces Time Travel, "Destruction" forces war (source easter egg,
    // port map 7, B29). War() already applies it on the first draw; it is applied again here in every case so a game
    // saved before that change still follows the code. Drawn in the click that shows the result (mirror mode).
    private static void GenerationOneWar(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        if (ForcedWar(globalData) is int forced) vars.War = forced;

        if (War(globalData) == 1) Warwarn(globalData);
        else GenerationOneWarEnd(globalData);
    }

    // War == 1: the town will be destroyed (-> Time Travel)
    private static void Warwarn(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(GenerationOneDevEnd);
    }

    // End of Generation text of TowardsWar (Unity S_OnEndOfGeneration overlay, shown before the passage)
    private static void GenerationOneWarEnd(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.EndOfGeneration, PopUpIcon.MFWlogo, PopUpButton.Confirm, TowardsWar);
    }

    // The source text with Barracks == False ("Against my wishes, the Barracks was built") is kept as shipped (port map 7, B4)
    private static void TowardsWar(GlobalData globalData)
    {
        bool barracks = Vars(globalData).Barracks == ExtendedBool.True;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Winner(globalData)).FormatWithCondition(1, () => barracks));
        globalData.ActiveWindow.AddClickHereToContinue(Vars(globalData).WarWinner == WarFaction.Separatists ? IntroMartialSeparatists : IntroMartialMonarchists);
    }

    // End of Generation text of TowardsDev (Unity S_OnEndOfGeneration overlay, shown before the passage)
    private static void GenerationOneDevEnd(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.EndOfGeneration, PopUpIcon.MFWlogo, PopUpButton.Confirm, TowardsDev);
    }

    private static void TowardsDev(GlobalData globalData)
    {
        bool barracks = Vars(globalData).Barracks == ExtendedBool.True;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Winner(globalData)).FormatWithCondition(1, () => barracks));
        globalData.ActiveWindow.AddClickHereToContinue(TimeTravelIntro);
    }

    #endregion End of Generation I
}
