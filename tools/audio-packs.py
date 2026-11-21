"""Writes <wwwroot>/audio/vo/packs.json: the narration downloads for offline play (js/audio-cache.js).

One pack per folder of clips under audio/vo (oliver/<Scenario> -> "oliver-<Scenario>", male, female), every file with
a content hash (first 16 hex digits of its SHA-256) and its size. The hash is part of the key a clip is stored under in
the browser, so an unchanged clip survives app updates and a changed one is downloaded again.

Run on the published wwwroot after `dotnet publish` (the GitHub Pages workflow does):
    python tools/audio-packs.py publish/wwwroot
"""
import hashlib
import json
import os
import sys


def main() -> int:
    if len(sys.argv) != 2:
        print("Usage: python tools/audio-packs.py <published wwwroot>", file=sys.stderr)
        return 2
    root = os.path.abspath(sys.argv[1])
    vo = os.path.join(root, "audio", "vo")
    if not os.path.isdir(vo):
        print(f"No audio/vo folder in {root}", file=sys.stderr)
        return 1

    packs = []
    for folder, _dirs, names in sorted(os.walk(vo)):
        clips = sorted(n for n in names if n.endswith(".mp3"))
        if not clips:
            continue
        rel = os.path.relpath(folder, vo).replace(os.sep, "/")
        parts = rel.split("/")
        files = []
        for name in clips:
            path = os.path.join(folder, name)
            with open(path, "rb") as f:
                digest = hashlib.sha256(f.read()).hexdigest()[:16]
            files.append([f"audio/vo/{rel}/{name}", digest, os.path.getsize(path)])
        packs.append({
            "id": rel.replace("/", "-"),
            "voice": parts[0],
            "scenario": parts[1] if len(parts) > 1 else "",
            "files": files,
        })

    out = os.path.join(vo, "packs.json")
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        json.dump({"version": 1, "packs": packs}, f, separators=(",", ":"))
    for p in packs:
        size = sum(f[2] for f in p["files"])
        print(f"{p['id']}: {len(p['files'])} clips, {size / 1e6:.1f} MB")
    return 0


if __name__ == "__main__":
    sys.exit(main())
