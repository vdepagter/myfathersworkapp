"""Checks a translated CSV against its English source.

    python tools/i18n/check-translation.py <english.csv> <translated.csv> [--partial]

Both are "Tag;Text" files (the header line and // comments are skipped). Reports, per tag:
  - tags missing from the translation (unless --partial) and tags the English does not have,
  - markup that differs: <b> <i> <br /> <icon=..> <sprite=..> <link=n> <p> <li> <h3> <a ..> counts,
  - placeholders that differ: the same ids, kinds ({{n=...}} replacement, {{n=?|..}} condition, {{n=|..}} options) and,
    for options, the same number of options,
  - a semicolon or line break inside the text, unbalanced [[...]] tokens, [[gN:..]] without {{N=..}} in the text,
    [[pl:..]] without three forms,
  - leading / trailing spaces that differ (they matter where a link follows inline).
Exit code 1 when there are errors.
"""
import re
import sys
from collections import Counter


def read(path):
    rows, order = {}, []
    with open(path, encoding='utf-8-sig') as f:
        for n, line in enumerate(f):
            line = line.rstrip('\r\n')
            if n == 0 or not line or line.startswith('//'):
                continue
            if ';' not in line:
                rows.setdefault('__bad__', []).append(line[:60])
                continue
            tag, text = line.split(';', 1)
            if tag in rows:
                rows.setdefault('__dup__', []).append(tag)
            rows[tag] = text
            order.append(tag)
    return rows, order


MARKUP = re.compile(r'<(/?)(b|i|br|p|li|ul|ol|h\d|a|span|strong|em|icon=[A-Za-z0-9_]+|sprite=[A-Za-z0-9_]+|link=\d+|/link)\b', re.I)


def markup(text):
    return Counter(m.group(0).lower() for m in MARKUP.finditer(text))


def placeholders(text):
    """id -> kind and option count, innermost first."""
    found = {}
    work = text
    for _ in range(6):
        m_all = list(re.finditer(r'\{\{(\d+)=([^{}]*)\}\}', work))
        if not m_all:
            break
        for m in m_all:
            pid, body = m.group(1), m.group(2)
            if body.startswith('?|') or body == '?':
                kind = ('cond', len(body.split('|')) - 1)
            elif body.startswith('|'):
                kind = ('opts', len(body.split('|')) - 1)
            else:
                kind = ('repl', 0)
            found.setdefault(pid, Counter())[kind] += 1
        work = re.sub(r'\{\{(\d+)=([^{}]*)\}\}', 'X', work)
    return {k: dict(v) for k, v in found.items()}


def main():
    args = [a for a in sys.argv[1:] if not a.startswith('--')]
    partial = '--partial' in sys.argv
    en, _ = read(args[0])
    tr, order = read(args[1])
    errors = []
    for key in ('__bad__', '__dup__'):
        if key in tr:
            errors.append(f'{key[2:-2]} lines: {tr.pop(key)}')
    en.pop('__bad__', None)
    en.pop('__dup__', None)

    if not partial:
        for tag in en:
            if tag not in tr:
                errors.append(f'{tag}: missing')
    for tag in order:
        if tag not in tr:
            continue
        t = tr[tag]
        if tag not in en:
            errors.append(f'{tag}: not in the English file')
            continue
        e = en[tag]
        if markup(e) != markup(t):
            errors.append(f'{tag}: markup differs {dict(markup(e) - markup(t))} missing / {dict(markup(t) - markup(e))} extra')
        if placeholders(e) != placeholders(t):
            errors.append(f'{tag}: placeholders differ EN {placeholders(e)} CS {placeholders(t)}')
        if ';' in t:
            errors.append(f'{tag}: semicolon in the text')
        if t.count('[[') != t.count(']]'):
            errors.append(f'{tag}: unbalanced [[ ]]')
        for m in re.finditer(r'\[\[g(\d):', t):
            if '{{' + m.group(1) + '=' not in t:
                errors.append(f'{tag}: [[g{m.group(1)}:..]] but no {{{{{m.group(1)}=..}}}} in this text (use [[g:..]])')
        for m in re.finditer(r'\[\[g\d?:([^\]]*)\]\]', t):
            if m.group(1).count('/') != 1:
                errors.append(f'{tag}: gender token needs exactly two forms: [[g..:{m.group(1)}]]')
        for m in re.finditer(r'\[\[pl:([^\]]*)\]\]', t):
            if m.group(1).count('/') != 2:
                errors.append(f'{tag}: plural token needs three forms: [[pl:{m.group(1)}]]')
        if (e[:1] == ' ') != (t[:1] == ' ') or (e[-1:] == ' ') != (t[-1:] == ' '):
            errors.append(f'{tag}: leading/trailing space differs from the English')
        stripped = t
        while re.search(r'\{\{[^{}]*\}\}', stripped):
            stripped = re.sub(r'\{\{[^{}]*\}\}', '', stripped)
        if '|' in stripped:
            errors.append(f'{tag}: "|" outside a placeholder')

    for line in errors:
        print(line)
    print(f'{len(order)} rows checked, {len(errors)} problem(s)')
    sys.exit(1 if errors else 0)


if __name__ == '__main__':
    main()
