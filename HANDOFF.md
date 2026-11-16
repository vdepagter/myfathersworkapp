# Handoff - active work

Only work in progress lives here. When a section is finished, move it to [`HANDOFF_ARCHIVE.md`](HANDOFF_ARCHIVE.md)
(newest first) and delete it from this file. Conventions are in [`CLAUDE.md`](CLAUDE.md).

## Before v1.0 (November 2026)

All three scenarios have been played at the table (November 2026) and are fully narrated by Oliver. Left before
version 1.0:
- Test on a real iPhone / iPad (Safari): autoplay unlock, MP3 playback, Oliver's sentence highlight and scrolling, the
  home-screen app.
- Code-sign the Windows exe (`MyFathersWorkApp/WindowsLauncher/`, `tools/build-windows.ps1`), so SmartScreen stops
  warning about an unknown publisher. This is the step before v1.0.
- Still open, defaults applied: the A Time of War owner questions in `docs/ATOW_PORT_MAP.md` section 7.
- To confirm on the owner's Android phone and tablet: the embers and the travelling spark of the reading highlight now
  show with the reduced-motion default (before 16 Nov only the glow showed there).

## Ideas, not planned

- Scenario picker: "5 of 8 endings" and locked silhouettes for endings not found yet.
- If GitHub Pages / LFS limits bite: the narration clips are ~340 MB; 64 kbps mono would cut that by a third.

## Reference

- Version: `0.66.6.<revision>`, revision = first-parent commits on main since 3d78856 minus 1 (deploy workflow).
- ElevenLabs: the API key is the user environment variable `ELEVENLABS_API_KEY` (the owner's upgraded Creator account,
  which holds the Oliver library voice; a running shell keeps the old value: read it with
  `[Environment]::GetEnvironmentVariable('ELEVENLABS_API_KEY','User')`). A 403 "ip_not_allowed" means the key's IP
  allowlist no longer matches the PC's public IP. Oliver in a new checkout (git-ignored `voices.local.json`):
  `python tools/narration/elevenlabs.py add oliver 503ef2e72fb506b3879737bc64638b2d48597374bf1247925feea5d30965f3bd
  jfIS2w2yJi0grJZPyEsk "Oliver Silk - Deep Gravel Narrative"`.
- Narration masters (192 kbps) are in the git-ignored `tools/narration/out/master/oliver/` of the main checkout - keep
  them.
- The visual overhaul (Unity look), audio and narration history, with design notes still referred to from code
  comments, are in the archive.
