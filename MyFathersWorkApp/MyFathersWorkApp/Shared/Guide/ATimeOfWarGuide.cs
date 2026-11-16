namespace MyFathersWorkApp;

// Guided mode hints for A Time of War (Shared/Guide/GuideService.cs, text in ATimeOfWar_Guide.csv).
// The rules mirror the scenario code; keep them in step when a branch condition changes:
// - Gen I -> II: $war, drawn at random (50/50, never shown here), village-name secrets.
// - Martial -> III: ResistSides / ResistEnd* (one of three tests, 1 in 3 each, the assessment letter forces the first;
//   whoever wins the war decides: Monarchists -> Monarchy Reigns, Separatists -> World Conquest).
// - Time Travel -> III: TimeTravelGenerationEnd / TimeEnd1 / TimeEnd2 (50/50: Paradox tokens or the Time Machine).
// - Endings: Reign crown penalties, Peace meat + gas, Warning Masterwork in the stable timeline, Paradox Giant
//   Masterworks.
public static class ATimeOfWarGuide
{
    public static GuideNote? Hub(GuideContext g)
    {
        GlobalData     data = g.GlobalData;
        ATimeOfWarVars vars = data.ATimeOfWarVars;

        return vars.HubId switch
        {
            ATimeOfWarHubId.TakingSides  => g.Note("TakingSides", now: TakingSidesNow(g), title: Route(g, 1)),
            ATimeOfWarHubId.Martial      => g.Note("Martial", Factions(data), g.Now("Martial", text => Factions(data)(text).FormatWithReplacement(2, vars.BuildingsDestroyed.ToString())), Route(g, 2)),
            ATimeOfWarHubId.TimeTravel   => g.Note("TimeTravel", Paradox(data), g.Now("TimeTravel", text => text.FormatWithCondition(0, () => vars.TimeMachineRepaired)), Route(g, 2)),
            ATimeOfWarHubId.MonarchReign => g.Note("Reign", CrownRule(data), g.Now("Reign", text => text.FormatWithReplacement(0, vars.CrownPenalties.ToString())), Route(g, 3)),
            ATimeOfWarHubId.Peace        => g.Note("Peace", now: PeaceNow(g), title: Route(g, 3)),
            ATimeOfWarHubId.Warning      => g.Note("Warning", now: g.Now("Warning", text => text
                                                                                      .FormatWithCondition(0, () => vars.TimelineStable)
                                                                                      .FormatWithCondition(1, () => vars.MasterworkDone)), title: Route(g, 3)),
            ATimeOfWarHubId.Paradox      => g.Note("Paradox", GiantRule(data), title: Route(g, 3)), // releases are secret: no tally
            _                            => null
        };
    }

    public static GuideNote? Screen(GuideContext g, string screen)
    {
        GlobalData data = g.GlobalData;

        return screen switch
        {
            // Martial: the three tests for the war
            "ResistSides" => g.Note("ResistSides", Factions(data)),
            "ResistEnd"   => g.Note("ResistBuildings", Factions(data)),
            "ResistEnd2"  => g.Note("ResistFlags", Factions(data)),
            "ResistEnd3"  => g.Note("ResistWeapons", Factions(data)),

            // Time Travel: the repairs and the two tests
            "ATimeMachineRepaired" => g.Note("Repaired"),
            "TimeEnd1"             => g.Note("ParadoxTokens"),
            "TimeEnd2"             => g.Note("TimeMachine"),

            // Generation III
            "FirstCrownCons" or "CrownCons"                                       => g.Note("Crown", CrownRule(data)),
            "FamineEvent" or "FamineVote" or "FamineRes" or "FamineBid2P" or "FamineBidRes2P" => g.Note("Famine"),
            "PeaceEvent" or "PeaceEventb" or "PeaceEventc"                        => g.Note("Gas"),
            "Stasis"                                                              => g.Note("Stasis"),
            "MonumentVote" or "Monu" or "MonuRevealed"                            => g.Note("Schematics", GiantRule(data)),
            "ParadoxGiantCheck"                                                   => g.Note("Giants", GiantRule(data)),

            _ => null
        };
    }

    private static string Route(GuideContext g, int generation) => g.Text(generation switch
    {
        1 => "Guide_Route1",
        2 => "Guide_Route2",
        _ => "Guide_Route3"
    });

    private static string? TakingSidesNow(GuideContext g)
    {
        GlobalData data = g.GlobalData;

        // Village-name secrets (Help page) decide the branch outright
        if (data.TownName == ATimeOfWarVars.TIME_TRAVEL_TOWN_NAME) return g.Text("Guide_TakingSides_NowForced", text => text.FormatWithCondition(0, () => true));
        if (data.TownName == ATimeOfWarVars.MARTIAL_TOWN_NAME) return g.Text("Guide_TakingSides_NowForced", text => text.FormatWithCondition(0, () => false));
        return null;
    }

    // {{0}} the faction the town supported in Generation I, {{1}} the other one
    private static Func<string, string> Factions(GlobalData data)
    {
        bool monarchists = data.ATimeOfWarVars.WarWinner == WarFaction.Monarchists;
        return text => text.FormatWithCondition(0, () => monarchists).FormatWithCondition(1, () => !monarchists);
    }

    // TimeEnd1: Paradox tokens against 1-3 / 2-4 / 3-5 for 2 / 3 / 4 players
    private static Func<string, string> Paradox(GlobalData data) => text => text
                                                                            .FormatWithReplacement(0, (data.PlayersNum - 1).ToString())
                                                                            .FormatWithReplacement(1, (data.PlayersNum + 1).ToString());

    // Reign: more than PlayersNum - 2 crown penalties -> ending 6
    private static Func<string, string> CrownRule(GlobalData data) => text => text.FormatWithReplacement(0, (data.PlayersNum - 2).ToString());

    // Paradox: Giant Masterworks (one fewer if the schematics were released) against PlayersNum - 1
    private static Func<string, string> GiantRule(GlobalData data) => text => text.FormatWithReplacement(0, (data.PlayersNum - 1).ToString());

    private static string? PeaceNow(GuideContext g)
    {
        ATimeOfWarVars vars = g.GlobalData.ATimeOfWarVars;
        return g.Text("Guide_Peace_Now", text => text
                                                 .FormatWithCondition(0, () => vars.Meat)
                                                 .FormatWithIndex(1, (int)vars.Gas));
    }
}
