"""Static check: derives every scenario localization tag the C# code requests (CallerMemberName conventions) and reports tags
missing from the scenario CSVs. Computed content indices are checked for every value when they are a ternary of literals
or listed in DYNAMIC_VALUES; the rest is listed for manual checking. Hub section helpers that pass their own tag prefix
(const string caller = "...") are checked like hubs.
Usage: python tools/tagcheck.py [--scenario=FearOfTheUnknown] [file-filter ...] [--unused]
The scenario defaults to TheCostOfDisease. Shared/Scenarios/FinalScoring.cs is checked against every scenario's CSVs."""
import re, glob, sys

import os
ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'MyFathersWorkApp', 'MyFathersWorkApp') + '/'
SCENARIO = next((a.split('=', 1)[1] for a in sys.argv[1:] if a.startswith('--scenario=')), 'TheCostOfDisease')
files = sorted(glob.glob(ROOT + 'Shared/Scenarios/' + SCENARIO + '/*.cs')) + [ROOT + 'Shared/Scenarios/FinalScoring.cs']
args=[a for a in sys.argv[1:] if not a.startswith("--")]
if args: files = [f for f in files if any(a in f for a in args)]

tags = set()
for f in [SCENARIO + '_Localization.csv', SCENARIO + '_Gameplay_Localization.csv']:
    for l in open(ROOT + 'wwwroot/localization/' + f, encoding='utf-8-sig').read().splitlines():
        if l and not l.startswith('//'): tags.add(l.split(';')[0])

allsrc = ''.join(open(f, encoding='utf-8-sig').read() for f in glob.glob(ROOT + 'Shared/Scenarios/' + SCENARIO + '/*.cs'))
consts = dict(re.findall(r'const string (\w+)\s*=\s*"([^"]*)"', allsrc))

need = {}   # tag -> where
dynamic = []
used_tags = set()

# Values a computed content index can take, per scenario: (method, expression) -> values. Checked like literal indices;
# keep in sync with the code (an expression that is not listed here or a simple "cond ? 1 : 2" ternary is printed as
# DYNAMIC for a manual check).
DYNAMIC_VALUES = {
    'ATimeOfWar': {
        ('RumorD1Revealed', 'War(globalData)'): [1, 2],                # _RND_WAR: 1 Time Travel, 2 Martial
        ('RumorD5Revealed', 'War(globalData)'): [1, 2],
        ('SeedResolution', 'index'): [1, 2, 3],                        # SeedResolutionIndex
        ('BattleTime', 'paragraph + 1'): [1, 2, 3],                    # Flavor(key, 3) + 1
        ('ParadoxTimeRandom', 'mistake'): list(range(1, 9)),           # $timemistake 1 - 8
        ('FinalEnding', 'ending'): list(range(1, 9)),                  # Vars.Ending 1 - 8
        ('Warning', 'world'): list(range(17, 23)),                     # _WORLDS (Village Chronicle pages 17 - 22)
    },
}.get(SCENARIO, {})

def add(tag, where):
    need.setdefault(tag, where)

def call_args(body, start):
    """Top-level comma separated arguments of the call whose '(' is at body[start - 1]."""
    args, depth, cur = [], 0, ''
    for ch in body[start:]:
        if ch in '([{': depth += 1
        elif ch in ')]}':
            if depth == 0: break
            depth -= 1
        if ch == ',' and depth == 0: args.append(cur.strip()); cur = ''
        else: cur += ch
    args.append(cur.strip())
    return args

def index_values(name, expr):
    """Values of a content index expression: a literal, a listed DYNAMIC_VALUES entry or a (nested) ternary of literals."""
    if expr.isdigit(): return [int(expr)]
    if (name, expr) in DYNAMIC_VALUES: return DYNAMIC_VALUES[(name, expr)]
    values, rest = [], expr
    while True:
        m = re.fullmatch(r'\(?[^?:]+\?\s*(\d+)\s*:\s*(.+?)\)?', rest.strip())
        if not m: return None
        values.append(int(m.group(1)))
        rest = m.group(2)
        if rest.strip().isdigit(): return values + [int(rest)]

for f in files:
    src = open(f, encoding='utf-8-sig').read()
    # split into methods
    methods = [(m.group(1), m.group(2), m.group(3)) for m in re.finditer(r'\n    (?:public|private) static (?:void|string|bool|int|Faction|string\[\]|Years|Action<GlobalData>) (\w+)\(([^)]*)\)\s*(=>[^\n]*\n|\n    \{\n(.*?)\n    \})', src, re.S)]
    # A shared body with [CallerMemberName] string caller (e.g. Gen1Fate) uses the tags of the screens that call it
    expanded = []
    for name, params, body in methods:
        if 'CallerMemberName' in params:
            callers = [n for n, _, b in methods if n != name and re.search(r'\b' + name + r'\(globalData', b or '')]
            expanded += [(caller, body) for caller in callers]
        else:
            expanded.append((name, body))
    for name, body in expanded:
        where = f.split('/')[-1] + ':' + name
        # Hub methods, and helpers that add sections to the shown hub under another tag prefix (const string caller)
        caller = re.search(r'const string\s+caller\s*=\s*"(\w+)"', body or '')
        if 'new GameplayHub' in body or (caller and 'AddSection(' in body):
            prefix = caller.group(1) if caller else name
            add(name + '_Title', where) if 'SetDefaultTitle' in body else None
            secs = dict(re.findall(r'const string\s+(\w+)\s*=\s*"([^"]*)"', body))
            sec = lambda token: token.strip('"') if token.startswith('"') else secs.get(token)
            for var in re.findall(r'AddSection\((\w+)', body):
                if var in secs and secs[var]: add(f'{prefix}_{secs[var]}_Title', where)
            for var in re.findall(r'\.AddDefaultContent\((\w+|"\w+")', body):
                add(f'{prefix}_{sec(var)}_Content', where)
            for call in re.finditer(r'\.AddNextContent\(', body):
                args = call_args(body, call.end())
                values = index_values(name, args[0])
                if values is None: dynamic.append((where, args[0])); continue
                for n in values: add(f'{prefix}_{sec(args[1])}_Content{n}', where)
            for var in re.findall(r'\.AddSpecialClickHere\((\w+|"\w+")', body):
                add(f'{prefix}_{sec(var)}_ClickHere', where)
            continue
        if re.search(r'\.AddDefaultTitle\(', body): add(name + '_Title', where)
        if re.search(r'\.AddDefaultBaseTitle\(', body): add(name.split('_')[0] + '_Title', where)
        if re.search(r'\.AddDefaultSubtitle\(', body): add(name + '_SubTitle', where)
        if re.search(r'ActiveWindow\.AddDefaultContent\(', body): add(name + '_Content', where)
        for call in re.finditer(r'ActiveWindow\.AddNextContent(?:WithLinks)?\(', body):
            arg = call_args(body, call.end())[0]
            values = index_values(name, arg)
            if values is None: dynamic.append((where, arg))
            else: [add(f'{name}_Content{n}', where) for n in values]
        for call in re.findall(r'new GameplayPopup\((.*?)\);', body, re.S):
            lit = re.findall(r'"([^"]*)"', call)
            if 'string.Empty' in call: continue
            if lit: [add(x, where) for x in lit]
            elif re.search(r'GlobalTags\.', call.split('PopUpButton')[1] if 'PopUpButton' in call else ''): continue
            else: add(name + '_Content', where)
        for call in re.findall(r'new GameplayInputPopup\(globalData,\s*"([^"]*)"(.*?)\);', body, re.S):
            add(name + '_Content', where)
            if re.search(r',\s*true\s*(,|$)', call[1].strip()) or call[1].rstrip().endswith('true'): add(call[0], where)
        for x in re.findall(r'GetScenarioLocalizedTag\("([^"]+)"\)', body): add(x, where)
        for x in re.findall(r'GetScenarioLocalizedTag\([^"()]+\?\s*"(\w+)"\s*:\s*"(\w+)"\)', body): [add(t, where) for t in x]
        for x in re.findall(r'GetScenarioLocalizedTag\((_[A-Z0-9_]+)\)', body): add(consts.get(x, x), where)
        for x, suffix in re.findall(r'GetScenarioLocalizedTag\((_[A-Z0-9_]+) \+ "(\w+)"\)', body): add(consts.get(x, x) + suffix, where)

missing = sorted(t for t in need if t not in tags)
print('REQUIRED', len(need), 'MISSING', len(missing))
for t in missing: print('  MISSING', t, '<-', need[t])
print('DYNAMIC (check manually):')
for w, a in dynamic: print('  ', w, a)
# unused tags heuristic
if '--unused' in sys.argv:
    prefixes = set()
    unused = sorted(t for t in tags if t not in need)
    print('NOT DIRECTLY REFERENCED', len(unused))
    for t in unused: print('  ', t)
