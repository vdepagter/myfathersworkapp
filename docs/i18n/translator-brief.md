# Brief for translators (Czech, first pass)

You translate part of the fan-made companion app for the board game *My Father's Work* into Czech. The text will be
reviewed by native Czech speakers, so aim for the version you would be proud to hand them: literary, idiomatic,
natural Czech that keeps the spirit of the original (gothic, deadpan-ironic, a pompous 19th-century chronicler; clear
rulebook Czech for rules), faithful in meaning but never word-for-word.

Repository root: `C:\GitHub\MyFathersWork-FanMadeCompanionApp\.claude\worktrees\charming-pascal-ffb1d9`.
English files: `MyFathersWorkApp\MyFathersWorkApp\wwwroot\localization\`.

## Before you start

1. Read `docs\i18n\cs-glossary.md` completely. It is binding: terms (section 3), tone (1), typography (1.4), the
   grammar mechanics (2: typed-in names stay in the nominative, `[[gN:mužský/ženský]]` gender tokens, `[[pl:1/2-4/5+]]`
   plural tokens, placeholders and markup kept exactly). The owner has accepted every recommendation in its section 5
   (open questions), including: Czech terms everywhere with the English component name in brackets at the first
   mention in a setup/rule popup (Q1, e.g. "karty Pochybných obchodů (*Dubious Bartering*)" - use `<i>` for the English
   name only where the English text already allows markup there; otherwise plain brackets), *veledílo*, *osobní
   figurka*, *děsivost*, *Uhry*, *-ová* for Nurse Varga, Czech-ised monster names, *klepnout*. The gender chips read
   "pán / dáma" ("Vaše postava je:").
2. Look at `docs\i18n\comparison\opus.csv` and `fable.csv`: two sample translations of the same passages (both
   acceptable; borrow their best solutions). Where your rows include those tags, you may start from either.
3. Read the English around your rows for context (the rows before and after, and the matching screens in the other CSV:
   story text is in `TheCostOfDisease_Localization.csv`, rules/setup text in `TheCostOfDisease_Gameplay_Localization.csv`;
   a screen's tags share the method name, e.g. `S5Special1_Content` (story) is followed by `S5Special1_Content1`
   (rules)). Some texts end mid-sentence and another tag (or a hub link) completes them: make your half join
   grammatically with the natural translation of the other half, and list such joins in your notes.

## Output

- A CSV file at the path you are given: first line `Tag;Czech`, then one line per row you translate, **in the same
  order and with the same tags as the English**, the Czech text on a single line. Copy `//` comment lines over
  unchanged (they help reviewers). Do not translate the tags.
- The text must never contain a semicolon (`;` is the separator) or a line break. Use `<br />` where the English has it.
- Keep leading and trailing spaces exactly as the English has them.
- Keep all markup (`<b>`, `<i>`, `<br />`, `<icon=..>`, `<sprite=..>`, `<link=n>..</link>`, `<p>`, `<ul><li>`,
  `<span class=..>`, `<a href=..>`) and every `{{n=...}}` placeholder with the same id, kind and number of options.
  Translate only human-readable option texts; previews like `...|Oliver|James|Olivia|...`, `...|Town|...`,
  `...|2|3|4|...` stay as they are.
- Gender: `[[gN:..]]` only where `{{N=...}}` is a player name in the same text; otherwise `[[g:..]]` (nearest player
  name before it, else the first player named on the screen, else masculine).
- Check your file before you finish, and fix every problem it reports:
  `python tools\i18n\check-translation.py <English csv> <your csv> --partial`
  (run it from the repository root; it compares markup, placeholders, tokens, spaces and semicolons).

## Your reply

When done, reply in at most 12 lines: the file you wrote, the number of rows, that the check passed, and the few
places a reviewer should look at (joins with other tags, liberties you took, terms you had to coin that are not in the
glossary - list each coined term as `English -> Czech` so it can be added to the glossary).
Do not edit any file other than your output file.
