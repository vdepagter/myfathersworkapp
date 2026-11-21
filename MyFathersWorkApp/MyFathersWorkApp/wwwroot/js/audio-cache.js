// Narration audio for offline play: opt-in downloads per pack, in a cache of their own ("mfw-audio") that app updates
// never wipe. Shared by the page (js/offline.js, Settings) and the service worker (service-worker.published.js), and
// meant to be reused as is by a Capacitor build (call match() / objectUrl() from the page when there is no worker).
//
// Packs: audio/vo/packs.json, written at deploy by tools/audio-packs.py. One pack per folder of clips under audio/vo
// (oliver-TheCostOfDisease, oliver-FearOfTheUnknown, oliver-ATimeOfWar, male, female), each file with a content hash
// and size. A clip is stored under "<path>?h=<hash>", so:
//   - an unchanged clip keeps its key across app updates and is never downloaded again;
//   - a clip whose content changed no longer matches the list: reconcile() drops it and the next top-up fetches it;
//   - status needs only cache.keys(), no extra bookkeeping.
// Which packs the player chose is stored in the same cache (__mfw/pack/<id>), the last packs list as __mfw/packs.json,
// so the worker sees both and everything works offline.
(function (scope) {
    "use strict";

    const CACHE      = "mfw-audio";
    const INDEX      = "audio/vo/packs.json";
    const META       = "__mfw/";
    const CLIP_DIR   = "audio/vo/";
    const INDEX_KEY  = META + "packs.json";
    const PACK_KEY   = id => META + "pack/" + encodeURIComponent(id);

    // The app's base: the worker's scope, or the page's <base href> (GitHub Pages serves the app from /<repo>/)
    const base = () => (scope.registration && scope.registration.scope) || document.baseURI;
    const abs  = path => new URL(path, base()).href;

    // Path relative to the base, without query ("audio/vo/oliver/X/abc.mp3"), or null for another origin / outside it
    function relPath(url) {
        const u = new URL(url, base());
        const b = new URL(base());
        if (u.origin !== b.origin || !u.pathname.startsWith(b.pathname)) return null;
        return decodeURIComponent(u.pathname.slice(b.pathname.length));
    }

    const isClip = url => { const p = relPath(url); return !!p && p.startsWith(CLIP_DIR) && p.endsWith(".mp3"); };
    const keyOf  = (path, hash) => abs(path) + "?h=" + hash;

    function parseKey(url) {
        const u = new URL(url);
        const p = relPath(u.href);
        return p && p.startsWith(CLIP_DIR) ? { path: p, hash: u.searchParams.get("h") || "" } : null;
    }

    const open = () => caches.open(CACHE);

    // ---- packs list ------------------------------------------------------------------------------------------------

    // Fetches the current list when online (and stores it), else the stored one. null when there is none at all.
    async function loadIndex(online) {
        const cache = await open();
        if (online !== false) {
            try {
                const response = await fetch(abs(INDEX), { cache: "no-cache" });
                if (response.ok) {
                    const index = await response.clone().json();
                    await cache.put(abs(INDEX_KEY), response);
                    return index;
                }
            } catch (e) { /* offline: use the stored list */ }
        }
        const stored = await cache.match(abs(INDEX_KEY));
        return stored ? stored.json() : null;
    }

    // path -> hash of every stored clip
    async function storedClips(cache) {
        const map = new Map();
        for (const request of await cache.keys()) {
            const k = parseKey(request.url);
            if (k) map.set(k.path, k.hash);
        }
        return map;
    }

    async function wantedPacks(cache) {
        const ids = new Set();
        const prefix = abs(META + "pack/");
        for (const request of await cache.keys())
            if (request.url.startsWith(prefix)) ids.add(decodeURIComponent(request.url.slice(prefix.length)));
        return ids;
    }

    // Per pack: files / bytes in total and stored, and whether the player downloaded it
    async function status(index) {
        if (!index) return [];
        const cache  = await open();
        const stored = await storedClips(cache);
        const wanted = await wantedPacks(cache);
        return index.packs.map(pack => {
            let files = 0, bytes = 0, storedFiles = 0, storedBytes = 0;
            for (const [path, hash, size] of pack.files) {
                files++; bytes += size;
                if (stored.get(path) === hash) { storedFiles++; storedBytes += size; }
            }
            return { id: pack.id, voice: pack.voice, scenario: pack.scenario || "", files, bytes, storedFiles, storedBytes,
                     wanted: wanted.has(pack.id) };
        });
    }

    // Drops clips that are no longer in the list or whose content changed (app update). Returns the ids of downloaded
    // packs that now miss files (to top up while online).
    async function reconcile(index) {
        if (!index) return [];
        const cache   = await open();
        const current = new Map();
        for (const pack of index.packs) for (const [path, hash] of pack.files) current.set(path, hash);
        for (const request of await cache.keys()) {
            const k = parseKey(request.url);
            if (k && current.get(k.path) !== k.hash) await cache.delete(request);
        }
        return (await status(index)).filter(p => p.wanted && p.storedFiles < p.files).map(p => p.id);
    }

    // ---- download / remove -----------------------------------------------------------------------------------------

    const running = new Map(); // pack id -> AbortController

    // Downloads the pack's missing files. onProgress({ storedBytes, bytes, storedFiles, files }). Resolves to true when
    // complete, false when cancelled or some files failed (the pack stays chosen: a later call resumes).
    async function download(index, id, onProgress, concurrency) {
        const pack = index && index.packs.find(p => p.id === id);
        if (!pack) throw new Error("Unknown audio pack " + id);
        if (running.has(id)) return false;

        const controller = new AbortController();
        running.set(id, controller);
        try {
            const cache  = await open();
            await cache.put(abs(PACK_KEY(id)), new Response(String(Date.now())));
            const stored = await storedClips(cache);

            let storedFiles = 0, storedBytes = 0, bytes = 0, failed = 0;
            const todo = [];
            for (const file of pack.files) {
                bytes += file[2];
                if (stored.get(file[0]) === file[1]) { storedFiles++; storedBytes += file[2]; }
                else todo.push(file);
            }
            const report = () => onProgress && onProgress({ storedBytes, bytes, storedFiles, files: pack.files.length });
            report();

            async function fetchOne([path, hash, size]) {
                for (let attempt = 0; attempt < 3; attempt++) {
                    if (controller.signal.aborted) return;
                    try {
                        const response = await fetch(abs(path), { signal: controller.signal, cache: "no-cache" });
                        if (!response.ok) throw new Error(response.status + " " + path);
                        // An older copy of this path (other hash) goes first
                        for (const old of await cache.keys(abs(path), { ignoreSearch: true })) await cache.delete(old);
                        await cache.put(keyOf(path, hash), response);
                        storedFiles++; storedBytes += size;
                        report();
                        return;
                    } catch (e) {
                        if (controller.signal.aborted) return;
                        if (attempt === 2) { failed++; console.warn("Offline audio: " + e.message); }
                    }
                }
            }

            let next = 0;
            const workers = Array.from({ length: Math.max(1, concurrency || 4) }, async () => {
                while (next < todo.length && !controller.signal.aborted) await fetchOne(todo[next++]);
            });
            await Promise.all(workers);
            return !controller.signal.aborted && failed === 0;
        } finally {
            running.delete(id);
        }
    }

    function cancel(id) {
        const controller = running.get(id);
        if (controller) controller.abort();
    }

    const isDownloading = id => running.has(id);

    // Forgets the choice but keeps the clips already stored (a cancelled download: no automatic top-up)
    async function forget(id) {
        await (await open()).delete(abs(PACK_KEY(id)));
    }

    // Forgets the choice and deletes the pack's clips
    async function remove(index, id) {
        cancel(id);
        const cache = await open();
        await cache.delete(abs(PACK_KEY(id)));
        const pack = index && index.packs.find(p => p.id === id);
        const paths = pack ? new Set(pack.files.map(f => f[0])) : null;
        const prefix = pack ? null : CLIP_DIR + id.replace(/-/g, "/") + "/";
        for (const request of await cache.keys()) {
            const k = parseKey(request.url);
            if (k && (paths ? paths.has(k.path) : k.path.startsWith(prefix))) await cache.delete(request);
        }
    }

    // ---- serving ---------------------------------------------------------------------------------------------------

    // A cached full response cut to the request's Range header (audio elements ask for ranges; Safari needs 206)
    async function sliceForRange(request, response) {
        const range = request.headers.get("range");
        if (!range || response.status !== 200) return response;
        const m = /^bytes=(\d*)-(\d*)$/.exec(range.trim());
        const blob = await response.blob();
        const size = blob.size;
        let start, end;
        if (m && m[1] !== "") { start = +m[1]; end = m[2] === "" ? size - 1 : Math.min(+m[2], size - 1); }
        else if (m && m[2] !== "") { start = Math.max(0, size - +m[2]); end = size - 1; }
        if (!m || start >= size || start > end)
            return new Response(null, { status: 416, headers: { "Content-Range": "bytes */" + size } });
        return new Response(blob.slice(start, end + 1), {
            status: 206,
            statusText: "Partial Content",
            headers: {
                "Content-Type":   response.headers.get("Content-Type") || "audio/mpeg",
                "Content-Range":  "bytes " + start + "-" + end + "/" + size,
                "Content-Length": String(end - start + 1),
                "Accept-Ranges":  "bytes"
            }
        });
    }

    // The stored clip for a url (the app's own ?v= is ignored), cut to its Range; undefined when not downloaded
    async function match(request) {
        if (typeof request === "string") request = new Request(abs(request));
        if (!isClip(request.url)) return undefined;
        const cache = await open();
        const response = await cache.match(abs(relPath(request.url)), { ignoreSearch: true });
        return response ? sliceForRange(request, response) : undefined;
    }

    // For the worker: stored clip, else the network as usual
    async function respond(request) {
        try {
            const cached = await match(request);
            if (cached) return cached;
        } catch (e) { /* fall through to the network */ }
        return fetch(request);
    }

    // For a page without a service worker (Capacitor): a blob: url of the stored clip, or the url itself
    async function objectUrl(url) {
        const response = await match(url);
        return response && response.ok ? URL.createObjectURL(await response.blob()) : url;
    }

    scope.mfwAudioCache = { CACHE, isClip, loadIndex, status, reconcile, download, cancel, isDownloading, forget, remove,
                            match, respond, objectUrl, sliceForRange };
})(self);
