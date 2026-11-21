"""Prepares the published wwwroot for GitHub Pages (.github/workflows/deploy-pages.yml); also used to test locally.

    python tools/pages-prepare.py publish/wwwroot <repo name>

- The site is served from /<repo>/, not from the domain root: <base href> in index.html.
- The offline service worker checks every file against its hash (service-worker-assets.js), so index.html's hash is
  written again after that change.
- Deep links (e.g. /Gameplay) fall back to the Blazor router: 404.html is a copy of index.html.
- .nojekyll: serve _framework and other underscore folders as they are.
- audio/vo/packs.json: the narration downloads for offline play (tools/audio-packs.py).
"""
import base64
import hashlib
import os
import re
import shutil
import subprocess
import sys


def main() -> int:
    if len(sys.argv) != 3:
        print(__doc__, file=sys.stderr)
        return 2
    root, repo = os.path.abspath(sys.argv[1]), sys.argv[2].strip("/")

    index = os.path.join(root, "index.html")
    with open(index, encoding="utf-8") as f:
        html = f.read()
    html, n = re.subn(r'<base href="/" />', f'<base href="/{repo}/" />', html)
    if n != 1:
        print('index.html: <base href="/" /> not found', file=sys.stderr)
        return 1
    with open(index, "w", encoding="utf-8", newline="") as f:
        f.write(html)

    with open(index, "rb") as f:
        digest = "sha256-" + base64.b64encode(hashlib.sha256(f.read()).digest()).decode()
    assets = os.path.join(root, "service-worker-assets.js")
    with open(assets, encoding="utf-8") as f:
        text = f.read()
    text, n = re.subn(r'("hash":\s*")[^"]+(",\s*"url":\s*"index\.html")', lambda m: m.group(1) + digest + m.group(2), text)
    if n != 1:
        print("service-worker-assets.js: index.html entry not found", file=sys.stderr)
        return 1
    with open(assets, "w", encoding="utf-8", newline="") as f:
        f.write(text)

    shutil.copyfile(index, os.path.join(root, "404.html"))
    open(os.path.join(root, ".nojekyll"), "w").close()

    packs = os.path.join(os.path.dirname(os.path.abspath(__file__)), "audio-packs.py")
    return subprocess.call([sys.executable, packs, root])


if __name__ == "__main__":
    sys.exit(main())
