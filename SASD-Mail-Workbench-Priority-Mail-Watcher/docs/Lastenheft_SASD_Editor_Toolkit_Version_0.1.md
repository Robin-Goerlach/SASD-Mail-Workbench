# Lastenheft
## SASD Editor Toolkit - moderne Neuinterpretation der Borland Turbo Editor Toolbox

| Merkmal | Wert |
|---|---|
| **Dokumenttyp** | Lastenheft / fachliche und qualitative Produktanforderungen |
| **Projekt** | SASD Editor Toolkit |
| **Dokumentversion** | 0.1 |
| **Status** | Arbeitsstand zur Prüfung und Freigabe |
| **Datum** | 24.07.2026 |
| **Sprache** | Deutsch |
| **Vorgesehene Projektsprache** | Englisch für Code und öffentliche API; Deutsch und Englisch für Dokumentation |
| **Erste Referenzoberfläche** | WinForms |
| **Spätere Oberflächen** | WPF sowie moderne ASP.NET-/Weboberflächen, bevorzugt Blazor oder Razor |
| **Repository** | Eigenständiges Repository |
| **Abgrenzung** | Eigenständig gegenüber SASD Desktop Components, SASD Data Toolbox, SASD Numerics, SASD Graphics und SASD GameWorks |

> **Dokumentcharakter:** Dieses Lastenheft beschreibt in erster Linie, **was** das Produkt leisten und welche Qualitätsmerkmale es erfüllen soll. Konkrete Klassen, Projektdateien, Algorithmen, UI-Techniken und Implementierungsdetails gehören in das nachfolgende Pflichtenheft und Architekturdokument. Bereits verbindlich besprochene Strukturentscheidungen - insbesondere der UI-unabhängige Kern und die eigenständige Repository-Führung - werden dennoch als Produktanforderungen festgehalten.

---

## 1. Zweck des Dokuments

Dieses Lastenheft definiert die fachlichen, funktionalen, qualitativen und organisatorischen Anforderungen an eine moderne C#/.NET-Neuinterpretation der **Turbo Editor Toolbox Version 1.0** von 1985.

Die historische Toolbox war nicht lediglich ein Textfeld oder ein fertiger Editor. Sie war ein modularer Baukasten, mit dem einfache und komplexe Editoren sowie Textbearbeitungsfunktionen innerhalb anderer Programme erstellt werden konnten. Das Handbuch behandelt unter anderem:

- Zeichen, Wörter, Zeilen, Textströme, Fenster, Blöcke und Dateien,
- unterschiedliche Textpufferstrukturen,
- Cursor-, Lösch-, Textverarbeitungs-, Block-, Fenster- und Dateibefehle,
- ein konfigurierbares Command- und Tastatursystem,
- mehrere Fenster und gemeinsam verwendete Textströme,
- Suche, Ersetzen, Marker, Word-Wrap, Auto-Indent, Ränder und Undo,
- Status-, Fehler-, Menü- und Popup-Anpassungen,
- kooperative Hintergrundaufgaben,
- die Einbettung des Editors in andere Programme,
- eine technische Referenz mit Konstanten, Datentypen, globalen Variablen sowie Prozeduren und Funktionen.

Das Lastenheft überführt diese Fähigkeiten in ein modernes Produktziel. Historische DOS-, Turbo-Pascal-, Speicher-, Overlay- und Videospeichertechniken werden nicht wörtlich übernommen. Ihre fachliche Absicht wird auf heutige .NET-, Desktop- und Webkonzepte übertragen.

---

## 2. Quellenbasis und Methodik

### 2.1 Primärquellen

Die Analyse basiert auf den beiden bereitgestellten PDF-Dateien:

1. `Turbo_Editor_Toolbox_Version_1.0_1985(1).pdf`
2. `Borland_Turbo_Editor_Toolbox_Version_1.0_1985(2).pdf`

Beide Dateien enthalten nach Sichtung dasselbe 264-seitige Handbuch **Turbo Editor Toolbox Version 1.0 - Owner's Handbook**, Copyright 1985 Borland International Inc. Die doppelte Ausführung wurde als Kontrollquelle betrachtet; sie stellt keine zweite, abweichende Spezifikation dar.

### 2.2 Ergänzende Projektgrundlage

Berücksichtigt wurden außerdem die bereits getroffenen Entscheidungen:

- Neuimplementierung in C#/.NET,
- WinForms als erste Referenzoberfläche,
- UI-unabhängiger Editor-Kern,
- spätere WPF- und Webintegration ohne Neuimplementierung des Kerns,
- bevorzugte Webintegration über Blazor oder Razor,
- eigenständiges Repository `SASD-Editor-Toolkit`,
- Nutzung in SASD Desktop Components nur über Abhängigkeit/Referenz,
- klare Abgrenzung zu Data Toolbox, Numerics, Graphics und GameWorks,
- keine unkritische 1:1-Übernahme historischer technischer Grenzen,
- ausführliche Dokumentation, gut kommentierter Code und automatisierte Tests.

### 2.3 Herkunftskennzeichnung

- **Original:** direkt aus dem Handbuch.
- **Modernisierung:** heutige Übertragung einer historischen Fähigkeit.
- **Projektentscheidung:** im Gespräch verbindlich festgelegtes Ziel.
- **SASD Development Standard:** organisatorische oder qualitative Vorgabe.
- **Abgeleitet:** notwendige Folgerung für eine zuverlässige moderne Umsetzung.

### 2.4 Vollständigkeitsgrenze

Die Werbeseiten am Ende des Handbuchs zu Turbo Graphix Toolbox, Turbo Database Toolbox und Turbo GameWorks sind keine funktionale Spezifikation der Editor Toolbox. Sie werden nicht als Editoranforderungen übernommen. Die weitere SASD-Produktfamilie wird lediglich organisatorisch abgegrenzt.

---

## 3. Ausgangssituation

Die historische Turbo Editor Toolbox löste ein weiterhin relevantes Problem: Anwendungen benötigen häufig Textbearbeitung, möchten aber weder einen vollständigen Editor neu entwickeln noch dauerhaft an eine konkrete Editoranwendung gekoppelt sein.

Für heutige SASD-Projekte bestehen insbesondere folgende Bedarfe:

- wiederverwendbares Editor-Control für WinForms,
- fachlicher Kern für spätere WPF- und Webadapter,
- einheitliches Command-System für Menüs, Tastatur, Toolbar und API,
- verlässliches Dokument-, Datei-, Undo/Redo- und Suchverhalten,
- mehrere Ansichten eines oder mehrerer Dokumente,
- Erweiterbarkeit für Markdown-, Log-, Konfigurations-, Quelltext- oder Spezialeditoren,
- Integration in Desktop Components und weitere SASD-Anwendungen,
- Lern- und Dokumentationswert durch die Modernisierung eines historischen Softwarebaukastens.

Ein schneller Wrapper um `RichTextBox` könnte einen kurzfristigen Prototyp liefern, würde bei unkontrollierter Kopplung an WinForms aber die spätere Wiederverwendung erschweren. Das Produkt muss deshalb von Anfang an zwischen fachlichem Editorzustand und plattformspezifischer Darstellung unterscheiden.

---

## 4. Produktvision

Das **SASD Editor Toolkit** soll ein offener, nachvollziehbarer und gut dokumentierter .NET-Baukasten für Textbearbeitung werden.

Es soll:

1. die wesentlichen fachlichen Fähigkeiten der Turbo Editor Toolbox bewahren,
2. historische technische Einschränkungen konsequent modernisieren,
3. als Bibliothek in andere Anwendungen eingebettet werden können,
4. mit WinForms beginnen, aber nicht auf WinForms beschränkt sein,
5. einfache und anspruchsvollere Editoren aus denselben Kernbausteinen ermöglichen,
6. als Grundlage für weitere SASD Desktop Components dienen,
7. eine belastbare API, Tests, Dokumentation und Beispielanwendungen besitzen,
8. schrittweise entwickelt werden, ohne im ersten Meilenstein unnötiges Framework-Over-Engineering zu betreiben.

---

## 5. Ziele

### 5.1 Hauptziele

- UI-unabhängiger, testbarer Editor-Kern,
- produktiv nutzbare WinForms-Komponente,
- moderne Abdeckung der historischen Editor-Toolbox-Kategorien,
- Unicode-, Datei-, Undo/Redo- und Mehransichtsfähigkeit,
- frei konfigurierbare Befehle und Tastaturprofile,
- einfache Einbettung in SASD- und Drittanwendungen,
- belastbare Grundlage für WPF und Web,
- eigenständige Produkt- und Releasefähigkeit.

### 5.2 Qualitätsziele

- hohe Verständlichkeit und Wartbarkeit,
- sichere Behandlung ungespeicherter Daten,
- reaktionsfähige Bedienung,
- automatisierte Tests,
- klare API- und Architekturgrenzen,
- lokalisierbare UI und Meldungen,
- keine versteckte Bindung an historische Hardware- oder Speicherannahmen.

### 5.3 Lern- und Dokumentationsziele

- Nachvollziehbarkeit, welche historische Funktion wie modernisiert wurde,
- dokumentierte Gründe für bewusste Nichtübernahmen,
- Beispielanwendungen nach dem Vorbild von FIRST-ED und MicroStar,
- Verwendung als Referenzprojekt für den SASD Development Standard.

---

## 6. Nichtziele

Für den ersten Produktzyklus sind keine zwingenden Ziele:

- vollständige Textverarbeitung mit Seitenlayout, Tabellen und Bildern,
- WYSIWYG-Dokumentformat wie Microsoft Word,
- kollaboratives Echtzeit-Editing,
- vollständige IDE mit Compiler und Debugger,
- fertiges Plugin-Ökosystem im ersten Meilenstein,
- vollständige Syntaxhervorhebung im ersten Meilenstein,
- 1:1-Kompatibilität mit Turbo-Pascal-Quellcode,
- direkte Nutzung historischer `.ED`, `.MS`, `.OVL` oder `.INC`-Module,
- DOS-Overlays, Chain-Files, BIOS-Aufrufe oder Videospeicherzugriff,
- historische 80x25-Oberfläche als primäre UI,
- Pflichtabhängigkeit von anderen SASD Toolboxes,
- First-Class-ASPX/Web-Forms-Control im ersten Ausbauschritt.

Ein optionaler Retro-Demomodus darf historische Bedienkonzepte zeigen, aber nicht die moderne Architektur bestimmen.

---

## 7. Stakeholder

| Stakeholder | Interesse |
|---|---|
| **SASD-GmbH / Product Owner** | Wiederverwendbares, langfristig wartbares Editorprodukt |
| **SASD-Entwickler** | Verständliche API, Kommentare, Tests und einfache Erweiterbarkeit |
| **WinForms-Anwendungsentwickler** | Schnell integrierbares Control mit Datei-, Such-, Undo- und Mehransichtsfunktionen |
| **Spätere WPF-/Webentwickler** | Wiederverwendung des identischen Kerns |
| **Endbenutzer** | Verlässliche, schnelle und verständliche Textbearbeitung |
| **Maintainer** | Klare Modulgrenzen, versionierte APIs und geringe technische Schulden |
| **Open-Source-Nutzer** | Nachvollziehbare Lizenzierung, Beispiele und stabile Releases |
| **SASD Desktop Components** | Nutzung und Demonstration ohne Besitz des Kerns |

---

## 8. Produktabgrenzung und Repository-Strategie

### 8.1 Eigenständiges Produkt

Das SASD Editor Toolkit wird als eigenständiges Repository geführt und besitzt eigene:

- Versionierung,
- Releases,
- Dokumentation,
- Tests,
- Roadmap,
- Paketierung,
- Beispielanwendungen.

### 8.2 Verhältnis zu SASD Desktop Components

`SASD Desktop Components` darf:

- den WinForms-Adapter referenzieren,
- eine Showcase-Seite oder Beispielanwendung enthalten,
- spezialisierte Komponenten wie Markdown-Editor oder Log-Viewer darauf aufbauen.

Es darf nicht:

- den Editor-Kern duplizieren,
- Kernklassen in das Komponentenprojekt verschieben,
- eine zyklische Abhängigkeit erzeugen.

### 8.3 Verhältnis zu weiteren Toolboxes

- **SASD Data Toolbox:** eigenständiges Daten-, Index- und Speicherprodukt; keine Pflichtabhängigkeit.
- **SASD Numerics:** eigenständige mathematische Bibliothek beziehungsweise Teil eines Building-Blocks-Labs.
- **SASD Graphics:** eigenständige Grafikbibliothek beziehungsweise Adapterfamilie.
- **SASD GameWorks:** möglicher Nutzer von Numerics und Graphics; darf Editorfunktionen konsumieren.
- **SASD Building Blocks:** mögliche Produktfamilie, aber keine monolithische DLL.

---

## 9. Systemkontext

```text
+-------------------------------------------------------------+
| Hostanwendungen                                             |
| SASD Desktop Components, Mail Workbench, Tools, Samples ... |
+--------------------------+----------------------------------+
                           |
                           v
+-------------------------------------------------------------+
| UI-Adapter                                                   |
| WinForms | später WPF | später Blazor/Razor | Hostadapter    |
+--------------------------+----------------------------------+
                           |
                           v
+-------------------------------------------------------------+
| Editor-Anwendungsfunktionen                                 |
| Commands, Sessions, Suche, Dateiabläufe, Einstellungen       |
+--------------------------+----------------------------------+
                           |
                           v
+-------------------------------------------------------------+
| UI-unabhängiger Editor-Kern                                  |
| Dokument, Textpuffer, Positionen, Auswahl, Undo/Redo, Marker |
+--------------------------+----------------------------------+
                           |
                           v
+-------------------------------------------------------------+
| Austauschbare Plattform- und Infrastrukturdienste            |
| Dateien/Streams, Clipboard, Logging, Dialoge, Druck, Autosave|
+-------------------------------------------------------------+
```

Die endgültige Anzahl von Assemblies und Schnittstellen wird im Pflichtenheft festgelegt. Eine unnötige Aufspaltung in viele Kleinstprojekte ist zu vermeiden.

---

## 10. Begriffe

| Begriff | Bedeutung im Projekt |
|---|---|
| **Editor** | System aus Kern, Befehlen und Oberfläche zur Anzeige und Veränderung von Text |
| **Dokument** | Fachlicher Textinhalt einschließlich Dateiinformationen und Dirty State |
| **Textpuffer** | Interne, austauschbare Struktur zur Speicherung und Änderung von Text |
| **Textstrom** | Historischer Begriff für eine zusammenhängende Folge von Zeilen |
| **Ansicht / View** | Darstellung eines Dokuments mit eigenem Cursor-, Auswahl- und Scrollzustand |
| **Fenster** | Historischer Begriff; modern meist Ansicht, Pane oder Editor-Control |
| **Cursor / Caret** | Aktuelle Einfüge- beziehungsweise Bearbeitungsposition |
| **Auswahl** | Markierter Textbereich; umfasst die historische Blockidee |
| **Block** | Historisch überwiegend zusammenhängende Folge von Zeilen |
| **Marker** | Benannte oder nummerierte, nachgeführte Position im Dokument |
| **Command** | Eindeutig identifizierbare Benutzer- oder API-Aktion |
| **Command Dispatcher** | Ordnet Eingaben beziehungsweise IDs ausführbaren Befehlen zu |
| **Command Processor** | Historischer Begriff für die Befehlsimplementierung |
| **Dirty State** | Aktueller Inhalt weicht vom letzten gespeicherten Zustand ab |
| **Word-Wrap** | Umbruchdarstellung oder historische Absatzumbruchfunktion |
| **Auto-Indent** | Übernahme der Einrückung bei einer neuen Zeile |
| **Adapter** | Plattformspezifische Umsetzung von Darstellung, Eingabe oder Diensten |
| **Host** | Anwendung, in die der Editor eingebettet wird |

---

## 11. Produktanforderungen

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| PROD-001 | Das Produkt muss als wiederverwendbare Editor-Toolbox und nicht nur als fertige Einzelanwendung bereitgestellt werden. | MUSS | Handbuch Einleitung und Kap. 14. |
| PROD-002 | Die Toolbox muss einfache Editoren, komplexe Editoren und in Fachanwendungen eingebettete Textbearbeitung ermöglichen. | MUSS | Handbuch Einleitung; FIRST-ED und MicroStar. |
| PROD-003 | Der fachliche Editor-Kern muss unabhängig von WinForms, WPF, ASP.NET und Browsertechniken sein. | MUSS | Verbindliche Projektentscheidung. |
| PROD-004 | WinForms ist die erste produktiv nutzbare Referenzoberfläche. | MUSS | Verbindliche Projektentscheidung. |
| PROD-005 | WPF muss später denselben Kern ohne fachliche Neuimplementierung verwenden können. | MUSS | Verbindliche Projektentscheidung. |
| PROD-006 | Der Kern muss aus ASP.NET-basierten Hosts nutzbar sein; eine moderne interaktive Webintegration soll bevorzugt über Blazor oder Razor erfolgen. | SOLL | Projektentscheidung; ASPX/Web Forms nur Kompatibilitätsweg. |
| PROD-007 | Das SASD Editor Toolkit wird als eigenständiges Repository, Paket und Releaseobjekt geführt. | MUSS | Verbindliche Projektentscheidung. |
| PROD-008 | SASD Desktop Components darf den Editor verwenden und demonstrieren, aber nicht Eigentümer des Editor-Kerns sein. | MUSS | Verbindliche Projektentscheidung. |
| PROD-009 | Die Editor-Toolbox muss unabhängig von Data Toolbox, Numerics, Graphics und GameWorks versionierbar bleiben. | MUSS | Produktfamilienentscheidung. |
| PROD-010 | Historische Fähigkeiten werden funktional modernisiert; DOS-, Turbo-Pascal-, Overlay- und Videospeichertechnik wird nicht 1:1 portiert. | MUSS | Handbuch plus Modernisierungsentscheidung. |
| PROD-011 | Code, öffentliche API und Paketnamen sollen Englisch sein; Dokumentation soll Deutsch und Englisch verfügbar sein. | SOLL | SASD-Projektstandard. |
| PROD-012 | Bibliotheken, UI-Komponenten und lauffähige Beispiele müssen gemeinsam bereitgestellt werden. | MUSS | Handbuchbeispiele plus moderne Produktanforderung. |

---

## 12. Benutzergruppen und zentrale Anwendungsfälle

### 12.1 Benutzergruppen

1. **Endbenutzer eines eingebetteten Editors** - bearbeiten Konfigurationen, Notizen, Quelltext, Logs, Vorlagen oder andere Textdaten.
2. **Anwendungsentwickler** - integrieren die Toolbox, registrieren eigene Befehle und stellen Hostdienste bereit.
3. **Komponentenentwickler** - entwickeln WinForms-, WPF- oder Webadapter sowie spezialisierte Komponenten.
4. **Maintainer und Tester** - prüfen API-Stabilität, Performance, Datenintegrität und historische Abdeckung.

### 12.2 Use Cases


| ID | Use Case | Kurzbeschreibung |
|---|---|---|
| UC-01 | Neues Dokument | Leeres Dokument erstellen, Text eingeben und speichern. |
| UC-02 | Datei bearbeiten | Datei öffnen, verändern und sicher speichern. |
| UC-03 | Editor einbetten | WinForms-Control in eine Fachanwendung integrieren und Hostdienste bereitstellen. |
| UC-04 | Mehrere Dateien vergleichen | Mehrere Dokumentansichten gleichzeitig verwenden. |
| UC-05 | Dasselbe Dokument teilen | Zwei Ansichten zeigen verschiedene Stellen desselben Dokuments. |
| UC-06 | Block bearbeiten | Auswahl kopieren, verschieben oder löschen. |
| UC-07 | Suchen und Ersetzen | Treffer finden, navigieren und einzeln oder gesammelt ersetzen. |
| UC-08 | Undo/Redo | Mehrere Änderungen rückgängig machen und wiederholen. |
| UC-09 | Historisches Tastaturprofil | Turbo-/WordStar-Präfixbefehle aktivieren. |
| UC-10 | Eigenen Befehl ergänzen | Host registriert anwendungsspezifischen Command für Menü, Toolbar und Taste. |
| UC-11 | Ungespeicherte Änderungen schützen | Schließen oder Öffnen erzwingt Speichern/Verwerfen/Abbrechen. |
| UC-12 | Große Datei | Große Datei reaktionsfähig öffnen und Vorgang abbrechen. |
| UC-13 | Autosave | Host speichert Wiederherstellungsstände im Hintergrund. |
| UC-14 | Weiteren UI-Adapter bauen | WPF- oder Webadapter verwendet denselben Core. |
| UC-15 | Desktop-Components-Showcase | Komponentenprojekt demonstriert Editor, Markdown- oder Log-Viewer. |

---

## 13. Funktionale Anforderungen

### 13.1 Dokument-, Text- und Puffermodell

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| CORE-001 | Text muss als Zeichen, Wörter, Zeilen, Textströme, Ansichten, Blöcke/Auswahlen und Dateien modellierbar sein. | MUSS | Einleitung und Kap. 1. |
| CORE-002 | Dokumentinhalt und Ansichtszustand müssen getrennt sein. | MUSS | Modernisierung von Textstrom und Fensterdeskriptor. |
| CORE-003 | Mehrere Ansichten müssen dasselbe Dokument gleichzeitig anzeigen und bearbeiten können. | MUSS | Window Linking, Kap. 9 und 12. |
| CORE-004 | Ein Dokument muss Text, optionalen Pfad, Anzeigenamen, Codierung, Zeilenenden, Dirty State, Marker und Undo/Redo verwalten können. | MUSS | Original plus Modernisierung. |
| CORE-005 | Eine Ansicht muss Cursor, Auswahl, Scrollposition, Insert/Overwrite, Word-Wrap, Auto-Indent, Ränder und Tabulatorverhalten verwalten. | MUSS | Windesc, Kap. 9. |
| CORE-006 | Der Textpuffer muss hinter einer austauschbaren Abstraktion gekapselt sein. | MUSS | Kap. 3: Array-of-Lines, Fixed Buffer, Linked List. |
| CORE-007 | Die erste Pufferimplementierung darf zeilenorientiert sein, muss aber spätere Large-File-Implementierungen erlauben. | SOLL | Projektentscheidung. |
| CORE-008 | Zeilen dürfen nicht auf 255 Zeichen begrenzt sein. | MUSS | Modernisierung von Maxlinelength. |
| CORE-009 | Die interne Textrepräsentation muss Unicode unterstützen. | MUSS | Modernisierung der ASCII-Grundlage. |
| CORE-010 | CRLF, LF und weitere unterstützte Zeilenenden müssen erkannt, erhalten und konvertiert werden können. | MUSS | Modernisierung der historischen Dateiverarbeitung. |
| CORE-011 | Leere und noch nicht benannte Dokumente müssen unterstützt werden. | MUSS | Nofile/NONAME. |
| CORE-012 | Textänderungen müssen als atomare, nachvollziehbare Operationen repräsentierbar sein. | MUSS | Undo/Redo, Tests und Erweiterbarkeit. |
| CORE-013 | Der Core muss ohne UI-Thread und ohne UI-Framework testbar sein. | MUSS | Projektentscheidung. |
| CORE-014 | Dokumentmodus und Nichtdokumentmodus sollen als konfigurierbare Bearbeitungsoption abbildbar sein. | SOLL | Kap. 1. |
| CORE-015 | Steuerzeichen müssen speicherbar und optional sichtbar darstellbar sein. | SOLL | Kap. 1 und Insert Control Character. |
| CORE-016 | Zeilen und Bereiche müssen erweiterbare Annotationen für Wrap, Auswahl, Markierung und Spezialdarstellung tragen können. | SOLL | Inblock, Wrapped, Colored. |
| CORE-017 | Textpositionen und Bereiche müssen eindeutig, validierbar und bei Änderungen deterministisch behandelbar sein. | MUSS | Abgeleitet aus Cursor, Block, Marker und Suche. |

---

### 13.2 Cursor, Navigation und Scrollen

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| NAV-001 | Cursor ein Zeichen nach links oder rechts bewegen. | MUSS | EditLeftChar/EditRightChar. |
| NAV-002 | Cursor eine Zeile nach oben oder unten bewegen. | MUSS | EditUpLine/EditDownLine. |
| NAV-003 | Ansicht eine Zeile nach oben oder unten scrollen. | MUSS | EditScrollUp/EditScrollDown. |
| NAV-004 | Seitenweise nach oben oder unten navigieren. | MUSS | EditUpPage/EditDownPage. |
| NAV-005 | Wortweise nach links oder rechts navigieren. | MUSS | EditLeftWord/EditRightWord. |
| NAV-006 | Zum Zeilenanfang und Zeilenende springen; ein Umschaltbefehl zwischen beiden Positionen soll möglich sein. | MUSS | EditBeginningLine, EditEndLine, EditBeginningEndLine. |
| NAV-007 | Zum Anfang und Ende des Dokuments springen. | MUSS | EditWindowTopFile/EditWindowBottomFile. |
| NAV-008 | Zum Anfang und Ende einer Auswahl beziehungsweise eines Blocks springen. | MUSS | EditTopBlock/EditBottomBlock. |
| NAV-009 | Zu einer angegebenen Zeile oder Spalte springen. | MUSS | EditGotoLine/EditGotoColumn und Cp-Varianten. |
| NAV-010 | Horizontales Scrollen muss bei breiten Zeilen unterstützt werden. | MUSS | Leftedge/EditHscroll. |
| NAV-011 | Navigation muss optional eine Auswahl erweitern können. | SOLL | Moderne Ergänzung. |
| NAV-012 | Die gewünschte visuelle Spalte soll bei vertikaler Navigation erhalten bleiben. | SOLL | Moderne UX-Anforderung. |

---

### 13.3 Texteingabe und Löschen

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| EDIT-001 | Normale Eingabe muss im Einfügemodus Text einschieben und im Überschreibmodus Text ersetzen. | MUSS | EditPrctxt/Insertflag. |
| EDIT-002 | Zwischen Einfüge- und Überschreibmodus muss umgeschaltet werden können. | MUSS | EditToggleInsert. |
| EDIT-003 | Neue Zeilen und leere Zeilen müssen eingefügt werden können. | MUSS | EditNewLine/EditInsertLine. |
| EDIT-004 | Zeichen rechts beziehungsweise unter dem Cursor und links vom Cursor müssen gelöscht werden können. | MUSS | EditDeleteRightChar/EditDeleteLeftChar. |
| EDIT-005 | Aktuelle Zeile, Wort rechts und Text bis Zeilenende müssen gelöscht werden können. | MUSS | EditDeleteLine/EditDeleteRightWord/EditDeleteTextRight. |
| EDIT-006 | Der gesamte Text eines Dokuments darf nur über eine ausdrücklich bestätigbare Aktion gelöscht werden. | SOLL | EditWindowDeleteText plus MicroStar-Sicherheitsanpassung. |
| EDIT-007 | Zeilen müssen geteilt, verbunden, verkürzt, erweitert, komprimiert und verschoben werden können, soweit interne oder öffentliche Befehle dies benötigen. | MUSS | EditJoinLine, EditShortLine, EditLongLine, EditCompressLine, EditShiftLine. |
| EDIT-008 | Groß-/Kleinschreibung eines Zeichens oder ausgewählten Textes muss geändert werden können. | MUSS | EditChangeCase, modern erweitert. |
| EDIT-009 | Steuerzeichen müssen über einen ausdrücklichen Befehl eingefügt werden können. | SOLL | EditInsertCtrlChar. |
| EDIT-010 | Ausschneiden, Kopieren und Einfügen über die Plattformzwischenablage müssen unterstützt werden. | MUSS | Moderne Abbildung der Blockfunktionen. |
| EDIT-011 | Mehrzeiliger Unicode-Text aus der Zwischenablage muss korrekt verarbeitet werden. | MUSS | Modernisierung. |
| EDIT-012 | Bearbeitungsoperationen müssen atomar sein und ihre Ausführbarkeit vorab prüfen können. | MUSS | Qualitätsanforderung. |

---

### 13.4 Textverarbeitung, Word-Wrap, Ränder und Tabulatoren

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| WP-001 | Word-Wrap muss je Ansicht aktivierbar und deaktivierbar sein. | MUSS | EditToggleWordwrap/WW. |
| WP-002 | Word-Wrap und Absatzformatierung müssen linken und rechten Rand berücksichtigen. | MUSS | Lmargin/Rmargin, EditPrctxt, EditReformat. |
| WP-003 | Absätze müssen neu formatiert werden können. | MUSS | EditReformat. |
| WP-004 | Auto-Indent muss je Ansicht aktivierbar sein und die Einrückung der vorherigen Zeile übernehmen. | MUSS | EditToggleAutoindent/AI. |
| WP-005 | Linker und rechter Rand müssen einstellbar sein. | MUSS | EditSetLeftMargin/EditSetRightMargin. |
| WP-006 | Eine Zeile muss innerhalb der aktiven Ränder zentriert werden können. | MUSS | EditCenterLine. |
| WP-007 | Tabulatoren und konfigurierbare Tabulatorbreite müssen unterstützt werden. | MUSS | EditTab/EditDefineTab. |
| WP-008 | Echte Tabulatorzeichen und visuelle Tabulatornavigation sollen unterscheidbar sein. | SOLL | Modernisierung. |
| WP-009 | Automatisch umgebrochene Zeilen sollen bei Bedarf als solche annotiert werden können. | SOLL | Wrapped-Flag. |
| WP-010 | Historische WordStar-Wrap-Markierungen dürfen nur als expliziter Kompatibilitätsimport/-export angeboten werden. | KANN | ASCII 141 in Kap. 12. |
| WP-011 | Wortgrenzen und Trennzeichen müssen erweiterbar und Unicode-gerecht behandelbar sein. | SOLL | Kap. 1 plus Modernisierung. |

---

### 13.5 Auswahl- und Blockfunktionen

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| SEL-001 | Beginn und Ende einer Auswahl beziehungsweise eines historischen Blocks müssen markierbar sein. | MUSS | EditBlockBegin/EditBlockEnd. |
| SEL-002 | Ausgewählter Text muss kopiert, verschoben und gelöscht werden können. | MUSS | EditBlockCopy/EditBlockMove/EditBlockDelete. |
| SEL-003 | Die Hervorhebung einer Auswahl muss ausblendbar sein, ohne den logischen Bereich zwingend zu verwerfen. | SOLL | EditBlockHide. |
| SEL-004 | Operationen zwischen Ansichten und Dokumenten müssen unterstützt werden, soweit semantisch zulässig. | SOLL | Kap. 12: Blockzugriff aus anderem Fenster oder Datei. |
| SEL-005 | Die moderne Standardauswahl muss zeichenweise und mehrzeilig arbeiten. | MUSS | Modernisierung der zeilenorientierten Blockidee. |
| SEL-006 | Ein historischer zeilenorientierter Blockmodus kann als Kompatibilitätsprofil angeboten werden. | KANN | Originaltreue Option. |
| SEL-007 | Gesamtes Dokument auswählen muss unterstützt werden. | MUSS | Moderne Basiserwartung. |
| SEL-008 | Auswahlpositionen müssen nach Änderungen deterministisch nachgeführt werden. | MUSS | Qualitätsanforderung. |

---

### 13.6 Suche, Ersetzen und Marker

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| SRCH-001 | Textsuche, nächste Fundstelle und Suchen/Ersetzen müssen unterstützt werden. | MUSS | EditFind/EditReplace und Cp-Varianten. |
| SRCH-002 | Ersetzen aller Fundstellen soll unterstützt werden. | SOLL | Moderne Ausprägung. |
| SRCH-003 | Suchoptionen sollen mindestens Groß-/Kleinschreibung, Richtung und Ganzwort umfassen. | SOLL | Handbuch: Find/Replace with options. |
| SRCH-004 | Reguläre Ausdrücke können optional angeboten werden. | KANN | Moderne Erweiterung. |
| SRCH-005 | Suche muss auf Dokument, Auswahl oder definierbarem Bereich arbeiten können. | SOLL | Moderne Erweiterung. |
| SRCH-006 | Such- und Ersetzvorgänge müssen abbrechbar sein. | MUSS | Abortcmd/EditAbort/EditBreathe, modernisiert. |
| SRCH-007 | Letzte Suchzeichenfolge, Ersetzungszeichenfolge und Optionen sollen erhalten bleiben. | SOLL | Searchstr/Replacestr/Optstr. |
| SRCH-008 | Nicht gefundene Muster müssen als eindeutiger Ergebniszustand gemeldet werden. | MUSS | Notfound. |
| SRCH-009 | Marker müssen gesetzt, angesprungen und über eine Eingabeaufforderung ausgewählt werden können. | MUSS | EditSetMarker/EditJumpMarker/EditCpjmpmrk. |
| SRCH-010 | Mindestens zwanzig Marker sollen kompatibel unterstützt werden; intern darf die Zahl höher sein. | SOLL | Maxmarker = 20. |
| SRCH-011 | Marker müssen dokumentbezogen und bei Textänderungen positionsstabil nachgeführt werden. | MUSS | Modernisierung. |

---

### 13.7 Mehrere Ansichten, Fenster und verknüpfte Textströme

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| VIEW-001 | Mehrere Dokumentansichten müssen gleichzeitig angezeigt werden können. | MUSS | Kap. 12: Multiple Windows. |
| VIEW-002 | Ansichten müssen erzeugt, geschlossen, aktiviert und in ihrer Größe verändert werden können. | MUSS | EditWindowCreate/Delete/Goto. |
| VIEW-003 | Schließen einer Ansicht darf ungespeicherten Dokumentinhalt nicht unbeabsichtigt vernichten. | MUSS | Original plus moderne Schutzanforderung. |
| VIEW-004 | Zwischen Ansichten muss vorwärts und rückwärts gewechselt werden können. | MUSS | EditWindowUp/EditWindowDown. |
| VIEW-005 | Ansichten müssen über Nummer, Kennung oder Auswahl angesprungen werden können. | MUSS | EditWindowGoto/EditCpgotowin. |
| VIEW-006 | Zwei oder mehr Ansichten müssen dasselbe Dokument teilen können. | MUSS | EditWindowLink/EditCplnkwin. |
| VIEW-007 | Änderungen in einer Ansicht müssen in allen verknüpften Ansichten konsistent sichtbar sein. | MUSS | Kap. 12. |
| VIEW-008 | Nicht sichtbare Dokumente oder Ansichten dürfen im Speicher gehalten und später wieder eingeblendet werden. | SOLL | Kap. 12. |
| VIEW-009 | Jede Ansicht muss eigenen Cursor-, Auswahl-, Scroll-, Modus- und Randzustand besitzen. | MUSS | Windesc. |
| VIEW-010 | Horizontale und vertikale Teilungen sollen in modernen Desktopadaptern möglich sein. | SOLL | Modernisierung der historischen horizontalen Teilung. |
| VIEW-011 | Die Zahl der Ansichten darf nicht künstlich auf acht oder neun begrenzt sein. | MUSS | Modernisierung. |
| VIEW-012 | Aktive Ansicht und Reihenfolge der Ansichten müssen eindeutig verwaltet werden. | MUSS | Curwin und zirkuläre Fensterliste. |

---

### 13.8 Datei- und Streamoperationen

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| FILE-001 | Textdateien müssen in neue oder bestehende Dokumente eingelesen werden können. | MUSS | EditFileRead/EditReatxtfil. |
| FILE-002 | Speichern, Speichern unter und Schreiben in einen angegebenen Dateinamen müssen unterstützt werden. | MUSS | EditFileWrite/EditFileSave. |
| FILE-003 | Dateioperationen müssen interaktiv über Host-Prompts und programmgesteuert über Parameter aufrufbar sein. | MUSS | Cp- und parameterbasierte Ebenen. |
| FILE-004 | Dateioperationen sollen asynchron und abbrechbar ausführbar sein. | SOLL | Modernisierung des Breathe/Abort-Konzepts. |
| FILE-005 | Zeichencodierung und Zeilenenden müssen erkannt, erhalten und bewusst konvertiert werden können. | MUSS | Modernisierung. |
| FILE-006 | Binäre oder nicht unterstützte Dateien dürfen nicht stillschweigend als normaler Text überschrieben werden. | MUSS | Sicherheitsanforderung. |
| FILE-007 | Lese- und Schreibfehler müssen ohne Verlust des bisherigen Dokumentinhalts behandelt werden. | MUSS | File-I/O-Fehler und Error Handling. |
| FILE-008 | Speichern soll nach Möglichkeit atomar über temporäre Datei und Ersetzung erfolgen. | SOLL | Moderne Robustheit. |
| FILE-009 | Vor dem Überschreiben extern geänderter Dateien soll gewarnt werden können. | SOLL | Moderne Schutzanforderung. |
| FILE-010 | Die API soll Stream-basierte Ein- und Ausgabe zusätzlich zu Dateipfaden ermöglichen. | SOLL | Moderne Integration. |
| FILE-011 | Historische WordStar-Wrap-Markierungen können als optionaler Import-/Exportmodus angeboten werden. | KANN | Kap. 12. |
| FILE-012 | Markierte Blöcke als separate Datei lesen oder schreiben ist eine optionale Erweiterung. | KANN | MicroStar Block Read/Write. |
| FILE-013 | Dateiumbenennung, Dateikopie, Dateilöschung und Directory Display sind optionale Hostfunktionen, nicht Pflicht des Editor-Kerns. | KANN | MicroStar-Erweiterungen. |
| FILE-014 | Vor Verwerfen ungespeicherter Änderungen muss der Host Speichern, Verwerfen oder Abbrechen anbieten können. | MUSS | Dirty Bit; Save/Open/Abandon. |

---

### 13.9 Undo, Redo und Änderungszustand

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| UNDO-001 | Bearbeitungs- und Löschoperationen müssen rückgängig gemacht werden können. | MUSS | EditUndo. |
| UNDO-002 | Mehrere Undo-Schritte und Redo müssen unterstützt werden. | MUSS | Moderne Basiserwartung. |
| UNDO-003 | Zusammengehörige Änderungen müssen als Transaktion gruppierbar sein. | MUSS | Moderne Anforderung. |
| UNDO-004 | Das Undo-Limit muss konfigurierbar sein. | MUSS | EditSetUndoLimit/Undolimit. |
| UNDO-005 | Das Limit soll über Operationen, Speicherbudget oder beides steuerbar sein. | SOLL | Modernisierung. |
| UNDO-006 | Undo/Redo muss dokumentbezogen und von der Anzahl der Ansichten unabhängig sein. | MUSS | Shared Document. |
| UNDO-007 | Dirty State muss verlässlich anzeigen, ob der Inhalt vom letzten gespeicherten Zustand abweicht. | MUSS | EditChangeFlag. |
| UNDO-008 | Nach erfolgreichem Speichern muss der gespeicherte Zustand als sauber markiert werden. | MUSS | Kap. 8. |
| UNDO-009 | Eigene Erweiterungsbefehle müssen Änderungen in Undo/Redo und Dirty State integrieren können. | MUSS | Kap. 8: eigene Befehle müssen ChangeFlag setzen. |

---

### 13.10 Command-System und Tastaturprofile

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| CMD-001 | Alle Benutzeraktionen müssen über eindeutig identifizierbare Editorbefehle aufrufbar sein. | MUSS | Command/Command Processor. |
| CMD-002 | Ein zentraler Dispatcher muss Eingaben Befehlen zuordnen. | MUSS | EditPrccmd. |
| CMD-003 | Befehle müssen unabhängig von Menü, Tastatur, Toolbar, Kontextmenü oder API aufrufbar sein. | MUSS | Handbuch plus Modernisierung. |
| CMD-004 | Befehle müssen ihre Ausführbarkeit prüfen und synchron oder asynchron ausführbar sein. | MUSS | Moderne Command-Anforderung. |
| CMD-005 | Hooks/Interceptor müssen Befehle filtern, ersetzen, unterdrücken oder selbst verarbeiten können. | MUSS | UserCommand. |
| CMD-006 | Ein vollständig behandelter Befehl darf nicht erneut durch den Standarddispatcher ausgeführt werden. | MUSS | UserCommand-Rückgabe 255. |
| CMD-007 | Einzelne Tastendrücke und mehrstufige Präfixsequenzen müssen abbildbar sein. | MUSS | UserCommand sowie Ctrl-K/O/Q. |
| CMD-008 | Tastaturprofile müssen austauschbar und benutzerdefiniert speicherbar sein. | MUSS | Frei änderbare Dispatcher; Modernisierung. |
| CMD-009 | Ein modernes Standardprofil und ein Turbo-Editor-/WordStar-kompatibles Profil sollen geliefert werden. | SOLL | Projektentscheidung. |
| CMD-010 | Hosts müssen eigene Befehle registrieren und Standardbefehle ersetzen oder deaktivieren können. | MUSS | Kap. 6. |
| CMD-011 | Mehrere Eingabeereignisse sollen programmatisch in eine Sequenz gestellt werden können. | KANN | EditUserPush/Pokechr; Makros. |
| CMD-012 | Makros und eine Befehlssprache müssen architektonisch möglich sein, gehören aber nicht zum ersten Meilenstein. | KANN | Kap. 6. |
| CMD-013 | Ein globaler Abbruchbefehl muss lang laufende Operationen beenden können. | MUSS | Ctrl-U/EditAbort. |
| CMD-014 | Unbekannte Eingaben dürfen keine unkontrollierte Aktion auslösen. | MUSS | Qualitätsanforderung. |

---

### 13.11 Oberfläche und Darstellung

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| UI-001 | Die WinForms-Integration muss als wiederverwendbares Control oder klar gekapselte Adapterkomponente vorliegen. | MUSS | Projektentscheidung. |
| UI-002 | Text, Cursor, Auswahl, Suchtreffer, Marker und besondere Annotationen müssen darstellbar sein. | MUSS | Screen routines, Blocks, Colored. |
| UI-003 | Eine Statusanzeige muss mindestens Dateiname, Zeile, Spalte, Dirty State und Bearbeitungsmodus anzeigen können. | MUSS | Status line/Windesc. |
| UI-004 | Statusanzeige muss durch den Host ersetzbar, erweiterbar oder ausblendbar sein. | MUSS | UserStatusLine. |
| UI-005 | Eine Befehls-/Meldungszeile oder funktional gleichwertige Prompt-Darstellung soll möglich sein. | SOLL | Command line. |
| UI-006 | Menüs, Toolbar und Kontextmenüs müssen dasselbe Command-System verwenden. | MUSS | MicroStar Pulldown Menu, modernisiert. |
| UI-007 | Eine optionale Retro-/MicroStar-Demo kann historische Pulldown-Menüs nachvollziehbar zeigen. | KANN | MicroStar. |
| UI-008 | Modale und nichtmodale Eingabeaufforderungen müssen über Hostdienste ersetzbar sein. | MUSS | MakeWindow/RestoreWindow/EditAskfor. |
| UI-009 | Fehler- und Bestätigungsdarstellung muss plattformspezifisch austauschbar sein. | MUSS | UserError. |
| UI-010 | Themes müssen mindestens normalen Text, Auswahl, Status, Befehle und besondere Inhalte unterscheiden können. | MUSS | Txtcolor/Blockcolor/Bordcolor/Cmdcolor/Usercolor. |
| UI-011 | Die UI darf nicht von Videospeicherzugriff, Retrace-Timing oder 80x25-Raster abhängen. | MUSS | Modernisierung Kap. 11. |
| UI-012 | Die WinForms-Demo muss Menüs, Toolbar, Statusleiste, mehrere Ansichten und Dateidialoge demonstrieren. | MUSS | Projektentscheidung. |
| UI-013 | WPF und Web erhalten eigene Adapter auf demselben Kern. | SOLL | Projektentscheidung. |
| UI-014 | Legacy-ASPX/Web-Forms ist im ersten Schritt kein First-Class-Control, darf aber den Kern integrieren. | SOLL | Projektentscheidung. |
| UI-015 | Unsichtbare Zeichen, Zeilenenden, Tabs, Leerzeichen und Steuerzeichen sollen optional darstellbar sein. | SOLL | Kap. 1, modernisiert. |
| UI-016 | Zeilennummern, Ränder und aktuelle Cursorzeile sollen optional darstellbar sein. | SOLL | Moderne Ergänzung. |
| UI-017 | Rendering und Scrollen müssen bei Eingabe reaktionsfähig bleiben. | MUSS | Unterbrechbare Screen Updates, Kap. 11. |
| UI-018 | High DPI, Größenänderung, Fokus und IME müssen im WinForms-Adapter berücksichtigt werden. | MUSS | Moderne Desktopanforderung. |

---

### 13.12 Fehler, Meldungen und Bestätigungen

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| ERR-001 | Der Kern muss strukturierte Fehler und Ergebnisobjekte liefern, nicht ausschließlich Meldungstext. | MUSS | Modernisierung von EDITERR.MSG. |
| ERR-002 | Hosts müssen Fehler vollständig selbst behandeln oder an eine Standardbehandlung weiterreichen können. | MUSS | UserError und Msgno=0. |
| ERR-003 | Fehlermeldungen müssen lokalisierbar sein. | MUSS | Modernisierung des externen Fehlerkatalogs. |
| ERR-004 | Datei-, Speicher-, Eingabe-, Such- und Zustandsfehler müssen unterscheidbar sein. | MUSS | Historische Fehlernummern. |
| ERR-005 | Fehler dürfen keine Teiländerungen oder Datenverluste verursachen. | MUSS | Qualitätsanforderung. |
| ERR-006 | Bestätigungen für destruktive Aktionen müssen zentral konfigurierbar sein. | MUSS | MicroStar Delete-Text-Bestätigung. |
| ERR-007 | Bibliothekscode darf keine UI-Dialoge erzwingen. | MUSS | UI-Unabhängigkeit. |

---

### 13.13 Hintergrundaufgaben

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| BG-001 | Ein generischer Mechanismus für inkrementelle Hintergrund- oder Hosttasks soll vorhanden sein, ohne proprietären Editor-Scheduler. | SOLL | UserTask, modernisiert. |
| BG-002 | Hintergrundaufgaben müssen schnell auf Eingaben und Abbruch reagieren und ihren Zustand erhalten können. | MUSS | Kooperatives Multitasking, Kap. 10. |
| BG-003 | Autosave muss als optionaler Hostdienst integrierbar sein. | SOLL | Vorgeschlagene UserTask-Anwendung. |
| BG-004 | Hintergrunddruck kann als Beispiel oder optionale Erweiterung dokumentiert werden. | KANN | MicroStar PrintNext. |
| BG-005 | Historische Modemwahl, Upload/Download oder Festplattenbackup werden nicht als Editorfunktionen übernommen. | NICHT | Kap. 10; nur generischer Task-Hook bleibt. |
| BG-006 | Moderne Hosts sollen Tasks, CancellationToken und Plattformdispatcher statt Polling verwenden. | SOLL | Modernisierungsentscheidung. |

---

### 13.14 Einbettung und Erweiterbarkeit

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| INT-001 | Der Editor muss in größere Anwendungen eingebettet werden können. | MUSS | Kap. 14. |
| INT-002 | Hosts müssen Dokumente, Commands, Dienste und Einstellungen programmgesteuert bereitstellen können. | MUSS | Original plus Modernisierung. |
| INT-003 | Der Core muss headless für Tests, Batchverarbeitung und serverseitige Funktionen nutzbar sein. | MUSS | Projektentscheidung. |
| INT-004 | Clipboard, Dateidialoge, Druck, Theme, Eingabe, Logging und Benachrichtigungen müssen über Adapter/Dienste austauschbar sein. | MUSS | Projektentscheidung. |
| INT-005 | Integration darf keine Include-Reihenfolge, globalen Variablen, Overlays oder Chain-Files verlangen. | MUSS | Modernisierung Kap. 6, 13, 14. |
| INT-006 | Dependency Injection und öffentliche Schnittstellen sollen unterstützt werden. | SOLL | SASD Development Standard. |
| INT-007 | Die Bibliotheksgruppe soll NuGet-fähig paketierbar sein. | SOLL | Moderne Distribution. |
| INT-008 | Öffentliche APIs müssen versioniert und dokumentiert sein. | MUSS | Produktanforderung. |
| INT-009 | Desktop Components soll den Editor nur referenzieren und als Showcase verwenden. | SOLL | Projektentscheidung. |
| INT-010 | Data Toolbox darf optionaler Speicherdienst sein, aber keine Pflichtabhängigkeit. | MUSS | Projektabgrenzung. |
| INT-011 | Numerics, Graphics und GameWorks dürfen den Editor konsumieren, aber keine zyklischen Abhängigkeiten erzeugen. | MUSS | Produktfamilienentscheidung. |
| INT-012 | Syntaxhervorhebung, Markdown-, Log-, Hex- oder Property-Editoren müssen später auf öffentlichen Erweiterungspunkten aufbauen können. | SOLL | Projektvision. |

---

### 13.15 Einstellungen

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| SET-001 | Einstellungen müssen Tastaturprofil, Theme, Schrift, Tabulatorbreite, Ränder, Word-Wrap, Auto-Indent, Undo-Limit und Anzeigeoptionen umfassen können. | MUSS | Original plus Modernisierung. |
| SET-002 | Globale Defaults und dokument-/ansichtsspezifische Überschreibungen sollen getrennt werden. | SOLL | Modernisierung. |
| SET-003 | Einstellungen müssen serialisierbar, validierbar und wiederherstellbar sein. | SOLL | Moderne Anforderung. |
| SET-004 | Ungültige Werte müssen mit klaren Fehlern oder sicheren Defaults behandelt werden. | MUSS | Qualitätsanforderung. |
| SET-005 | 80 Spalten, 25 Zeilen und historische Farben dürfen nur Demo-/Kompatibilitätswerte sein. | MUSS | Modernisierung. |

---

## 14. Nichtfunktionale Anforderungen

| ID | Anforderung | Priorität | Herkunft / Nachweis |
|---|---|---|---|
| NFR-ARCH-001 | Klare Trennung zwischen Core, Anwendungsdiensten, UI-Adaptern und Samples. | MUSS | Projektentscheidung. |
| NFR-ARCH-002 | Keine zyklischen Projekt- oder Paketabhängigkeiten. | MUSS | SASD Development Standard. |
| NFR-ARCH-003 | UI-Frameworktypen dürfen nicht in öffentliche Core-APIs gelangen. | MUSS | Projektentscheidung. |
| NFR-PERF-001 | Normale Bearbeitungs- und Cursoroperationen müssen subjektiv unmittelbar reagieren. | MUSS | Historisches Ziel 'lightning fast'. |
| NFR-PERF-002 | Rendering soll nur betroffene sichtbare Bereiche aktualisieren. | SOLL | Differenzielle Screen Updates, Kap. 11. |
| NFR-PERF-003 | Lange Datei- und Suchoperationen dürfen die UI nicht dauerhaft blockieren. | MUSS | Unterbrechbare Routinen, modernisiert. |
| NFR-PERF-004 | Die Architektur muss spätere optimierte Puffer für sehr große Dateien ermöglichen. | MUSS | Projektentscheidung. |
| NFR-ROB-001 | Ungespeicherte Änderungen müssen vor destruktiven Aktionen geschützt werden. | MUSS | Dirty Bit. |
| NFR-ROB-002 | Dateispeichern muss gegen I/O-Fehler möglichst robust sein. | MUSS | Moderne Anforderung. |
| NFR-ROB-003 | Der Core muss deterministisch und ohne globale mutable Singletons testbar sein. | MUSS | Modernisierung. |
| NFR-SEC-001 | Dateipfade und Inhalte sind als nicht vertrauenswürdig zu behandeln. | MUSS | Moderne Sicherheitsanforderung. |
| NFR-SEC-002 | Makros, Skripte oder eingebettete Inhalte dürfen nicht automatisch ausgeführt werden. | MUSS | Moderne Sicherheitsanforderung. |
| NFR-SEC-003 | Temporär- und Autosave-Dateien müssen kontrolliert und mit angemessenen Rechten gespeichert werden. | SOLL | Moderne Sicherheitsanforderung. |
| NFR-ACC-001 | Desktopadapter müssen vollständig per Tastatur bedienbar sein. | MUSS | Historische Tastaturorientierung plus Accessibility. |
| NFR-ACC-002 | Kontrast, Fokus und Screenreader-Informationen sollen berücksichtigt werden. | SOLL | Moderne Barrierefreiheit. |
| NFR-I18N-001 | Unicode, lokalisierbare Meldungen und kultursensible UI-Texte müssen unterstützt werden. | MUSS | Modernisierung. |
| NFR-TEST-001 | Der Core muss umfassende Unit Tests erhalten. | MUSS | SASD Development Standard. |
| NFR-TEST-002 | Architekturtests müssen die Abhängigkeitsrichtung sichern. | MUSS | Projektentscheidung. |
| NFR-TEST-003 | Integrationstests müssen Dateiabläufe, Undo/Redo, Suche, Ansichten und Tastaturprofile abdecken. | MUSS | Akzeptanzanforderung. |
| NFR-DOC-001 | Öffentliche APIs müssen XML-Dokumentationskommentare erhalten. | MUSS | Nutzerpräferenz und SASD-Standard. |
| NFR-DOC-002 | README, Lastenheft, Pflichtenheft, Architektur-, Entwickler-, Test- und Benutzerhandbuch müssen vorgesehen werden. | MUSS | SASD Development Standard. |
| NFR-DOC-003 | Historische Funktionen müssen auf moderne Anforderungen oder bewusste Nichtübernahme abgebildet werden. | MUSS | Vollständigkeitsanforderung. |
| NFR-COMP-001 | Der erste produktive Stand soll auf einer unterstützten .NET-LTS-Version basieren; die konkrete Version legt das Pflichtenheft fest. | SOLL | Moderne Projektanforderung. |
| NFR-COMP-002 | WinForms und WPF sind Windowsadapter; der Core bleibt plattformneutral. | MUSS | Projektentscheidung. |
| NFR-LIC-001 | Originalquellcode, Marken und geschützte Gestaltungselemente dürfen nicht ungeprüft übernommen werden. | MUSS | Copyright-Hinweis im Handbuch. |
| NFR-LIC-002 | Vor Veröffentlichung müssen Name, Markenbezug und Rechtehinweise geprüft werden. | MUSS | Rechtliche Vorsicht. |
| NFR-MAINT-001 | Code muss verständlich strukturiert, großzügig kommentiert und dokumentiert sein. | MUSS | Nutzerpräferenz. |
| NFR-MAINT-002 | Breaking Changes müssen dokumentiert und versioniert werden. | MUSS | Produktanforderung. |
| NFR-OBS-001 | Fehler und wichtige Dateioperationen sollen über abstrahiertes Logging protokollierbar sein. | SOLL | Moderne Betriebsanforderung. |

---

## 15. Historische Befehlsabdeckung

Die folgende Übersicht stellt sicher, dass keine im fachlichen Kapitel 12 oder in den Beispielprogrammen dargestellte Befehlskategorie stillschweigend verloren geht.

| Kategorie | Historischer Funktionsumfang | Behandlung |
|---|---|---|
| **Cursor/Navigation** | Left/Right Character, Up/Down Line, Scroll Up/Down, Up/Down Page, Left/Right Word, Beginning/End Line, Top/Bottom File, Top/Bottom Block, Go To Line, Go To Column | Durch NAV-Anforderungen abgedeckt |
| **Text löschen** | Delete Right/Left Character, Delete Line, Delete Window Text, Delete Block, Delete Text Right, Delete Right Word | Durch EDIT- und SEL-Anforderungen abgedeckt |
| **Textverarbeitung** | Replace, Center Line, Change Case, Find, Jump/Set Marker, Undo Limit, Ränder, Auto-Indent, Word-Wrap, Reformat, Control Character, Undo, Insert, Tab | Durch WP-, SRCH-, UNDO- und EDIT-Anforderungen abgedeckt |
| **Fenster/Ansichten** | Create, Delete, Top/Bottom File, Delete Text, Link, Up, Down, Goto | Durch VIEW-Anforderungen abgedeckt |
| **Blöcke** | Begin, End, Copy, Move, Delete, Hide/Show | Durch SEL-Anforderungen abgedeckt |
| **Dateien** | Read, Write, Save, interaktive Varianten und Textdatei-I/O | Durch FILE-Anforderungen abgedeckt |
| **Beenden** | Exit und interaktive Exit-Variante | Über Host-Lifecycle und Dirty-State-Entscheidung abgedeckt |
| **MicroStar-Erweiterungen** | Pulldown-Menüs, Popups, Save/Open/Abandon, Block Read/Write, Dateiverwaltung, Directory Display, Background Printing, Spell Check | Kernaspekte abgedeckt; optionale Erweiterungen gekennzeichnet |

### 15.1 FIRST-ED-Kompatibilitätsprofil

Das Handbuch dokumentiert für FIRST-ED eine WordStar-nahe Bedienung mit:

- Ctrl-A/S/D/F/E/X/R/C/W/Z für Navigation,
- Return, Insert Line, Zeichen-, Wort- und Zeilenlöschung,
- Reformat, Insert-Modus, Find Next und Undo,
- Ctrl-O-Kommandos für Fenster, Word-Wrap, Zentrieren, Spalten/Zeilen, Groß-/Kleinschreibung, Ränder und Undo-Limit,
- Ctrl-Q-Kommandos für schnelle Navigation, Auto-Indent, Blockgrenzen, Marker, Suche und Ersetzen,
- Ctrl-K-Kommandos für Blockoperationen, Dateien, Tabulatoren, Marker und Exit.

Diese Folgen werden nicht als alleinige moderne Standardbedienung vorgeschrieben. Sie sollen in einem optionalen Kompatibilitätsprofil abbildbar sein.

### 15.2 MicroStar-Erweiterungen

MicroStar ergänzt oder verändert:

- Pulldown-Menüs über F10,
- Popup-Fenster für Eingaben und Bestätigungen,
- Save-and-Resume, Save-and-Open, Abandon-and-Open,
- Block Read und Block Write,
- Hintergrunddruck,
- optionale Rechtschreibprüfung,
- benutzerdefinierte Status- und Fehlerdarstellung,
- geänderte WordStar-kompatible Tastenzuordnungen.

Für Version 1.0 sind Command-/Menüintegration, Status-/Fehleranpassung und sichere Dateiabläufe relevant. Rechtschreibprüfung, Blockdateien und Hintergrunddruck bleiben optionale Erweiterungen.

---

## 16. Daten- und Zustandsanforderungen

### 16.1 Dokumentzustand

Ein Dokument muss fachlich mindestens verwalten:

- eindeutige Identität,
- Textinhalt,
- optionalen Dateipfad,
- Anzeigenamen,
- Zeichencodierung,
- Zeilenendungsformat,
- gespeicherten Referenzzustand,
- Dirty State,
- Undo-/Redo-Historie,
- Marker,
- optionale Annotationen.

### 16.2 Ansichtszustand

Eine Ansicht muss mindestens verwalten:

- referenziertes Dokument,
- Cursorposition,
- Auswahl,
- vertikale und horizontale Scrollposition,
- bevorzugte visuelle Spalte,
- Insert/Overwrite,
- Word-Wrap,
- Auto-Indent,
- linker und rechter Rand,
- Tabulatorbreite,
- Zoom- und Anzeigeoptionen,
- aktiven Fokuszustand.

### 16.3 Sitzungszustand

Ein Host darf zusätzlich verwalten:

- geöffnete Dokumente,
- sichtbare und verborgene Ansichten,
- Layout und Teilungen,
- aktive Ansicht,
- Tastaturprofil,
- Theme,
- zuletzt verwendete Suchoptionen,
- Wiederherstellungs- und Autosaveinformationen.

### 16.4 Vermeidung globaler Zustände

Die historischen globalen Variablen werden nicht als prozessweite mutable Variablen reproduziert. Ihre Verantwortlichkeiten werden in Dokument-, Ansichts-, Sitzungs-, Command-, Rendering- oder Dienstezustände überführt.

---

## 17. Plattform- und Schnittstellenanforderungen

### 17.1 WinForms

Die erste Referenzintegration muss:

- als wiederverwendbare Komponente integrierbar sein,
- Tastatur und Maus verarbeiten,
- Clipboard unterstützen,
- Dateidialoge und Hostdienste anbinden,
- Menüs, Toolbar und Statusleiste demonstrieren,
- mehrere Ansichten unterstützen,
- High-DPI-fähig sein,
- keine fachliche Logik duplizieren.

### 17.2 WPF

Der spätere WPF-Adapter muss:

- denselben Kern verwenden,
- WPF-spezifische Commands, Input Bindings, Dependency Properties und Renderingmechanismen kapseln,
- keine WinForms-Abhängigkeit benötigen.

### 17.3 ASP.NET und Web

- Der Kern muss serverseitig nutzbar bleiben.
- Eine interaktive Browseroberfläche benötigt einen eigenen Adapter.
- Blazor oder Razor sind gegenüber einer neuen umfangreichen ASPX/Web-Forms-Komponente zu bevorzugen.
- ASPX-Anwendungen dürfen über Hostintegration, JavaScript-Komponente oder serverseitige Kerndienste angebunden werden.
- Browser-Clipboard, IME, Fokus, Auswahl und Rendering sind adapterspezifische Aufgaben.

### 17.4 Headless-Nutzung

Der Kern muss ohne sichtbare Oberfläche nutzbar sein für:

- Unit Tests,
- Texttransformationen,
- serverseitige Suche/Ersetzung,
- Import/Export,
- Batchverarbeitung,
- spezialisierte Hostanwendungen.

---

## 18. Sicherheits- und Datenschutzanforderungen

1. Textdateien und Pfade sind nicht vertrauenswürdig.
2. Dateien dürfen nicht automatisch als Code, Makro oder Skript ausgeführt werden.
3. Destruktive Dateioperationen benötigen eine explizite Hostentscheidung.
4. Temporär- und Wiederherstellungsdateien dürfen keine vermeidbaren Klartextkopien an unerwarteten Orten erzeugen.
5. Fehlermeldungen dürfen sensible Dateiinhalte oder Zugangsdaten nicht unnötig protokollieren.
6. Externe Dateiänderungen sollen erkannt werden können.
7. Ein Editor-Control darf keine Netzwerkkommunikation ohne ausdrücklichen Hostauftrag durchführen.
8. Optionale Plugins oder Makros benötigen später ein eigenes Sicherheitsmodell.

---

## 19. Dokumentationsanforderungen

Das Repository muss mindestens vorsehen:

```text
README.md
LICENSE
CHANGELOG.md
CONTRIBUTING.md
SECURITY.md

docs/
├── de/
│   ├── Lastenheft.md
│   ├── Pflichtenheft.md
│   ├── Architektur.md
│   ├── Benutzerhandbuch.md
│   ├── Entwicklerhandbuch.md
│   ├── Testhandbuch.md
│   └── Historical-Traceability.md
└── en/
    └── entsprechende englische Dokumente

src/
tests/
samples/
```

Die konkrete Struktur legt das Pflichtenheft fest. Öffentliche APIs benötigen XML-Dokumentation, kurze Beispiele, Angaben zu Seiteneffekten, Threading, Cancellation, Fehlerzuständen und API-Stabilität.

---

## 20. Test- und Qualitätsanforderungen

### 20.1 Unit Tests

Mindestens zu testen sind:

- Textpositionen und Bereiche,
- Einfügen, Überschreiben und Löschen,
- Zeilen teilen und verbinden,
- Cursorbewegungen,
- Auswahl und Blockoperationen,
- Word-Wrap und Auto-Indent,
- Ränder und Tabulatoren,
- Suche und Ersetzen,
- Marker,
- Undo/Redo,
- Dirty State,
- mehrere Ansichten,
- Datei- und Streamoperationen,
- Command Routing und Tastaturprofile,
- Abbruch und Fehlerbehandlung.

### 20.2 Integrations- und Adaptertests

- WinForms-Control in einer Beispielanwendung,
- Clipboard,
- Dateidialog- und Hostdienstintegration,
- High DPI und Größenänderungen,
- Eingabefokus und IME,
- Tastatursequenzen,
- mehrere Ansichten,
- Speichern/Verwerfen/Abbrechen.

### 20.3 Architekturtests

- Core referenziert kein WinForms, WPF oder ASP.NET.
- Adapter referenzieren Core, nicht umgekehrt.
- Samples enthalten keine Kernimplementierungen.
- Desktop Components konsumiert Projekte/Pakete, erzeugt aber keine zyklische Abhängigkeit.

### 20.4 Belastungs- und Performanceprüfungen

- sehr lange Zeilen,
- sehr viele Zeilen,
- wiederholtes Undo/Redo,
- Suche in großen Dateien,
- mehrere verknüpfte Ansichten,
- schnelles Scrollen und Tastatureingabe,
- Speicherfreigabe nach Schließen großer Dokumente.

---

## 21. Abnahmekriterien

| ID | Kriterium |
|---|---|
| AK-001 | Ein leeres Dokument kann erstellt, bearbeitet, gespeichert, geschlossen und identisch erneut geöffnet werden. |
| AK-002 | Unicode-Zeichen einschließlich Umlauten, nichtlateinischer Schrift und Emojis bleiben bei unterstützter Codierung erhalten. |
| AK-003 | Undo/Redo funktioniert über Einfügen, Löschen, mehrzeilige Änderungen, Blockoperationen und Ersetzen. |
| AK-004 | Zwei Ansichten desselben Dokuments teilen Inhalt, behalten aber eigene Cursor- und Scrollpositionen. |
| AK-005 | Alle Befehlskategorien aus Kap. 12 sind implementiert, modern abgebildet oder ausdrücklich verschoben/entfallen. |
| AK-006 | Ein Tastaturprofil kann geladen werden, ohne den Core neu zu kompilieren. |
| AK-007 | Ein Hostcommand kann registriert und über Menü und Tastatur ausgelöst werden. |
| AK-008 | Der Core lässt sich ohne Windows-UI-Thread testen. |
| AK-009 | Das Core-Projekt enthält keine WinForms-, WPF- oder ASP.NET-Referenzen. |
| AK-010 | Beim Schließen geänderter Dokumente erhält der Host einen Speichern/Verwerfen/Abbrechen-Entscheidungspunkt. |
| AK-011 | Dateifehler führen nicht zum Verlust des bisherigen Dokumentinhalts. |
| AK-012 | Suche und Ersetzen sind abbrechbar und melden 'nicht gefunden' eindeutig. |
| AK-013 | Die WinForms-Demo zeigt Status, mehrere Ansichten, Dateioperationen, Suche, Undo/Redo und mindestens zwei Tastaturprofile. |
| AK-014 | Öffentliche APIs besitzen XML-Dokumentation und Kernfunktionen automatisierte Tests. |
| AK-015 | Historische Kapitel- und API-Traceability ist im Repository vorhanden. |

---

## 22. Priorisierung und Meilensteine

| Meilenstein | Ziel | Inhalt |
|---|---|---|
| M0 | Anforderungs- und Architekturgrundlage | Lastenheft, Rechte-/Namensprüfung, Pflichtenheft, Architektur, Rendererentscheidung. |
| M1 | Modern FIRST-ED / 0.1 | Core, Basisbearbeitung, Datei, Undo/Redo, Suche, Dirty State, Commands, WinForms-Control, Demo, Tests. |
| M2 | Historische Funktionsabdeckung / 0.5 | Ränder, Wrap, Auto-Indent, Marker, Blocks, Fenster, Tastaturprofile, Themes, Fehler- und Promptdienste. |
| M3 | Stabile Bibliothek / 1.0 | API-Stabilisierung, NuGet, Performance, Large-File-Pfad, DE/EN-Dokumentation, Integrations- und Architekturtests. |
| M4 | WPF-Adapter | Eigenes WPF-Control und Sample auf identischem Core. |
| M5 | Web-Adapter | Blazor/Razor-Komponente; optionaler ASPX-Integrationsweg. |
| M6 | Erweiterungen | Syntaxhervorhebung, Markdown, Log-/Hex-Viewer, Makros, optionaler Hintergrunddruck. |

### 22.1 Prioritätsregel

- **MUSS:** erforderlich für Produktnutzen oder sichere Architektur.
- **SOLL:** wichtig, darf begründet verschoben werden.
- **KANN:** optionale Erweiterung.
- **NICHT:** historische Fähigkeit oder Technik, die bewusst nicht übernommen wird.

### 22.2 Empfohlene MVP-Grenze

Version 0.1 soll noch nicht erzwingen:

- vollständige WPF- oder Webadapter,
- Plugin-Framework,
- Syntaxhervorhebung,
- hochoptimierten Rope-/Piece-Table-Puffer,
- Hintergrunddruck,
- Rechtschreibprüfung,
- vollständige Retro-Oberfläche,
- komplexe Makrosprache.

Sie muss aber die Architektur so setzen, dass diese Funktionen später ohne Kernneubau ergänzt werden können.

---

## 23. Risiken und Gegenmaßnahmen

| Risiko | Auswirkung | Gegenmaßnahme |
|---|---|---|
| R-01 - Eigener Renderer wird unterschätzt | Ein vollständig eigener Editor-Renderer ist deutlich aufwendiger als ein RichTextBox-Wrapper. | Früh zwischen Wrapper, Hybrid und Custom Rendering entscheiden; MVP begrenzen. |
| R-02 - Zu frühe Multi-UI-Entwicklung | Parallelentwicklung destabilisiert API und Zeitplan. | Core und WinForms zuerst; weitere Adapter nach stabiler API. |
| R-03 - Historische Grenzen werden übernommen | ASCII, 255-Zeichen-Limit oder globale Zustände schaden Zukunftsfähigkeit. | Nur fachliche Ideen übernehmen. |
| R-04 - Over-Engineering | Zu viele Assemblies, Plugins und Pufferstrategien verzögern Version 0.1. | Schlanke Startstruktur und Erweiterung bei realem Bedarf. |
| R-05 - Große Dateien | Naive Strings oder Listen können langsam und speicherintensiv werden. | Pufferabstraktion, Benchmarks und später Piece Table/Rope. |
| R-06 - IME und internationale Eingabe | Eigene Eingabeverarbeitung ist komplex. | Adapterkapselung und frühe Unicode-/IME-Tests. |
| R-07 - Marken-/Urheberrechtsverwechslung | Name oder Originalcode können Rechtefragen auslösen. | Eigenständiger SASD-Name, Clean-Room-Neuimplementierung, Prüfung. |
| R-08 - Grenze zu Desktop Components verschwimmt | Codeverdopplung oder zyklische Abhängigkeiten. | Editor eigenständig; Desktop Components nur Consumer. |

---

## 24. Annahmen und offene Entscheidungen

Vor Entwicklung oder im Pflichtenheft sind zu entscheiden:

1. Baut der erste WinForms-Adapter auf einem vorhandenen Text-Control, einem Hybridansatz oder einem eigenen Renderer auf?
2. Welche .NET-LTS-Zielversion wird verwendet?
3. Welche maximale Dateigröße gilt für Version 0.1 und Version 1.0?
4. Welche Pufferimplementierung wird initial gewählt?
5. Werden Word-Wrap und Ränder als Darstellung, textverändernde Absatzformatierung oder beide Modi angeboten?
6. Welche Suchoptionen sind in Version 0.1 verpflichtend?
7. Wie werden Marker bei komplexen Änderungen stabil nachgeführt?
8. Welche NuGet-Pakete werden veröffentlicht?
9. Welche Lizenz erhält die Neuimplementierung?
10. Welcher öffentliche Produkt- und Paketname vermeidet Markenverwechslungen?
11. Kommt das historische Ctrl-K/O/Q-Profil in 0.1 oder 0.5?
12. Welche Accessibility-Ziele gelten für den ersten WinForms-Adapter?
13. Welche Form von Autosave und Crash Recovery soll 1.0 bieten?
14. Wird der Webadapter als Blazor-Komponente, JavaScript-Interop oder serverseitige Editor-API realisiert?

---

## 25. Historische Kapitel-Traceability

| Quelle | Wesentlicher Inhalt | Abdeckung |
|---|---|---|
| Einleitung | Zweck, einfache und komplexe Editoren, Zeichen/Wörter/Zeilen/Textströme/Fenster/Blöcke/Dateien/Bildschirm | PROD, CORE, INT |
| Kap. 1 | Begriffe und Dokument-/Nichtdokumentmodus | CORE, CMD |
| Kap. 2 | Line Editor, WYSIWYG, RAM-, Virtual- und Swapping-Editoren | Produktabgrenzung, Performance |
| Kap. 3 | Array-of-Lines, Fixed Buffer, Linked List | Pufferabstraktion und Large-File-Fähigkeit |
| Kap. 4 | Einfacher Editor | Samples und Lernpfad |
| Kap. 5 | FIRST-ED, Initialisierung, Hooks, Standardbefehle | Corebefehle, Status, Sample |
| Kap. 6 | UserCommand, neue Einzel- und Präfixbefehle, Makro-/Command-Language-Hooks | Command-System und Erweiterbarkeit |
| Kap. 7 | MicroStar, Menüs, erweiterte Befehle | UI und optionale Erweiterungen |
| Kap. 8 | Dispatcher, Menüs, Popups, Hintergrunddruck, Fehler, Status, Dirty Bit | UI, Fehler, Background, Dirty State |
| Kap. 9 | Text- und Fensterdatenstrukturen, Window Linking | Dokument-/Ansichtsmodell |
| Kap. 10 | Eingabe, Scheduler, Dispatcher, UserTask | Input, Commands, Cancellation, Background |
| Kap. 11 | Bildschirmaufbau, differenzielle Updates, Farben | Rendering, Performance, Themes |
| Kap. 12 | Navigation, Löschen, Textverarbeitung, Fenster, Blöcke, Dateien, Exit | Funktionale Anforderungen |
| Kap. 13 | Overlays und Speicheroptimierung | Nicht wörtlich übernommen; moderne Paketierung |
| Kap. 14 | Einbettung in Programme, Overlay/Chain | Bibliothek und Adapter |
| Kap. 15 | Historische Modul- und Dateistruktur | Modularitätsreferenz |
| Kap. 16 | Konstanten | Kompatibilitäts- und Traceability-Anhang |
| Kap. 17 | Datentypen | Modernes Datenmodell |
| Kap. 18 | Globale Variablen | Gekapselte Zustände und Dienste |
| Kap. 19 | Prozeduren und Funktionen | API-Traceability |

---

## 26. Historische Konstanten, Typen und Zustände

### 26.1 Konstanten

| Historisches Element | Bedeutung | Moderne Behandlung |
|---|---|---|
| `Col1`, `Line1` | Logische Startpositionen | Normale Positionsabstraktion |
| `Colored`, `Inblock`, `Wrapped` | Zeilenflags | Generische Annotationen, Auswahl und Wrap-Metadaten |
| `Ctrla` bis `Ctrlz`, `Del`, `Escape`, `Nul` | ASCII-Tastencodes | Key-Gesture- und Input-Abstraktion |
| `Defhelplen` | Hilfefensterhöhe | UI-Layoutkonfiguration |
| `Defnocols`, `Defnorows` | 80x25-Bildschirm | Nur Retro-Demo, kein Produktlimit |
| `Deftypahd` | Typeahead-Puffergröße | Konfigurierbare Queue oder Frameworkinput |
| `Errorfile` | `EDITERR.MSG` | Lokalisierbarer Fehlerkatalog |
| `Maxlinelength` | 255 Zeichen | Feste Grenze entfällt |
| `Maxmarker` | 20 Marker | Mindestens kompatibel, intern erweiterbar |
| `Nofile` | `NONAME` | Untitled-Dokumentzustand |
| `Screenadr` | Videospeicheradresse | Entfällt vollständig |
| Statuslabels | AI, Col, File, INS, Line, TE, Window, WW | Lokalisierbares Statusmodell |
| `Zero` bis `Nine` | ASCII-Ziffern | Normale Eingabeverarbeitung |

### 26.2 Datentypen

| Historischer Typ | Bedeutung | Moderne Behandlung |
|---|---|---|
| `Character` | Zeichen plus Farbe | Rendering-Glyph/Style im UI-Adapter |
| `Insflag` | Insert oder Typeover | EditorInsertMode |
| `Linedesc` / `Plinedesc` | Verketteter Zeilendeskriptor | Interne Pufferimplementierung |
| `Textline` / `Ptextline` | Text einer Zeile | Unicode-Zeilen- oder Span-Abstraktion |
| `Windesc` / `Pwindesc` | Fensterzustand und Textstromreferenz | EditorViewState plus DocumentReference |
| feste Pascal-Strings | String80, Varstring, Strvartype, St6 | `string`/Spans ohne historische Längengrenze |

### 26.3 Globale Variablen

Das Handbuch dokumentiert folgende globale Zustände:


- `Abortcmd`
- `Aborting`
- `Asking`
- `Blockcolor`
- `Blockfrom`
- `Blockhide`
- `Blockto`
- `Bordcolor`
- `Circbuf`
- `Circin`
- `Circout`
- `Cmdcol`
- `Cmdcolor`
- `Cmdlinest`
- `Curwin`
- `EditChangeflag`
- `EditUsercommandInput`
- `Interactive`
- `Intrflag`
- `Linelength`
- `Logscrcols`
- `Logscrrows`
- `Logtopscr`
- `Marker`
- `Nextstream`
- `Notfound`
- `Optstr`
- `Physcrcols`
- `Physcrrows`
- `Physcrsig`
- `Replacestr`
- `Retracemode`
- `Rundown`
- `Screen`
- `Searchstr`
- `Tabsize`
- `Txtcolor`
- `Typbufovl`
- `Undocount`
- `Undoend`
- `Undolimit`
- `Undostack`
- `Updcurflag`
- `Usercolor`
- `Window1`
- `Winstack`

Diese Zustände werden nicht als globale Variablen reproduziert. Ihre Verantwortlichkeiten verteilen sich auf Dokument, Ansicht, Sitzung, Command-Kontext, Suchdienst, Undo/Redo, Theme/Rendering, Eingabequeue sowie Host- und Cancellation-Dienste.

---

### 26.4 Historische Pflicht-Hooks und MicroStar-Erweiterungspunkte

Das Handbuch verlangt beziehungsweise demonstriert zusätzlich zu den Toolbox-Routinen mehrere vom Host bereitzustellende Hooks. Auch diese werden in der modernen Architektur ausdrücklich berücksichtigt:

| Historischer Hook / Erweiterungspunkt | Aufgabe im Handbuch | Moderne Behandlung |
|---|---|---|
| `UserCommand` | Eingaben filtern, umbelegen, selbst verarbeiten oder unterdrücken | Command Interceptor, Tastaturprofile und registrierbare Hostcommands |
| `UserError` | Fehler selbst darstellen oder an Standardbehandlung weiterreichen | Austauschbarer Error-/Notification-Service |
| `UserStatusLine` | Standardstatus ersetzen oder ergänzen | Statusmodell, Events und hostseitige Statuskomponente |
| `UserReplace` | Interaktive Entscheidungen während Find/Replace beeinflussen | Ersetzungsstrategie, Callback oder Prompt-Service |
| `UserTask` | Kooperative Hintergrundarbeit im Leerlauf | Tasks, CancellationToken, Dispatcher und Hostdienste |
| `MakeWindow` / `RestoreWindow` | Popup zeichnen und darunterliegenden Bildschirm sichern | Plattformspezifische Dialoge, Flyouts oder Overlays |
| `PulldownMenu` / `InitMainMenu` | Menüdefinition und Befehlsauslösung | Menüs auf gemeinsamem Command-Katalog |
| `PrintNext` | Inkrementeller Hintergrunddruck | Optionaler Print-Service / Sample |
| `ErrorCheck` | MicroStar-spezifische Fehlerdarstellung | Hostdefinierter Error Presenter |
| `SecondChar` | Zweites Zeichen einer Präfixsequenz abfragen | Key-Chord-/Sequence-Resolver |
| `ReadBlock` / Block Write | Block als Datei einlesen oder schreiben | Optionale Auswahl-Import-/Exportfunktion |
| `SpellingCheck` | Rechtschreibprüfung | Späterer externer Sprachdienst, nicht Core-Pflicht |

### 26.5 Historische Modul- und Dateiinventur

Die ursprüngliche Modularisierung wird nicht als Include-Dateistruktur übernommen, aber als Verantwortlichkeitsnachweis festgehalten.

| Historische Datei/Gruppe | Verantwortung | Moderne Zielzuordnung |
|---|---|---|
| `VARS.ED`, `VARS.MS` | Konstanten, Typen, globale Variablen | Gekapselte Core-Modelle, Optionen und Zustände |
| `USER.ED`, `USER.MS` | Niedrige Kernroutinen | Core/Text Buffer/Document Operations |
| `SCREEN.ED`, `SCREEN.MS` | Bildschirmdarstellung | WinForms-, WPF- oder Web-Renderingadapter |
| `INIT.ED`, `INIT.MS` | Initialisierung | Composition Root, Factory und Dependency Injection |
| `CMD.ED`, `CMD.MS`, `FASTCMD.MS` | Einzelbefehle | Command-Implementierungen |
| `KCMD.*`, `OCMD.*`, `QCMD.*` | Präfixbefehle | Command-Katalog und Key-Chord-Profile |
| `K.*`, `O.*`, `Q.*`, `DISP.*` | Dispatcher | Command Dispatcher / Input Mapping |
| `TASK.*` | Scheduler und Hauptschleife | Host-Eventloop, Tasks und Cancellation |
| `INPUT.*` | Tastatur und Typeahead | Plattform-Inputadapter |
| `EDITERR.MSG` | Fehlerkatalog | Lokalisierte Ressourcen / strukturierte Fehler |
| `FIRST-ED.PAS` | Einfaches Beispiel | Modern-FIRST-ED-Sample |
| `MS.PAS` und `MS.*` | Sophisticated Editor / MicroStar | Erweiterte Demo beziehungsweise Showcase |
| `PRINT.MS` | Hintergrunddruck | Optionaler Print-Service |
| `MSCMD.MS` | MicroStar-spezifische Befehle | Sample-/Hostcommands |
| `SPELL.MS` | Rechtschreibprüfung | Optionale externe Erweiterung |
| `PULLDOWN.MS` | Pulldown-Menüs | UI-Adapter und Command-Menüs |
| `.OVL` und Chain-/Overlay-Dateien | Speicherüberlagerung | Entfällt; Assemblies und normale Laufzeitverwaltung |
| `README.COM`, `READ.ME` | Versions- und Distributionshinweise | README, CHANGELOG und Release Notes |


---

## 27. Historische Prozeduren- und Funktionsinventur

Die folgende Inventur übernimmt die im Handbuchindex aufgeführten Routinen als Traceability-Liste. Sie bedeutet **nicht**, dass die öffentliche C#-API dieselben Namen oder dieselbe Granularität erhalten muss. Im Pflichtenheft wird jede Routine einer modernen API, einem internen Mechanismus, einer optionalen Erweiterung oder einer bewussten Nichtübernahme zugeordnet.


- `Advance`
- `EditAbort`
- `EditAppchar`
- `EditAppcmdnam`
- `EditAskfor`
- `EditBackground`
- `EditBeginningEndLine`
- `EditBeginningLine`
- `EditBlockBegin`
- `EditBlockCopy`
- `EditBlockDelete`
- `EditBlockEnd`
- `EditBlockHide`
- `EditBlockMove`
- `EditBottomBlock`
- `EditBreathe`
- `EditCenterLine`
- `EditChangeCase`
- `EditClsinp`
- `EditColorFile`
- `EditColorLine`
- `EditCompressLine`
- `EditCpcrewin`
- `EditCpdelwin`
- `EditCpexit`
- `EditCpFileSave`
- `EditCpFind`
- `EditCpgotocl`
- `EditCpgotoln`
- `EditCpgotowin`
- `EditCpjmpmrk`
- `EditCplnkwin`
- `EditCpReplace`
- `EditCprfw`
- `EditCpsetlm`
- `EditCpsetmrk`
- `EditCpsetrm`
- `EditCptabdef`
- `EditCpundlim`
- `EditCpwfw`
- `EditCvts2i`
- `EditDecline`
- `EditDefineTab`
- `EditDeleteLeftChar`
- `EditDeleteLine`
- `EditDeleteRightChar`
- `EditDeleteRightWord`
- `EditDeleteTextRight`
- `EditDelline`
- `EditDestxtdes`
- `EditDownLine`
- `EditDownPage`
- `EditEndLine`
- `EditErrormsg`
- `EditExit`
- `EditFileRead`
- `EditFileWrite`
- `EditFind`
- `EditGenlineno`
- `EditGotoColumn`
- `EditGotoLine`
- `EditHscroll`
- `EditIncline`
- `EditInitialize`
- `EditInsertCtrlChar`
- `EditInsertLine`
- `EditJoinLine`
- `EditJumpMarker`
- `EditK`
- `EditLeftChar`
- `EditLeftWord`
- `EditLongLine`
- `EditMarkblock`
- `EditNewLine`
- `EditO`
- `EditOffblock`
- `EditPrccmd`
- `EditPrctxt`
- `EditPushtbf`
- `EditQ`
- `EditRealign`
- `EditReatxtfil`
- `EditReformat`
- `EditReplace`
- `EditRightChar`
- `EditRightWord`
- `EditSchedule`
- `EditScrollDown`
- `EditScrollUp`
- `EditSetLeftMargin`
- `EditSetMarker`
- `EditSetRightMargin`
- `EditSetUndoLimit`
- `EditShiftLine`
- `EditShortLine`
- `EditSystem`
- `EditTab`
- `EditToggleAutoindent`
- `EditToggleInsert`
- `EditToggleWordwrap`
- `EditTopBlock`
- `EditUndo`
- `EditUpcase`
- `EditUpdphyscr`
- `EditUpdrowasm`
- `EditUpdwindow`
- `EditUpdwinsl`
- `EditUpLine`
- `EditUpPage`
- `EditUserpush`
- `EditWindowBottomFile`
- `EditWindowCreate`
- `EditWindowDelete`
- `EditWindowDeleteText`
- `EditWindowDown`
- `EditWindowGoto`
- `EditWindowLink`
- `EditWindowTopFile`
- `EditWindowUp`
- `EditZapcmdnam`
- `MoveFromScreen`
- `MoveToScreen`
- `Pokechr`

---

### 27.1 Bewusst nicht wörtlich zu übernehmende Routinen

- `MoveFromScreen`, `MoveToScreen`, `EditUpdrowasm`  
  Direkter Videospeicherzugriff entfällt; moderne UI-Frameworks übernehmen Rendering.

- `EditSchedule`, `EditSystem`, `UserTask` in ihrer DOS-Polling-Form  
  Werden durch Eventloops, Tasks, Dispatcher und Cancellation ersetzt.

- `Pokechr`, Typeahead-Ringpuffer und rohe ASCII-Scan-Codes  
  Werden durch Inputadapter und frameworkunabhängige Key Gestures ersetzt.

- Overlay- und Chain-Mechanismen  
  Werden durch Assemblies, Pakete, Lazy Loading und normale Speicherverwaltung ersetzt.

- globale Pointerlisten auf Zeilen und Fenster  
  Werden durch gekapselte Dokument-, Puffer- und Ansichtstypen ersetzt.

---

## 28. Rechtliche und historische Abgrenzung

Das Handbuch weist Borland International Inc. als Copyright-Inhaber des Dokuments von 1985 aus. Das SASD-Projekt soll eine **eigenständige moderne Neuimplementierung der beschriebenen Ideen und Funktionen** sein.

Daraus folgen:

- keine ungeprüfte Übernahme historischen Quellcodes,
- keine Veröffentlichung gescannter Handbuchinhalte im Repository,
- keine Übernahme von Produktlogo, Covergestaltung oder Werbetexten,
- Prüfung der Verwendung historischer Marken in Projektname und Marketing,
- klare Kennzeichnung als unabhängiges SASD-Projekt,
- separate Lizenz für den neu geschriebenen Code,
- Quellenangabe im historischen Hintergrunddokument, ohne den Eindruck einer offiziellen Borland-Nachfolge.

Dieses Lastenheft ist keine Rechtsberatung. Vor öffentlicher Vermarktung oder markennaher Benennung ist eine gesonderte Rechteprüfung erforderlich.

---

## 29. Definition of Done für das Lastenheft

Das Lastenheft gilt als freigabefähig, wenn:

- Produktvision und Abgrenzung bestätigt sind,
- MUSS-/SOLL-/KANN-Prioritäten geprüft wurden,
- Repository-Entscheidung bestätigt ist,
- UI-Adapter-Reihenfolge bestätigt ist,
- offene Rechts- und Namensfragen erfasst sind,
- alle historischen Kapitel und Befehlskategorien in der Traceability erscheinen,
- die MVP-Grenze bestätigt ist,
- anschließend ein Pflichtenheft mit Architektur, Projektstruktur, APIs, Datenstrukturen, Technologieentscheidungen und Testfällen erstellt werden kann.

---

## 30. Zusammenfassende Freigabeempfehlung

Das Projekt ist fachlich sinnvoll und realistisch, wenn es nicht als schneller WinForms-Wrapper, sondern als schrittweise entwickelte Editor-Bibliothek verstanden wird.

Die verbindliche Grundrichtung lautet:

1. eigenständiges SASD Editor Toolkit,
2. UI-unabhängiger Core,
3. WinForms als erste Referenzoberfläche,
4. vollständige moderne Abbildung der historischen Kernfunktionen,
5. WPF und Web erst nach Stabilisierung des Kerns,
6. Desktop Components als Verbraucher und Showcase,
7. andere Toolboxes als getrennte Produkte,
8. historische Traceability ohne technische 1:1-Portierung,
9. schlanker MVP statt frühem Over-Engineering,
10. saubere Dokumentation, Tests und langfristig stabile API.

Auf dieser Grundlage kann als nächstes das **Pflichtenheft und Architekturdokument für Version 0.1 - Modern FIRST-ED** erstellt werden.
