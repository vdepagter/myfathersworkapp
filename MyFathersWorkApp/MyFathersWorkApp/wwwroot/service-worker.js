// Development: no offline caching, so every reload shows the latest files. The published app uses
// service-worker.published.js (MyFathersWorkApp.csproj). js/offline.js asks for "mfw-info": offline is false here, so
// Settings hides the offline section.
self.addEventListener("install",  () => self.skipWaiting());
self.addEventListener("activate", event => event.waitUntil(self.clients.claim()));
self.addEventListener("message",  event => {
    if (event.data && event.data.type === "mfw-info" && event.ports[0]) event.ports[0].postMessage({ offline: false });
});
