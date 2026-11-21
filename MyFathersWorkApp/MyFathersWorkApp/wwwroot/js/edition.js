// Which download this is. tools/build-windows.ps1 writes its own copy into the Windows downloads: offline false (the
// launcher serves the files, so no service worker, js/offline.js), and audio false in the one without audio files
// (the app then plays nothing and hides its sound controls: js/audio.js, AudioService.Available).
window.mfwEdition = { audio: true, offline: true };
