// Offline service worker of the published app (GitHub Pages). Based on the Blazor template, with:
//   - the app shell (everything but the narration clips) cached per version; on an update unchanged files are copied
//     from the previous version's cache by hash instead of downloaded again;
//   - narration clips served from their own cache (js/audio-cache.js: opt-in downloads that survive updates);
//   - cached audio cut to Range requests (audio elements ask for byte ranges);
//   - a new version waits until the page asks for it (js/offline.js shows "a new version is ready").
// Paths are relative to the worker, so the GitHub Pages base path (/<repo>/) needs nothing special.

self.importScripts("./service-worker-assets.js");
self.importScripts("./js/audio-cache.js");

const cacheNamePrefix = "offline-cache-";
const cacheName       = `${cacheNamePrefix}${self.assetsManifest.version}`;
const hashesKey       = "__mfw/asset-hashes.json"; // url -> hash of what this cache holds

// Everything is cached except the narration clips (opt-in, js/audio-cache.js), source maps, unused Bootstrap files and
// precompressed copies. Music and sound effects (~10 MB) are part of the shell.
const offlineAssetsExclude = [
    /^service-worker\.js$/,
    /^audio\/vo\/.*\.mp3$/,
    /\.map$/,
    /\.(br|gz)$/,
    /^lib\/bootstrap\/(?!dist\/css\/bootstrap\.min\.css$)/
];

const assetUrls   = new Set(self.assetsManifest.assets.map(asset => new URL(asset.url, self.location).href));
const shellAssets = () => self.assetsManifest.assets.filter(asset => !offlineAssetsExclude.some(p => p.test(asset.url)));

self.addEventListener("install",  event => event.waitUntil(onInstall()));
self.addEventListener("activate", event => event.waitUntil(onActivate()));
self.addEventListener("fetch",    onFetch);
self.addEventListener("message",  onMessage);

async function broadcast(message) {
    for (const client of await self.clients.matchAll({ includeUncontrolled: true })) client.postMessage(message);
}

async function onInstall() {
    const assets = shellAssets();
    const cache  = await caches.open(cacheName);

    // What earlier versions hold, by hash
    const previous = [];
    for (const key of await caches.keys()) {
        if (!key.startsWith(cacheNamePrefix) || key === cacheName) continue;
        const old = await caches.open(key);
        const map = await old.match(hashesKey);
        if (map) previous.push({ cache: old, hashes: await map.json() });
    }

    let done = 0;
    const total = assets.length;
    const hashes = {};
    let lastReport = 0;
    const report = force => {
        const now = Date.now();
        if (force || now - lastReport > 250) { lastReport = now; broadcast({ type: "mfw-shell-progress", done, total }); }
    };

    async function cacheOne(asset) {
        const url = new URL(asset.url, self.location).href;
        for (const p of previous) {
            if (p.hashes[asset.url] !== asset.hash) continue;
            const response = await p.cache.match(url);
            if (response) { await cache.put(url, response); return; }
        }
        // Revalidate against the browser's HTTP cache; Firefox can answer an audio file from a partial (206) cache
        // entry left by an audio element, which the Cache API refuses: then fetch it whole from the network
        let response = await fetch(new Request(url, { integrity: asset.hash, cache: "no-cache" }));
        if (response.status === 206) response = await fetch(new Request(url, { integrity: asset.hash, cache: "reload" }));
        if (response.status !== 200) throw new Error(`${response.status} ${asset.url}`);
        await cache.put(url, response);
    }

    let next = 0;
    await Promise.all(Array.from({ length: 6 }, async () => {
        while (next < assets.length) {
            const asset = assets[next++];
            await cacheOne(asset);
            hashes[asset.url] = asset.hash;
            done++;
            report(false);
        }
    }));
    await cache.put(hashesKey, new Response(JSON.stringify(hashes), { headers: { "Content-Type": "application/json" } }));
    report(true);
}

async function onActivate() {
    // Old app shells go; the audio cache (mfwAudioCache.CACHE) stays
    for (const key of await caches.keys())
        if (key.startsWith(cacheNamePrefix) && key !== cacheName) await caches.delete(key);

    // Drop narration clips that changed in this version (the page tops up downloaded packs, js/offline.js)
    try { await self.mfwAudioCache.reconcile(await self.mfwAudioCache.loadIndex(true)); }
    catch (e) { console.warn("Offline audio: " + e.message); }

    // The first install takes over the open page at once, so it works offline without a reload
    await self.clients.claim();
    broadcast({ type: "mfw-shell-ready", version: self.assetsManifest.version });
}

function onFetch(event) {
    const request = event.request;
    if (request.method !== "GET") return;
    const url = new URL(request.url);
    if (url.origin !== self.location.origin) return;

    if (self.mfwAudioCache.isClip(request.url)) {
        event.respondWith(self.mfwAudioCache.respond(request));
        return;
    }
    event.respondWith(fromShell(request));
}

async function fromShell(request) {
    // Deep links (/Gameplay ...) get index.html, as the 404.html copy does online
    const navigate = request.mode === "navigate" && !assetUrls.has(request.url.split(/[?#]/)[0]);
    const cache    = await caches.open(cacheName);
    const cached   = await cache.match(navigate ? "index.html" : request);
    if (cached) return /\.mp3$/.test(new URL(request.url).pathname) ? self.mfwAudioCache.sliceForRange(request, cached) : cached;
    return fetch(request);
}

function onMessage(event) {
    const data = event.data || {};
    if (data.type === "mfw-skip-waiting") self.skipWaiting();
    else if (data.type === "mfw-info" && event.ports[0])
        event.ports[0].postMessage({ offline: true, version: self.assetsManifest.version });
}
