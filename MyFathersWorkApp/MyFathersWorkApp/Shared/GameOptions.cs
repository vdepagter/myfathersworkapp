using Microsoft.JSInterop;

namespace MyFathersWorkApp;

// Per-device play options kept in localStorage, outside the game save (New Game, load and undo never touch them):
// - Guided mode (mfwGuided): hints at the choices that steer the story (Shared/Guide/GuideService.cs), switchable at any time.
// - Play along with the official app (mfwMirror): a tester option, read when a new game starts (mirror mode,
//   Shared/GlobalData.Mirror.cs); a running game keeps the mode it started with.
// UI code only, never scenario code.
public sealed class GameOptions(IJSRuntime js)
{
    private const string _GUIDED_KEY = "mfwGuided";
    private const string _MIRROR_KEY = "mfwMirror";
    private const string _FOLDED_KEY = "mfwGuideFolded";

    private bool? _Guided;
    private bool? _Mirror;
    private bool? _Folded;

    public bool GuidedMode => _Guided ??= Read(_GUIDED_KEY);
    public bool MirrorMode => _Mirror ??= Read(_MIRROR_KEY);

    // The route card on round screens folded to its heading (Shared/GuideBox.razor)
    public bool GuideFolded => _Folded ??= Read(_FOLDED_KEY);

    public void SetGuideFolded(bool folded)
    {
        _Folded = folded;
        Write(_FOLDED_KEY, folded);
    }

    // Raised when an option changes, so an open game page can redraw its hints
    public event Action? Changed;

    public void SetGuidedMode(bool on)
    {
        _Guided = on;
        Write(_GUIDED_KEY, on);
        Changed?.Invoke();
    }

    public void SetMirrorMode(bool on)
    {
        _Mirror = on;
        Write(_MIRROR_KEY, on);
        Changed?.Invoke();
    }

    private bool Read(string key)
    {
        try
        {
            return js is IJSInProcessRuntime inProcess && inProcess.Invoke<string?>("localStorage.getItem", key) == "1";
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Options: could not read {key} ({exception.Message})");
            return false;
        }
    }

    private void Write(string key, bool on)
    {
        try
        {
            if (js is IJSInProcessRuntime inProcess) inProcess.InvokeVoid("localStorage.setItem", key, on ? "1" : "0");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Options: could not store {key} ({exception.Message})");
        }
    }
}
