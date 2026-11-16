// Which sentence of an Oliver clip is being spoken (used by js/audio.js to highlight it and keep it in view).
// A clip can hold a whole page of text. Its on-screen text is split into sentences, and the time each sentence starts
// is worked out from
//   - the pauses of the clip (wwwroot/audio/vo/oliver/<Scenario>.pauses.json, made by tools/narration/pauses.py):
//     every sentence end is matched with a pause near the moment the text length predicts (a sentence end without a
//     usable pause is placed between its neighbours by text length), or
//   - the text length alone when the clip has no pause list (or was rendered again since: its length differs).
// No DOM and no state here, so the functions can be tried from the browser console (window.mfwNarrationSync).
window.mfwNarrationSync = (function () {
    "use strict";

    // Weights in letters: a comma-like break, the pause after a sentence, the pause after a paragraph
    const COMMA = 3, SENTENCE = 9, PARAGRAPH = 13;
    const SWITCH_BEFORE = 0.15;     // the highlight moves this long before the next sentence starts (seconds)
    const SKIP = 3, REWARD = 2.5, MAX_SKIP = 4, LONG_PAUSE = 0.8, MIN_PAUSE = 0.22, EDGE = 0.05;

    const END = /[.!?]["'”’)]*(?=\s|$)/g;
    const ABBREVIATION = /(?:^|[^A-Za-z])(?:Dr|Mr|Mrs|St|Jr)$/;

    // markup: the clip's text as in the CSV (HTML); plain: markup -> text as shown (tags dropped, white space collapsed).
    // Returns [{ text, paragraph }] - the sentences in order (paragraph: last one before a line break or the end).
    // Joined by single spaces they are plain(markup).
    function sentences(markup, plain) {
        const result = [];
        for (const paragraph of String(markup).split(/(?:<br\s*\/?>\s*)+/i)) {
            const text = plain(paragraph);
            if (!text) continue;
            const first = result.length;
            let last = 0, match;
            END.lastIndex = 0;
            while ((match = END.exec(text))) {
                if (match[0][0] === "." && ABBREVIATION.test(text.slice(0, match.index))) continue;
                const end = match.index + match[0].length;
                const sentence = text.slice(last, end).trim();
                if (sentence) result.push({ text: sentence, paragraph: false });
                last = end;
            }
            const rest = text.slice(last).trim();
            if (rest) {
                // closing quotes or brackets on their own belong to the sentence before
                if (result.length > first && !/[A-Za-z0-9]/.test(rest)) result[result.length - 1].text += " " + rest;
                else result.push({ text: rest, paragraph: false });
            }
            if (result.length > first) result[result.length - 1].paragraph = true;
        }
        return result;
    }

    // [speech, pause after] per sentence, in letters
    function weights(parts) {
        return parts.map(part => {
            const letters = (part.text.match(/[A-Za-z]/g) || []).length + 4 * (part.text.match(/\d/g) || []).length;
            const breaks  = (part.text.match(/[,;:]| - |—|–/g) || []).length;
            return [letters + COMMA * breaks + 2, part.paragraph ? PARAGRAPH : SENTENCE];
        });
    }

    // Second at which each sentence starts to be shown (the first at 0).
    // pauses: [duration, start, end, start, end, ...] of the clip, or null (estimate by text length).
    // The result also has .spoken: [first word, last word] in seconds per sentence (the pause before and after left
    // out where the sentence end has a pause; without pauses the span to the next sentence start).
    function starts(parts, duration, pauses) {
        const n = parts.length;
        const list = [];
        if (pauses) for (let i = 1; i + 1 < pauses.length; i += 2) list.push([pauses[i], pauses[i + 1]]);
        const lead  = list.length && list[0][0] < EDGE ? list[0][1] : 0;
        const tail  = list.length && list[list.length - 1][1] > duration - EDGE ? list[list.length - 1][0] : duration;
        if (n <= 1 || !(duration > 0)) {
            const none = parts.map(() => 0);
            none.spoken = parts.map(() => [lead, duration > 0 ? tail : 0]);
            return none;
        }

        const w = weights(parts);
        const inner = list.filter(p => p[0] >= EDGE && p[1] <= duration - EDGE && p[1] - p[0] >= MIN_PAUSE);

        // Expected middle of the pause after sentence k
        let total = -w[n - 1][1];
        for (const [speech, pause] of w) total += speech + pause;
        const rate = (tail - lead) / total;
        const expected = [];
        for (let k = 0, t = 0; k < n - 1; k++) {
            t += w[k][0];
            expected.push(lead + (t + w[k][1] / 2) * rate);
            t += w[k][1];
        }

        const assigned = match(n, expected, inner, lead, tail);   // per sentence end: index of its pause, or -1

        // Sentence ends with a pause switch just before the pause ends - the others share the speech between the
        // pauses around them by text length
        const result = [0];
        let from = -1;                                             // last sentence end with a pause (-1: clip start)
        for (let k = 0; k <= n - 1; k++) {
            const last = k === n - 1;
            if (!last && assigned[k] < 0) continue;
            const begin = from < 0 ? lead : inner[assigned[from]][1];
            const end   = last ? tail : inner[assigned[k]][0];
            let span = 0;
            for (let i = from + 1; i <= k; i++) span += w[i][0] + (i < k ? w[i][1] : 0);
            for (let i = from + 1, t = 0; i < k; i++) {
                t += w[i][0] + w[i][1];
                result[i + 1] = begin + Math.max(0, end - begin) * t / span;
            }
            if (!last) {
                const pause = inner[assigned[k]];
                result[k + 1] = Math.max((pause[0] + pause[1]) / 2, pause[1] - SWITCH_BEFORE);
            }
            from = k;
        }
        result.spoken = [];
        for (let k = 0; k < n; k++) {
            const begin = k === 0 ? lead : assigned[k - 1] >= 0 ? inner[assigned[k - 1]][1] : result[k];
            const end   = k === n - 1 ? tail : assigned[k] >= 0 ? inner[assigned[k]][0] : result[k + 1];
            result.spoken.push([begin, Math.max(begin, end)]);
        }
        return result;
    }

    // Best pause for every sentence end (dynamic programming over sentence ends x pauses, both in order): the time
    // between two matched pauses should be what the text between them predicts, long pauses are preferred, and a
    // sentence end may stay without a pause at a price.
    function match(n, expected, inner, lead, tail) {
        const m = inner.length;
        const assigned = new Array(n - 1).fill(-1);
        if (!m) return assigned;

        const mid   = inner.map(p => (p[0] + p[1]) / 2);
        const bonus = inner.map(p => REWARD * Math.min(p[1] - p[0], LONG_PAUSE) / LONG_PAUSE);
        const at    = k => k < 0 ? lead : k >= n - 1 ? tail : expected[k];
        const price = (seconds, wanted) => {
            const spread = 0.3 + 0.15 * wanted;
            return ((seconds - wanted) / spread) * ((seconds - wanted) / spread);
        };

        // cost[k][j]: sentence end k on pause j; from[k][j] = [previous sentence end, its pause] (-1, -1: clip start)
        const cost = [], back = [];
        for (let k = 0; k < n - 1; k++) {
            cost.push(new Float64Array(m).fill(Infinity));
            back.push(new Array(m).fill(null));
            for (let j = 0; j < m; j++) {
                let best = Infinity, origin = null;
                for (let k0 = Math.max(-1, k - MAX_SKIP - 1); k0 < k; k0++) {
                    const wanted  = at(k) - at(k0);
                    const skipped = SKIP * (k - k0 - 1);
                    if (k0 < 0) {
                        const c = price(mid[j] - lead, wanted) + skipped;
                        if (mid[j] > lead && c < best) { best = c; origin = [-1, -1]; }
                        continue;
                    }
                    for (let j0 = 0; j0 < j; j0++) {
                        if (cost[k0][j0] === Infinity) continue;
                        const c = cost[k0][j0] + price(mid[j] - mid[j0], wanted) + skipped;
                        if (c < best) { best = c; origin = [k0, j0]; }
                    }
                }
                if (origin) {
                    cost[k][j] = best - bonus[j];
                    back[k][j] = origin;
                }
            }
        }

        // The end of the clip closes the chain
        let best = price(tail - lead, at(n - 1) - at(-1)) + SKIP * (n - 1), origin = [-1, -1];
        if (n - 1 > MAX_SKIP) best = Infinity;
        for (let k0 = Math.max(0, n - 2 - MAX_SKIP); k0 < n - 1; k0++) {
            for (let j0 = 0; j0 < m; j0++) {
                if (cost[k0][j0] === Infinity) continue;
                const c = cost[k0][j0] + price(tail - mid[j0], at(n - 1) - at(k0)) + SKIP * (n - 2 - k0);
                if (c < best) { best = c; origin = [k0, j0]; }
            }
        }
        for (let node = origin; node && node[0] >= 0; node = back[node[0]][node[1]]) assigned[node[0]] = node[1];
        return assigned;
    }

    return { sentences: sentences, starts: starts };
})();
