namespace MyFathersWorkApp;

// Guided mode hints for Fear of the Unknown (Shared/Guide/GuideService.cs, text in FearOfTheUnknown_Guide.csv).
// The rules mirror the scenario code; keep them in step when a branch condition changes:
// - Gen I -> II: Gen1End / Gen1FateVictim / Gen1FateMob (50/50: the executed Butcher or nobody with a Family Plot ->
//   Foreigners), village-name secrets.
// - Foreigners -> III: Gen2ForeignersEnd (50/50: enough bricks -> Isolation, the witch unmasked -> Tension).
// - Creature -> III: Gen2CreatureEnd (50/50: the Creature made -> Liberal, the Count got more money -> Privatized).
// - Endings: ControlledExperiment (Isolation), Comeuppance (Tension), the Warden rewards (Privatized), Asylum
//   admissions (Liberal).
public static class FearOfTheUnknownGuide
{
    private const int _VICTIM_BUTCHER = 2; // FearOfTheUnknown._VICTIM_BUTCHER

    public static GuideNote? Hub(GuideContext g)
    {
        GlobalData           data = g.GlobalData;
        FearOfTheUnknownVars vars = data.FearOfTheUnknownVars;

        return vars.HubId switch
        {
            FearOfTheUnknownHubId.WitchHunt  => g.Note("WitchHunt", now: WitchHuntNow(g), title: Route(g, 1)),
            FearOfTheUnknownHubId.Foreigners => g.Note("Foreigners", BricksRule(data), g.Now("Foreigners", text => text.FormatWithIndex(0, vars.Cursed)), Route(g, 2)),
            FearOfTheUnknownHubId.Creature   => g.Note("Creature", title: Route(g, 2)),
            FearOfTheUnknownHubId.Isolation  => g.Note("Isolation", CollaborationRule(data), IsolationNow(g), Route(g, 3)),
            FearOfTheUnknownHubId.Tension    => g.Note("Tension", now: TensionNow(g), title: Route(g, 3)),
            FearOfTheUnknownHubId.Privatized => g.Note("Privatized", Needed(data), g.Now("Privatized", text => text.FormatWithReplacement(0, Rewarded(data).ToString())), Route(g, 3)),
            FearOfTheUnknownHubId.Liberal    => g.Note("Liberal", Needed(data), g.Now("Liberal", text => text.FormatWithReplacement(0, vars.AsylumCount.ToString())), Route(g, 3)),
            _                                => null
        };
    }

    public static GuideNote? Screen(GuideContext g, string screen)
    {
        GlobalData data = g.GlobalData;

        return screen switch
        {
            // Generation I: the Family Plot (the mob) and the execution
            "PlotPlayerCheckin" or "GainFamilyPlot" => g.Note("FamilyPlot"),
            "PublicExecution" or "ExecutionVote" or "ExecutionBid" or "ExecutionVoteResolved" or "ExecutionBidResolved" => g.Note("Execution"),

            // Blame it on the Foreigners: the Wall vote, the accusation, the bricks
            "WallVote" or "WallVoteResolved"           => g.Note("WallVote"),
            "Accusations" or "DeathAndResolution"      => g.Note("Accusation"),
            "ConstructionMatters"                      => g.Note("Bricks"),

            // The Creature Re-emerges: the money for the Asylum and the Count, the Creature
            "AsylumResolution" or "BusinessOffer" or "BusinessOfferRevealed" or "TotalContributions" => g.Note("Money"),
            "GrandfathersCreation"                                                                    => g.Note("CreatureMade"),

            // Generation III
            "ControlledExperiment" => g.Note("Collaboration", CollaborationRule(data)),
            "HorrorOfExistence"    => g.Note("WardenReward", Needed(data)),
            "AdmittanceLog"        => g.Note("Admission", Needed(data)),

            _ => null
        };
    }

    private static string Route(GuideContext g, int generation) => g.Text(generation switch
    {
        1 => "Guide_Route1",
        2 => "Guide_Route2",
        _ => "Guide_Route3"
    });

    private static string? WitchHuntNow(GuideContext g)
    {
        GlobalData           data = g.GlobalData;
        FearOfTheUnknownVars vars = data.FearOfTheUnknownVars;

        // Village-name secrets (Help page) decide the branch outright
        if (data.TownName == FearOfTheUnknownVars.FOREIGNERS_TOWN_NAME) return g.Text("Guide_WitchHunt_NowForced", text => text.FormatWithCondition(0, () => true));
        if (data.TownName == FearOfTheUnknownVars.CREATURE_TOWN_NAME) return g.Text("Guide_WitchHunt_NowForced", text => text.FormatWithCondition(0, () => false));

        return g.Text("Guide_WitchHunt_Now", text => text
                                                     .FormatWithIndex(0, Math.Clamp(vars.Victim, 0, 2))
                                                     .FormatWithCondition(1, () => vars.Mobbed));
    }

    // ConstructionMatters: bricks needed for the Memorial Wall, drawn per player count
    private static Func<string, string> BricksRule(GlobalData data) => text => text.FormatWithIndex(0, PlayerIndex(data));

    // ControlledExperiment: 2 players all tokens, 3 players 2 collectors, 4 players 2 or 3 (drawn)
    private static Func<string, string> CollaborationRule(GlobalData data) => text => text.FormatWithIndex(0, PlayerIndex(data));

    // Privatized and Liberal: 2 with up to 3 players, else 3
    private static Func<string, string> Needed(GlobalData data) => text => text.FormatWithReplacement(0, data.PlayersNum <= 3 ? "2" : "3");

    private static int PlayerIndex(GlobalData data) => data.PlayersNum switch
    {
        2 => 0,
        3 => 1,
        _ => 2
    };

    private static int Rewarded(GlobalData data) => data.GetActivePlayers().Count(player => data.FearOfTheUnknownVars.CreepReward.GetValueOrDefault(player));

    private static string? IsolationNow(GuideContext g)
    {
        int ending = g.GlobalData.FearOfTheUnknownVars.Ending;
        return ending is 1 or 2 ? g.Text("Guide_Isolation_Now", text => text.FormatWithIndex(0, ending - 1)) : null;
    }

    // Comeuppance: the Butcher executed, the wrong person accused, Strength chosen; two or more -> ending 4
    private static string? TensionNow(GuideContext g)
    {
        FearOfTheUnknownVars vars = g.GlobalData.FearOfTheUnknownVars;
        int bad = (vars.Victim == _VICTIM_BUTCHER ? 1 : 0) + (vars.Cursed == 2 ? 1 : 0) + (vars.Walls == ExtendedBool.True ? 1 : 0);
        return g.Text("Guide_Tension_Now", text => text
                                                   .FormatWithReplacement(0, bad.ToString())
                                                   .FormatWithCondition(1, () => bad >= 2));
    }
}
