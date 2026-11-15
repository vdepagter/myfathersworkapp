"""Pauses of the Oliver clips, so the app can highlight the sentence that is being spoken.

A clip can hold many sentences (up to a few minutes). The app (wwwroot/js/narration-sync.js) splits the clip's on-screen
text into sentences and matches the sentence ends with the pauses listed here; without this list (or when a clip was
rendered again and its length no longer matches) it falls back to an estimate from the text length, which can be
seconds off in a long clip.

  python tools/narration/pauses.py [<Scenario> ...]     # all scenarios when none is given

Writes wwwroot/audio/vo/oliver/<Scenario>.pauses.json: {"<clip hash>": [duration, start, end, start, end, ...]} in
seconds (silences of at least 0.18 s below -38 dB, the ones at the very start and end included). Needs ffmpeg and the
real audio files (git lfs pull). Run it after every render and commit the file with the clips.
"""
import json
import os
import re
import subprocess
import sys
from concurrent.futures import ThreadPoolExecutor

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
WEB = os.path.join(ROOT, "MyFathersWorkApp", "MyFathersWorkApp", "wwwroot", "audio", "vo", "oliver")
NOISE, MINIMUM = "-38dB", 0.18


def pauses(path):
    """[duration, start, end, ...] of one clip, or None when ffmpeg cannot read it."""
    out = subprocess.run(["ffmpeg", "-hide_banner", "-nostdin", "-i", path, "-af", f"silencedetect=noise={NOISE}:d={MINIMUM}",
                          "-f", "null", "-"], capture_output=True, text=True, encoding="utf-8", errors="replace").stderr
    times = re.findall(r"time=(\d+):(\d+):([\d.]+)", out)  # the decoded length (the header's Duration is an estimate)
    if not times:
        return None
    duration = int(times[-1][0]) * 3600 + int(times[-1][1]) * 60 + float(times[-1][2])
    starts = [float(x) for x in re.findall(r"silence_start: (-?[\d.]+)", out)]
    ends = [float(x) for x in re.findall(r"silence_end: ([\d.]+)", out)]
    if len(ends) < len(starts):
        ends.append(duration)
    result = [round(duration, 2)]
    for start, end in zip(starts, ends):
        result += [round(max(0.0, start), 2), round(min(duration, end), 2)]
    return result


def run(scenario):
    folder = os.path.join(WEB, scenario)
    names = sorted(n for n in os.listdir(folder) if n.endswith(".mp3"))
    real = [n for n in names if os.path.getsize(os.path.join(folder, n)) > 1024]  # not a Git LFS pointer
    if len(real) < len(names):
        print(f"{scenario}: {len(names) - len(real)} clips are Git LFS pointers (git lfs pull) - skipped")
    with ThreadPoolExecutor(max_workers=max(2, (os.cpu_count() or 4) // 2)) as pool:
        found = list(pool.map(lambda n: pauses(os.path.join(folder, n)), real))
    result = {name[:-4]: p for name, p in zip(real, found) if p}
    path = os.path.join(WEB, scenario + ".pauses.json")
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("{\n" + ",\n".join(f' "{k}": {json.dumps(v, separators=(",", ":"))}' for k, v in sorted(result.items())) + "\n}\n")
    print(f"{scenario}: {len(result)} clips, {sum((len(v) - 1) // 2 for v in result.values())} pauses -> {os.path.relpath(path, ROOT)}")


if __name__ == "__main__":
    scenarios = sys.argv[1:] or sorted(n for n in os.listdir(WEB) if os.path.isdir(os.path.join(WEB, n)))
    for name in scenarios:
        if not os.path.isdir(os.path.join(WEB, name)):
            sys.exit(f"No clips folder for {name}")
        run(name)
