using System.Runtime.CompilerServices;

namespace MyFathersWorkApp;

public class GameplayInputPopup(GlobalData globalData, string placeholder, PopUpButton popUpButton, Func<string, bool> validate, Action<string> onClose, bool placeholderAsTag = false, Func<string, string>? contentFormatter = null, [CallerMemberName] string callerName = "")
{
    public string Message     { get; } = (contentFormatter ?? (content => content))(globalData.GetScenarioLocalizedTag(callerName + "_Content"));
    public string Placeholder { get; } = placeholderAsTag ? globalData.GetScenarioLocalizedTag(placeholder) : placeholder;

    public string ButtonText { get; } = popUpButton switch
    {
        PopUpButton.Confirm => globalData.GetLocalizedUITag(GlobalTags.Gameplay_Confirm),
        _                   => globalData.GetLocalizedUITag(GlobalTags.Gameplay_Accept)
    };

    public Func<string, bool> Validate { get; } = validate;
    public Action<string>     OnClose  { get; } = onClose;

    // Several fields on one page (final scores): a label per field, each value checked by Validate, OnCloseFields gets
    // them in order and replaces OnClose. FieldToggle: label of a tick box next to every field (the Masterwork at score
    // entry); its values come second, in the same order (all false without a label)
    public string[]                  Fields        { get; init; } = [];
    public string                    FieldToggle   { get; init; } = string.Empty;
    public Action<string[], bool[]>? OnCloseFields { get; init; }
}