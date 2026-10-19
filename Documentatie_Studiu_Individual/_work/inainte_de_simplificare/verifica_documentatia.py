from pathlib import Path
import json, hashlib, re, math
from PIL import Image, ImageOps, ImageDraw, ImageFont
from pypdf import PdfReader
import pdfplumber

OUT = Path(__file__).resolve().parents[1]
ROOT = OUT.parent
manifest = json.loads((OUT/'sursa'/'manifest_documentatie.json').read_text(encoding='utf-8'))
pdf = PdfReader(str(OUT/manifest['pdf']))
problems = []
texts = [p.extract_text() for p in pdf.pages]
if len(pdf.pages) != 28:
    problems.append('Număr de pagini diferit de 28')
cover_required = ['Bragari Eduard','P-2432','Diana Nebunealia','Programarea vizuală',
                  'Studiul Individual Nr. 1','Zombie Survivors','Chișinău, 2026']
for value in cover_required:
    if value not in texts[0]:
        problems.append('Lipsește de pe copertă: '+value)
for i,p in enumerate(pdf.pages,1):
    if len(texts[i-1].strip()) < 300:
        problems.append(f'Pagină fără conținut suficient: {i}')
    if abs(float(p.mediabox.width)-595.276) > .1 or abs(float(p.mediabox.height)-841.89) > .1:
        problems.append(f'Format greșit: {i}')
    if '\ufffd' in texts[i-1] or '\u25a0' in texts[i-1]:
        problems.append(f'Caracter suspect: {i}')

# Sunt verificate sursele originale, nu numai textul copiat în PDF.
for x in manifest['code_excerpts']:
    if not 5 <= sum(len(p['lines']) for p in x['parts']) <= 20:
        problems.append(f'Lungime fragment: {x["number"]}')
    for part in x['parts']:
        lines = (ROOT/'Vampire Survivors'/part['file']).read_text(encoding='utf-8-sig').splitlines()
        actual = '\n'.join(lines[part['start']-1:part['end']])
        if actual != part['original']:
            problems.append(f'Cod modificat sau inventat: {x["number"]}')
    if f'Codul {x["number"]}.' not in texts[x['page']-1]:
        problems.append(f'Fragment pe pagină incorectă: {x["number"]}')
for f in manifest['figures']:
    if not (OUT/'assets'/'capturi_originale'/f['source']).is_file():
        problems.append('Captură lipsă: '+f['source'])
    if f'Figura {f["number"]}.' not in texts[f['page']-1]:
        problems.append(f'Figură pe pagină incorectă: {f["number"]}')

before = json.loads((OUT/'_work'/'source_hashes_before.json').read_text(encoding='utf-8-sig'))
changed = []
for item in before:
    path = ROOT/'Vampire Survivors'/item['Path']
    if not path.is_file() or hashlib.sha256(path.read_bytes()).hexdigest().upper() != item['SHA256'].upper():
        changed.append(item['Path'])
if changed:
    problems.append('Fișiere ale jocului schimbate: '+', '.join(changed))

# Fiecare secțiune este verificată și pe pagina sa, iar cuprinsul este pe pagina 2.
normalize = lambda s: re.sub(r'\s+',' ',s).strip()
for section in manifest['sections']:
    title = normalize(section['title'])
    if title not in normalize(texts[section['page']-1]):
        problems.append('Secțiune pe pagină greșită: '+title)

# Coordonatele finale ale caracterelor, interpretate independent de ReportLab.
out_of_bounds = []
with pdfplumber.open(str(OUT/manifest['pdf'])) as pdf_positions:
    for page_number,p in enumerate(pdf_positions.pages,1):
        for ch in p.chars:
            if not ch['text'].strip():
                continue
            if ch['x0'] < 25 or ch['x1'] > 570 or ch['top'] < 30 or ch['bottom'] > 810:
                out_of_bounds.append({'page':page_number,'x':round(ch['x0'],2),
                                      'top':round(ch['top'],2),'text':ch['text']})
if out_of_bounds:
    problems.append('Text în afara bordurii: '+str(out_of_bounds[:5]))

# Contacte de câte patru pagini, păstrate pentru verificarea vizuală.
pages_dir = OUT/'_work'/'pagini'
pages = sorted(pages_dir.glob('pagina-*.png'))
contact_paths=[]
if pages:
    font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',19)
    thumbw,thumbh=570,808
    for j in range(0,len(pages),4):
        sheet=Image.new('RGB',(1180,1692),'#D9DED7')
        draw=ImageDraw.Draw(sheet)
        for k,path in enumerate(pages[j:j+4]):
            im=Image.open(path).convert('RGB')
            im.thumbnail((thumbw,thumbh),Image.Resampling.LANCZOS)
            x=15+(k%2)*590
            y=30+(k//2)*846
            sheet.paste(im,(x,y))
            draw.text((x,y-24),f'Pagina {j+k+1}',fill='#20352D',font=font)
        result=OUT/'_work'/f'contact-{j//4+1:02d}.png'
        sheet.save(result)
        contact_paths.append(str(result))

report={'pages':len(pdf.pages),'figures':len(manifest['figures']),
        'authentic_screenshots':manifest['unique_screenshot_count'],
        'code_excerpts':len(manifest['code_excerpts']),
        'game_files_hashed':len(before),'game_files_changed':changed,
        'text_out_of_bounds':out_of_bounds,'problems':problems,
        'contact_sheets':contact_paths,
        'visual_review':'Se completează după examinarea imaginilor randate.',
        'word_count':sum(len(t.split()) for t in texts)}
(OUT/'_work'/'verificare_tehnica.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(report,ensure_ascii=False,indent=2))
if problems:
    raise SystemExit(1)
