// Which download this is. tools/build-windows.ps1 writes { audio: false } into the Windows download without audio
// files; the app then plays nothing and hides its sound controls (js/audio.js, AudioService.Available).
window.mfwEdition = { audio: true };
