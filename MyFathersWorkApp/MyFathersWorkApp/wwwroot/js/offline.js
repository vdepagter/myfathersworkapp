// Installable offline app (PWA), page side: registers the service worker, tells the app when a new version is ready,
// offers "Install as an app", and runs the narration downloads in Settings (js/audio-cache.js). Blazor listens through
// Shared/OfflineService.cs: mfwOffline.subscribe(dotnetRef) -> OnOfflineState(json) on every change.
// The Windows download serves the files itself (js/edition.js offline: false): no worker there.
(function () {
    "use strict";

    const edition   = window.mfwEdition || {};
    const supported = "serviceWorker" in navigator && "caches" in window && window.isSecureContext && edition.offline !== false;
    const audio     = window.mfwAudioCache;

    const state = {
        supported,
        available:     false,  // the published worker is active (the app shell is cached)
        shellProgress: null,   // 0-100 while the app is being cached for the first time / an update
        update:        false,  // a new version is waiting
        installable:   false,  // the browser offers installing (beforeinstallprompt)
        iosHint:       /iPad|iPhone|iPod/.test(navigator.userAgent) || (navigator.platform === "MacIntel" && navigator.maxTouchPoints > 1),
        // Firefox for Android installs from its own menu and never offers it to the page (Firefox on a computer cannot)
        firefoxHint:   /Android/.test(navigator.userAgent) && /Firefox\//.test(navigator.userAgent),
        blocked:       false,  // the browser refused the worker (private window, site data cleared on close)
        askNarration:  false,  // just installed / first start as an app: offer the narration downloads
        asked:         false,  // the player chose narration packs before (Offline play applied or closed)
        standalone:    matchMedia("(display-mode: standalone)").matches || navigator.standalone === true,
        online:        navigator.onLine,
        packs:         [],     // js/audio-cache.js status() + downloading
        indexMissing:  false,  // no packs list (development build, or never online)
        usage:         0,
        quota:         0,
        error:         ""
    };

    const queue      = [];    // pack ids to download one after the other (the narration choice, Download all)
    let listener     = null;
    let registration = null;
    let index        = null;
    let installEvent = null;
    let reloading    = false;
    const progress   = {};    // pack id -> { storedBytes, bytes } while downloading
    const cancelled  = new Set();
    let justInstalled = false;
    const ASKED_KEY  = "mfwOfflineAsked";

    function askedBefore() {
        try { return localStorage.getItem(ASKED_KEY) === "1"; } catch (e) { return true; }
    }

    function emit() {
        state.asked = askedBefore();
        for (const row of state.packs) {
            row.downloading = audio.isDownloading(row.id);
            row.queued      = !row.downloading && queue.includes(row.id);
        }
        if (!listener) return;
        try { listener.invokeMethodAsync("OnOfflineState", JSON.stringify(state)); } catch (e) { /* app gone */ }
    }

    // ---- service worker ----------------------------------------------------------------------------------------------

    function ask(worker) {
        return new Promise(resolve => {
            const channel = new MessageChannel();
            const timer   = setTimeout(() => resolve(null), 3000);
            channel.port1.onmessage = e => { clearTimeout(timer); resolve(e.data); };
            worker.postMessage({ type: "mfw-info" }, [channel.port2]);
        });
    }

    function watchInstalling(worker) {
        if (!worker) return;
        worker.addEventListener("statechange", () => {
            if (worker.state === "installed" && navigator.serviceWorker.controller) { state.update = true; emit(); }
            if (worker.state === "activated" || worker.state === "redundant") { state.shellProgress = null; refresh(); }
        });
    }

    async function register() {
        if (!supported) {
            // A worker left from before (e.g. this folder opened as a website once) must not serve old files
            if ("serviceWorker" in navigator && edition.offline === false)
                navigator.serviceWorker.getRegistrations().then(list => list.forEach(r => r.unregister())).catch(() => { });
            return;
        }
        try {
            registration = await navigator.serviceWorker.register("service-worker.js", { updateViaCache: "none" });
        } catch (e) {
            console.warn("Offline: service worker not registered: " + e.message);
            state.blocked = true;
            emit();
            return;
        }
        if (registration.waiting && navigator.serviceWorker.controller) state.update = true;
        watchInstalling(registration.installing);
        registration.addEventListener("updatefound", () => watchInstalling(registration.installing));

        navigator.serviceWorker.addEventListener("controllerchange", () => {
            if (reloading) location.reload();
            else refresh();
        });

        // Look for a new version when the app comes back to the front, and every 30 minutes
        const check = () => { if (navigator.onLine) registration.update().catch(() => { }); };
        document.addEventListener("visibilitychange", () => { if (!document.hidden) check(); });
        setInterval(check, 30 * 60 * 1000);

        await refresh();
        topUp();
    }

    navigator.serviceWorker && navigator.serviceWorker.addEventListener("message", e => {
        const data = e.data || {};
        if (data.type === "mfw-shell-progress") {
            state.shellProgress = data.total ? Math.floor(100 * data.done / data.total) : null;
            emit();
        } else if (data.type === "mfw-shell-ready") {
            state.shellProgress = null;
            refresh();
        }
    });
    if (navigator.serviceWorker && navigator.serviceWorker.startMessages) navigator.serviceWorker.startMessages();

    // ---- status ------------------------------------------------------------------------------------------------------

    async function refresh() {
        if (!supported) { emit(); return; }
        try {
            const active = registration && registration.active;
            const info   = active ? await ask(active) : null;
            state.available = !!(info && info.offline);
            if (info && info.offline === false) state.supported = false; // development build: no offline worker
            state.online    = navigator.onLine;
            if (state.available) {
                if (!index) index = await audio.loadIndex(navigator.onLine);
                state.indexMissing = !index;
                state.packs = (await audio.status(index)).map(p => {
                    const live = progress[p.id];
                    return Object.assign(p, { storedBytes: live ? live.storedBytes : p.storedBytes });
                });
                // Right after installing, or on the first start as an installed app: ask which narration to keep
                state.askNarration = state.packs.length > 0 && !askedBefore() && (justInstalled || state.standalone) &&
                                     !state.packs.some(p => p.wanted || p.storedFiles > 0);
            }
            if (navigator.storage && navigator.storage.estimate) {
                const estimate = await navigator.storage.estimate();
                state.usage = estimate.usage || 0;
                state.quota = estimate.quota || 0;
            }
        } catch (e) {
            console.warn("Offline: " + e.message);
        }
        emit();
    }

    // After an update (or an interrupted download), fetch what the chosen packs now miss - quietly, while online
    async function topUp() {
        if (!state.available || !navigator.onLine) return;
        try {
            if (!index) index = await audio.loadIndex(true);
            for (const id of await audio.reconcile(index)) await download(id, true);
        } catch (e) { console.warn("Offline audio top-up: " + e.message); }
    }

    // ---- downloads -----------------------------------------------------------------------------------------------------

    async function download(id, quiet) {
        if (!state.available) return;
        state.error = "";
        try {
            if (navigator.onLine) index = await audio.loadIndex(true) || index;
            if (!index) { state.indexMissing = true; emit(); return; }
            // Ask the browser not to clear the downloads when space runs low (granted silently or by a prompt)
            if (navigator.storage && navigator.storage.persist) navigator.storage.persist().catch(() => { });

            const pack = index.packs.find(p => p.id === id);
            if (!quiet && pack && state.quota) {
                const missing = pack.files.reduce((sum, f) => sum + f[2], 0);
                if (state.quota - state.usage < missing * 1.1) { state.error = "space"; emit(); return; }
            }

            let lastEmit = 0;
            const run = audio.download(index, id, p => {
                progress[id] = p;
                const row = state.packs.find(r => r.id === id);
                if (row) { row.storedBytes = p.storedBytes; row.storedFiles = p.storedFiles; }
                const now = Date.now();
                if (now - lastEmit > 300) { lastEmit = now; emit(); }
            }, 4);
            const row = state.packs.find(r => r.id === id);
            if (row) row.wanted = true;
            emit();
            const complete = await run;
            // Cancelled: keep what came in, but no longer top it up by itself (Settings shows Resume)
            if (cancelled.has(id)) await audio.forget(id);
            else if (!complete && !quiet && navigator.onLine) state.error = "failed";
        } catch (e) {
            console.warn("Offline audio: " + e.message);
            if (!quiet) state.error = "failed";
        } finally {
            delete progress[id];
            cancelled.delete(id);
            await refresh();
        }
    }

    // Several packs one after the other (the narration choice, Download all)
    let draining = false;

    async function downloadMany(ids) {
        for (const id of ids) if (!queue.includes(id) && !audio.isDownloading(id)) queue.push(id);
        emit();
        if (draining) return;
        draining = true;
        try {
            while (queue.length) {
                const id = queue[0];
                await download(id, false);
                queue.shift();
                if (state.error === "space") { queue.length = 0; break; }
            }
        } finally {
            draining = false;
            refresh();
        }
    }

    function answerNarration(ids) {
        try { localStorage.setItem(ASKED_KEY, "1"); } catch (e) { /* storage blocked */ }
        state.askNarration = false;
        emit();
        if (ids && ids.length) downloadMany(ids);
    }

    // Offline play > Download: download the ticked packs, remove the unticked ones that are (partly) stored
    async function apply(adds, removes) {
        try { localStorage.setItem(ASKED_KEY, "1"); } catch (e) { /* storage blocked */ }
        state.askNarration = false;
        for (const id of removes || []) {
            const queued = queue.indexOf(id);
            if (queued >= 0 && !audio.isDownloading(id)) queue.splice(queued, 1);
            cancel(id);
            try { await audio.remove(index, id); } catch (e) { console.warn("Offline audio: " + e.message); }
        }
        await refresh();
        if (adds && adds.length) downloadMany(adds);
    }

    async function removeAll() {
        queue.length = 0;
        for (const p of state.packs) if (p.wanted || p.storedFiles > 0 || p.downloading) {
            cancel(p.id);
            try { await audio.remove(index, p.id); } catch (e) { console.warn("Offline audio: " + e.message); }
        }
        await refresh();
    }

    function cancel(id) {
        const queued = queue.indexOf(id);
        if (queued > 0) { queue.splice(queued, 1); emit(); }
        if (audio.isDownloading(id)) cancelled.add(id);
        audio.cancel(id);
    }

    async function remove(id) {
        cancel(id);
        try { await audio.remove(index, id); } catch (e) { console.warn("Offline audio: " + e.message); }
        await refresh();
    }

    // ---- install / update ----------------------------------------------------------------------------------------------

    window.addEventListener("beforeinstallprompt", e => {
        e.preventDefault();          // shown from Settings instead of the browser's mini bar
        installEvent = e;
        state.installable = true;
        emit();
    });
    window.addEventListener("appinstalled", () => {
        installEvent = null;
        state.installable = false;
        justInstalled = true;
        refresh();
    });
    window.addEventListener("online",  () => { state.online = true;  refresh(); topUp(); });
    window.addEventListener("offline", () => { state.online = false; emit(); });

    async function install() {
        if (!installEvent) return;
        installEvent.prompt();
        try { await installEvent.userChoice; } catch (e) { /* dismissed */ }
        installEvent = null;
        state.installable = false;
        emit();
    }

    // Activates the waiting version; the page reloads when it has taken over (the game reopens on its screen)
    async function applyUpdate() {
        const reg = (await navigator.serviceWorker.getRegistration()) || registration;
        let worker = reg && (reg.waiting || reg.installing);
        if (!worker) { location.reload(); return; }
        reloading = true;
        if (worker.state !== "installed") {
            await new Promise(resolve => worker.addEventListener("statechange", function done() {
                if (worker.state === "installed" || worker.state === "redundant") { worker.removeEventListener("statechange", done); resolve(); }
            }));
            worker = reg.waiting;
            if (!worker) { location.reload(); return; }
        }
        worker.postMessage({ type: "mfw-skip-waiting" });
        // Safety net: reload anyway if the switch-over never reports back
        setTimeout(() => location.reload(), 10000);
    }

    window.mfwOffline = {
        subscribe: ref => { listener = ref; emit(); refresh(); },
        unsubscribe: () => { listener = null; },
        refresh,
        download: id => { downloadMany([id]); },
        downloadMany: ids => { downloadMany(ids); },
        answerNarration: ids => { answerNarration(ids); },
        apply: (adds, removes) => { apply(adds, removes); },
        removeAll: () => { removeAll(); },
        cancel,
        remove: id => { remove(id); },
        install: () => { install(); },
        applyUpdate: () => { applyUpdate(); },
        dismissError: () => { state.error = ""; emit(); }
    };

    if (document.readyState === "complete") register();
    else window.addEventListener("load", register);
})();
