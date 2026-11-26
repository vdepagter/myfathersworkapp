"""Builds the small supplement fonts with the Czech letters that Averia (text) and Frijole (headings) lack.

    python tools/fonts/czech-supplement.py [--preview out.png]

Averia has no ě č ř ů ň ť ď, Frijole has no caron letters at all. Each missing letter is drawn from the font's own
base letter and its own accent (the caron of š in Averia, Frijole's circumflex turned upside down, the ring of å), so
it matches. The result holds only those letters, under a new family name (both fonts are OFL with a Reserved Font
Name, so a modified version may not use the original name):

    wwwroot/fonts/mfw-czech-text-{regular,bold,italic,bolditalic}.woff2   family "MFW Czech Text"
    wwwroot/fonts/mfw-czech-heading.woff2                                 family "MFW Czech Heading"

css/theme.css lists them after the original font, so the browser takes only the missing letters from them.
"""
import sys
from pathlib import Path

from fontTools.ttLib import TTFont
from fontTools.ttLib.tables._g_l_y_f import Glyph, GlyphCoordinates
from fontTools import subset

FONTS = Path(__file__).resolve().parents[2] / 'MyFathersWorkApp' / 'MyFathersWorkApp' / 'wwwroot' / 'fonts'

CARON_LOWER = {'ě': 'e', 'č': 'c', 'ř': 'r', 'ň': 'n', 'š': 's', 'ž': 'z'}
CARON_UPPER = {'Ě': 'E', 'Č': 'C', 'Ř': 'R', 'Ň': 'N', 'Ť': 'T', 'Ď': 'D', 'Š': 'S', 'Ž': 'Z'}
RING = {'ů': ('u', False), 'Ů': ('U', True)}
APOSTROPHE = {'ť': 't', 'ď': 'd'}


def contours(font, name):
    """The outline of a glyph (composites flattened) as a list of contours [(x, y, on_curve), ...]."""
    glyf = font['glyf']
    coords, ends, flags = glyf[name].getCoordinates(glyf)
    result, start = [], 0
    for end in ends:
        result.append([(coords[i][0], coords[i][1], bool(flags[i] & 1)) for i in range(start, end + 1)])
        start = end + 1
    return result


def bounds(cs):
    xs = [p[0] for c in cs for p in c]
    ys = [p[1] for c in cs for p in c]
    return min(xs), min(ys), max(xs), max(ys)


def accent_of(font, accented, base):
    """The contours of an accented glyph that lie above its base letter (the accent)."""
    top = bounds(contours(font, base))[3]
    return [c for c in contours(font, accented) if bounds([c])[1] > top * 0.92]


def move(cs, dx=0.0, dy=0.0, scale=1.0, flip=False):
    x0, y0, x1, y1 = bounds(cs)
    out = []
    for c in cs:
        pts = [(x, (y0 + y1) - y if flip else y, on) for x, y, on in c]
        if flip:
            pts = pts[::-1]  # keep the contour direction after the mirror
        out.append([(x0 + (x - x0) * scale + dx, y0 + (y - y0) * scale + dy, on) for x, y, on in pts])
    return out


def centred(mark, base_cs, dy=0.0):
    bx0, _, bx1, _ = bounds(base_cs)
    mx0, _, mx1, _ = bounds(mark)
    return move(mark, dx=(bx0 + bx1) / 2 - (mx0 + mx1) / 2, dy=dy)


def set_glyph(font, name, cs, advance, lsb_from=None):
    g = Glyph()
    g.numberOfContours = len(cs)
    pts = [p for c in cs for p in c]
    g.coordinates = GlyphCoordinates([(round(x), round(y)) for x, y, _ in pts])
    g.flags = bytearray(1 if on else 0 for _, _, on in pts)
    ends, n = [], 0
    for c in cs:
        n += len(c)
        ends.append(n - 1)
    g.endPtsOfContours = ends
    g.program = None
    from fontTools.ttLib.tables import ttProgram
    g.program = ttProgram.Program()
    g.program.fromBytecode(b'')
    glyf = font['glyf']
    glyf[name] = g
    g.recalcBounds(glyf)
    font['hmtx'][name] = (advance, g.xMin)
    if name not in font.getGlyphOrder():
        font.setGlyphOrder(font.getGlyphOrder() + [name])


def build(source, target, family, style, caron_lower, caron_upper, ring_lower, ring_upper):
    font = TTFont(FONTS / source)
    cmap = font.getBestCmap()
    gname = lambda ch: cmap[ord(ch)]
    hmtx = font['hmtx']
    new = {}

    def add(ch, cs, advance):
        name = f'uni{ord(ch):04X}'
        set_glyph(font, name, cs, advance)
        new[ord(ch)] = name

    for ch, base in CARON_LOWER.items():
        if ord(ch) in cmap: continue
        b = contours(font, gname(base))
        add(ch, b + centred(caron_lower, b), hmtx[gname(base)][0])
    for ch, base in CARON_UPPER.items():
        if ord(ch) in cmap: continue
        b = contours(font, gname(base))
        add(ch, b + centred(caron_upper, b), hmtx[gname(base)][0])
    for ch, (base, upper) in RING.items():
        if ord(ch) in cmap: continue
        b = contours(font, gname(base))
        add(ch, b + centred(ring_upper if upper else ring_lower, b), hmtx[gname(base)][0])

    # ť ď: the caron is a small apostrophe after the stem
    apostrophe = contours(font, gname('’'))
    for ch, base in APOSTROPHE.items():
        if ord(ch) in cmap: continue
        b = contours(font, gname(base))
        bx0, _, bx1, by1 = bounds(b)
        mark = move(apostrophe, scale=0.8)
        mx0, _, mx1, my1 = bounds(mark)
        shift_x = bx1 - mx0 - (mx1 - mx0) * 0.15
        mark = move(mark, dx=shift_x, dy=by1 - my1)
        advance = max(hmtx[gname(base)][0], round(bounds(mark)[2] + (hmtx[gname(base)][0] - bx1)))
        add(ch, b + mark, advance)

    for ch, name in new.items():
        for table in font['cmap'].tables:
            if table.isUnicode():
                table.cmap[ch] = name

    options = subset.Options()
    options.flavor = 'woff2'
    options.name_IDs = ['*']
    options.notdef_outline = True
    options.layout_features = []
    sub = subset.Subsetter(options)
    sub.populate(unicodes=list(new))
    sub.subset(font)

    names = font['name']
    full = f'{family} {style}' if style != 'Regular' else family
    ps = (family + '-' + style).replace(' ', '')
    for rec in list(names.names):
        if rec.nameID in (1, 3, 4, 6, 16, 17, 18, 21, 22):
            names.removeNames(nameID=rec.nameID)
    for nid, value in [(1, family), (2, style), (3, ps), (4, full), (6, ps)]:
        names.setName(value, nid, 3, 1, 0x409)
        names.setName(value, nid, 1, 0, 0)
    copyright_note = names.getDebugName(0) or ''
    names.setName(copyright_note + ' Czech letters added for My Father\'s Work (Unofficial), Nov 2026.', 0, 3, 1, 0x409)
    font.flavor = 'woff2'
    font.save(FONTS / target)
    return ''.join(chr(c) for c in sorted(new))


def averia(style_file, style):
    font = TTFont(FONTS / style_file)
    caron_l = accent_of(font, 'scaron', 's')
    caron_u = accent_of(font, 'Scaron', 'S')
    ring_l = accent_of(font, 'aring', 'a')
    ring_u = accent_of(font, 'Aring', 'A')
    target = style_file.replace('averia-', 'mfw-czech-text-')
    return target, build(style_file, target, 'MFW Czech Text', style, caron_l, caron_u, ring_l, ring_u)


def frijole():
    font = TTFont(FONTS / 'frijole.woff2')
    glyf = font['glyf']

    def placed(composite, mark):
        comp = next(c for c in glyf[composite].components if c.glyphName == mark)
        return move(contours(font, mark), dx=comp.x, dy=comp.y)

    caron_l = move(placed('ecircumflex', 'circumflex'), flip=True)
    caron_u = move(placed('Ecircumflex', 'circumflex'), flip=True)
    ring_l = accent_of(font, 'aring', 'a')
    ring_u = accent_of(font, 'Aring', 'A')
    return 'mfw-czech-heading.woff2', build('frijole.woff2', 'mfw-czech-heading.woff2', 'MFW Czech Heading', 'Regular', caron_l, caron_u, ring_l, ring_u)


def preview(path):
    from PIL import Image, ImageDraw, ImageFont
    import io
    rows = [('averia-regular.woff2', 'mfw-czech-text-regular.woff2'), ('averia-bold.woff2', 'mfw-czech-text-bold.woff2'),
            ('averia-italic.woff2', 'mfw-czech-text-italic.woff2'), ('averia-bolditalic.woff2', 'mfw-czech-text-bolditalic.woff2'),
            ('frijole.woff2', 'mfw-czech-heading.woff2')]
    text = 'Příliš žluťoučký kůň úpěl ďábelské ódy. ŘEŠENÍ ŤĎŮĚČŇ'
    img = Image.new('RGB', (1900, 110 * len(rows)), 'white')
    draw = ImageDraw.Draw(img)

    def ttf(name):
        f = TTFont(FONTS / name)
        f.flavor = None
        buf = io.BytesIO()
        f.save(buf)
        buf.seek(0)
        return ImageFont.truetype(buf, 56)

    for i, (main, extra) in enumerate(rows):
        main_font, extra_font = ttf(main), ttf(extra)
        main_cmap = TTFont(FONTS / main).getBestCmap()
        x = 10
        for ch in text:
            f = main_font if ord(ch) in main_cmap else extra_font
            draw.text((x, 20 + i * 110), ch, font=f, fill='black')
            x += f.getlength(ch)
    img.save(path)


if __name__ == '__main__':
    for file, style in [('averia-regular.woff2', 'Regular'), ('averia-bold.woff2', 'Bold'),
                        ('averia-italic.woff2', 'Italic'), ('averia-bolditalic.woff2', 'Bold Italic')]:
        print(*averia(file, style))
    print(*frijole())
    if '--preview' in sys.argv:
        preview(sys.argv[sys.argv.index('--preview') + 1])
