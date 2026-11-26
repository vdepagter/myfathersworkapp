"""Random walk through a whole game of The Cost of Disease in a translation (muted, headless), looking for text the
translation broke: placeholders or grammar tokens left in the text, "Missing ..." lines, English left on a Czech page,
console errors and Blazor errors.

    python tools/i18n/playtest-walk.py [--url http://localhost:5199/] [--lang Czech] [--seeds 1,2,3] [--steps 500] [--out DIR]

Needs the app running (dotnet run ... --urls http://localhost:5199). Writes <out>/walk-<seed>.txt (every screen: its
title and the issues found) and a screenshot of each screen with an issue.
"""
import argparse
import random
import re
import sys
import time
from pathlib import Path

from playwright.sync_api import sync_playwright

sys.stdout.reconfigure(encoding='utf-8')
ENGLISH = re.compile(r'\b(the|and|of|with|their|your|player|players|gain|lose)\b', re.I)
BROKEN = re.compile(r'\{\{|\}\}|\[\[|\]\]|Missing|\u001F|<link=|<icon=')


def args():
    p = argparse.ArgumentParser()
    p.add_argument('--url', default='http://localhost:5199/')
    p.add_argument('--lang', default='Czech')
    p.add_argument('--seeds', default='1,2,3')
    p.add_argument('--steps', type=int, default=500)
    p.add_argument('--players', type=int, default=3)
    p.add_argument('--out', default='.')
    return p.parse_args()


def setup(pg, a, rnd):
    pg.goto(a.url)
    pg.wait_for_selector('.mfw-button, button', timeout=60000)
    time.sleep(1.5)
    pg.locator('button').filter(has_text=re.compile('Nová hra|New Game')).first.click(); time.sleep(0.8)
    pg.locator('button').filter(has_text=str(a.players)).first.click(); time.sleep(0.8)
    pg.locator('button.mp-button').last.click(); time.sleep(0.8)
    names = ['Anna', 'Bohumil', 'Cecílie', 'David']
    for i in range(a.players):
        pg.fill('input.mp-input', names[i])
        if pg.locator('.gender-choice').count():
            pg.locator('.gender-choice button').nth(1 if names[i] in ('Anna', 'Cecílie') else 0).click()
        pg.locator('button.mp-button').last.click(); time.sleep(0.6)
        pg.locator('button.mp-button').last.click(); time.sleep(0.6)
    pg.fill('input.mp-input', 'Lhota'); pg.locator('button.mp-button').last.click(); time.sleep(0.6)
    pg.locator('button.mp-button').last.click(); time.sleep(0.6)
    pg.locator('button').filter(has_text=re.compile('Daň za nemoc|The Cost of Disease')).first.click()
    pg.wait_for_url(re.compile('Gameplay'), timeout=30000); time.sleep(2)


def screen_text(pg):
    root = '.reveal-panel' if pg.locator('.reveal-panel').count() else '.popup-box' if pg.locator('.popup-box').count() else '.gameplay-container'
    return root, pg.locator(root).first.inner_text()


def walk(pg, a, seed, out, errors):
    rnd = random.Random(seed)
    log = []
    titles = set()
    last = None
    last_seen, same = None, 0
    for step in range(a.steps):
        try:
            crashed = pg.locator('#blazor-error-ui').is_visible()
        except Exception as e:
            log.append(f'PAGE ERROR {e.__class__.__name__}: {str(e)[:80]}'); break
        if crashed:
            log.append('BLAZOR ERROR'); break
        root, text = screen_text(pg)
        title = text.strip().split('\n')[0][:80]
        issues = []
        if BROKEN.search(text):
            issues.append('broken: ' + ', '.join(sorted(set(m.group(0) for m in BROKEN.finditer(text)))))
        english = [w for w in ENGLISH.findall(text)]
        if len(english) >= 3:
            issues.append('english? ' + ' '.join(english[:8]))
        if text != last:
            print(f'  [{seed}:{step}] {title}', flush=True)
            log.append(f'[{step}] {title}' + (('  !! ' + ' | '.join(issues)) if issues else ''))
            if issues:
                pg.screenshot(path=str(out / f'issue-{seed}-{step}.png'), full_page=True)
            titles.add(title)
        last = text
        if 'KONEC?' in text or 'THE END?' in text:
            log.append('END reached'); break

        same = same + 1 if text == last_seen else 0
        last_seen = text
        for field in pg.locator(f'{root} input.input-field').all():
            numeric = field.get_attribute('type') == 'number' or field.get_attribute('inputmode') == 'numeric' or same % 2 == 0
            field.fill(str(rnd.randint(1, 4)) if numeric else rnd.choice(['Golem', 'Zázrak', 'heslo']))
        clickables = [c for c in pg.locator(f'{root} a, {root} button').all() if c.is_visible() and c.inner_text().strip() not in ('', '⬅', '⏸')]
        if not clickables:
            if root == '.reveal-panel':  # countdown running
                time.sleep(1); continue
            log.append('no links'); break
        end_round = [c for c in clickables if 'end-round-button' in (c.get_attribute('class') or '')]
        if root == '.reveal-panel':  # bid / vote countdown: Start, then Accept (never Back)
            green = [c for c in clickables if 'green' in (c.get_attribute('class') or '')]
            if not green:
                time.sleep(1); continue
            choice = green[0]
        elif end_round and rnd.random() < 0.55:
            choice = end_round[0]
        else:
            choice = rnd.choice(clickables)
        try:
            choice.click(timeout=5000)
        except Exception as e:  # covered by an overlay: try the popup's own button next time
            log.append(f'click failed: {e.__class__.__name__}')
            if sum(1 for line in log[-6:] if line.startswith('click failed')) >= 6:
                pg.screenshot(path=str(out / f'stuck-{seed}-{step}.png'))
                log.append('STUCK'); break
        time.sleep(0.35)
    (out / f'walk-{seed}.txt').write_text('\n'.join(log), encoding='utf-8')
    return log, titles


def main():
    a = args()
    out = Path(a.out); out.mkdir(parents=True, exist_ok=True)
    with sync_playwright() as p:
        browser = p.chromium.launch(channel='msedge', headless=True, args=['--mute-audio'])
        for seed in [int(s) for s in a.seeds.split(',')]:
            ctx = browser.new_context(viewport={'width': 1100, 'height': 900})
            ctx.add_init_script(f"try{{localStorage.setItem('mfwLanguage','{a.lang}')}}catch(e){{}}")
            pg = ctx.new_page()
            errors = []
            pg.on('pageerror', lambda e: errors.append(str(e)))
            pg.on('console', lambda m: errors.append(m.text) if m.type == 'error' and '404' not in m.text else None)
            setup(pg, a, random.Random(seed))
            log, titles = walk(pg, a, seed, out, errors)
            issues = [l for l in log if '!!' in l or 'ERROR' in l]
            print(f'seed {seed}: {len(log)} screens, {len(titles)} distinct, end: {log[-1]}, issues: {len(issues)}, console errors: {len(errors)}')
            for line in issues[:15]:
                print('   ', line)
            for e in errors[:5]:
                print('    console:', e[:200])
            ctx.close()
        browser.close()


if __name__ == '__main__':
    main()
