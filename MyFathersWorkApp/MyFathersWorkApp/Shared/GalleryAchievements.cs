namespace MyFathersWorkApp;

// Achievements and records of the Endings Gallery, worked out from the recorded games and ticked endings every time they
// are shown. Nothing is stored, so undo, edits in the Recent Games sheet, deletes and imports never leave a stale badge.
// The six "official" ones are the special achievements of the official app (Unity ViewEndings.GetSpecialAchievementComplete);
// the rest are fan additions (owner, 13 Nov 2026; score levels 75 / 100 / 150 VP).
public enum AchievementGroup { Endings, Masterworks, Scores }

public enum SealKind { Gold, Red }

// Detail: who and when once unlocked (or the progress text while locked); Progress 0..1 draws a bar while locked
public record Achievement(string Id, AchievementGroup Group, bool Official, string Seal, SealKind Kind, bool Unlocked, string Detail, double? Progress = null);

// The highest score of a scenario: the first game that reached it
public record ScoreRecord(ScenarioId Scenario, int Score, string Player, GalleryGame Game);

public static class GalleryAchievements
{
    public static readonly int[] ScoreLevels = [75, 100, 150];

    private const int _MANY_MASTERWORKS = 5;
    private const int _MOST_MASTERWORKS = 10;
    private const int _DYNASTY          = 3;

    public static string Title(GlobalData globalData, Achievement achievement) =>
        globalData.GetLocalizedUITag($"Ach_{achievement.Id}_Title");

    public static string Text(GlobalData globalData, Achievement achievement) =>
        globalData.GetLocalizedUITag($"Ach_{achievement.Id}_Text");

    private static IEnumerable<GalleryGame> Chronological(IEnumerable<GalleryGame> games) => games.OrderBy(game => game.Date).ThenBy(game => game.Id);

    public static ScoreRecord? Record(IEnumerable<GalleryGame> games, ScenarioId scenario)
    {
        ScoreRecord? best = null;

        foreach (GalleryGame game in Chronological(games.Where(game => game.Scenario == scenario)))
        {
            foreach (string player in game.Players)
            {
                int score = game.Scores.GetValueOrDefault(player);
                if (score > 0 && (best == null || score > best.Score)) best = new ScoreRecord(scenario, score, player, game);
            }
        }

        return best;
    }

    public static List<Achievement> All(GlobalData globalData, GalleryData data) => All(globalData, data.Games, data.Ticked);

    public static List<Achievement> All(GlobalData globalData, IReadOnlyCollection<GalleryGame> games, Dictionary<string, List<int>> ticked)
    {
        List<Achievement> list    = [];
        GalleryGame[]     ordered = Chronological(games).ToArray();

        string Notyet()                  => globalData.GetLocalizedUITag(GlobalTags.Ach_NotYet);
        string When(GalleryGame game)    => game.Date.ToString("d MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
        string WhoWhen(string who, GalleryGame game) => $"{who} · {When(game)}";
        string OfCount(int have, int need) => globalData.GetLocalizedUITag(GlobalTags.Ach_OfCount)
                                                        .FormatWithReplacement(0, have.ToString())
                                                        .FormatWithReplacement(1, need.ToString());
        string Best(string best)         => globalData.GetLocalizedUITag(GlobalTags.Ach_Best).FormatWithReplacement(0, best);

        // Endings (official): the end of each scenario, then all 8 of its endings (played or ticked off by hand)
        string[] numerals = ["I", "II", "III"];
        for (int i = 0; i < GalleryTrees.Scenarios.Length; i++)
        {
            ScenarioId   scenario = GalleryTrees.Scenarios[i];
            HashSet<int> done     = games.Where(game => game.Scenario == scenario && game.Ending > 0).Select(game => game.Ending).ToHashSet();
            if (ticked.TryGetValue(scenario.ToString(), out List<int>? hand)) done.UnionWith(hand.Where(ending => ending > 0));

            GalleryGame? first = ordered.FirstOrDefault(game => game.Scenario == scenario && game.Ending > 0);
            list.Add(new Achievement($"End{scenario}", AchievementGroup.Endings, true, numerals[i], SealKind.Gold, done.Count > 0,
                                     first != null ? When(first) : done.Count > 0 ? globalData.GetLocalizedUITag(GlobalTags.Gallery_TickedByHand) : Notyet()));
            list.Add(new Achievement($"All{scenario}", AchievementGroup.Endings, true, "8", SealKind.Gold, done.Count >= 8,
                                     OfCount(Math.Min(done.Count, 8), 8), Math.Min(done.Count, 8) / 8.0));
        }

        // Masterworks (ticked at score entry or in the Recent Games sheet)
        int          total     = 0;
        GalleryGame? firstMw   = null;
        GalleryGame? manyMw    = null;
        GalleryGame? mostMw    = null;
        GalleryGame? geniuses  = null;
        double       bestShare = 0;
        GalleryGame? bestShareGame = null;
        Dictionary<string, HashSet<ScenarioId>> mwScenarios = new();
        string?      allTrades = null;
        GalleryGame? allTradesGame = null;

        foreach (GalleryGame game in ordered)
        {
            string[] masterworks = game.Masterworks.Where(game.Players.Contains).Distinct().ToArray();
            if (masterworks.Length == 0) continue;

            firstMw ??= game;
            total += masterworks.Length;
            if (total >= _MANY_MASTERWORKS) manyMw ??= game;
            if (total >= _MOST_MASTERWORKS) mostMw ??= game;

            double share = masterworks.Length / (double)Math.Max(1, game.Players.Length);
            if (share >= 1) geniuses ??= game;
            if (share > bestShare)
            {
                bestShare     = share;
                bestShareGame = game;
            }

            foreach (string player in masterworks)
            {
                if (!mwScenarios.TryGetValue(player, out HashSet<ScenarioId>? set)) mwScenarios[player] = set = [];
                set.Add(game.Scenario);
                if (set.Count >= GalleryTrees.Scenarios.Length && allTrades == null)
                {
                    allTrades     = player;
                    allTradesGame = game;
                }
            }
        }

        list.Add(new Achievement("MwFirst", AchievementGroup.Masterworks, false, "✦", SealKind.Red, firstMw != null,
                                 firstMw != null ? WhoWhen(string.Join(", ", firstMw.Masterworks), firstMw) : Notyet()));
        list.Add(new Achievement("MwMany", AchievementGroup.Masterworks, false, _MANY_MASTERWORKS.ToString(), SealKind.Red, manyMw != null,
                                 manyMw != null ? When(manyMw) : OfCount(total, _MANY_MASTERWORKS), Math.Min(1, total / (double)_MANY_MASTERWORKS)));
        list.Add(new Achievement("MwMost", AchievementGroup.Masterworks, false, _MOST_MASTERWORKS.ToString(), SealKind.Red, mostMw != null,
                                 mostMw != null ? When(mostMw) : OfCount(Math.Min(total, _MOST_MASTERWORKS), _MOST_MASTERWORKS), Math.Min(1, total / (double)_MOST_MASTERWORKS)));
        list.Add(new Achievement("MwEveryone", AchievementGroup.Masterworks, false, "✦✦", SealKind.Red, geniuses != null,
                                 geniuses != null ? When(geniuses)
                                 : bestShareGame != null ? Best($"{bestShareGame.Masterworks.Count(bestShareGame.Players.Contains)} / {bestShareGame.Players.Length}")
                                 : Notyet()));
        int tradesBest = mwScenarios.Count == 0 ? 0 : mwScenarios.Values.Max(set => set.Count);
        string tradesLeader = mwScenarios.FirstOrDefault(pair => pair.Value.Count == tradesBest).Key ?? string.Empty;
        list.Add(new Achievement("MwAllScenarios", AchievementGroup.Masterworks, false, "3", SealKind.Red, allTrades != null,
                                 allTrades != null ? WhoWhen(allTrades, allTradesGame!)
                                 : tradesBest > 0 ? Best($"{tradesLeader}, {tradesBest} / 3") : Notyet(),
                                 tradesBest / 3.0));

        // Scores and the family
        int topScore = games.SelectMany(game => game.Players.Select(player => game.Scores.GetValueOrDefault(player))).DefaultIfEmpty(0).Max();
        foreach (int level in ScoreLevels)
        {
            (GalleryGame game, string player)? hit = null;
            foreach (GalleryGame game in ordered)
            {
                string? player = game.Players.Where(p => game.Scores.GetValueOrDefault(p) >= level).OrderByDescending(p => game.Scores.GetValueOrDefault(p)).FirstOrDefault();
                if (player == null) continue;
                hit = (game, player);
                break;
            }

            list.Add(new Achievement($"Score{level}", AchievementGroup.Scores, false, level.ToString(), SealKind.Gold, hit != null,
                                     hit != null ? WhoWhen(hit.Value.player, hit.Value.game) : topScore > 0 ? Best(topScore.ToString()) : Notyet(),
                                     Math.Min(1, topScore / (double)level)));
        }

        GalleryGame? family = ordered.FirstOrDefault(game => game.FamilyWinner);
        list.Add(new Achievement("FamilyWin", AchievementGroup.Scores, false, "❦", SealKind.Gold, family != null,
                                 family != null ? WhoWhen(string.Join(", ", family.Players), family) : Notyet()));

        // Dynasty: the same player wins three games in a row (any scenario; a family win breaks the streak)
        int          streak = 0, bestStreak = 0;
        string       last   = string.Empty;
        GalleryGame? dynasty = null;
        string       dynastyPlayer = string.Empty;
        foreach (GalleryGame game in ordered)
        {
            string winner = game.FamilyWinner ? string.Empty : game.Winner;
            streak = winner.Length > 0 && winner == last ? streak + 1 : winner.Length > 0 ? 1 : 0;
            last   = winner;
            bestStreak = Math.Max(bestStreak, streak);
            if (streak >= _DYNASTY && dynasty == null)
            {
                dynasty       = game;
                dynastyPlayer = winner;
            }
        }

        list.Add(new Achievement("Dynasty", AchievementGroup.Scores, false, _DYNASTY.ToString(), SealKind.Gold, dynasty != null,
                                 dynasty != null ? WhoWhen(dynastyPlayer, dynasty) : bestStreak > 0 ? Best(bestStreak.ToString()) : Notyet(),
                                 Math.Min(1, bestStreak / (double)_DYNASTY)));

        return list;
    }
}
