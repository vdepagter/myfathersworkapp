// Music and sound effects (docs/AUDIO_TRANSITIONS_MAP.md 5.1.6), driven from C# by Shared/AudioService.cs.
// Channels as in the Unity SoundManager:
//   music   - one element, loops (except the ending track), only switches when the wanted track differs
//   effects - one at a time, a new effect cuts the previous one (screen transitions, welcome, footsteps)
//   popup   - one at a time, a new popup sound cuts the previous one (setup, end of round, special event)
//   click   - may overlap
//   voice   - one element for the voice-over: thunder lead-in, then the narration clip (docs/AUDIO_TRANSITIONS_MAP.md
//             "Voice-over"), or the clips of the fan-made Oliver narration played one after the other. Own setting
//             (female / male / oliver / off), independent of the music and effects toggles; the
//             music is ducked while it plays. State changes are reported to one .NET listener (Pages/Gameplay.razor).
// Volumes (pause menu sliders): music, effects and speech, 0-100 each, stored per device. The effects volume scales
// every effect (page flips stay at 60% of it), the speech volume the narration and its thunder lead-in.
// Browsers block audio until the first user gesture: the wanted music starts on the first gesture, effects requested
// before it are dropped. Paths are relative so they resolve against <base href> (the site runs from a sub-path).
(function () {
    "use strict";

    const EFFECTS = ["sfx-click", "sfx-transition-cover", "sfx-transition-reveal", "sfx-setup", "sfx-end-of-round",
                     "sfx-special-event", "sfx-welcome", "sfx-footsteps", "sfx-bid-open", "sfx-reveal"];
    const VOLUMES       = { "sfx-transition-cover": 0.6, "sfx-transition-reveal": 0.6 }; // page flips: 60% (play test)
    const MUSIC_FADE_MS = 400;
    const KEY_EFFECTS   = "audio.effects";
    const KEY_MUSIC     = "audio.music";
    const KEY_VOICE     = "audio.voice";
    const KEY_VOLUME    = "audio.volume."; // + music | effects | voice

    const src = name => "audio/" + name + ".mp3";

    function readSetting(key) {
        try {
            return localStorage.getItem(key) !== "0";
        } catch (e) {
            return true;
        }
    }

    function writeSetting(key, on) {
        try {
            localStorage.setItem(key, on ? "1" : "0");
        } catch (e) {
            // Storage unavailable (private mode, blocked): keep the setting for this session only
        }
    }

    function tryPlay(element) {
        try {
            const promise = element.play();
            if (promise && promise.catch) promise.catch(() => { });
        } catch (e) {
            // Ignore: autoplay blocked or unsupported format
        }
    }

    function stop(element) {
        if (!element) return;
        try {
            element.pause();
            element.currentTime = 0;
        } catch (e) {
            // Ignore
        }
    }

    // Until a player moves a slider: music 80%, effects 40%, speech 100%
    const DEFAULT_VOLUMES = { music: 80, effects: 40, voice: 100 };

    function readVolume(kind) {
        try {
            const value = parseInt(localStorage.getItem(KEY_VOLUME + kind), 10);
            return isNaN(value) ? DEFAULT_VOLUMES[kind] : Math.min(100, Math.max(0, value));
        } catch (e) {
            return DEFAULT_VOLUMES[kind];
        }
    }

    const settings = { effects: readSetting(KEY_EFFECTS), music: readSetting(KEY_MUSIC) };
    const volumes  = { music: readVolume("music"), effects: readVolume("effects"), voice: readVolume("voice") };
    let unlocked = false;

    // ---- Effects --------------------------------------------------------------------------------------------------

    const clips = {};
    for (const name of EFFECTS) {
        const element   = new Audio();
        element.preload = "auto";
        element.src     = src(name);
        clips[name]     = element;
    }

    function applyEffectsVolume() {
        for (const name of EFFECTS) clips[name].volume = (VOLUMES[name] ?? 1) * volumes.effects / 100;
    }

    applyEffectsVolume();

    const channels = {
        effects: { current: null, timer: 0 },
        popup:   { current: null, timer: 0 }
    };

    function startOnChannel(channel, name) {
        const clip = clips[name];
        if (!clip || !settings.effects) return;
        if (channel.current && channel.current !== clip) stop(channel.current);
        channel.current = clip;
        try {
            clip.currentTime = 0;
        } catch (e) {
            // Ignore: not loaded yet
        }
        tryPlay(clip);
    }

    // channel: "effects" | "popup" | "click"; delayMs: start later (a newer request on the channel cancels it)
    function play(name, channelName, delayMs) {
        if (!settings.effects || !clips[name]) return;
        if (!unlocked) {
            if (channelName === "effects" && name === "sfx-welcome") pendingWelcome = true; // played on the first gesture
            return;
        }

        if (channelName === "click") {
            const clone  = clips[name].cloneNode();
            clone.volume = clips[name].volume; // volume is not an attribute: cloneNode does not copy it
            tryPlay(clone);
            return;
        }

        const channel = channels[channelName];
        if (!channel) return;
        clearTimeout(channel.timer);
        channel.timer = 0;

        if (delayMs > 0) {
            channel.timer = setTimeout(() => {
                channel.timer = 0;
                startOnChannel(channel, name);
            }, delayMs);
            return;
        }

        startOnChannel(channel, name);
    }

    function stopEffects() {
        for (const channel of Object.values(channels)) {
            clearTimeout(channel.timer);
            channel.timer = 0;
            stop(channel.current);
            channel.current = null;
        }
    }

    // ---- Music ----------------------------------------------------------------------------------------------------

    const music   = new Audio();
    music.preload = "none";

    let wanted      = null;      // { name, loop } or null (silence)
    let currentName = null;      // track loaded in the music element
    let switchingTo;             // track name (or null) a fade is switching to; undefined when no switch runs
    let fadeTimer   = 0;
    let musicEnded  = false;     // a non-looping track played to the end: do not restart it
    let musicGain   = 1;         // 1, or MUSIC_DUCK while the voice-over plays (times the music volume: musicLevel)
    let duckTimer   = 0;

    music.addEventListener("ended", () => { musicEnded = true; });

    // Full volume of the music element: the music volume setting, ducked while the voice-over plays
    const musicLevel = () => musicGain * volumes.music / 100;

    function cancelFade() {
        clearInterval(fadeTimer);
        fadeTimer   = 0;
        switchingTo = undefined;
    }

    function fadeOut(done) {
        clearInterval(duckTimer);
        duckTimer = 0;

        if (music.paused || currentName === null) {
            done();
            return;
        }

        const step = 1 / (MUSIC_FADE_MS / 40);
        fadeTimer = setInterval(() => {
            const volume = Math.max(0, music.volume - step);
            music.volume = volume;
            if (volume > 0) return;
            clearInterval(fadeTimer);
            fadeTimer = 0;
            done();
        }, 40);
    }

    function applyMusic() {
        if (!unlocked) return;

        if (!settings.music) {
            cancelFade();
            clearInterval(duckTimer);
            duckTimer = 0;
            music.pause();
            music.volume = musicLevel();
            return;
        }

        const targetName = wanted ? wanted.name : null;
        if (switchingTo !== undefined && switchingTo === targetName) return;

        if (switchingTo === undefined && currentName === targetName) {
            if (targetName !== null && music.paused && !musicEnded) tryPlay(music);
            return;
        }

        cancelFade();
        switchingTo  = targetName;
        const target = wanted;

        fadeOut(() => {
            switchingTo = undefined;
            music.pause();
            music.volume = musicLevel();
            musicEnded   = false;

            if (!target) {
                currentName = null;
                music.removeAttribute("src");
                return;
            }

            currentName = target.name;
            music.loop  = target.loop;
            music.src   = src(target.name);
            tryPlay(music);
        });
    }

    // name: track file name without extension, or null/"" for silence
    function setMusic(name, loop) {
        const targetName = name || null;
        if ((wanted ? wanted.name : null) === targetName) return;
        wanted = targetName ? { name: targetName, loop: loop !== false } : null;
        applyMusic();
    }

    // ---- Music ducking while the voice-over plays ------------------------------------------------------------------

    const MUSIC_DUCK = 0.3;

    function duckMusic(on) {
        const gain = on ? MUSIC_DUCK : 1;
        if (musicGain === gain) return;
        musicGain = gain;
        rampMusic();
    }

    // Moves the music element to musicLevel() in a short ramp (ducking, a volume change)
    function rampMusic() {
        const target = musicLevel();
        clearInterval(duckTimer);
        duckTimer = 0;
        if (fadeTimer) return; // a track switch is fading out; it starts the next track at musicLevel()

        // Short ramp instead of a jump
        duckTimer = setInterval(() => {
            const volume = music.volume;
            const next   = volume < target ? Math.min(target, volume + 0.07) : Math.max(target, volume - 0.07);
            music.volume = next;
            if (next !== target) return;
            clearInterval(duckTimer);
            duckTimer = 0;
        }, 30);
    }

    // ---- Voice-over -----------------------------------------------------------------------------------------------
    // Unity (SoundManager.OnVOPlayOneTime): 4.5 s of the thunder clip, then the narration clip on the same source.
    // Pause menu: pause; resume during the lead-in skips the rest of it (SoundManager.OnVOResume).

    const VOICES        = ["female", "male", "oliver", "off"];
    const VOICE_DEFAULT = "oliver"; // Oliver (owner, 30 Oct 2026); Unity defaulted to male (PlayerPrefs "AudioTrack" 0)
    const LEAD_IN_MS    = 4500;
    const THUNDER       = src("sfx-vo-thunder");

    function readVoice() {
        try {
            const value = localStorage.getItem(KEY_VOICE);
            return VOICES.indexOf(value) >= 0 ? value : VOICE_DEFAULT;
        } catch (e) {
            return VOICE_DEFAULT;
        }
    }

    const voiceElement   = new Audio();
    voiceElement.preload = "none";
    voiceElement.volume  = volumes.voice / 100;

    const voice = {
        setting:  readVoice(),
        id:       0,        // current request (0 = none); reported with every state change
        counter:  0,
        phase:    null,     // null | "leadin" | "clip"
        clip:     null,     // url of the narration clip of the current request
        queue:    [],       // urls still to play after clip (Oliver narration: one clip per text chunk)
        list:     [],       // every clip of the request, in order (progress bar and seeking)
        index:    0,        // position of clip in list
        lengths:  [],       // duration in seconds per clip of list, NaN until its metadata is loaded
        texts:    [],       // on-screen text (CSV markup) per clip of list, highlighted while it plays (Oliver only)
        timer:    0,        // lead-in timer
        paused:   false,    // paused by the pause menu
        listener: null      // DotNetObjectReference (Pages/Gameplay.razor), method OnVoiceState(id, state)
    };

    function notifyVoice(id, state) {
        if (!voice.listener || !id) return;
        try {
            const promise = voice.listener.invokeMethodAsync("OnVoiceState", id, state);
            if (promise && promise.catch) promise.catch(() => { });
        } catch (e) {
            // Listener already disposed
        }
    }

    function startVoiceSource(url) {
        const id    = voice.id;
        const phase = voice.phase;
        voiceElement.src = url;
        try {
            const promise = voiceElement.play();
            if (promise && promise.catch) {
                promise.catch(() => {
                    // Blocked or failed (an interrupted play() rejects too: ignore when the request moved on)
                    if (voice.id === id && voice.phase === phase && !voice.paused) endVoice();
                });
            }
        } catch (e) {
            endVoice();
        }
    }

    function startClip() {
        clearTimeout(voice.timer);
        voice.timer = 0;
        voice.phase = "clip";
        startVoiceSource(voice.clip);
    }

    // Stops the current request (if any) and reports "stopped" for it
    function endVoice() {
        const id = voice.id;
        clearTimeout(voice.timer);
        voice.timer  = 0;
        voice.id     = 0;
        voice.phase  = null;
        voice.clip   = null;
        voice.queue  = [];
        voice.list   = [];
        voice.index  = 0;
        voice.lengths = [];
        voice.texts  = [];
        voice.paused = false;
        clearHighlight();
        try {
            voiceElement.pause();
            voiceElement.removeAttribute("src");
            voiceElement.load();
        } catch (e) {
            // Ignore
        }
        duckMusic(false);
        notifyVoice(id, "stopped");
    }

    // name: clip file name (the Unity passage name); which: "female" | "male";
    // leadIn: thunder first.
    // Returns the request id, or 0 when nothing starts (audio not unlocked by a user gesture yet).
    function voicePlay(name, which, leadIn) {
        endVoice();
        if (!unlocked || !name) return 0;

        voice.id   = ++voice.counter;
        voice.clip = "audio/vo/" + (which === "male" ? "male" : "female") + "/" + name + ".mp3";
        setVoiceList([voice.clip]);
        duckMusic(true);

        if (leadIn) {
            voice.phase = "leadin";
            startVoiceSource(THUNDER);
            voice.timer = setTimeout(startClip, LEAD_IN_MS);
        } else {
            startClip();
        }

        return voice.id;
    }

    // urls: the clips of one Oliver narration, played in order without a lead-in; texts: what each clip says on screen.
    // Returns the request id, or 0.
    function voicePlayList(urls, texts) {
        endVoice();
        if (!unlocked || !urls || !urls.length) return 0;

        voice.id    = ++voice.counter;
        voice.texts = texts || [];
        loadPauses(urls[0]);
        voice.clip  = urls[0];
        voice.queue = urls.slice(1);
        setVoiceList(urls);
        duckMusic(true);
        startClip();
        return voice.id;
    }

    // Loads the duration of every clip of the request (metadata only) for the progress bar
    function setVoiceList(urls) {
        voice.list    = urls.slice();
        voice.index   = 0;
        voice.lengths = urls.map(() => NaN);
        const id = voice.id;
        urls.forEach((url, i) => {
            const probe   = new Audio();
            probe.preload = "metadata";
            probe.addEventListener("loadedmetadata", () => {
                if (voice.id === id && isFinite(probe.duration)) voice.lengths[i] = probe.duration;
            }, { once: true });
            probe.src = url;
        });
    }

    // Length of clip i: its duration once known, else the average of the known ones (10 s when none is known)
    function clipLength(i) {
        if (isFinite(voice.lengths[i])) return voice.lengths[i];
        const known = voice.lengths.filter(isFinite);
        return known.length ? known.reduce((a, b) => a + b, 0) / known.length : 10;
    }

    function voiceTotal() {
        let total = 0;
        for (let i = 0; i < voice.list.length; i++) total += clipLength(i);
        return total;
    }

    // Seconds from the start of the narration (0 during the thunder lead-in)
    function voicePosition() {
        if (voice.phase !== "clip") return 0;
        let pos = 0;
        for (let i = 0; i < voice.index; i++) pos += clipLength(i);
        return pos + (voiceElement.currentTime || 0);
    }

    // Jumps to the given second of the narration (across clips); past the end stops it
    function voiceSeek(seconds) {
        if (!voice.phase || !voice.list.length) return;
        let target = Math.max(0, seconds);
        let i = 0;
        while (i < voice.list.length && target >= clipLength(i)) {
            target -= clipLength(i);
            i++;
        }
        if (i >= voice.list.length) {
            endVoice();
            return;
        }
        clearTimeout(voice.timer);
        voice.timer = 0;
        if (voice.phase === "clip" && i === voice.index) {
            try { voiceElement.currentTime = target; } catch (e) { /* not seekable yet */ }
            return;
        }
        voice.phase = "clip";
        voice.index = i;
        voice.clip  = voice.list[i];
        voice.queue = voice.list.slice(i + 1);
        const offset = target;
        voiceElement.addEventListener("loadedmetadata", () => {
            try { voiceElement.currentTime = offset; } catch (e) { /* ignore */ }
        }, { once: true });
        if (voice.paused) {
            voiceElement.src = voice.clip; // stays paused at the new place until the pause menu closes
            return;
        }
        startVoiceSource(voice.clip);
    }

    // ---- Narration progress bar (Pages/Gameplay.razor .narration-progress): drawn here, no round trip to Blazor ----
    // The bar gets --p (0-1); a click or drag on it seeks; buttons with data-voice-skip="-10" / "10" jump.
    function drawProgress() {
        const bars = document.querySelectorAll(".narration-progress");
        if (bars.length) {
            const total = voiceTotal();
            const p     = total > 0 ? Math.min(1, voicePosition() / total) : 0;
            const text  = formatTime(voicePosition()) + " / " + formatTime(total);
            bars.forEach(bar => {
                bar.style.setProperty("--p", p.toFixed(4));
                bar.setAttribute("aria-valuenow", String(Math.round(p * 100)));
                bar.setAttribute("aria-valuetext", text);
                const label = bar.parentElement && bar.parentElement.querySelector(".narration-time");
                if (label) label.textContent = text;
            });
        }
        syncHighlight();
        requestAnimationFrame(drawProgress);
    }
    requestAnimationFrame(drawProgress);

    function formatTime(s) {
        s = Math.max(0, Math.round(s));
        return Math.floor(s / 60) + ":" + String(s % 60).padStart(2, "0");
    }

    function seekFromPointer(bar, clientX) {
        const rect = bar.getBoundingClientRect();
        const f    = Math.min(1, Math.max(0, (clientX - rect.left) / rect.width));
        voiceSeek(f * voiceTotal() * 0.999);
    }

    let dragging = null;
    document.addEventListener("pointerdown", e => {
        const bar = e.target instanceof Element && e.target.closest(".narration-progress");
        if (!bar) return;
        dragging = bar;
        seekFromPointer(bar, e.clientX);
    });
    document.addEventListener("pointermove", e => { if (dragging) seekFromPointer(dragging, e.clientX); });
    document.addEventListener("pointerup", () => { dragging = null; });
    document.addEventListener("click", e => {
        const button = e.target instanceof Element && e.target.closest("[data-voice-skip]");
        if (button) voiceSeek(voicePosition() + Number(button.getAttribute("data-voice-skip")));
    });
    document.addEventListener("keydown", e => {
        const bar = e.target instanceof Element && e.target.closest(".narration-progress");
        if (!bar) return;
        if (e.key === "ArrowLeft")  { voiceSeek(voicePosition() - 5); e.preventDefault(); }
        if (e.key === "ArrowRight") { voiceSeek(voicePosition() + 5); e.preventDefault(); }
    });

    let preload = null; // warms the browser cache for the next clip of a list, so the gap between clips stays short

    voiceElement.addEventListener("playing", () => {
        if (voice.phase !== "clip") return;
        highlightClip(voice.texts[voice.index]);
        if (voice.queue.length) {
            preload         = new Audio();
            preload.preload = "auto";
            preload.src     = voice.queue[0];
        }
        notifyVoice(voice.id, "speaking");
    });
    voiceElement.addEventListener("ended", () => {
        if (voice.phase === "clip" && voice.queue.length) {
            voice.clip = voice.queue.shift();
            voice.index++;
            startVoiceSource(voice.clip);
        }
        else if (voice.phase === "clip") endVoice();
        else if (voice.phase === "leadin") startClip(); // lead-in clip shorter than LEAD_IN_MS
    });
    voiceElement.addEventListener("error", () => {
        // Missing or unsupported file (not the src removal in endVoice)
        if (voice.phase && voiceElement.getAttribute("src")) endVoice();
    });

    // ---- Highlight of the sentence the narration is speaking, kept in view -----------------------------------------
    // The clip's text is looked up in the story window (title and parchment) after the previous clip's text. A clip
    // can hold a whole page, so its text is split into sentences and the one being spoken is marked: js/narration-sync.js
    // works out when each sentence starts from the pauses of the clip (audio/vo/oliver/<Scenario>.pauses.json), or from
    // the text length when the clip has no pause list. The mark uses the CSS Custom Highlight API
    // (::highlight(mfw-narration), css/app.css), so the page DOM stays as Blazor rendered it. Browsers without the API
    // only scroll. The page scrolls when the sentence nears the edge of the view, but not for a few seconds after the
    // reader scrolled themself.

    const HIGHLIGHT = "mfw-narration";
    const places = { id: 0, root: null, ends: [] };   // where each clip's text ended in the window text (current request)
    let reading = null;             // the clip on screen: { index, markup, parts, ranges, times, exact, shown }
    let readerScrolled = 0;         // time of the reader's last own scroll
    for (const type of ["wheel", "touchmove"]) window.addEventListener(type, () => { readerScrolled = Date.now(); }, { passive: true });

    // Pause lists per scenario folder: undefined = not asked, null = loading, object = { hash: [duration, start, end, ...] }
    const pauseLists = {};
    const CLIP_URL = /^(audio\/vo\/oliver\/[^\/]+)\/([0-9a-f]+)\.mp3/;

    function loadPauses(url) {
        const match = CLIP_URL.exec(url || "");
        if (!match || pauseLists[match[1]] !== undefined) return;
        pauseLists[match[1]] = null;
        fetch(match[1] + ".pauses.json")
            .then(response => response.ok ? response.json() : {})
            .then(list => { pauseLists[match[1]] = list || {}; })
            .catch(() => { pauseLists[match[1]] = {}; });
    }

    // The pauses of a clip: an array, null when it has none (or another length: rendered again), undefined while loading
    function clipPauses(url, duration) {
        const match = CLIP_URL.exec(url || "");
        if (!match) return null;
        const list = pauseLists[match[1]];
        if (!list) return list === null ? undefined : null;
        const pauses = list[match[2]];
        return pauses && Math.abs(pauses[0] - duration) < 0.3 ? pauses : null;
    }

    function clearHighlight() {
        reading = null;
        try { if (window.CSS && CSS.highlights) CSS.highlights.delete(HIGHLIGHT); } catch (e) { /* ignore */ }
    }

    function plainText(markup) {
        const div = document.createElement("div");
        div.innerHTML = String(markup).replace(/<br\s*\/?>/gi, " ").replace(/<(icon|sprite)=[^>]*>/gi, " ");
        return div.textContent.replace(/\s+/g, " ").trim();
    }

    // Text nodes of the story window with their place in the whitespace-collapsed text. A line break or the start of a
    // block counts as a space, as in plainText().
    function windowText(root) {
        const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT | NodeFilter.SHOW_ELEMENT, {
            acceptNode: node => {
                const element = node.nodeType === 1 ? node : node.parentElement;
                return element && element.closest(".narration-pill, .story-link, button") ? NodeFilter.FILTER_REJECT : NodeFilter.FILTER_ACCEPT;
            }
        });
        let text = "";
        const map = [];  // per character of text: [node, offset]
        let space = true;
        for (let node = walker.nextNode(); node; node = walker.nextNode()) {
            if (node.nodeType === 1) {
                if (!space && map.length && /^(BR|P|DIV|H[1-6]|LI|UL|OL|TABLE|TR)$/.test(node.nodeName)) {
                    space = true;
                    text += " ";
                    map.push(map[map.length - 1]);
                }
                continue;
            }
            const value = node.nodeValue;
            for (let i = 0; i < value.length; i++) {
                if (/\s/.test(value[i])) {
                    if (space) continue;
                    space = true;
                    text += " ";
                } else {
                    space = false;
                    text += value[i];
                }
                map.push([node, i]);
            }
        }
        return { text, map };
    }

    // Finds the text of the clip that starts to play and prepares one range per sentence
    function highlightClip(markup) {
        clearHighlight();
        const root = document.querySelector(".gameplay-container");
        if (!root || !markup || !window.mfwNarrationSync) return;
        const target = plainText(markup);
        if (!target) return;
        if (places.id !== voice.id || places.root !== root) {
            places.id   = voice.id;
            places.root = root;
            places.ends = [];
        }
        let from = 0;   // after the nearest earlier clip that was found
        for (let i = voice.index - 1; i >= 0 && !from; i--) from = places.ends[i] || 0;

        const { text, map } = windowText(root);
        let at = text.indexOf(target, from);
        if (at < 0) at = text.indexOf(target);
        let length = target.length;
        if (at < 0) {  // the shown text differs a little (a name written differently): match its start
            const head = target.slice(0, 40);
            at = text.indexOf(head, from);
            if (at < 0) return;
            length = Math.min(target.length, text.length - at);
        }
        places.ends[voice.index] = at + length;

        const parts  = mfwNarrationSync.sentences(markup, plainText);
        const ranges = [];
        let offset = 0;
        for (const part of parts) {
            const begin = Math.min(offset, length - 1), end = Math.min(offset + part.text.length, length);
            const first = map[at + begin], last = map[Math.min(at + Math.max(end, begin + 1), map.length) - 1];
            const range = document.createRange();
            range.setStart(first[0], first[1]);
            range.setEnd(last[0], last[1] + 1);
            ranges.push(range);
            offset += part.text.length + 1;
        }
        if (!ranges.length) return;
        reading = { index: voice.index, markup, parts, ranges, times: null, exact: false, shown: -1 };
        syncHighlight();
    }

    // Marks the sentence at the clip's current time (every animation frame; does nothing while it stays the same)
    function syncHighlight() {
        if (!reading || voice.phase !== "clip" || reading.index !== voice.index) return;
        if (!reading.ranges[0].startContainer.isConnected) {  // the page was rendered again: find the text again
            highlightClip(reading.markup);
            return;
        }

        const duration = voiceElement.duration;
        let shown = 0;
        if (isFinite(duration) && duration > 0) {
            if (!reading.exact) {
                const pauses = clipPauses(voice.clip, duration);   // undefined: the pause list is still loading
                if (!reading.times || pauses !== undefined) reading.times = mfwNarrationSync.starts(reading.parts, duration, pauses || null);
                reading.exact = pauses !== undefined;
            }
            const now = voiceElement.currentTime || 0;
            while (shown + 1 < reading.times.length && reading.times[shown + 1] <= now) shown++;
        }
        if (shown === reading.shown) return;
        reading.shown = shown;

        const range = reading.ranges[shown];
        try { if (window.CSS && CSS.highlights && window.Highlight) CSS.highlights.set(HIGHLIGHT, new Highlight(range)); } catch (e) { /* ignore */ }
        keepInView(range);
    }

    // Scrolls when the sentence is above the view or reaches its lower part, so the reader sees what comes next
    function keepInView(range) {
        if (Date.now() - readerScrolled < 4000) return;
        const rect = range.getBoundingClientRect();
        if (!rect || (rect.top === 0 && rect.bottom === 0)) return;
        const top = 90, bottom = window.innerHeight - 60;   // below the header buttons, above the bottom edge
        const height = bottom - top;
        if (rect.top >= top && rect.bottom <= top + height * 0.7) return;
        if (rect.top >= top && rect.top <= top + height * 0.3) return;   // a sentence too long for that: fine near the top
        glideBy(scrollParent(range.startContainer.parentElement), rect.top - (top + height * 0.2));
    }

    // Scrolls by delta pixels in a short glide. Animated here: the browser's own smooth scroll jumps when the device
    // asks for reduced motion. The reader's own scroll stops it.
    let glide = 0;
    function glideBy(scroller, delta) {
        cancelAnimationFrame(glide);
        const page    = scroller === window;
        const from    = page ? window.scrollY : scroller.scrollTop;
        const time    = Math.min(900, Math.max(450, Math.abs(delta) * 2));   // milliseconds
        const started = performance.now(), asked = Date.now();
        const step = now => {
            if (readerScrolled >= asked) return;
            const p     = Math.min(1, (now - started) / time);
            const eased = p < 0.5 ? 2 * p * p : 1 - (2 - 2 * p) * (2 - 2 * p) / 2;
            (page ? window : scroller).scrollTo({ top: from + delta * eased, behavior: "instant" });
            if (p < 1) glide = requestAnimationFrame(step);
        };
        glide = requestAnimationFrame(step);
    }

    function scrollParent(element) {
        for (let e = element; e && e !== document.body; e = e.parentElement) {
            const overflow = getComputedStyle(e).overflowY;
            if ((overflow === "auto" || overflow === "scroll") && e.scrollHeight > e.clientHeight) return e;
        }
        return window;
    }

    function voicePause() {
        if (!voice.phase || voice.paused) return;
        voice.paused = true;
        clearTimeout(voice.timer);
        voice.timer = 0;
        try {
            voiceElement.pause();
        } catch (e) {
            // Ignore
        }
    }

    function voiceResume() {
        if (!voice.paused) return;
        voice.paused = false;
        if (voice.phase === "leadin") {
            startClip();
            return;
        }
        if (voice.phase === "clip") {
            const id = voice.id;
            try {
                const promise = voiceElement.play();
                if (promise && promise.catch) promise.catch(() => { if (voice.id === id && !voice.paused) endVoice(); });
            } catch (e) {
                endVoice();
            }
        }
    }

    // ---- Unlock on the first user gesture -------------------------------------------------------------------------

    let pendingWelcome = false; // the welcome bell asked for before the browser allowed sound (main menu, cold start)

    function onGesture() {
        const first = !unlocked;
        unlocked = true;
        applyMusic();
        if (first && pendingWelcome) {
            pendingWelcome = false;
            play("sfx-welcome", "effects", 0);
        }
    }

    for (const type of ["pointerdown", "pointerup", "keydown", "touchend"]) {
        document.addEventListener(type, onGesture, { capture: true, passive: true });
    }

    // ---- API ------------------------------------------------------------------------------------------------------

    window.mfwAudio = {
        play: play,
        setMusic: setMusic,
        isEffectsOn: () => settings.effects,
        isMusicOn: () => settings.music,
        setEffectsOn: on => {
            settings.effects = !!on;
            writeSetting(KEY_EFFECTS, settings.effects);
            if (!settings.effects) stopEffects();
        },
        setMusicOn: on => {
            settings.music = !!on;
            writeSetting(KEY_MUSIC, settings.music);
            applyMusic();
        },
        // kind: "music" | "effects" | "voice"; value 0-100
        getVolume: kind => volumes[kind] ?? 100,
        setVolume: (kind, value) => {
            if (!(kind in volumes)) return;
            volumes[kind] = Math.min(100, Math.max(0, Math.round(Number(value) || 0)));
            try {
                localStorage.setItem(KEY_VOLUME + kind, String(volumes[kind]));
            } catch (e) {
                // Storage unavailable: keep the volume for this session only
            }
            if (kind === "effects") applyEffectsVolume();
            else if (kind === "voice") voiceElement.volume = volumes.voice / 100;
            else rampMusic();
        },
        isUnlocked: () => unlocked,
        // Voice-over
        getVoice: () => voice.setting,
        // The voice before narration was switched off (the narration toggle switches back to it)
        getLastVoice: () => {
            try {
                const value = localStorage.getItem(KEY_VOICE + ".last");
                return VOICES.indexOf(value) >= 0 && value !== "off" ? value : VOICE_DEFAULT;
            } catch (e) {
                return VOICE_DEFAULT;
            }
        },
        setVoice: which => {
            voice.setting = VOICES.indexOf(which) >= 0 ? which : VOICE_DEFAULT;
            try {
                localStorage.setItem(KEY_VOICE, voice.setting);
                if (voice.setting !== "off") localStorage.setItem(KEY_VOICE + ".last", voice.setting);
            } catch (e) {
                // Storage unavailable: keep the setting for this session only
            }
            endVoice(); // a change stops the current narration, it does not start a new one
        },
        voicePlay: voicePlay,
        voicePlayList: voicePlayList,
        voiceStop: endVoice,
        voiceSeek: voiceSeek,
        cancelWelcome: () => { pendingWelcome = false; },
        voicePause: voicePause,
        voiceResume: voiceResume,
        voiceSubscribe: listener => { voice.listener = listener || null; },
        voiceUnsubscribe: () => { voice.listener = null; },
        // Read-only snapshot for debugging
        state: () => ({
            unlocked: unlocked,
            effectsOn: settings.effects,
            musicOn: settings.music,
            wantedMusic: wanted ? wanted.name : null,
            currentMusic: currentName,
            musicPaused: music.paused,
            musicLoop: music.loop,
            musicVolume: music.volume,
            musicGain: musicGain,
            volumes: Object.assign({}, volumes),
            voice: voice.setting,
            voiceId: voice.id,
            voicePhase: voice.phase,
            voicePaused: voice.paused,
            voiceClip: voice.clip,
            voiceSrc: voiceElement.getAttribute("src"),
            voiceElementPaused: voiceElement.paused,
            voiceTime: voiceElement.currentTime
        })
    };
})();
