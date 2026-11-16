# Handoff archive

Finished work moved out of [`HANDOFF.md`](HANDOFF.md), newest first. Kept short: what was done and where it is documented.

## Table play tests (November 2026)

All three scenarios were played at the table by November 2026 (The Cost of Disease and Fear of the Unknown in
October, A Time of War last). Below: the handoff notes as they stood before those games. The A Time of War owner
questions are still open with defaults applied (see `HANDOFF.md`, Before v1.0).

### A Time of War (open list before the table test)

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

### Fear of the Unknown (open list before the table test)

Port and browser play test are done and live at https://vdepagter.github.io/myfathersworkapp/ (repository
`vdepagter/myfathersworkapp`, renamed from `vdepagter/MyFathersWork-FanMadeCompanionApp` on 30 Oct 2026). A PR to the
original repository (`Deusald/MyFathersWork-FanMadeCompanionApp`) can be opened from `main` at any time.

Next steps:
- Play a real game at the table (only crafted-save runs so far).
- Random sub-events not yet seen in the browser (they share code paths with tested ones): Liberal salts / Arise My
  Love / bribe / policy vote, Isolation smuggling trial outcomes, Foreigners Wall "Acceptance" + caravan visits,
  Creature cure accepted path, Gen I fates other than Creature, 2- and 4-player games.

## A Time of War port (October 2026)

Spec: [`docs/ATOW_PORT_MAP.md`](docs/ATOW_PORT_MAP.md) (one task per section 2.x). Status: **fully ported** - scaffold,
Gen I "Taking Sides", Gen II "Martial" and "Time Travel", Gen III "Heavy is the Crown", "Peace Through War", "Future
Consequences" and "You Cant Stop Progress", Scoring and all 8 endings (map 2.1-2.9), branch
`claude/port-a-time-of-war-132617`. No `// TODO ATOW` / `NotYetPorted` left; `GetHubMethod` maps all seven ATOW hubs. The status prefix was
removed from all menu descriptions (October 2026). Deviations: the "Ported" notes in map 2.1-2.8 and the ATOW
section in `CLAUDE.md`.

- Engine: `Shared/Scenarios/ATimeOfWar/` (files 00-09, `ATimeOfWarVars`), `ATimeOfWarHubId`, `WarFaction`,
  `MartialBuilding`, `PopUpIcon` S3 constants, the `GlobalData` cases, both ATOW CSVs, `mkSaveATOW` in
  `tools/test-save-helpers.js`.
- Browser-tested per branch (crafted saves at the end of the previous Generation, then every round transition): Gen I
  (2/3/4 players), Martial (both factions, every event chain), Time Travel (every Blame outcome), Reign and Peace (all
  decrees, votes / bids, statue, battles), Warning (all six world pages, time shifts to the stable timeline, old man's
  letter, Stasis), Paradox (both figure paths, the Time Machine vote with 0 / 1 / 2 releases, all eight random paradoxes,
  giant check), Scoring with score entry, tie-breakers (Masterwork, Estate Upgrades, family win) and rankings into all
  8 endings. 2, 3 and 4 players, undo on many screens incl. score entry, reload on the hub in every round, mirror-mode
  runs per branch without `Mirror: ... not visible` lines, no console errors. `tagcheck.py` (all three scenarios) 0
  missing, `sourcecheck.py ATimeOfWar ATimeOfWar` only lists justified items (unreachable passages, `;` / Harlowe `''`
  in labels, the Unity-only screens handled as popups).
- Independent browser play test from the main menu (no crafted saves, every screen clicked, hub explanation links
  visited): 3 players Martial (Separatists intro) -> Peace, ending 8; 2 players Martial (Monarchists intro) -> Reign,
  ending 6; 4 players Time Travel (town "Paradox") -> Paradox, ending 1; 2 players **mirror mode** Time Travel ->
  Warning, ending 4 (plus a mirror Martial -> Reign run from a Gen I save). Score entry, tie-breakers, rankings and
  endings (family and single winner) in each; undo across round and Generation changes, reload on hubs. No console
  errors; the only `Mirror: ... not visible` line is the documented `Quarter` slot of `RumorD3Revealed`. Fixed: undo
  from the first screen of a round reached straight from the end-of-round popup jumped back a round (engine,
  `GlobalData.SaveGame`, PLAYTEST_FINDINGS #7 - also affected Fear of the Unknown Witch Hunt); text "Monarchists's"
  -> "Monarchists'" (sabotage intros), "a Irresistible Aphrodisiac" -> "an". `tagcheck.py` now also checks computed
  content indices (`DYNAMIC_VALUES`, ternaries) and the shared `Gen3_*` hub sections.

## Endings Gallery (October 2026)

- Remembered players, win tally, endings trees; notes in `CLAUDE.md` "Endings Gallery". Design mockups:
  https://claude.ai/artifact/UHhtp9HGMdMVJmnYeSZo7L.
- Built clean (written without a .NET SDK). Fixed: the Endings tab crashed on the "Generation I/II/III" label
  (`FormatWithIndex` got depth + 1); a final ending with `Ending == 0` (crafted saves only) is no longer recorded.
- Browser-tested: empty gallery, Reveal / Show all / ticks (persist after reload, light the branch path); New Game hub
  records players and branch, "same table as last time" and name chips (taken names left out); crafted saves into a
  final ending for all three scenarios (single winner, family win, repeat ending = "Ending recorded", village name in
  the title); undo to score entry with a new winner replaces the record; export, text and file import after clearing,
  saved-game text rejected; forget player; Polish UI. tagcheck 0 missing. Re-tested on top of the persistent undo
  history: reloading on the ending keeps one record, undo to score entry with a new winner still replaces it.

## Cost of Disease playtest findings (October 2026)

- Findings, causes and a recurrence checklist in [`PLAYTEST_FINDINGS.md`](PLAYTEST_FINDINGS.md); new `tools/sourcecheck.py`.
- Fixed: hub text formatted with stale state (shared hub refresh in `RunScenarioAction`), Gen II end-of-generation
  text now in separate popups, truncated "South" hunt choice, mirror-mode questions show the screen heading.
- Same checks run against Fear of the Unknown: nothing to fix.

## Merge + small fixes (October 2026)

- Fear of the Unknown merged to the fork's `main` (`e18a535`).
- Letter dates move forward within a round; "Conducting Business" dated August to match its text.
- Rankings: equal players share a place unless a tie-breaker separated them (both scenarios).

## Fear of the Unknown - browser play test (October 2026)

- Played with crafted saves (`mkSaveFotU` in `tools/test-save-helpers.js`, auto-drivers `auto`/`autoFirst`): menu
  start, Gen I, Gen II Creature + Foreigners, Gen III Privatized (hunt, Junk Palace, Payment or Death, battle),
  Liberal (asylum assessment, program, taxes), Isolation (verification, smuggling, gorilla rampage), Tension
  (special event, crossroads), scoring with tie-breakers, endings, undo, reload, mirror mode Gen I -> Gen II.
  Cost of Disease regression: end of game + scoring OK. No console errors.
- Fixed: one-time events could be claimed repeatedly (Bloodsmith reward, crossroads, verification) - now once per
  player and the hub link disappears; letter dates after the last round used Gen III Early years; trailing blank
  lines in assembled popups (`TrimTrailingBreaks`); "The Family" as shared winner name; files normalized to CRLF.
- Scenario descriptions in the menu: Cost of Disease "PLAY TESTING", Fear of the Unknown "WORK IN PROGRESS".

## Fear of the Unknown - port (October 2026)

- Engine generalized for several scenarios: `ScenarioVars` base class, shared `FinalScoring`, scenario-aware undo,
  hub lookup, CSV loading, save (`ShouldSerialize*`) and mirror mode. See CLAUDE.md "Adding a scenario".
- All Fear of the Unknown passages ported (Gen I Witch Hunt, Gen II Foreigners/Creature/Obsessions, Gen III
  Isolation/Tension/Privatized/Liberal, battle, asylum assessment, endings) with both CSV files.
- New tools: `tools/cradle_dump.py`; `tools/tagcheck.py --scenario=`.
- Deliberate deviations: CLAUDE.md "Fear of the Unknown - notes and deliberate deviations".

## The Cost of Disease (September 2026)

- Complete port incl. Gen III, final scoring and 8 endings; mirror mode; phone layout. Deviations: CLAUDE.md
  "Cost of Disease - known deviations".
