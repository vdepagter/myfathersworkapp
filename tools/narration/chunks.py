"""Full narration ("Oliver" voice setting): split story text into clips keyed by a hash of their source text.

A window's story text (tags of the scenario's *_Localization.csv shown with AddDefaultContent / AddNextContent*) is cut
at "<b>CHOOSE" (rules are not narrated) and split around placeholders ({{n=...}}):

- every static chunk between placeholders is one clip, named by fnv64(chunk text) - the exact CSV text, HTML included.
  A changed text gets a new name, so only changed text is rendered again;
- a placeholder is not narrated itself: the app looks up the text it was formatted with (a fixed either/or variant, an
  NPC name, a number, ...) as a "value clip" named fnv64(value). Values nobody rendered (player names) are skipped.

The app does the same split at runtime (Shared/Narration.cs - keep the regex, the cut and the hash identical), finds
the clips in wwwroot/audio/vo/oliver/<Scenario>.json and plays them in order. A window is narrated only when every
speaking chunk of its story tags has a clip; otherwise Oliver falls back to the original recording, if any.

What is spoken for a chunk: normalize() of its text (HTML dropped, link texts dropped, numbers written out), or a
hand-written override in scripts/<Scenario>/<Tag>.txt (see read_overrides).

  python tools/narration/chunks.py plan <Scenario> [--private]   # windows, chunks, characters still to render
  python tools/narration/chunks.py show <Scenario> <Tag>         # chunks of a tag with hash and spoken text
  python tools/narration/chunks.py sync <Scenario>               # manifest: private tags, cuts, settings, silent chunks
"""
import json
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))
APP = os.path.join(ROOT, "MyFathersWorkApp", "MyFathersWorkApp")
LOCALIZATION = os.path.join(APP, "wwwroot", "localization")
CODE = os.path.join(APP, "Shared", "Scenarios")
WEB = os.path.join(APP, "wwwroot", "audio", "vo", "oliver")
SCRIPTS = os.path.join(HERE, "scripts")

# Same as Shared/Narration.cs: "{{<n>=" up to its balanced "}}", so nested placeholders ({{6=?|Vlad|{{0=Name}}}},
# A Time of War ending 5 nests three deep) are one placeholder.
PLACEHOLDER_START = re.compile(r"\{\{\d+=")


def placeholder_spans(text):
    """[(start, end)] of the outermost placeholders."""
    spans, i = [], 0
    while (m := PLACEHOLDER_START.search(text, i)) is not None:
        depth, j = 0, m.start()
        while j < len(text):
            if text.startswith("{{", j):
                depth, j = depth + 1, j + 2
            elif text.startswith("}}", j):
                depth, j = depth - 1, j + 2
                if depth == 0:
                    break
            else:
                j += 1
        if depth != 0:  # unbalanced: not a placeholder
            i = m.start() + 2
            continue
        spans.append((m.start(), j))
        i = j
    return spans


def split_top(body):
    """body split at the "|" that are not inside a nested placeholder."""
    parts, depth, last, j = [], 0, 0, 0
    while j < len(body):
        if body.startswith("{{", j):
            depth, j = depth + 1, j + 2
        elif body.startswith("}}", j):
            depth, j = depth - 1, j + 2
        elif body[j] == "|" and depth == 0:
            parts.append(body[last:j])
            last = j = j + 1
        else:
            j += 1
    return parts + [body[last:]]
CUT = "<b>CHOOSE"


def settings(scenario):
    """scripts/<Scenario>/settings.json: "titles" (the window title is narrated first) and "headers" (the leading
    letter / journal header line with its date is narrated). Fear of the Unknown was rendered before both choices:
    titles false, headers true. New scenarios: titles true, headers false."""
    path = os.path.join(SCRIPTS, scenario, "settings.json")
    result = {"titles": True, "headers": False}
    if os.path.exists(path):
        with open(path, encoding="utf-8") as f:
            result.update(json.load(f))
    return result


def header_end(text):
    """End of the leading bold header line ("<b>Letter from <name> - <i>date</i></b><br />"), or 0. Same rule as
    Narration.HeaderEnd: the text starts with <b> and the first </b> is followed by a line break or ends the text."""
    if not text.startswith("<b>"):
        return 0
    i = text.find("</b>")
    rest = text[i + 4:].lstrip() if i >= 0 else "x"
    return i + 4 if rest == "" or rest.startswith("<br") else 0


# Instructions to the reader at the start of a tag: who holds the device and who may see or hear the passage
# ("<b>Carefully pick up this Storybook device and hand it to ...</b>", "<b>Do not read the passage below to the other
# players.</b>", "<b>Reminder: Do not allow any other players to see this screen.</b>"). Not narrated.
READER_NOTE = re.compile(r"(?:<b>(?:[^<]|<i>|</i>)*?(?:Storybook device|Do not read the passage|Do not allow any other players)"
                         r"(?:[^<]|<i>|</i>|<b>)*?</b>(?:\s*<i>[^<]*</i>)?(?:\s*<b>[^<]*</b>)?\s*(?:<br\s*/?>\s*)*)+")


def reader_note_end(text):
    """End of the leading reader instructions (READER_NOTE), or 0."""
    m = READER_NOTE.match(text)
    return m.end() if m else 0


def fnv64(text):
    h = 0xcbf29ce484222325
    for b in text.encode("utf-8"):
        h = ((h ^ b) * 0x100000001b3) & 0xffffffffffffffff
    return f"{h:016x}"


def load_csv(scenario, gameplay=False):
    path = os.path.join(LOCALIZATION, f"{scenario}{'_Gameplay' if gameplay else ''}_Localization.csv")
    tags = {}
    with open(path, encoding="utf-8-sig") as f:
        for i, line in enumerate(f):
            data = line.rstrip("\r\n").split(";")
            if i == 0 or data[0].startswith("//") or not data[0] or len(data) < 2:
                continue
            tags[data[0]] = data[1]
    return tags


def segment(text):
    """[("s", static) | ("p", placeholder)] of the text before CUT. Statics may be empty."""
    text = text.split(CUT)[0]
    parts, last = [], 0
    for start, end in placeholder_spans(text):
        parts.append(("s", text[last:start]))
        parts.append(("p", text[start:end]))
        last = end
    parts.append(("s", text[last:]))
    return parts


def options(placeholder):
    """Texts a placeholder can be formatted with, as far as the CSV tells: either/or options, or the preview values
    of a replacement ({{0=...|The Butcher|...}}); "..." and player-name previews are not values."""
    body = placeholder[2:-2].split("=", 1)[1]
    if body.startswith("?|"):
        opts = split_top(body[2:])[:2]
    else:
        opts = split_top(body)[1:] if body.startswith("|") else split_top(body)
    return [o for o in opts if o.strip() and o != "..." and o not in ("Oliver", "James", "Olivia") and o not in STAND_INS
            and o not in STAND_IN_ALIASES and "{{" not in o]


# ---- Spoken text -------------------------------------------------------------------------------------------------

ONES = "zero one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen".split()
TENS = "_ _ twenty thirty forty fifty sixty seventy eighty ninety".split()
ORD = {"one": "first", "two": "second", "three": "third", "five": "fifth", "eight": "eighth", "nine": "ninth", "twelve": "twelfth"}
MONTHS = "January February March April May June July August September October November December".split()


def cardinal(n):
    if n < 20:
        return ONES[n]
    if n < 100:
        return TENS[n // 10] + ("" if n % 10 == 0 else "-" + ONES[n % 10])
    if n < 1000:
        return ONES[n // 100] + " hundred" + ("" if n % 100 == 0 else " and " + cardinal(n % 100))
    return cardinal(n // 1000) + " thousand" + ("" if n % 1000 == 0 else (" and " if n % 1000 < 100 else " ") + cardinal(n % 1000))


def ordinal(n):
    words = cardinal(n)
    head, _, last = words.rpartition("-") if "-" in words else words.rpartition(" ")
    sep = "-" if "-" in words else " "
    last = ORD.get(last, (last[:-1] + "ieth") if last.endswith("y") else last + "th")
    return (head + sep if head else "") + last


def year(n):
    if 1100 <= n <= 1999 and n % 100 == 0:  # 1800 -> "eighteen hundred"
        return cardinal(n // 100) + " hundred"
    if 1100 <= n <= 1999:
        return cardinal(n // 100) + " " + ("oh-" + ONES[n % 100] if n % 100 < 10 else cardinal(n % 100))
    return cardinal(n)


def numbers(text):
    months = "|".join(MONTHS)
    text = re.sub(rf"\b(\d{{1,2}}) ({months})\b", lambda m: f"the {ordinal(int(m.group(1)))} of {m.group(2)}", text)
    text = re.sub(rf"\b({months}) (\d{{1,2}})\b(?!\d)", lambda m: f"{m.group(1)} {ordinal(int(m.group(2)))}", text)
    text = re.sub(r"\b(1[0-9]00)'?s\b", lambda m: year(int(m.group(1))) + "s", text)  # "the 1800's" -> "eighteen hundreds"
    text = re.sub(r"\b(1[0-9]\d\d)s\b", lambda m: year(int(m.group(1))).replace("ty", "ties").replace("sixties", "sixties"), text)
    text = re.sub(r"\b(1[0-9]\d\d)\b", lambda m: year(int(m.group(1))), text)
    text = re.sub(r"\$(\d+)", lambda m: cardinal(int(m.group(1))) + (" dollar" if m.group(1) == "1" else " dollars"), text)
    text = re.sub(r"\b(\d+)(st|nd|rd|th)\b", lambda m: ordinal(int(m.group(1))), text)
    text = re.sub(r"\b(\d+)\s*VP\b", lambda m: cardinal(int(m.group(1))) + " victory points", text)
    return re.sub(r"\b\d+\b", lambda m: cardinal(int(m.group(0))), text)


# Letter / journal headers name a random player ("<b>Letter from {{0=name}} - <i>date</i></b>"): the chunk before the
# name is read as a generic header, the dash after it is dropped.
HEADERS = {
    "<b>Letter from ": "A letter.",
    "<b>Journal of ": "A journal.",
    "<b>Journal of <i>name illegible - ": "A journal. The name is illegible.",
    "<b>Journal Entry of ": "A journal entry.",
    "<b>Journal Entry - ": "A journal entry.",
    "<b>Journal Entry: ": "A journal entry.",
    "<b>Journal Entry: <i>name illegible - ": "A journal entry. The name is illegible.",
    "<b>Scribbled Note - ": "A scribbled note.",
    "<b>Newsprint - ": "Newsprint.",
    "<b>Collected Letters - ": "Collected letters.",
    "<b>Brief Journal Excerpt - ": "A brief journal excerpt.",
    "<b>Anonymous Editorial - ": "An anonymous editorial.",
    "<b>Excerpt - ": "An excerpt.",
    "<b>Correspondence - ": "Correspondence.",
    "<b>Correspondence from ": "Correspondence.",
    "<b>Case File: ": "A case file.",
}


def normalize(html, strip_lead=True):
    """Spoken form of CSV text: link texts (choices) and HTML dropped, line breaks as pauses, numbers in words."""
    if html in HEADERS:
        return HEADERS[html]
    t = re.sub(r"<link=\d+>.*?</link>", " ", html)
    t = re.sub(r"<br\s*/?>", "\n", t)
    t = re.sub(r"<(icon|sprite)=[^>]+>", " ", t)
    t = re.sub(r"<[^>]+>", "", t)
    t = t.replace("&nbsp;", " ").replace("&amp;", "&")
    t = numbers(t)
    t = re.sub(r"[ \t]+", " ", t)
    t = re.sub(r" *\n *", "\n", t)
    t = re.sub(r"\n{3,}", "\n\n", t).strip()
    t = re.sub(r"\s*\(cont\.\)", "", t, flags=re.I)
    if strip_lead:  # not after a stand-in, which keeps the punctuation
        t = re.sub(r"^'s( Journal| Handwriting)?", "", t)  # "<name>'s Journal - <date>"
        t = re.sub(r"^[-:,.\s]+", "", t)  # " - <i>date</i>" after a skipped name
    return t if re.search(r"[A-Za-z]", t) else ""


def read_overrides(scenario, tag):
    """scripts/<Scenario>/<Tag>.txt: '#' comment lines (the first names the source: '# <File>.csv <Tag>'), then the
    spoken text per static chunk. A tag with placeholders separates chunks with lines '## <index>' (index of the
    static chunk, 0-based, as 'show' prints); '## <index> free' skips the drift check for a rewritten chunk (a generic
    header instead of 'Letter from <name>'). An empty chunk is silent. Returns {index: (text, free)}."""
    path = os.path.join(SCRIPTS, scenario, tag + ".txt")
    if not os.path.exists(path):
        return {}
    chunks, index, free, lines = {}, 0, False, []
    with open(path, encoding="utf-8") as f:
        for line in f:
            line = line.rstrip()
            if m := re.match(r"^## (\d+)( free)?$", line):
                if lines or index in chunks:
                    chunks[index] = ("\n".join(lines).strip(), free)
                index, free, lines = int(m.group(1)), bool(m.group(2)), []
                chunks.setdefault(index, ("", free))
            elif not line.startswith("#"):
                lines.append(line)
    chunks[index] = ("\n".join(lines).strip(), free)
    return chunks


# ---- Where the story ends: choices and rules are not narrated -------------------------------------------------------

BREAKS = re.compile(r"(?:<br\s*/?>\s*)+")
RULE_WORDS = re.compile(r"(\bVP\b|End of the Generation|End of the Round|\b[Gg]ain\b|\b[Ll]ose\b|Storybook|[Cc]hoose|[Cc]lick|\bRULES?\b|\bRules?\b)")


def auto_cut(text):
    """Offset where the narration of a tag stops, or None: the first paragraph (split at line breaks) that is
    - a choice: starts with a link, is "OR", or has a link after an unfinished sentence ("If we created the X, <link>"),
    - or a rules line: a whole bold paragraph after the first that ends with ":" ("Please choose your name:", "Choose:"),
      reads like a heading ("SPECIAL RULE", "Accepting the Offer") or names VP / gain / lose ("At the End of the
      Generation, ..."). Bold questions ("Did we create the Puppy?") and bold story sentences stay.
    The cut is at the line break before that paragraph (or 0 when it is the first one)."""
    starts, ends = [0], []
    for m in BREAKS.finditer(text):
        ends.append(m.start())
        starts.append(m.end())
    ends.append(len(text))
    for k, (a, b) in enumerate(zip(starts, ends)):
        para = text[a:b].strip()
        words = plain(re.sub(r"\{\{\d+=[^|}]*\|([^|}]*)[^}]*\}\}", r"\1", para)).strip()
        if not words:
            continue
        choice = para.startswith("<link=") or words == "OR" or (
            "<link=" in para and not re.search(r"[.!?\"']\s*$", plain(para.split("<link=")[0]).strip()))
        rules = k > 0 and re.fullmatch(r"<b>.*</b>", para) is not None and "</b>" not in para[3:-4] \
            and not words.endswith("?") and (words.endswith(":") or not re.search(r"[.!?\"]$", words) or RULE_WORDS.search(words))
        if choice or rules:
            return a if k == 0 else ends[k - 1]
    return None


def cuts(scenario):
    """scripts/<Scenario>/cuts.txt: 'Tag' (not narrated at all), 'Tag | <text>' (stop at <text>), 'Tag | -' (no cut)."""
    path = os.path.join(SCRIPTS, scenario, "cuts.txt")
    result = {}
    if os.path.exists(path):
        with open(path, encoding="utf-8") as f:
            for line in f:
                if line.strip() and not line.startswith("#"):
                    tag, _, marker = line.rstrip("\r\n").partition(" | ")
                    result[tag.strip()] = None if marker == "-" else marker
    return result


def cut_marker(scenario, tag, text):
    """The text where the narration of a tag stops ("" = not narrated, None = no cut). The app cuts at the first
    occurrence of the marker (Narration.cs), so the marker is the static text from the cut on, long enough to be unique."""
    manual = cuts(scenario)
    text = text.split(CUT)[0]
    if tag in manual:
        return "" if manual[tag] == "" else manual[tag]
    if not plain(text[reader_note_end(text):]).strip():  # only instructions to the reader
        return ""
    pos = auto_cut(text)
    if pos is None:
        return None
    if pos == 0:
        return ""
    spans = placeholder_spans(text[pos:])
    static = text[pos:pos + spans[0][0]] if spans else text[pos:]
    for n in range(12, len(static) + 1):
        if text.find(static[:n]) == pos:
            return static[:n]
    sys.exit(f"{scenario} {tag}: no unique cut marker at {text[pos:pos + 40]!r}; add it to cuts.txt")


def span(scenario, tag, text):
    """(lo, hi): the part of a tag's text that is narrated - after the reader instructions and the header line (when
    headers are not narrated), and before the cut."""
    text = text.split(CUT)[0]
    marker = cut_marker(scenario, tag, text)
    hi = len(text) if marker is None else (0 if marker == "" else text.find(marker))
    if hi < 0:
        sys.exit(f"{scenario} {tag}: cut marker {marker!r} not in the text")
    note = reader_note_end(text)
    lo = note + (0 if settings(scenario)["headers"] or tag.endswith("_Title") else header_end(text[note:]))
    return lo, hi


# Values nobody can render (typed in by the players) are replaced by a stand-in spoken inside the neighbouring clip:
# (singular, plural). Keyed by the placeholder's preview text.
STAND_INS = {
    "Oliver": ("a cousin", "the cousins"),           # player names: "one of the cousins" at a sentence start
    "Town": ("the village", "the village"),          # the village name typed in at setup
    "The Town Ledger": ("the local newspaper", ""),  # "The <village> Ledger"
    "Steam Titan": ("the warrior", "the warriors"),  # warrior names typed in (Privatized)
    "Iron Maiden": ("the warrior", "the warriors"),
    "Iron Will": ("the program", ""),                # program name typed in (Liberal)
    "Spam": ("meat", "meat"),                        # the new meat product typed in (A Time of War, Famine)
}
# Previews that stand for the same kind of value in another scenario's CSV (A Time of War)
STAND_IN_ALIASES = {"Anna": "Oliver", "Bob": "Oliver", "Duke Anna": "Oliver", "Testville": "Town"}


def stand_in_kind(placeholder):
    body = placeholder[2:-2].split("=", 1)[1].split("|")
    if len(body) < 2 or body[0] != "...":
        return None
    kind = STAND_IN_ALIASES.get(body[1], body[1])
    return kind if kind in STAND_INS else None


def plain(html):
    return re.sub(r"<[^>]+>", "", re.sub(r"<br\s*/?>", "\n", html))


def stand_ins(parts, lo=0, hi=None):
    """{part index of a static: (prefix, suffix)} - the stand-in words around typed-in values.

    Not in a header (the leading <b>...</b> line: "Letter from <name>" is read as "A letter."), except a possessive
    ("<name>'s Journal" -> "A cousin's Journal"). Sentence start -> "One of the cousins" / "The village". Two or three
    names joined by " and " / ", " -> "two of the cousins" / "the cousins". "the town of <village>" -> "the town". The
    words go in front of the next static, or after the previous one when the next one is silent."""
    text = "".join(raw for _, raw in parts)
    head = header_end(text) - 4 if header_end(text) else -1  # the </b> of the header line
    hi = len(text) if hi is None else hi
    result, pos, i = {}, 0, 0
    offsets = []
    for _, raw in parts:
        offsets.append(pos)
        pos += len(raw)
    while i < len(parts):
        kind, raw = parts[i]
        k = stand_in_kind(raw) if kind == "p" else None
        if not k or not lo <= offsets[i] < hi:  # not narrated: in a skipped header or after the cut
            i += 1
            continue
        group = 1  # names joined by " and " / ", "
        while i + 2 * group < len(parts) and parts[i + 2 * group - 1][1] in (" and ", ", ", ", and ") \
                and stand_in_kind(parts[i + 2 * group][1]) == k:
            group += 1
        nxt = i + 2 * group - 1  # static after the group
        after = parts[nxt][1] if nxt < len(parts) else ""
        before = plain("".join(r for _, r in parts[:i]))
        numeral = re.match(r" (II|III|IV)\b", after) if k == "Oliver" else None  # "<name> II" -> "a cousin"
        possessive = after[numeral.end() if numeral else 0:].startswith("'s")
        in_header = 0 <= head and offsets[i] < head
        if (in_header and not possessive) or (in_header and plain(parts[i - 1][1]).strip() in
                                              [plain(h).strip() for h in HEADERS]):
            i += 2 * group - 1
            continue
        start = not before.strip() or re.search(r"[.!?:\"\n]\s*$", before) is not None
        single, plural = STAND_INS[k]
        if group > 1:
            words = "two of the cousins" if k == "Oliver" and group == 2 else plural
        elif k == "Oliver" and start and not possessive:
            words = "one of the cousins"
        else:
            words = single
        if possessive:
            words += "'s"
        if start:
            words = words[0].upper() + words[1:]
        for j in range(i + 1, nxt, 2):  # the " and " between grouped names is said by the stand-in
            result[j] = ("", "", True)
        if k == "Town" and re.search(r"\bthe\s*$", before, re.I):  # "the <village> Jail" -> "the village Jail"
            words = "village"
        elif words.startswith("the ") and re.search(r"\bthe\s*$", before, re.I):  # "due to the <program>"
            words = words[4:]
        if k == "Iron Will" and re.match(r"(</?b>|\s)*Program", after):  # "the <name> Program" -> "the Program"
            words = ""
        if k == "Oliver" and group == 1 and re.search(r"\bdear\s*$", before, re.I):  # "My dear <name>" -> "My dear cousin"
            words = "cousin"
        if k == "Town" and re.search(r"\b(town|village) of\s*$", before):  # "the town of <village>" -> "the town"
            result[i - 1] = ("", "", "of")
            words = ""
        if words:
            # The words end the previous clip ("...a location in the village"): v4 drops a dangling last word like
            # "in", and the next chunk's text may also follow a narrated value in another tag (one clip, one text).
            # Only a possessive, or a value with no speaking clip before it, begins the next clip.
            if not possessive and i > 0 and normalize(parts[i - 1][1]):
                result[i - 1] = ("", " " + words, False)
            elif nxt < len(parts) and normalize(after[(numeral.end() if numeral else 0) + 2:] if possessive else after):
                result[nxt] = (words + " ", "", possessive)
            elif i > 0:
                result[i - 1] = ("", " " + words, False)
        i += 2 * group - 1
    return result


def tag_chunks(scenario, tag, text):
    """[(kind, raw, hash, spoken, free)] for the statics (kind "s") and placeholder values (kind "v")."""
    overrides = read_overrides(scenario, tag)
    parts = segment(text)
    lo, hi = span(scenario, tag, text)
    extra = stand_ins(parts, lo, hi)
    out, i, dollars, off = [], 0, False, 0
    for n, (kind, raw) in enumerate(parts):
        start, off = off, off + len(raw)
        if kind == "s" and (off <= lo or start >= hi) and i not in overrides:  # skipped header or after the cut
            out.append(("s", raw, fnv64(raw), "", False))
            dollars = False
            i += 1
        elif kind == "s" and (start < lo or off > hi) and i not in overrides:  # partly narrated
            part = raw[max(0, lo - start):min(len(raw), hi - start)]
            prefix, suffix, flag = extra.get(n, ("", "", False))
            prefix = "" if start < lo else prefix
            suffix = "" if off > hi else suffix
            part = part[2:] if flag is True and prefix else part  # possessive: "'s" belongs to the stand-in
            spoken = normalize(part, not prefix)
            spoken = re.sub(r"(?<=\w) +(?=[.,;:!?])", "", (prefix + spoken + suffix).strip()) if spoken or prefix or suffix else ""
            out.append(("s", raw, fnv64(raw), spoken, False))
            dollars = False
            i += 1
        elif kind == "p" and not lo <= start < hi:  # a value in a skipped header or after the cut: not played
            continue
        elif kind == "s":
            # "<name> II" / "<name> III's": the generation numeral goes with the skipped name
            body = re.sub(r"^ (II|III|IV)\b", "", raw) if n and stand_in_kind(parts[n - 1][1]) == "Oliver" else raw
            # "$<b>{{0=3}}</b> in donations": the amount is a value clip, so "dollars" moves behind it
            money = n + 1 < len(parts) and re.search(r"\$(</?b>)*$", raw) is not None
            if i in overrides:
                spoken, free = overrides[i]
            else:
                prefix, suffix, flag = extra.get(n, ("", "", False))
                if flag is True and not prefix:  # " and " said by a group stand-in
                    spoken = ""
                elif flag is True:  # possessive: "'s" belongs to the stand-in
                    spoken = prefix + normalize(body[2:], False) if normalize(body[2:]) else prefix.strip()
                else:
                    spoken = normalize(body, not prefix)
                    if flag == "of":
                        spoken = re.sub(r"\s+of$", "", spoken)
                    spoken = (prefix + spoken + suffix).strip() if spoken or prefix or suffix else ""
                    spoken = re.sub(r"(?<=\w) +(?=[.,;:!?])", "", spoken)  # "the village ." -> "the village."
                if dollars:
                    spoken = re.sub(r"(?<=\w) +(?=[.,;:!?])", "", "dollars " + normalize(body, False)).strip()
                if money:
                    spoken = re.sub(r"\s*\$$", "", spoken)
                free = False
            dollars = money
            out.append(("s", raw, fnv64(raw), spoken, free))
            i += 1
        else:
            for o in options(raw):
                out.append(("v", o, fnv64(o), normalize(o), False))
    return out


# ---- Windows of a scenario (static parse of the scenario code) ----------------------------------------------------

METHOD = re.compile(r"^\s*(?:private|public) static void (\w+)\([^)]*GlobalData globalData\)", re.M)


def windows(scenario):
    """{method: [tags]} of the methods that build a GameplayWindow, in source order, plus the set of methods that are
    private (target of AddDoNotOtherPlayersToSee / AddOnceYouAreReady, or listed in scripts/<Scenario>/private.txt)."""
    folder = os.path.join(CODE, scenario)
    story = load_csv(scenario)
    result, private = {}, set()
    for name in sorted(os.listdir(folder)):
        if not name.endswith(".cs"):
            continue
        with open(os.path.join(folder, name), encoding="utf-8-sig") as f:
            code = f.read()
        heads = list(METHOD.finditer(code))
        for k, m in enumerate(heads):
            body = code[m.end():heads[k + 1].start() if k + 1 < len(heads) else len(code)]
            for p in re.finditer(r"AddDoNotOtherPlayersToSee\([^;]*?,\s*(\w+)\)|AddOnceYouAreReady\((\w+)\)", body):
                private.add(p.group(1) or p.group(2))
            if "new GameplayWindow" not in body:
                continue
            tags = []
            if settings(scenario)["titles"]:  # the window title first (GameplayWindow.AddDefaultTitle / AddDefaultBaseTitle)
                if ".AddDefaultTitle(" in body:
                    tags.append(m.group(1) + "_Title")
                elif ".AddDefaultBaseTitle(" in body:
                    tags.append(m.group(1).split("_")[0] + "_Title")
            for c in re.finditer(r"\.(AddDefaultContent|AddNextContentWithLinks|AddNextContent)\((\d*(?=\s*[,)]))?", body):
                if c.group(1) == "AddDefaultContent":
                    tags.append(m.group(1) + "_Content")
                elif c.group(2):
                    tags.append(m.group(1) + "_Content" + c.group(2))
                else:  # index computed in code: every <Method>_Content<n> of the CSV
                    tags += sorted((t for t in story if re.fullmatch(m.group(1) + r"_Content\d+", t)),
                                   key=lambda t: int(t.rsplit("Content", 1)[1]))
            tags += re.findall(r'\.AddStoryContent\("(\w+)"\)', body)
            result[m.group(1)] = list(dict.fromkeys(tags))
    def listed(name):
        path = os.path.join(SCRIPTS, scenario, name)
        if not os.path.exists(path):
            return set()
        with open(path, encoding="utf-8") as f:
            return {l.strip() for l in f if l.strip() and not l.startswith("#")}
    private |= listed("private.txt")
    for method in listed("skip.txt"):  # not narrated (input forms, battle)
        result.pop(method, None)
    return result, private


def manifest_path(scenario):
    return os.path.join(WEB, scenario + ".json")


def load_manifest(scenario):
    if not os.path.exists(manifest_path(scenario)):
        return {"clips": {}, "private": []}
    with open(manifest_path(scenario), encoding="utf-8") as f:
        return json.load(f)


def save_manifest(scenario, manifest):
    os.makedirs(WEB, exist_ok=True)
    manifest["clips"] = dict(sorted(manifest["clips"].items()))
    manifest["private"] = sorted(set(manifest["private"]))
    with open(manifest_path(scenario), "w", encoding="utf-8", newline="\n") as f:
        json.dump(manifest, f, indent=1)
        f.write("\n")


def expand(story, tag):
    """Every text a one-placeholder tag can produce ("@Warden" in values.txt -> "The Butcher", "The Bloodsmith")."""
    parts = segment(story[tag])
    if len(parts) != 3:
        sys.exit(f"values.txt: @{tag} must have exactly one placeholder")
    return [parts[0][1] + o + parts[2][1] for o in options(parts[1][1])]


def plan(scenario, include_private=True):
    """Work items in render order: [(method, tag, private, chunks)] for the story tags of every window."""
    story = load_csv(scenario)
    wins, private = windows(scenario)
    items = []
    for method, tags in wins.items():
        for tag in tags:
            if tag in story:
                items.append((method, tag, method in private, tag_chunks(scenario, tag, story[tag])))
    items.sort(key=lambda x: x[2])  # private sections last
    values = os.path.join(SCRIPTS, scenario, "values.txt")
    if os.path.exists(values):  # placeholder values the CSV previews do not show (NPC names, factions, ...)
        with open(values, encoding="utf-8") as f:
            vals = [l.rstrip("\r\n") for l in f if l.strip() and not l.startswith("#")]
        vals = [x for v in vals for x in (expand(story, v[1:]) if v.startswith("@") else [v])]
        items.insert(0, ("", "values.txt", False, [("v", v, fnv64(v), normalize(v), False) for v in vals]))
    return [x for x in items if include_private or not x[2]]


def sync_manifest(scenario):
    """Manifest fields that need no rendering: private tags, the cut markers, the title / header settings, and silent
    chunks ("" - also a chunk that was narrated before a cut moved). Saved; returns the manifest."""
    manifest = load_manifest(scenario)
    items = plan(scenario)
    story = load_csv(scenario)
    manifest["private"] = sorted(set(manifest["private"]) | {tag for m, tag, p, c in items if p})
    manifest["titles"] = settings(scenario)["titles"]
    manifest["headers"] = settings(scenario)["headers"]
    manifest["cuts"] = {}
    for method, tag, is_private, chunks in items:
        if tag in story and (marker := cut_marker(scenario, tag, story[tag])) is not None:
            manifest["cuts"][tag] = marker
    manifest["cuts"] = dict(sorted(manifest["cuts"].items()))
    speaking = {h for m, t, p, c in items for kind, raw, h, spoken, free in c if spoken}
    for m, t, p, c in items:
        for kind, raw, h, spoken, free in c:
            if not spoken and h not in speaking:
                manifest["clips"][h] = ""
    save_manifest(scenario, manifest)
    return manifest


def script_hash(spoken):
    return fnv64(spoken)[:8] if spoken else ""


# ---- Drift check of the hand-written overrides --------------------------------------------------------------------

def check(scenario, tag):
    """Word diff of every override chunk against its source chunk (elevenlabs.check rules); 'free' chunks are skipped."""
    import elevenlabs
    story = load_csv(scenario)
    if tag not in story:
        return [f"tag {tag} not in {scenario}_Localization.csv"]
    statics = [raw for kind, raw in segment(story[tag]) if kind == "s"]
    problems = []
    for index, (spoken, free) in read_overrides(scenario, tag).items():
        if index >= len(statics):
            problems.append(f"## {index}: the tag has only {len(statics)} chunks")
        elif not free:
            problems += [f"## {index} {p}" for p in elevenlabs.diff(statics[index], spoken)]
    return problems


def check_all():
    bad = 0
    for scenario in sorted(os.listdir(SCRIPTS)):
        for name in sorted(os.listdir(os.path.join(SCRIPTS, scenario))):
            if name.endswith(".txt") and name not in ("values.txt", "private.txt", "skip.txt", "cuts.txt"):
                problems = check(scenario, name[:-4])
                bad += bool(problems)
                print(f"{scenario}/{name[:-4]}: " + ("ok" if not problems else "DRIFT\n  " + "\n  ".join(problems)))
    return bad


# ---- Rendering ----------------------------------------------------------------------------------------------------

def strip_tags(text):
    return re.sub(r"[ ]*\[[^\]]+\][ ]*", " ", text).replace("\n ", "\n").strip()


def render(scenario, reserve=6000, limit=None, include_private=True, only=None, dry=False):
    """Renders every chunk whose clip is missing or whose spoken text changed, in plan order (private sections last).

    Model, settings and audio_tags come from voices.json "oliver". Context (previous_text / next_text: the neighbouring
    chunks of the tag) keeps the intonation across the joins. A 192 kbps master goes to out/master/oliver/<Scenario>/,
    a 96 kbps mono copy to wwwroot/audio/vo/oliver/<Scenario>/; the manifest is saved after every clip, so an
    interrupted run resumes. The quota is checked at the start and every ~10k characters; the run stops when fewer than
    'reserve' characters would be left, or after 'limit' characters.
    """
    import subprocess
    import elevenlabs
    voice = elevenlabs.load(elevenlabs.VOICES)["oliver"]
    voice_id = elevenlabs.load(elevenlabs.LOCAL).get("oliver")
    model, settings, tags = voice["model"], voice["settings"], voice.get("audio_tags", True)
    if check_all():
        sys.exit("Script overrides drift from the on-screen text (see above).")
    manifest = sync_manifest(scenario)
    master = os.path.join(HERE, "out", "master", "oliver", scenario)
    ship = os.path.join(WEB, scenario)
    os.makedirs(master, exist_ok=True)
    os.makedirs(ship, exist_ok=True)

    def remaining():
        s = elevenlabs.call("GET", "/v1/user/subscription")
        return s["character_limit"] - s["character_count"], s["character_count"]

    left, used0 = remaining()
    print(f"Quota: {left} characters left, reserve {reserve}")
    done, since_check, clips = 0, 0, 0
    for method, tag, is_private, chunks in plan(scenario, include_private):
        if only and tag not in only:
            continue
        spoken_all = [c[3] for c in chunks if c[0] == "s"]
        for kind, raw, h, spoken, free in chunks:
            if not spoken:
                continue
            if manifest["clips"].get(h) == script_hash(spoken) and os.path.exists(os.path.join(ship, h + ".mp3")):
                continue
            text = spoken if tags else strip_tags(spoken)
            if limit is not None and done + len(text) > limit:
                print(f"Stopped at the limit of {limit} characters.")
                return save_manifest(scenario, manifest)
            if since_check >= 10000 or left - since_check < reserve + len(text):
                left, used = remaining()
                since_check = 0
                print(f"Quota: {left} left; billed {used - used0} for {done} characters sent this run")
                if left < reserve + len(text):
                    print(f"Stopped: only {left} characters left (reserve {reserve}).")
                    return save_manifest(scenario, manifest)
            if dry:
                print(f"would render {tag} {h} ({len(text)})")
                done += len(text)
                continue
            body = {"text": text, "model_id": model, "voice_settings": settings}
            if kind == "s":
                i = spoken_all.index(spoken)
                before = " ".join(x for x in spoken_all[:i] if x)[-300:]
                after = " ".join(x for x in spoken_all[i + 1:] if x)[:300]
                if before:
                    body["previous_text"] = before if tags else strip_tags(before)
                if after:
                    body["next_text"] = after if tags else strip_tags(after)
            audio = elevenlabs.call("POST", f"/v1/text-to-speech/{voice_id}?output_format=mp3_44100_192", body, raw=True, retries=4)
            path = os.path.join(master, h + ".mp3")
            with open(path, "wb") as f:
                f.write(audio)
            subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", path, "-ac", "1", "-ar", "44100", "-b:a", "96k",
                            "-map_metadata", "-1", os.path.join(ship, h + ".mp3")], check=True)
            warn = "  WARNING: ends mid-sound" if elevenlabs.tail_db(path) > -50 else ""
            manifest["clips"][h] = script_hash(spoken)
            save_manifest(scenario, manifest)
            done += len(text)
            since_check += len(text)
            clips += 1
            print(f"{done:6} {'P ' if is_private else ''}{tag} {h} ({len(text)}){warn}", flush=True)
    save_manifest(scenario, manifest)
    print(f"Done: {clips} clips, {done} characters.")


if __name__ == "__main__":
    a = sys.argv[1:]
    if len(a) >= 2 and a[0] == "plan":
        man = load_manifest(a[1])["clips"]
        seen, total, todo, priv = set(), 0, 0, 0
        for method, tag, is_private, chunks in plan(a[1]):
            for kind, raw, h, spoken, free in chunks:
                if h in seen or not spoken:
                    continue
                seen.add(h)
                total += len(spoken)
                if man.get(h) != script_hash(spoken):
                    todo += len(spoken)
                    priv += len(spoken) if is_private else 0
            if "--list" in a:
                print(f"{'P ' if is_private else '  '}{tag}: {sum(len(c[3]) for c in chunks)}")
        print(f"{a[1]}: {len(seen)} clips, {total} characters, {todo} still to render ({priv} in private sections)")
        texts = {}
        for method, tag, is_private, chunks in plan(a[1]):
            for kind, raw, h, spoken, free in chunks:
                texts.setdefault(h, {}).setdefault(spoken, tag)
        for h, variants in texts.items():
            if len(variants) > 1:
                print(f"CONFLICT {h}: one chunk, different spoken text in {', '.join(variants.values())}")
    elif len(a) == 3 and a[0] == "show":
        text, i = load_csv(a[1])[a[2]], 0
        for kind, raw, h, spoken, free in tag_chunks(a[1], a[2], text):
            print(f"[{f'## {i}' if kind == 's' else 'value'} {h}{' free' if free else ''}] {raw[:80]!r}\n    -> {spoken!r}")
            i += kind == "s"
    elif len(a) == 2 and a[0] == "sync":
        sync_manifest(a[1])
    elif a and a[0] == "check":
        sys.exit(1 if check_all() else 0)
    elif len(a) >= 2 and a[0] == "render":
        opt = lambda k: next((x.split("=", 1)[1] for x in a if x.startswith(f"--{k}=")), None)
        render(a[1], int(opt("reserve") or 6000), int(opt("max")) if opt("max") else None, "--no-private" not in a,
               set(opt("tags").split(",")) if opt("tags") else None, "--dry" in a)
    else:
        sys.exit(__doc__)
