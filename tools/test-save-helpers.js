// Browser-console helpers for play-testing the web app without playing from the start.
// Paste into the console on http://localhost:<port>/ (or store in localStorage.helpers and eval after reloads).
//
//   mkSave(hubId, { n: 3, years: 2, gen: 1, vars: { ... } });  location.href = "/Gameplay";
//
// mkSave writes a crafted save into localStorage "continue"; /Gameplay loads it when no game is active.
// hubId = CostOfDiseaseHubId (1 Fever, 2 Devastation, 3 Hospital, 4 GloomyGothic, 5 Prosperity, 6 NoUniversity, 7 University).
// Years: 0 Early, 1 Middle, 2 Late. Generation: 0/1/2. Enum values in vars are numeric (see Shared/Enums).
// Every per-player dictionary in TheCostOfDiseaseVars must be present, otherwise lookups throw (Reset runs before
// the save is applied, with the *old* player names). RandomArray may be omitted - Reset rolls a fresh one.
// opts.mirror = true starts in mirror mode with every random draw unanswered.
//
//   await run(["end of the Generation", "Confirm", "Click here to continue", "type:42", "Confirm"])
//
// run() clicks the last link/button whose text contains each string ("type:X" fills the input popup) and
// returns the text shown after every step.

window.mkSave = (hub, opts = {}) => {
  const n = opts.n || 3; const ps = ["Anna", "Bob", "Cleo", "Dan"].slice(0, n);
  const d = v => Object.fromEntries(ps.map(p => [p, JSON.parse(JSON.stringify(v))]));
  const v = Object.assign({
    Wolves: 0, Hunters: 1, Tracker: 7, Mayor: "Bob", Building: 1, Charity: "Cleo", FeverCure: "Anna",
    Hosp: d(true), Life: d(false), LifeCount: 0, Ally: d(0), BuildingPlay: d(0), HelpedExposeBuilding: d([false, false, false]),
    Gen2Buildings: [1, 3, 2], BuildingsExposeValue: [1, 0, 2], Letter: ["", "", "", "", "", ""],
    MwDiscipline: d(0), MwType: d(0), MwCost: d(0), MwName: d(""), MwAdjective: d(0), Scores: d(0), HubId: hub,
    ...(opts.mirror ? { RandomArray: Array(301).fill(-1) } : {})
  }, opts.vars || {});
  const g = {
    Language: "English", ScenarioLanguage: "English_Source", ScenarioId: 1, PlayersNum: n, MirrorMode: !!opts.mirror,
    PlayerAName: "Anna", PlayerBName: "Bob", PlayerCName: n > 2 ? "Cleo" : "", PlayerDName: n > 3 ? "Dan" : "",
    TownName: "Testville", Years: opts.years ?? 2, Generation: opts.gen ?? 1, TmpValues: {}, TheCostOfDiseaseVars: v
  };
  localStorage.setItem("continue", btoa(JSON.stringify(g)));
  return "ok";
};

// Fear of the Unknown: hubId = FearOfTheUnknownHubId (1 WitchHunt, 2 Foreigners, 3 Creature, 4 Isolation, 5 Tension,
// 6 Privatized, 7 Liberal). Path is the branch of the current Generation (set it to the hub id for Gen II/III).
window.mkSaveFotU = (hub, opts = {}) => {
  const n = opts.n || 3; const ps = ["Anna", "Bob", "Cleo", "Dan"].slice(0, n);
  const d = v => Object.fromEntries(ps.map(p => [p, JSON.parse(JSON.stringify(v))]));
  const v = Object.assign({
    HubId: hub, Path: hub, Tracker: 8, Milestones: [false, false, false, false],
    Fate: d(1), FamilyPlot: d(false), Hex1: { ...d(0), "#Suspect": 0 }, Hex2: { ...d(0), "#Suspect": 0 },
    AsylumVisits: d(0), Debt: d(0), Crossroad: d(false), CrossroadVisited: d(false), Verified: d(false), CreepReward: d(false),
    Agility: d(0), Strength: d(0), Life: d(0), Hp: d(0), WarriorName: d(""), Scores: d(0),
    AsylumQuestions: [-1, -1, -1, -1],
    ...(opts.mirror ? { RandomArray: Array(900).fill(-1) } : {})
  }, opts.vars || {});
  const g = {
    Language: "English", ScenarioLanguage: "English_Source", ScenarioId: 2, PlayersNum: n, MirrorMode: !!opts.mirror,
    PlayerAName: "Anna", PlayerBName: "Bob", PlayerCName: n > 2 ? "Cleo" : "", PlayerDName: n > 3 ? "Dan" : "",
    TownName: "Testville", Years: opts.years ?? 2, Generation: opts.gen ?? 2, TmpValues: {}, FearOfTheUnknownVars: v
  };
  localStorage.setItem("continue", btoa(JSON.stringify(g)));
  return "ok";
};

// A Time of War: hubId = ATimeOfWarHubId (1 TakingSides, 2 Martial, 3 TimeTravel, 4 MonarchReign, 5 Peace, 6 Warning,
// 7 Paradox). Enums: WarWinner 1 Separatists / 2 Monarchists, War 0 undrawn / 1 Time Travel / 2 Martial,
// ExtendedBool (Barracks, Quarter, ...) 0 None / 1 False / 2 True. Defaults: Gen I (gen 0), Late Years, Barracks built.
window.mkSaveATOW = (hub, opts = {}) => {
  const n = opts.n || 3; const ps = ["Anna", "Bob", "Cleo", "Dan"].slice(0, n);
  const d = v => Object.fromEntries(ps.map(p => [p, JSON.parse(JSON.stringify(v))]));
  const v = Object.assign({
    HubId: hub, Tracker: 7, WarWinner: 1, War: 0, Quarter: 0, Barracks: 2, Showcase: 0,
    DecreeUsed: Array(9).fill(false),
    BlameVotes: d(0), Silent: d(false), DecreesGood: d(0), DecreesEvil: d(0), Titles: d(""), Released: d(false), Scores: d(0),
    ...(opts.mirror ? { RandomArray: Array(300).fill(-1) } : {})
  }, opts.vars || {});
  const g = {
    Language: "English", ScenarioLanguage: "English_Source", ScenarioId: 3, PlayersNum: n, MirrorMode: !!opts.mirror,
    PlayerAName: "Anna", PlayerBName: "Bob", PlayerCName: n > 2 ? "Cleo" : "", PlayerDName: n > 3 ? "Dan" : "",
    TownName: opts.town || "Testville", Years: opts.years ?? 2, Generation: opts.gen ?? 0, TmpValues: {}, ATimeOfWarVars: v
  };
  localStorage.setItem("continue", btoa(JSON.stringify(g)));
  return "ok";
};

window.clickText = t => {
  // Case-insensitive: buttons show their label in capitals. "Click here to continue" is the Continue button on story windows.
  const find = text => [...document.querySelectorAll("a,button")].filter(e => e.innerText.toLowerCase().includes(text.toLowerCase()));
  let els = find(t);
  if (!els.length && /^click here to continue/i.test(t)) els = find("continue").filter(e => e.classList.contains("continue-button"));
  if (!els.length) return "NOTFOUND: " + t;
  els[els.length - 1].click();
  return "clicked";
};

window.view = () => {
  const c = document.querySelector(".popup-box") || document.querySelector(".gameplay-container");
  return c ? c.innerText : document.body.innerText;
};

window.step = async t => {
  const r = clickText(t);
  await new Promise(res => setTimeout(res, 300));
  return (r === "clicked" ? "" : r + "\n") + view();
};

window.run = async (steps, len = 250) => {
  const out = [];
  for (const t of steps) {
    if (t.startsWith("type:")) {
      const i = document.querySelector(".input-field");
      i.value = t.slice(5);
      i.dispatchEvent(new Event("input", { bubbles: true }));
      await new Promise(res => setTimeout(res, 150));
      continue;
    }
    out.push("> " + t + "\n" + (await step(t)).slice(0, len));
  }
  return out.join("\n=====\n");
};

// links() lists the links/buttons of the popup (or the page when no popup is open).
// auto(picks, max) clicks through: a single link is clicked, several links take the next entry of picks
// (substring), "type:X" fills the next input popup. Stops at a choice without a pick or a Blazor error.
// autoFirst(max) always clicks the first link and stops at a hub that only offers "next round"/"end of the Generation".
window.links = () => {
  const root = document.querySelector(".popup-box") ? ".popup-box " : ".gameplay-container ";
  return [...document.querySelectorAll(root + "a, " + root + "button")].map(e => e.innerText.trim()).filter(x => x && !["⬅", "⏸"].includes(x));
};

window.blazorError = () => { const e = document.querySelector("#blazor-error-ui"); return !!e && getComputedStyle(e).display !== "none"; };

window.auto = async (picks = [], max = 30, len = 300) => {
  const out = [];
  for (let i = 0; i < max; i++) {
    const L = links();
    if (!L.length) { out.push("NOLINKS"); break; }
    const input = document.querySelector(".input-field");
    if (input) {
      input.value = picks.length && picks[0].startsWith("type:") ? picks.shift().slice(5) : "5";
      input.dispatchEvent(new Event("input", { bubbles: true }));
      await new Promise(res => setTimeout(res, 150));
    }
    let want = null;
    if (L.length === 1) want = L[0];
    else if (picks.length && !picks[0].startsWith("type:")) {
      const p = picks.shift(); want = L.find(x => x.includes(p));
      if (!want) { out.push("PICK NOT FOUND " + p + " in " + L.join(" | ")); break; }
    } else { out.push("CHOICE: " + L.join(" | ")); break; }
    out.push("> " + want + "\n" + (await step(want)).slice(0, len));
    if (blazorError()) { out.push("BLAZOR ERROR"); break; }
  }
  return out.join("\n=====\n");
};

window.autoFirst = async (max = 30, len = 400) => {
  const out = [];
  for (let i = 0; i < max; i++) {
    const L = links();
    if (!L.length) { out.push("NOLINKS"); break; }
    if (!document.querySelector(".popup-box") && L.some(x => /next round|end of the Generation/i.test(x)) && L.length <= 2) { out.push("HUB: " + L.join(" | ")); break; }
    out.push("> " + L[0] + "\n" + (await step(L[0])).slice(0, len));
    if (blazorError()) { out.push("BLAZOR ERROR"); break; }
  }
  return out.join("\n=====\n");
};
