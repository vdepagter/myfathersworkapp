# Czech glossary extension: *A Time of War* (*Čas války*)

Scope: `ATimeOfWar_Localization.csv` (story), `ATimeOfWar_Gameplay_Localization.csv` (rules), `ATimeOfWar_Guide.csv`,
`ATimeOfWar_LogBook.csv`. Read [`cs-glossary.md`](cs-glossary.md) first: it stays binding (base-game terms from the team
translating the official Czech rulebook: *mistrovské dílo*, *velmistrovské dílo*, *technika*, *sídlo*, *vylepšení sídla*,
*partner*, VB, `$2`, quotes, dashes, no semicolons). This file adds only what is specific to this scenario. Where the two
disagree, the main glossary wins, except for the **Caretaker**, which is *správce* in this scenario (rulebook team).

---

## 1. Tone

- **A memoir in the first person, three narrators.** Each generation's "I" is a different heir (a player), writing a
  self-congratulatory memoir about war, politics and time travel. Around it: an editor's foreword (`Foreword`), a
  20th-century coursebook (`FinalEnding_Content7`) and a fourth-generation post-script (`FinalEnding_Content8`). The
  register is the main glossary's chronicler voice, but vainer and more theatrical. The narrator is vain, petty and
  callous ("To hell with the people.", "Glory to ME."). Keep the deadpan and the boasting. Exclamations may stay
  exclamations: *Pryč s monarchií!*, *Ty hlupáci! Ti naprostí pitomci!*
- **First-person gender.** Many screens have no player name on them (secret pages handed to "the player with the
  Shield token", hub events). There `[[g:…]]` falls back to masculine. Write those passages without gender wherever
  Czech allows: dative and impersonal constructions (*Bylo mi jasné…, Zmocnil se mě vztek, Rozhodnutí padlo*), nouns
  (*Má volba byla jasná:*), present-tense comment, the reflexive passive. Use `[[g:…]]` only where a name is on the
  screen or no rephrasing works. First-person **link texts** ("I chose to sabotage the village.") become gender-free
  noun phrases or infinitives: *Sabotovat vesnici.* / *Má volba: sabotáž vesnice.*
- **Royal register** (Gen III *Heavy is the Crown*, the royal advisor, the Queen): mock-courtly Czech with a dash of
  Austro-Hungarian chancery style: *Jeho Veličenstvo, královský dvůr, rádce, výnos, ráčit, poddaní*. The advisor is a
  buffoon: let the courtly words sound slightly ridiculous. The "decrees" are the narrator's whispered suggestions to
  the advisor. The effect lines after a choice ("This will result in: …") stay plain rules Czech: *Následek: …*
- **War dispatches and military orders** (Military Demands, the Barracks, quartering, Separatist outposts): terse,
  bureaucratic and imperative for the Monarchy (*Monarchie žádá od svých poddaných službu.*), revolutionary
  for the Separatists (*občané, svoboda, Pryč s monarchií!*). Avoid words with a communist ring (*soudruh,
  lid pracující*). *Odboj* for "the resistance" is welcome: it rings true for Czech readers.
- **Time travel and paradox texts** (Gen II *Time Travel*, Gen III *Future Consequences* / *You Can't Stop Progress*):
  Victorian pseudo-science with a straight face (*proud času, čtvrtý rozměr, časová smyčka, mnohovesmír*). The invented
  devices keep their period flavour: radioscope → *radioskop*, zoetrope → *zoetrop*, zoopraxiscope → *zoopraxiskop*,
  Time Conveyance Device → *Přístroj k přepravě časem*. The fourth-wall jokes (`ParadoxTimeRandom_Content4`,
  `Warning_World_Content18`: the game "will see release in 2022", "the author … hasn't written the description") stay
  jokes, and only there may the wording sound modern.
- **Setting:** Hungary, 1831 (the time-travel past) to after 1901 (the famine). *Uhry, uherský* (main glossary Q5);
  Vienna → *Vídeň*; Hungarian plains → *uherské roviny*; Ferenczy (sculptor) → *Ferenczy, Ferenczyho*; borscht →
  *boršč*; Prometheus → *Prométheus*; Icarian → *ikarovský*; Swift's *A Modest Proposal* → *Skromný návrh* (the
  Czech title of the essay); Orwell's Big Brother → *Velký bratr*.

## 2. Grammar mechanics specific to this scenario

### 2.1 Names the code fills in from option lists: write them in the genitive

The code takes these words from shared option tags and drops them into many sentences, in every grammatical case.
A Czech form can only fit all of them if it is a **genitive complement after a declinable head noun** that each
sentence supplies and declines itself (*strana, budova, žeton, obor, řady, síly, postavení, vlajka*). Translate the
options as follows, and never let one of them stand alone as a subject or after a preposition:

| Tag | English options | Czech options (genitive) | Typical head nouns in the sentences |
|---|---|---|---|
| `Faction_Name` | ` ` / Separatists / Unified Monarchists | ` ` / separatistů / sjednocených monarchistů | *strana, tábor, řady, síly, vlajka, postavení, věc, stoupenci* |
| `Token_Name` | Sickle / Shield | srpu / štítu | *žeton, figurka ve tvaru* ("the player with the {{0}} token" → *hráč se žetonem {{0}}*) |
| `Building_Name` | ` ` / Barracks / Laborer's Union | ` ` / kasáren / Dělnického svazu | *budova* ("the {{1}} would fall" → *budova {{1}} padne*) |
| `TeaTarget_Name` | Barracks / Outpost | kasáren / základny | *budova* ("burning down the {{0}}" → *vypálit budovu {{0}}*) |
| `DuelField_Name` | Engineering / Chemistry / Biology / Occult | techniky / chemie / biologie / okultismu | *obor* ("prowess in {{0}}" → *zdatnost v oboru {{0}}*) |

The previews in the story CSV (`{{0=...|Separatists|...}}`, `{{0=...|Sickle|...}}`) stay as they are (main 2.4).
Option lists the translator owns inside one tag (`{{0=|Engineering|…|The Moment of Truth}}` in a title, the Guide's
`{{0=?|Unified Monarchists|Separatists}}`) are declined freely, in whatever case that sentence needs.

### 2.2 Regal titles typed in by a player ("Duke Anna")

In `Reign7` a player may choose "Glory to ME." and type a title (`Reign7c`). From then on the code shows
**title + space + name** (`DisplayName`) on the decree, statue and bonus screens: `{{0=...|Duke Anna|...}}`. Rules:

- The prompt (`Reign7c_Content`) asks for a Czech title in the nominative and gives both genders: *Zadejte svůj zbrusu
  nový šlechtický titul (např. vévoda, vévodkyně, hrabě, hraběnka, kníže, kněžna). Bude stát před vaším jménem.*
- Czech puts the title before the name, as the code does: *vévodkyně Anna, hrabě Oliver, král Bob*. The pair is a
  typed-in value: never decline it, and handle it like any name (main 2.1: subject, after a colon, or after a declinable noun).
- **Gender:** `[[g0:…]]` works in texts whose placeholder holds a title too: the app finds the player name inside
  "vévodkyně Anna" (Q4, fixed 26 Nov 2026). `[[g:…]]` also works.
- Regnal numbers follow main 1.4 (`{{0}} IV` → `{{0}} IV.`). "the fourth generation to take on the {{0}} name" →
  *čtvrtá generace nesoucí jméno {{0}}*. "Dr. {{0}}" stays *Dr. {{0}}*.

### 2.3 Other typed-in values

| Preview | What it is | Czech pattern |
|---|---|---|
| `Anna`, `Bob`, `Oliver` | player names | main 2.1 |
| `Testville` | the village name | *vesnice {{0}}* / *město {{0}}* / *kraj {{1}}* ("sponsored by the {{1}} region" → *pod záštitou kraje {{1}}*) |
| `Spam` | the new meat product typed in at the famine (`FamineYes`) | *maso značky {{0}}*, *příděly značky {{0}}*. Prompt: *Zadejte název nového masného výrobku:* |
| `Duke Anna` | regal title + name | 2.2 |

### 2.4 Town locations: Czech + English in brackets every time (rules, setup, hubs, UI)

Translator brief rule (rulebook team, 26 Nov 2026): every mention in rules, setup, hub and UI text, never in story prose,
Log Book summaries or story link texts. The bracket also goes into option lists and titles, after the declined Czech
word: *do kasáren (Barracks)*, *v kasárnách (Barracks)*, *u mechanika (Technician)*. Buildings take *do / v / z*
(*figurka zůstane v kasárnách (Barracks)*), the Town Hall *na* (*na radnici (Town Hall)*).

## 3. Term tables

### 3.1 Factions, politics, the crown

| English | Czech | Notes |
|---|---|---|
| Separatists; Separatist (adj.) | separatisté; separatistický | Lower case (Q1). Option tag: genitive (2.1). "Separatist token" → *žeton vlajky separatistů*. |
| Unified Monarchists; Monarchist (adj.) | sjednocení monarchisté; monarchistický | "Unified Monarchist Flag token" → *žeton vlajky sjednocených monarchistů*. |
| the Monarchy / monarchy | monarchie | "The Monarchy is not pleased." → *Monarchie není potěšena.* |
| the crown (the royal power) | koruna | "the ire of the crown" → *hněv koruny*. |
| His Majesty / the Queen / the royalty, royals | Jeho Veličenstvo / královna / královský dvůr, královská rodina | "Long live His Majesty." → *Ať žije Jeho Veličenstvo!* |
| royal advisor (person) | královský rádce | Story. The location is the Regional Advisor (3.3). |
| (royal) decree; make a decree | (královský) dekret; vydat dekret | Hub "Most Decrees" → *Nejvíce dekretů*. |
| Benevolent / Iron Fist decrees | dobrotivé dekrety / dekrety železné ruky | Czech idiom *vládnout železnou rukou*. Bonus titles: *Bonus za dobrotivost / Bonus za železnou ruku*. Move the noun into the options (example 4). |
| Royal Decision / Dilemma / Conundrum | Královské rozhodnutí / dilema / hlavolam | `Reign1` heading options. |
| "This will result in:" | Následek: | Decree links. |
| regal title; Duke / Duchess, King / Queen, Count / Countess | šlechtický titul; vévoda / vévodkyně, král / královna, hrabě / hraběnka | Typed in (2.2). Prince(ss) → *kníže / kněžna*. |
| Fate of the Town | osud města | Hub end links "…to determine the Fate of the Town" → *…a rozhodněte o osudu města*. |
| civil war; the resistance; uprising | občanská válka; odboj; povstání | "Victory for the Resistance" → *Vítězství odboje*. |
| tax, estate tax; taxation | daň, daň ze sídla; zdanění | "pays $1 in estate tax" → *zaplatí daň ze sídla $1*. |
| president / generals / the Empire (Peace hub) | prezident / generálové / Říše | Keep the irony of a republic calling itself an empire. |

### 3.2 Components of the scenario box (printed in English: bracket at first mention per popup, main Q1)

| English | Czech | Notes |
|---|---|---|
| Caretaker (piece) | **správce** (pl. správci) | This scenario's term (rulebook team). "takes a Caretaker of their color from Lost and places it on the Barracks" → *si vezme ze Ztracených (LOST) správce (Caretaker) své barvy a umístí ho do kasáren (Barracks)*. |
| Flag card / Flag token; "flip it Separatists side up" | karta vlajky / žeton vlajky (Flag); *otočte ho stranou separatistů nahoru* | |
| Sickle token / Shield token | žeton srpu / žeton štítu | Icons `S3_SickleToken`, `S3_ShieldToken`. Story: *kovová figurka ve tvaru srpu*. |
| Wooden Figure token | žeton dřevěné sošky (Wooden Figure) | **soška**, never *figurka*: *figurka* is a player piece. Story "misshapen wooden figure" → *neforemná dřevěná soška*. |
| Weapon token | žeton zbraně (pl. žetony zbraní) | "Weapon Token Bonus" → *Bonus za žetony zbraní*; "Weapon Cache" → *Skrýš zbraní*. |
| Advanced Weaponry (Experiment cards, deck) | Pokročilá výzbroj (karty Pokročilé výzbroje) | Story "advanced weaponry schematics" → *nákresy pokročilých zbraní*. |
| Crown token | žeton koruny | On the Creepy track (*stupnice děsivosti*). |
| Paradox token | žeton paradoxu | |
| Time Machine (Masterwork; Update card) | Stroj času (mistrovský experiment Stroj času; karta Úpravy mistrovského díla Stroj času) | Wells' Czech title. Story: *stroj času*. |
| Time Machine Repairs (cards) | karty Oprav stroje času (Time Machine Repairs) | "Repair the Time Machine" → *Oprava stroje času*. |
| Time Paradox (cards) | karty Časových paradoxů (Time Paradox) | |
| Utopian Experiments | utopické experimenty (Utopian Experiments) | |
| Advanced Ingredient (tokens); basic Ingredient | pokročilá přísada (Advanced Ingredient); základní přísada | |
| World Conquest card; Final Battle side; Battle card | karta Dobytí světa (World Conquest); strana Závěrečné bitvy (Final Battle); karta bitvy | Same card. Occult cube → *kostička okultní znalosti*. |
| Temporary Housing / Suspicious Barn / In Memoriam (Estate Upgrades) | Provizorní ubytování / Podezřelá stodola / In memoriam | *vylepšení sídla*. "In Memoriam" stays Latin (Czech uses it). |
| Giant Masterwork; "Giant version of their Masterwork" | velmistrovské dílo; velmistrovská verze jejich díla | "Giant Masterwork Check" → *Kontrola velmistrovských děl*. |
| Score Track (Marker) / Creepy Marker | počítadlo vítězných bodů (ukazatel na počítadle) / ukazatel děsivosti | |
| Town board | herní plán | Main 3.1. |

### 3.3 Town locations and the Chronicle pages of this scenario (2.4)

| English | Czech | English | Czech |
|---|---|---|---|
| Barracks | kasárna (n. pl., gen. *kasáren*) | Outpost | základna (*předsunutá základna* in story) |
| Town Hall | radnice | Laborer's Union | Dělnický svaz |
| Church; Charitable Church | kostel; Dobročinný kostel | Commemorative Statue (location) | pamětní socha |
| Regional Advisor | krajský rádce | Technician | mechanik (Q6) |
| University | univerzita | Park / Bank / Blacksmith / Bunker | park / banka / kovárna / bunkr |

### 3.4 War, the barracks, sabotage and duels

| English | Czech | English | Czech |
|---|---|---|---|
| martial law; martial government | stanné právo; vojenská vláda | Military Demands / Service / Incentive | Požadavky armády / Vojenská služba / Pobídka armády |
| Lack of Service Penalty | Postih za neodvedenou službu | quarter troops; Quartering Enforced / Voluntary | ubytovat vojáky; Vynucené / Dobrovolné ubytování vojska |
| militia, lieutenant, officer, riflemen | domobrana, poručík, důstojník, střelci | conscription | odvody |
| bribe; "money in their fists" | úplatek; *peníze v pěsti* | battle, battlefield, territory | bitva, bojiště, území |
| "To Battle" / Final Battle | Do boje / Závěrečná bitva | Engagement / Campaign Complete | Střetnutí skončilo / Tažení skončilo |
| sabotage; saboteur | sabotáž; sabotér | duel, Code Duello, dueling grounds | souboj, Soubojový kodex, místo souboje |
| scientific prowess | vědecká zdatnost | pistols at twenty paces | pistole na dvacet kroků |
| Everything is a Weapon | Zbraní může být cokoli | Turn the Tide | Zvrat ve válce |
| Seeds of Conflict | Semena sváru | gas (Peace event) | plyn (*smrtící plyn*) |

### 3.5 Time travel

| English | Czech | English | Czech |
|---|---|---|---|
| time travel; time traveller | cestování časem; cestovatel časem | timeline; timestream | časová linie; proud času |
| time loop; Time Shift | časová smyčka; časový posun | Lost in Time | Ztraceni v čase (keeps the pun on *Ztracení*) |
| paradox; paradoxical | paradox; paradoxní | "Paradoxical Mending Reward" | Odměna za zacelení paradoxů |
| multiverse; fourth dimension | mnohovesmír; čtvrtý rozměr | Timeline Stabilized | Časová linie ustálena |
| schematics (time machine) | nákresy | release / keep hidden (the schematics) | zveřejnit / utajit |
| Utopia, Utopian | utopie, utopický | great-grandfather; grandparents; progenitor | praděd; prarodiče; předek |

### 3.6 Branches, intros and endings (Gallery names in `cs/UI_Localization.csv` are binding)

| English | Czech | Notes |
|---|---|---|
| A Time of War | Čas války | Scenario. |
| Taking Sides (hub, Gen I) | Volba strany | |
| Martial Law (hub, Gen II); intros "Plagued by Conflict" / "Down with the Monarchy" | Stanné právo; *Sužováni válkou* / *Pryč s monarchií!* | |
| Time Travel (gallery) – hub title "Tides of War 2"; intro "A Return to Devastation" | Cestování časem; *Návrat do zkázy* | Hub title: Q3. |
| Monarchy Reigns (hub); intro "Heavy is the Crown" | Monarchie vládne; *Těžká je koruna* | |
| World Conquest (hub); intro "Peace Through War" | Dobytí světa; *Mír skrze válku* | |
| Peaceful Return / Not Again (hub, two variants); intro "Future Consequences" | Pokojný návrat / *Zase ne!*; *Budoucí následky* | Gallery: Pokojný návrat. |
| Forward to Progress (hub); intro "You Cant Stop Progress" | Vpřed za pokrokem; *Pokrok nezastavíš* | The Czech saying. |
| Endings 1–4 | Obří úspěch · Pozadu za dobou · Vráska v čase · Čas bez pout | `FinalEnding_Title` options in this order. |
| Endings 5–8 | Královská chvála · Jazýček na vahách · Dějiny užitých věd · Místní legenda | |
| The War to End All Wars / A Modest Proposal / THE END? | Válka, která ukončí všechny války / Skromný návrh / KONEC? | |

### 3.7 Log Book locations (no English brackets: the Log Book is story)

Publisher's Office → *nakladatelství*; Father's Manor → *otcovo panské sídlo*; Plantation → *plantáž*; Cafe → *kavárna*;
Hiding Place → *úkryt*; Estate / Estate Grounds → *sídlo / pozemky sídla*; Laboratory / Secret Laboratory →
*laboratoř / tajná laboratoř*; Royal Residence → *královská rezidence* (not *sídlo*); Royal Office → *královská
kancelář*; Post Office → *pošta*; Dueling Grounds → *místo souboje*; Battlefield → *bojiště*; Silver Mine → *stříbrný důl*;
Study → *pracovna*; Town Square → *náměstí*; Sitting Room → *salon*; Veranda → *veranda*; Vienna → *Vídeň*; Village →
*vesnice*; Town Hall / Barracks / Outpost / University → as in 3.3.

## 4. Worked examples

`{{n}}` abbreviates the full placeholder, which stays complete in the CSV.

| # | Tag | English | Czech (proposal) | Point |
|---|---|---|---|---|
| 1 | `ResolutionofEvent_0_Content` | All players who voted to build the Barracks receive a Flag token and flip it {{0}} side up{{2=?\| and <b>gain 2VP</b>}} | Všichni hráči, kteří hlasovali pro stavbu kasáren (Barracks), dostávají žeton vlajky (Flag) a otočí ho stranou {{0}} nahoru{{2=?\| a <b>získávají 2 VB</b>}} | genitive faction ("stranou separatistů"), bracket every time |
| 2 | `ATOWSabotageConfirm_Content` | I penned a letter to my contact within the {{0}}. The site had been chosen and the {{1}} would fall, weakening the {{2}}' position in the village. | Mému důvěrníkovi v řadách {{0}} odešel dopis psaný mou rukou. Cíl byl zvolen: budova {{1}} padne a postavení {{2}} ve vesnici bude oslabeno. | three genitive fills, no gendered past on a nameless secret screen |
| 3 | `DecreeHandOver`, `Reign7c2_Title`, `Reign7c2_Content` | Carefully hand the Storybook to {{0}}. This passage is then read aloud to all players. / Thank You, {{0}} / …I thrice reminded him to refer to me as {{0}}. Ah, {{0}}. Such a glorious title! | Opatrně předejte Knihu příběhů hráči: {{0}}. Úryvek se pak čte nahlas všem hráčům. / Poděkování – {{0}} / …Třikrát jsem mu [[g:připomněl/připomněla]], ať mě oslovuje novým titulem: {{0}}. Ach, {{0}}! Jak skvostně to zní! | title + name after a colon or dash, no vocative, `[[g:]]` (it finds the name in the window title) |
| 4 | `BenevolenceBonus_Player`, `_Tied`, `_0_Content` | {{0}} : {{1}} {{2=?\|Iron Fist\|Benevolent}} decrees.<br /> / Multiple players are tied, so no player / {{0}} gains {{1}}VP. | {{0}}: {{1}} {{2=?\|[[pl:dekret železné ruky/dekrety železné ruky/dekretů železné ruky]]\|[[pl:dobrotivý dekret/dobrotivé dekrety/dobrotivých dekretů]]}}.<br /> / nikdo – více hráčů má stejný počet / Bonus {{1}} VB: {{0}}. | plural tokens inside options; the tie text fills `{{0}}`, so *nikdo* must not meet a positive verb (Czech double negation): the colon pattern fits both |
| 5 | `Warning_Visit_Title`, `Warning_Visit_Content` | Visit the {{0=\|Park\|Bank\|Church\|Town Hall\|Blacksmith\|Bunker}} / …an action at the <b>{{0=\|Park\|…}}</b> location: | Návštěva {{0=\|parku (Park)\|banky (Bank)\|kostela (Church)\|radnice (Town Hall)\|kovárny (Blacksmith)\|bunkru (Bunker)}} / …akci <b>{{0=\|v parku (Park)\|v bance (Bank)\|v kostele (Church)\|na radnici (Town Hall)\|v kovárně (Blacksmith)\|v bunkru (Bunker)}}</b>: | each tag declines its own options; bracket inside the option |
| 6 | `FamineYesResult_Content` | With an influx of fresh, surprisingly tender cuts of <b>{{0}}</b> and the introduction of salted, canned <b>{{0}}</b> rations… | S přívalem čerstvých, podivuhodně křehkých plátků masa značky <b>{{0}}</b> a se zavedením solených konzervovaných přídělů značky <b>{{0}}</b>… | typed product name after *značky* |

## 5. Open questions

| # | Question | Recommendation |
|---|---|---|
| Q1 | Capitalise the factions (*Separatisté, Sjednocení monarchisté, Monarchie*)? | Lower case, as Czech writes adherents of a cause (*husité, monarchisté*). Only the printed English in brackets keeps capitals. |
| Q2 | The Caretaker is *opatrovník* in *The Cost of Disease* and *správce* here. | Settled for this scenario. Ask the rulebook team whether it is the same physical piece: if it is, use *správce* in both and update the main glossary. |
| Q3 | The Time Travel hub's English title is "Tides of War 2" (source quirk), the Endings Gallery says "Time Travel". | Title the hub *Cestování časem*, so the hub, the gallery tree and Help (*Tajná jména vesnic*) agree. |
| Q4 | `[[g0:…]]` resolves to masculine when the name carries a regal title ("vévodkyně Anna"). | **Done (26 Nov):** a bound value that contains a player name ("vévodkyně Anna") takes that player's gender, so `[[g0:…]]` works with the regal title too. |
| Q5 | Nameless first-person secret screens fall back to masculine. | Gender-free phrasing (section 1). Where impossible, accept the fallback rather than a slash form. |
| Q6 | Technician: *technik* clashes with *technika* (Engineering) (*u technika* = "at the Technician" and "of Engineering"). | *mechanik (Technician)*, unless the rulebook team has a term. |
| Q7 | Base-game action names used here are not in the main glossary: Estate Affairs, Record Knowledge. | Ask the rulebook team. Placeholders: *akce Záležitosti sídla (Estate Affairs)*, *akce Zapsat znalost (Record Knowledge)*. |
| Q8 | *Uhry* in the 20th-century coursebook (`FinalEnding_Content7`) and the future paradox (`ParadoxTimeRandom_Content7`)? | *Uhry* in the memoir; *Maďarsko* in the coursebook only, which is written decades later. |
| Q9 | The town-location bracket in hub **titles** makes them long on a phone ("Požadavky armády – kasárna (Barracks)"). | Keep it everywhere as the rule says, and check the width on a phone in the playtest. |

---

## 6. Terms settled during the first translation

Terms the seven translators had to coin (not in sections 3 and 4), as unified by the editing pass (26 Nov 2026). They
are binding like the rest of this file. Window titles are listed where other texts repeat them (Log Book, Guide, rules).

### 6.1 Decisions of the editing pass

| Point | Settled | Notes |
|---|---|---|
| The narrator's spouse (story) | **má drahá polovička** (*s mou drahou polovičkou, mé okouzlené polovičce*) | Gender-free, no `[[g:…]]` tokens (the spouse's gender is not the narrator's). *Partner* stays the game piece. |
| "masterpiece" outside the game term | **mistrovský kus** | Never *veledílo* (rejected by the rulebook team). The game term stays *mistrovské dílo*. |
| Hungary | *Uhry, uherský* in the memoir (also the future paradox `ParadoxTimeRandom_Content7`); *maďarský národ* only in the 20th-century coursebook (`FinalEnding_Content7`) | Q8 as recommended. |
| Giant Time Machine / Giant … (ending 1) | *obří stroj času, obří pavoučí vůz…* | Story wordplay ("an even BIGGER version"); rules say *velmistrovské dílo*. |
| Family win (`FinalEnding_Family` = *Rodina*) | Endings 3, 5, 6, 7 and 8 rephrased without gender: *to já, {{0}}, stojím za vydáním, sepsáním i sestavením…*, *v čerstvě svěřeném úřadu královského rádce*, *já, {{0}}, znám sladkost vítězství*, *pod autorským vedením: Dr. {{0}}*, *{{0}} pochází z okraje města…*, *zůstávám navždy k Vašim službám* | Ending 1 (*přísahám při svém jménu, {{0}} IV.*) cannot hold "Rodina IV." gracefully (the English "The family IV" cannot either). |
| A Log Book "cousin" (gender unknown) | *jeden z dědiců*; "the cousins" → *bratranci a sestřenice*, *příbuzní* | |

### 6.2 Rules and button phrases

| English | Czech |
|---|---|
| All players may again view the Storybook. | Všichni hráči se nyní opět mohou dívat do Knihy příběhů. |
| Carefully hand / give this Storybook device to the player with … and do not allow any other players to see the screen. | Opatrně podejte zařízení s Knihou příběhů hráči se žetonem … a nedovolte ostatním hráčům vidět na obrazovku. |
| Carefully hand the Storybook to {{0}}. This passage is then read aloud to all players. | Opatrně předejte Knihu příběhů hráči: {{0}}. Úryvek se pak čte nahlas všem hráčům. |
| Click here at the end of the round to move to the next round... | Na konci kola přejděte do dalšího kola… |
| Click here at the End of the Generation for Final Scoring and to determine the Fate of the Town. | Na konci generace přejděte k závěrečnému bodování a rozhodněte o osudu města. |
| Click here (at the End of the Generation) to determine the Fate of the Town. | (Na konci generace) pokračujte a rozhodněte o osudu města. |
| Click here to start the bribe... | Zahajte sbírku na úplatek… |
| BRIBE / Total Bribe | ÚPLATEK / Celkový úplatek |
| VOTE / Votes Tallied / Vote Complete / Vote Result | HLASOVÁNÍ / Hlasy sečteny / Hlasování skončilo / Výsledek hlasování |
| BID / Bid / Bid Complete / Bid Result, Bid Outcome / Bid Resolved | DRAŽBA / Dražba / Dražba skončila / Výsledek dražby / Dražba vyhodnocena |
| Yay / Nay in links | PRO / PROTI; "(Yay!)" → *(PRO!)* |
| player closest to the Crown token without overlapping or exceeding it | hráč nejblíže žetonu koruny, aniž by s ním stál na stejném poli nebo ho překročil |
| Church explanation (hub) | *Přečtěte si zde* + *vysvětlení, jak funguje kostel (Church).*; window title *Dobročinný kostel (Charitable Church)* |

### 6.3 Story terms

| English | Czech |
|---|---|
| messenger (she) | poslíčkyně |
| caretaker (servant at the estate) | správce (same word as the piece) |
| figurine in the shape of a {{0}} | figurka ve tvaru {{0}} |
| misshapen wooden figure / carving | neforemná dřevěná soška / řezba |
| scientific / pistol duel | vědecký souboj / souboj v pistolích |
| quartering edict | výnos o ubytování vojska |
| mustard gas concoction | yperitový odvar |
| radioscope / zoopraxiscope / zoetrope | radioskop / zoopraxiskop / zoetrop |
| Tiger Launcher, Psychic Mindbomb, Helmet-Mounted Flamethrower, Steam Grenade, Dream Stealer… | tygromet, psychická mozková bomba, plamenomet upevněný na přilbě, parní granát, mechanizovaná lidská zbroj, zloděj snů… (the coursebook declines the same words) |

### 6.4 Hub section titles

| English | Czech | English | Czech |
|---|---|---|---|
| Town Hall – Political Rumblings | Radnice (Town Hall) – politické vření | Second Round – Pledge of Support | Druhé kolo – Příslib podpory |
| Knowledge / Ingredient / Wealth Bonus | Bonus za znalosti / za přísady / za bohatství | Weapons Bonus | Bonus za zbraně |
| Military Incentive – Barracks | Pobídka armády – kasárna (Barracks) | NEW … | NOVĚ: … |
| Repair the Time Machine | Oprava stroje času | Paradox Explanation | Vysvětlení paradoxů |
| Royals Take Notice / Royal Eye | Dvůr si všímá / Královské oko | Like a Whispering Shadow | Jako šeptající stín |
| Monarchist / Separatist Allegiance | Věrnost monarchii / Věrnost separatistům | Most Decrees | Nejvíce dekretů |
| War on the World / Final Conflict | Válka proti světu / Poslední střet | Office of Public Relations | Úřad pro styk s veřejností |
| Science is Stressful | Věda drásá nervy | Reward for Perseverance | Odměna za vytrvalost |
| My Great Grandfather's Masterwork | Mistrovské dílo mého praděda | Science At any Cost | Věda za každou cenu |
| Angry Mob Return | Návrat rozzuřeného davu | Visit the {{0}} | Návštěva {{0=…parku (Park)…}} |

### 6.5 Window titles repeated elsewhere (Log Book, Guide)

| English | Czech | English | Czech |
|---|---|---|---|
| A Memoir Across Three Generations | Paměti tří generací | Village Meeting | Shromáždění vesnice |
| Military Protection | Vojenská ochrana | Seeds of Conflict | Semena sváru |
| Quartering Enforced / Voluntary Quartering | Vynucené / Dobrovolné ubytování vojska | Attracting Attentions | Vzbuzená pozornost |
| A Time for Sabotage | Čas sabotáže | Demolition for Change | Demolice ve jménu změny |
| The Target of My Wrath | Terč mého hněvu | Victory for the Resistance / Resistance Quashed | Vítězství odboje / Odboj potlačen |
| An Unfortunate Death / Wounded Pride | Nešťastná smrt / Raněná pýcha | The Moment of Truth | Okamžik pravdy |
| Fully Repaired / Rusty, But Worth a Try | Zcela opravený / Zrezivělý, ale za pokus to stojí | Suspicions Confirmed | Podezření potvrzeno |
| Thanks from the Advisor / Thank You (, {{0}}) | Poděkování od rádce / Poděkování (– {{0}}) | Where Duty Lies | Kam volá povinnost |
| Time Shifts / Time Shifts Naturally | Čas se posouvá / Čas se posouvá sám | Gift of Silence | Dar mlčení |
| The Future of Convenience | Budoucnost pohodlí | A Bend Towards Chaos / A Secure Future | Příklon k chaosu / Bezpečná budoucnost |
| Rebuilding / Monument to Progress | Obnova / Pomník pokroku | Engagement / Campaign Complete | Střetnutí skončilo / Tažení skončilo |
| Warning worlds (`Warning_World_Title`) | Zelený klid · Prolomení čtvrté stěny · Až příliš dokonalé · Velký bratr · Obojživelníci · Věk dinosaurů | Paradox events (`ParadoxTimeRandom_Title`) | Příval výkonnosti · Jasný cíl · Nikdo nezůstane pozadu · Zápisky z budoucnosti · Důvěra a shovívavost · Návrat podezření · Na mušce · Naprostá katastrofa |
| A Letter Out of Time / The Secret Laboratory (Log Book only) | Dopis mimo čas / Tajná laboratoř | | |
