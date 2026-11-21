// Android app only (tools/android-stage.py adds it to index.html, after js/capacitor.js): the Back button.
// An open panel closes first; in a game Back opens the pause menu (and closes it again), so a game is never left by
// accident; elsewhere it goes back a page, and on the Main Menu the app goes to the background instead of closing.
(function () {
    "use strict";

    const cap = window.Capacitor;
    if (!cap || !cap.isNativePlatform || !cap.isNativePlatform() || !cap.registerPlugin) return;

    const App = cap.registerPlugin("App");

    function click(selector) {
        const element = document.querySelector(selector);
        if (!element) return false;
        element.click();
        return true;
    }

    App.addListener("backButton", event => {
        // Panels that close when their dark background is clicked (Offline play may sit over Settings / pause menu)
        for (const overlay of [".offline-overlay", ".settings-overlay", ".logbook-overlay", ".mp-popup-overlay"])
            if (click(overlay)) return;
        if (document.querySelector(".pause-menu-overlay")) {
            click(".pause-menu .pause-buttons .mfw-button.green"); // Resume
            return;
        }
        if (click(".pause-button")) return;
        const home = new URL(document.baseURI).pathname;
        if (event && event.canGoBack && location.pathname !== home) history.back();
        else App.minimizeApp();
    });
})();
