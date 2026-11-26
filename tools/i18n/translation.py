"""Tools for the app's translations (docs/i18n/README.md).

    python tools/i18n/translation.py merge <prefixes> [--lang cs] parts in docs/i18n/work/<lang>/ -> wwwroot/localization/<lang>/
    python tools/i18n/translation.py check  [--lang cs]           every translated file against its English file
    python tools/i18n/translation.py sheet  [--lang cs] [out.xlsx] review workbook: English | Czech | suggestion | comment
    python tools/i18n/translation.py page   [--lang cs] [out.html] side-by-side page to read in a browser
    python tools/i18n/translation.py import <review.xlsx> [--lang cs]  applies the reviewers' suggestions, then checks

A translation lives in wwwroot/localization/<lang>/ with the same file names as the English files and the header
"Tag;<Language>". Tags it lacks fall back to English in the app.
"""
import html
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
LOC = ROOT / 'MyFathersWorkApp' / 'MyFathersWorkApp' / 'wwwroot' / 'localization'
WORK = ROOT / 'docs' / 'i18n' / 'work'
LANGUAGES = {'cs': 'Czech'}

# English file -> what the reviewers see it as. The gallery names are UI rows with an English source of their own.
FILES = [
    ('UI_Localization.csv', 'App menus and help'),
    ('TheCostOfDisease_Localization.csv', 'The Cost of Disease: story'),
    ('TheCostOfDisease_Gameplay_Localization.csv', 'The Cost of Disease: rules and setup'),
    ('TheCostOfDisease_Guide.csv', 'The Cost of Disease: guided mode'),
    ('TheCostOfDisease_LogBook.csv', 'The Cost of Disease: Log Book'),
    ('FearOfTheUnknown_Localization.csv', 'Fear of the Unknown: story'),
    ('FearOfTheUnknown_Gameplay_Localization.csv', 'Fear of the Unknown: rules and setup'),
    ('FearOfTheUnknown_Guide.csv', 'Fear of the Unknown: guided mode'),
    ('FearOfTheUnknown_LogBook.csv', 'Fear of the Unknown: Log Book'),
    ('ATimeOfWar_Localization.csv', 'A Time of War: story'),
    ('ATimeOfWar_Gameplay_Localization.csv', 'A Time of War: rules and setup'),
    ('ATimeOfWar_Guide.csv', 'A Time of War: guided mode'),
    ('ATimeOfWar_LogBook.csv', 'A Time of War: Log Book'),
]


def lang_arg():
    return sys.argv[sys.argv.index('--lang') + 1] if '--lang' in sys.argv else 'cs'


def read_rows(path, logbook=False):
    """[(tag or None for a comment, text)] in file order; Log Book rows are keyed by their English title."""
    rows = []
    if not path.exists():
        return rows
    for n, line in enumerate(path.read_text(encoding='utf-8-sig').splitlines()):
        if n == 0 or not line.strip():
            continue
        if line.startswith('//'):
            rows.append((None, line.split(';')[0]))
            continue
        if logbook:
            parts = line.split(';', 3)
            if len(parts) < 4 or parts[0] == 'Screens':
                continue
            rows.append((parts[1], f'{parts[1]}|{parts[2]}|{parts[3]}'))
        else:
            tag, _, text = line.partition(';')
            rows.append((tag, text))
    return rows


def english(name):
    rows = read_rows(LOC / name, logbook=name.endswith('_LogBook.csv'))
    if name == 'UI_Localization.csv':
        rows = [row for row in rows if not (row[0] or '').startswith('ScenarioLang_')]  # unused rows
        rows += read_rows(WORK / 'gallery-labels.en.csv')
    return rows


def translated(name, lang):
    path = LOC / lang / name
    if name.endswith('_LogBook.csv'):
        rows = []
        if path.exists():
            for n, line in enumerate(path.read_text(encoding='utf-8-sig').splitlines()):
                if n == 0 or not line.strip() or line.startswith('//'):
                    continue
                parts = line.split(';', 3)
                if len(parts) == 4:
                    rows.append((parts[0], f'{parts[1]}|{parts[2]}|{parts[3]}'))
        return dict(rows)
    return {tag: text for tag, text in read_rows(path) if tag}


def merge(lang, only):
    """Rebuilds the translated files of the scenarios named in `only` (file name prefixes, required: a merge overwrites
    the translated file, so never merge a file that was edited after its parts were written)."""
    if not only:
        sys.exit('merge needs the file prefixes to rebuild, e.g. merge FearOfTheUnknown ATimeOfWar')
    out_dir = LOC / lang
    out_dir.mkdir(parents=True, exist_ok=True)
    for name, _ in FILES:
        if not any(name.startswith(prefix) for prefix in only):
            continue
        stem = name[:-4]
        parts = sorted((WORK / lang).glob(f'{stem}.part*.csv')) or sorted((WORK / lang).glob(name))
        if not parts:
            print(f'{name}: no parts yet')
            continue
        lines = [f'Tag;{LANGUAGES[lang]}' if not name.endswith('_LogBook.csv') else f'Title;{LANGUAGES[lang]} title;Location;Summary']
        for part in parts:
            lines += [l for l in part.read_text(encoding='utf-8-sig').splitlines()[1:] if l.strip()]
        (out_dir / name).write_text('﻿' + '\n'.join(lines) + '\n', encoding='utf-8')
        print(f'{name}: {len(lines) - 1} lines from {len(parts)} part(s)')


def check(lang):
    ok = True
    for name, _ in FILES:
        target = LOC / lang / name
        if not target.exists() or name.endswith('_LogBook.csv'):
            continue
        print(f'== {name}')
        r = subprocess.run([sys.executable, str(ROOT / 'tools' / 'i18n' / 'check-translation.py'), str(LOC / name), str(target), '--partial'],
                           capture_output=True, text=True, encoding='utf-8')
        lines = [l for l in r.stdout.splitlines() if not (name == 'UI_Localization.csv' and 'Gallery_' in l and 'not in the English file' in l)]
        print('\n'.join(lines[-40:]))
        en_tags = {t for t, _ in english(name) if t}
        missing = [t for t in en_tags if t not in translated(name, lang)]
        if missing:
            print(f'   {len(missing)} tag(s) not translated yet (English shows), e.g. {sorted(missing)[:5]}')
        ok = ok and r.returncode == 0
    return ok


# ---- rendering helpers (shared by the page) ----
def render(text):
    text = re.sub(r'\[\[g(\d?):([^/\]]*)/([^\]]*)\]\]', r'<span class="g" title="player gender">\2 | \3</span>', text)
    text = re.sub(r'\[\[pl:([^\]]*)\]\]', lambda m: f'<span class="ph" title="plural 1 / 2-4 / 5+">{m.group(1).replace("/", " · ")}</span>', text)
    for _ in range(4):
        text = re.sub(r'\{\{(\d)=\.\.\.\|([^|{}]*)\|[^{}]*\}\}', r'<span class="name">\2</span>', text)
        text = re.sub(r'\{\{(\d)=\?\|([^|{}]*)\|([^{}]*)\}\}', r'<span class="cond"><span class="if">\2</span><span class="else">\3</span></span>', text)
        text = re.sub(r'\{\{(\d)=\?\|([^|{}]*)\}\}', r'<span class="cond one"><span class="if">\2</span></span>', text)
        text = re.sub(r'\{\{(\d)=\|([^{}]*)\}\}', lambda m: '<span class="ph">' + ' / '.join(o for o in m.group(2).split('|') if o.strip()) + '</span>', text)
    text = re.sub(r'<link=\d+>(.*?)</link>', r'<span class="link">\1</span>', text)
    text = re.sub(r'<(icon|sprite)=([A-Za-z0-9_]+)>', lambda m: f'<span class="ph icon">{m.group(2).replace("_Icon", "").replace("_", " ")}</span>', text)
    return text


def page(lang, out):
    template = (ROOT / 'tools' / 'i18n' / 'review-template.html').read_text(encoding='utf-8')
    sections, total, done = [], 0, 0
    for name, title in FILES:
        tr = translated(name, lang)
        rows = []
        for tag, text in english(name):
            if tag is None:
                label = text.strip('/=- ').strip()
                if label:
                    rows.append(f'<tr class="sec"><td colspan="3">{html.escape(label)}</td></tr>')
                continue
            total += 1
            cz = tr.get(tag)
            done += cz is not None
            en_html = render(text.replace('|', ' — ')) if name.endswith('_LogBook.csv') else render(text)
            cz_html = (render(cz.replace('|', ' — ')) if name.endswith('_LogBook.csv') else render(cz)) if cz is not None else '<span class="todo">not translated yet</span>'
            rows.append(f'<tr id="{html.escape(name[:-4] + "-" + tag)}"><td class="tag">{html.escape(tag)}</td><td class="en">{en_html}</td><td class="cs" lang="cs">{cz_html}</td></tr>')
        sections.append(f'<section><h2>{html.escape(title)}</h2><div class="scroll"><table><colgroup><col class="c-tag"><col><col></colgroup>'
                        f'<thead><tr><th>Tag</th><th>English</th><th>Čeština</th></tr></thead><tbody>{"".join(rows)}</tbody></table></div></section>')
    page_html = template.replace('<!--SECTIONS-->', ''.join(sections)).replace('<!--COUNT-->', f'{done} of {total} rows translated')
    Path(out).write_text(page_html, encoding='utf-8')
    print(out, f'{done}/{total}')


def sheet(lang, out):
    from openpyxl import Workbook
    from openpyxl.styles import Alignment, Font, PatternFill
    wb = Workbook()
    wb.remove(wb.active)
    for name, title in FILES:
        tr = translated(name, lang)
        ws = wb.create_sheet(title.replace(':', ' -')[:31])
        ws.append(['File', 'Tag', 'English', LANGUAGES[lang], 'Suggested ' + LANGUAGES[lang], 'Comment'])
        for cell in ws[1]:
            cell.font = Font(bold=True)
            cell.fill = PatternFill('solid', fgColor='E8E4DA')
        for tag, text in english(name):
            if tag is None:
                ws.append(['', '', text.strip('/=- ').strip()])
                ws.cell(ws.max_row, 3).font = Font(bold=True, italic=True)
                continue
            ws.append([name, tag, text, tr.get(tag, ''), '', ''])
        for col, width in zip('ABCDEF', (14, 28, 70, 70, 50, 30)):
            ws.column_dimensions[col].width = width
        for row in ws.iter_rows(min_row=2):
            for cell in row:
                cell.alignment = Alignment(wrap_text=True, vertical='top')
        ws.column_dimensions['A'].hidden = True
        ws.freeze_panes = 'C2'
    readme = wb.create_sheet('How to review', 0)
    for line in [
        'Czech translation of My Father\'s Work (Unofficial): review sheet',
        '',
        'Write an improved translation in the column "Suggested Czech" and leave the "Czech" column as it is. Use "Comment" for questions.',
        'Keep everything in < > and {{ }} exactly (HTML, icons, links and the game\'s placeholders). Translate the words inside {{ }} options only.',
        '{{0=...|Oliver|James|Olivia|...}} is a player name typed in at the table, {{0=...|Town|...}} the village: Czech cannot decline them, so keep them in the nominative.',
        '[[g0:přišel/přišla]] picks the masculine or feminine form by the player\'s gender. [[pl:dům/domy/domů]] picks the plural form (1 / 2-4 / 5+) by the number before it.',
        'Never use a semicolon ( ; ) in the text.',
        'The glossary and style guide: docs/i18n/cs-glossary.md in the repository.',
    ]:
        readme.append([line])
    readme.column_dimensions['A'].width = 140
    readme['A1'].font = Font(bold=True, size=14)
    wb.save(out)
    print(out)


def import_sheet(path, lang):
    from openpyxl import load_workbook
    wb = load_workbook(path)
    changes = {}
    for ws in wb.worksheets[1:]:
        for row in ws.iter_rows(min_row=2, values_only=True):
            if len(row) < 5 or not row[0] or not row[4]:
                continue
            changes.setdefault(row[0], {})[row[1]] = str(row[4]).replace('\n', '<br />').strip()
    for name, edits in changes.items():
        count = len(edits)
        target = LOC / lang / name
        lines = target.read_text(encoding='utf-8-sig').splitlines()
        logbook = name.endswith('_LogBook.csv')
        for i, line in enumerate(lines):
            key = line.split(';', 1)[0]
            if key in edits:
                text = edits.pop(key)
                # Log Book suggestions are "title|location|summary", like the sheet shows them
                lines[i] = f'{key};' + (';'.join(text.split('|', 2)) if logbook else text)
        for tag, text in edits.items():
            if not logbook:
                lines.append(f'{tag};{text}')
        target.write_text('﻿' + '\n'.join(lines) + '\n', encoding='utf-8')
        print(f'{name}: {count} suggestion(s) applied')
    check(lang)


if __name__ == '__main__':
    command = sys.argv[1] if len(sys.argv) > 1 else 'check'
    lang = lang_arg()
    rest = [a for a in sys.argv[2:] if not a.startswith('--') and a != lang]
    if command == 'merge':
        merge(lang, rest)
    elif command == 'check':
        sys.exit(0 if check(lang) else 1)
    elif command == 'sheet':
        sheet(lang, rest[0] if rest else str(ROOT / 'docs' / 'i18n' / f'{lang}-review.xlsx'))
    elif command == 'page':
        page(lang, rest[0] if rest else str(ROOT / 'docs' / 'i18n' / f'{lang}-review.html'))
    elif command == 'import':
        import_sheet(rest[0], lang)
