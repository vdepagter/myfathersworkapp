// Saved games (Shared/SavedGames.cs). The game being played lives in "continue", "continueTime", "undoMeta" and
// "undo/{i}" (Shared/GlobalData.Persistence.cs). Every other game is packed into one key: the JSON
// {continue, continueTime, undoMeta, undo: {i: value}}, gzipped and base64 encoded with the prefix "z:" ("j:" + plain JSON
// where the browser has no CompressionStream). The undo points of one game are nearly identical, so a whole history of a
// few hundred screens packs into a few dozen KB.
window.mfwSaves = (function () {
    "use strict";

    const SINGLE_KEYS = ["continue", "continueTime", "undoMeta"];

    function undoRange() {
        const parts = (localStorage.getItem("undoMeta") || "").split("|").map(Number);
        return parts.length === 2 && Number.isInteger(parts[0]) && Number.isInteger(parts[1]) ? parts : [0, 0];
    }

    function activeData(keepHistory) {
        const data = { undo: {} };
        for (const key of SINGLE_KEYS) data[key] = localStorage.getItem(key);
        if (!keepHistory) {
            data.undoMeta = null;
            return data;
        }

        const [first, end] = undoRange();
        for (let i = first; i < end; i++) {
            const value = localStorage.getItem("undo/" + i);
            if (value !== null) data.undo[i] = value;
        }
        return data;
    }

    function clearActive() {
        const [first, end] = undoRange();
        for (let i = first; i < end; i++) localStorage.removeItem("undo/" + i);
        for (const key of SINGLE_KEYS) localStorage.removeItem(key);
    }

    async function streamBytes(bytes, transform) {
        const stream = new Blob([bytes]).stream().pipeThrough(transform);
        return new Uint8Array(await new Response(stream).arrayBuffer());
    }

    function toBase64(bytes) {
        let text = "";
        for (let i = 0; i < bytes.length; i += 0x8000) text += String.fromCharCode.apply(null, bytes.subarray(i, i + 0x8000));
        return btoa(text);
    }

    function fromBase64(text) {
        const binary = atob(text);
        const bytes  = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
        return bytes;
    }

    async function pack(data) {
        const json = JSON.stringify(data);
        if (typeof CompressionStream === "undefined") return "j:" + json;
        return "z:" + toBase64(await streamBytes(new TextEncoder().encode(json), new CompressionStream("gzip")));
    }

    async function unpack(text) {
        if (!text) return null;
        if (text.startsWith("j:")) return JSON.parse(text.substring(2));
        const bytes = await streamBytes(fromBase64(text.substring(2)), new DecompressionStream("gzip"));
        return JSON.parse(new TextDecoder().decode(bytes));
    }

    return {
        // Packs the game being played into key (with or without its undo history). False when the browser storage is full.
        async archive(key, keepHistory) {
            if (!localStorage.getItem("continue")) return false;
            const packed = await pack(activeData(keepHistory));
            try {
                localStorage.setItem(key, packed);
                return true;
            } catch (e) {
                return false;
            }
        },

        // Makes the game packed in key the game being played (the caller archived the previous one). False when it is missing.
        async restore(key) {
            const data = await unpack(localStorage.getItem(key));
            if (!data || !data.continue) return false;

            clearActive();
            try {
                for (const [i, value] of Object.entries(data.undo || {})) localStorage.setItem("undo/" + i, value);
                if (data.undoMeta) localStorage.setItem("undoMeta", data.undoMeta);
            } catch (e) {
                // Storage full: open the game without its history (load then starts a new one from this screen)
                clearActive();
            }
            if (data.continueTime) localStorage.setItem("continueTime", data.continueTime);
            localStorage.setItem("continue", data.continue);
            return true;
        },

        // Deletes the game being played (Saved Games, Delete on the current game)
        clearCurrent() {
            clearActive();
        },

        // The save code (as in the pause menu) of a packed game
        async saveCode(key) {
            const data = await unpack(localStorage.getItem(key));
            return data ? data.continue : null;
        }
    };
})();
