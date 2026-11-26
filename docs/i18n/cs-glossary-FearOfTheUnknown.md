# Czech glossary extension: *Fear of the Unknown* (Strach z neznáma)

Scope: `FearOfTheUnknown_Localization.csv` (story), `FearOfTheUnknown_Gameplay_Localization.csv` (rules/setup),
`FearOfTheUnknown_LogBook.csv` and `FearOfTheUnknown_Guide.csv`. Read the main glossary `cs-glossary.md` first. It is
binding, and this file only adds to it. Where the two disagree, the main glossary wins, with one exception: the
Caretaker piece is **ošetřovatel** in this scenario (asylum orderlies). Branch and ending names come from the
`Gallery_*` rows of `cs/UI_Localization.csv`. They are already settled, so keep them.

Setting: Romania / Transylvania, 1831–1932. A family of mad scientists faces witch hunts, a hex game, the Romani
caravans, a Frankenstein-like Creature, a vampire Count, an asylum, a walled town, smugglers, a prison revolt with a
battle of mechanical warriors, and asylum case files.

---

## 1. Tone notes for this scenario

- **Letters and journals** (most story text): the cousins write in the first person to an unnamed confidant. Use the
  learned 19th-century register of the main glossary (1.1). Capitalise *Vy/Vás/Vám* in letters. Headers follow 2.13
  below (*Dopis – {{0}} – 14. července 1831*). First-person past tense needs gender: use `[[g:…]]`. Better still,
  rephrase without gender: "It was I who was chosen." → *Volba padla na mě.*
- **Telegraphic diary notes** ("Inspection day: Wore newest overcoat, but received no compliment…"): use a nominal
  style. It sounds right and it carries no gender: *Den prohlídky. Nejnovější svrchník – a ani slovo uznání.*
- **Newspapers** (*Novinová zpráva*, editorials, the *Zpravodaj* of the town): period journalese, impersonal and
  passive: *Včerejšího večera byl…, Jak se dovídáme…, Redakce s politováním oznamuje…*
- **Asylum case files** (Patient Entry Report, Assessment Results, Admittance Log, the final Case File): clinical,
  dry and telegraphic. Use Latinate words of the period (*subjekt, pacient, melancholie, mánie, bludy velikášství,
  laudanum, elektroterapie, bazální metabolismus*) and verbless or passive sentences: *Podána mírná sedativa.
  Doporučen klid na lůžku.* *Subjekt* (m.) is the clinical word for any patient, so options that open with "Subject…"
  stay gender-free in the present tense. Use `[[g0:…]]` only where a sentence names the player as "Patient {{0}}".
  The director is *ředitel*. The Count's mock reports (CountAssessmentResults) are the same form, spoiled by gushing
  flattery or disgust ("Uggh." → *Fuj.*).
- **Mob voices** (torches, pitchforks, the hounds at the gate): short and loud. Use plural imperatives and coarse
  rural words, but no dialect spelling: *Upalte ji! Ven s čarodějnicí!* The source keeps the mob indirect, so keep it
  rare.
- **The Creature**: slow, heavy, wounded dignity. Use short clauses, bookish but blunt words, no slang and no
  spelling mistakes (his letter has "poor penmanship", not poor spelling). He says *vy* to the family ("Dlužíte mi
  velký dluh.") and speaks of himself in the **masculine** (*byl jsem opuštěn*), although the narrator calls him
  *Stvůra* (f., see 2.6).
- **The Count**: courtly, archaic and over-polite (*můj drahý příteli* is fine without a typed name). He says *vy*.
  His accent joke ("I struggle to pronounce my W's") becomes a Czech one: *a čím dál víc mi dělá potíže vyslovit „ř“*.
- **The warden** (the prison-revolt leader, "The Butcher" / "The Bloodsmith"): rough, triumphant, the hierarchy
  turned upside down. Use light common-Czech touches (*teď to tu vedem my*) and **tykání** towards the aristocrat
  cousin (Q2). The guard's butcher puns ('Grinder', "Nice to meat you", "no beef with you") become Czech puns (e.g.
  *Mlejnek*). Never translate them literally.
- **Obsession monologues**: megalomaniac, breathless, exclamatory. They may be more purple than the chronicle voice.
- **The battle announcer**: a fairground barker (*Dámy a pánové! Představuji vám dnešní bojovníky!*). The battle
  lines are in the present tense (see example 1).

---

## 2. Term table

Brackets: town locations get Czech + English **every time** in rules, setup, hub and UI text (*do kostela (Church)*).
Components get the English only at the first mention in a setup or rule popup (main glossary 3, Q1). Story text,
Log Book and Guide get no English.

### 2.1 Components of the scenario box (printed in English)

| English | Czech | Notes |
|---|---|---|
| Fear of the Unknown Scenario box | krabice scénáře Strach z neznáma | First setup: *Strach z neznáma (Fear of the Unknown)*. |
| Family Plot (Vanity Estate Upgrade) | Rodinný hrob (okrasné vylepšení sídla) | A grave plot on the estate. Window "A Separate Plot" → *Hrob stranou*. |
| Memorial Brick (token) | pamětní cihla, žeton cihly | "Brick token" → *žeton cihly*. Bricks as money in Tension → *cihly*. |
| Key to the City (Vanity Upgrade) | Klíč od města | |
| Witch's Hex token | žeton Kletba čarodějnice | Short form in rules after first mention: *žeton kletby*. |
| Market token | žeton trhu | Given by the Romani caravan. |
| Romani token | žeton Romů | Tension track token. |
| Obsession cards (18) | karty posedlosti | Popup title "Obsession!" → *Posedlost!* See 2.4 for the card names. |
| Puppy / Bride (Special Experiment cards) | Štěně / Nevěsta (karty zvláštních experimentů) | Bride echoes *Frankensteinova nevěsta*. |
| The Creature Masterwork Upgrade card; THE CREATURE Masterwork Experiment | karta Úpravy mistrovského díla Stvůra; mistrovský experiment STVŮRA | Consistent with *Stvůra se vrací*. |
| Ball and Chain / Ball & Chain (Estate Upgrade) | Koule na noze | The Czech idiom for a marriage as a burden. It keeps the joke. |
| House on the Hill (tile) | Dům na kopci (dílek) | Also a town location once placed (2.12). |
| Asylum Labor Union / Key to the Asylum / Asylum Share | **Odborový svaz ústavu** / Klíč od ústavu / Podíl v ústavu | Estate Upgrades, value of tag `AsylumUpgrade` (2.13). |
| Blessing of the Strigoi (card) | Požehnání strigojů | *strigoj* as in the main glossary. |
| Hunt token (yellow crosshairs front, red Clue back) | žeton honu (žlutý zaměřovač / červená stopa) | Clue → *stopa*. "expedition amount" → *potřebný počet stop*. |
| Collaborative Experiment (cards); Collaboration token | společný experiment; žeton spolupráce | |
| Player (coloured money) token | investiční žeton (v barvě hráče) | Hub "Investment in Town" → *Investice ve městě*. |
| Battledome Warrior tile (cross-shaped) | dílek válečníka (*Battledome Warrior*) | |
| Rehabilitation token | žeton rehabilitace | |
| Cabinet of Curiosities (Vanity Upgrade) | Kabinet kuriozit | |
| Menagerie (Estate Upgrade) | Zvěřinec | |
| Collaborative Experiments named on cards | Rozrušená gorila napíchaná hormony a radiem / Látka, kterou nelze ušpinit / Robotický sluha, který vaří lasery | Bracket at first mention (`Preparations_0`). The Log Book titles follow them. |

### 2.2 Tracks, pieces, actions (on top of main glossary 3.1–3.2)

| English | Czech | Notes |
|---|---|---|
| Caretaker (piece) | **ošetřovatel** (pl. ošetřovatelé) | This scenario only. Victoria Stoker "your caretaker" (a guardian, story) → *poručnice*. |
| Angry Mob marker / track | ukazatel rozzuřeného davu | The source once says "Angry Mob track" for the Creepy track: write *stupnice děsivosti*. |
| Suspicion Marker | ukazatel podezření | |
| TOWN (board area) / TOWN Location | MĚSTO (*TOWN*) / místo ve městě | |
| Location (Chronicle spot) | místo | "This Location is hexed" → *Na toto místo padla kletba.* |
| Self token / piece | osobní figurka | Both source words, one term. |
| black / white border (Tension) | černý / bílý okraj | |
| expand to a new plot | otevřít novou parcelu | |

### 2.3 Generation I – Witch Hunt (Hon na čarodějnice)

| English | Czech | Notes |
|---|---|---|
| witch / witchcraft / witch hunt | čarodějnice / čarodějnictví / hon na čarodějnice | *Čarodějnice* is the accusation, whatever the player's gender, as in the hub title. |
| Blacksmith / Butcher | kovář / **řeznice** | The Butcher is Henriette (Henrietta), a woman: *řeznice* in story, rules, vote rows and Guide. The execution vote: PRO = *kovář*, PROTI = *řeznice* (*Hlasovali jsme pro smrt řeznice*, *stánek řeznice*). Her heirs' trade adjective stays *řeznický* (*Řeznický učeň*). |
| Public Execution | veřejná poprava | The source says both hanged and burned: translate each passage as written. |
| Accuse! / accusation / point a finger at | OBVIŇTE! / obvinění / ukázat prstem na | "fingers pointed at them" → *prsty namířené na něj* (gender-free: *hráč, na kterého ukazuje nejvíc prstů*). |
| milestones: Ceaseless Ambition / Boundless Study / Pressing the Boundaries / Feared and Misunderstood | Neutuchající ctižádost / Bezmezné studium / Za hranice možného / Strach a nepochopení | |
| PARANOIA – Town Hall | PARANOIA – radnice (Town Hall) | Hub title. |
| Second Round Event | událost druhého kola | |
| the Fates (which test decides) | sudičky | Main glossary 6.1. |

### 2.4 Obsessions (Generation II and III)

| English | Czech | Notes |
|---|---|---|
| Obsession / Obsession Deepens / Deepen | posedlost / POSEDLOST SE PROHLUBUJE / prohloubit posedlost | "did not DEEPEN" → *neprohloubil svou posedlost* (or present: *kdo neprohloubí*). |
| white / red diamond | bílý / červený kosočtverec | |
| Progression / Regression | Postup / Ústup | `Obsessions_Title`. |
| Isolation / Exploration / Agitation / A Calmer Mindset | Izolace / Objevování / Rozrušení / Klidnější mysl | `ObsessionsAltered_Title`. |
| FOR FAME / KNOWLEDGE / SCIENCE / REVENGE / RESPECT / ENTERTAINMENT | PRO SLÁVU / PRO POZNÁNÍ / PRO VĚDU / PRO POMSTU / PRO ÚCTU / PRO ZÁBAVU | Card names, capitals, English bracket at first mention: *posedlost PRO SLÁVU (FOR FAME)*. |
| completed: INFAMY / LIMITLESS KNOWLEDGE / TRUE SCIENCE / UTTER REVENGE / COMMANDING RESPECT / WITHOUT RESTRICTION | HANEBNÁ PROSLULOST / BEZMEZNÉ POZNÁNÍ / PRAVÁ VĚDA / NAPROSTÁ POMSTA / VYNUCENÁ ÚCTA / BEZ OMEZENÍ | After *kartu* as a label: *získává kartu <b>PRAVÁ VĚDA</b>*. |
| not completed: RISING STAR / GRAND KNOWLEDGE / THE TRUEST SCIENCE / SWEET REVENGE / DEMANDING RESPECT / THE WORLD MUST BURN | VYCHÁZEJÍCÍ HVĚZDA / VELKÉ POZNÁNÍ / TA NEJPRAVĚJŠÍ VĚDA / SLADKÁ POMSTA / VYŽADOVANÁ ÚCTA / SVĚT MUSÍ SHOŘET | |
| Obsession Penalty | Postih za posedlost | Main glossary: *postih* = a loss to suffer. |
| A Life Forsaken / Conviction / Rejecting My Fate | Promarněný život / Přesvědčení / Odmítám svůj osud | Crossroads scenes. At a Crossroads → *Na rozcestí*. |

### 2.5 Generation II – Blame it on the Foreigners (Mohou za to cizinci)

| English | Czech | Notes |
|---|---|---|
| hex (n.) / to hex a building / hexed | kletba / seslat kletbu na budovu / pod kletbou | Not *zakletý* (fairy-tale ring). Curse (family) → *prokletí*, cursed (player) → *prokletý*. |
| Hexes and Hysteria (intro) | Kletby a hysterie | |
| A Game of Hexes / Witching Hour | Hra kleteb / Hodina duchů | *Hodina duchů* is the Czech idiom. |
| the witch is unmasked / the wrong person accused | čarodějnice je odhalena / obviněn nevinný | Guide, Comeuppance. |
| Rumor Mill | Klepy | Hub section and window. |
| Town Strength / Pressing Forward | Síla města / Kupředu | |
| memorial / Memorial Wall / a proper Memorial | památník / Pamětní hradba / důstojný pomník | The wall encloses the town (*Město za hradbami*). |
| Strength / Acceptance (wall vote outcomes) | Síla / Přijetí | *Přijetí* is also the Gen III branch: same word on purpose. |
| Monetary VOTE | PENĚŽNÍ HLASOVÁNÍ | |
| Romani, Romani caravans | Romové, romské karavany | Never *cikáni* (the source deliberately avoids the slur). |
| Traveling Caravan / caravan / caravansery | kočovná karavana / karavana | Window "A Visit From the Caravans" → *Návštěva karavan*. |
| A Small Choice / Fortune / Misfortune | Malá volba / Štěstí / Neštěstí | |
| Most Honored Philanthropist | Nejváženější dobrodinec | |

### 2.6 Generation II – The Creature Re-emerges (Stvůra se vrací)

| English | Czech | Notes |
|---|---|---|
| the Creature, the Monster | **Stvůra** (f., capitalised as a name); netvor, monstrum in prose | The narrator agrees in the feminine with *Stvůra* (*Stvůra promluvila*). For "he" in description, *netvor* (m.) works. Signature "The Creature" → *Stvůra* (Q3). |
| Towards Civility (intro) | Vstříc civilizovanosti | |
| grandfather's creation | dědovo dílo / dědův výtvor | Fixed noun, possessive allowed. |
| the Count / Count Vlad III Dracula / the Businessman | hrabě / hrabě Vlad III. Dracula / obchodník | *hrabě, hraběte, hraběti*. "thrall" → *otrok*. |
| debt / the Count's men | dluh / hraběcí muži | Collection Day → *Den splátky*. |
| Asylum (location) / asylum (story) | ústav (Asylum) / ústav, ústav pro choromyslné, blázinec (mob, scorn) | Sanatorium → *sanatorium*. |
| Asylum Support / Option to Donate / Private Investors | Podpora ústavu / Možnost přispět / Soukromí investoři | |
| Charitable Church | Dobročinný kostel | Window title, story. |
| Wracked with Guilt | Výčitky svědomí | Gender-free (the screen may belong to any player). |

### 2.7 The Hunt (Hon) – Creature branches of Generation III

| English | Czech | Notes |
|---|---|---|
| the Hunt / The Hunt is On | hon / Hon začal | Main glossary: an event is *hon*. |
| expedition / lead the expedition / set off | výprava / vést výpravu / vyrazit | "player(s) who…" → *hráč nebo hráči, kteří…* |
| The Expedition Uncovers… / The Creature is Killed / Escapes | Výprava odhalila… / Stvůra je mrtvá / Stvůra uniká | |

### 2.8 Generation III – Walled City (Město za hradbami)

| English | Czech | Notes |
|---|---|---|
| Isolation and Ignorance (intro) | Izolace a nevědomost | |
| Verification of Identity / Citizenship Inspection | ověření totožnosti / kontrola občanství | "civic duty obligations" → *občanské povinnosti*. |
| Rising Prices / Brazen Offer | Rostoucí ceny / Drzá nabídka | |
| smuggling / smuggler / Barely Legal Importation | pašování / pašerák / Sotva legální dovoz | |
| The Trial / Caught in the Act / Shameful Display | Soud / Přistižen při činu / Ostudné divadlo | Title without a player name keeps the masculine of the source idiom. |
| Collaboration / A Controlled Experiment / Missed Opportunity | spolupráce / Kontrolovaný experiment / Promarněná příležitost | |
| Town Hall Destroyed / Path of Destruction / The Rampage Concludes | Radnice zničena / Cesta zkázy / Řádění končí | |
| City Hall (source variant) | radnice (Town Hall) | Same location (Q5). |

### 2.9 Generation III – Tensions Increase (Napětí roste)

| English | Czech | Notes |
|---|---|---|
| hub title variants: Tensions Increase in Peace Time / The Enemy of My Friend / A Peace Through Generations | Napětí roste i v míru / Nepřítel mého přítele / Mír napříč generacemi | |
| Old Habits (intro) / Two Sides | Staré zvyky / Dvě strany | |
| Special Event – Resolution / Calm Before the Tempest / Comeuppance | Zvláštní událost – rozuzlení / Ticho před bouří / Zasloužená odplata | |
| Good Neighbors / Come to an Agreement | Dobří sousedé / Dohoda | |
| Conspiracy to Purloin / Steal Resources / retain ethical distance | Spiknutí za účelem krádeže / ukrást zdroje / zachovat si etický odstup | |
| Goods Procured / Ethics Prevail | Zboží obstaráno / Etika vítězí | |
| Plot Expansion / Occult Experimentation | Rozšíření parcel / Okultní experimentování | |

### 2.10 Generation III – Private Investors (Soukromí investoři)

| English | Czech | Notes |
|---|---|---|
| the warden (role, Guide and Gallery) | Strážce | Gallery `[Strážce] navždy`. A title, used for either variant. |
| The Butcher / The Bloodsmith (tag `Warden`) | **Řeznice** (f.) / **Krvokovář** (m.) | The Butcher variant is a woman, the Bloodsmith a man (`MeetingAtTheJunkPalace_Content1/2`). Nominative only (Q1). |
| Conducting Business (intro) / Asylum Deteriorated / Hostile Takeover | Obchod je obchod / Úpadek ústavu / Nepřátelské převzetí | |
| inmates / lunatics / rebel leader | vězni / šílenci / vůdce vzbouřenců | |
| Jail / Junk Palace / Battledome | vězení (Jail) / Palác z harampádí (Junk Palace) / bitevní kopule (Battledome) | Town locations: bracket in rules and hubs. |
| Everything You Know is Wrong | Všechno, co víte, je jinak | |
| Impressing {{0}} | {{0}} oceňuje děsivost | Hub title: the warden is the subject (2.1 of the main glossary). |
| Penalty for Defiance / Junkyard Scrap / A Palace of Scraps | Trest za vzdor / Šrot z vrakoviště / Palác ze šrotu | |
| Warrior / combatant | válečník (game term) / bojovník (prose) | |
| Agility / Strength = Damage / Life = HP | **obratnost / síla / životy** | One term per stat (Q4): "Damage" → *síla*, "HP", "Life", "Life total" → *životy*. "0 Life" → *0 životů*. |
| enter the stats / locked in | zadat hodnoty / je zapsáno | "Agility Confirmed" → *Obratnost potvrzena*. |
| Enter the Battledome / The Main Event / The Battle is Over | Vstup do bitevní kopule / Hlavní zápas / Bitva skončila | |
| penny-farthing / speaking-trumpet | vysoké kolo / hlásná trouba | |

### 2.11 Generation III – Acceptance (Přijetí)

| English | Czech | Notes |
|---|---|---|
| Asylum Program / A Focus on Rehabilitation / be committed | ústavní program / Důraz na rehabilitaci / nechat se přijmout do ústavu | Admission → *přijetí do ústavu* (Guide). |
| Asylum Admittance Log / Patient Entry Report / Discharge Report | Přijímací kniha ústavu / přijímací zpráva / propouštěcí zpráva | |
| Case Number / Case File | číslo spisu / chorobopis | |
| Psychological Assessment / Assessment Question No. / Assessment Results | Psychologické vyšetření / Otázka vyšetření č. / Výsledky vyšetření | |
| alienist evaluation | vyšetření u psychiatra | *alienista* is too obscure in Czech. |
| Special Servant Rule | Zvláštní pravidlo pro sluhy | |
| rehabilitation program / the {{0}} Program | rehabilitační program / Program {{0}} | Typed name as a label after *Program*. |
| Program focus (Engineering / Chemistry / Biology / Occult) | zaměření programu (technika / chemie / biologie / okultismus) | |
| essential salts / Hope Springs Eternal / Arise, My Love / The Risk is Too Great | esenciální soli / Naděje umírá poslední / Povstaň, má lásko / Riziko je příliš velké | Spouse here is *choť* in story, *partner* in rules (main 2.2). |
| Increased Taxes / Stricter Citizenship and Funding Cuts | Vyšší daně / Přísnější občanství a škrty ve financování | Policy bid choices. "Voted to be Nationalist" → *Hlasovali jsme nacionalisticky*. |
| Economic Stimulus / Surprising Rebuke / Bribe | Oživení hospodářství / Nečekaná výtka / ÚPLATEK | |
| Estate Taxes / Death and Taxes / Seizure of Goods or Trade | Majetkové daně / Smrt a daně / Zabavení zboží či živnosti | The inheritance tax in prose → *dědická daň*. |
| Ethnic Separatism / Nationalist movement | Národnostní separatismus / nacionalistické hnutí | |
| The Creature Intervenes | Stvůra zasahuje | |

### 2.12 Town locations (bracket rule; Village Chronicle pages of this scenario)

Rules, setup, hubs and UI: radnice (Town Hall) · kostel (Church) · banka (Bank) · knihovna (Library) · park (Park) ·
kancelář stavitele (Builder's Office) · hřbitov (Cemetery) · Dělnický svaz (Laborer's Union) · kočovná karavana
(Traveling Caravan) · kovářský učeň (Blacksmith's Apprentice) · čeledín (Farmer's Apprentice) · ústav (Asylum) ·
Dům na kopci (House on the Hill) · Pašerácký přístav (Smuggler's Dock) · vězení (Jail) · Palác z harampádí (Junk
Palace) · bitevní kopule (Battledome). Decline the Czech part, keep the bracket unchanged: *v kanceláři stavitele
(Builder's Office)*.

Story and Log Book places (no bracket): náměstí, kovárna, řeznictví, pošta, redakce, soudní budova, přístaviště,
skladiště, rozcestí, mauzoleum, parlament, městské hradby, hostinec, panství.

### 2.13 Values filled in by code from CSV tags, and typed-in values

| Tag / value | English | Czech | Notes |
|---|---|---|---|
| `Newspaper` | The {{0}} Ledger / Gazette / Mercury / Village Voice / Examiner | `{{1=\|Zpravodaj\|Věstník\|Merkur\|Hlasatel\|Pozorovatel}} {{0=...\|Town\|...}}` | All masculine singular, so every verb or adjective agrees with any paper. Town name as a label after the title (example 5). |
| `Suspect_Name` | Apprentice / Blacksmith's / Butcher's Apprentice | `{{0=\|Učeň\|Kovářský učeň\|Řeznický učeň}}` | Capitalised: the value is the character's name, shown as a button next to the player names and inserted into player-name slots. Sentences treat it as a name in the nominative, masculine (*Prstem ukazuje i {{0}}*, *Ubohý {{0}} na nás plival*, *Prokletý nebyl {{0}}*), never as a common noun (Q6). |
| `HexLocation1` / `HexLocation2` | Church / Traveling Caravan / Laborer's Union; Park / Builder's Office / Cemetery | `{{0=\|kostel (Church)\|kočovná karavana (Traveling Caravan)\|Dělnický svaz (Laborer's Union)}}` and `{{0=\|park (Park)\|kancelář stavitele (Builder's Office)\|hřbitov (Cemetery)}}` | Used in rules, hubs and newspapers alike: nominative after a colon (example 6). In the story newspapers the bracket stays, as a known exception. |
| `AsylumUpgrade` | Asylum Labor Union / Key to the Asylum / Asylum Share | `{{0=\|Odborový svaz ústavu\|Klíč od ústavu\|Podíl v ústavu}}` | No bracket (story uses it too). Labels: *program <b>{{0}}</b>*, *dílky <b>{{0}}</b>* (Q7). |
| `CreatureRequest` | Puppy / Bride / Puppy and Bride | `{{0=\|Štěně\|Nevěstu\|Štěně i Nevěstu}}` | Only used in `GrandfathersCreation`: accusative fits both rows. |
| `Warden` | The Butcher / The Bloodsmith | `{{0=?\|Řeznice\|Krvokovář}}` | Q1. Ending 6 `{{2}} navždy`. |
| `JunkPenalty` | Lose 5VP. / Discard a Vanity Estate Upgrade… | Ztraťte 5 VB. / Je-li to možné, odhoďte ze svého sídla jedno okrasné vylepšení sídla. | |
| warrior names (typed) | Iron Maiden, Steam Titan | — | Nominative only. Oblique case after *válečník*: *na válečníka {{1}}*. `WarriorComplete_Title` "{{0}} Complete" → *Válečník {{0}} je hotov*. |
| program name (typed) | Iron Will | — | *Program {{0}}*. |
| village (typed) | Town | — | *ve městě {{1}}* where the source says town, *ve vesnici {{1}}* where it says village. Ending 3 *Jaro pro vesnici {{1}}* (Gallery). `the {{0}} Jail` → *Věznice {{0}}*. |
| dates (`Flavor_Date`, `Flavor_NewsDate`) | 14 July, 1831 | `{{0=...\|14}}. {{1=\|ledna\|února\|března\|dubna\|května\|června\|července\|srpna\|září\|října\|listopadu\|prosince}} {{2=...\|1831}}` | Both tags the same Czech order. |
| `Flavor_Letter` / `Flavor_Journal` | Letter from / Noted Letters - / Letter Excerpt from / Correspondence - ; Journal Entry - / Journal of / Collected Notes - / Diary Entry - | `{{0=\|Dopis – \|Zaznamenané dopisy – \|Úryvek z dopisu – \|Korespondence – }}`, `{{0=\|Zápis v deníku – \|Deník – \|Sebrané poznámky – \|Záznam v deníku – }}` | The name follows a dash, never a preposition. Keep the trailing spaces. |

Fixed headers: "Letter from {{0}}" → *Dopis – {{0}}*. "{{0}}'s Journal" → *Deník – {{0}}*. "Scribbled Note – {{0}}'s
Handwriting – undated" → *Načmáraný vzkaz – rukopis: {{0}} – bez data*. "name illegible" → *jméno nečitelné*.
"(cont.)" → *(pokr.)*. "Newsprint" → *Novinová zpráva*. "Collected Letters – Stoker, Victoria" → *Sebrané dopisy –
Stokerová, Victoria* (main glossary Q7). Herbert Shelley and Roderick Tenderson stay unchanged.
Places: Romania → *Rumunsko*, Transylvania → *Transylvánie* (Q8), Bucharest → *Bukurešť*, Siberia → *Sibiř*.

### 2.14 Branch and ending titles (from `cs/UI_Localization.csv`, do not change)

Hon na čarodějnice · Mohou za to cizinci · Stvůra se vrací · Město za hradbami (1 Upadli v zapomnění, 2 Vzorné město) ·
Napětí roste (3 Jaro pro vesnici {{1}}, 4 Hněv davu) · Soukromí investoři (5 Mír navzdory všem, 6 {{2}} navždy) ·
Přijetí (7 V ústavu, 8 Ve vlastních rukou). `FinalEnding_Family` "The Family" → *Rodina*. "THE END?" → *KONEC?*

---

## 3. Worked examples

| # | Tag | English | Czech (proposal) | Technique |
|---|---|---|---|---|
| 1 | `Battle_Content` | {{0}} {{2=\|attacks\|swings wildly at\|…\|delivers a startling open-handed slap to the chest of\|…}} {{1}}. | {{0}} {{2=\|útočí na válečníka\|se divoce ohání po válečníkovi\|…\|uštědří otevřenou dlaní nečekaný políček do hrudi válečníka\|…}} {{1}}. | Each option ends with *válečník* in the case its verb governs, so the typed name {{1}} stays nominative. Present tense: no gender. Keep all 13 options. |
| 2 | `TheEscape_Content3` | "They call me {{0}}. … {{0}} runs this town." | „Říkají mi {{0}}. … Teď tomuhle městu vládne {{0}}.“ | *Říkají mi* + nominative is idiomatic. Subject + present tense, so neither *Řeznice* nor *Krvokovář* needs agreement. |
| 3 | `Gen1FateVictim_Content1` | My dear {{0}} II,<br /><br />With your return… | Adresát: {{0}} II.<br /><br />Mé drahé dítě,<br /><br />Vzhledem k Vašemu brzkému návratu… | No vocative of a typed name. *Dítě* is gender-free, and its vocative equals the nominative. |
| 4 | `AssessmentResults_Content2` | Patient {{0}} has been admitted for static electro-therapy… Prescribed laudanum… | [[g0:Pacient {{0}} byl přijat/Pacientka {{0}} byla přijata]] k léčbě statickou elektřinou… Předepsáno laudanum a fosfáty na nervy. | Whole phrase in the token, placeholder kept inside both forms. Then nominal clinical style without gender. |
| 5 | `Neglected_Content` | {{0}} regrets to report that another season has gone by in {{1}} without… | {{0}} s politováním oznamuje, že ve městě {{1}} uplynula další sezóna bez… | The paper (*Zpravodaj Testville*) is the subject in the present tense. The village name follows *město*. |
| 6 | `FirstHexRevealed_Content` | Place the <b>Witch's Hex</b> token on the <b>{{0}}</b>. This Location is hexed and cannot be visited… | Položte žeton <b>Kletba čarodějnice</b> (<i>Witch's Hex</i>) na toto místo: <b>{{0}}</b>. Na místo padla kletba a v tomto kole je nesmí navštívit žádný hráč. | The value from `HexLocation1` (*kostel (Church)*) stands after a colon in the nominative and brings its own bracket. |

---

## 4. Open questions

| # | Question | Recommendation |
|---|---|---|
| Q1 | The warden value is *Řeznice* (f.) or *Krvokovář* (m.), and it is inserted unchanged into texts shared by both variants. Past tense and adjectives about the warden cannot agree. | Until the code can tell (a developer task: a gender token for the warden, e.g. `[[gW:…/…]]`, f. when the Butcher was executed), write shared texts with the warden as subject in the present tense, after a colon, or with *Strážce* as a title. The variant-only rows (`MeetingAtTheJunkPalace_Content1/2`, `TheEscape_Content1/2`) may agree freely. |
| Q2 | The warden addresses the aristocrat cousin. *Ty* or *vy*? | *Ty*: it shows the reversal of the hierarchy. The Count, the Creature and every letter writer keep *vy*. |
| Q3 | The Gallery calls the Creature *Stvůra* (f.), but he is male in the source. | Narrator: *Stvůra* with feminine agreement, *netvor* (m.) for variety. The Creature in the first person: masculine. Masterwork card: *STVŮRA*. |
| Q4 | The source uses Damage/Strength and HP/Life for the same two stats. | One Czech term each: *síla* and *životy*. Players enter one number per stat, so two names would confuse them. |
| Q5 | "City Hall" appears in `Preparations_0` and `SeizureOfGoods_0`. | It is the Town Hall: *radnice (Town Hall)*, unless the Chronicle page prints "City Hall". |
| Q6 | The Apprentice (`Suspect_Name`) fills player-name slots in Accuse, DeathAndResolution and CursedRemoval. A `[[gN:…]]` token bound to such a slot has no player gender. | Check the fallback (it should give the masculine, matching *učeň*). In these rows prefer gender-free wording anyway. |
| Q7 | `AsylumInvestors_0` asks players to fetch the Asylum upgrade tiles by a code-filled name, and the value cannot carry the English (the story uses it too). | Put the English at the heads of `AsylumSupport_Content1-3` (*<b>Podíl v ústavu</b> (<i>Asylum Share</i>)*). Optionally the developer adds a second tag with the English for the popup. |
| Q8 | Transylvania: *Transylvánie* or the historical *Sedmihradsko*? | *Transylvánie*: the Dracula associations matter here, and Czech Dracula editions use it. |
| Q9 | Town location whose Czech equals the English: *park (Park)*. | Keep the bracket for consistency with the rulebook team's rule. Raise it with them if it reads oddly at the table. |

---

## 5. Terms settled during the first translation

Settled by the editing pass after the seven translators (November 2026). Binding like the rest of this file. The
answers to the open questions above are as recommended; Q1 (warden agreement) and Q6 (Apprentice gender) still wait for
a developer token.

### 5.1 Rules wording (Gameplay CSV, hubs, Guide)

| English | Czech | Notes |
|---|---|---|
| Ties are broken by the one / player with the fewest (lowest) VP. | Při shodě vyhrává hráč s nejméně VB. | Every short tie-break. Where the English says "the one with the fewest VP will gain the Upgrade" the Czech keeps that structure (*Při shodě získá vylepšení hráč s nejméně VB.*). |
| If still tied, the one who went last … | Pokud shoda trvá, vyhrává ten, kdo byl … na tahu poslední. | Not *Trvá-li shoda i poté*. |
| The player who won the bid gains … / choose the outcome of your bid: | Vítěz dražby získává … / Vítěz dražby nyní zvolí, jak se svou výhrou naloží: | All four bid result screens. |
| Bid Complete / Vote Resolution / Vote Result / The Vote Has Been Resolved | Dražba skončila / Výsledek hlasování / Výsledek hlasování / Hlasování vyhodnoceno | Same as the other two scenarios. |
| voting token | hlasovací žeton | Bracket (*voting token*) at the first mention in a popup. |
| (hub) Click here … for an explanation of the Church location if needed. | Přečtěte si zde … vysvětlení ke kostelu (Church), pokud je potřebujete. | The three Church sections (`Creature_`, `Isolation_`, `Tension_Church`) read alike. |
| (hub) For a detailed explanation (of the hunt / battle), click here. | Podrobné vysvětlení (honu / závěrečné bitvy) si … přečtěte zde. | `_Content` ends with *si*, `_ClickHere` is *přečtěte zde.* (Town Strength, Bank & Library, both Hunt sections, Battledome). |
| (hub) When you visit X for the first time, click here for a message. | Při své první návštěvě … si zde přečtěte zprávu. | |
| Player (coloured money) token | investiční žeton (Player token) | |
| Self piece (first mention in a popup) | osobní figurka (Self) | |
| Collaborative Experiments / Collaboration token | společné experimenty (Collaborative Experiments) / žeton spolupráce (Collaboration) | |
| Angry Mob track (`AssessmentResults_0_Content6`) | stupnice děsivosti | Source slip for the Creepy track (2.2). |
| letter headers with a place | *30. března 1892. Rumunsko.* | Period before the place, as in the English; never a comma. |

### 5.2 Window titles repeated elsewhere (Log Book, Guide, hubs)

| English | Czech |
|---|---|
| Preparations / A Note for my Sister / Shared Mania | Přípravy / Vzkaz pro mou sestru / Sdílená mánie |
| Local Inventor Sees Meteoric Rise / The New Science / Dedication Should Not Be Punished / A Town's Suspicions – An Interview | Místní vynálezecká hvězda strmě stoupá / Nová věda / Oddanost by neměla být trestána / Podezření města – rozhovor |
| Over-Achievement / Overambitious / The Mob Moves In | Nad plán / Přehnaná ctižádost / Dav se blíží |
| A Public Execution? / The Vote Has Been Resolved | Veřejná poprava? / Hlasování vyhodnoceno |
| The Blacksmith is Dead. / The Butcher is Dead. | Kovář je mrtev. / Řeznice je mrtva. |
| Fear of Our Family / Flames of Hysteria / Paranoia Fades / More Secrets to Quell / A Return to Civility | Strach z naší rodiny / Plameny hysterie / Paranoia slábne / Další tajemství k umlčení / Návrat k civilizovanosti |
| Strengthening the Town / The Deadly Witching Game / The Rumor Is... | Posílení města / Smrtící čarodějnická hra / Klepy praví… |
| Fortunate Discovery / Memorial to the Past for the Future / Opportunity / A Vague Promise | Šťastný objev / Památník minulosti pro budoucnost / Příležitost / Neurčitý slib |
| Accusations / Death and Resolution / Cursed Removal / The Curse Continues | Obvinění / Smrt a rozuzlení / Odstranění prokletého / Prokletí trvá |
| Construction Matters / Protected Against Witchcraft | Stavební záležitosti / Chráněni před čarodějnictvím |
| The Creature's Demands / Hideous Motivation / Grandfather's Creation Returns / The Creature is Appeased | Požadavky Stvůry / Ohavná pohnutka / Dědovo dílo se vrací / Stvůra je usmířena |
| Visit to the Sanatorium / Retrieval of Property / Perhaps Stubborn, Perhaps Illiterate | Návštěva sanatoria / Vyzvednutí majetku / Možná tvrdohlavost, možná negramotnost |
| Meeting on the Hill / A Sly Investment / An Improved Deal / Declined Offer / Thanks Are In Order / To Remove A Curse | Schůzka na kopci / Lstivá investice / Lepší nabídka / Odmítnutá nabídka / Je načase poděkovat / Sejmutí prokletí |
| The Esteemed Institution / Business Opportunity / Asylum Resolution / Total Contributions | Ctihodný ústav / Obchodní příležitost / Rozhodnutí o ústavu / Celkové příspěvky |
| A Cursed Cure / Cure Confirmed / Cure Denied | Prokletý lék / Lék potvrzen / Lék odepřen |
| The Count's Payment Received / The Count is Disappointed / The Creature's Aid / Money Talks | Platba hraběti přijata / Hrabě je zklamán / Pomoc Stvůře / Peníze hýbou světem |
| The Creature Exacts Its Revenge / Minor Compassion | Stvůra se mstí / Špetka soucitu |
| A Debt Repaid / For Private Use / A Most Unwelcome Return / Payment Received / Premature End | Dluh splacen / K soukromému užívání / Nanejvýš nevítaný návrat / Platba přijata / Předčasný konec |
| Minor Recompense / Excellence Distrusted / Verification Completed / Verification of Identity Required | Drobné odškodnění / Nedůvěra k výjimečnosti / Ověření dokončeno / Vyžadováno ověření totožnosti |
| Neglected by the Caravan / Neglected | Opomenuti karavanami / Opomíjení |
| A Controlled Release / Consequences of Smuggling / Politics as Usual / Devastating and Expected Results | Řízené vypuštění / Důsledky pašování / Politika jako obvykle / Zničující a očekávané výsledky |
| Minor Reward for Investment / Reconciliation / In Support of Others / Ways to Ease the Tension | Drobná odměna za investici / Smíření / Na podporu druhých / Jak zmírnit napětí |
| Investment Dividends / Investment / The Escape / Build Me A Warrior / The World is Not the Same | Výnosy z investic / Investice / Útěk / Postav mi válečníka / Svět už není jako dřív |
| Merciful Heritage / A Sacrifice is Required / An Abrupt End / The Horror of Existence | Milosrdné dědictví / Je třeba oběti / Náhlý konec / Hrůza bytí |
| A Plan Revealed / The Meeting at the Junk Palace / Resolution | Plán odhalen / Setkání v Paláci z harampádí / Rozuzlení |
| Blood in the Battledome / Battle Warrior / The Battledome | Krev v bitevní kopuli / Válečník pro bitevní kopuli / Bitevní kopule |
| Understanding and Support / Discharged / Aid to the Institute / Choose the Program Focus / The Choice | Pochopení a podpora / Propuštění / Pomoc ústavu / Zvolte zaměření programu / Volba |

### 5.3 Log Book

Titles for screens whose window title carries a typed name or changes with play (the English Log Book uses a fixed
title): The Inventor in the News → *Vynálezce v novinách*, An Inherited Obsession → *Zděděná posedlost*, The Library
Falls → *Pád knihovny*, Patient Entry Report → *Přijímací zpráva*, Assessment Results → *Výsledky vyšetření*, The New
Program → *Nový program*. Screens with two or more entries (Gen1FateVictim, Gen1FateMob, Obsessions, ObsessionsAltered,
TrialOutcome, MercifulHeritage) use the exact Czech window-title variant, because the app matches it.

Places (no English bracket): Guest Quarters → *Pokoj pro hosty*, Estate → *Panství*, Estate Gates → *Brána panství*,
Estate Grounds → *Pozemky panství*, Village Streets → *Vesnické ulice*, Newspaper Office → *Redakce*, Smithy → *Kovárna*,
Butcher's Shop → *Řeznictví*, The Count's Estate → *Hraběcí panství*, Second Home → *Druhé sídlo*, Countryside →
*Venkov*, Town Walls → *Městské hradby*, Warehouse → *Skladiště*, Docks → *Přístaviště*, Courthouse → *Soudní budova*,
Parliament → *Parlament*, Mausoleum → *Mauzoleum*. Summaries: Generation I "cousins" are *sourozenci* (as in the story),
later ones *bratranci*; "a cousin" → *jeden z bratranců* (generic masculine, no tokens in the Log Book); Victoria
Stoker ("caretaker") → *poručnice*.
