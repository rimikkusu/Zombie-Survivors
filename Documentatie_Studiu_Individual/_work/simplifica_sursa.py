"""Migrare de aspect: păstrează textul și extragerile de cod ale studiului."""
from pathlib import Path
import re

base = Path(__file__).resolve().parents[1]
path = base / 'sursa' / 'genereaza_studiul.py'
old = (base / '_work' / 'inainte_de_simplificare' / path.name).read_text(encoding='utf-8')
helpers = old[old.index('def source('):old.index('class Study:')]
body = old[old.index('d=Study()'):]
toc_start = body.index('toc=[')
toc_end = body.index('# PAGINA 3')
body = body[:toc_start] + '\n' + body[toc_end:]
body = re.sub(r"^d\.sections\.append\(.*\)\n", '', body, flags=re.M)
body = body.replace('assert d.page == 28, d.page\n', '')
body = body.replace('Fluxul complet este la pagina 21.',
                    'Fluxul complet este explicat în subcapitolul 4.13.')

# Legenda generală era sub imagine. O mutăm înaintea acesteia și eliminăm
# afirmația despre Infinite Health, care nu se aplică noii capturi generale.
legend = "d.p('<b>1</b> – jucătorul; <b>2</b> – inamicii; <b>3</b> – bara HP; <b>4</b> – bara XP; <b>5</b> – nivelul; <b>6</b> – valul și inamicii rămași; <b>7</b> – upgrade-urile deținute. Pentru capturarea stabilă a progresiei a fost folosită opțiunea Infinite Health din meniul Debug.',size=10.8,leading=13.5)"
assert legend in body
body = body.replace(legend + '\n', '')
anchor = "d.fig('14_gameplay','Interfața principală a jocului Zombie Survivors; captură reală dintr-o rundă.',"
body = body.replace(anchor, "d.p('Săgețile indică: 1 - jucătorul; 2 - inamicii; 3 - bara HP; 4 - bara XP; 5 - nivelul; 6 - valul și inamicii rămași; 7 - lista de upgrade-uri.')\n" + anchor)

# Explicațiile meniului și istoricului precedă acum capturile.
menu_sentence = "    d.p('În Designer, logo-ul selectat aparține meniului chiar dacă alt panou îl acoperă pe suprafața de proiectare.',size=10.1,leading=12.5)\n"
assert menu_sentence in body
body = body.replace(menu_sentence, '')
body = body.replace("def menu_left():\n", "def menu_left():\n" + menu_sentence)
git_sentence = "d.p('Commit-uri observate: <b>5bb8566</b> – „Last commit”, 19.10.2026; <b>fdc653f</b> – „Fixed some bugs”, 06.10.2026; <b>66cc688</b> – „Refactored”, 27.09.2026. Versiunea locală documentată are și modificări necomise; nu presupun că toate se află deja pe GitHub.',size=10.3,leading=12.7)"
assert git_sentence in body
body = body.replace(git_sentence + '\n', '')
body = body.replace("d.fig('06_vs_git'", git_sentence + "\n\nd.fig('06_vs_git'", 1)

# Referințele rămân aceleași; eliminăm numai descrierile lungi și data accesării.
intro_start = body.index("d.p('Sursele online de mai jos")
intro_end = body.index('refs=[', intro_start)
body = body[:intro_start] + body[intro_end:]
loop_start = body.index('for n,title,url,desc in refs:')
loop_end = body.index('assert len(d.excerpts)', loop_start)
body = body[:loop_start] + '''for n,title,url,desc in refs:
    d.p(f'<b>{n}. {escape(title)}</b><br/><link href="{escape(url)}" color="#000000">{escape(url)}</link>')

d.p('Material de referință pentru structură: PAD_Studiu_Nr1.docx.pdf, furnizat pentru realizarea studiului.')

''' + body[loop_end:]
header = '''from pathlib import Path
from xml.sax.saxutils import escape
from layout_simplu import Study

ROOT = Path(__file__).resolve().parents[2]


'''
path.write_text(header + helpers + body, encoding='utf-8')
print(path)
