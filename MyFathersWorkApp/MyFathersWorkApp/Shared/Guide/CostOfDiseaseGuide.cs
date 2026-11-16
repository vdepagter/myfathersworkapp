namespace MyFathersWorkApp;

// Guided mode hints for The Cost of Disease (Shared/Guide/GuideService.cs, text in TheCostOfDisease_Guide.csv).
// The rules mirror the scenario code; keep them in step when a branch condition changes:
// - Gen I -> II: TheCostOfDisease FeverHeart2_1 / S5Fate1 / S5Fate2 (50/50 building or Hearts, village-name secrets).
// - Devastation -> III: MostInvestigated / DevastationFate1 (fewer than 2 Exposed buildings -> A Taste of Evil, whichever
//   faction is evil).
// - Hospital -> III: HospitalEndOfGeneration (one of three tests at random: Immortality, University, Panacea).
// - Gen III endings: GloomyGothic_0, Prosperity_0 (Confront + hunt target, the vial, the Frenzy, the hunts).
public static class CostOfDiseaseGuide
{
    public static GuideNote? Hub(GuideContext g)
    {
        GlobalData           data = g.GlobalData;
        TheCostOfDiseaseVars vars = data.TheCostOfDiseaseVars;

        switch (vars.HubId)
        {
            case CostOfDiseaseHubId.Fever:
                return g.Note("Fever", HeartsRule(data), FeverNow(g), Route(g, 1));

            case CostOfDiseaseHubId.Devastation:
                return g.Note("Devastation", title: Route(g, 2));

            case CostOfDiseaseHubId.Hospital:
                return g.Note("Hospital", ImmortalityRule(data), HospitalNow(g), Route(g, 2));

            case CostOfDiseaseHubId.GloomyGothic:
                return Hunters(vars)
                    ? g.Note("GloomyHunters", now: GloomyHuntersNow(g), title: Route(g, 3))
                    : g.Note("GloomyWolves", text => text.FormatWithReplacement(0, vars.Charity), GloomyWolvesNow(g), Route(g, 3));

            case CostOfDiseaseHubId.Prosperity:
                if (vars.ReturnToEvil) return g.Note("ProsperityEvil", title: Route(g, 3));
                return Hunters(vars)
                    ? g.Note("ProsperityHunters", now: g.Now("ProsperityHunters", text => text.FormatWithReplacement(0, vars.HuntCount.ToString())), title: Route(g, 3))
                    : g.Note("ProsperityOrder", title: Route(g, 3));

            case CostOfDiseaseHubId.University:
                return g.Note("University", title: Route(g, 3));

            case CostOfDiseaseHubId.NoUniversity:
                return g.Note("NoUniversity", title: Route(g, 3));

            default:
                return null;
        }
    }

    public static GuideNote? Screen(GuideContext g, string screen)
    {
        GlobalData           data = g.GlobalData;
        TheCostOfDiseaseVars vars = data.TheCostOfDiseaseVars;

        return screen switch
        {
            // Generation I: the Mayor bid, the building, the Hearts
            "S5Special1A"      => g.Note("MayorBid"),
            "S5SpecialSetup1B" => g.Note("Building"),
            "FeverHeart"       => g.Note("Hearts", HeartsRule(data)),

            // Devastation: investigations (secret, so no tally)
            "BuildPlay" => g.Note("Investigate"),

            // Hospital: the three tests for the University
            "InfinityClick1"   => g.Note("Immortality", ImmortalityRule(data)),
            "MethodUnicure_0"  => g.Note("FeverCure"),
            "SymposiumEvent3"  => g.Note("Symposium", text => text.FormatWithCondition(0, () => vars.Pub == ExtendedBool.True)),
            "PanaceaMessage_0" => g.Note("Panacea"),

            // A Taste of Evil, Fraternity of Hunters: the Confront vote and the hunt target
            "EvilHunter1Event" or "EvilHunter1Event2" => g.Note("Confront"),
            "HunterConf2" or "HunterConf3"            => g.Note("HuntTarget"),

            // A Taste of Evil, Order of St Hubertus: the cleansing vote and the vial
            "WolvesVote" or "WolvesVote2" or "WolvesVoteCheck" => g.Note("CleanseVote", text => text.FormatWithReplacement(0, vars.Charity)),
            "TheVialUse"                                       => g.Note("Vial"),

            // The Good Fight: the Frenzy (vote, or bid with 2 players) and the hunts
            "GoodFrenzyEvent2" or "GoodFrenzyEvent2b" or "FrenzyBid" or "FrenzyBid2" => g.Note("Frenzy"),
            "Huntround" or "HuntersChoice" or "HuntNight" => g.Note("Hunt", now: g.Now("ProsperityHunters", text => text.FormatWithReplacement(0, vars.HuntCount.ToString()))),

            _ => null
        };
    }

    private static bool Hunters(TheCostOfDiseaseVars vars) => vars.Society == Society.FraternityOfHunters;

    // "Where Generation I leads" / "Where this branch ends"
    private static string Route(GuideContext g, int generation) => g.Text(generation switch
    {
        1 => "Guide_Route1",
        2 => "Guide_Route2",
        _ => "Guide_Route3"
    });

    // S5Fate2: more than base + d6 Hearts builds the Hospital
    private static Func<string, string> HeartsRule(GlobalData data)
    {
        int needed = data.PlayersNum switch
        {
            2 => 3,
            3 => 6,
            _ => 8
        };

        return text => text
                      .FormatWithReplacement(0, needed.ToString())
                      .FormatWithReplacement(1, (needed + 1).ToString())
                      .FormatWithReplacement(2, (needed + 6).ToString())
                      .FormatWithReplacement(3, (needed + 7).ToString())
                      .FormatWithReplacement(4, (needed + 1).ToString());
    }

    private static string? FeverNow(GuideContext g)
    {
        GlobalData           data = g.GlobalData;
        TheCostOfDiseaseVars vars = data.TheCostOfDiseaseVars;

        // Village-name secrets (Help page) decide the branch outright
        if (data.TownName == TheCostOfDiseaseVars.HOSPITAL_TOWN_NAME) return g.Text("Guide_Fever_NowForced", text => text.FormatWithCondition(0, () => true));
        if (data.TownName == TheCostOfDiseaseVars.DEVASTATION_TOWN_NAME) return g.Text("Guide_Fever_NowForced", text => text.FormatWithCondition(0, () => false));

        return g.Text("Guide_Fever_Now", text => text.FormatWithIndex(0, (int)vars.Building));
    }

    // S5HospA1: at least 1-2 / 2 / 2-3 players with the Secret to Immortality (2 / 3 / 4 players)
    private static Func<string, string> ImmortalityRule(GlobalData data) => text => text
                                                                                    .FormatWithIndex(0, data.PlayersNum switch
                                                                                     {
                                                                                         2 => 0,
                                                                                         3 => 1,
                                                                                         _ => 2
                                                                                     });

    private static string? HospitalNow(GuideContext g)
    {
        TheCostOfDiseaseVars vars = g.GlobalData.TheCostOfDiseaseVars;

        // The Panacea choice is made in secret: only a published one is known to the table
        return g.Text("Guide_Hospital_Now", text => text
                                                    .FormatWithReplacement(0, vars.LifeCount.ToString())
                                                    .FormatWithIndex(1, (int)vars.Uni)
                                                    .FormatWithCondition(2, () => vars.Pana == PanaceaVal.Publish));
    }

    private static string? GloomyHuntersNow(GuideContext g)
    {
        TheCostOfDiseaseVars vars = g.GlobalData.TheCostOfDiseaseVars;
        if (vars.Confront == ExtendedBool.None) return null;
        return g.Text("Guide_GloomyHunters_Now", text => text
                                                         .FormatWithCondition(0, () => vars.Confront == ExtendedBool.True)
                                                         .FormatWithIndex(1, (int)vars.Taxes));
    }

    private static string? GloomyWolvesNow(GuideContext g)
    {
        TheCostOfDiseaseVars vars = g.GlobalData.TheCostOfDiseaseVars;
        if (vars.VialUse == ExtendedBool.None) return null;
        return g.Text("Guide_GloomyWolves_Now", text => text.FormatWithCondition(0, () => vars.VialUse == ExtendedBool.True));
    }
}
