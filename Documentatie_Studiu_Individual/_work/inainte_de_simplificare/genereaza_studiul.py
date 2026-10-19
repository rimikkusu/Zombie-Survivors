from pathlib import Path
import json, math, re, textwrap, hashlib
from xml.sax.saxutils import escape
from PIL import Image as PILImage
from reportlab.pdfgen import canvas
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.lib.colors import HexColor, Color, white
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle
from reportlab.platypus import Paragraph, Table, TableStyle
from reportlab.lib.utils import ImageReader

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parents[1]
CAP = OUT / 'assets' / 'capturi_originale'
CROP = OUT / 'assets' / 'detalii'
CROP.mkdir(exist_ok=True)
PDF = OUT / 'Studiu_Individual_Zombie_Survivors_Bragari_Eduard.pdf'
for name, filename in [('TNR', 'times.ttf'), ('TNR-Bold', 'timesbd.ttf'),
                       ('TNR-Italic', 'timesi.ttf'), ('TNR-BoldItalic', 'timesbi.ttf'),
                       ('Consolas', 'consola.ttf'), ('Consolas-Bold', 'consolab.ttf')]:
    pdfmetrics.registerFont(TTFont(name, str(Path('C:/Windows/Fonts') / filename)))
pdfmetrics.registerFontFamily('TNR', normal='TNR', bold='TNR-Bold',
                             italic='TNR-Italic', boldItalic='TNR-BoldItalic')
BG = HexColor('#F7F4EC')
GREEN = HexColor('#315A52')
INK = HexColor('#202B29')
MUTED = HexColor('#53665F')
LINE = HexColor('#B8C4B9')
PALE = HexColor('#EBEEE4')
ACCENT = HexColor('#C84E32')
PW, PH = A4
LEFT, RIGHT, TOP, BOTTOM = 54, 54, 759, 65
WIDTH = PW - LEFT - RIGHT
STYLE = ParagraphStyle('body', fontName='TNR', fontSize=11.4, leading=14.8,
                       textColor=INK, spaceAfter=7, splitLongWords=True)
SMALL = ParagraphStyle('small', parent=STYLE, fontSize=10.1, leading=12.5)
CAPTION = ParagraphStyle('caption', parent=SMALL, fontName='TNR-Italic', textColor=MUTED)
CELL = ParagraphStyle('cell', parent=STYLE, fontSize=10.1, leading=12.1, spaceAfter=0)
CELL_HEAD = ParagraphStyle('cellhead', parent=CELL, fontName='TNR-Bold', textColor=white)


def source(relative):
    return ROOT / 'Vampire Survivors' / relative


def span(relative, first, last=None, count=None, occurrence=0):
    lines = source(relative).read_text(encoding='utf-8-sig').splitlines()
    indices = [i for i, line in enumerate(lines) if first in line]
    start = indices[occurrence]
    if count is not None:
        end = start + count
    else:
        end = next(i + 1 for i in range(start, len(lines)) if last in lines[i])
    chosen = lines[start:end]
    indent = min((len(s) - len(s.lstrip()) for s in chosen if s.strip()), default=0)
    return dict(file=relative, start=start + 1, end=end,
                lines=[s[indent:] for s in chosen], original='\n'.join(chosen))


class Study:
    def __init__(self):
        self.c = canvas.Canvas(str(PDF), pagesize=A4, pageCompression=1)
        self.c.setTitle('Studiul Individual Nr. 1 - Zombie Survivors')
        self.c.setAuthor('Bragari Eduard')
        self.c.setCreator('ReportLab - documentație Zombie Survivors')
        self.c.setSubject('Programarea vizuală - C# Windows Forms, CEITI, P-2432')
        self.page = 0
        self.y = TOP
        self.x = LEFT
        self.w = WIDTH
        self.figures, self.excerpts, self.sections, self.bottoms = [], [], [], []

    def room(self, h):
        if self.y - h < BOTTOM - 0.1:
            raise RuntimeError(f'Pagina {self.page}: depășire cu {BOTTOM-(self.y-h):.1f} pt, y={self.y}, h={h}')

    def new(self, title='', label='', key=None):
        if self.page:
            self.bottoms.append({'page': self.page, 'bottom': round(self.y, 2)})
            self.c.showPage()
        self.page += 1
        c = self.c
        c.setFillColor(BG)
        c.rect(0, 0, PW, PH, fill=1, stroke=0)
        c.setStrokeColor(GREEN)
        c.setLineWidth(.65)
        c.rect(24, 25, PW-48, PH-50, fill=0, stroke=1)
        # Colțuri geometrice discrete, diferite de modelul de referință.
        for x, dx in [(31, 1), (PW-31, -1)]:
            for y, dy in [(32, 1), (PH-32, -1)]:
                c.setLineWidth(1.2)
                c.line(x, y, x + dx*13, y)
                c.line(x, y, x, y + dy*13)
        c.setFont('TNR', 9)
        c.setFillColor(MUTED)
        c.drawString(LEFT, PH-51, 'CEITI  |  Catedra de Informatică I')
        c.drawRightString(PW-RIGHT, PH-51, 'Programarea vizuală')
        c.setStrokeColor(LINE)
        c.setLineWidth(.45)
        c.line(LEFT, PH-60, PW-RIGHT, PH-60)
        c.line(LEFT, 53, PW-RIGHT, 53)
        c.setFont('TNR', 9.5)
        c.drawString(LEFT, 38, 'Chișinău, 2026')
        c.drawRightString(PW-RIGHT, 38, str(self.page))
        self.y, self.x, self.w = TOP, LEFT, WIDTH
        if label:
            self.p(label.upper(), size=9, leading=11, after=5, color=MUTED, font='TNR-Bold')
        if title:
            self.heading(title, 18, 21, after=12)
        if key:
            self.bookmark(key, title)

    def bookmark(self, key, title):
        self.c.bookmarkPage(key)
        self.c.addOutlineEntry(title, key, level=0, closed=False)

    def p(self, text, *, size=None, leading=None, after=7, color=None, font=None,
          align=0, style=None):
        base = style or STYLE
        st = ParagraphStyle('p', parent=base,
                            fontSize=size or base.fontSize,
                            leading=leading or base.leading,
                            textColor=color or base.textColor,
                            fontName=font or base.fontName, alignment=align)
        obj = Paragraph(text, st)
        _, h = obj.wrap(self.w, 2000)
        self.room(h + after)
        obj.drawOn(self.c, self.x, self.y-h)
        self.y -= h + after
        return h + after

    def heading(self, text, size=13.7, leading=16.5, after=6):
        return self.p(text, size=size, leading=leading, font='TNR-Bold', color=GREEN, after=after)

    def section(self, title):
        self.sections.append({'title': title, 'page': self.page})
        self.heading(title)

    def gap(self, h=5):
        self.room(h)
        self.y -= h

    def note(self, title, text):
        st = ParagraphStyle('note', parent=SMALL)
        p = Paragraph(f'<b>{title}</b> {text}', st)
        _, h = p.wrap(self.w-22, 2000)
        self.room(h+24)
        self.c.setFillColor(PALE)
        self.c.roundRect(self.x, self.y-h-17, self.w, h+17, 3, fill=1, stroke=0)
        self.c.setStrokeColor(GREEN)
        self.c.setLineWidth(2)
        self.c.line(self.x, self.y, self.x, self.y-h-17)
        p.drawOn(self.c, self.x+11, self.y-h-8)
        self.y -= h+24

    def table(self, headers, rows, widths=None, size=10.1):
        widths = [v*self.w for v in widths] if widths else [self.w/len(headers)]*len(headers)
        sty = ParagraphStyle('tablecell', parent=CELL, fontSize=size, leading=size+2.1)
        data = [[Paragraph(str(x), CELL_HEAD) for x in headers]]
        data += [[Paragraph(str(x), sty) for x in row] for row in rows]
        t = Table(data, colWidths=widths, hAlign='LEFT')
        t.setStyle(TableStyle([
            ('BACKGROUND', (0,0), (-1,0), GREEN),
            ('ROWBACKGROUNDS', (0,1), (-1,-1), [BG, PALE]),
            ('GRID', (0,0), (-1,-1), .4, LINE),
            ('VALIGN', (0,0), (-1,-1), 'TOP'),
            ('LEFTPADDING', (0,0), (-1,-1), 7),
            ('RIGHTPADDING', (0,0), (-1,-1), 7),
            ('TOPPADDING', (0,0), (-1,-1), 5),
            ('BOTTOMPADDING', (0,0), (-1,-1), 5),
        ]))
        _, h = t.wrap(self.w, 2000)
        self.room(h+9)
        t.drawOn(self.c, self.x, self.y-h)
        self.y -= h+9

    def row(self, left_fn, right_fn, ratio=.5, gutter=14):
        x, w, y = self.x, self.w, self.y
        wl = (w-gutter)*ratio
        wr = w-gutter-wl
        self.w = wl
        left_fn()
        yl = self.y
        self.x, self.w, self.y = x+wl+gutter, wr, y
        right_fn()
        yr = self.y
        self.x, self.w, self.y = x, w, min(yl, yr)

    def arrow(self, sx, sy, tx, ty):
        c = self.c
        c.setStrokeColor(ACCENT)
        c.setFillColor(ACCENT)
        c.setLineWidth(1)
        c.line(sx, sy, tx, ty)
        a = math.atan2(ty-sy, tx-sx)
        p = c.beginPath()
        p.moveTo(tx, ty)
        p.lineTo(tx-6*math.cos(a-.45), ty-6*math.sin(a-.45))
        p.lineTo(tx-6*math.cos(a+.45), ty-6*math.sin(a+.45))
        p.close()
        c.drawPath(p, fill=1, stroke=0)

    def fig(self, name, caption, *, crop=None, maxh=260, annotations=(), rects=(), maxw=None):
        path = CAP / (name+'.png')
        im = PILImage.open(path)
        if crop is None:
            crop = (0,0,im.width,im.height)
        im = im.crop(crop)
        cropped = CROP / (name+'_'+('_'.join(map(str,crop)))+'.png')
        im.save(cropped)
        scale = min((maxw or self.w)/im.width, maxh/im.height)
        iw, ih = im.width*scale, im.height*scale
        fx, fy = self.x+(self.w-iw)/2, self.y-ih
        number = len(self.figures)+1
        cp = Paragraph(f'<b>Figura {number}.</b> {caption}', CAPTION)
        _, ch = cp.wrap(self.w, 2000)
        self.room(ih+ch+14)
        self.c.drawImage(ImageReader(im), fx, fy, width=iw, height=ih)
        self.c.setStrokeColor(LINE)
        self.c.setLineWidth(.45)
        self.c.rect(fx,fy,iw,ih,fill=0,stroke=1)
        def pos(x,y):
            return fx+(x-crop[0])*scale, fy+ih-(y-crop[1])*scale
        for rx,ry,rw,rh in rects:
            px,py = pos(rx,ry)
            self.c.setStrokeColor(ACCENT)
            self.c.setLineWidth(.9)
            self.c.rect(px,py-rh*scale,rw*scale,rh*scale,fill=0,stroke=1)
        for num, tx,ty,bx,by in annotations:
            ex,ey=pos(tx,ty)
            cx,cy=pos(bx,by)
            distance=math.hypot(ex-cx,ey-cy)
            if distance>10:
                self.arrow(cx+(ex-cx)*8/distance,cy+(ey-cy)*8/distance,ex,ey)
            self.c.setFillColor(BG)
            self.c.setStrokeColor(ACCENT)
            self.c.circle(cx,cy,8.3,fill=1,stroke=1)
            self.c.setFillColor(ACCENT)
            self.c.setFont('Consolas-Bold',9.4)
            self.c.drawCentredString(cx,cy-3,str(num))
        cp.drawOn(self.c,self.x,fy-ch-5)
        self.y=fy-ch-14
        self.figures.append({'number':number,'page':self.page,'source':name+'.png',
                             'caption':caption,'crop':crop,'annotations':list(annotations)})

    def code(self, title, parts, context):
        number = len(self.excerpts)+1
        self.p(f'<b>Codul {number}. {title}</b>', size=10.3, leading=12.5, after=4, color=GREEN)
        labels = []
        for part in parts:
            label=f"{part['file'].replace(chr(92), '/')}:{part['start']}-{part['end']}"
            if label not in labels:
                labels.append(label)
        self.p(' • '.join(escape(x) for x in labels)+' | '+escape(context),
               size=9.2, leading=11.1, after=4, color=MUTED)
        display=[]
        fs=8.45
        charw=pdfmetrics.stringWidth('M','Consolas',fs)
        maxchars=int((self.w-41)/charw)
        for pi,part in enumerate(parts):
            if pi:
                display.append(('', '// ... porțiune intermediară omisă ...'))
            for i,line in enumerate(part['lines']):
                n=str(part['start']+i)
                if len(line)<=maxchars:
                    display.append((n,line))
                else:
                    # Împachetare vizuală; textul original este păstrat în manifest.
                    wrapped=textwrap.wrap(line, maxchars, subsequent_indent='    ',
                                          replace_whitespace=False, drop_whitespace=False,
                                          break_long_words=True, break_on_hyphens=False)
                    display.extend([(n if wi==0 else '', s) for wi,s in enumerate(wrapped)])
        leading=10.7
        h=len(display)*leading+17
        self.room(h+9)
        c=self.c
        c.setFillColor(HexColor('#F0F0E9'))
        c.setStrokeColor(LINE)
        c.setLineWidth(.45)
        c.roundRect(self.x,self.y-h,self.w,h,3,fill=1,stroke=1)
        by=self.y-12
        for n,line in display:
            c.setFont('Consolas',7.4)
            c.setFillColor(MUTED)
            c.drawRightString(self.x+25,by,n)
            c.setFont('Consolas',fs)
            c.setFillColor(MUTED if line.lstrip().startswith('//') else INK)
            c.drawString(self.x+33,by,line)
            by-=leading
        self.y-=h+9
        self.excerpts.append({'number':number,'page':self.page,'title':title,
                              'context':context,'parts':parts})

    def flow(self, labels, descriptions=None):
        n=len(labels)
        gap=16
        boxw=(self.w-gap*(n-1))/n
        height=50 if descriptions else 35
        self.room(height+13)
        c=self.c
        for i,label in enumerate(labels):
            x=self.x+i*(boxw+gap)
            c.setFillColor(PALE)
            c.setStrokeColor(GREEN)
            c.setLineWidth(.7)
            c.roundRect(x,self.y-height,boxw,height,3,fill=1,stroke=1)
            p=Paragraph(label,ParagraphStyle('flow',parent=SMALL,fontName='TNR-Bold',alignment=1,
                                           fontSize=10.1,leading=11.5))
            _,h=p.wrap(boxw-10,100)
            p.drawOn(c,x+5,self.y-h-8)
            if descriptions:
                q=Paragraph(descriptions[i],ParagraphStyle('flowdesc',parent=SMALL,alignment=1,fontSize=9.2,leading=10.5))
                _,qh=q.wrap(boxw-10,100)
                q.drawOn(c,x+5,self.y-height+5)
            if i<n-1:
                self.arrow(x+boxw+2,self.y-height/2,x+boxw+gap-2,self.y-height/2)
        self.y-=height+13

    def finish(self):
        self.bottoms.append({'page':self.page,'bottom':round(self.y,2)})
        self.c.save()
        names=sorted({x['source'] for x in self.figures})
        manifest={'pdf':PDF.name,'pages':self.page,'figures':self.figures,
                  'unique_screenshot_count':len(names),'screenshots':names,
                  'code_excerpt_count':len(self.excerpts),'code_excerpts':self.excerpts,
                  'sections':self.sections,'page_content_bottoms':self.bottoms,
                  'font':'Times New Roman 11.4 pt; Consolas 8.45 pt',
                  'source_date':'2026-10-19',
                  'note':'Capturi reale, doar decupate; adnotări vectoriale adăugate în PDF.'}
        (OUT/'sursa'/'manifest_documentatie.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
        print(json.dumps({'pdf':str(PDF),'pages':self.page,'figures':len(self.figures),
                          'screenshots':len(names),'code_excerpts':len(self.excerpts)},ensure_ascii=False))


d=Study()

# PAGINA 1 — COPERTĂ
d.new()
d.gap(18)
d.p('IP Centrul de Excelență în Informatică<br/>și Tehnologii Informaționale (CEITI)',
    font='TNR-Bold',size=15,leading=19,align=1,after=15)
d.p('Catedra de Informatică I',size=12.5,align=1,after=80)
d.p('Studiul Individual Nr. 1',font='TNR-Bold',size=25,leading=30,align=1,after=12)
d.p('Disciplina: <b>Programarea vizuală</b>',size=14,leading=18,align=1,after=33)
d.p('Tema:',font='TNR-Bold',size=13,align=1,after=9)
d.p('Dezvoltarea jocului desktop<br/><b>Zombie Survivors</b> în C# Windows Forms –<br/>Proiectarea interfeței grafice<br/>și implementarea funcționalităților',
    size=17,leading=23,align=1,after=57)
d.p('<b>Elev:</b> Bragari Eduard<br/><b>Grupa:</b> P-2432<br/><b>Profesor:</b> Diana Nebunealia',
    size=13,leading=22,after=45)
d.p('Chișinău, 2026',size=13,align=1,after=0)

# PAGINA 2 — CUPRINS
d.new('Cuprins','Studiul Individual Nr. 1',key='cuprins')
toc=[
('1. Documentare teoretică',3),('1.1 Ce este programarea vizuală?',3),('1.2 Ce este Windows Forms?',3),('1.3 Limbajul C# și platforma .NET',3),
('1.4 Cerințe funcționale',4),('1.5 Cerințe non-funcționale',4),('1.6 Tabel comparativ',4),
('2. Prezentarea aplicației',5),('2.1 Descrierea jocului',5),('2.2 Funcționalități principale',5),('2.3 Tehnologii utilizate',5),('2.4 Captură generală a jocului',5),
('3. Structura proiectului',6),('3.1 Organizarea fișierelor',6),('3.2 Rolul formularului Form1',6),('3.3 Legătura dintre componente',6),
('4. Realizarea practică a interfeței',7),('4.1 Crearea proiectului Windows Forms',7),('4.2 Utilizarea Toolbox-ului',8),('4.3 Form Designer și Properties',9),
('4.4 Crearea meniului principal',10),('4.5 Crearea barei HP',11),('4.6 Bara XP și nivelul',12),('4.7 Interfața pentru valuri',13),
('4.8 Interfața Level Up',14),('4.8 Aplicarea și afișarea upgrade-urilor',15),('4.9 Pause Menu',16),('4.10 Settings',17),('4.11 Game Over și Respawn',18),
('4.12 Debug Menu',19),('4.13 Legătura dintre UI și cod: Play',20),('4.13 Respawn, stări și actualizare',21),('4.13 Integrarea interfeței cu jocul',22),
('5. Explicarea codului jocului',23),('5.1 Mișcarea jucătorului',23),('5.2 Sistemul de shooting',23),('5.3 Inamicii',24),('5.4 Sistemul de valuri',24),
('5.5 XP și Level Up',24),('5.6 Upgrade-urile',25),('5.7 HP și bandajele',25),('5.8 Randarea grafică',25),
('6. Reflecție personală',26),('7. Git și GitHub',26),('7.1 Ce sunt Git și GitHub?',26),('7.2 Utilizarea Git în proiect',26),('7.3 Repository-ul proiectului',26),
('8. README și alte comenzi Git',27),('8.1 Ce este README?',27),('8.2 Comenzi Git utile',27),('9. Concluzie',27),('10. Bibliografie',28)]
def toc_column(items):
    for title,page in items:
        major=bool(re.match(r'^\d+\. ',title))
        f='TNR-Bold' if major else 'TNR'
        fs=10.15 if major else 9.85
        st=ParagraphStyle('toc',parent=SMALL,fontName=f,fontSize=fs,leading=12.6,textColor=GREEN if major else INK)
        p=Paragraph(escape(title),st)
        _,h=p.wrap(d.w-20,200)
        d.room(h+4)
        p.drawOn(d.c,d.x,d.y-h)
        d.c.setFont(f,fs)
        d.c.drawRightString(d.x+d.w,d.y-9.5,str(page))
        d.y-=h+4
d.row(lambda:toc_column(toc[:27]),lambda:toc_column(toc[27:]),ratio=.51,gutter=20)
d.gap(8)
d.note('Cum se citește lucrarea.', 'Capitolul 4 ocupă paginile 7–22. Figurile provin din aplicația reală și din Visual Studio; codul numerotat este extras din versiunea locală analizată la 19 octombrie 2026.')

# PAGINA 3 — TEORIE
d.new('1. Documentare teoretică','Noțiuni de bază',key='teorie')
d.section('1.1 Ce este programarea vizuală?')
d.p('Programarea vizuală permite construirea interfeței unei aplicații prin elemente grafice: ferestre, butoane, liste și panouri. În Visual Studio, pot așeza un control pe formular și îi pot modifica dimensiunea sau culoarea fără să scriu manual fiecare instrucțiune de configurare. Designer-ul salvează aceste alegeri în cod C#. [1, 2]')
d.p('Configurarea vizuală stabilește <i>cum arată</i> interfața. Codul scris de programator stabilește <i>ce se întâmplă</i> când utilizatorul interacționează cu ea. De exemplu, butonul PLAY poate fi desenat și poziționat în Designer, dar pornirea unei runde este realizată de metoda C# asociată evenimentului Click.')
d.p('Acest mod de lucru se folosește la aplicații de evidență, formulare de înscriere, calculatoare, editoare și proiecte educaționale. În Zombie Survivors, aceeași abordare este aplicată meniurilor și indicatoarelor jocului.')
d.section('1.2 Ce este Windows Forms?')
d.p('Windows Forms este o tehnologie .NET pentru aplicații desktop pe Windows. O fereastră este reprezentată printr-un obiect Form, iar în interiorul ei pot exista controale precum Button, Label, Panel sau ComboBox. Controalele expun proprietăți, metode și evenimente. Pentru desene proprii, formularul oferă acces la obiectul Graphics. [1]')
d.p('Pentru proiectul meu, avantajul principal este că pot vedea imediat structura interfeței și pot conecta fiecare buton la o metodă. Folosesc controale standard pentru meniuri și HUD, iar arena și personajele sunt desenate separat. Astfel, pot exersa atât programarea interfeței, cât și organizarea logicii în clase.')
d.section('1.3 Limbajul C# și platforma .NET')
d.p('C# este limbajul în care este scris proiectul, iar .NET oferă mediul de execuție și bibliotecile folosite. Fișierul proiectului declară ținta <b>net10.0-windows</b> și opțiunea <b>UseWindowsForms</b>. Aceasta este configurația reală analizată, nu o aplicație .NET Framework mai veche. [3, 4]')
d.table(['Noțiune', 'Exemplu concret în proiect'],[
('Clasă și obiect','Player descrie un jucător; session.Player este obiectul folosit în rundă.'),
('Variabilă / proprietate','Health păstrează viața curentă, iar Level păstrează nivelul.'),
('Metodă','Reset() reinițializează sesiunea; UpdateHealth() actualizează HUD-ul.'),
('Eveniment','Click anunță apăsarea unui buton; Tick declanșează actualizarea periodică.')],[.29,.71])

# PAGINA 4 — CERINȚE
d.new('Cerințele aplicației','Capitolul 1 • continuare')
d.section('1.4 Cerințe funcționale')
d.p('O cerință funcțională descrie o acțiune pe care aplicația trebuie să o realizeze. În acest proiect, jucătorul se deplasează cu WASD, țintește cu mouse-ul și trage cu butonul stâng. Inamicii apar în valuri, urmăresc jucătorul și oferă XP atunci când sunt eliminați. La creșterea nivelului se afișează trei opțiuni de upgrade.')
d.p('Interfața trebuie să permită pornirea jocului, pauza, alegerea modului de afișare și reluarea unei runde după Game Over. Bara HP, progresul XP, nivelul și numărul inamicilor trebuie să reflecte datele curente ale sesiunii.')
d.section('1.5 Cerințe non-funcționale')
d.p('O cerință non-funcțională descrie calitatea utilizării: claritate, stabilitate, performanță sau compatibilitate. Pentru un joc cu multe actualizări, interfața trebuie să rămână lizibilă, iar obiectele temporare trebuie eliminate când nu mai sunt necesare. Compatibilitatea vizată este Windows, conform configurației proiectului.')
d.p('Timer-ul are intervalul configurat la 16 ms, adică un ritm nominal de aproximativ 62,5 actualizări pe secundă. Această setare nu demonstrează un număr garantat de cadre: încărcarea calculatorului și mesajele interfeței pot întârzia Tick. În lucrare descriu mecanismele existente, fără a atribui rezultate de performanță nemăsurate.')
d.section('1.6 Tabel comparativ')
d.table(['Tip', 'Cerință', 'Realizare în proiect'],[
('Funcțională','Deplasare și țintire','GameInputController citește tastele; Player actualizează poziția și unghiul.'),
('Funcțională','Luptă și inamici','CombatSystem gestionează proiectilele; EnemyManager actualizează inamicii.'),
('Funcțională','Progresie','XP, niveluri și o alegere de upgrade pentru fiecare nivel câștigat.'),
('Funcțională','Navigare în meniuri','ScreenController afișează panoul corespunzător stării curente.'),
('Non-funcțională','Claritatea interfeței','Text alb pe panouri închise; bare distincte: roșu pentru HP și cyan pentru XP.'),
('Non-funcțională','Stabilitate la resetare','O rundă nouă golește listele de obiecte și resetează progresul.'),
('Non-funcțională','Utilizarea resurselor','Resursele bitmap sunt reutilizate; renderer-ul eliberează obiectele proprii.'),
('Non-funcțională','Adaptarea ferestrei','Anchor, Dock și recentrarea panourilor gestionează redimensionarea.')],[.20,.27,.53],size=10)

# PAGINA 5 — APLICAȚIE
d.new('2. Prezentarea aplicației','Zombie Survivors',key='aplicatie')
d.section('2.1 Descrierea jocului')
d.p('Zombie Survivors este un joc 2D de supraviețuire realizat în C# Windows Forms. Scopul unei runde este să rezist cât mai mult, să elimin inamicii și să îmbunătățesc personajul. Dificultatea crește prin numărul inamicilor din valuri și prin introducerea unor tipuri mai rezistente.')
d.section('2.2 Funcționalități principale')
d.p('<b>WASD</b> controlează deplasarea, <b>mouse-ul</b> stabilește ținta, iar <b>clicul stâng</b> încearcă să tragă un proiectil. <b>ESC</b> deschide sau închide pauza, iar <b>F3</b> afișează Debug Menu. Jocul include HP, XP, Level Up, upgrade-uri, bandaje, Settings, Game Over și Respawn.')
d.section('2.3 Tehnologii utilizate')
d.p('Visual Studio oferă editorul de cod, Designer-ul și instrumentele de proiect. C# exprimă regulile jocului, .NET și Windows Forms furnizează fereastra și controalele, iar Git și GitHub păstrează istoricul și repository-ul proiectului.')
d.section('2.4 Captură generală a jocului')
d.fig('14_gameplay','Interfața principală a jocului Zombie Survivors; captură reală dintr-o rundă.',
      crop=(0,24,1920,1032),maxh=260,
      annotations=[(1,951,518,1120,500),(2,914,503,760,460),(3,146,60,180,160),
                   (4,145,1008,380,924),(5,48,968,160,870),(6,960,60,1180,154),(7,1800,82,1590,240)])
d.p('<b>1</b> – jucătorul; <b>2</b> – inamicii; <b>3</b> – bara HP; <b>4</b> – bara XP; <b>5</b> – nivelul; <b>6</b> – valul și inamicii rămași; <b>7</b> – upgrade-urile deținute. Pentru capturarea stabilă a progresiei a fost folosită opțiunea Infinite Health din meniul Debug.',size=10.8,leading=13.5)
d.note('Denumiri păstrate în capturi și cod.', 'Soluția, namespace-urile și titlul ferestrei mai conțin denumiri vechi. Capturile și fragmentele sunt autentice, de aceea le păstrez. Numele utilizat în această lucrare este Zombie Survivors.')

# PAGINA 6 — STRUCTURĂ
d.new('3. Structura proiectului','Fișiere și responsabilități',key='structura')
d.section('3.1 Organizarea fișierelor')
def structure_left():
    d.fig('04_vs_structure','Fișierele actuale din Game și UI, în Solution Explorer.',
          crop=(1544,145,1917,581),maxh=259,annotations=[(1,1662,353,1853,312),(2,1651,521,1853,482)])
def structure_right():
    d.p('<b>Game/</b> conține Entities.cs (Player, statistici, proiectile și efecte), Enemies.cs (inamici și valuri), Combat.cs, GameSession.cs, GameRenderer.cs și Progression.cs.',size=10.6,leading=13.2)
    d.p('<b>UI/</b> conține GameUI.cs (HUD și Level Up), Menus.cs (ecrane și afișare), Input.cs (tastatură și mouse) și DebugMenu.cs.',size=10.6,leading=13.2)
    d.p('<b>Form1.cs</b> conectează aceste clase. <b>Form1.Designer.cs</b> definește controalele. <b>Program.cs</b> pornește aplicația. <b>Properties/Resources.resx</b> și directoarele cu imagini furnizează resursele grafice.',size=10.6,leading=13.2)
d.row(structure_left,structure_right,ratio=.40)
d.section('3.2 Rolul formularului Form1')
d.p('Form1 este fereastra principală și punctul de întâlnire al componentelor. În constructor creez sesiunea, renderer-ul și controlerele pentru HUD, meniuri, intrare și Debug. Formularul primește evenimentele Windows Forms și le transmite obiectului responsabil, fără să păstreze toate regulile de luptă în același fișier.')
d.p('Clasa este <b>partial</b>: Form1.cs și Form1.Designer.cs reprezintă două părți ale aceleiași clase. Această separare permite Designer-ului să gestioneze configurația controalelor, iar codului scris manual să organizeze comportamentul lor.')
d.section('3.3 Legătura dintre componente')
d.flow(['Interfață / intrare','Eveniment C#','GameSession','HUD + randare'])
d.p('Un Click sau o tastă produce un eveniment. Controlerul verifică starea ecranului și apelează logica sesiunii. Ulterior, HUD-ul citește valorile noi, iar Invalidate() solicită redesenarea prin OnPaint. În timpul jocului, Timer-ul repetă ciclul de actualizare.')
d.note('Versiunea analizată.', 'Structura este cea locală din 19 octombrie 2026. Modificările de simplificare aflate în directorul de lucru sunt analizate ca atare, chiar dacă nu apar încă într-un commit.')

# PAGINA 7 — UI / CREAREA PROIECTULUI
d.new('4. Realizarea practică a interfeței','Capitolul principal',key='interfata')
d.section('4.1 Crearea proiectului Windows Forms')
d.p('Pentru a realiza un proiect de acest tip în Visual Studio, deschid <b>Create a new project</b>, aleg șablonul <b>Windows Forms App</b> pentru limbajul C#, stabilesc numele și folderul soluției, apoi aleg versiunea .NET. Proiectul analizat folosește .NET 10 pentru Windows. Pașii descriu modul de creare; capturile din lucrare prezintă proiectul existent. [4]')
d.p('După creare, deschid Form1 în modul <b>Designer</b>. Suprafața formularului îmi permite să așez controale din Toolbox. Pentru configurarea unui control folosesc fereastra Properties. Pentru comportament trec în Form1.cs și scriu metodele asociate evenimentelor.')
d.table(['Fișier', 'Rol la pornire și în interfață'],[
('Program.cs','Main() inițializează configurația aplicației și execută Application.Run(new Form1()).'),
('Form1.Designer.cs','Declară câmpurile controalelor și implementează InitializeComponent(), cu obiecte, proprietăți, ierarhie și evenimente.'),
('Form1.cs','Apelează InitializeComponent(), construiește controlerele și conectează logica jocului la formular.')],[.32,.68])
d.code('Inițializarea formularului',[
    span('Form1.cs','public Form1()',last='DoubleBuffered = true;')],
    'Form1() • începutul constructorului; continuarea este omisă')
d.p('<b>InitializeComponent()</b> trebuie executată înainte ca programul să folosească lblHealth, btnPlay sau celelalte controale. Ea creează obiectele și le adaugă în containerele potrivite. Dacă un eveniment din Designer indică o metodă care nu mai există, proiectul sau Designer-ul pot raporta o eroare.')
d.p('<b>DoubleBuffered</b> activează desenarea intermediară într-un buffer. În acest joc ajută la reducerea pâlpâirii în timpul redesenării arenei. Proprietatea nu înlocuiește actualizarea logicii și nu fixează numărul de cadre pe secundă.')
d.heading('De la formular gol la interfață')
d.p('Am organizat interfața în panouri: meniul principal, setările, pauza, Level Up și Game Over. Pe formular rămân și controalele HUD-ului. Panourile reprezintă ecrane ale aceleiași ferestre; navigarea schimbă vizibilitatea lor, fără să deschidă un formular nou pentru fiecare meniu.')
d.note('Ordinea practică de lucru.', 'Mai întâi stabilesc containerele Panel, apoi adaug textele și butoanele, configurez proprietățile și, la final, conectez evenimentele. Ierarhia părinților este importantă: Location este relativă la containerul controlului.')

# PAGINA 8 — TOOLBOX
d.new('4.2 Utilizarea Toolbox-ului','Construirea interfeței vizuale')
d.sections.append({'title':'4.2 Utilizarea Toolbox-ului','page':8})
d.p('Toolbox este lista de controale și componente disponibile pentru Designer. Îl pot deschide din View → Toolbox sau prin Ctrl+Alt+X. Aleg un element și îl plasez pe formular ori în interiorul unui Panel. Controlul selectat se configurează apoi în Properties. [2, 4]')
def toolbox_left():
    d.fig('02_vs_toolbox','Toolbox deschis în Visual Studio: 1 – Button; 2 – ComboBox; 3 – Label; 4 – ListBox; 5 – PictureBox.',
          crop=(34,67,321,667),maxh=337,
          annotations=[(1,102,201,265,186),(2,111,273,266,260),(3,98,321,265,316),(4,107,369,265,369),(5,117,514,265,513)])
def toolbox_right():
    d.table(['Control', 'Utilizare efectivă'],[
        ('Button','PLAY, SETTINGS, RESUME, RESPAWN și cele trei upgrade-uri.'),
        ('Label','HP, XP, nivel, val, titluri și informații Debug.'),
        ('Panel','Ecrane, grupuri de controale și cele două straturi ale barelor.'),
        ('PictureBox','Logo-ul meniului: Image din resurse, SizeMode = Zoom.'),
        ('CheckBox','Infinite Health și Infinite XP în Debug.'),
        ('ComboBox','Lista închisă a modurilor de afișare.'),
        ('ListBox','Upgrade-urile deținute, în partea dreaptă-sus.'),
        ('Timer','Declanșează periodic evenimentul Tick al jocului.')],[.34,.66],size=9.7)
d.row(toolbox_left,toolbox_right,ratio=.36,gutter=16)
d.heading('De ce am ales aceste controale')
d.p('Button oferă direct evenimentul Click. Label este potrivit pentru informații scurte, iar ListBox poate afișa un număr variabil de upgrade-uri. Panel grupează controalele și permite ascunderea întregului meniu. ComboBox cu DropDownList împiedică introducerea unei valori care nu există în listă.')
d.p('Pentru fiecare control stabilesc un <b>Name</b> clar, apoi Text, Size, Location, culori și font. La butoane configurez și aspectul la trecerea mouse-ului. Pentru controalele HUD aleg Anchor în funcție de marginea ferestrei lângă care trebuie să rămână.')
d.note('Timer-ul din versiunea actuală.', 'Este o componentă Windows Forms, dar gameTimer este construit în Form1.cs, cu Interval = 16 și Tick += GameTimer_Tick. Nu este un control vizibil și nu a fost adăugat în fișierul Designer.')

# PAGINA 9 — DESIGNER / PROPERTIES
d.new('4.3 Form Designer și Properties','Poziționare, aspect și evenimente')
d.sections.append({'title':'4.3 Form Designer și Properties','page':9})
d.p('În Designer selectez un control pentru a-i vedea conturul și proprietățile. Properties modifică configurația obiectului selectat; Document Outline arată ierarhia, utilă când mai multe panouri se suprapun. În captura de mai jos este selectat pnlHealthFill, de tip Panel.')
def properties_left():
    d.fig('03_vs_properties_hp','Properties pentru pnlHealthFill: 1 – obiectul selectat; 2 – BackColor = Red; 3 – categoria Appearance.',
          crop=(1544,613,1917,943),maxh=213,
          annotations=[(1,1650,679,1850,648),(2,1742,796,1851,843),(3,1614,777,1817,742)])
def properties_right():
    d.p('<b>Name</b> este numele folosit în cod, de exemplu pnlHealthFill. <b>Text</b> este conținutul afișat, ca PLAY sau HP 100 / 100. Un Panel folosit ca bară nu are nevoie de text.',size=10.8,leading=13.2)
    d.p('<b>Size</b> înseamnă lățime și înălțime. <b>Location</b> este poziția în interiorul părintelui. pnlHealthFill începe la (0, 0) în panoul de fundal; nu folosește coordonatele întregului ecran.',size=10.8,leading=13.2)
    d.p('<b>BackColor</b> colorează fundalul, <b>ForeColor</b> textul, iar <b>Font</b> stabilește familia, mărimea și stilul literelor.',size=10.8,leading=13.2)
d.row(properties_left,properties_right,ratio=.43,gutter=16)
d.table(['Proprietate', 'Exemplu și efect în Zombie Survivors'],[
('Anchor','Bottom | Left pentru lblLevel, lblXp și pnlXpBackground; păstrează distanța față de stânga și de jos.'),
('Dock','Fill pentru pnlMainMenu și pnlSettings; panoul ocupă suprafața disponibilă a formularului.'),
('Visible','pnlLevelUp pornește ascuns; ScreenController îl afișează la alegerea unui upgrade.'),
('Font / culori','btnPlay folosește Segoe UI 12, Bold, text alb și fundal închis.'),
('Size / Location','picLogo are 350 × 150 și poziția (44, 22) în pnlMenuContent, în configurația Designer.')],[.25,.75],size=10.1)
d.heading('Click este un eveniment')
d.p('<b>Click nu este o proprietate obișnuită</b>, precum BackColor. Este un eveniment. În Properties trec la lista de evenimente, prin pictograma fulger, și asociez o metodă. Legătura reală a butonului PLAY este <b>btnPlay.Click += BtnPlay_Click;</b>. La apăsare, Windows Forms execută metoda respectivă. [5]')
d.note('Observație despre Designer.', 'Controalele ascunse în timpul rulării pot fi totuși vizibile sau suprapuse în Designer. Aspectul de proiectare nu trebuie confundat cu starea curentă a jocului.')

# PAGINA 10 — MENIU
d.new('4.4 Crearea meniului principal','Panouri, logo și butoane')
d.sections.append({'title':'4.4 Crearea meniului principal','page':10})
d.p('Am grupat meniul în <b>pnlMainMenu</b>, cu Dock = Fill. În interior se află <b>pnlMenuContent</b>, care conține picLogo, btnPlay, btnSettings și btnQuit. ScreenController recentrează grupul după dimensiunea ferestrei; pozițiile butoanelor rămân relative la acest grup.')
def menu_left():
    d.fig('19_vs_menu_designer','Meniul în Document Outline al Designer-ului: 1 – container; 2 – PictureBox; 3 – butoane.',
          crop=(35,684,322,804),maxh=174,
          annotations=[(1,156,700,285,702),(2,162,736,286,742),(3,164,772,286,781)])
    d.p('În Designer, logo-ul selectat aparține meniului chiar dacă alt panou îl acoperă pe suprafața de proiectare.',size=10.1,leading=12.5)
def menu_right():
    d.fig('08_game_menu','Meniul la rulare: logo, PLAY, SETTINGS și QUIT.',
          crop=(786,350,1134,710),maxh=204,
          annotations=[(1,961,444,1110,385),(2,960,570,1108,575)])
d.row(menu_left,menu_right,ratio=.51,gutter=18)
d.p('<b>picLogo</b> încarcă Properties.Resources.Logo. Modul Zoom păstrează proporțiile imaginii în spațiul disponibil. Logo-ul este static în versiunea actuală; nu există o animație de meniu care trebuie descrisă.')
d.code('Acțiunea butonului PLAY',[
    span('Form1.cs','private void BtnPlay_Click',count=1),
    span('Form1.cs','private void StartNewGame()',count=5)],
    'BtnPlay_Click() și StartNewGame()')
d.p('Evenimentul Click ajunge în <b>BtnPlay_Click</b>, care apelează StartNewGame(). Mai întâi RestartGame() pregătește sesiunea, apoi gameTimer.Start() pornește actualizările. SETTINGS afișează panoul de setări, iar QUIT execută Application.Exit().')
d.note('Aspect unitar.', 'Butoanele folosesc FlatStyle = Flat, Cursor = Hand, text alb și culori distincte la hover sau apăsare. Aceste proprietăți oferă feedback fără să necesite desenarea manuală a fiecărui buton.')

# PAGINA 11 — HP
d.new('4.5 Crearea barei HP','Două panouri și un indicator numeric')
d.sections.append({'title':'4.5 Crearea barei HP','page':11})
d.p('Pentru bara de viață am folosit două controale Panel. <b>pnlHealthBackground</b> este fundalul întunecat, cu bordură, iar <b>pnlHealthFill</b> este partea roșie din interior. Deasupra se află <b>lblHealth</b>, care afișează viața curentă și limita maximă.')
d.fig('16_game_hp_damage','Bara HP după o lovitură: 1 – textul; 2 – umplerea roșie; 3 – fundalul rămas liber.',
      crop=(0,25,291,104),maxh=108,
      annotations=[(1,65,44,58,90),(2,142,60,161,91),(3,266,60,263,91)])
d.p('În Designer, fundalul are Size = 263 × 18, iar umplerea începe la Location = (0, 0). Culoarea umplerii este Red. Lățimea inițială din Designer este numai o valoare de proiectare: la rulare, metoda UpdateBar() o calculează din viața jucătorului.')
d.code('Actualizarea vieții și a lățimii barei',[
    span('UI/GameUI.cs','public bool UpdateHealth()',count=8),
    span('UI/GameUI.cs','private static void UpdateBar(',count=7)],
    'HudController.UpdateHealth() și UpdateBar()')
d.p('<b>player.Health</b> reprezintă HP curent, iar <b>player.MaxHealth</b> limita dată de statistici. Formula este <b>procent HP = HP curent / HP maxim</b>. Conversia la float permite un rezultat fracționar. Math.Clamp limitează rezultatul între 0 și 1, iar verificarea maximum &gt; 0 evită împărțirea la zero.')
d.p('Procentul înmulțește lățimea interioară a panoului părinte. De exemplu, pentru 90 / 100 HP și o zonă interioară de 261 px, lățimea calculată este 234 px după conversia la int. Se schimbă numai umplerea; label-ul și fundalul își păstrează poziția.')
d.note('Legătura cu Game Over.', 'UpdateHealth() returnează și player.IsDead. GameTimer_Tick folosește rezultatul primit prin RefreshFrame() pentru a afișa panoul Game Over când viața ajunge la zero.')

# PAGINA 12 — XP
d.new('4.6 Bara XP și nivelul','Progres, Anchor și niveluri în așteptare')
d.sections.append({'title':'4.6 Bara XP și nivelul','page':12})
d.p('În partea stângă-jos am plasat <b>lblLevel</b>, <b>lblXp</b> și <b>pnlXpBackground</b>. În panoul de fundal se află pnlXpFill, de culoare Cyan. Cele trei controale exterioare au Anchor = Bottom | Left; astfel își păstrează distanțele față de marginile de jos și din stânga la redimensionare.')
def xp_before():
    d.fig('10_game_start','La pornire: nivel 1; săgeata 1 indică 0 / 20 XP.',
          crop=(0,945,300,1032),maxh=70,annotations=[(1,51,989,232,979)])
def xp_after():
    d.fig('14_gameplay','După +100 XP: 1 – nivelul 4; 2 – bara 16 / 44 XP.',
          crop=(0,945,300,1032),maxh=70,annotations=[(1,48,968,189,960),(2,68,1009,222,1015)])
d.row(xp_before,xp_after,ratio=.5,gutter=15)
d.p('HudController.UpdateExperience() citește Player, actualizează textele și apelează aceeași metodă UpdateBar() ca la HP. Aici fracția este <b>Experience / ExperienceToNextLevel</b>. După eliminarea unui inamic, CombatSystem acordă XP, iar sesiunea înregistrează nivelurile nou câștigate.')
d.code('Calculul progresului în Player',[
    span('Game/Entities.cs','public int AddExperience(int amount)',count=19)],
    'Player.AddExperience()')
d.p('<b>amount</b> este recompensa primită. <b>Experience</b> păstrează XP acumulat, iar <b>ExperienceToNextLevel</b> începe de la 20 și crește cu 8 după fiecare nivel. Bucla while păstrează surplusul: o recompensă mare poate acorda mai multe niveluri, fără să piardă XP rămas.')
d.p('În captura reală, comanda Debug +100 XP a trecut prin pragurile 20, 28 și 36: 100 − 20 − 28 − 36 = 16. De aceea HUD-ul arată LEVEL 4 și 16 / 44 XP. Meniurile de alegere pentru nivelurile 2, 3 și 4 sunt prezentate pe rând, prin coada PendingLevels.')
d.note('După Respawn.', 'Player.Reset() readuce Level la 1, Experience la 0 și pragul la 20. RestartGame() cere imediat actualizarea HUD-ului, pentru ca valorile vechi să nu rămână pe ecran.')

# PAGINA 13 — VALURI UI
d.new('4.7 Interfața pentru valuri','Text din starea sesiunii')
d.sections.append({'title':'4.7 Interfața pentru valuri','page':13})
d.p('Pentru valuri folosesc un singur <b>Label</b>, lblWave, cu două rânduri de text și aliniere centrală. Primul rând indică numărul valului, iar al doilea numărul inamicilor rămași. În pauza dintre valuri, același label anunță finalizarea și timpul până la următorul val.')
d.fig('14_gameplay','Detaliu al lblWave: 1 – valul curent; 2 – totalul inamicilor rămași în val.',
      crop=(839,27,1081,124),maxh=103,
      annotations=[(1,958,49,1058,95),(2,960,70,865,97)])
d.code('Actualizarea informațiilor despre val',[
    span('UI/GameUI.cs','public void UpdateWave()',count=9)],
    'HudController.UpdateWave()')
d.p('<b>session.Waves</b> este managerul valurilor. <b>CurrentWave</b> este numărul valului, iar <b>IsWaveActive</b> selectează textul afișat. Environment.NewLine separă rândurile. Pentru pauză, expresia (milisecunde + 999) / 1000 rotunjește în sus numărul de secunde, astfel încât un rest de timp să nu fie afișat ca zero.')
d.table(['Element', 'Semnificație pentru interfață'],[
('WAVE 1','Valul din care fac parte inamicii și aparițiile încă programate.'),
('ENEMIES: 3','Include inamicii vii și pe cei din val care încă nu au apărut.'),
('WAVE … CLEARED','Valul s-a încheiat și se execută pauza dintre valuri.'),
('NEXT WAVE: …s','Timpul rămas până la începerea următorului val.')],[.31,.69])
d.heading('Poziționarea la redimensionare')
d.p('HudController.Layout() centrează label-ul pe orizontală. Dacă spațiul este prea mic lângă butonul Pause sau lista de upgrade-uri, mută textul mai jos. Astfel, poziția este stabilită după dimensiunea curentă a ferestrei, nu doar după coordonatele inițiale din Designer.')
d.note('Afișaj și logică.', 'Label-ul nu creează inamici și nu avansează valul. El citește datele din WaveManager. Această separare face mai ușoară modificarea textului fără schimbarea regulilor jocului.')

# PAGINA 14 — LEVEL UP DESIGN
d.new('4.8 Interfața Level Up','Construirea panoului și generarea opțiunilor')
d.sections.append({'title':'4.8 Interfața Level Up','page':14})
d.p('Panoul <b>pnlLevelUp</b> conține lblLevelUpTitle și trei Button: btnUpgrade1, btnUpgrade2, btnUpgrade3. Are fundal închis, bordură și Visible = false în configurația inițială. ScreenController îl centrează și îl aduce în față când starea devine LevelUp.')
def level_left():
    d.fig('01_vs_designer','Panoul Level Up în Designer: titlu și trei butoane.',
          crop=(553,263,1053,557),maxh=147,
          annotations=[(1,804,300,1006,298),(2,806,428,1015,442)])
def level_right():
    d.fig('12_game_levelup','Opțiuni generate la rulare pentru nivelul 2.',
          crop=(714,381,1205,673),maxh=147,
          annotations=[(1,949,418,1175,408),(2,956,545,1175,556)])
d.row(level_left,level_right,ratio=.5,gutter=14)
d.code('Completarea textelor celor trei butoane',[
    span('UI/GameUI.cs','int level = session.PendingLevels.Peek();',last='screens.ShowLevelUp();')],
    'LevelUpController.ShowNext() • fragmentul care pregătește opțiunile')
d.p('<b>PendingLevels.Peek()</b> indică nivelul pentru care trebuie făcută alegerea. <b>buttons.Length</b> este 3. GetRandomUpgrades() alege din lista upgrade-urilor normale și scoate opțiunea aleasă din grupul temporar, astfel încât același meniu să nu conțină duplicate.')
d.p('În buclă, fiecare buton devine vizibil și primește numele upgrade-ului, descrierea și numărul de exemplare deținute, afișat prin <b>Owned</b>. Textele inițiale UPGRADE 1–3 sunt doar valori de proiectare; la rulare sunt înlocuite cu informațiile opțiunilor reale.')
d.note('Oprirea temporară a rundei.', 'Cât timp starea este LevelUp, GameTimer_Tick se oprește înaintea actualizării sesiunii. Jucătorul poate citi și alege fără ca inamicii să continue să-l atace.')

# PAGINA 15 — ALEGERE / HUD UPGRADES
d.new('Aplicarea și afișarea upgrade-urilor','Subcapitolul 4.8 • continuare')
d.p('Fiecare buton are propriul eveniment Click. Metodele BtnUpgrade1_Click, BtnUpgrade2_Click și BtnUpgrade3_Click transmit indicii 0, 1 și 2 către ChooseLevelUpOption(). Alegerea ajunge apoi în LevelUpController.Choose(), care validează starea înainte să modifice jucătorul.')
d.code('Aplicarea alegerii și continuarea meniurilor',[
    span('UI/GameUI.cs','public bool Choose(int index, bool infiniteXp)',count=15)],
    'LevelUpController.Choose()')
d.p('<b>index</b> este poziția butonului ales, iar <b>upgradeChoices</b> conține opțiunile curente. ApplyUpgrade() modifică statisticile și numărul de stive; Dequeue() elimină nivelul rezolvat. La final sunt actualizate HP, XP și lista de bonusuri, apoi ShowNext() verifică dacă mai există o alegere în așteptare.')
def owned_left():
    d.fig('13_game_upgrade_owned','Lista din dreapta-sus după două alegeri Speed Boost: x2, total +6%.',
          crop=(1691,26,1910,180),maxh=119,
          annotations=[(1,1793,61,1864,138)])
def owned_right():
    d.p('<b>lstUpgrades</b> este un ListBox, ancorat sus-dreapta. Fiecare tip de upgrade apare pe un singur rând, cu numărul de stive și bonusul total.',size=10.8,leading=13.2)
    d.p('HudController.UpdateUpgrades() citește Version. Dacă versiunea nu s-a schimbat, lista nu este reconstruită inutil la fiecare Tick.',size=10.8,leading=13.2)
d.row(owned_left,owned_right,ratio=.44,gutter=18)
d.heading('Când dispare panoul?')
d.p('Dacă mai există niveluri în coadă, ShowNext() pregătește un nou set de trei opțiuni. Dacă nu mai există, CloseLevelUp() trece la Playing și ascunde panoul. Când jucătorul este mort, aceeași verificare duce la GameOver. În modul Debug, infiniteXp poate acorda încă un nivel după fiecare alegere.')
d.note('Exemplu de acumulare.', 'Speed Boost adaugă de fiecare dată 3% la bonusul de viteză. Două alegeri dau +6%, nu 1,03 × 1,03. La viteza de bază 6, rezultatul este 6 × 1,06 = 6,36 unități pe actualizare.')

# PAGINA 16 — PAUZĂ
d.new('4.9 Pause Menu','Tasta ESC și starea Paused')
d.sections.append({'title':'4.9 Pause Menu','page':16})
d.p('Meniul de pauză este <b>pnlPause</b>, cu lblPaused și butoanele RESUME, SETTINGS și MAIN MENU. Este centrat de ScreenController. Același meniu se poate deschide din butonul PAUSE al HUD-ului sau prin ESC în timpul jocului.')
d.fig('15_game_pause','Meniul Pause: 1 – RESUME; 2 – SETTINGS; 3 – MAIN MENU.',
      crop=(730,375,1190,680),maxh=156,
      annotations=[(1,958,485,1146,466),(2,958,536,1146,535),(3,958,590,1146,605)])
d.code('Schimbarea stării pentru pauză și reluare',[
    span('UI/Menus.cs','public bool Pause()',count=16)],
    'ScreenController.Pause() și Resume()')
d.p('Pause() acceptă cererea doar din Playing, iar Resume() doar din Paused. În Form1, PauseGame() și ResumeGame() șterg tastele de mișcare reținute. Astfel, reluarea nu pornește o deplasare rămasă dinaintea pauzei. La reluare se verifică și existența nivelurilor în așteptare.')
d.p('<b>Pauza nu înseamnă oprirea și recrearea Timer-ului.</b> În GameTimer_Tick, prima verificare returnează imediat dacă starea nu este Playing. Datele rundei rămân în memorie, dar mișcarea, lupta și valurile nu mai sunt actualizate. KeyDown gestionează ESC o singură dată pentru fiecare apăsare, folosind setul de taste apăsate.')
d.note('Celelalte butoane.', 'SETTINGS reține că revenirea trebuie făcută la pauză. MAIN MENU oprește Timer-ul, reinițializează sesiunea și afișează meniul principal. RESUME continuă runda curentă.')

# PAGINA 17 — SETTINGS
d.new('4.10 Settings','ComboBox și moduri de afișare')
d.sections.append({'title':'4.10 Settings','page':17})
d.p('Setările sunt grupate în <b>pnlSettings</b> și pnlSettingsContent. Panoul interior conține titlul, eticheta DISPLAY MODE, <b>cmbDisplayMode</b> și butoanele APPLY și BACK. ComboBox are DropDownStyle = DropDownList, deci utilizatorul poate alege numai una dintre cele trei valori existente.')
d.fig('09_game_settings','Setările reale: 1 – lista celor trei moduri; 2 – butonul BACK.',
      crop=(813,399,1110,628),maxh=193,
      annotations=[(1,942,537,1090,443),(2,959,598,1087,597)])
d.table(['Mod', 'Efect implementat de DisplayManager'],[
('Windowed','Formular cu bordură redimensionabilă; poziția și dimensiunea ferestrei sunt memorate pe durata sesiunii aplicației.'),
('Windowed Maximized','Formular cu bordură redimensionabilă și WindowState = Maximized.'),
('Fullscreen','Formular fără bordură, așezat pe dreptunghiul complet al monitorului curent.')],[.31,.69])
d.heading('Aplicarea unei alegeri')
d.p('Click pe APPLY ajunge în BtnApplySettings_Click(), apoi în display.ApplySelectedMode(). SelectedIndex este convertit la DisplayMode, iar Apply() modifică FormBorderStyle, WindowState și Bounds. Pentru Windowed, dimensiunea și poziția sunt limitate la zona de lucru disponibilă a monitorului.')
d.p('Variabila <b>applying</b> împiedică tratarea schimbărilor intermediare de dimensiune ca modificări făcute manual. La redimensionare, Form1 cere recentrarea panourilor și actualizarea dimensiunii arenei. Nu există în acest ecran setări de volum sau de dificultate.')
d.note('Întoarcerea la ecranul corect.', 'Din meniul principal, BACK revine la MainMenu. Dacă Settings a fost deschis din Pause, BACK revine la Paused. Această alegere este păstrată în SettingsReturnToPause; nu se pierde runda aflată în pauză.')

# PAGINA 18 — GAME OVER
d.new('4.11 Game Over și Respawn','Detectarea morții și resetarea rundei')
d.sections.append({'title':'4.11 Game Over și Respawn','page':18})
d.p('Când HP ajunge la zero, Player.IsDead devine true. Actualizarea HUD-ului raportează moartea, iar Form1 apelează screens.ShowGameOver(). <b>pnlGameOver</b> afișează titlul și butonul RESPAWN. Starea GameOver oprește actualizările de joc prin aceeași verificare de la începutul Tick-ului.')
d.fig('17_game_over','Game Over real: 1 – mesajul; 2 – butonul RESPAWN.',
      crop=(716,445,1204,611),maxh=145,
      annotations=[(1,955,503,1168,476),(2,961,566,1168,584)])
d.code('Reinițializarea sesiunii și a HUD-ului',[
    span('Form1.cs','private void RestartGame()',count=15)],
    'Form1.RestartGame()')
d.p('<b>session.Resize(ClientSize)</b> folosește dimensiunea actuală a ferestrei. Reset() pornește o sesiune curată, iar gameInput.Reset() elimină tastele reținute. SetPlaying() ascunde Game Over și readuce starea de joc. Cele patru apeluri HUD afișează imediat viața, XP, upgrade-urile și valul resetat.')
d.table(['Date resetate', 'Rezultatul unei runde noi'],[
('Jucător și progres','HP 100 / 100, nivel 1, 0 / 20 XP, statistici de bază și fără upgrade-uri.'),
('Lumea rundei','Fără inamici vechi, bandaje, proiectile, texte de luptă sau particule.'),
('Valuri și alegeri','Valul 1, contorul de apariții resetat, PendingLevels golit.')],[.30,.70])
d.note('Evenimentul Respawn.', 'BtnRespawn_Click() apelează RestartGame(). Timer-ul rămâne pornit în GameOver, iar revenirea la Playing permite actualizarea. Fluxul complet este la pagina 21.')

# PAGINA 19 — DEBUG
d.new('4.12 Debug Menu','Instrumente pentru observarea jocului')
d.sections.append({'title':'4.12 Debug Menu','page':19})
d.p('Meniul Debug se deschide prin <b>F3</b> și este un Panel cu etichete, două CheckBox-uri și cinci butoane. DebugController.Toggle() îl ascunde sau îl afișează și îl aduce în față. Nu îl deschide din MainMenu sau Settings.')
def debug_left():
    d.fig('11_game_debug','Meniul Debug real: 1 – CheckBox-uri; 2 – instrumente; 3 – datele sesiunii.',
          crop=(1614,176,1910,494),maxh=226,
          annotations=[(1,1705,225,1877,210),(2,1746,334,1877,261),(3,1693,444,1868,460)])
def debug_right():
    d.p('<b>Infinite Health</b> previne damage-ul de contact și reface viața dacă personajul este încă viu.',size=10.7,leading=13.1)
    d.p('<b>Infinite XP</b> acordă încă un nivel după fiecare alegere de upgrade. Nu este o valoare numerică infinită în proprietatea XP.',size=10.7,leading=13.1)
    d.p('<b>+100 XP / Level Up</b> acordă experiență sau exact cât lipsește pentru pragul următor.',size=10.7,leading=13.1)
    d.p('<b>Next Wave</b> curăță inamicii și avansează valul. <b>Kill Enemies (No Rewards)</b> încheie valul fără recompense. <b>Heal</b> vindecă un personaj viu.',size=10.7,leading=13.1)
d.row(debug_left,debug_right,ratio=.44,gutter=18)
d.code('Acordarea de XP prin interfața Debug',[
    span('UI/DebugMenu.cs','public void AddExperience(int amount)',count=11)],
    'DebugController.AddExperience()')
d.p('Metoda ignoră valorile nepozitive și jucătorul mort. După session.AddExperience(amount), actualizează bara XP și deschide Level Up dacă există alegeri și jocul este Playing. Apoi reîmprospătează informațiile Debug și solicită redesenarea prin callback-ul repaint().')
d.note('Utilitate practică.', 'Acest meniu permite verificarea rapidă a nivelurilor, stivelor și valurilor. Capturile progresiei din lucrare au fost obținute prin aceste controale existente, fără schimbarea surselor sau simularea unor ecrane.')

# PAGINA 20 — UI → COD: PLAY
d.new('4.13 Legătura dintre UI și cod','Exemplul 1: butonul PLAY')
d.sections.append({'title':'4.13 Legătura dintre UI și cod','page':20})
d.p('În Windows Forms, un buton nu funcționează doar pentru că apare pe formular. El trebuie să existe ca obiect, să fie adăugat într-un container și să aibă evenimentul conectat. Pentru PLAY pot urmări toate aceste etape în fișierele reale ale proiectului.')
d.heading('Declarare, creare și proprietăți')
d.p('În Form1.Designer.cs, câmpul este declarat <b>private System.Windows.Forms.Button btnPlay;</b>. În InitializeComponent() este creat prin <b>btnPlay = new Button();</b> și adăugat în pnlMenuContent. Aceste instrucțiuni aparțin aceleiași clase partial Form1 folosite și de codul manual.')
d.code('Proprietățile butonului și legarea evenimentului',[
    span('Form1.Designer.cs','btnPlay.BackColor =',last='btnPlay.Click += BtnPlay_Click;')],
    'InitializeComponent() • configurația reală a btnPlay')
d.p('<b>Name</b> identifică obiectul în cod, <b>Text</b> stabilește inscripția PLAY, iar Size și Location definesc dreptunghiul din container. Font și ForeColor controlează textul. Proprietățile FlatAppearance stabilesc bordura și feedback-ul la hover sau apăsare.')
d.flow(['Apăsare PLAY','Click','BtnPlay_Click()','StartNewGame()'])
d.p('Operatorul <b>+=</b> abonează metoda BtnPlay_Click la eveniment. Parametrul sender reprezintă obiectul care a produs evenimentul, iar EventArgs e transportă datele generale ale acestuia. Metoda din Form1.cs transmite acțiunea la StartNewGame(), prezentată în Codul 2.')
d.note('Rezultatul în aplicație.', 'RestartGame() resetează datele, SetPlaying() ascunde meniul și face HUD-ul vizibil, apoi Start() permite actualizările Timer-ului. Un singur Click se transformă astfel într-o rundă nouă, pregătită complet.')

# PAGINA 21 — UI → COD: RESPAWN
d.new('De la RESPAWN la o rundă nouă','Subcapitolul 4.13 • exemplul 2')
d.p('Al doilea exemplu este <b>btnRespawn</b>. Câmpul de tip Button este declarat în Form1.Designer.cs, creat în InitializeComponent() și adăugat în pnlGameOver. Text este RESPAWN, iar Name este btnRespawn. Linia <b>btnRespawn.Click += BtnRespawn_Click;</b> conectează butonul la codul manual.')
d.flow(['RESPAWN','Click','BtnRespawn_Click()','RestartGame()'])
d.fig('18_game_respawn','Runda imediat după RESPAWN: 1 – HP refăcut; 2 – XP și nivel resetate; 3 – valul 1; 4 – lista de upgrade-uri goală.',
      crop=(0,24,1920,1032),maxh=222,
      annotations=[(1,140,60,205,169),(2,73,988,368,897),(3,961,61,1173,161),(4,1800,100,1580,232)])
d.heading('Ce face fiecare nivel al aplicației?')
d.table(['Nivel', 'Responsabilitate în acest exemplu'],[
('Designer','Păstrează obiectul Button, proprietățile, părintele și asocierea evenimentului.'),
('Form1.cs','Primește Click prin BtnRespawn_Click și coordonează RestartGame().'),
('GameSession','Golește obiectele rundei, resetează jucătorul, valurile și upgrade-urile.'),
('ScreenController / HUD','Afișează ecranul de joc și valorile inițiale, apoi solicită redesenarea.')],[.32,.68])
d.p('Separarea este utilă când modific interfața: pot schimba inscripția sau poziția butonului în Designer fără să rescriu resetarea sesiunii. Invers, pot completa regulile de resetare în GameSession fără să mut butonul sau să modific resursa grafică a meniului.')
d.note('Asemănarea cu o alegere de upgrade.', 'Și btnUpgrade1 are o asociere Click în Designer. Metoda sa trimite indicele 0, iar LevelUpController validează alegerea și modifică statisticile. Controlul vizual doar inițiază acțiunea; regulile rămân în clasele corespunzătoare.')

# PAGINA 22 — INTEGRARE UI
d.new('Integrarea interfeței cu jocul','Subcapitolul 4.13 • actualizare și stări')
d.p('HudController primește referințe la controalele create în Designer și la GameSession. Astfel, citește aceleași date ca logica jocului și le prezintă prin Text sau Width. Captura editorului arată metodele reale UpdateHealth(), UpdateExperience() și UpdateBar().')
d.fig('05_vs_code','Codul HUD-ului în Visual Studio: 1 – viața; 2 – XP și nivelul; 3 – formula comună a barelor.',
      crop=(45,231,850,819),maxh=272,
      annotations=[(1,367,300,786,290),(2,394,603,785,574),(3,423,755,786,719)])
d.table(['Stare', 'Efect principal asupra interfeței și rundei'],[
('MainMenu / Settings','Panoul corespunzător este vizibil; HUD-ul este ascuns.'),
('Playing','Timer-ul actualizează sesiunea; HUD-ul citește valorile noi.'),
('LevelUp / Paused','Panoul este vizibil; Tick returnează înaintea logicii jocului.'),
('GameOver','Panoul Game Over este afișat; runda așteaptă RESPAWN.')],[.33,.67])
d.p('La fiecare Tick acceptat, sesiunea primește mouse-ul în coordonate client și direcția tastelor. RefreshFrame() actualizează HUD-ul. Dacă există niveluri în așteptare, se deschide Level Up; dacă jucătorul a murit, se afișează Game Over. Invalidate() solicită un nou OnPaint, fără să deseneze direct în evenimentul butonului.')
d.note('Observații din capturarea aplicației.', 'Au fost parcurse efectiv PLAY, SETTINGS, F3, +100 XP, alegerea upgrade-urilor, ESC, Game Over și RESPAWN. Capturile confirmă stările prezentate; nu înlocuiesc o suită automată de teste sau o măsurare de performanță.')

# PAGINA 23 — DEPLASARE / SHOOTING
d.new('5. Explicarea codului jocului','Deplasare și luptă',key='codjoc')
d.section('5.1 Mișcarea jucătorului')
d.p('GameInputController păstrează tastele apăsate într-un HashSet&lt;Keys&gt;. D și A dau direcția orizontală, iar S și W direcția verticală. La eliberare, tasta este scoasă din set. Poziția se actualizează în Tick după starea tastelor.')
d.code('Direcția tastelor WASD',[
    span('UI/Input.cs','public PointF GetMovementDirection()',count=10)],
    'GameInputController.GetMovementDirection()')
d.p('Înmulțirea cu <b>0.7071f</b> normalizează diagonala. Player.Move() înmulțește direcția cu Stats.MoveSpeed și o adaugă la X și Y. ClampToArena() limitează poziția la zona client; viteza inițială este 6 unități pe actualizare, înaintea upgrade-urilor.')
d.section('5.2 Sistemul de shooting')
d.p('Mouse-ul este convertit prin PointToClient(Cursor.Position), astfel încât țintirea și arena folosesc aceleași coordonate. Player.UpdateAngle() folosește Atan2. CombatSystem.Shoot() calculează gura țevii, direcția către cursor și o normalizează. Dacă mouse-ul este prea aproape, se păstrează sensul armei pentru a evita tragerea inversă.')
d.code('Lovitura de aproape sau crearea proiectilului',[
    span('Game/Combat.cs','// Check the body-to-muzzle path',last='player.RegisterShot();')],
    'CombatSystem.Shoot() • partea finală a metodei')
d.p('Glonțul are viteza inițială 20 și verifică întregul segment parcurs pentru prima coliziune. Damage-ul este 10–14, critica are inițial 2,5% și multiplicator 1,5 sau 2. Cooldown-ul este 210 ms. Fiecare clic stâng încearcă un foc, fără autofoc.')

# PAGINA 24 — INAMICI / VALURI / XP
d.new('Inamici, valuri și experiență','Capitolul 5 • continuare')
d.section('5.3 Inamicii')
d.p('Cele trei imagini sunt asociate cu EnemyType. Tipurile urmăresc direct jucătorul; diferențele provin din statistici, dimensiune și reacția la lovitură. Tabelul arată valorile de bază, înainte de scalarea vieții și a recompenselor.')
d.table(['Resursă / tip', 'HP', 'Viteză', 'Contact', 'XP de bază'],[
('Zombie_1 / ZombieFast','30','6','10','12–18'),
('Zombie_2 / ZombieHeavy','80','3,6','20','22–32'),
('Vampire_1 / Vampire','110','6,6','30','40–55')],[.38,.11,.15,.16,.20],size=10)
d.code('Urmărirea jucătorului',[
    span('Game/Enemies.cs','PointF center = enemy.GetCenter();',count=14,occurrence=0)],
    'EnemyManager.Update() • calculul deplasării unui inamic')
d.p('<b>dx</b> și <b>dy</b> sunt diferențele către jucător, iar distance este lungimea direcției. Viteza scade temporar la stagger. După deplasare sunt limitate pozițiile și separate corpurile suprapuse. Viața la apariție crește cu 4% pentru fiecare nivel al jucătorului după nivelul 1.')
d.section('5.4 Sistemul de valuri')
d.p('WaveManager pornește de la valul 1. Numărul necesar este <b>3 + (CurrentWave − 1) × 2</b>, cu o apariție programată la 650 ms. După toate aparițiile și eliminarea inamicilor vii, urmează 2.000 ms de pauză. ZombieHeavy apare de la valul 3, iar Vampire de la valul 10; alegerea ține cont și de indicele apariției.')
d.section('5.5 XP și Level Up')
d.p('La eliminare, recompensa este acordată imediat. Enemy.RollExperienceReward() alege XP de bază și aplică +10% pentru fiecare val după primul, cu multiplicator maxim 4. Player.AddExperience(), din Codul 4, păstrează surplusul și mărește pragul cu 8. GameSession pune fiecare nivel câștigat în PendingLevels; interfața cere apoi câte o alegere pentru fiecare. Numai textul plutitor XP este întârziat cu 250 ms, nu recompensa.')

# PAGINA 25 — UPGRADES / HP / GRAPHICS
d.new('Statistici, vindecare și randare','Capitolul 5 • continuare')
d.section('5.6 Upgrade-urile')
d.p('UpgradeSystem păstrează câte un contor pentru cele șapte tipuri. Fiecare alegere adaugă același bonus. Valorile procentuale sunt aditive; limita efectivă se aplică în PlayerStats, iar lista HUD prezintă totalul stivelor.')
d.table(['Upgrade normal', 'Bonus pentru fiecare alegere'],[
('Health Boost','+10 HP maxim și vindecare de până la 10 HP'),
('Speed Boost / Damage Boost','+3% viteză de mișcare / +5% damage'),
('Durability / Critical Training','+3 puncte procentuale rezistență / +1 punct procentual șansă critică'),
('Bullet Speed / Rapid Fire','+5% viteză proiectile / +4% rată de foc')],[.43,.57],size=10)
d.p('Rezistența are plafonul 60%, iar șansa critică totală 35%. Rapid Fire calculează cooldown-ul prin 210 / (1 + bonusul ratei de foc), fără să scadă direct procentul din timp.',size=11,leading=13.7,after=4)
d.section('5.7 Sistemul HP și bandajele')
d.p('Player.TakeDamage() aplică rezistența, păstrează minimum 1 damage și oferă 750 ms de invulnerabilitate după o lovitură. La eliminarea unui inamic, condiția reală este <b>Random.Shared.NextDouble() &lt; 0.10</b>. Bandajul vindecă până la 20 HP, rămâne pe teren la viață plină și are un hitbox de 38 × 38 pentru desenul de 28 × 28. Vindecarea produce text verde; resetarea golește bandajele.')
d.section('5.8 Randarea grafică')
d.p('OnPaint() transmite Graphics la GameRenderer.Draw(), care desenează terenul, obiectele și personajele. Sprite-urile provin din Resources; bandajul este desenat prin forme. Jucătorul se rotește cu player.Angle în jurul pivotului sprite-ului.')
d.code('Rotirea și desenarea unui inamic',[
    span('Game/GameRenderer.cs','PointF center = enemy.GetCenter();',last='g.Restore(state);')],
    'GameRenderer.DrawEnemies() • fragmentul de randare')
d.p('Save() și Restore() izolează transformările. Originea se mută în centrul inamicului, se aplică rotația, apoi DrawSprite desenează imaginea. Renderer-ul reutilizează bitmap-urile din resurse și eliberează obiectele proprii. Invalidate() solicită un nou OnPaint.',size=11,leading=13.7,after=4)

# PAGINA 26 — REFLECȚIE / GIT
d.new('6. Reflecție personală','Organizare și lucru cu proiectul',key='reflectie')
d.sections.append({'title':'6. Reflecție personală','page':26})
d.p('Prin acest proiect am înțeles mai bine diferența dintre a construi o interfață și a programa comportamentul ei. Toolbox și Designer mă ajută să așez controalele, dar rezultatul devine util numai după ce proprietățile și evenimentele sunt conectate corect. Un exemplu simplu este PLAY: aspectul este în Designer, iar pornirea rundei ajunge în metodele din Form1.')
d.p('Am învățat să folosesc Panel pentru meniuri, Label pentru informații și Button pentru acțiuni. La HP și XP, partea vizuală m-a ajutat să urmăresc valorile din clase. Anchor și recentrarea panourilor arată de ce o interfață nu trebuie gândită doar pentru o singură dimensiune de fereastră.')
d.p('Una dintre provocări a fost organizarea codului și păstrarea unei interfețe ușor de modificat. Istoricul include refactorizări, iar structura actuală separă sesiunea, lupta, progresia și controlerele UI. Simplificarea funcțiilor și meniul Debug fac mai ușoară urmărirea unei probleme: pot verifica separat valorile, evenimentul și rezultatul afișat.')
d.heading('7. Git și GitHub',size=16,leading=19,after=7)
d.bookmark('git','7. Git și GitHub')
d.sections.append({'title':'7. Git și GitHub','page':26})
d.section('7.1 Ce sunt Git și GitHub?')
d.p('Git urmărește local versiunile fișierelor prin commit-uri. GitHub găzduiește repository-uri și permite accesul la ele prin rețea. În acest proiect, origin indică repository-ul Zombie-Survivors al contului rimikkusu. Git și GitHub au roluri diferite, dar sunt folosite împreună. [6, 7]')
d.section('7.2 Utilizarea Git în proiect')
d.p('Un flux obișnuit este <b>git status</b> pentru verificare, <b>git add .</b> pentru pregătire, <b>git commit -m &quot;mesaj&quot;</b> pentru salvarea versiunii și <b>git push</b> pentru trimitere. <b>git pull</b> preia și integrează schimbările, iar <b>git log</b> arată istoricul. Aceste comenzi sunt explicate, fără să creez sau să trimit un commit pentru documentație.')
d.section('7.3 Repository-ul proiectului')
d.fig('06_vs_git','Istoric real în Visual Studio, ramura main: 1 – identificatori; 2 – mesaje de commit.',
      crop=(8,124,649,274),maxh=104,
      annotations=[(1,199,237,131,252),(2,533,238,612,184)])
d.p('Commit-uri observate: <b>5bb8566</b> – „Last commit”, 19.10.2026; <b>fdc653f</b> – „Fixed some bugs”, 06.10.2026; <b>66cc688</b> – „Refactored”, 27.09.2026. Versiunea locală documentată are și modificări necomise; nu presupun că toate se află deja pe GitHub.',size=10.3,leading=12.7)

# PAGINA 27 — README / CONCLUZIE
d.new('8. README și alte comenzi Git','Prezentarea și păstrarea proiectului',key='readme')
d.section('8.1 Ce este README?')
d.p('README este fișierul de prezentare al unui repository: descrie scopul, cerințele de rulare, instalarea și comenzile utilizatorului. În directorul de lucru analizat, <b>README.md este șters local</b>, deci nu prezint un conținut inexistent drept documentație actuală. Pentru o versiune viitoare ar fi utile: descrierea jocului, .NET 10 și Windows, deschiderea soluției, comenzile WASD/mouse/ESC/F3 și structura Game/UI.')
d.section('8.2 Comenzi Git utile')
d.table(['Comandă', 'Explicație'],[
('git init','Inițializează un repository în folderul curent.'),
('git clone &lt;adresă&gt;','Copiază un repository existent, împreună cu istoricul său.'),
('git status','Afișează fișierele modificate, pregătite sau neurmărite.'),
('git add .','Pregătește modificările din directorul curent, inclusiv ștergeri.'),
('git commit -m &quot;mesaj&quot;','Creează un commit din modificările pregătite.'),
('git push','Trimite commit-urile către repository-ul remote configurat.'),
('git pull','Preia și integrează schimbările din ramura remote urmărită.'),
('git log / git diff','Arată istoricul / diferențele dintre versiuni sau față de fișierele locale.')],[.38,.62],size=10.1)
d.gap(5)
d.heading('9. Concluzie',size=17,leading=21,after=9)
d.bookmark('concluzie','9. Concluzie')
d.sections.append({'title':'9. Concluzie','page':27})
d.p('Realizarea jocului Zombie Survivors m-a ajutat să leg noțiunile de la Programarea vizuală de o aplicație pe care o pot folosi și explica. Am construit meniuri din controale Windows Forms, am configurat proprietăți și am asociat evenimentele cu metode C#. Bara HP, progresul XP și alegerea upgrade-urilor arată direct cum datele din clase ajung în interfață.')
d.p('Am înțeles că o interfață clară depinde și de organizarea codului. Separarea dintre Designer, formular și clasele auxiliare îmi permite să urmăresc traseul unei acțiuni: apăsarea unui buton, verificarea stării, modificarea sesiunii și actualizarea ecranului. Această organizare este utilă atât când adaug o funcție, cât și când rezolv o eroare.')
d.p('Consider că proiectul își îndeplinește rolul educațional: include o rundă de joc, progresie și meniuri funcționale într-o aplicație desktop C#. Pentru continuare, aș îmbunătăți documentația repository-ului și aș urmări mai atent comportamentul la diferite dimensiuni de fereastră. Cunoștințele exersate aici pot fi aplicate și la alte aplicații cu formulare, controale și evenimente.')

# PAGINA 28 — BIBLIOGRAFIE
d.new('10. Bibliografie','Surse oficiale și materialul analizat',key='bibliografie')
d.sections.append({'title':'10. Bibliografie','page':28})
d.p('Sursele online de mai jos au fost consultate la <b>19 octombrie 2026</b>. Explicațiile despre implementarea jocului se bazează în primul rând pe fișierele locale analizate și pe capturile realizate în aplicație. Referințele [1]–[7] susțin noțiunile despre platformă și instrumente.')
refs=[
('1','Microsoft Learn. Windows Forms overview.','https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/','Prezentarea Windows Forms, a controalelor și a desenării personalizate.'),
('2','Microsoft Learn. Toolbox.','https://learn.microsoft.com/en-us/visualstudio/ide/toolbox?view=visualstudio','Rolul Toolbox-ului în Visual Studio și folosirea controalelor în Designer.'),
('3','Microsoft Learn. C# documentation.','https://learn.microsoft.com/en-us/dotnet/csharp/','Noțiuni despre limbaj, clase, metode, proprietăți și evenimente.'),
('4','Microsoft Learn. Create a Windows Forms app in Visual Studio.','https://learn.microsoft.com/en-us/dotnet/desktop/winforms/get-started/create-app-visual-studio','Crearea unei aplicații moderne .NET și utilizarea Designer, Toolbox și Properties.'),
('5','Microsoft Learn. Events in Windows Forms controls.','https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls/events','Evenimentele controalelor și asocierea metodelor de tratare.'),
('6','Pro Git. Getting Started – About Version Control.','https://git-scm.com/book/en/v2/Getting-Started-About-Version-Control','Principiile controlului versiunilor și rolul Git.'),
('7','GitHub Docs. About repositories.','https://docs.github.com/en/repositories/creating-and-managing-repositories/about-repositories','Rolul unui repository găzduit pe GitHub.'),
('8','Bragari Eduard. Zombie Survivors – proiectul analizat.','https://github.com/rimikkusu/Zombie-Survivors','Surse locale: Form1.cs, Form1.Designer.cs, Program.cs, Game/*.cs, UI/*.cs și Resources.resx. Versiunea de lucru: 19.10.2026; branch main, HEAD 5bb8566, cu modificări locale necomise.'),
]
for n,title,url,desc in refs:
    d.p(f'<b>[{n}] {escape(title)}</b><br/><link href="{escape(url)}" color="#315A52">{escape(url)}</link><br/>{escape(desc)}',
        size=10.3,leading=12.8,after=11)
d.heading('Material de referință pentru prezentare',size=12.4,leading=15,after=4)
d.p('PAD_Studiu_Nr1.docx.pdf, 18 pagini, furnizat ca model de structură și aspect. A fost analizat înainte de redactare. Conținutul despre alt proiect nu a fost preluat; fundalul, bordura și exemplele acestei lucrări au fost realizate pentru Zombie Survivors.',size=10.3,leading=12.8,after=8)
d.note('Despre imagini și cod.', 'Toate capturile sunt reale. Au fost decupate pentru lizibilitate și adnotate cu săgeți și numere. Fragmentele de cod sunt extrase din surse, cu fișier și linii; omisiunile sunt marcate. Documentarea nu modifică jocul.')

assert d.page == 28, d.page
assert len(d.excerpts) == 15, len(d.excerpts)
assert len({f['source'] for f in d.figures}) == 18
d.finish()
