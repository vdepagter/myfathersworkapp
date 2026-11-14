using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace MyFathersWorkApp;

// Fan-made Oliver narration of all story text (voice setting Oliver; docs/AUDIO_TRANSITIONS_MAP.md 8.4). The clips are
// made by tools/narration/chunks.py: the story text of a tag is cut at "<b>CHOOSE" and split around its placeholders;
// every static chunk is one clip named after the FNV-1a 64 hash of its CSV text, and the text a placeholder was formatted
// with (an either/or variant, an NPC name) is looked up the same way ("value clip"; player names have none and are
// skipped). wwwroot/audio/vo/oliver/<Scenario>.json lists the clips: hash -> script hash ("" = silent chunk), and per
// scenario whether window titles and the leading letter / journal header line are narrated, and per tag where the
// narration stops (choices and rules lines: chunks.auto_cut). Keep the regex, the cuts, the header rule and the hash
// identical to chunks.py.
public static class Narration
{
    private const string Cut = "<b>CHOOSE";

    private static readonly Regex PlaceholderStart = new(@"\{\{\d+=", RegexOptions.Compiled);

    private sealed class Manifest
    {
        public Dictionary<string, string> Clips   { get; set; } = new();
        public List<string>               Private { get; set; } = new();
        public bool                       Titles  { get; set; }
        public bool                       Headers { get; set; } = true;
        public Dictionary<string, string> Cuts    { get; set; } = new();
        public bool                       Sentences { get; set; }        // sentence mode (chunks.sentence_units)
        public List<string>               StandIns  { get; set; } = new(); // previews of typed-in values (chunks.STAND_INS)
    }

    // Where a sentence ends (chunks.BOUNDARY)
    private static readonly Regex Boundary = new(@"(?<!Dr)(?<!Mr)(?<!Mrs)(?<!St)[.!?][""'”’)]*(?:</[bi]>)*(?=\s|<br|$)|<br\s*/?>",
                                                 RegexOptions.Compiled);

    private static readonly Dictionary<ScenarioId, Manifest> Manifests = new();

    private static string Folder(ScenarioId scenario) => scenario switch
    {
        ScenarioId.CostOfDisease => "TheCostOfDisease",
        ScenarioId.FearOfUnknown => "FearOfTheUnknown",
        ScenarioId.TimeOfWar     => "ATimeOfWar",
        _                        => string.Empty
    };

    // Loads the scenario's clip list once (a missing list means: no Oliver narration yet)
    public static async Task LoadAsync(HttpClient http, ScenarioId scenario)
    {
        if (Manifests.ContainsKey(scenario) || Folder(scenario) == string.Empty) return;

        try
        {
            Manifests[scenario] = await http.GetFromJsonAsync<Manifest>($"audio/vo/oliver/{Folder(scenario)}.json") ?? new Manifest();
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Oliver narration list not loaded: {exception.Message}");
            Manifests[scenario] = new Manifest();
        }
    }

    // The clip urls of a window in play order, or null when some speaking chunk of its story text has no clip (not
    // rendered yet, another text language, or text changed by code in a way the split cannot follow).
    public static List<NarrationClip>? Clips(GlobalData globalData, GameplayWindow window)
    {
        if (!Manifests.TryGetValue(globalData.ScenarioId, out Manifest? manifest) || manifest.Clips.Count == 0) return null;

        List<(string Tag, string Text)> stories = window.StoryTexts.Where(story => manifest.Titles || !story.Tag.EndsWith("_Title")).ToList();
        if (stories.Count == 0) return null;

        string        folder = Folder(globalData.ScenarioId);
        List<NarrationClip> urls = new();
        foreach ((string tag, string text) in stories)
        {
            if (!globalData.Scenario_Localization.TryGetValue(tag, out string? source) || !AddTag(manifest, folder, tag, source, text, urls))
            {
                Console.WriteLine($"Oliver narration: no clips for {tag}");
                return null;
            }
        }

        return urls.Count > 0 ? urls : null;
    }

    // Private sections (one player reads alone): narrated, but only played when the player presses Play
    public static bool IsPrivate(GlobalData globalData, GameplayWindow window) =>
        Manifests.TryGetValue(globalData.ScenarioId, out Manifest? manifest) && window.StoryTexts.Any(story => manifest.Private.Contains(story.Tag));

    // Every chunk is matched (the split must follow the whole text), but only the part between the header line (when
    // headers are not narrated) and the cut is played: chunks that end after the header and start before the cut, values
    // whose placeholder lies in between.
    private static bool AddTag(Manifest manifest, string folder, string tag, string source, string rendered, List<NarrationClip> urls)
    {
        source   = Before(source,   Cut);
        rendered = Before(rendered, Cut);

        int from = manifest.Headers || tag.EndsWith("_Title") ? 0 : HeaderEnd(source);
        int to   = source.Length;
        if (manifest.Cuts.TryGetValue(tag, out string? marker))
        {
            int cut = marker == string.Empty ? 0 : source.IndexOf(marker, StringComparison.Ordinal);
            if (cut >= 0) to = cut;
        }

        // The rendered text of every run of placeholders (several when only empty text lies between them), keyed by the
        // index of its first placeholder
        List<(int Index, int Length)>                 spans  = Placeholders(source);
        Dictionary<int, (int Count, string Value, int Rendered)> runs   = new();
        List<(int Start, string Chunk, int Rendered)>            chunks = new();
        int position = 0;
        int first    = -1;
        int last     = 0;
        for (int k = 0; k <= spans.Count; k++)
        {
            int    chunkStart = last;
            string chunk      = k == spans.Count ? source[last..] : source[last..spans[k].Index];
            if (k < spans.Count) last = spans[k].Index + spans[k].Length;

            if (chunk != string.Empty)
            {
                int index = Find(manifest, rendered, chunk, position, first < 0 ? 0 : k - first);
                if (index < 0) return false;
                if (first >= 0) runs[first] = (k - first, rendered[position..index], position);
                chunks.Add((chunkStart, chunk, index));
                position = index + chunk.Length;
                first    = -1;
            }

            if (k < spans.Count && first < 0) first = k;
        }

        if (first >= 0) runs[first] = (spans.Count - first, rendered[position..], position);

        if (manifest.Sentences) return AddSentences(manifest, folder, source, rendered, spans, runs, chunks, from, to, urls);

        foreach ((int start, string chunk, _) in chunks)
        {
            // the value run that ends at this chunk is played before it
            foreach ((int k, (int count, string value, _)) in runs.Where(run => spans[run.Key + run.Value.Count - 1].Index + spans[run.Key + run.Value.Count - 1].Length == start))
                if (spans[k].Index >= from && spans[k].Index < to) AddValue(manifest, folder, count == 1 ? source.Substring(spans[k].Index, spans[k].Length) : null, value, urls);

            if (!manifest.Clips.TryGetValue(Hash(chunk), out string? script)) return false;
            if (script != string.Empty && start + chunk.Length > from && start < to) urls.Add(new(Url(folder, chunk, script), chunk));
        }

        foreach ((int k, (int count, string value, _)) in runs.Where(run => spans[run.Key + run.Value.Count - 1].Index + spans[run.Key + run.Value.Count - 1].Length == source.Length))
            if (spans[k].Index >= from && spans[k].Index < to) AddValue(manifest, folder, count == 1 ? source.Substring(spans[k].Index, spans[k].Length) : null, value, urls);

        return true;
    }

    // Sentence mode: static text between sentences and whole sentences with their values (chunks.sentence_chunks)
    private static bool AddSentences(Manifest manifest, string folder, string source, string rendered, List<(int Index, int Length)> spans,
                                     Dictionary<int, (int Count, string Value, int Rendered)> runs,
                                     List<(int Start, string Chunk, int Rendered)> chunks, int from, int to, List<NarrationClip> urls)
    {
        // Where a source offset at a unit edge lies in the rendered text (for the text shown with the clip)
        int Map(int position)
        {
            foreach ((int start, string chunk, int at) in chunks)
                if (position >= start && position <= start + chunk.Length) return at + position - start;
            foreach ((int k, (int count, string value, int at)) in runs)
            {
                if (spans[k].Index == position) return at;
                if (spans[k + count - 1].Index + spans[k + count - 1].Length == position) return at + value.Length;
            }

            return -1;
        }

        foreach ((bool group, int start, int end) in SentenceUnits(source, spans))
        {
            if (group && !(start >= from && start < to)) continue; // not played: its values need no clip (choices, rules)

            string key;
            if (!group)
            {
                key = source[start..end];
            }
            else
            {
                StringBuilder builder = new();
                int           at      = start;
                for (int k = 0; k < spans.Count; k++)
                {
                    if (spans[k].Index < start || spans[k].Index >= end || !runs.TryGetValue(k, out (int Count, string Value, int Rendered) run)) continue;

                    int runEnd = spans[k + run.Count - 1].Index + spans[k + run.Count - 1].Length;
                    string? piece = run.Count == 1 ? KeyPiece(manifest, source.Substring(spans[k].Index, spans[k].Length), run.Value)
                                  : spans.Skip(k).Take(run.Count).Any(span => StandIn(manifest, source.Substring(span.Index, span.Length))) ? null
                                  : run.Value;
                    if (piece == null) return false;
                    builder.Append(source, at, spans[k].Index - at).Append(piece);
                    at = runEnd;
                }

                key = builder.Append(source, at, end - at).ToString();
            }

            if (!manifest.Clips.TryGetValue(Hash(key), out string? script)) return false;
            bool played = group ? start >= from && start < to : end > from && start < to;
            int textStart = Map(start), textEnd = Map(end);
            string text = !group ? key : textStart >= 0 && textEnd >= textStart ? rendered[textStart..textEnd] : string.Empty;
            if (script != string.Empty && played) urls.Add(new(Url(folder, key, script), text));
        }

        return true;
    }

    // [(group, start, end)]: static text between sentences, and sentence groups from the sentence start before a
    // placeholder to the sentence end after it (chunks.sentence_units - keep identical)
    private static List<(bool Group, int Start, int End)> SentenceUnits(string text, List<(int Index, int Length)> spans)
    {
        List<(bool, int, int)> units      = new();
        int?                   groupStart = null;
        for (int k = 0; k <= spans.Count; k++)
        {
            int    baseIndex = k == 0 ? 0 : spans[k - 1].Index + spans[k - 1].Length;
            int    endIndex  = k == spans.Count ? text.Length : spans[k].Index;
            string s         = text[baseIndex..endIndex];
            List<int> ends   = Boundary.Matches(s).Select(match => match.Index + match.Length).ToList();

            int h;
            if (k == 0)
            {
                h = 0;
            }
            else
            {
                List<string>? options = Options(text.Substring(spans[k - 1].Index, spans[k - 1].Length));
                bool closed = options != null && options.All(EndsSentence) && Regex.IsMatch(s, @"^(\s|<br)");
                h = closed ? 0 : ends.Count > 0 ? ends[0] : s.Length;
            }

            bool lastStatic = k == spans.Count;
            if (groupStart != null && !lastStatic && h == s.Length && ends.Count == 0) continue;

            int t = lastStatic ? s.Length : ends.Where(e => e >= h).DefaultIfEmpty(h).Max();
            if (groupStart != null) units.Add((true, groupStart.Value, baseIndex + h));
            if (t > h) units.Add((false, baseIndex + h, baseIndex + t));
            groupStart = lastStatic ? null : baseIndex + t;
        }

        return units;
    }

    private static bool EndsSentence(string option)
    {
        string text = Regex.Replace(option, @"(?:<[^>]+>|\s)+$", string.Empty);
        return text == string.Empty || Regex.IsMatch(text, @"[.!?][""'”’)]*$");
    }

    // Top-level options of an index / condition placeholder, null for a replacement (chunks.placeholder_options)
    private static List<string>? Options(string placeholder)
    {
        string body = placeholder[2..^2];
        body = body[(body.IndexOf('=') + 1)..];
        if (body.StartsWith("?|", StringComparison.Ordinal)) return SplitTop(body[2..]).Append(string.Empty).Take(2).ToList();
        if (body.StartsWith('|')) return SplitTop(body).Skip(1).ToList();
        return null;
    }

    // body split at the "|" that are not inside a nested placeholder (chunks.split_top)
    private static List<string> SplitTop(string body)
    {
        List<string> parts = new();
        int depth = 0, last = 0, index = 0;
        while (index < body.Length)
        {
            if (string.CompareOrdinal(body, index, "{{", 0, 2) == 0) { depth++; index += 2; }
            else if (string.CompareOrdinal(body, index, "}}", 0, 2) == 0) { depth--; index += 2; }
            else if (body[index] == '|' && depth == 0) { parts.Add(body[last..index]); last = index = index + 1; }
            else index++;
        }

        parts.Add(body[last..]);
        return parts;
    }

    // A typed-in value ({{0=...|Anna|...}}, {{0=Town}}): it stays as the placeholder in a key (chunks.stand_in_of)
    private static bool StandIn(Manifest manifest, string placeholder)
    {
        if (Options(placeholder) != null) return false;
        string   body  = placeholder[2..^2];
        string[] parts = body[(body.IndexOf('=') + 1)..].Split('|');
        string   name  = parts.Length > 1 && parts[0] == "..." ? parts[1] : parts[0];
        return manifest.StandIns.Contains(name);
    }

    // What a placeholder contributes to a key: the placeholder itself for a typed-in value, else the text it was formatted
    // with; an option with nested placeholders is matched against the value, its nested values resolved the same way.
    // Null when the value is no known option.
    private static string? KeyPiece(Manifest manifest, string placeholder, string value)
    {
        if (StandIn(manifest, placeholder)) return placeholder;
        List<string>? options = Options(placeholder);
        if (options == null || options.Contains(value)) return value;

        foreach (string option in options.Where(option => option.Contains("{{")))
        {
            List<(int Index, int Length)> nested  = Placeholders(option);
            StringBuilder                 pattern = new("^");
            int                           at      = 0;
            foreach ((int index, int length) in nested)
            {
                pattern.Append(Regex.Escape(option[at..index])).Append("(.*?)");
                at = index + length;
            }

            Match match = Regex.Match(value, pattern.Append(Regex.Escape(option[at..])).Append('$').ToString(), RegexOptions.Singleline);
            if (!match.Success) continue;

            StringBuilder key = new();
            at = 0;
            for (int n = 0; n < nested.Count; n++)
            {
                string? piece = KeyPiece(manifest, option.Substring(nested[n].Index, nested[n].Length), match.Groups[n + 1].Value);
                if (piece == null) return null;
                key.Append(option, at, nested[n].Index - at).Append(piece);
                at = nested[n].Index + nested[n].Length;
            }

            return key.Append(option[at..]).ToString();
        }

        return null;
    }

    // The outermost placeholders: "{{<n>=" up to its balanced "}}", so nested placeholders are one (chunks.placeholder_spans)
    private static List<(int Index, int Length)> Placeholders(string text)
    {
        List<(int Index, int Length)> spans = new();
        int                           start = 0;
        while (PlaceholderStart.Match(text, start) is { Success: true } match)
        {
            int depth = 0;
            int index = match.Index;
            while (index < text.Length)
            {
                if (string.CompareOrdinal(text, index, "{{", 0, 2) == 0)
                {
                    depth++;
                    index += 2;
                }
                else if (string.CompareOrdinal(text, index, "}}", 0, 2) == 0)
                {
                    depth--;
                    index += 2;
                    if (depth == 0) break;
                }
                else
                {
                    index++;
                }
            }

            if (depth != 0)
            {
                start = match.Index + 2;
                continue;
            }

            spans.Add((match.Index, index - match.Index));
            start = index;
        }

        return spans;
    }

    // End of the leading bold header line ("<b>Letter from <name> - <i>date</i></b><br />"), or 0 (chunks.header_end)
    private static int HeaderEnd(string text)
    {
        if (!text.StartsWith("<b>", StringComparison.Ordinal)) return 0;

        int index = text.IndexOf("</b>", StringComparison.Ordinal);
        if (index < 0) return 0;

        string rest = text[(index + 4)..].TrimStart();
        return rest == string.Empty || rest.StartsWith("<br", StringComparison.Ordinal) ? index + 4 : 0;
    }

    // Where a static chunk follows in the rendered text. After one placeholder, a short chunk (" " between two either/or
    // sentences) can also occur inside the value: prefer the first position where the value before it has a clip, else
    // the first occurrence (player names have no clip).
    private static int Find(Manifest manifest, string rendered, string chunk, int position, int placeholders)
    {
        int first = rendered.IndexOf(chunk, position, StringComparison.Ordinal);
        if (placeholders != 1) return first;

        for (int index = first; index >= 0; index = rendered.IndexOf(chunk, index + 1, StringComparison.Ordinal))
        {
            if (manifest.Clips.ContainsKey(Hash(rendered[position..index]))) return index;
        }

        return first;
    }

    // A placeholder's value is narrated when it has a clip (a fixed variant or name); anything else (player names) is skipped
    // An option with nested placeholders (a typed-in name inside an either/or sentence) is looked up by its key text
    private static void AddValue(Manifest manifest, string folder, string? placeholder, string value, List<NarrationClip> urls)
    {
        if (value == string.Empty) return;
        string key = !manifest.Clips.ContainsKey(Hash(value)) && placeholder != null && KeyPiece(manifest, placeholder, value) is { } piece ? piece : value;
        if (manifest.Clips.TryGetValue(Hash(key), out string? script) && script != string.Empty) urls.Add(new(Url(folder, key, script), value));
    }

    private static string Url(string folder, string text, string script) => $"audio/vo/oliver/{folder}/{Hash(text)}.mp3?v={script}";

    private static string Before(string text, string cut)
    {
        int index = text.IndexOf(cut, StringComparison.Ordinal);
        return index < 0 ? text : text[..index];
    }

    public static string Hash(string text)
    {
        ulong hash = 0xcbf29ce484222325;
        foreach (byte b in Encoding.UTF8.GetBytes(text))
        {
            hash ^= b;
            hash *= 0x100000001b3;
        }

        return hash.ToString("x16");
    }
}

// One Oliver clip: its url and the on-screen text it speaks (CSV markup; js/audio.js highlights it while it plays)
public readonly record struct NarrationClip(string Url, string Text);
