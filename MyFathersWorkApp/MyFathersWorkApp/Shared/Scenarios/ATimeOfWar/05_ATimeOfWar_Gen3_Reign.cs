namespace MyFathersWorkApp;

// Generation III "Heavy is the Crown" (port map 2.4): reached when the Unified Monarchists win the war at the end of
// Generation II Martial. Intro, Crown token, royal decrees (Royal Advisor), Crown penalties, Wooden Figures, the
// benevolence / iron fist vote, the Commemorative Statue, the Church and the end-of-Generation bonuses before Scoring.
public static partial class ATimeOfWar
{
    // Decrees of the Royal Advisor (originally $advise; 5 and 9 do not exist). Decree 1 is repeatable, the others are
    // made once per game.
    private static readonly int[] _DECREES = [1, 2, 3, 4, 6, 7, 8];

    // Generic decree results (originally $genericr / $ch1 / $ch2): 14 texts in DecreeOption
    private const int _DECREE_OPTIONS = 14;

    // Originally $weaponrew
    private const int _WEAPON_REWARD_MONEY = 1;
    private const int _WEAPON_REWARD_VP    = 2;

    public static void MonarchReign(GlobalData globalData)
    {
        // Round 1 -> MonarchReign1
        // Round 2 -> MonarchReign2
        // Round 3 -> MonarchReign3
        ATimeOfWarVars vars  = Vars(globalData);
        Years          years = ReignYears(globalData);

        vars.HubId           = ATimeOfWarHubId.MonarchReign;
        globalData.ActiveHub = new GameplayHub(globalData);
        globalData.ActiveHub.SetDefaultTitle();
        globalData.ActiveHub.SetSubtitle(years);

        // Late Years, after the Commemorative Statue was built (originally $pageturn)
        bool statueShown = years == Years.Late && vars.StatueBuilt;

        const string       statueSection = "Statue";
        GameplayHubSection statue        = globalData.ActiveHub.AddSection(statueSection);
        statue.ReplaceShouldShow(() => statueShown);
        statue.AddDefaultContent(statueSection);
        statue.AddSpecialClickHere(statueSection, CommemorativeSignIn);

        const string       churchSection = "Church";
        GameplayHubSection church        = globalData.ActiveHub.AddSection(churchSection);
        church.ReplaceShouldShow(() => statueShown);
        church.AddSpecialClickHere(churchSection, ATOWChurch);
        church.AddDefaultContent(churchSection);

        // Originally $reignsep: 0 Self acts once without a Monarchist Flag, "yes" (crack down) cannot visit, "no" removed
        const string       allegianceSection = "Allegiance";
        GameplayHubSection allegiance        = globalData.ActiveHub.AddSection(allegianceSection);
        allegiance.ReplaceShouldShow(() => vars.SeparatistsBanned != ExtendedBool.False);
        allegiance.AddDefaultContent(allegianceSection, content => content.FormatWithCondition(0, () => vars.SeparatistsBanned == ExtendedBool.True));

        const string       decreeSection = "RoyalDecree";
        GameplayHubSection decree        = globalData.ActiveHub.AddSection(decreeSection, true);
        decree.AddDefaultContent(decreeSection);
        decree.AddSpecialClickHere(decreeSection, ReignIntro);

        // Middle / Late: the bonus chosen by the vote at the end of the Early Years ($decbon is drawn there)
        const string       mostSection = "MostDecrees";
        GameplayHubSection most        = globalData.ActiveHub.AddSection(mostSection);
        if (years == Years.Early) most.AddDefaultContent(mostSection);
        else
        {
            int bonus = DecreeBonus(globalData);
            most.AddNextContent(1, mostSection, false, content => content
                                                                 .FormatWithIndex(0, vars.Benevolent == ExtendedBool.False ? 1 : 0)
                                                                 .FormatWithReplacement(1, bonus.ToString()));
        }

        const string       shadowSection = "WhisperingShadow";
        GameplayHubSection shadow        = globalData.ActiveHub.AddSection(shadowSection);
        shadow.AddDefaultContent(shadowSection);

        const string       noticeSection = "RoyalsTakeNotice";
        GameplayHubSection notice        = globalData.ActiveHub.AddSection(noticeSection);
        notice.AddDefaultContent(noticeSection);
        notice.AddSpecialClickHere(noticeSection, vars.CrownPenalties == 0 ? FirstCrownCons : CrownCons);

        const string       eyeSection = "RoyalEye";
        GameplayHubSection eye        = globalData.ActiveHub.AddSection(eyeSection);
        eye.ReplaceShouldShow(() => vars.CrownPenalties > 0);
        eye.AddDefaultContent(eyeSection);

        const string       weaponSection = "WeaponBonus";
        GameplayHubSection weapon        = globalData.ActiveHub.AddSection(weaponSection);
        weapon.ReplaceShouldShow(() => vars.WeaponReward != 0);
        weapon.AddDefaultContent(weaponSection, content => content.FormatWithCondition(0, () => vars.WeaponReward == _WEAPON_REWARD_MONEY));

        const string       endSection = "End";
        GameplayHubSection end        = globalData.ActiveHub.AddSection(endSection);
        end.ReplaceShouldShow(() => years != Years.Late);
        end.AddSpecialClickHere(endSection, MonarchReign_0, endOfRound: true);

        const string       endLateSection = "EndLate";
        GameplayHubSection endLate        = globalData.ActiveHub.AddSection(endLateSection);
        endLate.ReplaceShouldShow(() => years == Years.Late);
        endLate.AddSpecialClickHere(endLateSection, MonarchReign_0, endOfRound: true);
    }

    private static void MonarchReign_0(GlobalData globalData)
    {
        globalData.ShowEndOfRoundPopUp(ReignYears(globalData) switch
        {
            Years.Early  => StickfunCheck,
            Years.Middle => StatueCheck,
            _            => WeaponsCheck
        });
    }

    // Round of Generation III. After the Late end-of-round popup Generation is 3 (outside the enum): the Late Years.
    private static Years ReignYears(GlobalData globalData)
    {
        return globalData.Generation == Generation.Third ? globalData.Years : Years.Late;
    }

    // Originally $decbon: VP for the most benevolent / iron fist decrees
    private static int DecreeBonus(GlobalData globalData)
    {
        return Vars(globalData).RandomElement([5, 6, 7, 8], _RND_DECREE_BONUS);
    }

    // Name links showing the regal title (Reign7c): the source renamed $nameX
    private static void AddPlayerLinks(GlobalData globalData, Action<GlobalData, string> callback)
    {
        foreach (string player in globalData.GetActivePlayers())
        {
            globalData.ActiveWindow!.AddCustomLink(DisplayName(globalData, player), data => callback(data, player));
        }
    }

    #region Intro

    // Originally MonarchReignIntro. The source draws $decbon and $commtrigger here; they are drawn where they are shown
    // (CoWEventVote / CoWEventBid2P) or used (StatueCheck). "debug" town name "v1.0" dropped (port map 7, B29).
    private static void MonarchReignIntro(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.Tracker           -= 1;
        vars.Decrees           =  0;
        vars.CrownPenalties    =  0;
        vars.SeparatistsBanned =  ExtendedBool.None;
        vars.WeaponReward      =  0;
        vars.Benevolent        =  ExtendedBool.None;
        vars.DecreeUsed        =  new bool[9];
        vars.StatueBuilt       =  false;
        vars.StatueOwner       =  string.Empty;

        foreach (string player in globalData.GetActivePlayers())
        {
            vars.DecreesGood[player] = 0;
            vars.DecreesEvil[player] = 0;
        }

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("MonarchReignIntro");
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_III);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ReignBarnCheck);
    }

    // The Suspicious Barn was built for the Separatists (the Generation I losers) in Generation II
    private static void ReignBarnCheck(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        if (vars.WarWinner == WarFaction.Separatists && vars.SuspiciousBarn) RoyalBarnCheck(globalData);
        else BribeDeniedCheck(globalData);
    }

    private static void RoyalBarnCheck(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(RoyalBarnCheck_0);
    }

    private static void RoyalBarnCheck_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_EstateUpgradeBack, PopUpButton.Confirm, BribeDeniedCheck);
    }

    // The bribe of Generation II succeeded: Weapon tokens no longer replace Ingredients
    private static void BribeDeniedCheck(GlobalData globalData)
    {
        if (Vars(globalData).Bribe == ExtendedBool.True) BribeDenied(globalData);
        else RoyalDecreeExplain(globalData);
    }

    private static void BribeDenied(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(BribeDenied_0);
    }

    private static void BribeDenied_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.S3_WeaponToken, PopUpButton.Confirm, RoyalDecreeExplain);
    }

    private static void RoyalDecreeExplain(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content
                                                             .FormatWithIndex(0, vars.Flavor("RoyalDecreeExplain#0", 4))
                                                             .FormatWithIndex(1, vars.Flavor("RoyalDecreeExplain#1", 5)));
        globalData.ActiveWindow.AddClickHereToContinue(RoyalPriceisRight);
    }

    private static void RoyalPriceisRight(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(RoyalPriceisRight_0);
    }

    private static void RoyalPriceisRight_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_CrownToken, PopUpButton.Confirm, MonarchReignSetup,
            content => content.FormatWithReplacement(0, Vars(globalData).Tracker.ToString()));
    }

    // Originally the setupStyle block of MonarchReign1 (guarded by $gen3pg; shown once at the start of the round)
    private static void MonarchReignSetup(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.VillageChronicleCover, PopUpButton.Confirm, MonarchReign,
            content => content.FormatWithCondition(0, () => globalData.PlayersNum == 3));
    }

    #endregion Intro

    #region Crown penalties

    // Originally 1stCrownCons: the first player to overlap or exceed the Crown token
    private static void FirstCrownCons(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(FirstCrownCons_0);
    }

    private static void FirstCrownCons_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).CrownPenalties += 1;
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, MonarchReign);
    }

    private static void CrownCons(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CrownCons_0);
    }

    private static void CrownCons_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).CrownPenalties += 1;
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.Creepy_Icon, PopUpButton.Confirm, MonarchReign);
    }

    #endregion Crown penalties

    #region Royal decrees

    // Visit with the Royal Advisor. Source bug B14 fixed: $royaladvisor was never incremented, so the first-visit text
    // always showed; it now shows on the first decree only.
    private static void ReignIntro(GlobalData globalData)
    {
        bool first = Vars(globalData).Decrees == 0;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => first));
        AddPlayerLinks(globalData, (data, player) =>
        {
            Vars(data).DecreePlayer = player;
            ReignHUB(data);
        });
    }

    // Originally ReignHUB (no screen): a random decree from the unused ones, drawn in the name click so mirror mode asks
    // when the decree is shown
    private static void ReignHUB(GlobalData globalData)
    {
        ATimeOfWarVars vars    = Vars(globalData);
        List<int>      options = _DECREES.Where(d => d == 1 || !vars.DecreeUsed[d]).ToList();
        int            decree  = vars.RandomElement(options, _RND_DECREE + Math.Min(vars.Decrees, 9));

        vars.Decrees += 1;
        if (decree != 1) vars.DecreeUsed[decree] = true;

        switch (decree)
        {
            case 2:  Reign2(globalData); break;
            case 3:  Reign3(globalData); break;
            case 4:  Reign4(globalData); break;
            case 6:  Reign6(globalData); break;
            case 7:  Reign7(globalData); break;
            case 8:  Reign8(globalData); break;
            default: Reign1(globalData); break;
        }
    }

    // Random values of the current decree (n = decree number - 1, capped like every repeatable draw)
    private static int DecreeSlot(GlobalData globalData) => Math.Min(Vars(globalData).Decrees - 1, 9);

    // Originally $crownL / $crownR: "" or "Move the Crown token 1 space to the left / right."
    private static string DecreeCrown(GlobalData globalData, bool right)
    {
        bool move = Vars(globalData).RandomBool(_RND_CROWN + DecreeSlot(globalData) * 2 + (right ? 1 : 0));
        return move ? globalData.GetScenarioLocalizedTag(right ? "DecreeCrownRight" : "DecreeCrownLeft") : string.Empty;
    }

    // Originally $ch1: one of the 14 generic results (RandomElement, not RandomInRange: mirror mode then lists the texts
    // instead of asking for a number that is not shown)
    private static int DecreeOption1(GlobalData globalData)
    {
        return Vars(globalData).RandomElement(Enumerable.Range(0, _DECREE_OPTIONS).ToList(), _RND_DECREE_OPTION + DecreeSlot(globalData) * 2);
    }

    // Originally $ch2: distinct from $ch1 when the decree shows both (Reign1), otherwise any of the 14
    private static int DecreeOption2(GlobalData globalData, bool distinct)
    {
        int       first   = distinct ? DecreeOption1(globalData) : -1;
        List<int> options = Enumerable.Range(0, _DECREE_OPTIONS).Where(option => option != first).ToList();
        return Vars(globalData).RandomElement(options, _RND_DECREE_OPTION + DecreeSlot(globalData) * 2 + 1);
    }

    private static string DecreeOptionText(GlobalData globalData, int option)
    {
        return globalData.GetScenarioLocalizedTag("DecreeOption").FormatWithIndex(0, option);
    }

    // "Carefully hand the Storybook to X. This passage is then read aloud to all players." (bold line before the title)
    private static string DecreeHandOver(GlobalData globalData)
    {
        return globalData.GetScenarioLocalizedTag("DecreeHandOver").FormatWithReplacement(0, DisplayName(globalData, Vars(globalData).DecreePlayer));
    }

    // Reminder at the end of every decree result popup
    private static string DecreeReminder(GlobalData globalData) => globalData.GetScenarioLocalizedTag("DecreeReminder");

    // Originally $rc and pl?reigng / pl?reigne: the decree helped (people) or hurt the people
    private static void DecreeChosen(GlobalData globalData, bool people, Action<GlobalData> next)
    {
        ATimeOfWarVars vars = Vars(globalData);
        vars.DecreeChoice = people ? 1 : 2;

        if (people) vars.DecreesGood[vars.DecreePlayer] += 1;
        else vars.DecreesEvil[vars.DecreePlayer] += 1;

        next(globalData);
    }

    private static bool DecreeForThePeople(GlobalData globalData) => Vars(globalData).DecreeChoice == 1;

    // Royal Decision / Dilemma / Conundrum (repeatable)
    private static void Reign1(GlobalData globalData)
    {
        string crownL = DecreeCrown(globalData, false);
        string crownR = DecreeCrown(globalData, true);
        int    ch1    = DecreeOption1(globalData);
        int    ch2    = DecreeOption2(globalData, true);

        ATimeOfWarVars vars = Vars(globalData);
        string         key  = "Reign1#" + vars.Decrees;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => DecreeHandOver(globalData) + content
                                                                                         .FormatWithIndex(0, vars.Flavor(key + "#Title", 3))
                                                                                         .FormatWithIndex(1, vars.Flavor(key + "#Opening", 3)));
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => DecreeChosen(data, true, Reign1b), data => DecreeChosen(data, false, Reign1b)], true, content => content
            .FormatWithReplacement(0, crownL)
            .FormatWithReplacement(1, DecreeOptionText(globalData, ch1))
            .FormatWithReplacement(2, crownR)
            .FormatWithReplacement(3, DecreeOptionText(globalData, ch2)));
    }

    private static void Reign1b(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, vars.Flavor("Reign1b#" + vars.Decrees, 3)));
        globalData.ActiveWindow.AddClickHereToContinue(Reign1b_0);
    }

    private static void Reign1b_0(GlobalData globalData)
    {
        bool   people = DecreeForThePeople(globalData);
        string crown  = DecreeCrown(globalData, !people);
        int    option = people ? DecreeOption1(globalData) : DecreeOption2(globalData, true);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.MFWlogo, PopUpButton.Confirm, MonarchReign,
            content => content.FormatWithReplacement(0, crown).FormatWithReplacement(1, DecreeOptionText(globalData, option)) + DecreeReminder(globalData));
    }

    // Drifters
    private static void Reign2(GlobalData globalData)
    {
        string crownL = DecreeCrown(globalData, false);
        string crownR = DecreeCrown(globalData, true);
        int    ch2    = DecreeOption2(globalData, false);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => DecreeHandOver(globalData) + content);
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => DecreeChosen(data, true, Reign2b), data => DecreeChosen(data, false, Reign2b)], true, content => content
            .FormatWithReplacement(0, crownL)
            .FormatWithReplacement(1, crownR)
            .FormatWithReplacement(2, DecreeOptionText(globalData, ch2)));
    }

    private static void Reign2b(GlobalData globalData)
    {
        bool people = DecreeForThePeople(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => people));
        globalData.ActiveWindow.AddClickHereToContinue(Reign2b_0);
    }

    private static void Reign2b_0(GlobalData globalData)
    {
        bool   people = DecreeForThePeople(globalData);
        string crown  = DecreeCrown(globalData, !people);
        string option = people ? string.Empty : DecreeOptionText(globalData, DecreeOption2(globalData, false));

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.MFWlogo, PopUpButton.Confirm, MonarchReign,
            content => content.FormatWithCondition(0, () => people).FormatWithReplacement(1, crown).FormatWithReplacement(2, option) + DecreeReminder(globalData));
    }

    // The Hunting Grounds
    private static void Reign3(GlobalData globalData)
    {
        string crownL = DecreeCrown(globalData, false);
        string crownR = DecreeCrown(globalData, true);
        int    ch2    = DecreeOption2(globalData, false);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => DecreeHandOver(globalData) + content);
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => DecreeChosen(data, true, Reign3b), data => DecreeChosen(data, false, Reign3b)], true, content => content
            .FormatWithReplacement(0, crownL)
            .FormatWithReplacement(1, crownR)
            .FormatWithReplacement(2, DecreeOptionText(globalData, ch2)));
    }

    private static void Reign3b(GlobalData globalData)
    {
        bool people = DecreeForThePeople(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => people));
        globalData.ActiveWindow.AddClickHereToContinue(Reign3b_0);
    }

    private static void Reign3b_0(GlobalData globalData)
    {
        bool   people = DecreeForThePeople(globalData);
        string crown  = DecreeCrown(globalData, !people);
        string option = people ? string.Empty : DecreeOptionText(globalData, DecreeOption2(globalData, false));

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.MFWlogo, PopUpButton.Confirm, MonarchReign,
            content => content.FormatWithCondition(0, () => people).FormatWithReplacement(1, crown).FormatWithReplacement(2, option) + DecreeReminder(globalData));
    }

    // Freedom of Speech: sets the Monarchist Allegiance rule (originally $reignsep, set in the result popup)
    private static void Reign4(GlobalData globalData)
    {
        string crownL = DecreeCrown(globalData, false);
        string crownR = DecreeCrown(globalData, true);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => DecreeHandOver(globalData) + content);
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => DecreeChosen(data, true, Reign4b), data => DecreeChosen(data, false, Reign4b)], true, content => content
            .FormatWithReplacement(0, crownL)
            .FormatWithReplacement(1, crownR));
    }

    private static void Reign4b(GlobalData globalData)
    {
        bool people = DecreeForThePeople(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => people));
        globalData.ActiveWindow.AddClickHereToContinue(Reign4b_0);
    }

    private static void Reign4b_0(GlobalData globalData)
    {
        bool   people = DecreeForThePeople(globalData);
        string crown  = DecreeCrown(globalData, !people);

        globalData.SaveToUndo();
        Vars(globalData).SeparatistsBanned = people ? ExtendedBool.False : ExtendedBool.True;

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.MFWlogo, PopUpButton.Confirm, MonarchReign,
            content => content.FormatWithCondition(0, () => people).FormatWithReplacement(1, crown) + DecreeReminder(globalData));
    }

    // Spiritualism Craze
    private static void Reign6(GlobalData globalData)
    {
        string crownL = DecreeCrown(globalData, false);
        string crownR = DecreeCrown(globalData, true);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => DecreeHandOver(globalData) + content);
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => DecreeChosen(data, true, Reign6b), data => DecreeChosen(data, false, Reign6b)], true, content => content
            .FormatWithReplacement(0, crownL)
            .FormatWithReplacement(1, crownR));
    }

    private static void Reign6b(GlobalData globalData)
    {
        bool people = DecreeForThePeople(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => people));
        globalData.ActiveWindow.AddClickHereToContinue(Reign6b_0);
    }

    private static void Reign6b_0(GlobalData globalData)
    {
        bool   people = DecreeForThePeople(globalData);
        string crown  = DecreeCrown(globalData, !people);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.MFWlogo, PopUpButton.Confirm, MonarchReign,
            content => content.FormatWithCondition(0, () => people).FormatWithReplacement(1, crown) + DecreeReminder(globalData));
    }

    // The Choice of Recognition: a generic result or a regal title
    private static void Reign7(GlobalData globalData)
    {
        string crownL = DecreeCrown(globalData, false);
        string crownR = DecreeCrown(globalData, true);
        int    ch1    = DecreeOption1(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => DecreeHandOver(globalData) + content);
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => DecreeChosen(data, true, Reign7b), data => DecreeChosen(data, false, Reign7c)], true, content => content
            .FormatWithReplacement(0, crownL)
            .FormatWithReplacement(1, DecreeOptionText(globalData, ch1))
            .FormatWithReplacement(2, crownR));
    }

    private static void Reign7b(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(Reign7b_0);
    }

    private static void Reign7b_0(GlobalData globalData)
    {
        string crown  = DecreeCrown(globalData, false);
        int    option = DecreeOption1(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.MFWlogo, PopUpButton.Confirm, MonarchReign,
            content => content.FormatWithReplacement(0, crown).FormatWithReplacement(1, DecreeOptionText(globalData, option)) + DecreeReminder(globalData));
    }

    // The regal title is kept per player and shown before the name in later story text (port map 7, B15)
    private static void Reign7c(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveInputPopup = new GameplayInputPopup(globalData, string.Empty, PopUpButton.Confirm, value => value.Trim().Length is > 0 and <= 40, value =>
        {
            ATimeOfWarVars vars = Vars(globalData);
            vars.Titles[vars.DecreePlayer] = value.Trim();
            Reign7c2(globalData);
        });
    }

    private static void Reign7c2(GlobalData globalData)
    {
        string name = DisplayName(globalData, Vars(globalData).DecreePlayer);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(title => title.FormatWithReplacement(0, name));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, name));
        globalData.ActiveWindow.AddClickHereToContinue(Reign7c2_0);
    }

    private static void Reign7c2_0(GlobalData globalData)
    {
        string crown = DecreeCrown(globalData, true);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.MFWlogo, PopUpButton.Confirm, MonarchReign,
            content => content.FormatWithReplacement(0, crown) + DecreeReminder(globalData));
    }

    // Weapon Technology: Weapon tokens become money or VP (originally $weaponrew, set in the result popup)
    private static void Reign8(GlobalData globalData)
    {
        string crownL = DecreeCrown(globalData, false);
        string crownR = DecreeCrown(globalData, true);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultContent(content => DecreeHandOver(globalData) + content);
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => DecreeChosen(data, true, Reign8b), data => DecreeChosen(data, false, Reign8b)], true, content => content
            .FormatWithReplacement(0, crownL)
            .FormatWithReplacement(1, crownR));
    }

    private static void Reign8b(GlobalData globalData)
    {
        bool people = DecreeForThePeople(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => people));
        globalData.ActiveWindow.AddClickHereToContinue(Reign8b_0);
    }

    private static void Reign8b_0(GlobalData globalData)
    {
        bool   people = DecreeForThePeople(globalData);
        string crown  = DecreeCrown(globalData, !people);

        globalData.SaveToUndo();
        Vars(globalData).WeaponReward = people ? _WEAPON_REWARD_MONEY : _WEAPON_REWARD_VP;

        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.MFWlogo, PopUpButton.Confirm, MonarchReign,
            content => content.FormatWithCondition(0, () => people).FormatWithReplacement(1, crown) + DecreeReminder(globalData));
    }

    #endregion Royal decrees

    #region End of the Early Years

    // Runs after the Early end-of-round popup (Years == Middle): the Wooden Figures of Generation I (originally $crazy)
    private static void StickfunCheck(GlobalData globalData)
    {
        if (Vars(globalData).WoodenFigures == 0) CoWEvent(globalData);
        else Stickfun(globalData);
    }

    private static void Stickfun(GlobalData globalData)
    {
        bool combined = Vars(globalData).WoodenFigures > 1;

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContent(combined ? 1 : 2, true);
        globalData.ActiveWindow.AddClickHereToContinue(Stickfun_0);
    }

    private static void Stickfun_0(GlobalData globalData)
    {
        bool combined = Vars(globalData).WoodenFigures > 1;

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, combined ? PopUpIcon.S3_WeaponToken : PopUpIcon.S3_WoodenFigureToken, PopUpButton.Confirm, CoWEvent,
            content => content.FormatWithCondition(0, () => combined));
    }

    private static void CoWEvent(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, globalData.TownName));
        globalData.ActiveWindow.AddClickHereToContinue(globalData.PlayersNum == 2 ? CoWEventBid2P : CoWEventVote);
    }

    // Originally CoWEventb
    private static void CoWEventVote(GlobalData globalData)
    {
        int bonus = DecreeBonus(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, bonus.ToString()));
        globalData.ActiveWindow.AddClickHereAfterVote(CoWRes);
    }

    // Originally 2PCoWEvent. Source bug B16 fixed: the 2-player text said benevolence moves the Crown to the right and
    // iron fist to the left, the opposite of the vote text and the decrees.
    private static void CoWEventBid2P(GlobalData globalData)
    {
        int bonus = DecreeBonus(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, bonus.ToString()));
        globalData.ActiveWindow.AddClickHereAfterBid(CoWRes2P);
    }

    // Originally COWRes
    private static void CoWRes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => CoWChosen(data, true), data => CoWChosen(data, false)], true);
    }

    // Originally 2PCoWRes
    private static void CoWRes2P(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddNextContentWithLinks(1, [data => CoWChosen(data, true), data => CoWChosen(data, false)], true);
    }

    private static void CoWChosen(GlobalData globalData, bool benevolent)
    {
        Vars(globalData).Benevolent = benevolent ? ExtendedBool.True : ExtendedBool.False;
        MonarchReign(globalData);
    }

    #endregion End of the Early Years

    #region Commemorative Statue

    // Runs after the Middle end-of-round popup (Years == Late). Originally $advisorcount >= $commtrigger (2p 2-4,
    // 3p 3-5, 4p 4-6); the trigger is only drawn when the decree count does not decide it already.
    private static void StatueCheck(GlobalData globalData)
    {
        ATimeOfWarVars vars    = Vars(globalData);
        int            players = globalData.PlayersNum;

        bool statue = vars.Decrees >= players + 2 ||
                      (vars.Decrees >= players && vars.Decrees >= vars.RandomInRange(players, players + 2, _RND_STATUE_TRIGGER));

        if (statue) CommemorativeStatueEvent(globalData);
        else MonarchReign(globalData);
    }

    private static void CommemorativeStatueEvent(GlobalData globalData)
    {
        globalData.SaveToUndo();
        Vars(globalData).StatueBuilt = true;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CommemorativeStatueEventb);
    }

    private static void CommemorativeStatueEventb(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereAfterBid(CommemorativeStatueEventRes);
    }

    private static void CommemorativeStatueEventRes(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        AddPlayerLinks(globalData, (data, player) =>
        {
            Vars(data).StatueOwner = player;
            CommemorativeStatueEventResb(data);
        });
    }

    // The source passage is only the setup block
    private static void CommemorativeStatueEventResb(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, MonarchReign,
            content => content.FormatWithReplacement(0, DisplayName(globalData, Vars(globalData).StatueOwner)));
    }

    private static void CommemorativeSignIn(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        AddPlayerLinks(globalData, (data, player) =>
        {
            ATimeOfWarVars vars = Vars(data);
            vars.SelectedPlayer = player;

            if (player == vars.StatueOwner) CommemorativeMAIN(data);
            else CommemorativeOTHER(data);
        });
    }

    // The statue owner visits their own statue
    private static void CommemorativeMAIN(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(CommemorativeMAIN_0);
    }

    private static void CommemorativeMAIN_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S3_CrownToken, PopUpButton.Confirm, MonarchReign,
            content => content.FormatWithReplacement(0, DisplayName(globalData, Vars(globalData).SelectedPlayer)));
    }

    // Another player visits the statue
    private static void CommemorativeOTHER(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, vars.Flavor("CommemorativeOTHER#" + vars.SelectedPlayer, 2)));
        globalData.ActiveWindow.AddClickHereToContinue(CommemorativeOTHER_0);
    }

    private static void CommemorativeOTHER_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.SearchDeck_Icon, PopUpButton.Confirm, MonarchReign,
            content => content.FormatWithReplacement(0, DisplayName(globalData, Vars(globalData).SelectedPlayer)));
    }

    // Originally ATOW-Church. The costs are printed "X/X/X" in the source (port map 7, B17: kept); its return links to
    // Warning2 / Warning3 are dead, only the Reign hub links here.
    private static void ATOWChurch(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("ClickToReturn"), MonarchReign);
    }

    #endregion Commemorative Statue

    #region End of Generation III

    // Runs after the Late end-of-round popup (Generation == 3): Weapon tokens are worth 1VP when their sale was restricted
    private static void WeaponsCheck(GlobalData globalData)
    {
        if (Vars(globalData).WeaponReward == _WEAPON_REWARD_VP) Weapons(globalData);
        else ShadowBonus(globalData);
    }

    private static void Weapons(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ShadowBonus);
    }

    // The 6VP Crown bonus. Ending (port map 8): more Crown penalties than 0 (2p) / 1 (3p) / 2 (4p) -> 6 "Tipping the
    // Scales", else 5 "Royal Praise".
    private static void ShadowBonus(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.Ending = vars.CrownPenalties > globalData.PlayersNum - 2 ? 6 : 5;

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(BenevolenceBonus);
    }

    // Decrees of the chosen kind per player (originally pl?reigng / pl?reigne)
    private static int ChosenDecrees(GlobalData globalData, string player)
    {
        ATimeOfWarVars vars = Vars(globalData);
        return vars.Benevolent == ExtendedBool.False ? vars.DecreesEvil[player] : vars.DecreesGood[player];
    }

    // Source bug B18 fixed: the player with more decrees of the chosen kind than every other player wins; any tie
    // rewards nobody (the source comparison could name a winner despite a tie). Empty = nobody.
    private static string MostDecreesPlayer(GlobalData globalData)
    {
        string[] players = globalData.GetActivePlayers();
        int      most    = players.Max(player => ChosenDecrees(globalData, player));
        string[] best    = players.Where(player => ChosenDecrees(globalData, player) == most).ToArray();
        return best.Length == 1 ? best[0] : string.Empty;
    }

    private static void BenevolenceBonus(GlobalData globalData)
    {
        bool   evil = Vars(globalData).Benevolent == ExtendedBool.False;
        string line = globalData.GetScenarioLocalizedTag("BenevolenceBonus_Player");
        string list = string.Concat(globalData.GetActivePlayers().Select(player => line
                                                                                  .FormatWithReplacement(0, DisplayName(globalData, player))
                                                                                  .FormatWithReplacement(1, ChosenDecrees(globalData, player).ToString())
                                                                                  .FormatWithCondition(2, () => evil))).TrimTrailingBreaks();

        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle(title => title.FormatWithCondition(0, () => evil));
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithCondition(0, () => evil));
        globalData.ActiveWindow.AddCustomContent(list, true);
        globalData.ActiveWindow.AddCustomLink(globalData.GetScenarioLocalizedTag("BenevolenceBonus_Continue"), BenevolenceBonus_0);
    }

    private static void BenevolenceBonus_0(GlobalData globalData)
    {
        string player = MostDecreesPlayer(globalData);
        string winner = player == string.Empty ? globalData.GetScenarioLocalizedTag("BenevolenceBonus_Tied") : DisplayName(globalData, player);
        int    bonus  = DecreeBonus(globalData);

        globalData.SaveToUndo();
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, PopUpIcon.ScoreTrackMarker, PopUpButton.Confirm, Scoring,
            content => content.FormatWithReplacement(0, winner).FormatWithReplacement(1, bonus.ToString()));
    }

    #endregion End of Generation III
}
