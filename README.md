# Zombie Survivors

Zombie Survivors este un joc 2D de supraviețuire realizat în **C# folosind Windows Forms** în Visual Studio. Proiectul a fost creat pentru disciplina **Programarea vizuală**.

Jucătorul trebuie să supraviețuiască mai multor valuri de zombi, să câștige experiență și să aleagă upgrade-uri pentru a deveni mai puternic.

## Despre joc

Jocul se desfășoară într-o arenă 2D, unde inamicii apar în valuri și încearcă să atace jucătorul.

Există trei tipuri de inamici:

- **Zombie 1** – mic și rapid, dar cu puțină viață.
- **Zombie 2** – mai mare, mai rezistent, dar mai lent.
- **Vampire** – rapid și puternic, apare în valurile mai avansate.

După eliminarea inamicilor, jucătorul primește XP. La fiecare nivel nou poate alege unul dintre trei upgrade-uri aleatorii, precum mai multă viață, viteză sau damage.

Inamicii pot lăsa și bandaje care refac viața jucătorului.

## Funcționalități

- Mișcare și țintire cu mouse-ul.
- Sistem de shooting și damage.
- Trei tipuri de inamici.
- Valuri cu dificultate progresivă.
- Sistem de HP și XP.
- Upgrade-uri care pot fi acumulate.
- Bandaje pentru recuperarea vieții.
- Meniu principal și setări.
- Pause Menu.
- Game Over și Respawn.
- Meniu Debug pentru testarea jocului.

## Controale

| Tastă | Acțiune |
|---|---|
| W A S D | Deplasare |
| Mouse | Țintire |
| Click stânga | Tragere |
| ESC | Pauză / Continuare |
| F3 | Meniu Debug |

## Tehnologii utilizate

- C#
- .NET 10
- Windows Forms
- Visual Studio
- Git și GitHub

Interfața jocului a fost realizată folosind controale din **Windows Forms Designer**, precum Button, Panel, Label și PictureBox.

Pentru desenarea jucătorului, inamicilor și proiectilelor am folosit `System.Drawing.Graphics`.

## Cum se pornește proiectul

1. Descarcă proiectul de pe GitHub.
2. Deschide fișierul `.sln` sau `.csproj` în Visual Studio.
3. Verifică să ai instalat .NET 10 și suportul pentru Windows Forms.
4. Apasă **Start (F5)** pentru a rula jocul.

## Scopul proiectului

Scopul proiectului a fost să învăț să lucrez cu Windows Forms, să creez interfețe grafice folosind Toolbox-ul și să folosesc evenimente pentru a controla diferite funcționalități.

De asemenea, am exersat lucrul cu clase, liste, timere, coliziuni și randarea imaginilor în C#.

## Autor

**Bragari Eduard**

Grupa P-2432

CEITI – Centrul de Excelență în Informatică și Tehnologii Informaționale

Disciplina: Programarea vizuală
