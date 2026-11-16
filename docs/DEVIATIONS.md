# Deliberate deviations from the official app

Where this app knowingly differs from the official *My Father's Work* app, per scenario: bugs in the original stories
that were fixed, ones that were kept as the official app shipped them, and open questions. Read the scenario's section
before changing its code. A Time of War's full list of source bugs, with the owner's open questions, is
[`ATOW_PORT_MAP.md`](ATOW_PORT_MAP.md) section 7.

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
- Battle: only alive warriors fight, the last one standing wins; stalemate / 250-step guard -> most HP. Warriors entered with 0 Life never fight (rules line on The Main Event); with one Warrior left it wins at once, with none nobody gains the 9VP. The official battle is buggy, so in mirror mode the battle can differ.
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
- Ties follow the rulebook (End of the Game): most points, then completed Masterwork, then most Estate Upgrades, else the family shares the win. Since Nov 2026 the Masterwork is ticked per player at score entry (`ScenarioVars.Masterworks`, `GameplayInputPopup.FieldToggle`); the old "Did X complete their Masterwork?" screen (`TieBreaker1`) is gone. Tie-breakers pick the winner without changing scores (the original app added +1 / the entered number to the score). Rankings (shared `FinalScoring`) give equal players the same place: the score, then Masterwork, then Estate Upgrades (when asked) separate them, so a shared win shows several "1st".
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
- Not ported: sharing, 5-player games. The Endings Gallery replaces the original endings and achievements lists (see below).
