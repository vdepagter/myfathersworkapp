namespace MyFathersWorkApp;

// The final ranking shown as a podium by Shared/RankingsView.razor. Entries are sorted best first; Place 0 is first and
// equal players share a place. A family win (the top is still tied after the tie-breakers) puts everyone in first place:
// the family shares the win. Not saved: the window is rebuilt on load and undo.
public record RankingsEntry(string Name, int Score, int Place);

public enum RankingsTieBreak
{
    None,
    Masterwork,
    Upgrades,
    Family
}

public record RankingsData(RankingsEntry[] Entries, RankingsTieBreak TieBreak)
{
    public bool FamilyWin => TieBreak == RankingsTieBreak.Family;
}
