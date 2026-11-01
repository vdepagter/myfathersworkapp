# Handoff - active work

Only work in progress lives here. When a section is finished, move it to [`HANDOFF_ARCHIVE.md`](HANDOFF_ARCHIVE.md)
(newest first) and delete it from this file. Conventions are in [`CLAUDE.md`](CLAUDE.md).

## Visual overhaul (October 2026)

Bringing the Unity app's look into the web app. Plan (owner approved 30 Oct 2026, original fonts):
https://claude.ai/artifact/KaN3ACVp4fjv5jn4duurP8 , live preview of every effect: https://claude.ai/artifact/GPLQg5ChvkKijdTWoNsyQU .
No libraries: CSS animations plus `wwwroot/js/atmosphere.js` (canvas particles with the original textures, tap
sparkle, lightning, Graphics quality in localStorage `mfwGraphics`, `html[data-graphics]`). Shared look in
`wwwroot/css/theme.css` (fonts, `.mfw-button` textured buttons, page background). Art: resized WebP copies of
`UnityOriginalApp/Assets` in `wwwroot/images/ui/` (WebP and WOFF2 are not LFS, so the cloud can push them). Fonts in
`wwwroot/fonts/`: Evil Dead (headings), Absolute Empire (buttons), Averia (text, OFL); the first two are free for personal use only.

- Phase 1, Main Menu (`Pages/Home.razor` + `.css`): the Unity scene rebuilt from its RectTransforms (canvas 1920 x 1082,
  `Layer` records in %): sky, mountain, village with flickering lights, the big manor with the trees in front of it (Unity
  tints them grey 0.557), rain, fog, smoke, embers, vignette, blue glow, lightning, the Main_Border frame fixed to the
  window, the bobbing logo, the red help "?" (opens Help) and a 2 x 3 grid of textured buttons (Continue, New Game,
  Saved Games, Load Game, Settings, Endings). Tap sparkle everywhere. Phones in portrait stack logo and buttons.
- Phase 2:
  - Settings (`Shared/SettingsPopup.razor`, Main Menu): `Shared/DisplaySettings.razor` (Graphics High/Medium/Low,
    story text size 0.85-1.6 in localStorage `mfwTextSize` -> CSS `--mfw-story-scale`), voice, language, volumes. The
    pause menu (`Shared/PauseMenu.razor`, dark Settings Panel) shows the same DisplaySettings. Default voice: Oliver.
  - Game pages (`Pages/Gameplay.razor` + `.css`) as the Unity views: a story window (or a popup over it) is the
    GanrationEnding view - the scenario illustration (`story-bg-cod/fotu/atow.webp`, Unity GenerationEnding/BGs 1-3),
    blue glow, the Wide_Textbox parchment with black Averia text, red links (`.story-link`), the gold frame
    (MFW_Border_Gold2); a hub is the GanrationHUb view - dark metal (Main_BG), warm glow, embers and smoke
    (`mfwAtmosphere.start("game-fx")`), white text in light panels, light blue links, the steel frame (MFW_Border_1).
    Titles in Evil Dead. `--u` (css/theme.css) is one Unity canvas pixel; portrait phones use a bigger unit.
  - The frames' round slot was cut out of the frame images (`frame-*.webp` are 9-slice, slice 140) into
    `round-tray-*.webp`; the round bar (`round-bar.webp`, Unity MFW_ProgressBar) shows through its nine windows, dark
    after the current round, which glows. Cell positions: `_RoundCells` in Gameplay.razor.
  - Round icon buttons (Back_BTN, mirrored for redo, PauseBTN, BookIcon) top left; sound toggles top right.
  - Dialogs on parchment (`.mfw-parchment`): setup / special rule popups drop from the top, end of round / Generation
    and input popups zoom in (`js/transitions.js` ENTRANCES, keyframes in css/styles.css), a new story page rises from
    the bottom; Graphics Low and reduced motion fade instead.
  - Final scores on one page (`FinalScoring.ScoreEntry`, `GameplayInputPopup.Fields` / `OnCloseFields`, tag
    `ScoreEntry_Player`), first field focused, Enter moves to the next field. Every input popup focuses its field.
- Feedback round (30 Oct): wider parchment padding, darker and slightly heavier ink (`.mfw-ink`, text stroke 0.35px)
  on story text and dialogs, darker links (#7a170b); square icons no longer clipped to circles; help "?" re-cropped.
  Sound toggles (`Shared/AudioToggles.razor`): Effects, Music and a Narration toggle (off / last voice), each with a
  vertical volume slider on hover or focus. Narration progress bar under the title (`.narration-progress`, drawn and
  seeked in `js/audio.js` across the clip list, 10 s skip buttons, arrow keys 5 s). The church bell is the original
  welcome sound; it now also plays on the first tap when the browser blocked autoplay.
- Phase 3:
  - SPECIAL EVENT banner (Unity ViewSpecialEvent, `special-event-*.webp`) fades over every newly shown window with
    `SoundCue.SpecialEvent` (not on load/undo).
  - Vote / bid countdown (`Shared/RevealPanel.razor`, Unity ViewBiddingSystem): a link "start the vote/bid" opens it
    and Accept follows the link; other SpecialEvent windows that mention a vote or bid get a Start Voting/Bidding button
    above their links (Accept just closes). 3, 2, 1 (1 s each), REVEAL!, Accept. Unity's bid sounds are not ported
    (mp3 = LFS); REVEAL plays the special event sound.
  - Final ending: Next opens the ending page ("Ending unlocked!" / "Ending reached", "You have unlocked X out of 8
    endings", eight pips, Endings Gallery, Return to Main Menu). Replaces the inline gallery card.
  - Optional, not done: "5 of 8" on the scenario picker, locked silhouettes, achievements.
- Menu pages (31 Oct): every page before a game (`Players*`), Saved Games, Help and the Endings Gallery sit in
  `Shared/MenuPage.razor` (dark metal Main_BG, glow and embers, the steel frame, the round back button, Evil Dead
  title); shared content classes in `wwwroot/css/menu-pages.css` (`.mp-parchment`, `.mp-input` = Unity Textbox,
  `.mp-chip` small buttons and tabs, `.mp-button`, `.mp-popup`). Unity art: player count icons (`players-*.webp`),
  scenario cards (`scenario-*.webp`) in the steel / gold Scenario borders, the PlayerIntro paper strip for the
  welcome. Load Game left the Main Menu (5 buttons, Settings centred on the last row): loading a save code is the
  "Load a save code" section of Saved Games (`LoadGamePopup` removed). Vote / bid panel sounds from Unity
  ViewBiddingSystem: `sfx-bid-open` (Set up-window appear) when it opens, `sfx-reveal` (Reveal) under the countdown.
- Mobile feedback (1 Nov): screen transitions use Unity's ink-blot flip-book (`images/ui/transition-ink.webp`, the 22
  `ScreenTransitions` frames as one sprite sheet, `steps(22)`; menu page navigation recedes it too). Dialogs and menu
  papers zoom from 0.62 and ease into place in 1.4 s (no slow "breathe"); the pause menu and Settings only fade. No
  pull-to-refresh (`overscroll-behavior-y`). Frames carry a 100vmax dark shadow so a Firefox Android toolbar change that
  leaves a fixed layer short shows dark instead of page content; the round bar sits in a fixed `inset: 0` dock like the frame (was `bottom: 0` + transform, which floated below the lifted frame on Firefox, drawn white). Absolute
  Empire has ascent/descent overrides (capitals centred in the line box in every browser) and `.mfw-button` lifts its
  label in em (was % of the container width). Countdown / REVEAL! sized in cqw of the plaque. Village names are
  remembered in the gallery (`GalleryData.Villages`, prefilled + chips). Log Book on Unity's open book (3 + 3 events,
  arrows, red X); narrow windows get one 9-slice page. Main Menu buttons at 82 % with a larger fan-made note below.
  Not verified on a real Firefox Android phone (no device here).

## A Time of War - table play test (October 2026)

Fully ported and played through in the browser (details in the archive). Spec: [`docs/ATOW_PORT_MAP.md`](docs/ATOW_PORT_MAP.md).

Next steps:
- Table play test.
- Owner questions (defaults applied, map section 7): B1 GunsAnnouncement text, B4 TowardsWar wording, B9 figure-token
  holder may not exist, B10 duel death odds, B14 first-visit text, B16 2-player CoW text, B17 Church costs "X/X/X",
  B19 random paradox only after a release (fixed), B21 University "pay $1" in Warning (fixed); endings 1/5 print
  "The family IV" on a shared win (as the source), ending 8 "the The family name" / "Dr. The family" and ending 6 "I, The family" likewise; the Drifters decree popup starts with a dangling "Then,"
  when the Crown does not move (source `{$crownL} Then, ...`).
- Not play-tested from the menu: the Martial duel / bribe-success / tea-yay sub-chains, Peace gas release, Warning
  stable timeline + Stasis, Paradox figure path with two Wooden Figures (covered by the per-branch crafted-save runs).

## Fear of the Unknown - table play test (October 2026)

Port and browser play test are done and live at https://vdepagter.github.io/myfathersworkapp/ (repository
`vdepagter/myfathersworkapp`, renamed from `vdepagter/MyFathersWork-FanMadeCompanionApp` on 30 Oct 2026). A PR to the
original repository (`Deusald/MyFathersWork-FanMadeCompanionApp`) can be opened from `main` at any time.

Next steps:
- Play a real game at the table (only crafted-save runs so far).
- Random sub-events not yet seen in the browser (they share code paths with tested ones): Liberal salts / Arise My
  Love / bribe / policy vote, Isolation smuggling trial outcomes, Foreigners Wall "Acceptance" + caravan visits,
  Creature cure accepted path, Gen I fates other than Creature, 2- and 4-player games.

## Music, sound effects, voice-over and screen transitions (October 2026)

Implemented and deployed; mapping from the Unity app in `docs/AUDIO_TRANSITIONS_MAP.md`.
`wwwroot/audio/*.mp3` (12 clips, ~9.3 MB, LFS), `wwwroot/js/audio.js` + `Shared/AudioService.cs`, `wwwroot/js/transitions.js`,
toggles in `Shared/AudioToggles.razor`. Sounds and transitions are derived in `Pages/Gameplay.razor` (`DetectScreenChange`);
scenario code only sets `GameplayWindow.SetCue` / `MarkFinalEnding` - never call audio from scenario code (mirror sandbox, undo, load).

Next steps:
- Test on a real iPhone/iPad (Safari autoplay unlock, MP3 playback) and at the table (volume of the effects vs music).
- Voice-over: 17 narrated passages in Cost of Disease / Fear of the Unknown (map section 8), Female/Male/Oliver/Off (default Oliver since 30 Oct 2026; Unity defaulted to Male), "Narrated" pill on those windows.
  Uncertain: the female `GloomyWolvesIntro` clip lives in Unity's male folder (judged female by pitch). Recordings follow the
  English source text, so other text versions differ from the narration.
- A Time of War VO: 11 narrated passages wired (map 8.3, `ParadoxIntro` reuses the `WarningIntro` clip as in Unity).
- AI narration: see the section below.
- Not ported (deliberate): Unity bidding/reveal screen, clips the shipped app never played (map 2.3).
- Version: `0.66.6.<revision>`, revision = first-parent commits on main since 3d78856 minus 1 (deploy workflow).

## AI narration - ElevenLabs (October 2026)

Rules and commands: CLAUDE.md "AI narration", `docs/AUDIO_TRANSITIONS_MAP.md` 8.4, docstrings of `tools/narration/chunks.py`
and `elevenlabs.py`.

State (27 Oct 2026, branch `claude/oliver-v4-narration`, merged and pushed; deployed and live, first deploy with the
LFS cache step succeeded - cache ~158 MB):
- Voice setting "Oliver" (was "Oliver (AI, whole story)") next to Female / Male / Off. Male and Female play only the original recordings
  (the 7 v3 intro clips in `wwwroot/audio/vo/ai/` are gone). Oliver plays chunk clips per window, falls back to the
  original male recording where a window has none, and never autoplays private screens (Play only).
- Model `eleven_v4`, stability 0.8, similarity 0.8 (user's choice). Verified: v4 accepts but ignores style, speed and
  speaker boost (model metadata `can_use_style` / `can_use_speaker_boost` false; speed 0.7 gave the same clip length);
  it does not read audio tags aloud (speech-to-text check); no clipped endings.
- Fear of the Unknown: **fully rendered** - 509 clips, 124k characters sent, ~58k billed (v4 bills ~0.45 per character
  sent), 94 MB at 96 kbps mono (`wwwroot/audio/vo/oliver/`, LFS). Browser walk-throughs (Witch Hunt, Foreigners, Creature,
  all four Gen III branches, endings) found every window narrated except `skip.txt`; private screens offer Play only.
- Stand-ins for values nobody can render (user's choice, 27 Oct): player names -> "one of the cousins" / "a cousin" /
  "a cousin's" / "two of the cousins", village -> "the village" ("the town of <village>" -> "the town"), newspaper ->
  "the local newspaper", warrior / program names -> "the warrior" / "the program" (`chunks.STAND_INS`). Random dates
  are skipped. Headers: "A letter." etc. "$<amount>" -> "<amount> dollars" (amount is a value clip, 1-20).
- Quota after Fear of the Unknown: ~57.8k of 126.6k left (resets 16 Nov 2026).
- ElevenLabs Creator: 126,634 characters per month, resets 16 Nov 2026. API key in `ELEVENLABS_API_KEY`; the key has an
  IP allowlist (a 403 "ip_not_allowed" means the PC's public IP changed). Oliver: `python tools/narration/elevenlabs.py add
  oliver 503ef2e72fb506b3879737bc64638b2d48597374bf1247925feea5d30965f3bd jfIS2w2yJi0grJZPyEsk "Oliver Silk - Deep Gravel
  Narrative"` in a new checkout (git-ignored `voices.local.json`).
- Masters (192 kbps) are in the git-ignored `tools/narration/out/master/oliver/` of the main checkout - keep them.
  Older comparison renders: `.claude/worktrees/elevenlabs-voice-narration-3a3e64/tools/narration/out/`.

Play-test fixes (28 Oct 2026, branch `claude/playtest-feedback-fotu`): `chunks.py` drops "of" in "the village of <village>",
says "My dear cousin" for "My dear <name>", drops the generation numeral after a skipped name ("<name> II"), and lists every
`<Method>_Content<n>` when the index is computed (`1 + vars.ProgramType`); `CureDenied` is private; `Narration.cs` finds
a static chunk that also occurs inside the previous value (`PatientEntry`: " " between two either/or sentences).
Narration logic check (29 Oct): choices and rules lines are no longer narrated (`auto_cut`, `cuts.txt`), headers fixed
("Correspondence from <name>" -> "Correspondence.", "Anonymous Editorial", "Excerpt", "Case File"), "the 1800's" ->
"the eighteen hundreds", "<b><name></b> must suffer" -> "One of the cousins must suffer", "due to the <program>
Program" -> "due to the Program". Titles / headers per scenario (CLAUDE.md). Not changed: "Vlad III" is sent as written
(speech-to-text: v4 says "Vlad the Third"). **Rendered 29 Oct** (37 clips, 9,979 characters, 4,189 billed; ~51.7k left
until 16 Nov). Flagged "ends mid-sound", to listen to: Gen1FateVictim_Content1 / _Content2 and Gen1FateMob_Content2
(chunk ending "My dear cousin"), FinalEnding_Content8 (chunk ending "due to the" before "Program"). Merged to main.
For the next scenarios: review the `_Title` texts with placeholders (Cost of Disease has dates and names in titles,
e.g. "Upon a Secret Meeting - Letter - <date>") before rendering; Cost of Disease is now ~157k characters sent
(titles on, headers off), more than one month of quota.

Next steps:
1. The Cost of Disease (~126k characters sent, ~57k billed) and A Time of War (~106k, ~48k billed); private sections of
   each last. Cost of Disease fits roughly in the remaining quota; A Time of War after the reset. Both need a `values.txt`,
   `private.txt` and `skip.txt` like Fear of the Unknown (review `chunks.py plan <Scenario> --list` and the chunk starts).
2. Listen-through at the table: joins around skipped names, the stand-ins, the private Play button. Size: the three
   scenarios will be ~300 MB of clips; 64 kbps mono would cut that by a third if Pages/LFS limits bite.
3. Play-test the other scenarios with Oliver once rendered: console lines "Oliver narration: no clips for <Tag>" show
   windows whose text code changes beyond placeholders (not narrated); fix the split or accept.
4. User play-test of Fear of the Unknown with Oliver first; the other scenarios start in a new session.
