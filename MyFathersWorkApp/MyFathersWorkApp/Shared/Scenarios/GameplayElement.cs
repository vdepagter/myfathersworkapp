namespace MyFathersWorkApp;

public class GameplayElement(string content, Action<GlobalData>? callback = null, bool fromNewLine = false, bool endOfRound = false, string? buttonTag = null)
{
    public string              Content     { get; } = content;
    public Action<GlobalData>? Callback    { get; } = callback;
    public bool                FromNewLine { get; } = fromNewLine;

    // The link that ends the round or the Generation on a hub: Pages/Gameplay.razor adds the End of Round button under it
    // and the jump-down chip (js/transitions.js) that scrolls to it
    public bool                EndOfRound  { get; } = endOfRound;

    // UI tag of the button Pages/Gameplay.razor shows instead of this link: the one way on from a story window
    // ("Click here to continue...", after a bid or vote, the reveal of a secret conversation)
    public string?             ButtonTag   { get; } = buttonTag;
}