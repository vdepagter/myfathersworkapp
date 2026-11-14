"""ElevenLabs narration pilot: design voices from a prompt and render tagged narration scripts.

Needs only the Python standard library. The API key is read from the environment variable
ELEVENLABS_API_KEY (never put it in a file in the repository).

  python tools/narration/elevenlabs.py design male            # 3 preview MP3s for the "male" prompt in voices.json
  python tools/narration/elevenlabs.py save male <generated_voice_id>   # keep a preview as a voice, id -> voices.local.json
  python tools/narration/elevenlabs.py render male TheCostOfDisease/WolvesEvil1         # scripts/<name>.txt -> out/male/<name>.mp3
  python tools/narration/elevenlabs.py render male TheCostOfDisease/WolvesEvil1 --plain # without audio tags -> out/male-plain/
  python tools/narration/elevenlabs.py render oliver TheCostOfDisease/Dubious --model=eleven_v4  # -> out/oliver-v4/
  python tools/narration/elevenlabs.py render oliver <name> --stored               # the voice's own saved settings
  python tools/narration/elevenlabs.py render oliver <name> --set=similarity_boost=0.9 --label=sim90  # -> out/oliver-sim90/
  python tools/narration/elevenlabs.py render oliver TheCostOfDisease/Dubious --web     # for the game, with the model and
        # settings of voices.json: 192 kbps master -> out/master/oliver/<name>.mp3 (keep it, not in git), 96 kbps mono
        # copy (ffmpeg) -> wwwroot/audio/vo/ai/<name>.mp3 (GameplayWindow.SetAiVoiceOver)
  # v3 renders get " ... [long pause]" appended (v3 cuts off the last word otherwise; --no-tail to skip); every render
  # warns when the clip still ends mid-sound.
  python tools/narration/elevenlabs.py check [<name> ...]               # script vs on-screen text (all scripts);
                                                                        # render runs it too and refuses on drift
  python tools/narration/elevenlabs.py credits                          # remaining character credits

Output goes to tools/narration/out/ (git-ignored). Scripts are plain text with ElevenLabs v3 audio tags
([whispers], [sighs], ...); lines starting with # are comments. The first line names the source: "# <File>.csv <Tag>". They are kept apart from the localization CSVs
so tags never appear on screen.
"""
import base64
import difflib
import json
import os
import re
import subprocess
import sys
import urllib.error
import urllib.parse
import urllib.request

HERE = os.path.dirname(os.path.abspath(__file__))
API = "https://api.elevenlabs.io"
VOICES = os.path.join(HERE, "voices.json")
LOCAL = os.path.join(HERE, "voices.local.json")  # saved voice ids, git-ignored
SCRIPTS = os.path.join(HERE, "scripts")
OUT = os.path.join(HERE, "out")
WEB = os.path.join(HERE, "..", "..", "MyFathersWorkApp", "MyFathersWorkApp", "wwwroot", "audio", "vo", "ai")


def key():
    k = os.environ.get("ELEVENLABS_API_KEY")
    if not k and sys.platform == "win32":  # set with setx but this terminal started before it
        import winreg
        try:
            with winreg.OpenKey(winreg.HKEY_CURRENT_USER, "Environment") as reg:
                k = winreg.QueryValueEx(reg, "ELEVENLABS_API_KEY")[0]
        except OSError:
            pass
    if not k:
        sys.exit("Set ELEVENLABS_API_KEY first (setx ELEVENLABS_API_KEY <key>, then open a new terminal).")
    return k


def call(method, path, body=None, raw=False, retries=0):
    """API call; retries (2, 4, 8, ... s) on rate limits, server errors and network errors."""
    import time
    for attempt in range(retries + 1):
        req = urllib.request.Request(API + path, method=method,
                                     data=json.dumps(body).encode() if body is not None else None,
                                     headers={"xi-api-key": key(), "Content-Type": "application/json"})
        try:
            with urllib.request.urlopen(req, timeout=300) as r:
                data = r.read()
            return data if raw else json.loads(data)
        except urllib.error.HTTPError as e:
            message = f"HTTP {e.code} on {path}: {e.read().decode(errors='replace')}"
            if e.code not in (429, 500, 502, 503, 504) or attempt == retries:
                sys.exit(message)
        except (urllib.error.URLError, TimeoutError) as e:
            message = f"{e} on {path}"
            if attempt == retries:
                sys.exit(message)
        print(f"  {message} - retrying", flush=True)
        time.sleep(2 ** (attempt + 1))


def load(path):
    if not os.path.exists(path):
        return {}
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def script_text(name):
    with open(os.path.join(SCRIPTS, name + ".txt"), encoding="utf-8") as f:
        lines = [l.rstrip() for l in f if not l.startswith("#")]
    return "\n".join(lines).strip()


LOCALIZATION = os.path.join(HERE, "..", "..", "MyFathersWorkApp", "MyFathersWorkApp", "wwwroot", "localization")
SOURCE = re.compile(r"^# (\w+\.csv) (\w+)")  # first line of every script: "# <CSV file> <Tag> ..."


def words(text):
    text = re.sub(r"\[[^\]]+\]|<[^>]+>", " ", text)                  # audio tags, HTML
    text = re.sub(r"\{\{\d+=[^}]*\}\}", " _ ", text)                    # placeholder -> wildcard "_"
    return re.findall(r"[a-z0-9_']+", text.lower().replace("’", "'"))


def diff(src, spoken):
    """Word-level diff of spoken text against its CSV source text: words of the on-screen text missing or changed.

    Allowed: inserted words (interjections, generic headers), and replacing numbers, "Dr" or a placeholder (spelled-out
    years, names). The source is compared up to "<b>CHOOSE" (rules are not narrated). Returns a list of problems.
    """
    a, b = words(src.split("<b>CHOOSE")[0]), words(spoken)
    problems = []
    for op, i1, i2, j1, j2 in difflib.SequenceMatcher(None, a, b, autojunk=False).get_opcodes():
        lost = [w for w in a[i1:i2] if not (w.isdigit() or w in ("_", "dr", "ii", "iii", "iv"))]
        if op in ("delete", "replace") and lost:
            problems.append(f"{op}: '{' '.join(a[i1:i2])}' -> '{' '.join(b[j1:j2])}'")
    return problems


def check(name):
    """Drift check of scripts/<Scenario>/<Tag>.txt, chunk by chunk (chunks.check)."""
    import chunks
    scenario, tag = name.split("/")
    return chunks.check(scenario, tag)


def check_all(names):
    bad = 0
    for name in names:
        problems = check(name)
        bad += bool(problems)
        print(f"{name}: " + ("ok" if not problems else "DRIFT\n  " + "\n  ".join(problems)))
    return bad


def all_scripts():
    return sorted(os.path.relpath(os.path.join(d, f), SCRIPTS)[:-4].replace(os.sep, "/")
                  for d, _, fs in os.walk(SCRIPTS) for f in fs if f.endswith(".txt"))


def design(voice):
    v = load(VOICES)[voice]
    res = call("POST", "/v1/text-to-voice/design?output_format=mp3_44100_128", {
        "voice_description": v["prompt"],
        "model_id": "eleven_ttv_v3",
        "text": v["preview_text"],
        "guidance_scale": v.get("guidance_scale", 5),
    })
    d = os.path.join(OUT, "design", voice)
    os.makedirs(d, exist_ok=True)
    for i, p in enumerate(res["previews"], 1):
        path = os.path.join(d, f"preview{i}_{p['generated_voice_id']}.mp3")
        with open(path, "wb") as f:
            f.write(base64.b64decode(p["audio_base_64"]))
        print(f"{path}  ({p['duration_secs']:.0f} s)")
    print(f"Listen, then: python tools/narration/elevenlabs.py save {voice} <generated_voice_id>")


def save(voice, generated_id):
    v = load(VOICES)[voice]
    res = call("POST", "/v1/text-to-voice", {
        "voice_name": v["name"],
        "voice_description": v["prompt"][:1000],
        "generated_voice_id": generated_id,
    })
    local = load(LOCAL)
    local[voice] = res["voice_id"]
    with open(LOCAL, "w", encoding="utf-8") as f:
        json.dump(local, f, indent=2)
    print(f"Saved {voice} as voice {res['voice_id']}")


# Eleven v3 tends to cut off the end of the last word (the clip stops mid-sound). Text after the last sentence gives
# the cut somewhere harmless to land; tail_db() then checks whether the clip still ends in sound.
V3_TAIL = " ... [long pause]"


def tail_db(path):
    """RMS level (dB) of the last 80 ms; above about -50 dB the clip ends mid-sound (cut off)."""
    out = subprocess.run(["ffmpeg", "-sseof", "-0.08", "-i", path, "-af", "astats", "-f", "null", "-"],
                         capture_output=True, text=True).stderr
    levels = re.findall(r"RMS level dB: (-?[\d.]+|-inf)", out)
    return float(levels[-1]) if levels and levels[-1] != "-inf" else -200.0


def trim_tail(path, keep=0.5):
    """Cut a clip 'keep' seconds into its last silence (>= 0.4 s below -60 dB), dropping what v3 made of the tail."""
    out = subprocess.run(["ffmpeg", "-i", path, "-af", "silencedetect=noise=-60dB:d=0.4", "-f", "null", "-"],
                         capture_output=True, text=True).stderr
    starts = [float(x) for x in re.findall(r"silence_start: ([\d.]+)", out)]
    duration = float(subprocess.run(["ffprobe", "-v", "error", "-show_entries", "format=duration", "-of", "csv=p=0", path],
                                    capture_output=True, text=True).stdout)
    if not starts or duration - starts[-1] > 3:  # no silence near the end: leave it (tail_db will warn)
        return
    tmp = path + ".tmp.mp3"
    subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", path, "-t", f"{starts[-1] + keep:.3f}", "-c", "copy", tmp], check=True)
    os.replace(tmp, path)


def render(voice, names, plain=False, web=False, model=None, stored=False, overrides=None, label=None, tail=True):
    """Render scripts. Model and voice_settings come from voices.json (what the game clips use) unless overridden.

    stored: send no voice_settings, so the voice's own saved settings apply (for a library voice: the creator's).
    web: render a 192 kbps master to out/master/<voice>/ (kept offline) and ship a 96 kbps mono copy (ffmpeg) to WEB.
    """
    v = load(VOICES)[voice]
    voice_id = load(LOCAL).get(voice) or v.get("voice_id")
    if not voice_id:
        sys.exit(f"No voice id for {voice}: run design + save first.")
    model = model or v.get("model", "eleven_v3")
    settings = None if stored else {**v.get("settings", {}), **(overrides or {})}
    if web:
        d, fmt = os.path.join(OUT, "master", voice), "mp3_44100_192"
    else:
        suffix = label or (("-plain" if plain else "") + ("" if model == "eleven_v3" else "-" + model.removeprefix("eleven_")))
        d, fmt = os.path.join(OUT, voice + (("-" + suffix) if label else suffix)), "mp3_44100_128"
    for name in names:
        text = script_text(name)
        if plain:  # comparison render without audio tags
            text = re.sub(r"[ ]*\[[^\]]+\][ ]*", " ", text).replace("\n ", "\n").strip()
        if tail and model.startswith("eleven_v3"):
            text += V3_TAIL
        body = {"text": text, "model_id": model}
        if settings is not None:
            body["voice_settings"] = settings
        audio = call("POST", f"/v1/text-to-speech/{voice_id}?output_format={fmt}", body, raw=True)
        path = os.path.normpath(os.path.join(d, name + ".mp3"))
        os.makedirs(os.path.dirname(path), exist_ok=True)
        with open(path, "wb") as f:
            f.write(audio)
        print(f"{path}  ({len(text)} characters, {model}, settings {settings if settings is not None else 'stored'})")
        if tail and model.startswith("eleven_v3"):
            trim_tail(path)
        if (db := tail_db(path)) > -50:
            print(f"  WARNING: ends mid-sound ({db:.0f} dB in the last 80 ms) - the last word is probably cut off, render again")
        if web:
            ship = os.path.normpath(os.path.join(WEB, name + ".mp3"))
            os.makedirs(os.path.dirname(ship), exist_ok=True)
            subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", path, "-ac", "1", "-ar", "44100", "-b:a", "96k",
                            "-map_metadata", "-1", ship], check=True)
            print(f"  -> {ship}")


def library(search):
    res = call("GET", "/v1/shared-voices?page_size=20&search=" + urllib.parse.quote(search))
    for v in res["voices"]:
        print(f"{v['public_owner_id']} {v['voice_id']}  {v['name']}  ({v.get('accent')}, {v.get('age')}, {v.get('category')})")


def add(voice, owner_id, voice_id, name):
    res = call("POST", f"/v1/voices/add/{owner_id}/{voice_id}", {"new_name": name})
    local = load(LOCAL)
    local[voice] = res["voice_id"]
    with open(LOCAL, "w", encoding="utf-8") as f:
        json.dump(local, f, indent=2)
    print(f"Added {name} as {voice} ({res['voice_id']})")


def credits():
    s = call("GET", "/v1/user/subscription")
    print(f"{s['tier']}: {s['character_count']} / {s['character_limit']} characters used")


if __name__ == "__main__":
    a = sys.argv[1:]
    if not a:
        sys.exit(__doc__)
    if a[0] == "design":
        design(a[1])
    elif a[0] == "save":
        save(a[1], a[2])
    elif a[0] == "render":
        flags = [x for x in a if x.startswith("--")]
        a = [x for x in a if not x.startswith("--")]
        opt = lambda k: next((f.split("=", 1)[1] for f in flags if f.startswith(f"--{k}=")), None)
        overrides = {k: float(x) for k, x in (f.split("=", 1)[1].split("=") for f in flags if f.startswith("--set="))}
        if "--force" not in flags and check_all(a[2:]):
            sys.exit("Script text drifts from the on-screen text (see above). Fix the script, or pass --force.")
        render(a[1], a[2:], "--plain" in flags, "--web" in flags, opt("model"), "--stored" in flags, overrides, opt("label"),
               "--no-tail" not in flags)
    elif a[0] == "library":
        library(" ".join(a[1:]))
    elif a[0] == "add":
        add(a[1], a[2], a[3], " ".join(a[4:]) or a[1])
    elif a[0] == "check":
        import chunks
        sys.exit(1 if (check_all(a[1:]) if a[1:] else chunks.check_all()) else 0)
    elif a[0] == "credits":
        credits()
    else:
        sys.exit(__doc__)
