namespace MyFathersWorkApp;

public class GameplayElement(string content, Action<GlobalData>? callback = null, bool fromNewLine = false, bool endOfRound = false)
{
    public string              Content     { get; } = content;
    public Action<GlobalData>? Callback    { get; } = callback;
    public bool                FromNewLine { get; } = fromNewLine;

    // The link that ends the round or the Generation on a hub: Pages/Gameplay.razor adds the End of Round button under it
    // and the jump-down chip (js/transitions.js) that scrolls to it
    public bool                EndOfRound  { get; } = endOfRound;
}