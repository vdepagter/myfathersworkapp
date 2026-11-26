# Translations

The app's own text is English (`wwwroot/localization/*.csv`, which the narration tools read). A translation lives in
its own folder with the same file names: `wwwroot/localization/<code>/<file>.csv`, header `Tag;<Language>`. Czech
(`cs`) is the first, for the menus and *The Cost of Disease* (November 2026).

## How the app uses it

- `Shared/AppLanguages.cs` lists the languages (`AppLanguages.Translations`: name, code, translated scenarios, whether
  the grammar needs the players' gender). The menu language is a device setting (localStorage `mfwLanguage`, Settings
  and the pause menu; on the first start the browser's language when the app has it). A saved game, undo and load keep
  the device's choice.
- Every tag a translation lacks falls back to English, so a file can be translated a bit at a time.
- The story follows the menu language when the scenario is translated (`GlobalData.ApplyScenarioLanguage`); other
  scenarios stay English. The separate Scenario Language choice of the pause menu was dropped.
- A translated story is silent: narration is English only (`Pages/Gameplay.razor` `HasNarration`).
- Links that become green buttons and bid / vote events are recognised by the English text of the same tag
  (`GameplayElement.EnglishContent`), so translators word them freely.
- Log Book: `<code>/<Scenario>_LogBook.csv` with `Title;<Language> title;Location;Summary`, keyed by the English title
  (the game state keeps English titles).
- Guided mode: `<code>/<Scenario>_Guide.csv`. Endings Gallery names: UI tags `Gallery_<English label, letters and
  digits only>` (`GalleryTrees.Localize`). Dates: UI tags `Format_Date` / `Format_DateTime`.

## Grammar tokens (resolved when shown, `Grammar.Resolve`)

- `[[pl:one/few/many]]`: Czech plural (1 / 2–4 / 0 and 5+) by the nearest number before it.
- `[[gN:male/female]]`: the gender of the player whose name fills `{{N=...}}` in the same text. `[[g:male/female]]`:
  the nearest player name before it, else the first player named on the screen, else male. Czech players pick their
  gender at name entry (remembered per name, localStorage `mfwGenders`).
- Czech text gets a non-breaking space after one-letter prepositions and conjunctions.

## Fonts

Averia (text) and Frijole (headings) lack most Czech letters. `tools/fonts/czech-supplement.py` draws the missing ones
from each font's own letters and accents into `fonts/mfw-czech-*.woff2` (new family names, as the OFL Reserved Font
Names require); `css/theme.css` lists them after the original font. Run it again after a font change.

## Working on a translation

- Glossary and style guide: [`cs-glossary.md`](cs-glossary.md); brief for translators: [`translator-brief.md`](translator-brief.md).
- `python tools/i18n/check-translation.py <English csv> <translated csv> [--partial]` compares markup, placeholders,
  tokens, spaces and semicolons.
- `python tools/i18n/translation.py merge|check|sheet|page|import`: merge work parts (`docs/i18n/work/<code>/`), check
  every file, write the review workbook (English | Czech | suggested Czech | comment) and the side-by-side page, and
  apply a reviewed workbook.
- `docs/i18n/comparison/`: the Fable 5.2 vs Opus 5.5 sample (blind A/B page, `tools/i18n/comparison-page.py`).
