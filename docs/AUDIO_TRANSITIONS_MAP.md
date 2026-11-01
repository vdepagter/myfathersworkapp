# Audio and screen transitions: Unity -> web app map

Scope: music + sound effects + screen transitions (sections 1-7) and voice-over (VO, section 8).
Sources: `UnityOriginalApp/Assets` (scene `Scenes/Main.unity` is the only scene in the build settings), clips resolved by GUID
from the `.meta` files. Clip paths below are relative to `UnityOriginalApp/Assets/`. Web paths are relative to
`MyFathersWorkApp/MyFathersWorkApp/`. "Uncertain" marks what could not be proven from the files.

## 1. How the Unity app plays sound

`Scripts/Manager/SoundManager.cs` (object `Manager/SoundManager`, `DontDestroyOnLoad`) has one AudioSource per channel. Every
source has `volume 1`, `playOnAwake 0`.

| Channel (AudioSource) | Clip in the scene | Loop | Used by |
|---|---|---|---|
| `EffectsSource` ("Audio Source") | set per call | `Play` loops, `PlayOneTime` does not | screen transition sounds (`TransitinoSoundScript`), welcome sound, setup-page sounds. **One source: a new effect cuts the previous one.** |
| `MusicSource` ("Music") | set per call | yes (`Loop: 1`) | `PlayMusic` / `StopMusic`: title theme, scenario theme, help |
| `ClickSource` | `New SFX/click-to-continue.ogg` | no | link clicks in story/hub text (`TwineTMProLinkHandler.s_OnClickSFX` -> `OnPlayClickToContinue`), confirm/cancel/go-to-main buttons of the confirmation and pause popups |
| `PopupAudioSource` | set per call | no | `OnOpenPopupPlayAudio` / `OnClosePopupStopAudio`: setup, end of round/generation, special event, bidding. **One source: a new popup sound cuts the previous one.** |
| `EndGameAudioSource` | `SFX/New_8_April/My Fathers Work-OST/Chronicle Part  Three.ogg` | no | end of game (`ViewGenerationEnding.EnableDisableContinueBtn(true)`) |
| `MayorSource` | `New SFX/Mayor selected.ogg` | no | **dead code** (see 2.3) |
| `BankLibrarySource` | `New SFX/Build Bank or library.ogg` | no | **dead code** (see 2.3) |
| `VOSource` | set per call (+ `thunderVoxStoryClip`) | no | VO (section 8) |

Besides these, almost every UI `Button` has its own AudioSource with `click-to-continue.ogg` wired to `onClick` (popup
ACCEPT/CONFIRM, main menu, back/pause/continue buttons). Exceptions: the main menu **New Game** button also plays
`UI/PlayerSelection/ViewArea` (`SFX/Enter room-footsteps.ogg`); the scenario buttons play `UI/Scenarios/Viewarea/Back`
(click). AudioSources on `*/ViewArea` objects with `Enter room-footsteps.ogg` that no button references are never played.

Volume: Settings (main menu) and both Pause popups have a music slider (`MusicSource.volume`) and an effects slider
(`AudioListener.volume` = master, also scales music), saved in PlayerPrefs `Music` / `volume` (default 1). Music never
fades: `PlayMusic` swaps the clip abruptly. `ViewGeneration`/`ViewGenerationEnding` only call `PlayMusic` when the clip name
differs from the playing one, so the theme does not restart on every hub/story switch.

## 2. Sound effects

### 2.1 Wired and audible in the shipped Unity project (needed)

| Sound | Clip file | Unity trigger | Web app trigger (file:method) | Channel / loop |
|---|---|---|---|---|
| Click | `New SFX/click-to-continue.ogg` | `TwineTMProLinkHandler.OnPointerClick` (any link in story or hub text, 1 s re-click lock) -> `SoundManager.OnPlayClickToContinue`; every Button's own AudioSource `onClick` | `Pages/Gameplay.razor:InvokeWindowCallback` (window and hub links), `GoBack`, `Pause`; `Shared/InfoPopup.razor:OnAcceptClicked`; `Shared/InputPopup.razor:OnAcceptClicked`; `Shared/PauseMenu.razor` buttons; setup pages: `Home.razor:OnNewGameClick/ShowLoadPopup/OnContinue`, `PlayersSelect.razor:SelectPlayers/GoBack`, `PlayersInitial/PlayersNameEntry/PlayersWelcome/PlayersVillage/PlayersVillageConfirm.razor:Continue/GoBack`, `PlayersScenario.razor:SelectScenario/GoBack`, `PlayersScenarioLanguage.razor:SelectLanguage/GoBack`. `Shared/MirrorPopup.razor:Answer/ConfirmNumber/Roll` has no Unity equivalent (optional). | click, no loop, may overlap |
| Screen cover | `New SFX/UI_screen transitions.ogg` | `Animations/ScreenTransitioin.controller` state `TransitionAnim` (`TransitinoSoundScript`), entered by `UIView.HideAnimated` on every `ViewController.ChangeView` | every screen change in section 4 rows 1-2: setup page navigation (`Nav.NavigateTo` in `Pages/*.razor`), and in `Pages/Gameplay.razor` a change of the base screen (window <-> hub, see 5.1) | effects, no loop; cut at 1.0 s by the reveal sound |
| Screen reveal | `New SFX/UI_screen transitions-reverse tear.ogg` | state `TransitionAnimRev`, entered by `UIView.ShowAnimated`: every view shown by `ChangeView` (1.0 s after the cover) **and every popup view opened with `ShowAnimated`** (setup, end of round/generation, bidding) | 1.0 s after "Screen cover"; and when `Pages/Gameplay.razor` starts showing a `GameplayPopup` (`InfoPopup`) | effects, no loop |
| Setup popup | `SFX/New_8_April/set up window-new.ogg` | `ViewItemObtain.ShowAnimated` (`setupStyleEvnt` block of a story passage, shown 0.35 s after the passage) and `ViewItemObtainHUB.ShowAnimated` (`setupStyle` block of a hub passage, 1 s after the hub) | `Pages/Gameplay.razor` when a `GameplayPopup` with `PopUpTitle.Setup` or `PopUpTitle.SpecialSetup` is first shown (created in ~250 places in `Shared/Scenarios/*/*.cs` - derive, do not edit each) | popup, no loop |
| End of round | `New SFX/Fathers work SFX pt 2/End of round.ogg` | `ViewEndOfRound.ShowAnimated` (from `PassageTracker.CheckProgress` -> `SetEndOfRound`, +0.25 s) | `Shared/GlobalData.cs:ShowEndOfRoundPopUp` (popup `PopUpTitle.EndOfRound`) | popup, no loop |
| End of generation | same clip `End of round.ogg` | `ViewEndOfGeneration.ShowAnimated` (`S_OnEndOfGeneration`, +0.1 s) | popups with `PopUpTitle.EndOfGeneration`: `TheCostOfDisease.FeverHeart2_1`, `.DevastationEndOfGeneration`, `.HospitalEndOfGeneration`; `FearOfTheUnknown.Gen1End`, `.Gen2ForeignersEnd`, `.Gen2CreatureEnd` | popup, no loop |
| Special event | `New SFX/Fathers work SFX pt 2/Special Event-words appear.ogg` (12.25 s file) | `ViewSpecialEvent.ShowEventPopup`, called while the special-event passage renders. Overlay + sound volume fade in 0->1 over 2 s, hold 0.5 s, fade out over 2 s, then `OnClosePopupStopAudio` (heard ~4.5 s) | explicit cue on the window built by: Cost of Disease `S5Special1A` (01_Gen1_Fever), `EvilHunter1Event`, `HunterConf2`, `WolvesVote` (04_Gen3_GloomyGothic), `GoodFrenzyEvent2`, `FrenzyBid` (05_Gen3_Prosperity), `S5SpecialVote`, `ElectionBid` (06_Gen3_NoUniversity), `UniEvent2UltimateDisease` (07_Gen3_University); Fear of the Unknown `ExecutionVote`, `ExecutionBid` (01_Gen1_WitchHunt), `WallVote` (03_Gen2_Foreigners), `StealBid`, `StealVote` (07_Gen3_Tension), `ProgramBid`, `PolicyBid`, `PolicyVote`, `Bribe` (09_Gen3_Liberal) | popup, no loop, volume envelope |
| Bidding window | `New SFX/Fathers work SFX pt 2/Set up-window appear.ogg` | `ViewBiddingSystem.OnShowBidding` (link "click to start the bid/vote" in each special-event passage, +0.1 s) | **no web equivalent**: the web app links straight from the special-event window to the result (`AddClickHereAfterBid` / `AddClickHereAfterVote` / `AddNextContentWithLinks`). Targets if a bidding screen is added: `S5Special1B`, `EvilHunter1Event2`, `HunterConf3`, `WolvesVote2`, `GoodFrenzyEvent2b`, `FrenzyBid2`, `S5SpecialVote2`, `ElectionBid2`, `UniEvent2UltimateDisease2`, `ExecutionVoteResolved`, `ExecutionBidResolved`, `WallVoteResolved`, `StealBidResolved`, `StealVoteResolved`, `ProgramFocus`, `PolicyBidResolved`, `PolicyVoteResolved`, `BribeResolved`. Open question 1. | popup, no loop |
| Reveal (countdown) | `New SFX/Fathers work SFX pt 2/Reveal.ogg` | `ViewBiddingSystem.OnStartBidding` ("Start Bidding" button): sound + text 3, 2, 1 (1 s each) then "reveal" | none (same open question) | popup, no loop |
| Welcome | `SFX/New_8_April/welcome to my fathers work-window.ogg` | `ViewMainMenu.Start` +0.2 s, app launch only; cut by `SoundManager.Stop()` when the main menu hides | `Pages/Home.razor` first render after app start, once per session; blocked by browser autoplay until the first user gesture (see 6) | effects, no loop |
| New Game footsteps | `SFX/Enter room-footsteps.ogg` | main menu New Game button `onClick` -> `UI/PlayerSelection/ViewArea` AudioSource (plus its own click) | `Pages/Home.razor:OnNewGameClick` | own source, no loop |

`ViewPlayerNameEnter.ShowAnimated` also calls `PlayOneTime(Enter room-footsteps)`, but on the same `EffectsSource` that
`TransitionAnimRev` uses in the same frame, so it is most likely cut immediately (uncertain). `ViewGamePlayerCounts`,
`ViewPlayerIntro`, `ViewEnterVillageName`, `ViewVillageIntro`, `ViewRankingPage` have `clip` fields that are empty in the
scene: no sound.

### 2.2 Wired but not needed

| Clip | Unity use | Why not needed |
|---|---|---|
| `SFX/New_8_April/select-Help tab.ogg` | `ViewHelp.OnOpenHelp` -> `PlayMusic` (loops as music while Help is open) | web app has no Help screen |
| `SFX/UI Select.ogg` | AudioSources on inactive `LinkTemplate`/`ContinueButton` templates | never active |

### 2.3 Present in the project but never played (owner decision, open question 2)

`Mayor selected.ogg` and `Build Bank or library.ogg` are assigned to `MayorSource` / `BankLibrarySource`, but they are only
played from `Cradle-master/Players/TwineTextPlayer/Script/TwineTextPlayer.cs:SetCustomClickSound` (link text containing
"bank"/"library" or a player name). That component is not in `Main.unity` (the scene uses `Cradle/Players/TwineTMProPlayer`),
so the shipped project never plays them. All other clips below have **no reference at all** (GUID not found in any scene,
prefab, controller or asset).

| Clip (size, length) | Intended meaning (from name) | Candidate web trigger if wanted (uncertain - not in Unity) |
|---|---|---|
| `New SFX/Mayor selected.ogg` (293 KB, 6.8 s) | mayor chosen | `TheCostOfDisease.S5Special1B` player-name links -> `S5SpecialSetup1` |
| `New SFX/Build Bank or library.ogg` (153 KB, 4.5 s) | Bank/Library chosen | `TheCostOfDisease.S5SpecialSetup1B` links -> `S5SpecialSetup1B_0` |
| `New SFX/Fathers work SFX pt 2/Mayor Elected.ogg` (253 KB, 12.8 s) | mayor elected | `TheCostOfDisease.S5SpecialSetup1` popup |
| `New SFX/Fathers work SFX pt 2/Medieval Battle.ogg` (328 KB, 16.9 s) | battle | `FearOfTheUnknown.MainEvent` (battle intro) or `Battle` (08_Gen3_Privatized) |
| `New SFX/Fathers work SFX pt 2/The mark of genius.ogg` (394 KB, 13.1 s) | Mark of Genius | `TheCostOfDisease.Gen1SanityTrack` (Fever hub section "The Mark of Genius") |
| `New SFX/Fathers work SFX pt 2/Build Building.ogg` (350 KB, 13.9 s) | building | no clear point; Gen II tiles are drawn inside hub text (`Gen2Building`), hubs must stay side-effect free |
| `Attract attention.ogg`, `Game End.ogg`, `Game-End-FadeIn.ogg` (pt 2) | - | none |
| `SFX/`: `Page Flip 1-3`, `Score Screen appears`, `Tense-death-you lose-something bad happened`, `UI Continue-in game`, `UI Start Game`, `UI window text appears`, `UI add player-trinket on desk (2)`, `Windy Loop`, `Menu Loop-strings and wind`; `SFX/New_8_April/`: `Are you sure window`, `confirm select-window-go to title`, `My Fathers Work-OST/Endings-strings and wind`; `New SFX/`: `Menu-rain-loop`, `Cost of disease theme loop` (quieter copy of the "-louder" file), `Chronicle theme-demo.wav` (30 MB) | - | none |

## 3. Music

| Context | Clip file | Unity trigger | Loop | Stopped / replaced | Web app trigger |
|---|---|---|---|---|---|
| Main menu and whole setup flow | `SFX/Fathers work Title theme.ogg` (119 s) | `ViewController.Start` -> `mainMenu.Show()` -> `ViewMainMenu.PlayBGSound(true)`; again in `ViewMainMenu.ShowAnimated` (return to menu) | yes | replaced by the scenario theme when the first story/hub view shows; replaced by Help music | `Pages/Home.razor` and all setup pages (`PlayersSelect` ... `PlayersScenarioLanguage`); also after `Shared/PauseMenu.razor:OnReturnToMainMenu` |
| The Cost of Disease | `SFX/New_8_April/My Fathers Work-OST/Chronicle Part one_1-2.ogg` (205 s) | `ViewGenerationEnding.ShowAnimated` / `ViewGeneration.ShowAnimated`: `clip[StoryIndex]` when its name differs from the playing clip (first view after scenario select or Continue) | yes | end of game | `/Gameplay` with `ScenarioId.CostOfDisease` (after `GlobalData.StartScenario` or any `LoadGameAsync`) |
| Fear of the Unknown | `.../My Fathers Work-OST/Chronicle Part Two_2-2.ogg` (214 s) | same, `StoryIndex` 1 | yes | end of game | `/Gameplay` with `ScenarioId.FearOfUnknown` |
| A Time of War | `.../My Fathers Work-OST/Cost of disease theme loop-louder.ogg` (173 s) | same, `StoryIndex` 2 (as assigned in the scene; looks like a placeholder - uncertain intent) | yes | end of game | `music-cost-of-disease.mp3` (the "louder" file only differs in level) |
| Score entry, tie breaker, rankings | (no change) | - | - | - | `FinalScoring.*` keeps the scenario theme |
| End of game | `.../My Fathers Work-OST/Chronicle Part  Three.ogg` (52 s, two spaces in the name) | `ViewGenerationEnding.EnableDisableContinueBtn(true)` from the ending passages (CoD `END-*`, `WinnerHUB`; FotU `FOTU-End1..8`, `VarEndingsPassage`): `EndGameAudioSource.Play()` + `MusicSource.Stop()` | **no** (plays once) | never stopped explicitly (`EnableDisableContinueBtn(false)` is never called) | the final ending window: `TheCostOfDisease.FinalEnding` (08_TheCostOfDisease_Scoring.cs), `FearOfTheUnknown.FinalEnding` (05_FearOfTheUnknown_Gen3_Shared.cs), reached via `GlobalData.ShowFinalEnding` from `FinalScoring.Rankings` |

## 4. Screen transitions

The transition overlay is `UI/TransitionAnim` (Canvas sorting order 10, above every view and popup; views/popups use 0-5).
Its Image plays a 22-frame sprite flip-book `New_UI_Assets/ScreenTransitions/Transition 100.png ... 121.png` (2048x2048
textures, sprite rect 1920x1080 bottom-left, ~1 MB total): a black ink blot with a grey rim that spreads from the top-right
corner until it covers the screen (frame 121 = full black). Child `White-BG` (full-screen Image, black) fades alpha 0 -> 1.

| Screen change type | Unity animation | Duration | CSS approximation |
|---|---|---|---|
| 1. View change (`ViewController.ChangeView`): setup pages, story <-> hub, -> score entry / tie breaker / rankings, -> main menu | t=0 `HideAnimated`: `TransitionAnim.anim` frames 100->121 (+ `White-BG` alpha 0->1), cover sound. t=0.5 old view disabled; outgoing story/hub text is hidden at t=0 (`DisableContent`). t=1.0 `ShowAnimated`: `TransitionAnimRev.anim` frames 121->100 (ink recedes), reveal sound. Input lock: overlay `raycastTarget`, 0.5 s `ChangeView` re-entry guard | cover 0.5 s, hold black 0.5 s, reveal 0.5 s (clips stop at 0.517 s, 60 fps samples, frame steps of 1-2 samples) | full-screen black overlay: `clip-path: circle(0% at 100% 0%)` -> `circle(150% at 100% 0%)` over 0.5 s (cover), hold, reverse over 0.5 s; or reuse the 22 PNGs as `mask-image` frames with `steps(21)`. Block pointer events while active |
| 2. Elements of the shown view (`UILayoutAnimator`, started by `ShowAnimated`) | each tagged element starts offset and slides back with iTween `easeOutQuart` while CanvasGroup alpha goes 0->1 linearly. Side per element: titles, top buttons, popup prompt panel: from **top**; scroll views/hub list, ACCEPT/CONFIRM buttons, ranking list: from **bottom**; story text (`GenerationEndingText`), name/village input: from **left**; story Continue button: from **right** | 2 s | `transform: translateY(-40vh)` (top) / `translateY(40vh)` (bottom, hub list ~80vh) / `translateX(-40vw)` -> 0, `cubic-bezier(0.25, 1, 0.5, 1)`; `opacity` 0->1 `linear`, 2 s. Offset 0.5 world units at plane distance 1 with a 60 deg FOV camera = ~43 % of screen height (1.0 unit for hub list / endings) - uncertain |
| 3. Popup open (`UIView.ShowAnimated` on ItemObtain, ItemObtainHUB, EndOfRound, EndOfGeneration, BiddingSystem) | reveal only: `TransitionAnimRev` (starts full black, ink recedes) + reveal sound + popup sound; prompt panel from top, button from bottom (row 2). Delays after the passage: 0.35 s setup, 1 s hub setup, 0.25 s end of round, 0.1 s end of generation/bidding | 0.5 s reveal, 2 s elements | same overlay, reveal phase only, starting fully covered |
| 4. Popup close (ACCEPT/CONFIRM) | instant `Hide()` after 0.1-0.3 s, no animation; next passage follows | 0.1-0.3 s | none |
| 5. Story passage -> story passage (web window -> window) | no transition: text replaced in place (same view) | 0 | none |
| 6. Hub rebuild (web hub -> hub) | n/a | - | none |
| 7. Special event overlay (`ViewSpecialEvent`) | full-screen vignette + "SPECIAL EVENT" title graphic with left/right lines (`SpecialEventText`, `SpecialEventLineLeft/Right`); CanvasGroup alpha 0->1 linear, hold, 1->0, then hidden; sound volume follows alpha. Has an ACCEPT button but hides itself after 4.5 s | 2 s in + 0.5 s hold + 2 s out = 4.5 s | `@keyframes` opacity 0 (0 %) -> 1 (44 %) -> 1 (56 %) -> 0 (100 %), 4.5 s linear, `pointer-events: none` |
| 8. Input popup for scenario values (`ViewPopupPanel`, `PopupAnimator.controller` trigger `OnEnable` -> `PopupAinm.anim`): web `InputPopup` from `FeverHeart`, `EnvPasscode`, `NewMasterName`, `WarriorAgility/Strength/Life/Name`, `ProgramName` | `Show()` (no ink, no sound). Alpha 0 until 0.167 s then -> 1 at 1.5 s; panel anchoredY 250 -> 100 (0.167 s) -> -396 (1.5 s) i.e. drops in from above; button -250 -> -130 -> 6 rises from below | 1.5 s | opacity + `translateY(-30vh)` -> 0 for the panel, `translateY(+20vh)` -> 0 for the button, 1.5 s ease-out |
| 9. Score entry / tie breaker (web `InputPopup` from `FinalScoring.ScoreEntry` / `TieBreaker2`) | full views `ViewScoreEntry` / `ViewTieBreaker` via `ChangeView` (row 1), no own sound | as row 1 | as row 1 (needs a flag to tell them from row 8, see 5.1) |
| 10. Pause / settings popup (`Settings UI.controller`) | `SettingsAnim.anim`: panel anchoredY +904 -> 0 (slides down), BG black alpha 0 -> 0.71. Close `PopupClose.anim`: panel -> +930, BG alpha 1 -> 0 (popup disabled after 0.2 s) | open 0.75 s, close 0.33 s | `Shared/PauseMenu.razor`: `translateY(-100vh)` -> 0 0.75 s, backdrop `rgba(0,0,0,0.7)` fade |
| 11. Confirmation popups ("go back?", "overwrite save?") (`Rewritedata UI.controller`) | `PopupInRewrite.anim`: panel anchoredY +1097 -> 0, BG alpha 0 -> 0.7. Close: `PopupOutRewrite` reversed, disabled after 0.2 s | 0.75 s | candidates: `Shared/SavePopup.razor`, `Shared/LoadGamePopup.razor` (no exact Unity counterpart) |
| 12. Setup page intros | `PlayersInitial` = `MFWPaper` `PlayerIntro.anim`: scale 0.85 -> 1 over 6.08 s, alpha 0 -> 1 over 1.82 s, continue button fades in 6.1-7.9 s (not clickable before). `PlayersWelcome` = `PLayerNamePaper` `PlayerNameIntro.anim`: scale 0.8 -> 1 and alpha 0 -> 1 over 4 s, button alpha 1.5-4 s. `PlayersVillageConfirm` = `VillageIntroPaper.anim`: scale 0.85 -> 1, alpha 0 -> 1, 3.5 s. Main menu logo `LogoAnim.anim`: loop 3 s, y 0 -> -12.6 -> 0 | see left | `transform: scale()` + `opacity` keyframes with the same timings |
| 13. Typewriter | story text uses Febucci TextAnimator typewriter only on the "Preparations" passages (`Preparations-TCOD`, `ScenarioBox-FOTU`, `ATOW-Preparations`; web `TheCostOfDisease.Init`, `FearOfTheUnknown.Init`), ~0.02-0.03 s per character (which component applies is uncertain) | - | optional |

Unity view <-> web element (for rows 1-3): `ViewGenerationEnding` (story) = `GameplayWindow`; `ViewGeneration` (hub) =
`GameplayHub`; `ViewItemObtain`/`ViewItemObtainHUB` = `GameplayPopup` Setup/SpecialSetup; `ViewEndOfRound` = popup
EndOfRound; `ViewEndOfGeneration` = popup EndOfGeneration; `ViewPopupPanel` = `GameplayInputPopup`; `ViewScoreEntry` /
`ViewTieBreaker` / `ViewRankingPage` = `FinalScoring.ScoreEntry` / `TieBreaker1`+`TieBreaker2` / `Rankings`; `ViewSpecialEvent`
= the special-event windows of 2.1; `ViewBiddingSystem` = none. Setup: `ViewMainMenu` = `Home`, `ViewGamePlayerCounts` =
`PlayersSelect`, `ViewPlayerIntro` = `PlayersInitial` + `PlayersWelcome`, `ViewPlayerNameEnter` = `PlayersNameEntry`,
`ViewEnterVillageName` = `PlayersVillage`, `ViewVillageIntro` = `PlayersVillageConfirm`, `ViewScenarios` = `PlayersScenario`;
`PlayersScenarioLanguage` and `MirrorPopup` have no Unity counterpart (`ViewAudioTrack` = VO choice: the Narration select on
`PlayersScenarioLanguage`, section 8).

## 5. Hook design and suppression

### 5.1 Recommendation

1. **Cues live on the UI-model object, not in a GlobalData queue.** Add `SoundCue Cue` to `GameplayWindow` (set by scenario
   code only for the special-event windows of 2.1, e.g. `globalData.ActiveWindow.SetCue(SoundCue.SpecialEvent)`) and store the
   `PopUpTitle` on `GameplayPopup` so its sound (setup / end of round / end of generation) is derived, not edited into ~250
   call sites. A final-ending flag on the window (`TheCostOfDisease.FinalEnding`, `FearOfTheUnknown.FinalEnding`) drives the
   end-game music. A flag on `GameplayInputPopup` (set in `FinalScoring.ScoreEntry` / `TieBreaker2`) can select row 9 vs row 8.
   Why: mirror mode already snapshots and restores `ActiveHub/Window/Popup/InputPopup` (`CaptureMirrorState` /
   `RestoreMirrorState`), so objects built in an aborted or sandboxed run are thrown away with their cues for free. A queue on
   `GlobalData` would need its own rollback in `RestoreMirrorState`, guards in `SimulateMirror`, `Undo`, `LoadGameAsync`, and
   draining rules.
2. **`Pages/Gameplay.razor` is the only consumer.** It keeps the last presented object (popup / input popup / window / hub
   reference) and the last base screen kind (window or hub; popups overlay it). After a render where the reference changed:
   base kind changed -> row 1 transition + cover/reveal sounds; a `GameplayPopup` appeared -> row 3 + popup sound by
   `PopUpTitle`; new window with a cue -> play it; hub -> hub or window -> window -> nothing (click only). Render from the
   presented reference while the cover phase runs, if the full Unity cover/hold/reveal is wanted (otherwise reveal only).
3. **Origin flag in `Gameplay.razor`**: `GoBack` (undo) and the first render (`OnInitializedAsync`, i.e. load / reload /
   arrival from `StartScenario`) only record the new reference without sound or transition. Every other change comes from
   a user click (`InvokeWindowCallback`, `InfoPopup`, `InputPopup`, `MirrorPopup` answers) and plays.
4. **Click sounds play directly in the click handlers** (before `RunScenarioAction`), so they are never replayed.
5. **Music is declarative and idempotent**: a single place (e.g. `Layout/MainLayout.razor` on route change + `Gameplay.razor`
   after render) computes the wanted track - route `/` and setup pages -> title theme; `/Gameplay` -> theme by `ScenarioId`,
   or end-game track when the shown window has the final-ending flag - and the JS side only switches when it differs from the
   playing track (as Unity compares clip names). Load, undo, refresh and mirror replays then never restart music. (Undo from
   the final ending back to Rankings would return to the scenario theme - Unity has no undo, so this is a choice.)
6. A small `wwwroot/js/audio.js` with the Unity channels: music (loop), effects (single, cut previous), popup (single, cut
   previous, volume envelope for the special event), click (overlapping). Music and effects volume in `localStorage`.

### 5.2 Where a sound must not play (or must be deferred)

| Place | What happens | Needed handling |
|---|---|---|
| `Shared/GlobalData.Mirror.cs:RunScenarioActionCore` | first `action()` can throw `MirrorQuestionException` after building screens; `RestoreMirrorState` restores the old objects | object cues: nothing to do; a GlobalData queue would need rollback here |
| `GlobalData.Mirror.cs:SimulateMirror` (via `BuildMirrorQuestion` / `PreviewMirror`) | runs the action 2x per option with `IsMirrorSimulating = true` | never play; object cues are discarded by `RestoreMirrorState`; any direct JS call from scenario code would fire here - forbid it |
| `GlobalData.Mirror.cs:AnswerMirrorQuestion` / `RollMirrorQuestion` / the "value changes nothing" branch | real run of the stored `Retry`; may raise the next question | play only what is presented after the final successful run (falls out of 5.1.2) |
| `GlobalData.Mirror.cs:RefreshShownHub` | rebuilds the hub after every action ending on the hub | hub methods and `GameplayHubSection` builders must never cue (they also run while sections are hidden); hub appearance is derived from the base-kind change |
| `Shared/GlobalData.cs:Undo` | restores JSON, re-invokes `GetHubMethod()` and the saved screen method under `_UndoInProgress` | suppress (origin flag set in `Gameplay.razor:GoBack`); with an open mirror question it only closes the question |
| `GlobalData.cs:LoadGameAsync` (from `Gameplay.razor:OnInitializedAsync`, `Home.razor:OnContinue`, `LoadGamePopup.razor:LoadGame`) | re-invokes the hub method | no SFX; music starts from state |
| `GlobalData.cs:StartScenario` (from `PlayersScenarioLanguage.razor:SelectLanguage`) | runs `Init` before navigating to `/Gameplay` | page transition + scenario music; `Init` screens carry no cue |
| `Shared/PauseMenu.razor:OnLanguageChanges` / `OnScenarioLanguageChanges` | `NavigateTo(Nav.Uri, true)` = full reload -> load path | treated as load; audio must be unlocked again by the next gesture |
| `Gameplay.razor` re-renders without a screen change (`SaveGameAsync` 3 s notification, pause/save popups, `Refresh`) | same references | compare by reference so nothing replays |
| Scenario screen methods (all `SaveToUndo` callers) | re-invoked by undo/load | set cues only on the object they build; never call audio directly |

## 6. Clips to copy (needed) and constraints

| Clip (source, under `UnityOriginalApp/Assets/`) | Size | Length | Use |
|---|---|---|---|
| `New SFX/click-to-continue.ogg` | 32 KB | 0.84 s | click |
| `New SFX/UI_screen transitions.ogg` | 66 KB | 1.74 s | screen cover |
| `New SFX/UI_screen transitions-reverse tear.ogg` | 37 KB | 0.86 s | screen/popup reveal |
| `SFX/New_8_April/set up window-new.ogg` | 36 KB | 1.92 s | setup popup |
| `New SFX/Fathers work SFX pt 2/End of round.ogg` | 49 KB | 2.62 s | end of round / generation |
| `New SFX/Fathers work SFX pt 2/Special Event-words appear.ogg` | 209 KB | 12.25 s | special event (Unity fades it out after ~4.5 s) |
| `SFX/New_8_April/welcome to my fathers work-window.ogg` | 108 KB | 6.24 s | app start |
| `SFX/Enter room-footsteps.ogg` | 126 KB | 3.79 s | New Game |
| `SFX/Fathers work Title theme.ogg` | 2,260 KB | 119 s | menu/setup music |
| `SFX/New_8_April/My Fathers Work-OST/Chronicle Part one_1-2.ogg` | 3,367 KB | 205 s | Cost of Disease music |
| `SFX/New_8_April/My Fathers Work-OST/Chronicle Part Two_2-2.ogg` | 3,710 KB | 214 s | Fear of the Unknown music |
| `SFX/New_8_April/My Fathers Work-OST/Chronicle Part  Three.ogg` | 852 KB | 52 s | end of game (once) |
| Only with a bidding screen: `New SFX/Fathers work SFX pt 2/Set up-window appear.ogg`, `.../Reveal.ogg` | 129 + 147 KB | 7.1 s, 8.7 s | bidding open, countdown |
| Later (A Time of War): `.../My Fathers Work-OST/Cost of disease theme loop-louder.ogg` | 2,634 KB | 173 s | ATOW music |
| Optional visual: `New_UI_Assets/ScreenTransitions/Transition 100-121.png` | ~1 MB (22 x 2048x2048) | - | exact ink frames (downscale before use) |

Total for the 12 required clips: ~10.9 MB (music 10.2 MB). All are Ogg Vorbis, stereo, 44.1/48 kHz.

Constraints:
- `*.ogg`/`*.png` under the web app resolve to Git LFS (`git check-attr` says `filter: lfs` for `wwwroot/audio/*.ogg`); a
  worktree without `git lfs pull` gets pointer files.
- Browser autoplay: nothing can play before the first user gesture (the cookie banner click or any click). The welcome sound
  and title theme on a cold start must wait for that gesture (or be skipped); the same after the full reloads in 5.2.
- Load music lazily (`preload="none"` / on demand); effects can be preloaded (~0.5 MB).
- Safari / iOS support for Ogg Vorbis in `<audio>` varies by version - uncertain; an AAC/MP3 fallback may be needed for
  iPad players.
- The static files are served from a build-time snapshot (see CLAUDE.md); new files under `wwwroot` need a rebuild.

## 7. Open questions for the orchestrator / owner

1. Bidding/voting: add a bidding screen (Start -> 3-2-1 -> "reveal", `Set up-window appear` + `Reveal` sounds) or leave the
   web links as they are (then those two clips are not needed)?
2. Play any of the never-wired clips (2.3), e.g. Medieval Battle for the Battledome or Mayor selected for the mayor choice?
3. Transition length: copy Unity (1.5 s per view change + 2 s element ease-in, 0.5 s reveal for each popup) or shorten?
4. Where do music/effects volume (or mute) controls go (Unity: main-menu Settings and Pause popups)?
5. Undo from the final ending: resume the scenario theme (recommended) or keep the end-game track?
6. Mirror question popup: no sound (no Unity counterpart) or reuse the popup reveal?

## 8. Voice-over

### 8.1 How the Unity app plays it

- Data: `Scripts/ScriptableObject/VOAudio.asset` (`VOAudioFileScriptable`: `MaleAudioDatas` / `FemaleAudioDatas`, each a list of
  `PassageName` + clip), referenced by `PassageTracker.VOAudioFile` in `Main.unity`.
- Trigger: `PassageTracker.OnPassageStatus` runs on **every passage enter** (`Story.OnPassageEnter`): first `s_OnStopVO`, then
  0.1 s later `OnPlayVOAudioFile(passageName)`, which looks the passage up in the list of the chosen voice and, if found, raises
  `s_OnPlayVO(clip)`. Voice: PlayerPrefs `AudioTrack`, 0 = male (default), 1 = female.
- Playback (`SoundManager.OnVOPlayOneTime` -> `Delay`) on `VOSource` (volume 1, music not ducked): plays
  `SFX/New_8_April/Thunder-vox story.ogg` (8.3 s file), after **4.5 s** stops it and plays the narration clip once. It also
  starts `ViewGenerationEnding.S_OnScroll(clip length)`: the story text auto-scrolls from 7.5 s on (not ported).
- Stop rules:
  - every passage enter (`OnVOStop`): the next story passage, a hub passage, a passage that only shows a setup popup, the
    story Back/Forward buttons (they enter a passage - a narrated one then plays again with the lead-in), Continue/load;
  - pause button of the story or hub view: `OnVOPause` (pauses clip or lead-in); the pause popup's confirm button:
    `OnVOResume` (during the lead-in it skips straight to the clip, otherwise unpauses); Go to Main: `OnVOStop`;
  - nothing else stops it: end-of-round popups, setup popups and the special-event overlay are not passages. **None of the
    narrated passages has a setup block of its own** (no `setupStyleEvnt`; all end with a link to the next passage), so VO never
    continues under a setup popup: such a popup belongs to the next passage, whose enter stops the VO.
  - Volume: the effects slider (`AudioListener.volume`, master) scales it; the music slider does not.
- Voice choice: `ViewAudioTrack` (`UI/VoiceTrack`, shown after New Game): Male / Female toggles and Continue, preselected from
  `AudioTrack`, so **Male by default**. Also `ViewMainMenu.OnChangeVoiceTrack` (main-menu Settings, `VoiceSetting` left/right
  arrows). There is no "off" (only the master volume slider).
- Unity plays the English recordings for every story language (no language check).

### 8.2 Web app

- Scenario code only marks a narrated window: `globalData.ActiveWindow.SetVoiceOver("<Unity passage name>")`
  (`GameplayWindow.VoiceOver`). Mirror sandbox runs discard their windows, so they never narrate.
- `Pages/Gameplay.razor` (`UpdateVoiceOver`, after `DetectScreenChange`): a narrated window newly presented by a click (not
  `Suppressed`), narration not Off and audio unlocked -> `AudioService.VoicePlay(clip, voice, leadIn: true)`, plus the indicator
  pill under the title. `wwwroot/js/audio.js` channel `voice`: one element, thunder for 4.5 s, then
  `audio/vo/<female|male>/<clip>.mp3`; the music is ducked to 30 % from the lead-in until the narration ends or stops.
- Stops: anything else presented (popup, input popup, another window, hub, mirror question), undo (`GoBack`), leaving
  `/Gameplay` (`Dispose`), any change of the Narration setting. Pause menu: pause / resume as in Unity (resume during the
  lead-in skips to the clip).
- Deliberate deviations: undo, load and reload never autoplay (the pill offers Play / Replay); a narrated window shown before
  the first user gesture does not play (browser autoplay rule, pill shows Play); an Off setting exists (pill "Narration
  available" + Play, which plays the female voice without the lead-in); Play / Replay in the pill play the clip without the
  lead-in; no auto-scroll; VO volume is independent of the music and effects toggles.
- Setting: `localStorage` `audio.voice` = `female` | `male` | `oliver` (8.4) | `off`, default `oliver` (owner, 30 Oct 2026; Unity defaulted to male). UI: Narration select on
  `Pages/PlayersScenarioLanguage.razor` and in `Shared/PauseMenu.razor`.

### 8.3 Passage -> web window -> clips

Clips under `UnityOriginalApp/Assets/SFX/`, resolved by GUID from `VOAudio.asset`. Web files
`wwwroot/audio/vo/female/<Passage>.mp3` and `wwwroot/audio/vo/male/<Passage>.mp3` (mono, 64 kbps, 44.1 kHz). Lengths female /
male in seconds.

| Passage (Unity) | Web method | Female clip | Male clip | Notes |
|---|---|---|---|---|
| `Scenario5Start` | `TheCostOfDisease.ScenarioStart` (00_Setup) | `female Takes VO oggs/Scenario5Start.ogg` | `Male VO oggs/Scenario-5-start.ogg` | Gen I intro "Yellow Fever". 84 / 98 s |
| `DevastationIntro` | `TheCostOfDisease.DevastationIntro` (02) | `female Takes VO oggs/DevastationIntro.ogg` | `Male VO oggs/DevestationIntro.ogg` | 100 / 109 s |
| `Hospitalintro` | `TheCostOfDisease.HospitalIntro` (03) | `female Takes VO oggs/Hospitalintro.ogg` | `Male VO oggs/HospitalIntro.ogg` | 103 / 113 s |
| `GloomyHunterIntro` | `TheCostOfDisease.GloomyHunterIntro` (04) | `female Takes VO oggs/GloomyHunterIntro.ogg` | `Male VO oggs/GloomyHunterIntro.ogg` | 111 / 132 s |
| `GloomyWolvesIntro` | `TheCostOfDisease.GloomyWolvesIntro` (04) | `Male VO oggs/GloomeyWolvesIntro-The Order Revealed.ogg` | `Male VO oggs/GloomyWolvesIntro.ogg` | The female entry points into the **male** folder. Uncertain, but very likely a female take: median pitch ~145 Hz (female takes ~151 Hz, male ~113-118 Hz) and 14 % shorter than the male take, like the other female takes. 120 / 139 s |
| `ProsperityHunterIntro` | `TheCostOfDisease.ProsperityHunterIntro` (05) | `female Takes VO oggs/ProsperityHunterIntro.ogg` | `Male VO oggs/ProsperityHunterIntro.ogg` | 113 / 133 s |
| `ProsperityWolvesIntro` | `TheCostOfDisease.ProsperityWolvesIntro` (05) | `female Takes VO oggs/ProsperityWolvesIntro.ogg` | `Male VO oggs/ProsperityWolvesIntro.ogg` | 108 / 132 s |
| `NoUniversityIntro` | `TheCostOfDisease.NoUniversityIntro` (06) | `female Takes VO oggs/NoUniversityIntro.ogg` | `Male VO oggs/NoUniversityIntro.ogg` | 91 / 104 s |
| `UniversityIntro` | `TheCostOfDisease.UniversityIntro` (07) | `female Takes VO oggs/UniversityIntro.ogg` | `Male VO oggs/UniversityIntro.ogg` | 72 / 85 s |
| `FearoftheUnknownStart` | `FearOfTheUnknown.ScenarioStart` (00_Setup) | `female Takes VO oggs/FearoftheUnknownStart.ogg` | `Male VO oggs/FearoftheUnknownStart.ogg` | Gen I intro. The letter header names a random player; what the recording says there is unverified. 108 / 120 s |
| `ForeignIntro` | `FearOfTheUnknown.ForeignersIntro` (03) | `female Takes VO oggs/ForeignIntro.ogg` | `Male VO oggs/ForeignIntro.ogg` | 77 / 86 s |
| `CreatureIntro` | `FearOfTheUnknown.CreatureIntro` (04) | `female Takes VO oggs/CreatureIntro.ogg` | `Male VO oggs/Creature intro-2.ogg` | 80 / 89 s |
| `CreatureDemands` | `FearOfTheUnknown.CreatureDemands` (04) | `female Takes VO oggs/CreatureDemands.ogg` | `Male VO oggs/CreatureDemands.ogg` | Follows `CreatureIntro` directly (two narrated windows in a row). 61 / 74 s |
| `IsolationIntro` | `FearOfTheUnknown.IsolationIntro` (06) | `female Takes VO oggs/IsolationIntro.ogg` | `Male VO oggs/IsolationIntro.ogg` | 76 / 87 s |
| `TensionIntro` | `FearOfTheUnknown.TensionIntro` (07) | `female Takes VO oggs/TensionIntro.ogg` | `Male VO oggs/TensionIntro.ogg` | 81 / 89 s |
| `PrivatizedIntro` | `FearOfTheUnknown.PrivatizedIntro` (08) | `female Takes VO oggs/PrivatizedIntro.ogg` | `Male VO oggs/PrivatizedIntro.ogg` | The web app shows 9 August (deliberate date fix); whether the recording reads the date is unverified. 81 / 91 s |
| `LiberalIntro` | `FearOfTheUnknown.LiberalIntro` (09) | `female Takes VO oggs/LiberalIntro.ogg` | `Male VO oggs/Liberal-intro.ogg` | 82 / 88 s |

Lead-in: `SFX/New_8_April/Thunder-vox story.ogg` -> `wwwroot/audio/sfx-vo-thunder.mp3` (mono, 96 kbps; only the first 4.5 s are
heard).

A Time of War:

| Passage (Unity) | Web method -> `SetVoiceOver` clip | Female clip | Male clip | Notes |
|---|---|---|---|---|
| `ATimeofWarIntro` | `ATimeOfWar.ScenarioStart` (00_Setup) -> `ATimeofWarIntro` | `female Takes VO oggs/ATimeofWarIntro.ogg` | `Male VO oggs/ATimeofWarIntro-2.ogg` | 127 / 144 s |
| `IntroMartial-Monarchists` | `ATimeOfWar.IntroMartialMonarchists` (02) -> `IntroMartial-Monarchists` | `female Takes VO oggs/IntroMartial-Monarchists.ogg` | `Male VO oggs/IntroMartial-Monarchist.ogg` | 101 / 110 s |
| `IntroMartial-Separatists` | `ATimeOfWar.IntroMartialSeparatists` (02) -> `IntroMartial-Separatists` | `female Takes VO oggs/IntroMartial-Separatists.ogg` | `Male VO oggs/IntroMartial-Separatists.ogg` | 96 / 102 s |
| `MonarchReignIntro` | `ATimeOfWar.MonarchReignIntro` (05) -> `MonarchReignIntro` | `female Takes VO oggs/MonarchReignIntro.ogg` | `Male VO oggs/MonarchReignIntro.ogg` | 75 / 87 s |
| `WarningIntro` | `ATimeOfWar.WarningIntro` (07) -> `WarningIntro` | `female Takes VO oggs/WarningIntro.ogg` | `Male VO oggs/Paradox Intro Warning Intro.ogg` | 125 / 139 s |
| `ParadoxIntro` | `ATimeOfWar.ParadoxIntro` (08) -> `WarningIntro` | same as `WarningIntro` | same as `WarningIntro` | Unity maps both passages to one clip per voice; no separate file. The texts open identically ("A Modern Future" vs "A Brighter Future" heading); whether they diverge later in the recording is unverified |
| `PeaceIntro` | `ATimeOfWar.PeaceIntro` (06) -> `PeaceIntro` | `female Takes VO oggs/PeaceIntro.ogg` | `Male VO oggs/PeaceIntro.ogg` | 82 / 89 s |
| `WarningEvent` | `ATimeOfWar.WarningEvent` (07) -> `WarningEvent` | `female Takes VO oggs/WarningEvent.ogg` | `Male VO oggs/WarningEvent.ogg` | 82 / 96 s |
| `TimeTravelintro1` | `ATimeOfWar.TimeTravelIntro1` (04) -> `TimeTravelintro1` | `female Takes VO oggs/TimeTravelintro1.ogg` | `Male VO oggs/TimeTravelintro1.ogg` | 70 / 80 s |
| `TimeTravelIntro2` | `ATimeOfWar.TimeTravelIntro2` (04) -> `TimeTravelIntro2` | `female Takes VO oggs/TimeTravelIntro2.ogg` | `Male VO oggs/TimeTravelintro2.ogg` | 61 / 68 s |
| `TTIntro3` | `ATimeOfWar.TTIntro3` (04) -> `TTIntro3` | `female Takes VO oggs/TTIntro3.ogg` | `Male VO oggs/TTintro3.ogg` | 50 / 55 s |

All 11 passage names exist in `ATimeOfWar.txt`. Clip names are case-sensitive on GitHub Pages.

Size: 34 clips, female 12.4 MB + male 14.2 MB + thunder 0.1 MB = 26.6 MB; A Time of War adds 20 clips (female 6.6 MB + male 7.4 MB), loaded on demand (`preload="none"`).

Open points: the recordings read the English source text, so with another scenario language (`English_Simple`,
`English_Short`, Italian) the narration differs from the text on screen (Unity also plays English VO for every language).

### 8.4 Fan-made Oliver narration (not in the Unity app)

Voice setting **Oliver** (`NarrationVoice.Oliver`, `audio.voice` = `oliver`): an AI narration of all story text (ElevenLabs
library voice "Oliver Silk - Deep Gravel Narrative", model `eleven_v4`, stability 0.8, similarity 0.8, clone strength 0.5
(new in v4, separate from similarity; clips rendered before 1 Nov 2026 used the API default); v4 ignores style, speed
and speaker boost). Female and Male play the original recordings only (8.2/8.3).

- Clips: `tools/narration/chunks.py` takes every window's story text (tags of `<Scenario>_Localization.csv` shown with
  `AddDefaultContent`, `AddNextContent*` or `AddStoryContent`), cuts it at `<b>CHOOSE` and splits it around its
  placeholders. Each static chunk is one clip `wwwroot/audio/vo/oliver/<Scenario>/<fnv64 of the chunk's CSV text>.mp3`
  (96 kbps mono; 192 kbps masters stay offline in `tools/narration/out/master/oliver/`). A placeholder's formatted value
  (an either/or variant, an NPC or place name, a number; `scripts/<Scenario>/values.txt`) has its own clip named after
  the value text; player names, the town name and typed names have none and are skipped. Headers such as "Letter from
  <name> - <date>" are read as "A letter." (`chunks.HEADERS`). Changed text gets a new hash, so only it is rendered again.
- `wwwroot/audio/vo/oliver/<Scenario>.json`: `clips` (hash -> script hash, `""` = silent chunk; the script hash is the
  url's `?v=` so a re-render is not cached) and `private` (tags read by one player alone).
- App: `GameplayWindow.StoryTexts` keeps each story tag with its formatted text. `Shared/Narration.cs` repeats the split
  (same regex, cut and hash) and lines the CSV chunks up with the formatted text to find the values; a window is
  narrated only when every speaking chunk has a clip, otherwise Oliver plays the original male recording (8.3) if the
  window has one. `audio.js` `voicePlayList` plays the clips in order (next clip preloaded), without the thunder lead-in.
- Private sections (targets of `AddDoNotOtherPlayersToSee` / `AddOnceYouAreReady`, plus `scripts/<Scenario>/private.txt`)
  are narrated but never autoplayed: the pill offers Play. `scripts/<Scenario>/skip.txt` lists windows that are not
  narrated at all (input forms, pick-a-player screens, the battle).
- Hand-written overrides: `scripts/<Scenario>/<Tag>.txt` (the former v3 intro scripts), see `chunks.read_overrides`.

State (27 Oct 2026): Fear of the Unknown fully rendered (509 clips); Cost of Disease and A Time of War not rendered yet.
Stand-ins for typed-in values (player names -> "a cousin", village -> "the village", ...): `chunks.STAND_INS`.
