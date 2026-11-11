namespace MyFathersWorkApp;

// A Time of War - port map: docs/ATOW_PORT_MAP.md
public static partial class ATimeOfWar
{
    // RandomArray indices - fixed per decision so undo / load / mirror mode replay identically (port map section 4.1)
    private const int _RND_WAR_WINNER        = 0;
    private const int _RND_WAR               = 1;
    private const int _RND_QUARTER           = 2;
    private const int _RND_BARRACKS_ACT      = 3;
    private const int _RND_TRACKER           = 4;
    private const int _RND_FIRST_PLAYER      = 5;
    private const int _RND_SHOWCASE_1        = 6;
    private const int _RND_SHOWCASE_2        = 7;
    private const int _RND_SHOWCASE_VP       = 8;
    private const int _RND_RUMOR_REWARD      = 10;  // 10 - 15, + rumor - 1 (14, the hunched figure, is unused)
    private const int _RND_RUMOR_OPTIONS     = 20;  // 20 - 39, + min(visit, 9) * 2 + 0 / 1
    private const int _RND_MARTIAL_TRACKER   = 40;
    private const int _RND_SEP_INC1          = 41;
    private const int _RND_SEP_INC2          = 42;
    private const int _RND_SEP_INC3          = 43;
    private const int _RND_MON_INC3          = 44;
    private const int _RND_WAR_DETERMINE     = 45;
    private const int _RND_DEFENSE           = 46;
    private const int _RND_HEAT_REWARD       = 47;
    private const int _RND_SEP_EVENT         = 48;
    private const int _RND_QUARTER_VP        = 49;
    private const int _RND_BRIBE_TARGET      = 50;
    private const int _RND_RESIST_FLAGS      = 51;
    private const int _RND_RESIST_END        = 52;
    private const int _RND_BARRACKS_PENALTY  = 53;  // 53 - 55, + Years
    private const int _RND_BARRACKS_AMOUNT   = 56;  // 56 - 58, + Years
    private const int _RND_SABOTAGE_DISCARD  = 59;  // 59 - 60, + Years
    private const int _RND_DUEL_FIELD        = 61;  // 61 - 62, + Years
    private const int _RND_DUEL_SHOT         = 63;  // 63 - 64, + Years
    private const int _RND_DUEL_DEATH        = 65;  // 65 - 66, + Years
    private const int _RND_MENDING_VP        = 90;
    private const int _RND_TIME_END          = 91;
    private const int _RND_DOX               = 92;
    private const int _RND_DECREE_BONUS      = 100;
    private const int _RND_STATUE_TRIGGER    = 101;
    private const int _RND_DECREE            = 110; // 110 - 119, + min(Decrees, 9)
    private const int _RND_DECREE_OPTION     = 120; // 120 - 139, + min(n, 9) * 2 + 0 / 1
    private const int _RND_CROWN             = 140; // 140 - 159, + min(n, 9) * 2 + 0 (left) / 1 (right)
    private const int _RND_WEAPON_LIMIT      = 200;
    private const int _RND_WEAPON_INCOME     = 201;
    private const int _RND_SHIFT_LIMIT       = 210;
    private const int _RND_FIGURE_FORTUNE    = 211;
    private const int _RND_WORLD             = 220; // 220 - 229, + min(WorldShifts, 9)
    private const int _RND_SHIFT_VP          = 230; // 230 - 239, + min(TimeShifts, 9)
    private const int _RND_ACCOLADE          = 260;
    private const int _RND_PARADOX_FIGURE    = 261;
    private const int _RND_TIME_MISTAKE      = 262;

    public static void Init(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithIndex(0, globalData.LocalizedPlayerNumberIndex()));
        globalData.ActiveWindow.AddClickHereToContinue(Foreword);
    }

    // Originally ForewordScen2
    private static void Foreword(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(ScenarioStart);
    }

    // Originally ATimeofWarIntro. The source draws $war, $warwinner, $quarter and $barracksact here; the web app draws
    // each one on the screen that shows it (mirror mode).
    private static void ScenarioStart(GlobalData globalData)
    {
        globalData.SaveToUndo();
        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.SetVoiceOver("ATimeofWarIntro");
        globalData.ActiveWindow.AddGameplayTitle(GlobalTags.Gameplay_Generation_I);
        globalData.ActiveWindow.AddDefaultSubtitle();
        globalData.ActiveWindow.AddDefaultContent();
        globalData.ActiveWindow.AddClickHereToContinue(IntroConflict);
    }

    private static void IntroConflict(GlobalData globalData)
    {
        globalData.SaveToUndo();

        ATimeOfWarVars vars = Vars(globalData);
        vars.WarWinner = vars.RandomElement([WarFaction.Separatists, WarFaction.Monarchists], _RND_WAR_WINNER);

        globalData.ActiveWindow = new GameplayWindow(globalData);
        globalData.ActiveWindow.AddDefaultTitle();
        globalData.ActiveWindow.AddDefaultContent(content => content.FormatWithReplacement(0, Winner(globalData)));
        globalData.ActiveWindow.AddClickHereToContinue(IntroConflict_0);
    }

    private static void IntroConflict_0(GlobalData globalData)
    {
        globalData.SaveToUndo();
        string flag = Vars(globalData).WarWinner == WarFaction.Separatists ? PopUpIcon.S3_FlagSeparatists : PopUpIcon.S3_FlagUnitfiedMonarchists;
        globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.Setup, flag, PopUpButton.Confirm, TakingSidesEarlySetup,
            content => content.FormatWithReplacement(0, Winner(globalData)));
    }

    #region Helpers

    private static ATimeOfWarVars Vars(GlobalData globalData) => globalData.ATimeOfWarVars;

    // Faction the town supported in Generation I (originally $warwinner) and the other one ($warloser)
    private static WarFaction LoserFaction(GlobalData globalData)
    {
        return Vars(globalData).WarWinner == WarFaction.Separatists ? WarFaction.Monarchists : WarFaction.Separatists;
    }

    private static string FactionName(GlobalData globalData, WarFaction faction)
    {
        return globalData.GetScenarioLocalizedTag("Faction_Name").FormatWithIndex(0, (int)faction);
    }

    private static string Winner(GlobalData globalData) => FactionName(globalData, Vars(globalData).WarWinner);

    private static string Loser(GlobalData globalData) => FactionName(globalData, LoserFaction(globalData));

    // The winner's token (originally $crest: Sickle for the Separatists, Shield for the Unified Monarchists) and the
    // loser's token ($figure)
    private static bool CrestIsSickle(GlobalData globalData) => Vars(globalData).WarWinner == WarFaction.Separatists;

    private static string Crest(GlobalData globalData)
    {
        return globalData.GetScenarioLocalizedTag("Token_Name").FormatWithIndex(0, CrestIsSickle(globalData) ? 0 : 1);
    }

    private static string Figure(GlobalData globalData)
    {
        return globalData.GetScenarioLocalizedTag("Token_Name").FormatWithIndex(0, CrestIsSickle(globalData) ? 1 : 0);
    }

    private static string CrestIcon(GlobalData globalData) => CrestIsSickle(globalData) ? PopUpIcon.S3_SickleToken : PopUpIcon.S3_ShieldToken;

    private static string FigureIcon(GlobalData globalData) => CrestIsSickle(globalData) ? PopUpIcon.S3_ShieldToken : PopUpIcon.S3_SickleToken;

    // Generation II branch (originally $war): 1 Time Travel, 2 Martial. Drawn on the first screen that shows it. The
    // village names "Paradox" / "Destruction" force it before the draw (port map 7, B29), so every screen that reads it
    // early (e.g. RumorD3Revealed) agrees with the forced branch - the designer's "known bug" was a late override.
    private static int War(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);
        if (vars.War == 0) vars.War = ForcedWar(globalData) ?? vars.RandomElement([1, 2], _RND_WAR);
        return vars.War;
    }

    // Village-name secret (Help page): the Generation II branch it forces, or null.
    private static int? ForcedWar(GlobalData globalData) => globalData.TownName switch
    {
        ATimeOfWarVars.TIME_TRAVEL_TOWN_NAME => 1,
        ATimeOfWarVars.MARTIAL_TOWN_NAME     => 2,
        _                                    => null,
    };

    // Separatists only (originally $quarter): troops will be quartered by force. Drawn on the first screen that shows it.
    private static bool Quarter(GlobalData globalData)
    {
        ATimeOfWarVars vars = Vars(globalData);
        if (vars.Quarter == ExtendedBool.None) vars.Quarter = vars.RandomElement([ExtendedBool.True, ExtendedBool.False], _RND_QUARTER);
        return vars.Quarter == ExtendedBool.True;
    }

    // Player name with the regal title chosen in Heavy is the Crown (port map 7, B15)
    private static string DisplayName(GlobalData globalData, string player)
    {
        string title = Vars(globalData).Titles.GetValueOrDefault(player, string.Empty);
        return title == string.Empty ? player : title + " " + player;
    }

    // Link label "When you have finished reading, click here to continue..."
    private static void AddFinishedReading(GlobalData globalData, Action<GlobalData> nextCallback)
    {
        globalData.ActiveWindow!.AddCustomLink(globalData.GetScenarioLocalizedTag("FinishedReading"), nextCallback);
    }

    #endregion Helpers
}
