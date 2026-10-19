"""Verificări ale documentației; nu compilează și nu modifică proiectul C#."""
from pathlib import Path
from collections import Counter
import hashlib
import json
import re

from PIL import Image, ImageDraw, ImageFont
from pypdf import PdfReader
import pdfplumber
from layout_simplu import simple_text

OUT = Path(__file__).resolve().parents[1]
ROOT = OUT.parent
WORK = OUT / '_work' / 'simplificat'
WORK.mkdir(exist_ok=True)
manifest = json.loads((OUT / 'sursa' / 'manifest_documentatie.json').read_text(encoding='utf-8'))
pdf = PdfReader(str(OUT / manifest['pdf']))
texts = [p.extract_text() or '' for p in pdf.pages]
problems = []
normalize = lambda value: re.sub(r'\s+', ' ', value).strip()

if len(pdf.pages) != manifest['pages']:
    problems.append('Numărul paginilor nu corespunde manifestului.')
for value in ['Bragari Eduard', 'P-2432', 'Diana Nebunelea', 'Programarea vizuală',
              'Studiul Individual Nr. 1', 'Zombie Survivors', 'Chișinău, 2026',
              'Catedra de Informatică I']:
    if simple_text(value) not in texts[0]:
        problems.append('Lipsește de pe copertă: ' + value)

if not manifest.get('cover_logo') or len(pdf.pages[0].images) != 1:
    problems.append('Logo-ul lipsește de pe copertă.')
if 'PAD_Studiu_Nr1.docx.pdf' in '\n'.join(texts):
    problems.append('Ultimul paragraf din bibliografie nu a fost eliminat.')

for number, page in enumerate(pdf.pages, 1):
    if len(texts[number - 1].strip()) < 50 and not page.images:
        problems.append(f'Pagină goală: {number}')
    if abs(float(page.mediabox.width) - 595.276) > .1 or abs(float(page.mediabox.height) - 841.89) > .1:
        problems.append(f'Format greșit: {number}')
    if '\ufffd' in texts[number - 1] or '\u25a0' in texts[number - 1]:
        problems.append(f'Caracter suspect: {number}')
    if re.search(r'Figura\s+\d+\.', texts[number - 1]):
        problems.append(f'Descriere veche de figură: {number}')
    if 'Observații din capturarea' in texts[number - 1]:
        problems.append(f'Observație decorativă rămasă: {number}')
    if re.search('[ăâîșțĂÂÎȘȚşţŞŢ]', texts[number - 1]):
        problems.append(f'Diacritice rămase în document: {number}')
    if re.search(r'\[\d+(?:\s*,\s*\d+)*\]', texts[number - 1]):
        problems.append(f'Referință numerică între paranteze pătrate: {number}')
    if '(CEITI)' in texts[number - 1] or 'Nebunealia' in texts[number - 1]:
        problems.append(f'Date vechi rămase în document: {number}')

if manifest['body_leading'] != 18:
    problems.append('Spațierea textului nu este de 1,5 rânduri.')

old = json.loads((OUT / '_work' / 'inainte_de_simplificare' / 'manifest_documentatie.json').read_text(encoding='utf-8'))
if manifest['screenshots'] != old['screenshots']:
    problems.append('S-a pierdut o captură distinctă din studiul existent.')
if manifest['code_excerpt_count'] != 15:
    problems.append('Nu sunt păstrate cele 15 fragmente de cod.')
for excerpt, previous in zip(manifest['code_excerpts'], old['code_excerpts']):
    if excerpt['parts'] != previous['parts']:
        problems.append(f'Fragment diferit de studiul existent: {excerpt["number"]}')
    for part in excerpt['parts']:
        lines = (ROOT / 'Vampire Survivors' / part['file']).read_text(encoding='utf-8-sig').splitlines()
        if '\n'.join(lines[part['start'] - 1:part['end']]) != part['original']:
            problems.append(f'Cod care nu corespunde sursei: {excerpt["number"]}')
    if f'Codul {excerpt["number"]}.' not in texts[excerpt['page'] - 1]:
        problems.append(f'Etichetă de cod pe altă pagină: {excerpt["number"]}')
for figure in manifest['figures']:
    if not (OUT / figure['source_path']).is_file():
        problems.append('Captură lipsă: ' + figure['source_path'])
    if not figure['cursor_checked']:
        problems.append(f'Captură neverificată: {figure["number"]}')

before = json.loads((OUT / '_work' / 'source_hashes_before.json').read_text(encoding='utf-8-sig'))
changed = []
for item in before:
    path = ROOT / 'Vampire Survivors' / item['Path']
    if not path.is_file() or hashlib.sha256(path.read_bytes()).hexdigest().upper() != item['SHA256'].upper():
        changed.append(item['Path'])
if changed:
    problems.append('Fișiere ale jocului schimbate: ' + ', '.join(changed))

toc_last = min(section['page'] for section in manifest['sections']) - 1
with pdfplumber.open(str(OUT / manifest['pdf'])) as positioned_toc:
    toc_text = normalize(' '.join(page.extract_text() or ''
                                  for page in positioned_toc.pages[1:toc_last]))
for section in manifest['sections']:
    title = normalize(section['title'])
    if title not in normalize(texts[section['page'] - 1]):
        problems.append('Titlu pe pagină greșită: ' + title)
    pattern = re.escape(title) + r'\s*(?:\.\s*)+\s*' + str(section['page']) + r'\b'
    if not re.search(pattern, toc_text):
        problems.append('Intrare în cuprins neconfirmată: ' + title)

bounds = []
font_sizes = Counter()
small = []
with pdfplumber.open(str(OUT / manifest['pdf'])) as positioned:
    for number, page in enumerate(positioned.pages, 1):
        for char in page.chars:
            if not char['text'].strip():
                continue
            font_sizes[round(char['size'], 2)] += 1
            if char['x0'] < 25 or char['x1'] > 570 or char['top'] < 30 or char['bottom'] > 811:
                bounds.append({'page': number, 'x': round(char['x0'], 2),
                               'top': round(char['top'], 2), 'text': char['text']})
            minimum = 10.5 if 'Consolas' in char['fontname'] else 12
            if char['size'] < minimum - .05:
                small.append({'page': number, 'size': char['size'], 'text': char['text']})
if bounds:
    problems.append('Text în afara bordurii: ' + str(bounds[:5]))
if small:
    problems.append('Text prea mic: ' + str(small[:5]))

contact_paths = []
pages = sorted((WORK / 'pagini').glob('pagina-*.png'))
font = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 19)
for start in range(0, len(pages), 4):
    sheet = Image.new('RGB', (1180, 1692), '#E3E3E3')
    draw = ImageDraw.Draw(sheet)
    for offset, path in enumerate(pages[start:start + 4]):
        image = Image.open(path).convert('RGB')
        image.thumbnail((570, 808), Image.Resampling.LANCZOS)
        x, y = 15 + offset % 2 * 590, 30 + offset // 2 * 846
        sheet.paste(image, (x, y))
        draw.text((x, y - 24), f'Pagina {start + offset + 1}', fill='black', font=font)
    path = WORK / f'contact-{start // 4 + 1:02d}.png'
    sheet.save(path)
    contact_paths.append(str(path))

# Galeria folosește exact decupajele incluse în PDF.
crops = sorted((OUT / 'assets' / 'detalii_simple').glob('*.png'))
crop_contacts = []
for start in range(0, len(crops), 4):
    sheet = Image.new('RGB', (1400, 1500), '#E3E3E3')
    draw = ImageDraw.Draw(sheet)
    for offset, path in enumerate(crops[start:start + 4]):
        image = Image.open(path).convert('RGB')
        image.thumbnail((670, 700), Image.Resampling.LANCZOS)
        x, y = 15 + offset % 2 * 700, 35 + offset // 2 * 750
        sheet.paste(image, (x, y))
        draw.text((x, y - 26), path.stem, fill='black', font=font)
    path = WORK / f'capturi-{start // 4 + 1:02d}.png'
    sheet.save(path)
    crop_contacts.append(str(path))

report = {
    'pages': len(pdf.pages), 'figures': len(manifest['figures']),
    'authentic_screenshots': manifest['unique_screenshot_count'],
    'code_excerpts': len(manifest['code_excerpts']), 'game_files_hashed': len(before),
    'game_files_changed': changed, 'font_sizes': dict(sorted(font_sizes.items())),
    'text_out_of_bounds': bounds, 'small_text': small, 'problems': problems,
    'contact_sheets': contact_paths, 'screenshot_sheets': crop_contacts,
    'visual_review': 'Se completează după examinarea tuturor paginilor și capturilor.',
    'word_count': sum(len(text.split()) for text in texts),
}
(WORK / 'verificare_tehnica.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(report, ensure_ascii=False, indent=2))
if problems:
    raise SystemExit(1)
