"""Fills android-app/www (Capacitor's webDir) from a published wwwroot, for the Android app (.github/workflows/android-apk.yml).

    dotnet publish MyFathersWorkApp/MyFathersWorkApp/MyFathersWorkApp.csproj -c Release -o publish
    python tools/android-stage.py publish/wwwroot [--remote https://vdepagter.github.io/myfathersworkapp/]
    cd android-app && npx cap sync android

- Capacitor serves the files from https://localhost/, so <base href="/" /> stays as published (never change the
  scheme or host in capacitor.config.json: localStorage, the saves and the downloaded narration belong to that origin).
- The narration clips (audio/vo/**/*.mp3, ~385 MB) are left out. audio/vo/packs.json is written first from the full
  set (tools/audio-packs.py), so the app's Offline play panel downloads exactly the clips this build uses, from the
  published site (edition.remote, js/audio-cache.js); clips that are not downloaded stream from there (js/audio.js).
  Music, sound effects, the clip lists (<Scenario>.json) and pause lists stay in the app.
- js/edition.js says native: true and where the site is; no service worker (the app serves its own files): the worker
  files and the precompressed .br/.gz copies are left out.
- js/capacitor.js (@capacitor/core, run `npm install` in android-app first) and android-app/web/native.js (Back button)
  are added to index.html.
"""
import argparse
import os
import re
import shutil
import subprocess
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
APP = os.path.join(ROOT, "android-app")
OUT = os.path.join(APP, "www")
REMOTE = "https://vdepagter.github.io/myfathersworkapp/"


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("wwwroot", help="published wwwroot (dotnet publish -o <dir> -> <dir>/wwwroot)")
    parser.add_argument("--remote", default=REMOTE, help="published site the narration is downloaded from")
    args = parser.parse_args()
    src = os.path.abspath(args.wwwroot)
    remote = args.remote.rstrip("/") + "/"

    capacitor_js = os.path.join(APP, "node_modules", "@capacitor", "core", "dist", "capacitor.js")
    if not os.path.isfile(capacitor_js):
        print("android-app/node_modules is missing: run `npm ci` in android-app first", file=sys.stderr)
        return 1
    if not os.path.isfile(os.path.join(src, "index.html")):
        print(f"No index.html in {src}", file=sys.stderr)
        return 1

    if os.path.isdir(OUT):
        shutil.rmtree(OUT)
    shutil.copytree(src, OUT, ignore=shutil.ignore_patterns("*.br", "*.gz", "*.map"))

    # Packs list from the full set of clips, then the clips go
    if subprocess.call([sys.executable, os.path.join(ROOT, "tools", "audio-packs.py"), OUT]) != 0:
        return 1
    removed = 0
    for folder, _dirs, names in os.walk(os.path.join(OUT, "audio", "vo")):
        for name in names:
            if name.endswith(".mp3"):
                path = os.path.join(folder, name)
                removed += os.path.getsize(path)
                os.remove(path)
    print(f"Narration clips left out: {removed / 1e6:.1f} MB")

    for name in ("service-worker.js", "service-worker-assets.js", "404.html", ".nojekyll"):
        path = os.path.join(OUT, name)
        if os.path.exists(path):
            os.remove(path)

    with open(os.path.join(OUT, "js", "edition.js"), "w", encoding="utf-8", newline="\n") as f:
        f.write("// Android app (tools/android-stage.py): no service worker, narration downloaded from the published site\n")
        f.write(f'window.mfwEdition = {{ audio: true, offline: true, native: true, remote: "{remote}" }};\n')

    shutil.copyfile(capacitor_js, os.path.join(OUT, "js", "capacitor.js"))
    shutil.copyfile(os.path.join(APP, "web", "native.js"), os.path.join(OUT, "js", "native.js"))

    index = os.path.join(OUT, "index.html")
    with open(index, encoding="utf-8") as f:
        html = f.read()
    if '<base href="/" />' not in html:
        print('index.html: <base href="/" /> not found', file=sys.stderr)
        return 1
    html, n1 = re.subn(r'(\s*)<script src="js/edition.js"></script>',
                       r'\1<script src="js/capacitor.js"></script>\1<script src="js/edition.js"></script>', html)
    html, n2 = re.subn(r'(\s*)<script src="js/offline.js"></script>',
                       r'\1<script src="js/offline.js"></script>\1<script src="js/native.js"></script>', html)
    if n1 != 1 or n2 != 1:
        print("index.html: script tags for edition.js / offline.js not found", file=sys.stderr)
        return 1
    with open(index, "w", encoding="utf-8", newline="") as f:
        f.write(html)

    total = sum(os.path.getsize(os.path.join(d, n)) for d, _s, ns in os.walk(OUT) for n in ns)
    print(f"android-app/www: {total / 1e6:.1f} MB")
    return 0


if __name__ == "__main__":
    sys.exit(main())
