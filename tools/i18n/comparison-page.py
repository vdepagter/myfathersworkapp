"""Builds docs/i18n/comparison/index.html: English and two Czech translations side by side, unlabelled (A / B) until the
reader reveals which model wrote which.   python tools/i18n/comparison-page.py"""
import html, json, re
from pathlib import Path

DIR = Path(__file__).resolve().parents[2] / 'docs' / 'i18n' / 'comparison'
VERSIONS = {'A': 'fable.csv', 'B': 'opus.csv'}  # blind order; the page reveals it on request
MODELS = {'fable.csv': 'Fable 5.2 (high effort)', 'opus.csv': 'Opus 5.5 (high effort)'}

LABELS = {
    'IntroImmortalitySetup_Content': ('A Foreword', 'Story · the chronicler opens the book'),
    'Gen1CreepyTrackRes_Title': ('A Seedy Arrangement', 'Window title'),
    'Gen1CreepyTrackRes_1_Content': ('A Seedy Arrangement', 'Story · dialogue, a player name, gender'),
    'Gen1CreepyTrackRes_1_Content1': ('A Seedy Arrangement', 'Choice links'),
    'S5Special1A_Content': ('A Bid for Mayor', 'Rules · special event'),
    'FinalEnding_Content4': ('The Crawl (ending)', 'Story · first-person journal'),
    'FinalEnding_Content8': ('An Uplifting Tale (ending)', "Story · the editor's letter, many conditions"),
}


def read(name):
    rows = {}
    for line in (DIR / name).read_text(encoding='utf-8-sig').splitlines():
        if ';' in line and not line.startswith('Tag;'):
            tag, text = line.split(';', 1)
            rows[tag] = text
    return rows


def chip(cls, text, title=''):
    t = f' title="{html.escape(title)}"' if title else ''
    return f'<span class="ph {cls}"{t}>{text}</span>'


def render(text):
    # gender tokens: both forms, the page shows one by the switch
    text = re.sub(r'\[\[g(\d?):([^/\]]*)/([^\]]*)\]\]',
                  lambda m: f'<span class="g"><span class="gm">{m.group(2)}</span><span class="gf">{m.group(3)}</span></span>', text)
    text = re.sub(r'\[\[pl:([^\]]*)\]\]', lambda m: chip('pl', m.group(1).replace('/', ' · '), 'plural: 1 / 2–4 / 5+'), text)
    for _ in range(4):  # innermost placeholders first
        text = re.sub(r'\{\{(\d)=\.\.\.\|([^|{}]*)\|[^{}]*\}\}', lambda m: chip('name', m.group(2), 'typed in by the players'), text)
        text = re.sub(r'\{\{(\d)=\?\|([^|{}]*)\|([^{}]*)\}\}',
                      lambda m: f'<span class="cond"><span class="if">{m.group(2)}</span><span class="else">{m.group(3)}</span></span>', text)
        text = re.sub(r'\{\{(\d)=\?\|([^|{}]*)\}\}', lambda m: f'<span class="cond one"><span class="if">{m.group(2)}</span></span>', text)
        text = re.sub(r'\{\{(\d)=\|([^{}]*)\}\}',
                      lambda m: chip('opt', ' / '.join(o for o in m.group(2).split('|') if o.strip()), 'one of these, chosen by the game'), text)
    text = re.sub(r'<link=\d+>(.*?)</link>', r'<span class="link">\1</span>', text)
    text = re.sub(r'<icon=([A-Za-z0-9_]+)>', lambda m: chip('icon', m.group(1).replace('_Icon', '').replace('_', ' '), 'game icon'), text)
    return text


src, a, b = read('source.csv'), read(VERSIONS['A']), read(VERSIONS['B'])
passages = [{'tag': tag, 'title': LABELS.get(tag, (tag, ''))[0], 'kind': LABELS.get(tag, ('', ''))[1],
             'en': render(src[tag]), 'a': render(a.get(tag, '')), 'b': render(b.get(tag, ''))} for tag in src]

page = (Path(__file__).parent / 'comparison-template.html').read_text(encoding='utf-8')
page = page.replace('/*PASSAGES*/[]', json.dumps(passages, ensure_ascii=False))
page = page.replace('/*MODELS*/{}', json.dumps({k: MODELS[v] for k, v in VERSIONS.items()}))
(DIR / 'index.html').write_text(page, encoding='utf-8')
print(DIR / 'index.html')
