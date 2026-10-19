"""Machetare simplă pentru conținutul existent al studiului individual."""

from pathlib import Path
from xml.sax.saxutils import escape
import json
import math
import re
import textwrap
import unicodedata

from PIL import Image as PILImage
from reportlab.lib.colors import HexColor, black
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.utils import ImageReader
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    BaseDocTemplate, CondPageBreak, Flowable, Frame, PageBreak, PageTemplate,
    Paragraph, Spacer, Table, TableStyle,
)
from reportlab.platypus.tableofcontents import TableOfContents

OUT = Path(__file__).resolve().parents[1]
PDF = OUT / 'Studiu_Individual_Zombie_Survivors_Bragari_Eduard.pdf'
CAP = OUT / 'assets' / 'capturi_originale'
CLEAN = OUT / 'assets' / 'capturi_fara_cursor'
CROP = OUT / 'assets' / 'detalii_simple'
CROP.mkdir(exist_ok=True)
PW, PH = A4
MARGIN = 54
WIDTH = PW - 2 * MARGIN
BG = HexColor('#F7F4EC')
BORDER = HexColor('#3E4541')
ARROW = HexColor('#AE3C26')

for name, filename in [
    ('TNR', 'times.ttf'), ('TNR-Bold', 'timesbd.ttf'),
    ('TNR-Italic', 'timesi.ttf'), ('TNR-BoldItalic', 'timesbi.ttf'),
    ('Consolas', 'consola.ttf'), ('Consolas-Bold', 'consolab.ttf'),
]:
    pdfmetrics.registerFont(TTFont(name, str(Path('C:/Windows/Fonts') / filename)))
pdfmetrics.registerFontFamily('TNR', normal='TNR', bold='TNR-Bold',
                             italic='TNR-Italic', boldItalic='TNR-BoldItalic')

BODY = ParagraphStyle('Text', fontName='TNR', fontSize=12, leading=18,
                      textColor=black, spaceAfter=6, allowWidows=0,
                      allowOrphans=0, splitLongWords=True)
H1 = ParagraphStyle('Titlu', parent=BODY, fontName='TNR-Bold', fontSize=16,
                    leading=21, spaceBefore=14, spaceAfter=8, keepWithNext=True)
H2 = ParagraphStyle('Subtitlu', parent=H1, fontSize=13, leading=17.6,
                    spaceBefore=12, spaceAfter=6)
CELL = ParagraphStyle('Celulă', parent=BODY, spaceAfter=0, leading=18)
CELL_HEAD = ParagraphStyle('Antet tabel', parent=CELL, fontName='TNR-Bold')


def without_diacritics(value):
    return ''.join(char for char in unicodedata.normalize('NFD', value)
                   if unicodedata.category(char) != 'Mn')


def simple_text(value):
    value = value.replace('\u2011', '-').replace('\u2013', '-').replace('\u2014', '-')
    return without_diacritics(value)


class SectionParagraph(Paragraph):
    def __init__(self, text, style, entry=None):
        super().__init__(text, style)
        self.entry = entry


class CodeLines(Flowable):
    """Cod cu împachetare vizuală și despărțire între rânduri, fără casetă."""

    def __init__(self, lines, record, first=True):
        super().__init__()
        self.lines = lines
        self.record = record
        self.first = first
        self.font_size = 10.5
        self.leading = 14.2
        self.spaceAfter = 8

    def wrap(self, availWidth, availHeight):
        self.width = availWidth
        self.height = len(self.lines) * self.leading + 4
        return self.width, self.height

    def split(self, availWidth, availHeight):
        count = int((availHeight - 4) / self.leading)
        if count < 3:
            return []
        if len(self.lines) - count < 3:
            count = len(self.lines) - 3
        if count < 3:
            return []
        return [CodeLines(self.lines[:count], self.record, self.first),
                CodeLines(self.lines[count:], self.record, False)]

    def draw(self):
        c = self.canv
        if self.first:
            self.record['code_page'] = c.getPageNumber()
        c.setFont('Consolas', self.font_size)
        c.setFillColor(black)
        y = self.height - self.font_size - 1
        for line in self.lines:
            c.drawString(2, y, without_diacritics(line))
            y -= self.leading


class AnnotatedScreenshot(Flowable):
    def __init__(self, path, crop, annotations, record, maxh):
        super().__init__()
        self.path, self.crop = path, crop
        self.annotations, self.record = annotations, record
        self.maxh = maxh
        self.spaceAfter = 9
        with PILImage.open(path) as im:
            im.crop(crop).save(CROP / f"{record['number']:02d}_{path.stem}.png")
        self.image = CROP / f"{record['number']:02d}_{path.stem}.png"

    def wrap(self, availWidth, availHeight):
        iw, ih = self.crop[2] - self.crop[0], self.crop[3] - self.crop[1]
        self.scale = min(availWidth / iw, self.maxh / ih)
        self.iw, self.ih = iw * self.scale, ih * self.scale
        self.width, self.height = availWidth, self.ih
        return self.width, self.height

    def draw(self):
        c = self.canv
        self.record['page'] = c.getPageNumber()
        fx = (self.width - self.iw) / 2
        c.drawImage(str(self.image), fx, 0, width=self.iw, height=self.ih)

        def position(x, y):
            return fx + (x - self.crop[0]) * self.scale, self.ih - (y - self.crop[1]) * self.scale

        for number, tx, ty, bx, by in self.annotations:
            ex, ey = position(tx, ty)
            cx, cy = position(bx, by)
            distance = math.hypot(ex - cx, ey - cy)
            radius = 8
            c.setStrokeColor(ARROW)
            c.setFillColor(ARROW)
            c.setLineWidth(.85)
            if distance > radius + 1:
                sx, sy = cx + (ex - cx) * radius / distance, cy + (ey - cy) * radius / distance
                c.line(sx, sy, ex, ey)
                angle = math.atan2(ey - sy, ex - sx)
                arrow = c.beginPath()
                arrow.moveTo(ex, ey)
                arrow.lineTo(ex - 5 * math.cos(angle - .4), ey - 5 * math.sin(angle - .4))
                arrow.lineTo(ex - 5 * math.cos(angle + .4), ey - 5 * math.sin(angle + .4))
                arrow.close()
                c.drawPath(arrow, fill=1, stroke=0)
            c.setFillColor(BG)
            c.circle(cx, cy, radius, fill=1, stroke=1)
            c.setFont('TNR-Bold', 12)
            c.setFillColor(ARROW)
            c.drawCentredString(cx, cy - 4, str(number))


class StudyDocument(BaseDocTemplate):
    def __init__(self, owner):
        super().__init__(str(PDF), pagesize=A4, leftMargin=MARGIN, rightMargin=MARGIN,
                         topMargin=MARGIN, bottomMargin=MARGIN, pageCompression=1,
                         title='Studiul Individual Nr. 1 - Zombie Survivors',
                         author='Bragari Eduard', subject=simple_text('Programarea vizuală - CEITI, P-2432'),
                         allowSplitting=1)
        self.owner = owner
        frame = Frame(MARGIN, MARGIN, WIDTH, PH - 2 * MARGIN,
                      leftPadding=0, rightPadding=0, topPadding=0, bottomPadding=0)
        self.addPageTemplates(PageTemplate(id='Simplu', frames=frame, onPage=self.background))

    @staticmethod
    def background(c, doc):
        c.saveState()
        c.setFillColor(BG)
        c.rect(0, 0, PW, PH, fill=1, stroke=0)
        c.setStrokeColor(BORDER)
        c.setLineWidth(.55)
        c.rect(24, 24, PW - 48, PH - 48, fill=0, stroke=1)
        if doc.page > 1:
            c.setFont('TNR', 12)
            c.setFillColor(black)
            c.drawCentredString(PW / 2, 36, str(doc.page))
        c.restoreState()

    def beforeDocument(self):
        for entry in self.owner.sections:
            entry.pop('page', None)

    def afterFlowable(self, obj):
        if isinstance(obj, SectionParagraph) and obj.entry:
            entry = obj.entry
            entry['page'] = self.page
            key = entry['key']
            self.canv.bookmarkPage(key)
            level = entry['level']
            self.canv.addOutlineEntry(entry['title'], key, level=level, closed=False)
            self.notify('TOCEntry', (level, entry['title'], self.page, key))
        if getattr(obj, 'excerpt_label', None):
            obj.excerpt_label['page'] = self.page


class Study:
    def __init__(self):
        self.story = []
        self.figures, self.excerpts, self.sections = [], [], []
        self.in_cover = True
        self.pending_toc = False
        self.last_heading = None

    def new(self, title='', label='', key=None):
        if not title:
            return
        if title == 'Cuprins':
            self.in_cover = False
            self.story.append(PageBreak())
            self.story.append(Paragraph('Cuprins', H1))
            toc = TableOfContents()
            toc.dotsMinLevel = 0
            toc.levelStyles = [
                ParagraphStyle('Cuprins capitol', parent=BODY, spaceBefore=6,
                               spaceAfter=0, leading=18, rightIndent=25),
                ParagraphStyle('Cuprins subcapitol', parent=BODY, leftIndent=16,
                               spaceBefore=0, spaceAfter=0, leading=18, rightIndent=25),
            ]
            self.story.append(toc)
            self.pending_toc = True
            return
        if self.pending_toc:
            self.story.append(PageBreak())
            self.pending_toc = False
        if title == '10. Bibliografie':
            self.story.append(PageBreak())
        # Titlurile de continuare rămân subtitluri; nu forțează pagini noi.
        self.heading(title)

    def heading(self, text, size=None, leading=None, after=None):
        major = bool(re.match(r'^\d+\. ', text))
        numbered = bool(re.match(r'^\d+\.', text))
        entry = None
        if numbered:
            entry = {'title': simple_text(text), 'level': 0 if major else 1,
                     'key': f'sectiune-{len(self.sections) + 1}'}
            self.sections.append(entry)
        obj = SectionParagraph(simple_text(text), H1 if major else H2, entry)
        self.story.append(obj)
        self.last_heading = obj

    def section(self, title):
        self.heading(title)

    def bookmark(self, key, title):
        # Destinațiile sunt create din titlurile efectiv paginate.
        pass

    def p(self, text, *, size=None, leading=None, after=6, color=None,
          font=None, align=0, style=None):
        if self.in_cover:
            fs = min(16, max(12, size or 12))
            st = ParagraphStyle('Copertă', parent=BODY, fontName=font or 'TNR',
                                fontSize=fs, leading=max(fs * 1.35, leading or 0),
                                alignment=align, spaceAfter=after)
        else:
            st = ParagraphStyle('Paragraf', parent=BODY, alignment=align,
                                fontName=font or 'TNR', spaceAfter=6)
        self.story.append(Paragraph(simple_text(text), st))

    def gap(self, h=5):
        if self.in_cover:
            self.story.append(Spacer(1, h))

    def note(self, title, text):
        # Păstrăm informația tehnică în paragrafe obișnuite, fără etichete.
        if title in {'Cum se citește lucrarea.', 'Observații din capturarea aplicației.',
                     'Despre imagini și cod.', 'Versiunea analizată.'}:
            return
        self.p(text)

    def table(self, headers, rows, widths=None, size=None):
        widths = [v * WIDTH for v in widths] if widths else [WIDTH / len(headers)] * len(headers)
        # Ultimul tabel de statistici avea proporțiile însumate la 110%.
        widths = [v * WIDTH / sum(widths) for v in widths]
        data = [[Paragraph(simple_text(str(v)), CELL_HEAD) for v in headers]]
        data.extend([Paragraph(simple_text(str(v)), CELL) for v in row] for row in rows)
        obj = Table(data, colWidths=widths, repeatRows=1, hAlign='LEFT', spaceAfter=9)
        obj.setStyle(TableStyle([
            ('GRID', (0, 0), (-1, -1), .4, HexColor('#555555')),
            ('VALIGN', (0, 0), (-1, -1), 'TOP'),
            ('LEFTPADDING', (0, 0), (-1, -1), 6),
            ('RIGHTPADDING', (0, 0), (-1, -1), 6),
            ('TOPPADDING', (0, 0), (-1, -1), 5),
            ('BOTTOMPADDING', (0, 0), (-1, -1), 5),
        ]))
        # Rezervăm loc pentru titlu, antet și primul rând; tabelul lung se poate
        # continua firesc pe pagina următoare, fără a lăsa jumătate de pagină goală.
        if self.story and isinstance(self.story[-1], SectionParagraph):
            heading = self.story.pop()
            heading.keepWithNext = False
            obj.wrap(WIDTH, 10000)
            first_height = sum(obj._rowHeights[:2])
            self.story.extend([CondPageBreak(first_height + 42), heading])
        self.story.append(obj)

    def row(self, left_fn, right_fn, ratio=.5, gutter=14):
        # Explicațiile din vechea coloană alăturată precedă acum captura.
        start = len(self.story)
        left_fn()
        middle = len(self.story)
        right_fn()
        left = self.story[start:middle]
        right = self.story[middle:]
        if (any(isinstance(obj, AnnotatedScreenshot) for obj in left)
                and not any(isinstance(obj, AnnotatedScreenshot) for obj in right)):
            self.story[start:] = right + left

    def fig(self, name, caption, *, crop=None, maxh=260, annotations=(), rects=(), maxw=None):
        path = CAP / (name + '.png')
        clean_names = {'02_vs_toolbox', '19_vs_menu_designer', '09_game_settings',
                       '15_game_pause', '17_game_over', '18_game_respawn'}
        full_gameplay = name == '14_gameplay' and crop == (0, 24, 1920, 1032)
        if name in clean_names or full_gameplay:
            path = CLEAN / (name + '.png')
        if full_gameplay:
            crop = (0, 31, 1491, 582)
            annotations = [(1, 737, 304, 887, 320), (2, 704, 306, 565, 273),
                           (3, 144, 68, 184, 150), (4, 143, 557, 362, 509),
                           (5, 49, 517, 150, 443), (6, 745, 63, 946, 147),
                           (7, 1374, 93, 1187, 223)]
        elif name == '18_game_respawn':
            crop = (0, 31, 1491, 582)
            annotations = [(1, 140, 68, 205, 150), (2, 76, 540, 366, 490),
                           (3, 744, 63, 942, 146), (4, 1375, 102, 1180, 221)]
        elif name == '15_game_pause':
            crop = (510, 150, 980, 466)
            annotations = [(1, 744, 264, 943, 245), (2, 744, 317, 943, 317),
                           (3, 744, 368, 943, 392)]
        elif name == '17_game_over':
            crop = (505, 225, 986, 389)
            annotations = [(1, 744, 280, 950, 250), (2, 744, 345, 950, 370)]
        elif name == '09_game_settings':
            crop = (600, 178, 890, 407)
            annotations = [(1, 744, 316, 865, 219), (2, 744, 375, 865, 383)]
        with PILImage.open(path) as im:
            if crop is None:
                crop = (0, 0, im.width, im.height)
            if crop[0] < 0 or crop[1] < 0 or crop[2] > im.width or crop[3] > im.height:
                raise ValueError(f'Decupaj în afara capturii: {name}: {crop}, {im.size}')
        number = len(self.figures) + 1
        record = {'number': number, 'source': name + '.png',
                  'source_path': str(path.relative_to(OUT)).replace('\\', '/'),
                  'explanation_before_image': simple_text(caption), 'crop': crop,
                  'annotations': list(annotations), 'cursor_checked': True}
        self.figures.append(record)
        # Explicația este înaintea imaginii, la 12 pt, fără etichetă de figură.
        paragraph = Paragraph(simple_text(caption), BODY)
        paragraph.keepWithNext = True
        # Detaliile din interfață sunt mărite față de versiunea anterioară.
        height = maxh
        if name == '02_vs_toolbox':
            height = 415
        elif name == '03_vs_properties_hp':
            height = 265
        elif name == '04_vs_structure':
            height = 310
        elif name in {'01_vs_designer', '12_game_levelup', '15_game_pause'}:
            height = max(height, 210)
        elif name == '05_vs_code':
            height = 355
        elif name == '11_game_debug':
            height = 285
        image = AnnotatedScreenshot(path, crop, annotations, record, height)
        self.story.extend([paragraph, image])

    def code(self, title, parts, context):
        number = len(self.excerpts) + 1
        record = {'number': number, 'title': title, 'context': context, 'parts': parts}
        self.excerpts.append(record)
        label = Paragraph(simple_text(f'Codul {number}. {escape(title)}'),
                          ParagraphStyle('Titlu cod', parent=BODY, fontName='TNR-Bold',
                                         spaceBefore=5, spaceAfter=4, keepWithNext=True))
        label.excerpt_label = record
        labels = [f"{p['file']}:{p['start']}-{p['end']}" for p in parts]
        location = Paragraph(simple_text(escape('; '.join(dict.fromkeys(labels))) +
                                          '<br/>' + escape(context)),
                             ParagraphStyle('Sursă cod', parent=BODY, spaceAfter=6,
                                            keepWithNext=True))
        limit = int((WIDTH - 4) / pdfmetrics.stringWidth('M', 'Consolas', 10.5))
        display = []
        for index, part in enumerate(parts):
            if index:
                display.append('// ... porțiune intermediară omisă ...')
            for line in part['lines']:
                wrapped = textwrap.wrap(line.expandtabs(4), width=limit,
                                        subsequent_indent='    ', replace_whitespace=False,
                                        drop_whitespace=False, break_on_hyphens=False)
                display.extend(wrapped or [''])
        block = CodeLines(display, record)
        # Un fragment care încape pe o pagină rămâne întreg.
        self.story.extend([label, location, block])

    def flow(self, labels, descriptions=None):
        self.p(' → '.join(escape(label) for label in labels))
        if descriptions:
            self.p('; '.join(escape(v) for v in descriptions))

    def finish(self):
        doc = StudyDocument(self)
        doc.multiBuild(self.story)
        names = sorted({x['source'] for x in self.figures})
        manifest = {
            'pdf': PDF.name, 'pages': doc.page, 'figures': self.figures,
            'unique_screenshot_count': len(names), 'screenshots': names,
            'code_excerpt_count': len(self.excerpts), 'code_excerpts': self.excerpts,
            'sections': self.sections, 'font': 'Times New Roman 12 pt; Consolas 10.5 pt',
            'body_leading': 18, 'paragraph_after': 6,
            'border': 'O singură linie dreptunghiulară, 0.55 pt, #3E4541',
            'background': '#F7F4EC', 'source_date': '2026-10-19',
            'diacritics': False,
            'note': 'Conținutul studiului existent, reașezat automat. Capturi reale, '
                    'decupaje fără cursor și săgeți vectoriale. Fără descrieri sub imagini.',
        }
        (OUT / 'sursa' / 'manifest_documentatie.json').write_text(
            json.dumps(manifest, ensure_ascii=False, indent=2), encoding='utf-8')
        print(json.dumps({'pdf': str(PDF), 'pages': doc.page, 'figures': len(self.figures),
                          'screenshots': len(names), 'code_excerpts': len(self.excerpts)},
                         ensure_ascii=False))
