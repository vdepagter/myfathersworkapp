<p align="center">
  <img src="MyFathersWorkApp/MyFathersWorkApp/wwwroot/images/logo.png" alt="My Father's Work" width="300" />
</p>

# My Father's Work App (Unofficial)

A free, fan-made companion app for the board game *My Father's Work* by Renegade Game Studios. It runs in any modern
browser on a phone, tablet or computer, with nothing to install.

## Play now

There are two ways to play:

1. **In your browser: https://vdepagter.github.io/myfathersworkapp/**
   Open the link on a phone, tablet or computer. Nothing to install, and it always runs the latest version. You can
   also add it to your home screen, where it opens full screen like an app (see
   [Add it to your home screen](#add-it-to-your-home-screen) below).
2. **Offline on Windows:** download `MyFathersWork-Windows-<version>.zip` from the
   [latest release](https://github.com/vdepagter/myfathersworkapp/releases/latest), unzip the whole folder anywhere and
   double-click `MyFathersWork.exe`. Short on space or bandwidth? The `-light.zip` has smaller audio files (narration
   at a lower quality) and the `-no-audio.zip` leaves out music, sound and narration altogether. The app opens in its own window and needs no internet connection and no
   installation. Saved games are kept on that PC and stay when you unzip a newer version over the old one. The exe is
   not code-signed yet, so Windows SmartScreen may warn about an unknown publisher the first time: choose
   **More info > Run anyway**.

### Add it to your home screen

The website can be installed as an app (a PWA): it gets its own icon and opens full screen, without the browser's
address bar. Where the browser offers it, the Main Menu shows an **Install app** button (on iPhone and iPad it explains
how). After the first visit it also works **without an internet connection**: the app itself is kept on the
device automatically. Narration is large, so it plays from the internet unless you download it: **Settings > Play
offline** (Main Menu) downloads Oliver per scenario (about 110-140 MB each) or the original voices (about 20 MB each).
Downloads stay through app updates, and only changed clips are fetched again. When a new version is out, the app
shows **A new version of the app is ready** with an **Update** button (a game in progress reopens on its screen).

| Device and browser | How |
|---|---|
| **iPhone / iPad, Safari** | Tap the **Share** button (the square with an arrow), scroll down, tap **Add to Home Screen**, then **Add**. |
| **iPhone / iPad, Chrome, Edge or Firefox** | Tap the **Share** button (in the address bar or the menu), then **Add to Home Screen** and **Add**. Needs iOS 16.4 or later. |
| **Android, Chrome** | Tap the **⋮** menu, then **Add to Home screen** (or **Install app**), then **Install** or **Add**. |
| **Android, Edge** | Tap the **…** menu, then **Add to phone** (or **Install app**). |
| **Android, Firefox** | Tap the **⋮** menu, then **Add to Home screen** (or **Install**), then **Add**. |
| **Android, Samsung Internet** | Tap the **≡** menu, then **Add page to** > **Home screen**. |
| **Windows / Mac, Chrome** | Click the install icon at the right of the address bar, or **⋮** > **Cast, save and share** > **Install page as app**. |
| **Windows / Mac, Edge** | Click the install icon at the right of the address bar, or **…** > **Apps** > **Install this site as an app**. |
| **Mac, Safari** (macOS 14 or later) | **File** > **Add to Dock**. |

Firefox on Windows and Mac cannot install websites as apps; use a bookmark instead. Menu names change a little between
browser versions, so look for "Install" or "Add to Home screen".

Saved games and the Endings Gallery are kept per browser. An installed app can keep its own data apart from the
browser tab (on iPhone and iPad it always does), so a game started in the browser does not show up in the app. To move
a game, press **Save code** next to it under **Saved Games** (Main Menu), then open that file in the app under
**Saved Games > Load a save code**. To move your endings, achievements and Hall of Fame, press **Export** on the
Endings Gallery page, then **Import** that text or file in the app: the two galleries are merged.

## Why this exists

The official companion app is no longer available in the app stores. An
[official web app](https://renegadegamestudios.com/playmyfatherswork) still exists, but it is no longer maintained.

Renegade made the source of the original app publicly available for projects like this one. This app converts that
source into a new web app, fixes the issues found in the original stories, and adds features and extra narration.

This repository is a fork of [Deusald's fan-made companion app](https://github.com/Deusald/MyFathersWork-FanMadeCompanionApp),
which ported the first two Generations of *The Cost of Disease*. Starting from there, this fork:

- checked those existing ports passage by passage against the original source and filled the gaps it found,
- ported the rest: *The Cost of Disease* Generation III, *Fear of the Unknown* and *A Time of War*, each with scoring,
  tie-breakers and all 8 endings.

## Scenarios

| Scenario | Status |
|---|---|
| **The Cost of Disease** | ✅ Complete, played at the table, narrated by Oliver |
| **Fear of the Unknown** | ✅ Complete, played at the table, narrated by Oliver |
| **A Time of War** | ✅ Complete, played at the table, narrated by Oliver |

## Features

**Everything the official app did**
- The full original story text of all three scenarios, for 2 to 4 players
- Setup instructions with pictures, round screens with the active rules, and the end-of-round and end-of-generation steps
- The vote and bid countdown, final score entry, tie-breakers, rankings and all endings
- The original look: the manor main menu, parchment story pages, ink-blot page transitions, rain and embers
- Music, sound effects and the original male and female voice recordings
- The Log Book of events you have played through, now for every scenario (the official app had one for *The Cost of
  Disease* only)

**Added in this app**
- **Oliver**, a narrator who reads all story text aloud in all three scenarios. The sentence he is speaking is
  highlighted and the page scrolls along with him; the pause menu lets you choose how the page follows (just before the
  next sentence, continuously, or a screen at a time) and how the sentence is marked. Private pass-the-device pages only
  play when the reader presses play.
- **Clear buttons** to move on: every story page ends in a green button (Continue, Reveal, Start Voting, Final Scoring,
  ...), and every round screen ends with an **End of Round** button. While that button is further down a long page, a
  shortcut chip (↓ Continue, ↓ End of Round) jumps down to it.
- **Votes and bids** with the official app's 3-2-1 countdown: **Start Voting** (or **Start Bidding**) opens it, and
  **Accept** after REVEAL! goes straight on to the result.
- **Back (⬅) and Forward (➡)**: undo any step, all the way to the start of the game, and redo what you undid.
- **Auto-save and Saved Games**: close the page and continue later on the exact same screen. Starting a new game keeps
  the old one, so every game on the device can be opened again and paged back through.
- **Save codes and gallery export**: download any game as a file from Saved Games and load it on another device to
  continue there. The Endings Gallery and Hall of Fame are kept apart from saved games: **Export** and **Import** on
  the Endings Gallery page copy them to another device or keep a backup (an import is merged with what is there).
- **Endings Gallery**: a map of every scenario's branches and endings showing which you have reached, achievements (the
  official app's and new ones for Masterworks, high scores, family wins and win streaks), and a **Hall of Fame** with
  wins, records and recent games you can correct, add or delete.
- **Rankings podium** at the end of a game, and score entry on one page where you tick who completed their Masterwork.
- **Remembered names**: players and village names from earlier games can be picked with one tap.
- **Village name secrets**: a few village names decide which way the story goes after Generation I (listed in Help).
- **Guided mode (spoilers!)**, for hunting every ending: a route card on each round screen and notes on the choices
  that steer the story show which votes, bids and links lead to which branch or ending, which endings you have already
  found, and the odds of the hidden rolls (without revealing them). Switch it on in Settings or the pause menu.
- **Play along with the official app**, a tester option in Settings: if someone else runs the official app, this app
  asks for its random outcomes so both stay in step.
- **Help** pages that explain setup, the round screens, secret pages, bids and votes.
- **Settings** (also in the pause menu during a game) for the narrator (Oliver, the original male or female
  recordings, or off), how the page follows Oliver and how the spoken sentence is marked, graphics quality, story text
  size and separate volumes for music, effects and speech.
- **An offline Windows version** (see Play now).
- Many fixes to the original stories: unreachable or always-true conditions, rules that contradicted each other,
  missing and duplicated text. Each deviation is written down in [`docs/DEVIATIONS.md`](docs/DEVIATIONS.md).

What changed in each version: [`CHANGELOG.md`](MyFathersWorkApp/MyFathersWorkApp/wwwroot/CHANGELOG.md), also in the
app under **Help > What's new**.

## Still to do

- [x] Port all three scenarios with scoring and all endings
- [x] Play every scenario at the table
- [x] Oliver's narration for all three scenarios
- [x] Offline Windows version
- [ ] Test on iPhone and iPad (Safari)
- [ ] Code-sign the Windows exe, the last step before version 1.0

## Feedback and bug reports

Found a bug, a typo in a story, or have an idea? Please let us know on GitHub:

1. Sign in to GitHub (creating an account is free).
2. Go to the [Issues page](https://github.com/vdepagter/myfathersworkapp/issues) and press **New issue**.
3. Give it a short title and describe what happened. These details help a lot:
   - the scenario, Generation and screen you were on,
   - what you expected and what happened instead,
   - your device and browser (for example "iPhone, Safari" or "Windows laptop, Chrome"),
   - a screenshot, if you can. You can paste or drag it straight into the issue.
4. Press **Create** (or **Submit new issue**).

If your report is about a game in progress, a saved game helps us see exactly where you were: go to **Saved Games** on
the Main Menu, press **Save code** next to the game and drag the downloaded file into the issue.

No GitHub account? You can also ask in the **My Father's Work Fanmade App** Discord (link below).

## To test locally for development purposes

You only need this if you want to change the app. To play, use one of the options under [Play now](#play-now).

1. **Install the .NET SDK.** Go to https://dotnet.microsoft.com/download, download the **.NET 9 SDK** (not just the
   runtime) for your system and run the installer. To check it worked, open a terminal (on Windows: press Start, type
   `cmd` and press Enter) and type `dotnet --version`. It should print a number starting with 9 or higher.
2. **Install Git and Git LFS.** Get Git from https://git-scm.com/downloads (the Windows installer includes Git LFS).
   On macOS or Linux, also install Git LFS from https://git-lfs.com/. Then run `git lfs install` once in a terminal.
   The images and audio are stored with Git LFS, so without it the app shows broken pictures and plays no sound.
3. **Download the app.** In the terminal, go to the folder where you want it (for example `cd Documents`) and run:
   ```
   git clone https://github.com/vdepagter/myfathersworkapp.git
   cd myfathersworkapp
   ```
   (The green **Code > Download ZIP** button on GitHub does not include the images and audio, so use `git clone`.)
4. **Start it.** In that same `myfathersworkapp` folder, run:
   ```
   dotnet run --project MyFathersWorkApp/MyFathersWorkApp/MyFathersWorkApp.csproj
   ```
   On Windows you can instead double-click `launch.bat`. The first start takes a minute.
5. **Open it** at http://localhost:5166 in your browser. Press Ctrl+C in the terminal to stop it.

To get the latest version later, run `git pull` in the `myfathersworkapp` folder.

**Test on a phone or tablet** on the same Wi-Fi network (needs the **.NET 10 SDK**):
```
dotnet build MyFathersWorkApp/MyFathersWorkApp.sln -p:DebugType=none -p:DebugSymbols=false
dotnet run tools/lan-serve.cs
```
It prints the addresses to open on the phone (port 5199; pass another port as the last argument). Windows Firewall has
to allow the port on the private network once; the command is at the top of `tools/lan-serve.cs`. Changes to JavaScript,
CSS and the story `.csv` files show on a reload, C# changes need the build again.

**Build the offline Windows version** with `pwsh tools/build-windows.ps1 -Version <version>`: the full, light and
no-audio zips land in `dist/` (the light one needs `ffmpeg`; `-Edition Full` builds only one). Pushing a tag `v*` makes
GitHub build all three and attach them to a release.

## How the scenarios were ported

1. **Source:** the official app was built in Unity, and its stories were written in Twine and compiled to C#. The
   decompiled story code of all three scenarios (in [`source/`](source)) is the starting point.
2. **Make it readable:** helper scripts in `tools/` turn that code into readable passages with their original link texts.
3. **Map the flow:** follow the links from each Generation's start to see which passages belong where, and which
   choices lead to which branch.
4. **Rebuild:** each passage becomes a screen or a setup popup. Each round becomes a screen with that round's rules,
   and all text goes into editable `.csv` files.
5. **Verify:** scripts check that every text the code asks for exists. Every branch is then played through in the
   browser, using crafted save files to jump straight to later Generations.

Developer details (conventions and pitfalls) are in [`CLAUDE.md`](CLAUDE.md), deliberate deviations from the
original in [`docs/DEVIATIONS.md`](docs/DEVIATIONS.md).
Open work is listed in [`ROADMAP.md`](ROADMAP.md), finished work in [`docs/DEVLOG.md`](docs/DEVLOG.md).

## Other community projects

- [Deusald's fan-made companion app](https://github.com/Deusald/MyFathersWork-FanMadeCompanionApp), the project this
  app is based on
- [Chronicle](https://shrukan.github.io/chronicle/)
- [Masterwork](https://github.com/GameGhost/Masterwork)
- The **My Father's Work Fanmade App** Discord, also run by Deusald: https://discord.gg/QvGD44Ck44

## License and credits

*My Father's Work*, its stories, artwork, music and recordings are © Renegade Game Studios. The original app's source
is published under the [Creative Commons Attribution-NonCommercial 4.0](LICENSE) license, and this app is shared under
the same terms: free to use, share and adapt for non-commercial purposes, with credit to Renegade Game Studios. This
app changes the original: the stories were ported to a new web app, bugs were fixed, and features and narration were
added.

This version of the app was developed by Valentijn using Claude

The fonts are [Frijole](https://fonts.google.com/specimen/Frijole), [Bowlby One SC](https://fonts.google.com/specimen/Bowlby+One+SC)
and [Averia](https://iotic.com/averia/), all under the SIL Open Font License (licence texts in
`MyFathersWorkApp/MyFathersWorkApp/wwwroot/fonts/`).

Oliver's narration is generated with [ElevenLabs](https://elevenlabs.io) text-to-speech (voice "Oliver Silk") on a paid
plan.

This is a non-profit fan project, made by players for players. It is not affiliated with or endorsed by Renegade Game
Studios.
