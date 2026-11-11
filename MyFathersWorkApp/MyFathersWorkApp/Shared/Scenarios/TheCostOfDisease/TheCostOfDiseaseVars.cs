using Newtonsoft.Json;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace MyFathersWorkApp;

// RandomArray indices: 0 - 58 (Gen I & II), Gen III uses 59 - 203 (see _RND_ constants in the Gen III files)
public class TheCostOfDiseaseVars() : ScenarioVars(300 + 1)
{
    public CostOfDiseaseHubId HubId   { get; set; }
    // Which faction is evil is drawn when a screen first depends on it, so mirror mode asks once the official app reveals it
    [JsonIgnore] public Affiliation Wolves  { get { DrawAffiliations(); return _Wolves; }  set => _Wolves = value; }
    [JsonIgnore] public Affiliation Hunters { get { DrawAffiliations(); return _Hunters; } set => _Hunters = value; }

    [JsonProperty(nameof(Wolves))]  private Affiliation _Wolves;
    [JsonProperty(nameof(Hunters))] private Affiliation _Hunters;

    public bool AffiliationsPending { get; set; }
    public int                Tracker { get; set; } // Rename to -> SuspicionMarkerPos

    public bool         Creepy4   { get; set; }
    public string       Gen1Creep { get; set; } = string.Empty;
    public ExtendedBool Seedy     { get; set; } = ExtendedBool.None;

    public bool         Sane3    { get; set; }
    public string       Gen1Sane { get; set; } = string.Empty;
    public ExtendedBool Vacation { get; set; } = ExtendedBool.None;

    public string        Mayor    { get; set; } = string.Empty;
    public BankOrLibrary Building { get; set; } = BankOrLibrary.None;

    public int     CharityTotal { get; set; }
    public string  Charity      { get; set; } = string.Empty;
    public Science Sci3         { get; set; } = Science.None;

    public bool Trigger35         { get; set; }
    public bool ThirtyFiveVpCreep { get; set; }

    public ExtendedBool Cured      { get; set; } // 0 - None, 1 - True, 2 - Complete
    public string       FeverCure  { get; set; } = string.Empty;
    public int          FeverVp    { get; set; }
    public int          FeverMoney { get; set; }
    public ExtendedBool Pub        { get; set; }
    public string       PanaCure   { get; set; } = string.Empty;
    public PanaceaVal   Pana       { get; set; } = PanaceaVal.None;

    public bool         SciAdv  { get; set; }
    public string       Gen2Exp { get; set; } = string.Empty;
    public ExtendedBool Uni     { get; set; }
    public int          Symp    { get; set; }

    public int                      HospCount { get; set; }
    public Dictionary<string, bool> Hosp      { get; set; } = new();

    public bool                     SetInf    { get; set; } // To setup Infinity
    public int                      LifeCount { get; set; } // Originally Life
    public Dictionary<string, bool> Life      { get; set; } = new();
    public bool                     Immort    { get; set; }

    public string[]                    Letter               { get; set; } = new string[6];
    public int                         WCount               { get; set; }
    public int                         HCount               { get; set; }
    public Dictionary<string, Faction> Ally                 { get; set; } = new();
    public BuildingS1A[]               Gen2Buildings        { get; set; } = new BuildingS1A[3]; // originally ba, bb, bc
    public Dictionary<string, int>     BuildingPlay         { get; set; } = new();              // originally playA, playB, etc.
    public Dictionary<string, bool[]>  HelpedExposeBuilding { get; set; } = new();              // originally pAA, pAB, pAC, etc.
    public int[]                       BuildingsExposeValue { get; set; } = new int[3];         // originally exposeA, exposeB, exposeC
    public int                         GoodCount            { get; set; }
    public Society                     Society              { get; set; } = Society.None;

    // Generation III - common
    public CostOfDiseaseEnding Ending          { get; set; } = CostOfDiseaseEnding.None;
    public bool                Lycan           { get; set; }
    public bool                Gen3SetupShown  { get; set; } // originally gen3pg
    public bool                SuspicionMoved  { get; set; } // originally Prosperity1 / University1

    // Generation III - Gloomy Gothic
    public int          HuntVp     { get; set; }
    public int          CharityPenalty { get; set; } // originally conpat
    public ExtendedBool Confront   { get; set; } = ExtendedBool.None;
    public ExtendedBool Taxes      { get; set; } = ExtendedBool.None;
    public ExtendedBool VialUse    { get; set; } = ExtendedBool.None;
    public int          HuntNumber { get; set; } // originally huntnum

    // Generation III - Prosperity
    public int          HuntCount      { get; set; }
    public string[]     HunterOrder    { get; set; } = new string[4];  // originally hunt1a, hunt1b, hunt2a, hunt2b, empty = not drawn yet
    public int[]        HuntRewards    { get; set; } = new int[8];     // originally reward1 - reward8, -1 = not drawn yet
    public int          HuntReward     { get; set; }                   // originally huntreward1 / huntreward2
    public int          HuntDirection  { get; set; }                   // 0 - North, 1 - East, 2 - West, 3 - South
    public int          HuntBeast      { get; set; }                   // originally huntbeast
    public string       HuntName       { get; set; } = string.Empty;
    public ExtendedBool Overrun        { get; set; } = ExtendedBool.None;
    public ExtendedBool Frenzy         { get; set; } = ExtendedBool.None;
    public bool         ReturnToEvil   { get; set; } // Prosperity3b
    public int          AngryMobCount  { get; set; }

    // Generation III - University
    public int    Mental     { get; set; }
    public string SanePlayer { get; set; } = string.Empty;
    public bool   Ultimate   { get; set; }
    public bool   Killed     { get; set; }
    public bool[] DetVisited { get; set; } = new bool[4]; // originally det1 - det4
    public int    DetCount   { get; set; }

    // Generation III - No University
    public ExtendedBool                             Electricity     { get; set; } = ExtendedBool.None; // True -> peeps 0, False -> peeps 1
    public bool                                     ImmortalityNone { get; set; }                      // originally imm == "none"
    public int                                      BarCount        { get; set; }
    public int                                      NewMasterIndex  { get; set; }
    public string                                   MasterworkCheck { get; set; } = string.Empty; // originally tempcheck / tempcomp
    public Dictionary<string, MasterworkDiscipline> MwDiscipline    { get; set; } = new();
    public Dictionary<string, MasterworkType>       MwType          { get; set; } = new();
    public Dictionary<string, int>                  MwCost          { get; set; } = new(); // Science index: 0 - Engineering, 1 - Chemistry, 2 - Biology
    public Dictionary<string, string>               MwName          { get; set; } = new();
    public Dictionary<string, int>                  MwAdjective     { get; set; } = new();


    public const string WOLVES_EVIL_TOWN_NAME  = "Rage";
    public const string HUNTERS_EVIL_TOWN_NAME = "Kraven";
    public const string DEVASTATION_TOWN_NAME  = "Plague"; // forces the Generation II branch (Help page)
    public const string HOSPITAL_TOWN_NAME     = "Mercy";

    public override IEnumerable<string> TypedNames() => MwName.Values;

    public void DrawAffiliations()
    {
        if (!AffiliationsPending) return;
        bool swap = RandomBool(0);
        AffiliationsPending = false;
        _Wolves             = swap ? Affiliation.Evil : Affiliation.Good;
        _Hunters            = swap ? Affiliation.Good : Affiliation.Evil;
    }

    public void Reset(GlobalData globalData)
    {
        ResetBase(globalData);

        _Wolves              = Affiliation.Good;
        _Hunters             = Affiliation.Evil;
        AffiliationsPending  = false;
        Tracker              = 0;
        Creepy4              = false;
        Seedy                = ExtendedBool.None;
        Sane3                = false;
        Gen1Creep            = string.Empty;
        Gen1Sane             = string.Empty;
        Vacation             = ExtendedBool.None;
        Mayor                = string.Empty;
        Building             = BankOrLibrary.None;
        CharityTotal         = 0;
        Charity              = string.Empty;
        Sci3                 = Science.None;
        ThirtyFiveVpCreep    = false;
        FeverVp              = 0;
        FeverMoney           = 0;
        Cured                = ExtendedBool.None;
        SciAdv               = false;
        Trigger35            = false;
        Gen2Exp              = string.Empty;
        FeverCure            = string.Empty;
        Uni                  = ExtendedBool.None;
        Pub                  = ExtendedBool.None;
        PanaCure             = string.Empty;
        Pana                 = PanaceaVal.None;
        HospCount            = 0;
        SetInf               = false;
        LifeCount            = 0;
        Immort               = false;
        Symp                 = 0;
        WCount               = 0;
        HCount               = 0;
        Gen2Buildings        = [BuildingS1A.None, BuildingS1A.None, BuildingS1A.None];
        BuildingsExposeValue = [0, 0, 0];
        GoodCount            = 0;
        Society              = Society.None;

        Ending          = CostOfDiseaseEnding.None;
        Lycan           = false;
        Gen3SetupShown  = false;
        SuspicionMoved  = false;
        HuntVp          = 0;
        CharityPenalty  = 0;
        Confront        = ExtendedBool.None;
        Taxes           = ExtendedBool.None;
        VialUse         = ExtendedBool.None;
        HuntNumber      = 0;
        HuntCount       = 0;
        HunterOrder     = [string.Empty, string.Empty, string.Empty, string.Empty];
        HuntRewards     = [-1, -1, -1, -1, -1, -1, -1, -1];
        HuntReward      = 0;
        HuntDirection   = 0;
        HuntBeast       = 0;
        HuntName        = string.Empty;
        Overrun         = ExtendedBool.None;
        Frenzy          = ExtendedBool.None;
        ReturnToEvil    = false;
        AngryMobCount   = 0;
        Mental          = 0;
        SanePlayer      = string.Empty;
        Ultimate        = false;
        Killed          = false;
        DetVisited      = [false, false, false, false];
        DetCount        = 0;
        Electricity     = ExtendedBool.None;
        ImmortalityNone = false;
        BarCount        = 0;
        NewMasterIndex  = 0;
        MasterworkCheck = string.Empty;

        string[] players = [globalData.PlayerAName, globalData.PlayerBName, globalData.PlayerCName, globalData.PlayerDName];

        foreach (string player in players)
        {
            Hosp[player]                 = false;
            Life[player]                 = false;
            Ally[player]                 = Faction.None;
            BuildingPlay[player]         = 0;
            HelpedExposeBuilding[player] = [false, false, false];
            MwDiscipline[player]         = MasterworkDiscipline.None;
            MwType[player]               = MasterworkType.None;
            MwCost[player]               = 0;
            MwName[player]               = string.Empty;
            MwAdjective[player]          = 0;
        }

        for (int x = 0; x < 6; ++x) Letter[x] = string.Empty;
    }
}