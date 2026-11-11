// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace MyFathersWorkApp;

// RandomArray indices: see the _RND_ constants in 00_FearOfTheUnknown_Setup.cs
public class FearOfTheUnknownVars() : ScenarioVars(900)
{
    public FearOfTheUnknownHubId HubId { get; set; }
    public FearOfTheUnknownHubId Path  { get; set; } // Branch of the current Generation (originally path4)
    public int                   Tracker { get; set; } // Suspicion Marker space

    // Generation I - Witch Hunt
    public bool[]                   Milestones { get; set; } = new bool[4]; // 10VP, Research Level 2, Insanity 3, Creepy 3 (originally fp10vp, fpresearch2, ...)
    public int                      ClaimCount { get; set; }                // originally onecount
    public int                      Milestone  { get; set; }                // milestone being claimed
    public string                   MilestonePlayer { get; set; } = string.Empty; // originally tempname / tempplot
    public Dictionary<string, int>  Fate       { get; set; } = new();       // 1 Fame, 2 Knowledge, 3 Science, 4 Revenge, 5 Respect, 6 Entertainment (originally plA...)
    public Dictionary<string, bool> FamilyPlot { get; set; } = new();       // originally plotA...
    public bool                     Mobbed     { get; set; }
    public int                      Victim     { get; set; } // 1 Blacksmith, 2 Butcher (originally kill)

    // Generation II - Blame it on the Foreigners
    public int                     ObsessionIndex { get; set; } // player receiving an Obsession
    public int                     HexIndex       { get; set; } // player choosing a hex
    public string                  RumorTarget    { get; set; } = string.Empty; // originally rum
    public Dictionary<string, int> Hex1           { get; set; } = new(); // 0 Church, 1 Traveling Caravan, 2 Laborer's Union (originally witA...)
    public Dictionary<string, int> Hex2           { get; set; } = new(); // 0 Park, 1 Builder's Office, 2 Cemetery (originally witA2...)
    public ExtendedBool            RevengePlot    { get; set; } = ExtendedBool.None; // True Fortune, False Misfortune
    public bool                    RevengeResolved { get; set; }
    public ExtendedBool            Walls          { get; set; } = ExtendedBool.None; // True Strength, False Acceptance
    public int                     Cursed         { get; set; } // 1 witch removed, 2 curse continues
    public string                  FalseWitch     { get; set; } = string.Empty;
    public int                     CaravanVisits  { get; set; } // originally carcount

    // Generation II - The Creature
    public bool                     CreatureCard     { get; set; }
    public int                      CreatureMade     { get; set; } // 1 not created, 2 created (originally creature)
    public int                      CreatureHelp     { get; set; }
    public Dictionary<string, int>  AsylumVisits     { get; set; } = new(); // originally Av...
    public Dictionary<string, int>  Debt             { get; set; } = new(); // money pledged to the Count (originally Acont...)
    public string                   HouseOnTheHill   { get; set; } = string.Empty; // originally bhome
    public int                      BallAndChain     { get; set; } // originally bnc
    public string                   SelectedPlayer   { get; set; } = string.Empty; // player picked for the current event (originally tempas, tempid, tempname)
    public int                      PaymentIndex     { get; set; } // player paying the Count
    public string                   BusinessPlayer   { get; set; } = string.Empty; // originally busm
    public bool                     BusinessBargain  { get; set; } // originally busvp
    public bool                     BusinessUpgrade  { get; set; } // originally btemp
    public int                      BusinessMeetings { get; set; }
    public ExtendedBool             Cure             { get; set; } = ExtendedBool.None; // False offered, True cured (originally curecount)
    public string                   CurePlayer       { get; set; } = string.Empty;
    public int                      AsylumCount      { get; set; }
    public int                      Jail             { get; set; } // 1 Asylum supported, 2 the Count supported
    public bool                     Gen2Asylum       { get; set; } // originally Gen2End == "asylum"

    // Generation III - common
    public int                      Ending    { get; set; } // 1 - 8 (originally FOTU-End1...)
    public Dictionary<string, bool> CrossroadVisited { get; set; } = new(); // reached the crossroads (each player once)
    public Dictionary<string, bool> Crossroad { get; set; } = new(); // kept the Obsession at the crossroads (originally token2a...)

    // Generation III - Isolation
    public Dictionary<string, bool> Verified       { get; set; } = new(); // originally idA...
    public int                      SmugglingCount { get; set; }
    public int                      Experiment     { get; set; } // 0 Biology, 1 Chemistry, 2 Engineering (originally ex)
    public int                      Destroyed      { get; set; } // Village Chronicle page after the rampage: 16, 17, 18 (originally dev)
    public int                      SmuggleTrial   { get; set; } // 0 paid, 1 trial (originally pay)

    // Generation III - Tension
    public ExtendedBool TensionGood   { get; set; } = ExtendedBool.None;
    public bool         TenseResolved { get; set; } // originally ten
    public bool         Stole         { get; set; }

    // Generation III - Privatized
    public Dictionary<string, bool>   CreepReward  { get; set; } = new(); // originally creepA...
    public Dictionary<string, int>    Agility      { get; set; } = new();
    public Dictionary<string, int>    Strength     { get; set; } = new();
    public Dictionary<string, int>    Life         { get; set; } = new();
    public Dictionary<string, int>    Hp           { get; set; } = new();
    public Dictionary<string, string> WarriorName  { get; set; } = new();
    public int                        StatIndex    { get; set; }
    public int                        BattleStep   { get; set; }
    public string                     BattleWinner { get; set; } = string.Empty;
    public int                        JunkOffer    { get; set; } // $ paid to the warden

    // Generation III - Liberal
    public bool   Expedition       { get; set; } // Creature killed (originally exped)
    public int    ProgramType      { get; set; } // 0 Engineering, 1 Chemistry, 2 Biology, 3 Occult (originally mentype)
    public string ProgramName      { get; set; } = string.Empty; // originally menname
    public int    Policy           { get; set; } // 1 taxes, 2 nationalist (originally lib)
    public ExtendedBool Bribe      { get; set; } = ExtendedBool.None;
    public int    AsylumAdmissions { get; set; }
    public string Patient          { get; set; } = string.Empty; // originally question
    public int    AsylumScore      { get; set; } // originally test
    public int    AsylumStep       { get; set; } // originally next
    public int    AsylumOutcome    { get; set; } // originally outcome
    public int[]  AsylumQuestions  { get; set; } = [-1, -1, -1, -1];

    // Flavor dates (see RandomDate)
    public int DateRound { get; set; } // first year of the round the last date was made for
    public int DateDays  { get; set; } // days after 1 January of that year

    public const string SUSPECT = "#Suspect"; // the Apprentice who can be the witch with 2 or 3 players

    // Village-name secrets (case sensitive, Help page): force the Generation II branch.
    public const string FOREIGNERS_TOWN_NAME = "Xenophobia";
    public const string CREATURE_TOWN_NAME   = "Abomination";

    public override IEnumerable<string> TypedNames() => WarriorName.Values.Append(ProgramName);

    public void Reset(GlobalData globalData)
    {
        ResetBase(globalData);

        DateRound       = 0;
        DateDays        = 0;
        HubId           = FearOfTheUnknownHubId.None;
        Path            = FearOfTheUnknownHubId.None;
        Tracker         = 0;
        Milestones      = [false, false, false, false];
        ClaimCount      = 0;
        Milestone       = 0;
        MilestonePlayer = string.Empty;
        Mobbed          = false;
        Victim          = 0;
        ObsessionIndex  = 0;
        HexIndex        = 0;
        RumorTarget     = string.Empty;
        RevengePlot     = ExtendedBool.None;
        RevengeResolved = false;
        Walls           = ExtendedBool.None;
        Cursed          = 0;
        FalseWitch      = string.Empty;
        CaravanVisits   = 0;

        CreatureCard     = false;
        CreatureMade     = 0;
        CreatureHelp     = 0;
        HouseOnTheHill   = string.Empty;
        BallAndChain     = 0;
        SelectedPlayer   = string.Empty;
        PaymentIndex     = 0;
        BusinessPlayer   = string.Empty;
        BusinessBargain  = false;
        BusinessUpgrade  = false;
        BusinessMeetings = 0;
        Cure             = ExtendedBool.None;
        CurePlayer       = string.Empty;
        AsylumCount      = 0;
        Jail             = 0;
        Gen2Asylum       = false;

        Ending         = 0;
        SmugglingCount = 0;
        Experiment     = 0;
        Destroyed      = 0;
        SmuggleTrial   = 0;
        TensionGood    = ExtendedBool.None;
        TenseResolved  = false;
        Stole          = false;
        StatIndex      = 0;
        BattleStep     = 0;
        BattleWinner   = string.Empty;
        JunkOffer      = 0;

        Expedition       = false;
        ProgramType      = 0;
        ProgramName      = string.Empty;
        Policy           = 0;
        Bribe            = ExtendedBool.None;
        AsylumAdmissions = 0;
        Patient          = string.Empty;
        AsylumScore      = 0;
        AsylumStep       = 0;
        AsylumOutcome    = 0;
        AsylumQuestions  = [-1, -1, -1, -1];

        Hex1[SUSPECT] = 0;
        Hex2[SUSPECT] = 0;

        foreach (string player in AllPlayerSlots(globalData))
        {
            Fate[player]         = 0;
            FamilyPlot[player]   = false;
            Hex1[player]         = 0;
            Hex2[player]         = 0;
            AsylumVisits[player] = 0;
            Debt[player]         = 0;
            Crossroad[player]    = false;
            CrossroadVisited[player] = false;
            Verified[player]     = false;
            CreepReward[player]  = false;
            Agility[player]      = 0;
            Strength[player]     = 0;
            Life[player]         = 0;
            Hp[player]           = 0;
            WarriorName[player]  = string.Empty;
        }
    }
}
