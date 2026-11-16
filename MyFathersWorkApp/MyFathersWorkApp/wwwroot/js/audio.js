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
        highlightClip();
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
    // the text length when the clip has no pause list. The marks are drawn over the page or use the CSS Custom
    // Highlight API (see STYLES below), so the page DOM stays as Blazor rendered it. The page scrolls when the sentence
    // nears the edge of the view, but not for a few seconds after the reader scrolled themself.

    // Where each clip's text ended in the window text (current request), and the first and last character found
    // ([node, offset]; all clips once `complete`)
    const places = { id: 0, root: null, ends: [], first: null, last: null, at: 0, to: 0, complete: false };
    let reading = null;             // the clip on screen: { index, parts, ranges, times, exact, shown, led }
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
        clearMarks();
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
                return element && element.closest(".narration-pill, .story-link, button, .guide-note") ? NodeFilter.FILTER_REJECT : NodeFilter.FILTER_ACCEPT;
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

    // Finds the text of clip `index` of the request in the story window: one range per sentence, or null
    function locateClip(index) {
        const markup = voice.texts[index];
        const root = document.querySelector(".gameplay-container");
        if (!root || !markup || !window.mfwNarrationSync) return null;
        const target = plainText(markup);
        if (!target) return null;
        if (places.id !== voice.id || places.root !== root) {
            places.id   = voice.id;
            places.root = root;
            places.ends = [];
            places.first = places.last = null;
            places.complete = false;
        }
        let from = 0;   // after the nearest earlier clip that was found
        for (let i = index - 1; i >= 0 && !from; i--) from = places.ends[i] || 0;

        const { text, map } = windowText(root);
        let at = text.indexOf(target, from);
        if (at < 0) at = text.indexOf(target);
        let length = target.length;
        if (at < 0) {  // the shown text differs a little (a name written differently): match its start
            const head = target.slice(0, 40);
            at = text.indexOf(head, from);
            if (at < 0) return null;
            length = Math.min(target.length, text.length - at);
        }
        places.ends[index] = at + length;
        if (!places.first || at < places.at) { places.first = map[at]; places.at = at; }
        if (!places.last || at + length > places.to) { places.last = map[Math.min(at + length, map.length) - 1]; places.to = at + length; }

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
        return ranges.length ? { index, parts, ranges, times: null, exact: false, shown: -1, led: -1 } : null;
    }

    // The clip that starts to play
    function highlightClip() {
        clearHighlight();
        reading = locateClip(voice.index);
        syncHighlight();
    }

    // Marks the sentence at the clip's current time and moves the page with it (every animation frame)
    function syncHighlight() {
        if (!reading || voice.phase !== "clip" || reading.index !== voice.index) return;
        if (!reading.ranges[0].startContainer.isConnected) {  // the page was rendered again: find the text again
            highlightClip();
            return;
        }

        const duration = voiceElement.duration;
        const timed    = isFinite(duration) && duration > 0;
        const now      = voiceElement.currentTime || 0;
        let shown = 0;
        if (timed) {
            if (!reading.exact) {
                const pauses = clipPauses(voice.clip, duration);   // undefined: the pause list is still loading
                if (!reading.times || pauses !== undefined) reading.times = mfwNarrationSync.starts(reading.parts, duration, pauses || null);
                reading.exact = pauses !== undefined;
            }
            while (shown + 1 < reading.times.length && reading.times[shown + 1] <= now) shown++;
        }
        const range = reading.ranges[shown];
        if (shown !== reading.shown) {
            reading.shown = shown;
            markSentence(range);
            if (scrollMode !== "flow") keepInView(range);
        } else if (rows.length) {
            placeLines(range);   // the layout may have moved (text size, a turned phone)
        }
        if (!timed) return;

        const last = shown + 1 >= reading.ranges.length;
        const end  = last ? duration : reading.times[shown + 1];
        if (scrollMode === "lead" && reading.led !== shown && now >= end - LEAD_SECONDS) {
            // the sentence is almost over: bring the next one (of this clip or the next) into place already
            reading.led = shown;
            const next = !last ? reading.ranges[shown + 1]
                       : voice.index + 1 < voice.list.length ? (locateClip(voice.index + 1) || { ranges: [] }).ranges[0] : null;
            if (next) keepInView(next);
        }
        const start    = reading.times[shown];
        const progress = end > start ? Math.min(1, Math.max(0, (now - start) / (end - start))) : 0;
        if (voice.paused) return;
        if (scrollMode === "flow") followLine(range, progress);
        // from the first word to the last (the sentence is shown a little before it is spoken and stays through its pause)
        const spoken = reading.times.spoken && reading.times.spoken[shown] || [start, end];
        const words  = spoken[1] > spoken[0] ? Math.min(1, Math.max(0, (now - spoken[0]) / (spoken[1] - spoken[0]))) : progress;
        animateMarks(words, now - spoken[0], now - spoken[1]);
    }

    // ---- How the spoken sentence is marked (pause menu, stored per device) -----------------------------------------
    //   off         - no mark (the page still follows the narration)
    //   dim         - the rest of the narrated text fades a little, the sentence keeps its ink (::highlight)
    //   quill       - a thin line of light that draws itself under the words; the rest fades slightly
    //   glow        - soft candlelight on the parchment around the sentence
    //   emberglow   - candle glow with warm ember-brown ink, and embers drifting behind the words being spoken
    //   wisp        - a spark that travels under the words as they are spoken, wandering a little, with a trail (the
    //                 default for now, owner 16 Nov 2026)
    // Order as in the pause menu (owner, 16 Nov 2026).
    // Bars, sparks and lights are drawn over the page (children of <body> in page coordinates, css/app.css
    // .mfw-reading-*), so the page DOM stays as Blazor rendered it. The ember-ink bar only colours what is darker
    // than it is (mix-blend-mode: lighten): the ink, not the parchment. All marks are parchment only (on the title
    // they would show as blocks). The fades use the CSS Custom Highlight API. Sparks, motes and lights are left out
    // when Graphics is Low (Settings; reduced motion). Dropped 16 Nov 2026: Gold backlight, Underline, Ember text (with
    // sparks), Sparks, Wandering wisp (now the motion of the travelling spark), soft travelling spark, Firefly, Candle
    // glow with motes.
    //   bars:   classes of the bars laid over each line (.mfw-reading-<class>), in drawing order
    //   rest:   ::highlight name for the rest of the narrated text
    //   light:  the travelling light (LIGHTS); embers: embers around the words being spoken; quill: the line
    const HIGHLIGHT_REST = "mfw-narration-rest";
    const HIGHLIGHT_SOFT = "mfw-narration-soft";
    const STYLES = {
        off:       {},
        dim:       { rest: HIGHLIGHT_REST },
        quill:     { rest: HIGHLIGHT_SOFT, quill: true },
        glow:      { bars: ["glow"] },
        emberglow: { bars: ["glow", "ember-ink"], embers: true },
        wisp:      { bars: ["haze"], light: "wisp" }
    };
    const DEFAULT_MARK = "wisp";
    const KEY_MARK = "audio.highlight";
    const MARKS    = Object.keys(STYLES);
    let markStyle = DEFAULT_MARK;
    try {
        const stored = localStorage.getItem(KEY_MARK);   // gold and underline were removed: back to the default
        if (MARKS.indexOf(stored) >= 0) markStyle = stored;
    } catch (e) { /* default */ }
    let rows = [];        // the lines of the sentence in page coordinates: [left, top, width, height]
    let rowsKey = "";     // to leave everything alone while nothing moved
    let lines = [];       // the bars on the page
    let quills = [];      // the quill line, one per row
    let light = null;     // the travelling light
    let sparkAt = 0;      // time of the last spark

    function setHighlight(name, ranges) {
        try { if (ranges.length && window.CSS && CSS.highlights && window.Highlight) CSS.highlights.set(name, new Highlight(...ranges)); } catch (e) { /* ignore */ }
    }

    function clearMarks() {
        try {
            if (window.CSS && CSS.highlights) {
                CSS.highlights.delete(HIGHLIGHT_REST);
                CSS.highlights.delete(HIGHLIGHT_SOFT);
            }
        } catch (e) { /* ignore */ }
        for (const line of lines) line.remove();
        releaseLight();
        lines = [];
        quills = [];
        rows = [];
        rowsKey = "";
    }

    function markSentence(range) {
        clearMarks();
        const box   = range.startContainer.parentElement && range.startContainer.parentElement.closest(".parchment-box");
        const style = STYLES[markStyle];
        if (style.rest) setHighlight(style.rest, restOf(range, box));
        if (box && markStyle !== "off" && markStyle !== "dim") placeLines(range);
    }

    // The narrated text of the sentence's parchment before and after the sentence
    function restOf(range, box) {
        if (!box) return [];
        if (!places.complete) {
            for (let i = 0; i < voice.texts.length; i++) locateClip(i);
            places.complete = true;
        }
        const result = [];
        if (!places.first || !places.last) return result;
        try {
            const before = document.createRange(), after = document.createRange();
            if (box.contains(places.first[0])) before.setStart(places.first[0], places.first[1]); else before.setStart(box, 0);
            before.setEnd(range.startContainer, range.startOffset);
            after.setStart(range.endContainer, range.endOffset);
            if (box.contains(places.last[0])) after.setEnd(places.last[0], places.last[1] + 1); else after.setEnd(box, box.childNodes.length);
            if (!before.collapsed) result.push(before);
            if (!after.collapsed) result.push(after);
        } catch (e) { /* ignore */ }
        return result;
    }

    // Where the lines of the sentence are, and the bars of the style over them
    function placeLines(range) {
        const found = [];
        for (const rect of range.getClientRects()) {
            if (rect.width < 1 || rect.height < 1) continue;
            const middle = (rect.top + rect.bottom) / 2;
            const row = found.find(r => Math.abs((r.top + r.bottom) / 2 - middle) < Math.min(r.bottom - r.top, rect.height) / 2);
            if (row) {
                row.left   = Math.min(row.left, rect.left);
                row.right  = Math.max(row.right, rect.right);
                row.top    = Math.min(row.top, rect.top);
                row.bottom = Math.max(row.bottom, rect.bottom);
            } else {
                found.push({ left: rect.left, right: rect.right, top: rect.top, bottom: rect.bottom });
            }
        }
        const x = window.scrollX, y = window.scrollY;
        const boxes = found.map(r => [Math.round(r.left + x) - 2, Math.round(r.top + y), Math.round(r.right - r.left) + 4, Math.round(r.bottom - r.top)]);
        const key = markStyle + boxes.join(";");
        if (key === rowsKey) return;
        for (const line of lines) line.remove();
        rowsKey = key;
        rows    = boxes;
        lines   = [];
        quills  = [];
        const style = STYLES[markStyle];
        const add = (className, box, top, height) => {
            const bar = document.createElement("div");
            bar.className    = "mfw-reading-line " + className;
            bar.style.left   = box[0] + "px";
            bar.style.top    = top + "px";
            bar.style.width  = box[2] + "px";
            bar.style.height = height + "px";
            document.body.appendChild(bar);
            lines.push(bar);
            return bar;
        };
        for (const name of style.bars || []) for (const box of boxes) add("mfw-reading-" + name, box, box[1], box[3]);
        if (style.quill) for (const box of boxes) quills.push(add("mfw-reading-quill", box, box[1] + box[3] - 1, 2));
    }

    // The place at a fraction (0-1) of the length of all lines together -> [x, row index]
    function atLength(fraction) {
        let along = fraction * rows.reduce((sum, r) => sum + r[2], 0);
        for (let i = 0; i < rows.length; i++) {
            if (along <= rows[i][2]) return [rows[i][0] + along, i];
            along -= rows[i][2];
        }
        const last = rows.length - 1;
        return [rows[last][0] + rows[last][2], last];
    }

    const ease  = t => t <= 0 ? 0 : t >= 1 ? 1 : t * t * (3 - 2 * t);
    const clamp = t => Math.min(1, Math.max(0, t));

    // The quill line, embers and the travelling light (every animation frame while a sentence with rows is spoken). words: 0-1 from the first word of the sentence to its last; since / after: seconds since the first word
    // started and since the last one ended (negative before)
    function animateMarks(words, since, after) {
        if (!rows.length) return;
        const style = STYLES[markStyle];
        if (style.quill && quills.length === rows.length) {
            const [x, at] = atLength(words);
            quills.forEach((bar, i) => {
                const width = i < at ? rows[i][2] : i > at ? 0 : Math.max(0, x - rows[i][0]);
                bar.style.width = width.toFixed(1) + "px";
            });
        }
        if (document.documentElement.dataset.graphics === "low") return;
        const now = performance.now();
        // embers: born around the words being spoken (by the time passed in the sentence), while it is spoken
        if (style.embers && since > -0.2 && after < 0.3 && now - sparkAt > 45) {
            sparkAt = now;
            const spread = (Math.random() + Math.random() + Math.random() - 1.5) * 50;   // mostly close to the words
            const total  = rows.reduce((sum, r) => sum + r[2], 0) || 1;
            const [x, at] = atLength(clamp(words + spread / total));
            const row = rows[at];
            emitEmber(x, row[1] - 3 + Math.random() * (row[3] + 6));
        }
        if (style.light) moveLight(LIGHTS[style.light], words, since, after, now);
    }

    // Embers: small glowing specks, drawn in overlay so the ink stays on top and they seem to drift behind the words.
    // Each one wanders: its heading turns a little at random every frame, with a slight pull upwards, and it flickers
    // and fades out. They keep moving on their own loop after the sentence or the narration ends.
    const embers = [];
    let emberFrame = 0, emberTime = 0;

    function emitEmber(x, y) {
        if (embers.length >= 70) return;
        const el = document.createElement("div");
        el.className = "mfw-reading-ember";
        const size = 4 + Math.random() * 6;
        el.style.width = el.style.height = size.toFixed(1) + "px";
        el.style.opacity = "0";
        document.body.appendChild(el);
        embers.push({ el, x, y, heading: Math.random() * Math.PI * 2, speed: 10 + Math.random() * 22, turn: (Math.random() - 0.5) * 2,
                      age: 0, life: 1.4 + Math.random() * 1.8, seed: Math.random() * 10 });
        if (!emberFrame) {
            emberTime  = performance.now();
            emberFrame = requestAnimationFrame(stepEmbers);
        }
    }

    function stepEmbers(time) {
        const dt = Math.max(0, Math.min(0.05, (time - emberTime) / 1000));
        emberTime = time;
        for (let i = embers.length - 1; i >= 0; i--) {
            const e = embers[i];
            e.age += dt;
            if (e.age >= e.life) {
                e.el.remove();
                embers.splice(i, 1);
                continue;
            }
            e.turn     = Math.max(-2.5, Math.min(2.5, e.turn + (Math.random() - 0.5) * 9 * dt));   // the turn itself drifts
            e.heading += e.turn * dt;
            e.x += Math.cos(e.heading) * e.speed * dt;
            e.y += Math.sin(e.heading) * e.speed * dt - 7 * dt;   // warm air: a slight pull upwards
            const t = e.age / e.life;
            const flicker = 0.7 + 0.3 * Math.sin(e.age * 14 + e.seed) * Math.sin(e.age * 5.3 + e.seed * 2);
            const opacity = Math.min(1, e.age / 0.2) * (t > 0.55 ? (1 - t) / 0.45 : 1) * flicker;
            e.el.style.transform = "translate(" + e.x.toFixed(1) + "px, " + e.y.toFixed(1) + "px) scale(" + (1 - t * 0.5).toFixed(2) + ")";
            e.el.style.opacity   = opacity.toFixed(3);
        }
        emberFrame = embers.length ? requestAnimationFrame(stepEmbers) : 0;
    }

    // Travelling lights: they come in from just before the first word, a moment ahead of it, and trail off past the
    // last one (and past the end of a line where the sentence goes on below), instead of jumping.
    //   below: pixels under the line; trail: [milliseconds between sparks, spark class, size, life]; wander: drifts
    //   off its route like a will-o'-the-wisp
    const LIGHTS = {
        wisp: { className: "mfw-reading-wisp", below: 5, wander: true, trail: [35, "", 6, 1500] }
    };
    // seconds: the fade in starts LIGHT_EARLY before the first word and takes LIGHT_IN, the trail off takes LIGHT_OUT
    const LIGHT_EARLY = 0.35, LIGHT_IN = 0.45, LIGHT_OUT = 0.7;

    function moveLight(kind, words, since, after, now) {
        let [x, at] = atLength(words);
        const row = rows[at];
        let y = row[1] + row[3] + kind.below;
        const fadeIn  = ease(clamp((since + LIGHT_EARLY) / LIGHT_IN));
        const fadeOut = ease(clamp(after / LIGHT_OUT));
        let opacity = fadeIn * (1 - fadeOut);
        x += -(1 - fadeIn) * 28 + fadeOut * 34;
        y -= fadeOut * 5;
        // where the sentence goes on on the next line: dim towards the end of this one and up again on the next
        if (at < rows.length - 1) opacity *= clamp((row[0] + row[2] - x) / 36 + 0.1);
        if (at > 0) opacity *= clamp((x - row[0]) / 36 + 0.1);
        if (kind.wander) {
            x += Math.sin(now / 610) * 5 + Math.sin(now / 237 + 1.3) * 2;
            y += Math.sin(now / 430 + 0.4) * 3.5 + Math.sin(now / 1130) * 2;
        }
        if (!light) {
            light = document.createElement("div");
            light.className = kind.className;
            light.style.opacity = "0";
            document.body.appendChild(light);
        }
        light.style.transform = "translate(" + x.toFixed(1) + "px, " + y.toFixed(1) + "px)";
        light.style.opacity   = opacity.toFixed(3);
        if (kind.trail && opacity > 0.2 && now - sparkAt > kind.trail[0]) {   // sparks that stay where it was and fade
            sparkAt = now;
            const size = kind.trail[2];
            spark(x + (Math.random() - 0.5) * 2, y + (Math.random() - 0.5) * 3, (Math.random() - 0.5) * 5, 1 + Math.random() * 5,
                  (size + Math.random() * size * 0.8) * (0.5 + opacity / 2), kind.trail[3] * (0.8 + Math.random() * 0.4), kind.trail[1]);
        }
    }

    // The light of the sentence before fades where it is (it has usually trailed off already)
    function releaseLight() {
        if (!light) return;
        const old = light;
        light = null;
        old.style.transition = "opacity 0.45s ease-out";
        old.style.opacity = "0";
        setTimeout(() => old.remove(), 500);
    }

    // One spark at a place on the page: it drifts by (dx, dy) and fades within `life` milliseconds
    function spark(x, y, dx, dy, size, life, kind) {
        const dot = document.createElement("div");
        dot.className = "mfw-reading-spark" + (kind ? " " + kind : "");
        dot.style.left   = x.toFixed(1) + "px";
        dot.style.top    = y.toFixed(1) + "px";
        dot.style.width  = dot.style.height = size.toFixed(1) + "px";
        dot.style.animationDuration = life + "ms";
        dot.style.setProperty("--dx", dx.toFixed(1) + "px");
        dot.style.setProperty("--dy", dy.toFixed(1) + "px");
        document.body.appendChild(dot);
        setTimeout(() => dot.remove(), life + 50);
    }

    // Firefox on Android: a scroll the page sets itself, frame by frame, left the frame and the header buttons one
    // toolbar height too low, with the page background (a black bar) above them until the next tap. There the browser
    // runs the scroll (behavior: "smooth"), which takes the same path as a finger scroll.
    // For a bug report, ?mfwglide=browser or ?mfwglide=page in the address picks the way for this tab.
    let BROWSER_GLIDE = /Android/.test(navigator.userAgent) && /Firefox\//.test(navigator.userAgent);
    try {
        const asked = /[?&]mfwglide=(browser|page)/.exec(location.search);
        if (asked) sessionStorage.setItem("mfwglide", asked[1]);
        const forced = sessionStorage.getItem("mfwglide");
        if (forced) BROWSER_GLIDE = forced === "browser";
    } catch (e) { /* storage blocked */ }
    let flowAt = 0;   // time of the last flow step where the browser runs the scroll

    // Where a scroller is and how far it can go: scrolls are kept inside that range, and left out when they would
    // not move the page.
    // For the page itself the room is measured against the large viewport (100lvh, the view with the browser's
    // toolbar hidden). Firefox on Android keeps clientHeight at the small viewport: a scroll to that end while the
    // toolbar hides put the page a toolbar height past its real end (visualViewport.offsetTop -64), with the frame
    // and header buttons that much too low and a black bar above them. So the app never scrolls into the last
    // toolbar height of the page itself; the reader still can.
    let largeProbe = null;
    function largeViewport() {
        if (!largeProbe) {
            largeProbe = document.createElement("div");
            largeProbe.style.cssText = "position:fixed;left:0;top:0;width:0;height:100vh;height:100lvh;visibility:hidden;pointer-events:none";
            document.body.appendChild(largeProbe);
        }
        return largeProbe.getBoundingClientRect().height;
    }
    // Should the page still end up there (seen on Firefox for Android: scrollY 375 while the visual viewport stayed at
    // 311, offsetTop -63.9), the page is scrolled back to where the reader sees it, which puts the frame back in place.
    // A negative offsetTop never happens otherwise (pinch zoom gives a positive one).
    if (window.visualViewport) {
        let realigning = false;
        const realign = () => {
            const vv = window.visualViewport;
            if (realigning || vv.offsetTop > -1 || vv.scale > 1.01) return;
            realigning = true;
            requestAnimationFrame(() => {
                realigning = false;
                if (vv.offsetTop <= -1) window.scrollTo({ top: Math.max(0, Math.round(vv.pageTop)), behavior: "instant" });
            });
        };
        window.visualViewport.addEventListener("scroll", realign);
        window.visualViewport.addEventListener("resize", realign);
        window.addEventListener("scroll", realign, { passive: true });
    }
    function scrollRoom(scroller) {
        if (scroller !== window) return { top: scroller.scrollTop, max: Math.max(0, scroller.scrollHeight - scroller.clientHeight) };
        const element = document.scrollingElement || document.documentElement;
        const view    = Math.max(element.clientHeight, window.innerHeight, largeViewport());
        return { top: element.scrollTop, max: Math.max(0, element.scrollHeight - view) };
    }

    // ---- How the page follows the narration (pause menu, stored per device) ---------------------------------------
    //   off   - the page does not move by itself
    //   lead  - glides just before a sentence ends when the next one is outside the comfortable part of the view, so
    //           it is in place when it lights up (the default, owner's pick 15 Nov 2026; a sentence that still starts
    //           out of place, e.g. after a seek, glides when it starts)
    //   flow  - moves all the time, so the words being spoken stay at one height (like a teleprompter)
    //   page  - only when the sentence would leave the view, then by nearly a whole screen
    const KEY_SCROLL    = "audio.scroll";
    const SCROLL_MODES  = ["off", "lead", "flow", "page"];
    const LEAD_SECONDS  = 0.8;
    let scrollMode = "lead";
    try {
        const stored = localStorage.getItem(KEY_SCROLL);
        if (SCROLL_MODES.indexOf(stored) >= 0) scrollMode = stored;
    } catch (e) { /* default */ }

    // The part of the window the text is read in: below the header buttons, above the bottom edge
    function readingArea() {
        const top = 90, bottom = window.innerHeight - 60;
        return { top, bottom, height: bottom - top };
    }

    // Glides when the sentence is not where the scroll style wants it (not while the reader scrolls or a glide runs)
    function keepInView(range) {
        if (scrollMode === "off" || Date.now() - readerScrolled < 4000 || performance.now() < glideUntil) return;
        const rect = range.getBoundingClientRect();
        if (!rect || (rect.top === 0 && rect.bottom === 0)) return;
        const { top, bottom, height } = readingArea();
        if (scrollMode === "page") {
            if (rect.top >= top && rect.bottom <= bottom) return;
            glideBy(scrollParent(range.startContainer.parentElement), rect.top - (top + height * 0.04));
            return;
        }
        if (rect.top >= top && rect.bottom <= top + height * 0.7) return;
        if (rect.top >= top && rect.top <= top + height * 0.3) return;   // a sentence too long for that: fine near the top
        glideBy(scrollParent(range.startContainer.parentElement), rect.top - (top + height * 0.2));
    }

    // Flow: the place in the sentence that is being spoken (progress 0-1 through its lines) is pulled towards one
    // height of the view, a little every frame
    function followLine(range, progress) {
        if (Date.now() - readerScrolled < 4000) return;
        const rect = range.getBoundingClientRect();
        if (!rect || (rect.top === 0 && rect.bottom === 0)) return;
        const { top, height } = readingArea();
        const off = rect.top + rect.height * progress - (top + height * 0.35);
        if (Math.abs(off) < 1) return;
        const step = Math.abs(off) > 400 ? off * 0.2 : off * 0.06;   // far away (a seek): catch up faster
        const scroller = scrollParent(range.startContainer.parentElement);
        const room = scrollRoom(scroller);
        const to   = Math.min(room.max, Math.max(0, room.top + step));
        if (Math.abs(to - room.top) < 0.5) return;
        if (BROWSER_GLIDE) {   // small smooth steps instead of a move every frame
            const wanted = Math.min(room.max, Math.max(0, room.top + off));
            if (performance.now() - flowAt < 450 || Math.abs(wanted - room.top) < 6) return;
            flowAt = performance.now();
            scroller.scrollTo({ top: wanted, behavior: "smooth" });
            return;
        }
        scroller.scrollTo({ top: to, behavior: "instant" });
    }

    // Scrolls by delta pixels in a short glide. Animated here: the browser's own smooth scroll jumps when the device
    // asks for reduced motion. The reader's own scroll stops it.
    let glide = 0, glideUntil = 0;
    function glideBy(scroller, delta) {
        cancelAnimationFrame(glide);
        const room = scrollRoom(scroller);
        const from = room.top;
        delta = Math.min(room.max, Math.max(0, from + delta)) - from;   // never past the top or the bottom of the page
        if (Math.abs(delta) < 4) return;
        if (BROWSER_GLIDE) {
            glideUntil = performance.now() + 700;
            scroller.scrollTo({ top: from + delta, behavior: "smooth" });
            return;
        }
        const time    = Math.min(900, Math.max(450, Math.abs(delta) * 2));   // milliseconds
        const started = performance.now(), asked = Date.now();
        glideUntil = started + time;
        const step = now => {
            if (readerScrolled >= asked) { glideUntil = 0; return; }
            const p     = Math.min(1, (now - started) / time);
            const eased = p < 0.5 ? 2 * p * p : 1 - (2 - 2 * p) * (2 - 2 * p) / 2;
            scroller.scrollTo({ top: from + delta * eased, behavior: "instant" });
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
        // How the page follows the Oliver narration: "lead" | "flow" | "page"
        getScroll: () => scrollMode,
        setScroll: mode => {
            if (SCROLL_MODES.indexOf(mode) < 0) return;
            scrollMode = mode;
            try {
                localStorage.setItem(KEY_SCROLL, mode);
            } catch (e) {
                // Storage unavailable: keep the setting for this session only
            }
        },
        // How the spoken sentence is marked: one of MARKS ("glow", "wisp", ..., "off")
        getHighlight: () => markStyle,
        setHighlight: mode => {
            if (MARKS.indexOf(mode) < 0) return;
            markStyle = mode;
            if (reading) reading.shown = -1;   // drawn again in the new style on the next frame
            try {
                localStorage.setItem(KEY_MARK, mode);
            } catch (e) {
                // Storage unavailable: keep the setting for this session only
            }
        },
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
