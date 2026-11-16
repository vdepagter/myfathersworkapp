using Microsoft.JSInterop;

namespace MyFathersWorkApp;

// Thin wrapper around wwwroot/js/audio.js (window.mfwAudio). Only UI code calls it (click handlers, Pages/Gameplay.razor
// after a render, Layout/MainLayout.razor on navigation) - never scenario code, which also runs in mirror simulations,
// undo and load. Every call is synchronous (Blazor WebAssembly runs JS in-process) and never throws.
public sealed class AudioService(IJSRuntime js)
{
    private MusicTrack? _RequestedMusic;
    private bool        _WelcomePlayed;

    public bool EffectsOn => Invoke("mfwAudio.isEffectsOn", true);
    public bool MusicOn   => Invoke("mfwAudio.isMusicOn",   true);

    public void Play(SoundCue cue, int delayMs = 0)
    {
        (string? clip, string channel) = cue switch
        {
            SoundCue.Click            => ("sfx-click",             "click"),
            SoundCue.TransitionCover  => ("sfx-transition-cover",  "effects"),
            SoundCue.TransitionReveal => ("sfx-transition-reveal", "effects"),
            SoundCue.Welcome          => ("sfx-welcome",           "effects"),
            SoundCue.Footsteps        => ("sfx-footsteps",         "effects"),
            SoundCue.Setup            => ("sfx-setup",             "popup"),
            SoundCue.EndOfRound       => ("sfx-end-of-round",      "popup"),
            SoundCue.SpecialEvent     => ("sfx-special-event",     "popup"),
            SoundCue.BidOpen          => ("sfx-bid-open",          "popup"),
            SoundCue.Reveal           => ("sfx-reveal",            "popup"),
            _                         => ((string?)null, string.Empty)
        };

        if (clip != null) InvokeVoid("mfwAudio.play", clip, channel, delayMs);
    }

    public void PlayClick() => Play(SoundCue.Click);

    // App start sound, once per app session (dropped by the browser when no user gesture happened yet)
    public void PlayWelcomeOnce()
    {
        if (_WelcomePlayed) return;
        _WelcomePlayed = true;
        Play(SoundCue.Welcome);
    }

    // Declarative: call with the track the current screen wants; the same track again changes nothing
    public void SetMusic(MusicTrack track)
    {
        if (_RequestedMusic == track) return;
        _RequestedMusic = track;

        string? name = track switch
        {
            MusicTrack.Title            => "music-title",
            MusicTrack.CostOfDisease    => "music-cost-of-disease",
            MusicTrack.FearOfTheUnknown => "music-fear-of-the-unknown",
            MusicTrack.Ending           => "music-ending",
            _                           => null
        };

        InvokeVoid("mfwAudio.setMusic", name, track != MusicTrack.Ending);
    }

    public static MusicTrack ScenarioTheme(ScenarioId scenarioId) => scenarioId switch
    {
        ScenarioId.CostOfDisease => MusicTrack.CostOfDisease,
        ScenarioId.FearOfUnknown => MusicTrack.FearOfTheUnknown,
        ScenarioId.TimeOfWar     => MusicTrack.CostOfDisease, // as in Unity (StoryIndex 2 uses the Cost of Disease theme, map 3)
        _                        => MusicTrack.None
    };

    public void SetEffectsOn(bool on) => InvokeVoid("mfwAudio.setEffectsOn", on);
    public void SetMusicOn(bool on)   => InvokeVoid("mfwAudio.setMusicOn",   on);

    // Volume 0-100 per channel (pause menu sliders), stored per device by audio.js
    public int  GetVolume(AudioVolume kind)            => Invoke("mfwAudio.getVolume", 100, VolumeName(kind));
    public void SetVolume(AudioVolume kind, int value) => InvokeVoid("mfwAudio.setVolume", VolumeName(kind), value);

    private static string VolumeName(AudioVolume kind) => kind switch
    {
        AudioVolume.Music   => "music",
        AudioVolume.Effects => "effects",
        _                   => "voice"
    };

    // True after the first user gesture; before it the browser blocks playback
    public bool IsUnlocked => Invoke("mfwAudio.isUnlocked", false);

    #region Voice-over (docs/AUDIO_TRANSITIONS_MAP.md "Voice-over")

    private NarrationVoice? _Narration;

    // Narration setting (localStorage via audio.js); read once, then cached
    public NarrationVoice Narration => _Narration ??= Invoke("mfwAudio.getVoice", "oliver") switch
    {
        "female" => NarrationVoice.Female,
        "male"   => NarrationVoice.Male,
        "off"    => NarrationVoice.Off,
        _        => NarrationVoice.Oliver
    };

    // Stores the setting and stops the current narration (a change never starts one)
    public void SetNarration(NarrationVoice narration)
    {
        _Narration = narration;
        InvokeVoid("mfwAudio.setVoice", VoiceName(narration));
        NarrationChanged?.Invoke();
    }

    // How the page follows the Oliver narration (audio.js SCROLL_MODES, stored per device)
    public static readonly string[] ScrollModes = ["lead", "flow", "page"];
    public string NarrationScroll               => Invoke("mfwAudio.getScroll", ScrollModes[0]);
    public void   SetNarrationScroll(string mode) => InvokeVoid("mfwAudio.setScroll", mode);

    // How the sentence Oliver is speaking is marked (audio.js MARKS, stored per device)
    public static readonly string[] HighlightStyles = ["glow", "wisp", "wispsoft", "firefly", "quill", "emberglow", "candlemotes", "ember", "embersparks", "sparks", "dim", "off"];
    public string NarrationHighlight                 => Invoke("mfwAudio.getHighlight", HighlightStyles[0]);
    public void   SetNarrationHighlight(string mode) => InvokeVoid("mfwAudio.setHighlight", mode);

    // Raised by SetNarration, so a page showing the narration state (Pages/Gameplay.razor) can redraw
    public event Action? NarrationChanged;

    // Narration toggle next to the sound toggles: Off, or back to the voice used before
    public void ToggleNarration() => SetNarration(Narration != NarrationVoice.Off
        ? NarrationVoice.Off
        : Invoke("mfwAudio.getLastVoice", "oliver") switch
        {
            "female" => NarrationVoice.Female,
            "male"   => NarrationVoice.Male,
            _        => NarrationVoice.Oliver
        });

    // Plays the original recording of a narrated window (GameplayWindow.VoiceOver) in the given voice (Male or Female),
    // optionally after the thunder lead-in. Returns the request id reported to the listener, or 0 when nothing started.
    public int VoicePlay(string clip, NarrationVoice voice, bool leadIn) =>
        Invoke("mfwAudio.voicePlay", 0, clip, voice == NarrationVoice.Male ? "male" : "female", leadIn);

    // Plays the Oliver clips of a window (Narration.Clips) one after the other, without a lead-in
    public int VoicePlayList(IReadOnlyList<NarrationClip> clips) =>
        Invoke("mfwAudio.voicePlayList", 0, clips.Select(clip => clip.Url).ToArray(), clips.Select(clip => clip.Text).ToArray());

    public void VoiceStop()   => InvokeVoid("mfwAudio.voiceStop");
    public void VoicePause()  => InvokeVoid("mfwAudio.voicePause");
    public void VoiceResume() => InvokeVoid("mfwAudio.voiceResume");

    // One listener with a [JSInvokable] OnVoiceState(int id, string state) - "speaking" or "stopped"
    public void VoiceSubscribe<T>(DotNetObjectReference<T> listener) where T : class => InvokeVoid("mfwAudio.voiceSubscribe", listener);
    public void VoiceUnsubscribe()                                                     => InvokeVoid("mfwAudio.voiceUnsubscribe");

    private static string VoiceName(NarrationVoice narration) => narration switch
    {
        NarrationVoice.Female => "female",
        NarrationVoice.Off    => "off",
        NarrationVoice.Oliver => "oliver",
        _                     => "male"
    };

    #endregion

    private void InvokeVoid(string identifier, params object?[] args)
    {
        try
        {
            if (js is IJSInProcessRuntime inProcess) inProcess.InvokeVoid(identifier, args);
            else _ = js.InvokeVoidAsync(identifier, args).AsTask().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Audio: {identifier} failed: {exception.Message}");
        }
    }

    private T Invoke<T>(string identifier, T fallback, params object?[] args)
    {
        try
        {
            return js is IJSInProcessRuntime inProcess ? inProcess.Invoke<T>(identifier, args) : fallback;
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Audio: {identifier} failed: {exception.Message}");
            return fallback;
        }
    }
}
