# My Father's Work App (Unofficial)

The app's own name everywhere (page title, home-screen name, Windows launcher, releases): "My Father's Work App (Unofficial)" (owner, 16 Nov 2026).

Blazor WebAssembly (.NET) re-implementation of the official *My Father's Work* companion app (a Unity app whose
stories were written in Twine/Harlowe and compiled to C# by Cradle). Players make choices on a phone/tablet next to the
board game; the app tells them what to set up and narrates the story over 3 Generations x 3 rounds.

## Status (November 2026)

**Open work and next steps are in [`ROADMAP.md`](ROADMAP.md) - read it first and keep it current.**
When a piece of work is finished, move its section to [`docs/DEVLOG.md`](docs/DEVLOG.md) (newest first) so the roadmap stays short.
Player-facing changes per version go in `wwwroot/CHANGELOG.md` (see Build, run, test).

All three scenarios are complete, played at the table (November 2026) and fully narrated by Oliver. Left before v1.0:
a test on iPhone / iPad (Safari) and code-signing the Windows exe.

| Scenario | Web app | Notes |
|---|---|---|
| The Cost of Disease (`ScenarioId.CostOfDisease`) | **Complete, played at the table** - Gen I, II, III, final scoring, all 8 endings | Only English text. |
| Fear of the Unknown (`FearOfUnknown`) | **Complete, played at the table** - all branches, battle, asylum assessment, scoring, 8 endings | See `docs/DEVIATIONS.md`. |
| A Time of War (`TimeOfWar`) | **Complete, played at the table** - Gen I Taking Sides, Gen II Martial / Time Travel, Gen III Heavy is the Crown / Peace Through War / Future Consequences / You Cant Stop Progress, scoring, all 8 endings | Spec: `docs/ATOW_PORT_MAP.md`. See `docs/DEVIATIONS.md`. |

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
- **Page-ending buttons** (Nov 2026): the one way on from a story window is a green button instead of a link (`Pages/Gameplay.razor` `LinkButtonTag`). The shared helpers set `GameplayElement.ButtonTag`: `AddClickHereToContinue` (Continue), `AddClickHereAfterBid` / `AfterVote` (Bid / Vote Resolved; on a vote or bid special event window it is the Start Voting / Bidding button, and Accept after the countdown follows it to the result page, as in The Ultimate Disease), `AddOnceYouAreReady` / `AddDoNotOtherPlayersToSee` (Reveal, under the line `Gameplay_RevealLead`). A scenario's own link becomes a button by its text when it stands alone: "Click (here) to start the vote / bid..." (Start Voting / Bidding), and as the window's only link "Click (here) to continue..." (Continue), "Click to tabulate scores..." (Tabulate Scores), "Continue to Scoring..." / "Click to continue / move to Final Scoring..." (Final Scoring). Links inside a sentence or next to a choice stay links. The button keeps the link text in `data-link` (test helpers find it by either text). While the page's last button is below the round bar, a jump-down chip with its label ("↓ Continue", like "↓ End of Round" on a round page) scrolls to it (`ShownPageEnd`, `#mfw-page-end`, `js/transitions.js`).
- **Undo** (`GlobalData.SaveToUndo`) finds the caller by name via reflection on the scenario class (`GlobalData.ScenarioType`), then `FinalScoring`: every method that calls it must be unique by name, `static`, and take exactly `(GlobalData)`. Call it before mutating state; methods that only route (no screen) must not call it. Undo restores the JSON snapshot, rebuilds the hub (`GetHubMethod`) and re-invokes the method. Hubs become undo points in `GlobalData.SaveGame` (a hub re-shown in the same round with no undoable screen in between is not stored twice; the next round's hub shown straight after the end-of-round popup is a new point, `PLAYTEST_FINDINGS.md` #7). The whole undo stack is kept in the browser after every screen (`Shared/GlobalData.Persistence.cs`: `continue` = newest point, `undoMeta` + `undo/{i}` = history, RandomArray stored only when it changes, oldest points dropped when storage is full); load reopens the exact screen through `ResumeMethod` and restores the history. Persistent data that must outlive a game (e.g. an endings gallery) needs its own localStorage keys: New Game clears these. **Saved Games** (`Shared/SavedGames.cs`, `wwwroot/js/saves.js`, `Pages/SavedGames.razor`): New Game and loading a save code first pack those keys (+ `continueTime`) into one gzipped key `savedGame/<Scenario>-<FlavorSeed>`, listed in `savedGames`; opening one packs the current game away and unpacks the chosen one into the active keys, history included.
- **Hubs** (`GameplayHub`) are the per-generation round screens. One hub method per branch (e.g. `GloomyGothic`), sections switched by `ReplaceShouldShow(() => globalData.Years ...)` plus state flags. Set `HubId` and register the hub in `GlobalData.GetHubMethod()` (used by load and undo). End the round with a section calling `globalData.ShowEndOfRoundPopUp(nextEvent)`; the last round uses `AddEndOfGenerationSection`. Hub text is formatted when the hub is built; `RunScenarioAction` rebuilds the shown hub after every action that ends with no window/popup open (`RefreshShownHub`), so hub methods must have no side effects. Ending a chain with the hub method instead of `_ => { }` is still fine.
- **`ShowEndOfRoundPopUp` advances `Years` (and `Generation` after Late) *before* calling the next event** - events after the Early round run with `Years == Middle`. At the generation change `ActiveHub` becomes null, so a callback of `_ => { }` then renders a **blank page** (this was the old "blank after Gen II" bug).
- **Randomness** must use the pre-rolled `ScenarioVars.RandomArray` (`RandomElement`, `RandomBool`, `RandomInRange`) with a fixed index per decision so undo/load replay identically. Never read `RandomArray` directly. Indices in use for Cost of Disease: 0-58 (Gen I/II) and 59-203 (`_RND_*` constants in the Gen III files). Fear of the Unknown: `_RND_*` constants in `00_FearOfTheUnknown_Setup.cs` (0-164, battle 400-899). For events that can repeat, use `base + min(counter, 9)`. Shuffles are drawn one position at a time from the remaining items (`HunterAt`, `HuntRewardAt`).
- **Flavor randomness** (who writes a letter, dates, alternative sentences that change nothing in the game) uses `vars.Flavor(key, count)` / `FlavorElement` - a hash of `FlavorSeed` and a unique key. Deterministic for undo/load and never asked in mirror mode. Anything players act on (setup, VP, money, branches) must use `RandomArray`.
- **Mirror mode** ("Play along with the official app", a tester option in Settings on the Main Menu (`Shared/PlayOptions.razor`, localStorage `mfwMirror`, read at New Game), `GlobalData.MirrorMode`, `Shared/GlobalData.Mirror.cs`, `Shared/MirrorPopup.razor`): `RandomArray` starts at -1 (not drawn). The first draw of an unanswered slot throws `MirrorQuestionException`; `GlobalData.RunScenarioAction` restores the state, replays the action in a sandbox once per value, shows the players the text that differs, and reruns the action with their answer. Rules for scenario code:
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
5. Keep source bugs visible: several Twine passages have dead or always-true conditions. Decide deliberately and note it in `docs/DEVIATIONS.md`.

## Adding a scenario (A Time of War) - checklist

The engine was generalized while porting Fear of the Unknown. A new scenario needs:

- A Vars class deriving from `ScenarioVars(randomCount)` whose `Reset(GlobalData)` calls `ResetBase`; a property on `GlobalData` with `ShouldSerialize*`, and cases in `GlobalData.ScenarioVars`, `ScenarioType`, `ResetScenarioVars`, `StartScenario`, `ShowFinalEnding`, `GetHubMethod` and `LoadScenarioLanguagesOptionsAsync` (CSV paths; unknown scenarios navigate home).
- A hub id enum in `Shared/Enums/`, mapped in `GetHubMethod`.
- After the last generation link to `FinalScoring.Start`; provide `public static void FinalEnding(GlobalData)` and the shared scoring tags (`ScoreEntry_Content`, `ScoreEntry_Player`, `TieBreaker2_Content`, `Rankings_*`) in the scenario CSV.
- A `tools/tagcheck.py --scenario=` entry and `test-save-helpers.js` support. Run `tools/sourcecheck.py` and the `PLAYTEST_FINDINGS.md` checklist.
- `Shared/Enums/Generation.cs` has 3 values; after the last round `Generation` becomes 3 (guard like `EndedYears()` in Fear of the Unknown).

Source size: `ATimeOfWar.txt` 297 passages, English. It has time-travel/paradox mechanics - look at the Unity `View*` scripts (`ViewBiddingSystem`, etc.) for UI the Twine text relies on.

## Scenario branches and deliberate deviations

Every known difference from the official app (fixed source bugs, bugs kept as shipped, open questions) is in
[`docs/DEVIATIONS.md`](docs/DEVIATIONS.md). Read the scenario's section before changing its code, and add new ones there.

- A Time of War: Gen I Taking Sides -> Gen II Martial (Monarchists / Separatists intro) or Time Travel -> Gen III Reign or Peace
  (after Martial, by the war winner), Warning or Paradox (after Time Travel). Endings: Paradox 1/2, Warning 3/4, Reign 5/6,
  Peace 7/8 (`Vars.Ending`).
- Fear of the Unknown: Gen I Witch Hunt -> Gen II Foreigners or Creature (+ Obsessions) -> Gen III Isolation / Tension (after Foreigners) or Privatized / Liberal (after Creature).
- The Cost of Disease: Gen I Fever -> Gen II Devastation -> Gen III Gloomy Gothic or Prosperity, or Gen II Hospital ->
  Gen III University or No University (hub ids, `CostOfDiseaseHubId`).

## Help, Log Book and village name secrets

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
- Tabs (Nov 2026): Endings, Achievements, Hall of Fame (was Players). `Shared/GalleryAchievements.cs` computes the
  achievements and the highest score per scenario from the games and ticks every time (nothing stored, so undo, edits and
  imports stay right): the official app's six (end of each scenario, all 8 endings of each, Unity `ViewEndings`) plus fan
  ones for Masterworks (`GalleryGame.Masterworks`, ticked at score entry or in the game sheet), scores (75 / 100 / 150 VP,
  owner's levels), a family win and a three-win streak. Texts: `Ach_<Id>_Title/_Text` in `UI_Localization.csv`. The
  ending page lists new records and achievements of the game just recorded (compared with the gallery without it).
- Export / Import (base64 JSON) merges: players, games by id, branches and ticks are combined. The original Unity app
  kept ending titles in PlayerPrefs `GameEndingsData`, matched by title text (village names made duplicates).

## Rankings podium

`FinalScoring.Rankings` keeps its text elements but also sets `GameplayWindow.Rankings` (`Shared/Scenarios/RankingsData.cs`);
`Pages/Gameplay.razor` then shows `Shared/RankingsView.razor` (styles `wwwroot/css/rankings.css`) instead of the text, and
its Continue button follows the window's continue link. Design: mockup "H3 Relic with rain" (owner's pick, Nov 2026):
players by their own name (no "III"), the scenario and the Generation III branch (Endings Gallery tree label) instead of
"Generation III", gold/silver/bronze/iron plinths, original crown (`images/ui/crown.webp`, Unity Ranking/MFW_CrownIcon).
A family win puts everyone on an equal gold plinth without place numbers. Rain is `mfwAtmosphere` on the scene canvas.

## Guided mode (spoilers!)

Settings / pause menu toggle (`mfwGuided`, `Shared/GameOptions.cs`, `Shared/PlayOptions.razor`). A route card on every hub
(foldable, `mfwGuideFolded`) and notes on the screens whose choices steer a branch or ending (`Shared/GuideBox.razor`, in
Gameplay windows, InfoPopup and InputPopup). Engine `Shared/Guide/GuideService.cs`, rules per scenario in
`Shared/Guide/<Scenario>Guide.cs` keyed by screen method (`ResumeMethod`) or hub id, text in
`wwwroot/localization/<Scenario>_Guide.csv` (`[[hub:<HubId>]]` / `[[end:n]]` become Endings Gallery names with ✓ found /
☆ not yet). Rules: guide code only reads state (never `RandomArray`), explains odds instead of revealing a roll, and shows
no tally of secret choices (investigations, Panacea, schematics, money given in secret). Not narrated (no story tag);
`js/audio.js` skips `.guide-note`. **When a branch condition or a screen method changes, update the guide too.**

The Scenario Language page is skipped since 16 Nov 2026 (`SKIP_PAGE` in `Pages/PlayersScenarioLanguage.razor`, it starts
the first scenario language and replaces itself in the history); narration is chosen in Settings and the pause menu.

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
- **After every render run `python tools/narration/pauses.py <Scenario>`** (ffmpeg, no credits) and commit
  `<Scenario>.pauses.json`: the pauses of every clip, used to highlight the sentence being spoken.
- What is narrated (28 Oct 2026): story only. `chunks.auto_cut` stops a tag at the first choice paragraph (a link,
  "OR") or bold rules line ("Choose:", "Please choose your name:", "SPECIAL RULE", "At the End of the Generation ...
  VP"); bold questions stay. Manual cuts in `scripts/<Scenario>/cuts.txt`. `scripts/<Scenario>/settings.json`:
  `titles` (window title narrated first) and `headers` (leading "Letter from <name> - <date>" line narrated). New
  scenarios: titles on, headers off. Fear of the Unknown was rendered with titles off and headers on and keeps that
  (changing it would re-render ~41k characters). Cuts and settings go into the manifest (`chunks.py sync`), and
  `Narration.cs` applies the same header rule and cut markers.
- Sentence mode (`settings.json` `"sentences": true`, A Time of War since 13 Nov 2026, The Cost of Disease since 14 Nov, Fear of the Unknown since 15 Nov): a sentence with placeholders is
  one clip per value combination (no joins around a value); its clip is named by the sentence's "key text" (CSV text,
  values filled in, typed-in names kept as the placeholder). Values come from the options or, for replacements, the
  `values.txt` family of the preview (a `@Tag` line or a block of plain lines). Overrides become `replace.txt` lines
  (`Tag | find | replace`, drift-checked). `Narration.SentenceUnits` must match `chunks.sentence_units`. With headers
  on (Fear of the Unknown), a header line with a typed-in name or random date is one clip named by its CSV text,
  spoken as the generic header (`chunks.header_group`).
- While Oliver speaks, `js/audio.js` highlights the sentence being spoken (CSS Custom Highlight API,
  `::highlight(mfw-narration)` in `css/app.css`) and scrolls it into view (not for 4 s after the reader scrolled). A
  clip can hold a whole page: `js/narration-sync.js` splits its text (`NarrationClip.Text`, the narrated part only) into
  sentences and matches the sentence ends with the clip's pauses (`<Scenario>.pauses.json`). A clip without pauses, or
  whose length differs from the list (rendered again, `pauses.py` not run), is timed by text length, which can be
  seconds off in a long clip. How the page follows is the "Scrolling" setting in the pause menu (`audio.scroll`; owner's
  choice, 15 Nov 2026): `lead` (default) glides 0.8 s before a sentence ends so the next one is in place, `flow`
  moves continuously, `page` a screen at a time. Scroll targets are clamped to the page: Firefox on Android drew a
  black bar over the header buttons when the app kept asking for a scroll past the bottom; for the page itself the
  end is measured against the large viewport (`100lvh`, toolbar hidden), so the app never scrolls into the last
  toolbar height (Firefox keeps `clientHeight` at the small viewport; diag showed `visualViewport.offsetTop` -64 at
  that end). The scroll style `off` leaves the page alone. How the sentence is marked is a trial setting next to it
  (`audio.highlight`, `STYLES` in `audio.js`, menu order None, Fade the rest, Candle glow, Quill line, Ember by
  candlelight, Travelling spark - owner, 16 Nov 2026; dropped: Gold backlight, Underline, Ember text (with sparks),
  Sparks, Wandering wisp, soft travelling spark, Firefly, Candle glow with motes): `off`, `dim` (fade the rest,
  `::highlight`), `quill` (a line of light under the spoken words; the rest fades slightly,
  `::highlight(mfw-narration-soft)`), `glow` (candle light bars, `mix-blend-mode: overlay`), `emberglow` (the default since 16 Nov 2026: candle glow
  + embers born around the words being spoken, moved every frame on their own loop with a
  wandering heading, overlay so they drift behind the words; the ink stays black, the ember-brown ink was dropped) and `wisp` (a spark that moves from
  the sentence's first word to its last, by `narration-sync.js` `starts().spoken`, fades in a moment ahead of it,
  trails off past the end and at line wraps, wanders off its route, on a faint haze). Bars and lights are children
  of `<body>` in page coordinates (`.mfw-reading-*` in `css/app.css`), parchment only. Embers and the spark
  are left out only when Graphics Low was chosen in Settings (not for the reduced-motion default: Android power
  saving turned that on and hid them on the owner's phone and tablet). The glide is animated in `audio.js` (the browser's smooth scroll jumps with
  reduced motion), except on Firefox for Android, where a scroll set frame by frame left the frame and the header
  buttons a toolbar height too low with a black bar above until the next tap (owner's phone, not reproducible on a
  desktop): there the browser's smooth scroll is used. For a report from a phone, `?mfwdiag` in the address shows a
  viewport readout and `?mfwglide=browser` / `page` picks the scroll path (both kept for the tab). Animations do not
  run in a hidden Browser pane: play-test them with a headless browser (Python Playwright is on the owner's laptop).
  No word-level timing: that needs real timestamps (ElevenLabs with-timestamps at render
  time, or a local aligner).

## Offline app (PWA)

The published site (GitHub Pages) is an installable offline app (21 Nov 2026).

- **App shell:** `wwwroot/service-worker.published.js` (the Blazor offline worker, `ServiceWorkerAssetsManifest` in the
  csproj; `service-worker.js` is the do-nothing development worker). It caches every published file (~40 MB) except
  the narration clips (`audio/vo/**/*.mp3`), source maps, unused Bootstrap files and `.br`/`.gz`, in
  `offline-cache-<manifest version>`. On an update unchanged files are copied from the previous version's cache by
  hash (`__mfw/asset-hashes.json`), so only changed files are downloaded. Cached `.mp3` responses are cut to the
  request's Range (206), which audio elements and Safari need. Deep-link navigations get `index.html`.
- **Narration downloads:** `wwwroot/js/audio-cache.js` (`mfwAudioCache`), shared by the page and the worker and meant
  to be reused by a Capacitor build (`match()` / `objectUrl()` without a worker). Its own cache `mfw-audio`, never
  touched by version bumps. Packs come from `audio/vo/packs.json` (one per clip folder: `oliver-<Scenario>`, `male`,
  `female`; path, SHA-256 prefix and size per file), written at deploy by `tools/audio-packs.py`. A clip is stored as
  `<path>?h=<hash>` and served for any `?v=`: unchanged clips survive updates, `reconcile()` (worker activate and page
  start) drops changed ones and the page tops up the packs the player chose (`__mfw/pack/<id>`).
- **Page side:** `wwwroot/js/offline.js` (`mfwOffline`: registration with `updateViaCache: "none"`, update checks on
  return to the front and every 30 min, install prompt, downloads one pack after the other, `apply(adds, removes)`),
  `Shared/OfflineService.cs`, `Shared/UpdateBanner.razor` ("A new version of the app is ready" -> Update posts
  `mfw-skip-waiting` and reloads; "Updated to <version>" after; a new version never takes over by itself while the app
  is open) and **Offline play** (mockup A, owner's pick 21 Nov 2026): `Shared/OfflinePanel.razor`, opened by the
  "Offline play" button in Settings and the pause menu (`Shared/OfflineEntry.razor`, with a status line) and the
  "⤓ Offline play" chip on the Main Menu (`Pages/Home.razor`; shown while the app is not opened as an installed app;
  × hides it per device, localStorage `mfwInstallHidden`). It shows whether the app works offline, Install as an app
  (or the iPhone / Firefox for Android menu hint), and one tickbox per pack with what Download will do (Downloaded,
  Download, Will be removed, Not downloaded): Download downloads the ticked packs and removes the unticked ones that
  are stored. Until the player has chosen once (localStorage `mfwOfflineAsked`) Oliver's three scenarios are ticked
  and the original voices are not. Right after `appinstalled`, or on the first start in standalone mode, it opens by
  itself on the Main Menu (`askNarration`); closing it counts as an answer.
- **Firefox:** an audio file the page played earlier can sit in its HTTP cache as a partial (206) entry, and a
  `cache: "no-cache"` fetch then returns that 206, which `Cache.put` refuses (the whole install failed, so Firefox
  never went offline). The worker and `audio-cache.js` fetch such a file again with `cache: "reload"`.
- JS interop: pass a `string[]` to `InvokeVoidAsync` as `(object)array`, or it is spread as the params array.
- **Deploy:** `tools/pages-prepare.py` (base href, then index.html's hash in `service-worker-assets.js` again - without
  it the worker's install fails its integrity check -, 404.html, .nojekyll, packs.json).
- The Windows downloads write `offline: false` into `js/edition.js` (`tools/build-windows.ps1`): no worker there.
- Test locally: `dotnet publish ... -o <dir>`, `python tools/pages-prepare.py <dir>/wwwroot myfathersworkapp`, serve
  `<dir>/wwwroot` at `/myfathersworkapp/` with byte ranges (a small Kestrel app) and drive it with Playwright
  (`channel="msedge"`, `--mute-audio`, `set_offline(True)`). A publish into an existing folder does not copy an
  unchanged `index.html` again: start from an empty folder or copy it over before `pages-prepare.py`.

## Android app (android-app/, Capacitor)

Sideloadable APK (Nov 2026), launcher name "My Father's Work (Unofficial)" (owner, 21 Nov 2026; the web pages keep
the full app name), details in [`android-app/README.md`](android-app/README.md). `tools/android-stage.py`
copies the published wwwroot to `android-app/www` without the narration clips (`js/edition.js` `native: true`,
`remote` = the GitHub Pages site, no service worker, `js/capacitor.js` + `android-app/web/native.js` for the Back
button). Players download narration in Offline play: `audio-cache.js` fetches from `remote` but stores under the app's
own url, `audio.js` plays stored clips as `blob:` urls and streams the rest from `remote`. Never change the appId,
`androidScheme` https or `hostname` localhost, and sign every build with the one release keystore (outside the repo,
repository secrets `ANDROID_*`): a changed origin or key loses the players' saves and downloads.
`.github/workflows/android-apk.yml` builds it (versionCode = the revision of deploy-pages.yml).

## Build, run, test

- **Changelog:** `wwwroot/CHANGELOG.md` (shown in Help > What's new, linked from the README). Add a player-facing entry
  for every version merged to main (`## v0.66.6.<revision> - <date>`, revision as in the deploy workflow).
- **Claude sessions:** the cloud container cannot download .NET. Build, run and browser-test in a Remote Control session on the owner's laptop (pre-approved), after pushing the branch.
- **Phone test over the LAN:** `dotnet build MyFathersWorkApp/MyFathersWorkApp.sln -p:DebugType=none -p:DebugSymbols=false`,
  then `dotnet run tools/lan-serve.cs` (.NET 10 SDK) and open `http://<laptop Wi-Fi IP>:5199`. Not `dotnet run --urls
  http://0.0.0.0:...`: the Blazor dev server (DevServer 9.0.5) sends the whole file for a byte-range request, so audio
  jumps back to its start mid-clip (the highlight keeps going). JS/CSS/CSV changes show on a reload, C# needs the build.
- `dotnet build MyFathersWorkApp/MyFathersWorkApp.sln`
- `dotnet run --project MyFathersWorkApp/MyFathersWorkApp/MyFathersWorkApp.csproj` (http://localhost:5166; use `--no-launch-profile --urls http://localhost:5199` if 5166 is taken).
- Offline Windows release: `pwsh tools/build-windows.ps1 [-Version x] [-Edition All|Full|Light|NoAudio]` ->
  `dist/MyFathersWork-Windows-<version>.zip` (`MyFathersWork.exe` + `wwwroot`, ~450 MB zipped, mostly audio; needs the LFS
  files), `-light.zip` (~270 MB: ffmpeg re-encodes speech to 48 kbps mono, music to 96 kbps; same durations, so the pauses
  lists still fit) and `-no-audio.zip` (~60 MB, no `wwwroot/audio`; `js/edition.js` says `audio: false`, so `audio.js` loads
  nothing and `AudioService.Available` hides the sound toggles, narration settings, volumes and the narration pill). The exe (`MyFathersWorkApp/WindowsLauncher/`,
  self-contained, no .NET needed) serves `wwwroot` on the fixed port `http://localhost:51660` (fixed because saves live in
  localStorage per origin) and opens an Edge app window with its own profile (`%LOCALAPPDATA%\MyFathersWork\Browser`,
  holds the saves); the server stops when the profile's `lockfile` is released (last window closed). A second start
  just opens another window; without Edge it falls back to the default browser plus a console window. Push a tag `v*`
  to have `.github/workflows/windows-release.yml` attach the zip to a GitHub Release (manual runs: workflow artifact).
  The exe is not code-signed yet (SmartScreen warning).
- There are no automated tests. Play-test with `tools/test-save-helpers.js`: craft a save at a late-round hub, reload `/Gameplay`, and drive the flow with `run([...])`. Check the browser console for exceptions and run `python tools/tagcheck.py` (and `--scenario=FearOfTheUnknown`). Fear of the Unknown saves: `mkSaveFotU(hubId, {...})`; `auto([...])` / `autoFirst()` click through screens. Static files are served from a build-time snapshot, so inject the helpers with the JS console (or keep them in `localStorage.helpers` and `eval` after reloads) instead of copying them into `wwwroot`. To test mirror mode, pass `mirror: true` to `mkSave`; random click-throughs that report `Mirror:` console lines find draws made too early. Also play through round transitions, not only crafted saves: load and undo rebuild the hub, which hides hub text that was formatted with stale state (`PLAYTEST_FINDINGS.md` #1).
