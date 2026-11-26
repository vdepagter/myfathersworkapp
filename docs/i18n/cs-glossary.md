# Czech translation: glossary and style guide (draft for review)

Scope: the app UI (`UI_Localization.csv`) and the scenario *The Cost of Disease* (`TheCostOfDisease_Localization.csv`,
`TheCostOfDisease_Gameplay_Localization.csv`, `TheCostOfDisease_LogBook.csv`, `TheCostOfDisease_Guide.csv`).
Status: proposal. The owner and native Czech reviewers settle the open questions (section 5) before the bulk
translation starts. After that, translators follow this file as written.

Background that drives many choices: **no Czech edition of the board game exists.** Players hold English cards, tiles,
boards and the English *Village Chronicle*, and they read the app in Czech. The Czech text has to be atmospheric, and
it also has to let a player find the right physical component quickly.

---

## 1. Tone and register

### 1.1 Story text (`TheCostOfDisease_Localization.csv`)

The source is a mock-scholarly chronicle of a mad-science dynasty, 1859 to 1923. A stuffy, slightly pompous historian,
Dr. Ensign Benwallis, narrates it, and he quotes journal entries, letters, newspaper headlines, a nurse's report,
sworn statements and an editor's letter. The tone is florid, gothic and deadpan-ironic: horror told with a raised
eyebrow ("Another good servant gone. It was somewhat inevitable, yet still an unfortunate loss of life, time and money.").

- **Tense:** narrative past (préteritum), as in the source. Pick the aspect for rhythm: perfective for events,
  imperfective for description. Keep the source's present tense where it has one: the chronicler's own comments ("It is
  said that…", "I shudder to think…"), the present-tense hunt results, and the rule boxes.
- **The chronicler's voice:** a 19th-century learned Czech register in the manner of Czech translations of Poe or
  Stoker. Use long periodic sentences, but keep them readable. Archaic words are welcome as seasoning (*pravil, nuže,
  jakožto, nadmíru, toliko, onen, kterak, ba, vskutku, panské sídlo, služebnictvo, kočár*). Use at most one or two per
  paragraph. Avoid *kterýžto*-style pastiche and Church Slavonicisms. Keep the irony dry. Do not explain the jokes,
  and do not soften the gore or the bawdy *Bar-venture* statements.
- **Calques to avoid:** "udělat rozhodnutí" (use *rozhodnout se*), "pro pokračování klikněte" (see 2.6), "v pořádku",
  "mít smysl" for *make sense*, "být schopen" for every *can*. The passive with *být* is acceptable where the source
  uses it for effect, otherwise prefer the reflexive passive or the active voice. Write "na konci kola", not
  "na konci od kola".
- **Anachronisms to avoid:** *OK, manažer, farmářský trh, zverimex, mobil, šoupnout*. The source's own anachronisms
  stay (the automaton, "robot", "Meat is murder").
- **Letters and dialogue:** strangers, colleagues, envoys, monsters and the editor say *vy* to each other. Capitalise
  *Vy/Vás/Vám/Váš* inside letters (Czech letter convention) but not in spoken dialogue. Close family in journal entries
  ("my dear, witless cousins") is talked *about*, not addressed. Write headers like
  "Letter – April 3, 1865" as "Dopis – 3. dubna 1865" and "Journal Entry – November, 1859 – {{0}}" as
  "Zápis v deníku – listopad 1859 – {{0}}".
- **Journal entries** are in the first person. Past tense needs gender, see 2.2 and open question Q6.
- **Typos and slips in the source** (e.g. "headless to their warnings", a stray quote mark, "sook") are translated
  as intended. Game logic stays exactly as written, even where it looks wrong (`docs/DEVIATIONS.md`).

### 1.2 Rules / setup text (`*_Gameplay_Localization.csv`, Guide, rule boxes inside story tags)

Use clear, short, consistent rulebook Czech. The same English term always gets the same Czech term (section 3), even
where the English varies: the source says "Start Player marker", "Starting Player token" and "starting player marker",
and all three become *žeton začínajícího hráče*.

- **Address the players with the 2nd person plural imperative:** *Umístěte, Získejte, Zaplaťte, Lízněte si, Vraťte*.
  This is the Czech rulebook standard. It works for the whole table and for a single player alike, because the
  plural doubles as polite *vykání*, so no screen ever needs *ty* forms or a singular/plural switch. Statements about a
  named or generic player use the 3rd person present: "{{0}} získává 3 VB." "Hráč s nejvíce VB…"
- **Generic "player"** is the generic masculine *hráč* (pl. *hráči*), as in every Czech rulebook. Do not write
  *hráč/hráčka*.
- **Present tense and infinitives carry no gender.** Prefer them in rules and UI (see 2.2).
- Section headings in capitals stay in capitals: *PŘÍPRAVA, ZVLÁŠTNÍ PŘÍPRAVA, ZVLÁŠTNÍ PRAVIDLO, ZVOLTE:,
  HLASOVÁNÍ, DRAŽBA, ZVLÁŠTNÍ UDÁLOST, ZVLÁŠTNÍ AKCE, VÝSLEDEK SYMPOZIA, ÚSPĚCH!, NEÚSPĚCH!*

### 1.3 UI text (`UI_Localization.csv`)

Buttons and menu items use the infinitive or a noun, following Czech UI convention: *Pokračovat, Uložit, Zrušit,
Nová hra, Nastavení*. Descriptions and hints use the 2nd person plural (*Klepněte…, Vyberte…*). The app's ironic
butler voice on the setup screens (Players, Village, Welcome) belongs to the story register (1.1).

### 1.4 Typography and numbers

- **Quotes:** „…“, nested ‚…‘. Never "…" or »…«. Comma placement as in standard Czech: „Přijdu zítra,“ řekl.
- **Dashes:** the source's " - ", "—" and " – " all become a spaced en dash ` – `. Ranges get an unspaced en dash
  (1–3). The hyphen is for compounds only.
- **Ellipsis:** the single character `…` (U+2026). Link texts must match section 2.6 exactly.
- **No semicolons.** `;` is the CSV separator, and a semicolon in the text breaks the file. Use ` – `, a comma or a
  new sentence. Czech uses semicolons more than English does, so watch for them.
- **Non-breaking spaces:** do **not** type `&nbsp;` after one-letter prepositions or conjunctions (v, s, z, k, o, u,
  a, i). The app should add them centrally, which is a developer task (Q11). Type ordinary spaces everywhere.
- **Victory Points:** abbreviate as **VB** (*vítězné body*), with a space: `3VP` → `3 VB`, "gain 2 VP" → "získejte
  2 VB". VB takes no plural ending. Write it out only in running story text ("vítězných bodů") where the source does.
- **Money:** keep the board's dollar sign *before* the number, exactly as in the source: `$2`, `${{0}}`. This matches
  the coins and icons on the board and keeps placeholders in place. Never write "2 dolary" or "2 $".
- **Ordinals:** `1st/2nd/3rd/4th` → `1./2./3./4.`. Regnal and generation numbers take a period:
  `{{0}} II` → `{{0}} II.`, `{{0}} III` → `{{0}} III.`, "the 1st" → `I.`. "Jr." → `ml.` ("Dr. {{0}} ml.").
- **Percent:** `40 %` (with a space). **Dates:** `3. dubna 1865`, `v létě 1892`, `listopad 1859`.
- **Numbers in rules** are digits ("2 pole", "o 3 pole"). In story text, small numbers are written out as in the source.

---

## 2. Grammar mechanics the app supports (how to use them)

### 2.1 Typed-in names are never declined

Player names (`{{n=...|Oliver|James|Olivia|...}}`), the village name (`{{n=...|Town|...}}`,
`{{n=...|London|Dubai|Paris|...}}`) and the Masterwork name (`{{n=...|Creation|...}}`) are whatever the players typed.
The app inserts them unchanged, so **they always stand in the nominative.** Rephrase the sentence so that the name:

1. is the **subject** of the sentence (the best option, and usually possible);
2. stands **after a colon**, in a heading, or on its own line;
3. stands **in apposition after a declinable common noun** that carries the case: *vědec/vědkyně, bratranec/sestřenice,
   Dr., jméno, rod, vesnice, město, obec, dílo* or *X jménem {{0}}* ("dílo jménem {{0}}", "vesnice jménem {{0}}"). This
   is the accepted Czech UI pattern ("soubor {name}"). In story text, use it sparingly.

Never build possessive adjectives (*Oliverův*) and never let a preposition govern the name directly
(*k {{0}}*, *v {{0}}*). Fixed names in the text (Ensign Benwallis, Nurse Varga, Hagen and Mabel Varga, Celia
Benwallis Cartwright, St. Hubertus) are **normal Czech words**: decline them (see section 4).

Worked examples (from the CSVs; `{{0}}` abbreviates the full placeholder, which must stay complete in the CSV):

| # | Tag | English | Czech (proposal) | Technique |
|---|---|---|---|---|
| 1 | `FirstPlayer_Content` | Give the **Start Player** marker to **{{0}}.** | **Žeton začínajícího hráče** dostává **{{0}}.** | dative → subject |
| 2 | `Gen1CreepyRefuseBonus_0_Content` | a group of individuals arrived at {{0}}'s estate | skupina lidí dorazila na panství [[g0:vědce/vědkyně]] {{0}} | possessive → apposition |
| 3 | `S5Special1B_Content` | life in {{0}} would never again be the same | život ve vesnici {{0}} už nikdy nebude takový jako dřív | "in X" → "ve vesnici X" |
| 4 | `HospitalVisitCheck_Content` | The {{0}} General Hospital | Všeobecná nemocnice {{0}} | name as label (cf. "Nemocnice Motol") |
| 5 | `WolvesBankMayorGood_Content` | every time they entered {{1}}'s limits | kdykoli vstoupili do města {{1}} | genitive → "město X" |
| 6 | `Unicure2_Content` | {{0}} Jr. was known for their incisive wit | {{0}} ml. [[g0:byl znám/byla známa]] svým ostrým vtipem | subject, `ml.` |
| 7 | `Gen1InsanityYes_0_Content` | {{0}} II's childhood guidance came more from the servants | {{0}} II. [[g0:vyrůstal/vyrůstala]] spíš pod dohledem služebnictva než rodičů | possessive → subject |
| 8 | `VialCharity_0_Content` | aptly addressed to {{0}} the 1st | příhodně adresováno: {{0}} I. | dative → colon |
| 9 | `HubertusWolves_Content` | I, {{0}}, am writing this letter to you | Já, {{0}}, Vám píši tento dopis | nominative apposition |
| 10 | `PlayersWelcome_First` | Welcome to My Father's Work,<br />{{0}}<br />Our warmest regards. | Vítejte ve hře My Father's Work.<br />{{0}}<br />Přijměte naše nejvřelejší pozdravy. | no vocative: the name stands alone on its line |
| 11 | `MWComplete_Content5` | The {{0}} had been a stunning success | Dílo jménem {{0}} slavilo ohromující úspěch | Masterwork name after "dílo jménem" |
| 12 | `NewMasterResult_Title` | The {{0=\|Infamous\|…}} {{1}} | {{0=\|Nechvalně proslulé\|Transcendentní\|…}} dílo: {{1}} | the adjective agrees with *dílo*, the name follows the colon |
| 13 | `Huntround_Content` | {{0}} III and {{1}} III were chosen. | Volba padla na: {{0}} III. a {{1}} III. | avoids a plural past participle of unknown gender |

**Vocative:** never put a typed name where Czech needs a vocative ("Milý {{0}}," is wrong). Use a greeting without
the name (*Vážený příteli, Milá rodino*), or give the name its own line or a colon.

### 2.2 Grammatical gender of players: `[[gN:masculine/feminine]]`

English uses singular *they* for player characters. At name entry each Czech player picks masculine or feminine
(Czech UI only). Token syntax:

- `[[g0:mužský tvar/ženský tvar]]` picks by the gender of the player whose name fills `{{0=...}}` **in the same CSV
  text**. `[[g2:…]]` goes with `{{2=...}}`, and so on. Match the placeholder number, not the order in the sentence.
- `[[g:mužský/ženský]]` (no number) picks by the **nearest player name earlier in the same displayed text**.
- The separator is `/`, never `|` (which belongs to `{{…}}`). Tokens may sit **inside placeholder options**.
- **Write whole words or phrases**, never suffix fragments: `[[g0:spěchal/spěchala]]`, not `spěchal[[g0:/a]]`.
  Whole words are easier to review and to search for.

Examples:

- `Gen1CreepyNo_Content`: "And so {{0}} waved them away…" → "A tak {{0}} [[g0:mávl/mávla]] rukou a zástupce
  s nonšalantním pohrdáním [[g0:odbyl/odbyla]]…"; later "unbeknownst to the semi-virtuous scientist" → "aniž to
  [[g:napůl ctnostný vědec tušil/napůl ctnostná vědkyně tušila]]".
- `HospitalVisitCheck2_0_Content1`: "Dr. {{0}} seemed in a hurry." → "Dr. {{0}} [[g0:vypadal/vypadala]], že spěchá."
  Inside the option `{{1=?|…They retrieved a scalpel…|…}}` → `[[g0:Popadl/Popadla]] skalpel…`.
- `UniCharity_Content` names `{{2}}` and `{{3}}`: use `[[g2:…]]` and `[[g3:…]]` accordingly.
- A common noun for a player: `[[g0:vědec/vědkyně]]`, `[[g0:bratranec/sestřenice]]`, `[[g0:dědic/dědička]]`,
  `[[g0:starosta/starostka]]`.

How to keep the number of tokens down (each token is a review risk):

- **Present tense, infinitives, nouns and reflexives have no gender:** *svůj, se, si*. Write "{{0}} vítězí!", not
  "{{0}} zvítězil/a"; "Vítěz: {{0}}"; "{{0}} se rozhodl/a" → "{{0}} se musí rozhodnout" where the meaning allows.
- **Spouse:** *choť* works for husband and wife. Only the possessive changes: `[[g0:jeho choť/její choť]]`, or
  gender-free *svůj choť / svá choť*. Prefer `[[g0:jeho/její]] choť`.
- **Groups are plural.** Siblings, cousins, the family members and "the scientists" take the **masculine animate
  plural**, which covers mixed groups: *sourozenci přijeli, bratranci se dohodli, vědci se smáli*. Gen I says
  *sourozenci* (gender-neutral in effect). Gen II and III "cousins" may be *bratranci a sestřenice* once, then
  *bratranci*, *příbuzní* or *rodina* (f. sg.: *rodina se rozhodla*). Never use tokens for groups.
- **A player's parent or child** (previous or next generation; the gender is unknown and is not in the text): use
  *rodič* or *potomek* in noun phrases ("rozhodnutí rodiče", "rodičovo odmítnutí"), or the plural *rodiče, předkové*.
  Avoid a past-tense verb with *rodič* as the subject where you can.
- **Two named players together** (`Huntround`, `NoUniMayornCreepy`): avoid a shared past participle (example 13).
- **Unknown individuals** (the representative, the messenger, "the scientist who reached space 4"): use the generic
  masculine (*zástupce, posel, vědec*), unless the source gives a gender (the envoy "she", Nurse Varga).

### 2.3 Plurals: `[[pl:1/2-4/5+]]`

Three forms, picked by the **nearest number (digits) before the token** in the displayed text. Use the token only
where a number from a placeholder governs a noun. Fixed numbers are written normally ("2 pole"). Icons, "VB" and "$"
need none. Write each form **in the case the sentence requires**:

- `HuntSuccessCheckSuccess_Content` "with {{0=...|1|2|...}} successful hunt" →
  "po {{0=...|1|2|...}} [[pl:úspěšném lovu/úspěšných lovech/úspěšných lovech]]" (locative).
- `Gallery_GamesPlayed` "{{0}} games played" → "{{0}} [[pl:odehraná hra/odehrané hry/odehraných her]]".
- `Fame_Players` "{{0}} players" → "{{0}} [[pl:hráč/hráči/hráčů]]".
- `HospA1Yes_Content` "The {{0}} scientists" → "{{0}} [[pl:vědec/vědci/vědců]]" (the subject's verb must then not
  vary: put it in the present tense or rephrase).
- "X of Y …" counts ("{{0}} z 8 konců", "{{0}} z {{1}} úspěchů") always take the genitive plural after *z*: no token.

**Player-count words** `{{0=|two|three|four}}` are *options*, not tokens. Translate them in the case and gender the
sentence needs, and keep exactly three options: "for {{0}} players" → "pro {{0=|dva|tři|čtyři}} hráče";
"of {{0}} young aristocrats" → "{{0=|dvou|tří|čtyř}} mladých aristokratů"; "the {{0}} of us" → "my
{{0=|dva|tři|čtyři}}" (mixed group: masculine *dva*); `{{0=|duo|trio|quartet}}` → `{{0=|dvojice|trojice|čtveřice}}`.

### 2.4 Markup and placeholders: keep the structure exactly

- Keep every `<b>`, `<i>`, `<br />`, `<icon=…>`, `<sprite=…>`, `<link=n>…</link>`, `<p>`, `<ul><li>`, `<span class=…>`,
  `<h3>` and `<a href=…>`. Translate only the text between them. Bold and italic may move with the words they
  mark. Do not add markup of your own.
- **Placeholders:** keep id, type and **the number and order of options**. Translate the options (except names and
  numbers). The text after `=` in `{{0=...|Oliver|...}}` is only a preview: leave previews as they are. Placeholders
  may move to wherever Czech word order needs them, for example a date `{{1}}. {{0=|dubna|srpna|října}} 1865`.
- Option text may not contain `|` or `}`. Nested placeholders stay nested.
- `{{2=?|if true|if false}}` and `{{0=?|text}}` (one branch): translate both branches so that each reads correctly
  with the surrounding text, including spaces at the edges.
- **Agreement with an option value:** when an option list feeds into a sentence (a monster, a building, a science),
  move the words that must agree **into the options**: `{{0=|Odporný strigoj|Odporné měsíční zjevení|Odporná
  mantichora|…}}`, not "Odporný {{0=|strigoj|…}}". The `[[g]]` tokens only follow players.
- **Links:** link texts within one string must stay unique. A trailing space before an inline link is significant
  (`… mohou ` + `<link=0>…`). Some story tags end mid-sentence and the next tag or a hub link finishes the sentence
  (`S5Special1_Content` "…the new mayor would immediately " + `S5Special1_Content1`). Translate such pairs together so
  they join grammatically.
- Hub `…_ClickHere` texts complete the sentence of their `…_Content` (`Devastation_StableHand_Content` "When a player
  visits the Stables, they may " + "click here to speak with the Stable Hand.").
- `Xvillage` in the Log Book and `[[hub:…]]` / `[[end:n]]` in the Guide are tokens: copy them unchanged.

### 2.5 Text that must stay in sync

- **Log Book** (`cs/TheCostOfDisease_LogBook.csv`, columns `Title;Czech title;Location;Summary`): the first column is
  the **English** title, copied unchanged (the key). The Czech title should equal the translated window title of the
  same screen (screens with two events are told apart by it). Locations and summaries are free translations. The word
  "Player" in summaries becomes *hráč*.
- **Endings Gallery** names: UI tags `Gallery_<English label without spaces and punctuation>` in `cs/UI_Localization.csv`
  (e.g. `Gallery_TheCostofDisease`, `Gallery_TheGoodFightMovingForward`). Keep `[your village]` in a label as it is.
  They should match the translated hub and ending titles (3.8).
- Chapter names in Help (`Help_VillageNames`) must match 3.8.

### 2.6 Link and button phrases

The app recognises the stand-alone links it turns into green buttons by the **English** text of the same tag, so the
Czech wording is free. For consistency, use these:

| English | Czech |
|---|---|
| Click here to continue… / Click to continue… | Klepnutím pokračujte… |
| Click here to start the vote… / Click to start the vote… | Klepnutím zahajte hlasování… |
| Click to start the bid… | Klepnutím zahajte dražbu… |
| Click to tabulate scores for posterity… | Klepnutím sečtěte body pro potomstvo… |
| Click to continue to Final Scoring… / Continue to Scoring… | Klepnutím přejděte k závěrečnému bodování… |
| Click here after resolving the bid / vote… | Po vyhodnocení dražby / hlasování klepněte sem… |
| click here (inside a sentence) | klepněte sem |

Use *klepnout* (tap), the Czech touch-UI verb, rather than *kliknout*, because the app runs mostly on phones and
tablets (Q10).

---

## 3. Term table

"On component" means the term is printed in English on something the players hold. For those terms, the recommended
policy (Q1) is: **Czech everywhere, plus the English name in parentheses at its first mention in each setup/rule
popup** where players must find, place or return that physical item: "Vezměte z krabice karty Pochybných obchodů
(*Dubious Bartering*)." Plain parentheses, no extra tags. No English for generic terms (žeton, VB, generace), for
icons, or in story text.

### 3.1 Game system

| English | Czech | Gram. | Notes |
|---|---|---|---|
| Victory Points, VP | vítězné body, **VB** | m. pl. | `3VP` → `3 VB`. VP track → *počítadlo vítězných bodů*. |
| Generation | generace | f. | GENERATION I: → *GENERACE I:*; First Generation → *první generace*. |
| Early / Middle / Late Years | Raná léta / Zralá léta / Pozdní léta | n. pl. | Round names, capitalised. "of the Early Years" → *raných let*. Alt.: *Rané/Střední/Pozdní roky* (flatter). |
| round | kolo | n. | End of Round → *Konec kola*; Start of Round actions → *akce začátku kola*. |
| End of Generation; Perform the End of a Generation | Konec generace; Proveďte kroky konce generace | m. | As in the rulebook's End of Generation section. |
| turn, turn order | tah, pořadí tahů | m. | "later in turn order" → *později v pořadí tahů*. |
| Start Player / Starting Player marker/token | žeton začínajícího hráče | m. | One term for all source variants. |
| Generation Bonus | generační bonus | m. | |
| special event | zvláštní událost | f. | Popup title *Zvláštní událost*. |
| setup / special setup / special rule | příprava / zvláštní příprava / zvláštní pravidlo | f./n. | |
| bid / bidding | dražba / přihazování | f. | BID → *DRAŽBA*; win the bid → *vyhrát dražbu*. |
| vote, Yay / Nay | hlasování, **PRO / PROTI** | n. | Voting token sides: "PRO (*Yay*)" at first mention. "(YAY!)" → *(PRO!)*. |
| election, elect | volby, zvolit | f. pl. | "wins the election" → *vyhrává volby*. |
| tie, ties are broken by | shoda, shodu rozhoduje | f. | "If there is a tie" → *Při shodě…*. |
| supply | zásoba | f. | "pay $1 to the supply" → *zaplaťte $1 do zásoby*. |
| game box / scenario box | krabice hry / krabice scénáře | f. | |
| board / game board | herní plán | m. | Czech convention. Player boards: *deska*. |
| track / space | stupnice / pole | f./n. | "space 3 of the Insanity track" → *pole 3 stupnice šílenství*. |
| token / marker / piece / cube / tile / card / deck | žeton / ukazatel / figurka / kostička / dílek / karta / balíček | | Community standard. *Ukazatel* only for track markers (Suspicion). |
| face up / face down | lícem nahoru / lícem dolů | | |
| draw / discard / retrieve / return | líznout si / odhodit / vzít si (z krabice) / vrátit | | "discard to the box" → *odhoďte do krabice*. |
| gain / lose (with an icon) | získat / ztratit | | "Gain 1 <icon=Creepy_Icon>" → *Získejte 1 <icon=Creepy_Icon>*; "lose 3VP" → *ztraťte 3 VB*. |
| action / action space | akce / akční pole | f./n. | |
| Perform (an) Experiment action | akce Provést experiment | f. | Capitalised as an action name. |
| resources | zdroje | m. pl. | "2 resources" → *2 zdroje*. Kept distinct from *přísady*. |

### 3.2 Player components (base game, printed in English)

| English | Czech | Gram. | Notes |
|---|---|---|---|
| Estate (board) | panství; deska panství | n. | Story also uses *sídlo, statek*. "Estate action" → *akce panství*. |
| estate plot / new plot | parcela / nová parcela | f. | "cost to open a new plot" → *cena za otevření nové parcely*. |
| Estate Upgrade (tile) | vylepšení panství (dílek) | n. | On component. |
| Vanity Estate Upgrade | prestižní vylepšení panství | n. | On component. Alt.: *okázalé*. |
| Quarters | komnaty | f. pl. | Where pieces and cubes rest: *do svých komnat*. Alt.: *pokoje*, *ubikace* (too military). |
| Self (piece) | osobní figurka; "visit with your Self" → *navštívit osobně* | f. | Gender-free. Alt.: *figurka vědce*. On component (Q3). |
| Spouse | choť | m./f. | Works for both genders (see 2.2). |
| Servant | sluha (pl. sluhové), služebnictvo | m. | "Servant/Spouse piece" → *figurka sluhy nebo chotě*. |
| Caretaker (scenario piece) | opatrovník | m. | Distinct from the Order's lodge "caretaker" in story (*správce*). |
| Lost (the LOST area) | Ztracení | m. pl. | "gain a Servant from Lost" → *získejte sluhu ze Ztracených*; "return to Lost" → *vraťte mezi Ztracené*; "become Lost" → *přejde mezi Ztracené*. English (*LOST*) at first mention. |
| Journal (player board) | Deník | m. | Capitalised for the board; *deník* in story. |
| Journal Track / research track / research level | výzkumná stupnice (v Deníku) / úroveň výzkumu | f. | "third level of research" → *třetí úroveň výzkumu*. Journaled Knowledge → *znalost zapsaná v Deníku*. |
| Knowledge (cube) | znalost (kostička znalosti) | f. | "a Knowledge of your choice" → *libovolná znalost*. |
| Chemistry / Engineering / Biology / Occult | chemie / inženýrství / biologie / okultismus | | Knowledge: *znalost chemie…*; Occult knowledge → *okultní znalost*. Experiment adjectives: *chemický, inženýrský, biologický, okultní*. Cost lists: `5× biologie`, `2× zvíře` (multiplication-sign style, no case problems). |
| ingredient | přísada | f. | Alt.: *ingredience*. |
| Body / Animal / Chemical, Bottle / Gear | tělo / zvíře / chemikálie, lahvička / ozubené kolo | | Ingredient tokens. |
| Experiment (card), A/B/C-Level | experiment (karta experimentu), experiment úrovně A/B/C | m. | Short form *experiment B*. Story may vary with *pokus*. Experiment deck → *balíček experimentů*. |
| Stored Experiments | uschované experimenty | m. pl. | Avoids clashing with *Uložené hry*. |
| Masterwork (Experiment) | **veledílo** (karta veledíla) | n. | Alt.: *mistrovské dílo* (Q2). Player-typed name follows *dílo jménem…*. |
| Masterwork Update / Upgrade card | karta Úpravy veledíla | f. | Unify the source variants. |
| Masterwork Recipe | recept veledíla | m. | |
| Compulsion (card) | Nutkání (karta Nutkání) | n. | On component. |
| Maladjustment (card) | Porucha (karta Poruchy) | f. | On component. |
| Creepy track / creepy location | stupnice **děsivosti** / děsivé místo | f. | Mostly an icon. Alt.: *podivínství, zlověstnost* (Q4). |
| Insanity track | stupnice šílenství | f. | "calm their sanity" → *uklidnit mysl*. |
| Angry Mob (track, token) | rozzuřený dav (stupnice, žeton rozzuřeného davu) | m. | "overtaken by the Angry Mob" → *dostižen rozzuřeným davem*. |
| Suspicion marker | ukazatel podezření | m. | |
| Village Chronicle (printed book) | Kronika vesnice | f. | On component. First setup popup: *Kronika vesnice (Village Chronicle)*. "Turn to page 4" → *Nalistujte stranu 4*. |
| Storybook (the app) | **Kniha příběhů** | f. | "hand the Storybook / storybook device to…" → *podejte Knihu příběhů / zařízení s Knihou příběhů…*. |
| Storybook token (<icon=Storybook>, <icon=StorybookToken>) | žeton Knihy příběhů | m. | Both icons, one term. |

### 3.3 Scenario components (*The Cost of Disease* box, printed in English)

| English | Czech | Gram. | Notes |
|---|---|---|---|
| Dubious Bartering (card) | Pochybný obchod (karta) | m. | Window title *Pochybné obchody*. |
| Heart token | žeton srdce | m. | "Return the Heart token" → *Vraťte žeton srdce*. |
| Commemorative Mayoral Coin | pamětní starostovská mince | f. | |
| Yellow Fever Inoculation (Experiment) | Očkování proti žluté zimnici | n. | |
| Panacea: Cure for all Ailments | Panacea: lék na všechny neduhy | f. | Story: *panacea, všelék*. |
| Immortality (cards, Masterwork) | Nesmrtelnost (karty Nesmrtelnosti) | f. | |
| Hereditary Disease (card, Experiment) | Dědičná choroba | f. | "Disease card" → *karta choroby*. Use *choroba* for the game term, *nemoc/nákaza* in story. |
| Letter cards | karty dopisů | f. pl. | "seal side up" → *pečetí nahoru*. |
| Sealed Envelope (card) | Zapečetěná obálka | f. | password → *heslo*. |
| Suspicious Building tiles | dílky podezřelých budov | m. pl. | Exposed / Concealed → *odhalená / utajená*. Investigate → *prošetřit*. |
| Hardware Store / Wire Service / Book Store / Warehouse / Pet Store | Železářství / Telegrafní úřad / Knihkupectví / Skladiště / Obchod se zvířaty | n./m. | Option lists keep 6 entries including the leading blank. |
| Fraternity of Hunters / Order of St. Hubertus tokens | žetony Bratrstva lovců / Řádu sv. Huberta | m. pl. | "Hunter token" → *žeton lovců*. |
| Vial token | žeton lahvičky | m. | |
| Voting token | hlasovací žeton | m. | |
| Lycanthropic Strength (Masterwork, Update card) | Lykantropická síla | f. | |
| Creepy tokens | žetony děsivosti | m. pl. | |
| A Building location (Chronicle spot A/B/C) | místo budovy A | n. | |

### 3.4 Town locations (printed on the Village Chronicle pages)

| English | Czech | Notes |
|---|---|---|
| Church | kostel | |
| Bank / Library | banka / knihovna | |
| Hospital; General Hospital | nemocnice; Všeobecná nemocnice | Board of Trustees → *správní rada*. |
| Town Hall | radnice | |
| Stables; Stable Hand | stáje; podkoní | |
| Farmer's Market | tržiště | Not *farmářský trh* (modern). |
| General Store | obchod se smíšeným zbožím | Short: *smíšené zboží*. |
| Blacksmith | kovárna (the person: kovář) | |
| Fortune Teller | věštkyně | |
| Laborer's Union | dělnický spolek | |
| Cemetery | hřbitov | |
| Pub / Bar / tavern / public house | hostinec / výčep / krčma / hospoda | Hub title PUB → *HOSTINEC*. |
| University | univerzita | |
| Traveling Caravan | kočovná karavana | |
| The Hunter's Rest / Hunter's Haven / Hunter's Lodge | Lovcův odpočinek / Lovecké útočiště / Lóže lovců | The Order's Lodge → *lóže Řádu*. |
| Monster Spawn | líheň příšer | |
| School for Hybridized Individuals | Škola pro hybridní jedince | |

### 3.5 Estate upgrades and rooms

| English | Czech | English | Czech |
|---|---|---|---|
| Reading Room | Čítárna | Stasis Chamber | Stázová komora |
| Shrine | Svatyně | Curious Supply Room | Podivuhodná zásobárna |
| Storage Shed | Kůlna | Personal Library | Soukromá knihovna |
| Master's Study (Chemistry / Engineering / Biology / Occult) | Pracovna mistra (chemie / inženýrství / biologie / okultismu) | Charity Memorial | Pomník dobročinnosti |
| Spawning Pods | Líhňové tobolky | Catacombs | Katakomby |

### 3.6 Story world: disease, factions, monsters

| English | Czech | Notes |
|---|---|---|
| Yellow Fever | žlutá zimnice | The historical Czech name. |
| miasma | miasma, výpary | |
| inoculation / cure / vaccine | očkování / lék / vakcína | publish / patent → *zveřejnit (publikovat) / patentovat*. |
| Symposium | sympozium | |
| Mayor | starosta (starostka) | With a player: `[[g0:starosta/starostka]]`. |
| Fraternity of Hunters | Bratrstvo lovců | International … → *Mezinárodní bratrstvo lovců*. |
| Order of St. Hubertus | Řád svatého Huberta / Řád sv. Huberta | St. Hubert is the patron saint of hunters, so the irony carries over. International … → *Mezinárodní řád sv. Huberta*. |
| wolves, werewolf, lycanthrope, lycanthropic | vlci, vlkodlak, lykantrop, lykantropický | |
| Strigoi | strigoj (pl. strigojové) | Romanian folklore. |
| Pricolici | pricolič (pl. pricoliči) | Czech-ised so it declines. Alt.: keep *pricolici* (Q8). |
| Moonlight Presence | Měsíční přízrak | Masculine, so the adjectives in the options match. |
| Manticore | mantichora (f.) | Agreeing words go into the options (2.4). |
| (Stone) Golem | (kamenný) golem | |
| (Shambling) Wight | (potácivý) umrlec | |
| Troll | troll | |
| Priest | kněz | |
| The Frenzy | Běsnění | night of "The Frenzy" → *noc „Běsnění“*. |
| The Grand Contest | Velké klání | |
| the Hunt / Night of the Hunt | hon / Noc honu | *lov* where it means the act of hunting. |
| holy water, vial | svěcená voda, lahvička | |
| expose / conceal | odhalit / zatajit | |

### 3.7 UI and menus

| English | Czech | English | Czech |
|---|---|---|---|
| New Game | Nová hra | Continue / Resume | Pokračovat |
| Load Game / Load a save code | Nahrát hru / Nahrát kód uložené hry | Saved Games | Uložené hry |
| Save code | Kód uložené hry | Settings | Nastavení |
| Help | Nápověda | Log Book | Kniha záznamů |
| Endings Gallery | Galerie konců | Ending / endings | konec / konce (gen. pl. *konců*) |
| Achievements | Úspěchy | Hall of Fame | Síň slávy |
| Guided mode (spoilers!) | Režim s průvodcem (prozrazuje děj!) | Pause Menu / Pause | Pauza |
| Back (undo) / Forward (redo) | Zpět / Vpřed | Return to Main Menu | Zpět do hlavní nabídky |
| Offline play | Hra offline | Install as an app | Nainstalovat jako aplikaci |
| Narration / narrator | Předčítání / předčítající | Oliver / Male voice / Female voice | Oliver / Mužský hlas / Ženský hlas |
| Sound effects / Music / Volume | Zvukové efekty / Hudba / Hlasitost | Highlight / Scrolling | Zvýraznění / Posouvání |
| Graphics Low/Medium/High | Grafika nízká/střední/vysoká | Story text size | Velikost textu příběhu |
| Accept / Confirm / Cancel / Save / Delete | Přijmout / Potvrdit / Zrušit / Uložit / Smazat | Delete for good? | Smazat natrvalo? |
| Start Voting / Start Bidding | Zahájit hlasování / Zahájit dražbu | REVEAL! | UKAŽTE! |
| Bid Resolved / Vote Resolved | Dražba vyhodnocena / Hlasování vyhodnoceno | Reveal (secret page button) | Odkrýt |
| Final Scoring / Tabulate Scores | Závěrečné bodování / Sečíst body | End of Round / End of Generation | Konec kola / Konec generace |
| Ending unlocked! | Odemčen nový konec! | Tester option | Volba pro testery |
| Play along with the official app | Hrát souběžně s oficiální aplikací | Roll for me | Hodit za mě |
| Update / Later | Aktualizovat / Později | Download / Remove / Downloaded | Stáhnout / Odebrat / Staženo |
| Players / Player A | Hráči / Hráč A | Enter name… / Enter village name… | Zadejte jméno… / Zadejte název vesnice… |
| *(new)* gender picker label and chips | Oslovení: **pan / paní** | is victorious! (after a name) | vítězí! |

Fixed UI forms: "Dr. {{0}} Jr." → `Dr. {{0}} ml.`, "{{0}} III" → `{{0}} III.`, "Page 1 of 3" → `Strana 1 z 3`,
"{{0}} won" → `Vítěz: {{0}}`, "Remove {{0}} from the name suggestions" → `Odebrat jméno {{0}} z návrhů`.

### 3.8 Chapter, branch and ending titles; scenarios; achievements

| English (hub id) | Czech | Notes |
|---|---|---|
| Yellow Fever (Fever) | Žlutá zimnice | Gen I |
| Thriving in Adversity (Hospital) | Rozkvět navzdory nepřízni | Gen II |
| Devastation (Devastation); intro "The Epidemic Strikes" | Zkáza; „Epidemie udeří“ | Gen II |
| Search for the Cure (University); intro "The University Advances" | Hledání léku; „Univerzita postupuje“ | Gen III |
| The Crawl (NoUniversity); intro "DROWN YOUR SORROWS" | Tah po hospodách; „ZAPIJTE SVÉ ŽALY“ | Plays on *pub crawl* |
| A Taste of Evil (GloomyGothic) | Příchuť zla | Gen III |
| The Good Fight / Moving Forward / A Return to Evil (Prosperity) | Dobrý boj / Cesta vpřed / Návrat zla | Gen III, one hub title with three variants |
| intros "The Fraternity of Hunters" / "The Order Revealed" | Bratrstvo lovců / Řád odhalen | |
| Ending 1 Their Own Destiny | Strůjci vlastního osudu | |
| Ending 2 Overshadowed and Exploited | Zastíněni a zneužiti | |
| Ending 3 Local Legends | Místní legendy | |
| Ending 4 The Spread of Evil | Zlo se šíří | |
| Ending 5 To Be Rid of Evil | Zbaveni zla | Echoes *zbav nás od zlého* |
| Ending 6 There Could Never Be Peace | Míru nebylo souzeno | |
| Ending 7 A Legacy Will Live On | Odkaz bude žít dál | |
| Ending 8 heading "Editor's note – August 1923"; gallery name "An Uplifting Tale" | Poznámka redaktora – srpen 1923; „Povznášející příběh“ | |
| The Cost of Disease / Fear of the Unknown / A Time of War | Daň za nemoc / Strach z neznáma / Čas války | The box says the English name: show it on the scenario screen as *Daň za nemoc (The Cost of Disease)* (Q1) |
| Achievements: A Life's Work · A Workshop of Wonders · An Age of Invention · A Family of Geniuses · Master of All Trades | Dílo celého života · Dílna divů · Věk vynálezů · Rodina géniů · Mistr všech oborů | |
| Every Fate of Disease / Fear / War | Všechny osudy nemoci / strachu / války | |
| Respected / Renowned / Legendary Name · Shared Legacy · Dynasty | Vážené / Proslulé / Legendární jméno · Společný odkaz · Dynastie | |

Masterwork types (`NewMasterType_Content1`): Creature / Robot / Medicine / Demonic Creature → *Tvor / Robot / Lék /
Démonický tvor*. Superpower / Weapon / Drug / Ritual → *Nadlidská schopnost / Zbraň / Droga / Rituál*. "Robot" is
Čapek's Czech word: a pleasant anachronism, keep it.

---

## 4. Names and places

- **Typed-in names** (Oliver/James/Olivia, Town/London/Dubai/Paris, Creation) are previews only. Keep the previews
  untouched (section 2.1).
- **Fixed characters, declined normally:** Dr. Ensign Benwallis (*Dr. Ensigna Benwallise*; the editor's "Hello Ensign"
  → *Milý Ensigne* or *Vážený pane Benwallisi*); Ensign Benwallis Jr. → *Ensign Benwallis ml.*; Celia Benwallis
  Cartwright; Nurse Varga → *sestra Vargová*; Hagen and Mabel Varga → *Hagen a Mabel Vargovi* (Q7).
- **Titles:** Dr. → *Dr.*; Jr. → *ml.*; Mayor → *starosta*; bishop → *biskup*; diocese → *diecéze*; vicar → *vikář*;
  Sheriff → *strážmistr*; constabulary → *četníci*; police station → *strážnice*; St. → *sv.* (*svatý Hubert*);
  God → *Bůh*; Grandfather → *děd*; father → *otec*.
- **Historical setting (1859–1923):** Hungary → **Uhry**, Hungarian → *uherský*, Austro-Hungarian Empire →
  *Rakousko-Uhersko* (Q5; alt. *Maďarsko/maďarský*); Budapest → *Budapešť*; Bucharest → *Bukurešť*; Romanian →
  *rumunský*; Carpathian mountains → *Karpaty*; Rovinj → *Rovinj*; the Mediterranean → *Středomoří*; Eastern Europe →
  *východní Evropa*; London → *Londýn*; Paris → *Paříž*; Persian / Turkish carpets → *perský / turecký*; "bohemian
  glass piece" → *kus českého křišťálu* (a pleasant nod for Czech readers); Grecian Panacea → *řecká panacea*;
  Pythagorean diet → *pythagorejská strava*; pálinka → *pálinka*; War to End All Wars → *válka, která měla ukončit
  všechny války*.
- **Real works and terms:** Nobel Prize in Medicine → *Nobelova cena za lékařství*; phrenology → *frenologie*;
  formaldehyde → *formaldehyd*; carbolic acid → *karbolová kyselina*; Tarot → *tarot*.
- **Game title:** *My Father's Work* stays in English (the brand on the box). The app title "My Father's Work
  (Unofficial)" → *My Father's Work (neoficiální)* (Q9).

---

## 5. Open questions for the reviewers

| # | Question | Recommendation |
|---|---|---|
| Q1 | How should Czech terms relate to the English components? | Czech everywhere. English in parentheses at the first mention in each setup/rule popup, only for card, tile and token names and Chronicle locations players must find or place. Scenario names shown once in English on the scenario screen. (Rejected: English component names in Czech text, which is jarring; Czech only, which is hard to match at the table.) |
| Q2 | Masterwork: *veledílo* or *mistrovské dílo*? | *veledílo*: one word, literary, easy to decline. |
| Q3 | Self: *osobní figurka* or *figurka vědce*? | *osobní figurka* / *osobně* (gender-free). |
| Q4 | Creepy: *děsivost*, *podivínství* or *zlověstnost*? | *děsivost / děsivý*. |
| Q5 | Hungary: historical *Uhry* or modern *Maďarsko*? | *Uhry / uherský* (period setting). The chronicler writes in 1923, so either works. |
| Q6 | First-person journals and sworn statements whose tag contains no player name (`Trigger35Points_A/B_Content`, `PanaceaUnleash_Content`, `MWComplete_Content1-8`, `Barventures_Content1-7`, `NewMasterDiscipline_Content`) need gendered past tense. | **Done:** `[[g:…]]` with no player name before it in the same text uses the first player named anywhere on the screen (title, other paragraphs, popup). Use it in these texts; where no player is named on the screen at all, it falls back to masculine, so rephrase without gender there. |
| Q7 | Add *-ová* to fixed female surnames (Vargová, Benwallisová Cartwrightová)? | Yes for Nurse Varga and the Vargas (literary norm). Keep Celia Benwallis Cartwright unchanged as a full foreign name. |
| Q8 | Monster names: Czech-ise (*pricolič*) or keep the Romanian form? | Czech-ise where a Czech form exists (*strigoj, pricolič*), keep the rest. |
| Q9 | The app name in Czech? | *My Father's Work (neoficiální)*. Repository names unchanged. |
| Q10 | *klepnout* (touch) or *kliknout* (mouse)? | *klepnout*. The Windows version is the minority. |
| Q11 | Should the app insert non-breaking spaces (after v, s, z, k, o, u, a, i, between a number and its unit, before VB) centrally? | **Done:** the app inserts them after v, s, z, k, o, u, a, i in Czech. Translators type plain spaces. |
| Q12 | Village name secret codes (Rage, Kraven, Plague, Mercy) are compared in code. | Keep the English codes, explain them in Czech in Help. |
| Q13 | Oliver narration is English audio, and sentence highlighting matches the English text. | **Done:** a Czech story is silent (owner: Czech starts without narration). |
| Q14 | `[[pl]]` and zero: Czech uses the 5+ form for 0 ("0 her"). | **Done:** 0 and 5+ take the third form, also 11–14. |
| Q15 | Gender picker wording at name entry (Czech only). | Label `PlayersNameEntry_Gender` + chips `Gender_Male` / `Gender_Female`, default masculine (English: "Your character is: a gentleman / a lady"). Proposal: *Vaše postava je: pán / dáma*, or *Oslovení: pan / paní*. |

---

## 6. Terms settled during the first translation

Terms the eight translators had to coin (not in sections 3 and 4), as unified by the editing pass (November 2026).
They are binding like the rest of this file. Window titles are listed where other texts repeat them (Log Book, Guide,
rules screens). The settled answers to the open questions (section 5) are not repeated here: the gender chips read
*Vaše postava je: pán / dáma*.

### 6.1 Game and rules terms

| English | Czech | Notes |
|---|---|---|
| the Fates (Guide: which test decides a branch) | sudičky | "the Fates test the building" → *sudičky zkoušejí budovu*. |
| Confront / Believe (the Hunters) | postavit se lovcům / uvěřit lovcům | Guide and the vote links ("pokud jste hlasovali, že se lovcům postavíte"). The vote screen's "Challenge the Hunters / Accept their story" → *zpochybnit tvrzení lovců / přijmout jejich verzi*. |
| Cleanse (the vote, the Wolves branch) | očista; *očistný odvar* | Vote link: "pokud jste hlasovali pro očistu". |
| concoction | odvar | |
| unleash (the Panacea, the concoction) | vypustit; vypuštění | Opposite of *zveřejnit* (publish). |
| encourage / not encourage the Frenzy | podpořit / nepodpořit Běsnění | Rules, links and Guide (the Guide said *podnítit* before the editing pass). |
| hunt won / successful hunt | úspěšný hon | A hunt as an event is always *hon* (pl. *hony*): "po 2 úspěšných honech". |
| Hearts (the charity count) | srdce | Token: *žeton srdce*. |
| Investigation (window titles) / investigate (rules) | Pátrání / prošetřit, prošetření | Titles: *Pátrání*, *Pátrání pokračuje*, *Nejvíce pátrání*. |
| Sciences (the Gen I choice) | vědní obor | "Zvolte, kterému vědnímu oboru se naše rodina bude věnovat". |
| Masterwork Experiments (the cards) | karty veledíla | |
| Masterwork Experiment (a named one) | veledílo + name | "Má některý hráč veledílo <b>Lykantropická síla</b>?" |
| Nasty Rumor (35 VP choice) | ošklivé pomluvy | Window title „Ošklivé pomluvy“. |
| Unfortunate Accident (35 VP choice) | nešťastná náhoda | Window title „Nešťastná náhoda“, choice "Stane se mu nešťastná náhoda." |
| seedy dealings | nekalé obchody | |
| penalty | pokuta (money), postih (a list of losses to choose from), penalizace (VP at scoring) | |
| Biology award | ocenění za biologii | |
| research marker | ukazatel výzkumu | |
| mainstream merit | punc uznávané vědy | |
| Ties are broken by the last player in turn order. | Při shodě rozhoduje poslední hráč v pořadí tahů. | Vote screens: "Sečtěte všechny hlasy. Vítězí strana s více hlasy." / "Klepněte na výsledek svého hlasování:". |
| Then, since this is a 3-player game, pass the starting player marker clockwise 1 additional time. | Protože hrajete ve třech, předejte poté žeton začínajícího hráče ještě o jedno místo dál po směru hodinových ručiček. | One sentence in every setup. "Pass the Starting Player token as normal." → *Předejte žeton začínajícího hráče jako obvykle.* |
| spot A/B/C on the Village Chronicle | místo budovy A/B/C v Kronice vesnice | |
| Click Here (UI link inside a hub sentence) | klepněte sem | Lower case: it always completes a sentence ("Pokud hráč…, klepněte sem"). Also *klepnutím získáte zvláštní odměnu*. |

### 6.2 Story terms

| English | Czech | Notes |
|---|---|---|
| operative (of the Order / Fraternity) | zmocněnec | The Order's messenger → *posel Řádu*, the envoy (she) → *vyslankyně*. |
| skilled workers | zruční řemeslníci | |
| operating theater | anatomický sál (the Symposium's lecture hall), operační sál (a hospital's) | |
| bath chair | pojízdné křeslo | |
| county | župa | |
| SWORN STATEMENT | PŘÍSEŽNÉ PROHLÁŠENÍ | Bar-venture statements. |
| Bar-venture | hospodská eskapáda | Window title *Hospodská eskapáda*; link "vydejte se na hospodskou eskapádu". |
| morningstar / glaive | kropáč / glévie | |

### 6.3 Window titles repeated elsewhere

| English | Czech |
|---|---|
| Attracting Attention / The Mark of Genius | Vzbuzená pozornost / Znamení génia |
| Election Season (Second round Event – …) | Čas voleb |
| A Bid for Mayor / Direction of the Town | Dražba o úřad starosty / Budoucnost města |
| Financial Woes | Finanční strasti |
| Legacy of Charity / A Legacy of Charity / Charity's Legacy | Odkaz dobročinnosti (all three) |
| Charity Awarded | Dobročinnost odměněna |
| Too Old | Na sklonku života |
| Board of Trustees | Správní rada |
| Clandestine Meetings / Relief from Disease / Suspicion | Tajné schůzky / Úleva od choroby / Podezření |
| Death Becomes Us | Smrt nám sluší |
| A Gift of Spawning / The Hunter's Tax | Dar líhně / Daň pro lovce |
| Taxation / Final Taxation | Zdanění / Poslední zdanění |
| Masterwork Completion | Dokončení veledíla |
| The Hunt is On / The Hunt Begins / Endless Hunt | Hon je v plném proudu / Hon začíná / Nekonečný hon |
| Safe and Protected | V bezpečí a pod ochranou |
| The Monster Without | Netvor zvenčí |
| Engineering Achievement / Scientific Achievement | Inženýrský počin / Vědecký počin |
| Experiments are Feared | Strach z experimentů |
| Overrun By Demons / Monsters | Napospas démonům / netvorům |
| A Call for Assistance | Žádost o pomoc |
| Once More the Accursed Contribution | Znovu prokletá daň |
| Deplorable Equity | Zavrženíhodná rovnost |
| The Ultimate Disease | Nemoc všech nemocí |
| Hungarian Bright | Uherské osvícení |
| Masters of Their Own Destiny | Páni vlastního osudu |
| Scoring / Rankings | Bodování / Pořadí |

### 6.4 UI

| English | Czech |
|---|---|
| Hide spoilers | Skrýt prozrazení děje |
| Local Save Game | Uložit hru v tomto zařízení |
| How to use (Help tab) | Návod k použití |
| Village name secrets (Help tab) | Tajná jména vesnic |
| Highlight styles: Quill line / Ember by candlelight / Travelling spark | Linka brkem / Uhlíky ve svitu svíčky / Putující jiskra |
| Branch and ending names of *Fear of the Unknown* and *A Time of War* | see the `Gallery_*` rows of `cs/UI_Localization.csv` |
