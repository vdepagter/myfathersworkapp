# My Father's Work - Fan Made Companion App

Blazor WebAssembly (.NET) re-implementation of the official *My Father's Work* companion app (a Unity app whose
stories were written in Twine/Harlowe and compiled to C# by Cradle). Players make choices on a phone/tablet next to the
board game; the app tells them what to set up and narrates the story over 3 Generations x 3 rounds.

## Status (October 2026)

**Active work, open tasks and next steps are in [`HANDOFF.md`](HANDOFF.md) - read it first and keep it current.**
When a piece of work is finished, move its section to [`HANDOFF_ARCHIVE.md`](HANDOFF_ARCHIVE.md) so the handoff stays short.

| Scenario | Web app | Notes |
|---|---|---|
| The Cost of Disease (`ScenarioId.CostOfDisease`) | **Complete, play testing** - Gen I, II, III, final scoring, all 8 endings | Only English text. |
| Fear of the Unknown (`FearOfUnknown`) | **Ported, browser play test done** - all branches, battle, asylum assessment, scoring, 8 endings | Waiting for a table play test. See the section below. |
| A Time of War (`TimeOfWar`) | **Fully ported, browser play test per branch done** - Gen I Taking Sides, Gen II Martial / Time Travel, Gen III Heavy is the Crown / Peace Through War / Future Consequences / You Cant Stop Progress, scoring, all 8 endings | Played through from the menu in the browser (all four Gen III branches); waiting for a table play test. Spec: `docs/ATOW_PORT_MAP.md`. See the section below. |

## Repository layout

- `MyFathersWorkApp/MyFathersWorkApp/` - the web app.
  - `Pages/` - menu/setup flow (`Home`, `PlayersSelect` ... `PlayersScenarioLanguage`) and `Gameplay.razor` (renders the active window / hub / popup).
  - `Shared/GlobalData.cs` - all game state (serialized to JSON for save/load/undo), localization lookup, undo stack, end-of-round popup.
  - `Shared/Scenarios/Gameplay*.cs` - UI model: `GameplayWindow`, `GameplayHub` + `GameplayHubSection`, `GameplayPopup`, `GameplayInputPopup`.
  - `Shared/Scenarios/ScenarioVars.cs` - abstract base of every scenario Vars class: `RandomArray` + draw helpers, flavor randomness (`Flavor`, `FlavorElement`), final scoring state. `Shared/Scenarios/FinalScoring.cs` - shared score entry, tie-breakers and rankings (tags live in each scenario's CSV).
  - `Shared/Scenarios/TheCostOfDisease/` - one `static partial class TheCostOfDisease`, split per generation/branch (`00_Setup`, `01_Gen1_Fever`, ..., `08_Scoring`) + `TheCostOfDiseaseVars.cs` (scenario state).
  - `Shared/Scenarios/FearOfTheUnknown/` - `static partial class FearOfTheUnknown` (`00_Setup`, `01_Gen1_WitchHunt`, `02_Gen2_Obsessions`, `03_Gen2_Foreigners`, `04_Gen2_Creature`, `05_Gen3_Shared`, `06_Isolation`, `07_Tension`, `08_Privatized`, `09_Liberal`) + `FearOfTheUnknownVars.cs`. Hub ids: `Shared/Enums/FearOfTheUnknownHubId.cs`.
  - `wwwroot/localization/` - `UI_Localization.csv` (English; the Polish menus from the earlier fan port were removed in October 2026, and the language picker only shows when the CSV has a second language column), per scenario `<Scenario>_Localization.csv` (story text) and `<Scenario>_Gameplay_Localization.csv` (rules/setup text).
  - `wwwroot/images/setup/` (popup images, referenced by `Shared/Consts/PopUpIcon.cs`) and `wwwroot/images/gameplay/` (inline `<icon=...>`). S2_*/S3_* images for the other scenarios already exist. Images are **Git LFS** - a worktree without `git lfs pull` shows broken icons.
- `source/` - the decompiled Cradle story classes of the official app for all three scenarios (`TheCostofDisease.txt`, `FearOfTheUnknown.txt`, `ATimeOfWar.txt`; the only source for Fear of the Unknown and A Time of War). Read them with `tools/cradle_dump.py`. (The old console port `MyFathersWorkConsole/` and `ProcessingScenario/` were removed on 30 Oct 2026; they are in git history.)
- `MyFathersWorkApp/LocalizationMerger/` - console tool that merges a translated column from a spreadsheet export into a localization CSV.
- `tools/` - `cradle_dump.py` (decompiled Cradle `.txt` -> readable pseudo-Twine with link labels; `--graph <Passage>` BFS from a passage, `--html` CSV-ready text - the preferred way to read a scenario), `tagcheck.py` (verify localization tags; `--scenario=FearOfTheUnknown`), `sourcecheck.py` (source link labels missing from the CSVs, Unity-only end-of-generation/setup screens), `test-save-helpers.js` (play-test from crafted saves). `narration/` - AI narration (ElevenLabs): `chunks.py` (Oliver clips per text chunk: plan, render), `elevenlabs.py` (voices, comparison renders, `check`), overrides/values/private lists in `narration/scripts/<Scenario>/`, `local_tts.py` (open models).
- `PLAYTEST_FINDINGS.md` - findings from real play-tests, their causes and a checklist against recurrence. Read it before porting or reviewing a scenario.
- `UnityOriginalApp/` - the original Unity project is also in the repo (same content as the path below).
- Original Unity project (outside the repo, owner's machine): `C:\Users\ValentijndePagter\Downloads\my-fathers-work-master-4-20260622T163118Z-3-001\my-fathers-work-master-4`. Useful parts: `Assets/Scripts/OldStory/TheCostofDisease*.cs` (Cradle output, Cost of Disease only), `Assets/Scripts/UI/View*.cs` (score entry, tie breaker, ranking, endings UI flow), `Assets/Resources/setupImages`.

## How a scenario is written (conventions - follow them exactly)

Every screen is a `private static void X(GlobalData globalData)` method in the scenario's partial class:

```csharp
private static void Changes(GlobalData globalData)
{
    globalData.SaveToUndo();                               // FIRST, before any state change
    globalData.ActiveWindow = new GameplayWindow(globalData);
    globalData.ActiveWindow.AddDefaultTitle();             // tag "Changes_Title"
    globalData.ActiveWindow.AddDefaultContent();           // tag "Changes_Content"
    globalData.ActiveWindow.AddClickHereToContinue(Changes_0);
}

private static void Changes_0(GlobalData globalData)       // setup popup
{
    globalData.SaveToUndo();
    globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_LetterBACK, PopUpButton.Confirm, Diseases1); // tag "Changes_0_Content"
}
```

- **Localization tags come from `[CallerMemberName]`**: `Method_Title`, `Method_SubTitle`, `Method_Content`, `Method_Content{n}` (`AddNextContent(n)` / `AddNextContentWithLinks(n, [...])`), popups `Method_Content`. `AddDefaultBaseTitle` uses the part before the first `_`. Hubs: `Hub_Title`, `Hub_{Section}_Title/_Content/_Content{n}/_ClickHere`. Calls inside lambdas still use the enclosing method name. Use `AddCustomContent(string)` for text built in code.
- **Undo** (`GlobalData.SaveToUndo`) finds the caller by name via reflection on the scenario class (`GlobalData.ScenarioType`), then `FinalScoring`: every method that calls it must be unique by name, `static`, and take exactly `(GlobalData)`. Call it before mutating state; methods that only route (no screen) must not call it. Undo restores the JSON snapshot, rebuilds the hub (`GetHubMethod`) and re-invokes the method. Hubs become undo points in `GlobalData.SaveGame` (a hub re-shown in the same round with no undoable screen in between is not stored twice; the next round's hub shown straight after the end-of-round popup is a new point, `PLAYTEST_FINDINGS.md` #7). The whole undo stack is kept in the browser after every screen (`Shared/GlobalData.Persistence.cs`: `continue` = newest point, `undoMeta` + `undo/{i}` = history, RandomArray stored only when it changes, oldest points dropped when storage is full); load reopens the exact screen through `ResumeMethod` and restores the history. Persistent data that must outlive a game (e.g. an endings gallery) needs its own localStorage keys: New Game clears these. **Saved Games** (`Shared/SavedGames.cs`, `wwwroot/js/saves.js`, `Pages/SavedGames.razor`): New Game and loading a save code first pack those keys (+ `continueTime`) into one gzipped key `savedGame/<Scenario>-<FlavorSeed>`, listed in `savedGames`; opening one packs the current game away and unpacks the chosen one into the active keys, history included.
- **Hubs** (`GameplayHub`) are the per-generation round screens. One hub method per branch (e.g. `GloomyGothic`), sections switched by `ReplaceShouldShow(() => globalData.Years ...)` plus state flags. Set `HubId` and register the hub in `GlobalData.GetHubMethod()` (used by load and undo). End the round with a section calling `globalData.ShowEndOfRoundPopUp(nextEvent)`; the last round uses `AddEndOfGenerationSection`. Hub text is formatted when the hub is built; `RunScenarioAction` rebuilds the shown hub after every action that ends with no window/popup open (`RefreshShownHub`), so hub methods must have no side effects. Ending a chain with the hub method instead of `_ => { }` is still fine.
- **`ShowEndOfRoundPopUp` advances `Years` (and `Generation` after Late) *before* calling the next event** - events after the Early round run with `Years == Middle`. At the generation change `ActiveHub` becomes null, so a callback of `_ => { }` then renders a **blank page** (this was the old "blank after Gen II" bug).
- **Randomness** must use the pre-rolled `ScenarioVars.RandomArray` (`RandomElement`, `RandomBool`, `RandomInRange`) with a fixed index per decision so undo/load replay identically. Never read `RandomArray` directly. Indices in use for Cost of Disease: 0-58 (Gen I/II) and 59-203 (`_RND_*` constants in the Gen III files). Fear of the Unknown: `_RND_*` constants in `00_FearOfTheUnknown_Setup.cs` (0-164, battle 400-899). For events that can repeat, use `base + min(counter, 9)`. Shuffles are drawn one position at a time from the remaining items (`HunterAt`, `HuntRewardAt`).
- **Flavor randomness** (who writes a letter, dates, alternative sentences that change nothing in the game) uses `vars.Flavor(key, count)` / `FlavorElement` - a hash of `FlavorSeed` and a unique key. Deterministic for undo/load and never asked in mirror mode. Anything players act on (setup, VP, money, branches) must use `RandomArray`.
- **Mirror mode** ("Play along with the official app" on the scenario language screen, `GlobalData.MirrorMode`, `Shared/GlobalData.Mirror.cs`, `Shared/MirrorPopup.razor`): `RandomArray` starts at -1 (not drawn). The first draw of an unanswered slot throws `MirrorQuestionException`; `GlobalData.RunScenarioAction` restores the state, replays the action in a sandbox once per value, shows the players the text that differs, and reruns the action with their answer. Rules for scenario code:
  - Every entry into scenario code (links, popup buttons, undo, load, start) goes through `RunScenarioAction`.
  - **Draw a random value on the screen where it becomes visible**, not earlier. Otherwise mirror mode asks before the official app shows it ("not visible on the current screen" in the console, shown as a guess-or-roll question). Examples: `Gen2Building`, `HuntVp`, `DrawFeverReward`, `MasterworkCost`, `DrawAffiliations` (which faction is evil is drawn on first use), the Symposium target.
  - A random next screen is chosen inside the callback: `data => vars.RandomElement([A, B], i).Invoke(data)`, not `AddClickHereToContinue(RandomBool(i) ? A : B)`.
  - Hub sections are built even while hidden - don't draw in a hidden section's text (guard with the section's condition, see `SuspicionBuilding`).
  - Answers live in `RandomArray`, so undo past the screen that asked forgets them and asks again (that is how a wrong answer is corrected).
- **State** lives in the scenario Vars class (derives from `ScenarioVars`; only the active scenario's Vars are saved, via `GlobalData.ShouldSerialize*`). Use arrays, not `List<T>` (Newtonsoft `ObjectCreationHandling.Reuse` appends to lists on load/undo). Add every field to `Reset()`. Per-player dictionaries are keyed by player name and filled in `Reset()`. `GlobalData.TmpValues` is cleared whenever a hub is displayed - only use it inside one event chain.
- **Placeholders** in CSV text (`Shared/Extensions.cs`):
  - `{{0=...|Oliver|James|...}}` + `FormatWithReplacement(0, value)` (the text after `=` is only a preview),
  - `{{1=|option0|option1|...}}` + `FormatWithIndex(1, index)` (enums starting at `None = 0` use `{{1=| |First|Second}}`),
  - `{{2=?|if true|if false}}` + `FormatWithCondition(2, () => ...)`.
  - The regex is `\{\{(id=[^}]+)\}\}`: option text cannot contain `|` or `}`. Nested placeholders work only if the inner one is formatted first.
  - Player-count words: `{{0=|two|three|four}}` with `LocalizedPlayerNumberIndex()`.
  - Links: `<link=0>text</link>` with `AddNextContentWithLinks` (link texts in one string must be unique).
- **CSV rules**: `Tag;Text` per line, `;` is the separator, so source semicolons become ` - `. Lines starting with `//` are comments. Duplicate tags throw at load. Trailing spaces are significant when a link follows inline (`... they may ` + `[click here]`) - some editors strip them. HTML is allowed (`<b>`, `<i>`, `<br />`, `<icon=Name>`, `<sprite=Name>`). Story text goes to `*_Localization.csv`, rules/setup text to `*_Gameplay_Localization.csv`. Both are searched.
- UI strings shared by all scenarios are in `UI_Localization.csv` + `Shared/Consts/GlobalTags.cs` (English only).

## Porting workflow (what worked for Cost of Disease Gen III and Fear of the Unknown)

1. Produce readable passages: `python tools/cradle_dump.py <scenario .txt>` (add `--html` for CSV-ready text).
2. Build the passage graph from the scenario start / generation intros (`cradle_dump.py --graph <Passage>`) to know what belongs to each generation.
3. Map Twine concepts: a passage with text + `<hook>` "Click to continue" + `setupStyleEvnt` block -> window + setup popup. `$PrintEndOfTheRoundText(tag, next)` -> hub end-of-round section. `_SetupImage` -> `PopUpIcon`. A bold "SPECIAL RULE" block inside story text or a setup -> its own `PopUpTitle.SpecialRule` popup after it (story CSV text stays story only). `either(...)`/`GetRandom` -> `RandomArray` index. `$nameA..E` / `allyA..E` ifs -> loops over `GetActivePlayers()` (the web app supports 2-4 players, drop 5-player branches). `(display:)` / abort chains -> direct method calls.
4. Write code, then the CSV rows, then run `python tools/tagcheck.py` (0 missing) and `python tools/sourcecheck.py <Scenario> <CsvPrefix>`, and play-test. Go through the checklist in `PLAYTEST_FINDINGS.md`.
5. Keep source bugs visible: several Twine passages have dead or always-true conditions. Decide deliberately and note it (see Cost of Disease notes below).

## Adding a scenario (A Time of War) - checklist

The engine was generalized while porting Fear of the Unknown. A new scenario needs:

- A Vars class deriving from `ScenarioVars(randomCount)` whose `Reset(GlobalData)` calls `ResetBase`; a property on `GlobalData` with `ShouldSerialize*`, and cases in `GlobalData.ScenarioVars`, `ScenarioType`, `ResetScenarioVars`, `StartScenario`, `ShowFinalEnding`, `GetHubMethod` and `LoadScenarioLanguagesOptionsAsync` (CSV paths; unknown scenarios navigate home).
- A hub id enum in `Shared/Enums/`, mapped in `GetHubMethod`.
- After the last generation link to `FinalScoring.Start`; provide `public static void FinalEnding(GlobalData)` and the shared scoring tags (`ScoreEntry_Content`, `TieBreaker1_*`, `TieBreaker2_Content`, `Rankings_*`) in the scenario CSV.
- A `tools/tagcheck.py --scenario=` entry and `test-save-helpers.js` support. Run `tools/sourcecheck.py` and the `PLAYTEST_FINDINGS.md` checklist.
- `Shared/Enums/Generation.cs` has 3 values; after the last round `Generation` becomes 3 (guard like `EndedYears()` in Fear of the Unknown).

Source size: `ATimeOfWar.txt` 297 passages, English. It has time-travel/paradox mechanics - look at the Unity `View*` scripts (`ViewBiddingSystem`, etc.) for UI the Twine text relies on.

## A Time of War - notes and deliberate deviations

Details per branch: the "Ported" notes in `docs/ATOW_PORT_MAP.md` 2.1-2.8; source bugs and owner questions: map section 7.

- Branches: Gen I Taking Sides -> Gen II Martial (Monarchists / Separatists intro) or Time Travel -> Gen III Reign or Peace
  (after Martial, by the war winner), Warning or Paradox (after Time Travel). Endings: Paradox 1/2, Warning 3/4, Reign 5/6,
  Peace 7/8 (`Vars.Ending`).
- Source `setupStyle` blocks that Unity re-showed on every hub render are setup popups shown once at round entry; their
  guard flags (`$pg13`, `$gen3pg`, `$pgGen3`, `$pea1-3`, ...) are dropped. Hub setup text that depends on a draw (world
  page, weapon income, ...) is drawn on that popup.
- Random values are drawn where they become visible: `$war` / `$quarter` lazily (Gen I), Martial incentives in the shown
  hub section, `$decbon` on the CoW vote, world pages on the page popup, `$timemistake` on `ParadoxTimeRandom`, the next
  random passage in the click/close callback. Hidden thresholds (`$commtrigger`, `$total`) are only drawn when the counts
  do not decide them already. Once-per-game "sticky" passages use one fixed index.
- Fixed source bugs: B5-B7 (afternoon tea sabotage pages / state), B14 (Royal Advisor first-visit text), B16 (2-player
  CoW left/right), B18 (decree tie rewards nobody), B19 (random paradox only after a release), B20 (Paradox3b end-of-round
  popup), B21 (University "pay $1" in Warning), B29 (town-name override applied in every case, "debug" text dropped).
- Kept as shipped: B1 (GunsAnnouncement unreachable), B4 (TowardsWar text), B8, B9 (figure-token holder may not exist),
  B10 (duel death on 1 of 5), B11, B17 (Church "X/X/X"), B22, B24, B30 (Time Machine -5VP only on the Time Travel path).
- Regal titles (Reign7c) are shown via `DisplayName` in later story text; player names, rankings and endings use the plain
  name. Endings use "The family" on a shared win (endings 1/5 then read "The family IV", as the source).
- Martial end of round: Special Setup popup first, events with `Years` unchanged, then the end-of-round popup. Time Travel
  end of Generation: one End of Generation popup, then `TimeEnd1` / `TimeEnd2`.
- Warning: `$search + 1` of the Middle end link is applied in `TimeWarp2`; the stable-timeline page 16 popup is shown after
  `WarningEvent2` and at the start of the Late Years while the Masterwork is open.
- Small text fixes: source `;` written as " - ", "When the the round" -> "When the round", doubled line breaks around
  optional paragraphs reduced to one, "{faction}'s noble cause" -> "{faction}' noble cause" (sabotage intros), "a
  Irresistible Aphrodisiac" -> "an" (Royal Power). 5-player branches dropped.

## Fear of the Unknown - notes and deliberate deviations

- Branches: Gen I Witch Hunt -> Gen II Foreigners or Creature (+ Obsessions) -> Gen III Isolation / Tension (after Foreigners) or Privatized / Liberal (after Creature).
- The shipped app's once-per-game random "stickiness" (`nextpsg`) is replaced by one index per occurrence (`base + min(count, 9)`).
- End-of-generation popups are shown before the random next branch is drawn (drawn in the popup close callback) so mirror mode asks at the right moment.
- `IsoBricksRes` / `WallCheck` dead "witch" branches are skipped, as in the shipped app.
- TenseConfirm at the end of the Early Years without overtake shows the "move mob to Romani" branch directly (source needed a second click).
- Obsession penalty is tied to `Stole` (source used inverted tension checks).
- AsylumMeet checks the visiting player's own visit count (source checked any player).
- Battle: only alive warriors fight, the last one standing wins; stalemate / 250-step guard -> most HP. The official battle is buggy, so in mirror mode the battle can differ.
- Salts, everybody refuses: 50% "Risk Too Great", else the story continues (source could jump to the Liberal round 3 hub, skipping events).
- ExpNo after the last round uses the "return tokens" text.
- TownHallDestroyed gives no bonus when nobody verified (source gave it to player A). SmugConsequence letter header says "name illegible" (source showed a stale name).
- One-time events per player (Bloodsmith reward `CreepReward`, crossroads `CrossroadVisited`, verification `Verified`) only list players who have not had them, and the hub link disappears once everybody had it (source always listed everybody).
- Random letter dates (`RandomDate`) move forward within a round (cursor `DateRound`/`DateDays` in the Vars, 1-10 months per date); the source drew each date independently, so they jumped back and forth. After the last round `Generation` is 3; `RandomDate` then uses the Gen III Late years. Dates written into the CSV text are fixed and can still be out of order with the random ones.
- "Conducting Business" (`PrivatizedIntro`) is dated 9 August, 1892 (source: 9 February while the letter says "it is August").
- Mirror mode: the witch identity is asked via the hex location.
- 5-player branches dropped.

## Cost of Disease - known deviations / open questions

- Gen III building tiles: Gloomy Gothic uses `BuildingsExposeValue <= 1`, Prosperity `> 0`. Looks inconsistent, but `<= 1` appears identically in every story version in the Unity project (earlier English edit, latest English, Spanish), so it was kept as the developer shipped it. The owner decided to keep it, but suspects it may be flawed.
- "Still Wary" popup lists Wolves allies only (source always appended the first player due to an always-true condition).
- Ties follow the rulebook (End of the Game): most points, then completed Masterwork, then most Estate Upgrades, else the family shares the win. Tie-breakers pick the winner without changing scores (the original app added +1 / the entered number to the score). Rankings (shared `FinalScoring`) give equal players the same place: the score, then Masterwork, then Estate Upgrades (when asked) separate them, so a shared win shows several "1st".
- "A Return to Evil" keeps the end-of-generation prompt (source jumped straight to scoring).
- Gen II Hospital end: players who skipped the Hospital lose 6VP and an Estate Upgrade ("Lack of Hospitality"), then
  "A Steady Focus" moves those same players 1 level on a Journal Track. When everybody visited, both are skipped. (Until
  October 2026 the port gave the Journal level to the visitors and showed "A Steady Focus" to everyone.)
- Gen I/II were compared passage by passage with the decompiled source in October 2026. Intended differences from the
  source: the Gen I start player can be any player (source left out player C in 4-player games); a Gen II Sealed Envelope
  password works once and ignores case and surrounding spaces (source: exact or capitalised only, reusable); "The Secret
  to Immortality" only lists players who have not used it and disappears when all have; the Immortality Masterwork
  Update discard line in "Escaping The Curse" is shown (source never set its `$tmmasterwork` flag, so it never showed);
  random draws are made where they become visible (factions, buildings, Fever cure rewards, Symposium target).
- End of Generation texts of Gen II (`DevastationEndOfGeneration`, `HospitalEndOfGeneration`) are separate popups, as the official app shows them on their own screen.
- Not ported: the original achievements list, sharing, 5-player games. The Endings Gallery replaces the original endings list (see below).
- Help (`Pages/Help.razor`, main menu): the official app's tabs (How to use, Story Actions, End of Round, its text
  kept) plus Setup, End of Generation, Secret pages, Bids and votes, and This app. Text in `UI_Localization.csv`
  (`Help_Tab_<Page>` label, `Help_<Page>` HTML); the page list is in the razor file.
- Log Book (📖 in the header): see "Log Book" below.
- Village name secrets (Help page, case sensitive, compared to `TownName`): Cost of Disease "Rage" / "Kraven" (evil
  faction), "Plague" / "Mercy" (Devastation / Hospital); Fear of the Unknown "Xenophobia" / "Abomination" (Foreigners /
  Creature); A Time of War "Paradox" / "Destruction" (Time Travel / Martial). Constants in each Vars class. With a code
  the Gen I fate screen is not drawn: the one whose text agrees with play is shown (S5Fate2 picks its heart number by
  flavor among agreeing values); when none agrees the text is slightly off but the link follows the code.

## Endings Gallery

`Shared/Gallery.cs` (`GalleryService`, `GalleryTrees`), `Pages/EndingsGallery.razor` (main menu), the card under the final
ending and the hub hook in `Pages/Gameplay.razor` (`NoteGalleryAsync`), name chips in `Pages/PlayersNameEntry.razor`.

- Stored in localStorage key `gallery`, never in `GlobalData` (New Game, load and undo must not touch it). UI code only,
  never scenario code (mirror sandbox, undo replays).
- A game is keyed by `<ScenarioId>:<FlavorSeed>`; every showing of its final ending replaces the record, so undo and a
  corrected score never count a game twice. Hubs record the players ("same table as last time", name suggestions) and
  the branch (hub id) reached.
- The endings trees (branch = hub title, endings = `Vars.Ending` numbers) are in `GalleryTrees.Tree`; update them when
  an ending or hub changes. Cost of Disease ending 4 is reachable from A Taste of Evil and Prosperity.
  Gallery names: an ending whose page heading is not a title gets `GalleryName: true` and the gallery shows its Label
  (`GalleryTrees.Title`). Cost of Disease ending 8 (heading "Editor's note - August 1923") is "An Uplifting Tale" (owner, 1 Nov 2026).
- Layout (Nov 2026): Endings tab = scenario covers + a map (one grid column per generation, lines drawn by
  `wwwroot/js/gallery.js` above 1000px, a top-down tree below). Lists sit on the Settings panel plate.
- Export / Import (base64 JSON) merges: players, games by id, branches and ticks are combined. The original Unity app
  kept ending titles in PlayerPrefs `GameEndingsData`, matched by title text (village names made duplicates).

## Rankings podium

`FinalScoring.Rankings` keeps its text elements but also sets `GameplayWindow.Rankings` (`Shared/Scenarios/RankingsData.cs`);
`Pages/Gameplay.razor` then shows `Shared/RankingsView.razor` (styles `wwwroot/css/rankings.css`) instead of the text, and
its Continue button follows the window's continue link. Design: mockup "H3 Relic with rain" (owner's pick, Nov 2026):
players by their own name (no "III"), the scenario and the Generation III branch (Endings Gallery tree label) instead of
"Generation III", gold/silver/bronze/iron plinths, original crown (`images/ui/crown.webp`, Unity Ranking/MFW_CrownIcon).
A family win puts everyone on an equal gold plinth without place numbers. Rain is `mfwAtmosphere` on the scene canvas.

## Log Book

Ported from the official app (`ViewLogBook.cs`, data `UnityOriginalApp/Assets/CSV/Updated Features MFW - Log Book Data (1).csv`),
which had one for Cost of Disease only. `Shared/LogBook.cs` (`LogBookService`), `Shared/LogBookPopup.razor` (📖 in the
gameplay header), data `wwwroot/localization/<Scenario>_LogBook.csv` (`Screens;Title;Location;Summary`, English only).
Cost of Disease: 119 events from the official app. Fear of the Unknown (167) and A Time of War (136): fan-written in
October 2026 (owner's choice), condensed from each scenario's own story text; rules-only screens (bids, votes,
explanations, warrior statistics, battles) are left out. Check with `python tools/logbookcheck.py`.

- An event is logged the first time one of its screens is shown. `Screens` are screen methods (the caller of `SaveToUndo`,
  `GlobalData.ResumeMethod`); for Cost of Disease the Unity passages that set `locationName` in `TheCostofDiseaseLatest.cs`,
  renamed to our methods. A screen listed under several events logs the one whose title is in the window title
  (`HuntersChoice`, title variants like `Gen1FateVictim`). A renamed or new screen method must be updated in the CSV.
- `Pages/Gameplay.razor` notes the screen after it is rendered (never scenario code). Logged titles live in
  `ScenarioVars.LogBook`, so undo forgets screens it goes back over and load/New Game behave like the game state.
- As in Unity: six per page, oldest first, the event on display is left out while it is the newest, `Xvillage` (location or summary) -> village
  name (Unity did this only for Financial Woes). Not ported: the per-title location icons (the sprites are not in the
  repository copy of the Unity project) and the lorem-ipsum "setup" button.
- Cost of Disease deviations: exact title match for "logged once" (Unity's `Contains` skipped e.g. "Published" after "A
  Weapon Published"); "The Order Revealed" also logs on the Prosperity intro (Unity only on Gloomy Gothic); "Vote!"
  (`UniEvent2`, unreachable in the source) is left out. The summaries are as shipped, including "A Call for Assistance"
  repeating the summary of "The Work is Complete".

## AI narration (tools/narration)

Voice setting "Oliver": AI narration of all story text, played as clips keyed by a hash of their CSV text
(`tools/narration/chunks.py`, `Shared/Narration.cs`, `docs/AUDIO_TRANSITIONS_MAP.md` 8.4). Male and Female are the
original recordings only.
In user-facing text (Help, settings, labels) the narrator is just "Oliver", never "AI narration" (owner, 30 Oct 2026).

- `python tools/narration/chunks.py plan <Scenario>` (characters still to render), `show <Scenario> <Tag>` (chunks,
  hashes, spoken text), `render <Scenario> [--max=N] [--reserve=N] [--tags=A,B] [--no-private] [--dry]` (checks the
  quota every ~10k characters and stops above the reserve; resumable). Model and settings: `voices.json` `oliver`.
- `Shared/Narration.cs` must split exactly like `chunks.py` (balanced `{{n=...}}` placeholders, nested ones are one, `<b>CHOOSE` cut, FNV-1a 64 hash).
  Scenario code that rewrites story text beyond placeholders makes the split fail: that window is not narrated
  (console "Oliver narration: no clips for <Tag>"). Story text added with `AddCustomContent` is not narrated; use
  `AddStoryContent(tag)` for a plain story tag.
- New screens read by one player alone: list them in `scripts/<Scenario>/private.txt` unless they are the target of
  `AddDoNotOtherPlayersToSee` / `AddOnceYouAreReady`. New placeholder values worth narrating go in `values.txt`.
  Rules-only windows go in `skip.txt`. Typed-in values get stand-ins by their CSV preview (`chunks.STAND_INS`;
  A Time of War previews `Anna` / `Bob` / `Duke Anna` / `Testville` are aliases, `Spam` -> "meat"); where the narrator
  names themself ("it was I, <name>", "<name> IV") an override drops or rewords the name.
- A hand-written override (`scripts/<Scenario>/<Tag>.txt`) rewrites text by hand, which drifts: a narrator clause was
  once dropped unnoticed, and listeners reading along got lost. **Always run `python tools/narration/elevenlabs.py
  check`** after editing an override or the CSV text (word-level diff per chunk; render refuses on drift).
- A CSV text change makes new hashes: render the scenario again (only the changed chunks are sent) and commit the clips
  and `wwwroot/audio/vo/oliver/<Scenario>.json`.
- What is narrated (28 Oct 2026): story only. `chunks.auto_cut` stops a tag at the first choice paragraph (a link,
  "OR") or bold rules line ("Choose:", "Please choose your name:", "SPECIAL RULE", "At the End of the Generation ...
  VP"); bold questions stay. Manual cuts in `scripts/<Scenario>/cuts.txt`. `scripts/<Scenario>/settings.json`:
  `titles` (window title narrated first) and `headers` (leading "Letter from <name> - <date>" line narrated). New
  scenarios: titles on, headers off. Fear of the Unknown was rendered with titles off and headers on and keeps that
  (changing it would re-render ~41k characters). Cuts and settings go into the manifest (`chunks.py sync`), and
  `Narration.cs` applies the same header rule and cut markers.

## Build, run, test

- **Claude sessions:** the cloud container cannot download .NET. Build, run and browser-test in a Remote Control session on the owner's laptop (pre-approved), after pushing the branch.
- `dotnet build MyFathersWorkApp/MyFathersWorkApp.sln`
- `dotnet run --project MyFathersWorkApp/MyFathersWorkApp/MyFathersWorkApp.csproj` (http://localhost:5166; use `--no-launch-profile --urls http://localhost:5199` if 5166 is taken).
- There are no automated tests. Play-test with `tools/test-save-helpers.js`: craft a save at a late-round hub, reload `/Gameplay`, and drive the flow with `run([...])`. Check the browser console for exceptions and run `python tools/tagcheck.py` (and `--scenario=FearOfTheUnknown`). Fear of the Unknown saves: `mkSaveFotU(hubId, {...})`; `auto([...])` / `autoFirst()` click through screens. Static files are served from a build-time snapshot, so inject the helpers with the JS console (or keep them in `localStorage.helpers` and `eval` after reloads) instead of copying them into `wwwroot`. To test mirror mode, pass `mirror: true` to `mkSave`; random click-throughs that report `Mirror:` console lines find draws made too early. Also play through round transitions, not only crafted saves: load and undo rebuild the hub, which hides hub text that was formatted with stale state (`PLAYTEST_FINDINGS.md` #1).
