// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace MyFathersWorkApp;

// RandomArray indices: see the _RND_ constants in 00_ATimeOfWar_Setup.cs. Field list: docs/ATOW_PORT_MAP.md section 3.
public class ATimeOfWarVars() : ScenarioVars(300)
{
    public ATimeOfWarHubId HubId   { get; set; }
    public int             Tracker { get; set; } // Suspicion Marker space

    // Generation I - Taking Sides
    public WarFaction   WarWinner       { get; set; } // faction the town supported (originally warwinner; warloser, crest, figure are derived)
    public int          War             { get; set; } // Gen II branch: 0 not drawn, 1 Time Travel, 2 Martial (lazy, see War())
    public ExtendedBool Quarter         { get; set; } = ExtendedBool.None; // Separatists only: troops are quartered (lazy, see Quarter())
    public int          TownHallVisits  { get; set; } // originally rumblings
    public bool         MetGuard        { get; set; } // originally rumor1
    public bool         MetGentleman    { get; set; } // originally rumor2
    public bool         ShowcaseChosen  { get; set; } // originally rumor6
    public bool         TownHallMeeting { get; set; } // originally nomore
    public int          TownHallIndex   { get; set; } // players who talked during the meeting (originally th)
    public int          WoodenFigures   { get; set; } // originally crazy
    public bool         HeardSeparatists { get; set; } // originally sep (win/lose derived from WarWinner)
    public bool         HeardMonarchists { get; set; } // originally mon
    public int          Showcase        { get; set; } // 0 none, 1 Knowledge, 2 Ingredients, 3 Money (originally gunsbonus)
    public ExtendedBool Barracks        { get; set; } = ExtendedBool.None; // True built, False voted against

    // Generation II - Martial
    public bool            Assessment          { get; set; } // originally iou
    public bool            WeaponsBonusClaimed { get; set; } // originally martweapons
    public string          SelectedPlayer      { get; set; } = string.Empty; // player picked for the current event (originally heat, signincomm)
    public MartialBuilding SabotagedBuilding   { get; set; } // originally bldg1
    public bool            OutpostDestroyed    { get; set; } // originally bldg2
    public int             ChroniclePage       { get; set; } // originally sabpage
    public int             BuildingsDestroyed  { get; set; } // originally wardestroy
    public bool            ChroniclePageMiddle { get; set; } // originally warpage2
    public string          SabotageTarget      { get; set; } = string.Empty; // originally sabotagee1 ("" = none)
    public string          Saboteur            { get; set; } = string.Empty; // originally playersabotage1
    public int             DuelType            { get; set; } // 1 science, 2 pistols
    public string          DuelLoser           { get; set; } = string.Empty;
    public ExtendedBool    Bribe               { get; set; } = ExtendedBool.None;
    public bool            SuspiciousBarn      { get; set; } // originally barn

    // Generation II - Time Travel
    public int                      FlagsBought           { get; set; } // originally buy
    public int                      BlameIndex            { get; set; } // originally counter
    public Dictionary<string, int>  BlameVotes            { get; set; } = new(); // originally pl1...
    public Dictionary<string, bool> Silent                { get; set; } = new(); // originally silentA...
    public string                   Blamed                { get; set; } = string.Empty;
    public bool                     AllSilent             { get; set; } // originally late == 2
    public bool                     TimeMachineMasterwork { get; set; } // originally tmmasterwork
    public bool                     TimeMachineRepaired   { get; set; } // originally tmrepair
    public bool                     MendingClaimed        { get; set; } // originally ttset
    public int                      RepairsDone           { get; set; } // originally tmnumber

    // Generation III - Heavy is the Crown
    public int                        Decrees           { get; set; } // originally advisorcount
    public int                        CrownPenalties    { get; set; } // originally crowncount / onestcrown
    public ExtendedBool               SeparatistsBanned { get; set; } = ExtendedBool.None; // True crack down, False allowed (originally reignsep)
    public int                        WeaponReward      { get; set; } // 0 none, 1 money, 2 VP (originally weaponrew)
    public ExtendedBool               Benevolent        { get; set; } = ExtendedBool.None; // True benevolence, False iron fist
    public string                     DecreePlayer      { get; set; } = string.Empty; // originally reigntemp
    public bool[]                     DecreeUsed        { get; set; } = new bool[9]; // originally r2...r8 (index = decree)
    public int                        DecreeChoice      { get; set; } // originally rc
    public Dictionary<string, int>    DecreesGood       { get; set; } = new(); // originally pl?reigng
    public Dictionary<string, int>    DecreesEvil       { get; set; } = new(); // originally pl?reigne
    public Dictionary<string, string> Titles            { get; set; } = new(); // regal title per player (Reign7c)
    public bool                       StatueBuilt       { get; set; } // originally pageturn
    public string                     StatueOwner       { get; set; } = string.Empty; // originally ATOWStatue

    // Generation III - Peace Through War
    public string       NewMeat { get; set; } = string.Empty;
    public bool         Meat    { get; set; } // the town chose the meat (originally peacecount)
    public ExtendedBool Gas     { get; set; } = ExtendedBool.None; // True released, False not (originally peac)

    // Generation III - Future Consequences
    public int  TimeShifts     { get; set; } // originally search
    public int  World          { get; set; } // Village Chronicle page 17 - 22, 0 = not drawn (originally cont / twcode)
    public int  PreviousWorld  { get; set; }
    public int  WorldShifts    { get; set; }
    public bool TimelineStable { get; set; } // originally stasis
    public bool MasterworkDone { get; set; } // originally master

    // Generation III - You Cant Stop Progress
    public int                      TimeMistake { get; set; } // 0 none, 1 - 8 (originally timemistake)
    public Dictionary<string, bool> Released    { get; set; } = new(); // originally platm...
    public int                      Releases    { get; set; } // originally release
    public int                      Giants      { get; set; }
    public int                      MonuIndex   { get; set; }

    // Endings
    public int Ending { get; set; } // 1 - 8 (originally ATOW-End1...)

    // Village-name secrets (case sensitive, Help page): force the Generation II branch.
    public const string TIME_TRAVEL_TOWN_NAME = "Paradox";
    public const string MARTIAL_TOWN_NAME     = "Destruction";

    public override IEnumerable<string> TypedNames() => Titles.Values.Append(NewMeat);

    public void Reset(GlobalData globalData)
    {
        ResetBase(globalData);

        HubId   = ATimeOfWarHubId.None;
        Tracker = 0;

        WarWinner        = WarFaction.None;
        War              = 0;
        Quarter          = ExtendedBool.None;
        TownHallVisits   = 0;
        MetGuard         = false;
        MetGentleman     = false;
        ShowcaseChosen   = false;
        TownHallMeeting  = false;
        TownHallIndex    = 0;
        WoodenFigures    = 0;
        HeardSeparatists = false;
        HeardMonarchists = false;
        Showcase         = 0;
        Barracks         = ExtendedBool.None;

        Assessment          = false;
        WeaponsBonusClaimed = false;
        SelectedPlayer      = string.Empty;
        SabotagedBuilding   = MartialBuilding.None;
        OutpostDestroyed    = false;
        ChroniclePage       = 0;
        BuildingsDestroyed  = 0;
        ChroniclePageMiddle = false;
        SabotageTarget      = string.Empty;
        Saboteur            = string.Empty;
        DuelType            = 0;
        DuelLoser           = string.Empty;
        Bribe               = ExtendedBool.None;
        SuspiciousBarn      = false;

        FlagsBought           = 0;
        BlameIndex            = 0;
        Blamed                = string.Empty;
        AllSilent             = false;
        TimeMachineMasterwork = false;
        TimeMachineRepaired   = false;
        MendingClaimed        = false;
        RepairsDone           = 0;

        Decrees           = 0;
        CrownPenalties    = 0;
        SeparatistsBanned = ExtendedBool.None;
        WeaponReward      = 0;
        Benevolent        = ExtendedBool.None;
        DecreePlayer      = string.Empty;
        DecreeUsed        = new bool[9];
        DecreeChoice      = 0;
        StatueBuilt       = false;
        StatueOwner       = string.Empty;

        NewMeat = string.Empty;
        Meat    = false;
        Gas     = ExtendedBool.None;

        TimeShifts     = 0;
        World          = 0;
        PreviousWorld  = 0;
        WorldShifts    = 0;
        TimelineStable = false;
        MasterworkDone = false;

        TimeMistake = 0;
        Releases    = 0;
        Giants      = 0;
        MonuIndex   = 0;

        Ending = 0;

        foreach (string player in AllPlayerSlots(globalData))
        {
            BlameVotes[player]  = 0;
            Silent[player]      = false;
            DecreesGood[player] = 0;
            DecreesEvil[player] = 0;
            Titles[player]      = string.Empty;
            Released[player]    = false;
        }
    }
}
