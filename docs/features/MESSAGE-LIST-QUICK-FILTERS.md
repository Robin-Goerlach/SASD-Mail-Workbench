# Schnellfilter in der Nachrichtenübersicht

Status: Beschlossene Produktanforderung für Version 1.0; Implementierung noch offen.  
Stand: 2026-11-06

## Ziel und Bedienkonzept

Die tabellarische Nachrichtenübersicht erhält unmittelbar unter den Spaltenüberschriften eine ein- und ausblendbare Filterzeile. Jede filterbare Spalte besitzt ein eigenes Eingabefeld. Änderungen schränken die angezeigten Nachrichten ohne zusätzlichen Suchdialog ein. Alle gleichzeitig aktiven Spaltenfilter werden mit logischem **UND** verknüpft. Die Sortierung durch Klick auf Spaltenköpfe bleibt unabhängig von der Filterung.

Beispiel: Datum `06.11.2026`, Absender `info@`, Betreff `Rechnung` zeigt ausschließlich Nachrichten, die alle drei Bedingungen erfüllen.

**Geltungsbereich V1:** nur der aktuell ausgewählte Mailordner. Konto-, ordner- oder kontenübergreifende Suche ist eine separate spätere Funktion.

## Funktionale Anforderungen für V1.0

| ID | Anforderung |
| --- | --- |
| SF-01 | Filterzeile unmittelbar unter den Spaltenüberschriften, ein-/ausblendbar. |
| SF-02 | Textfilter für Absender und Betreff; Teilzeichenfolgenabgleich, standardmäßig ohne Unterscheidung von Groß-/Kleinschreibung. |
| SF-03 | Datumsfilter: exakter Tag sowie inklusive Von-/Bis-Grenzen. |
| SF-04 | Mehrere aktive Filter sind UND-verknüpft. |
| SF-05 | Sortierung und Filterung sind unabhängig kombinierbar. |
| SF-06 | Einzelne Filter und alle Filter gemeinsam können zurückgesetzt werden. |
| SF-07 | Statusanzeige „N von M Nachrichten angezeigt“ für den aktuellen Ordner. |
| SF-08 | Filtereingaben werden validiert; fehlerhafte Werte werden kenntlich gemacht und führen nicht zu unbemerkten falschen Ergebnissen. |
| SF-09 | Aktualisierung während der Eingabe mit kurzer Entprellzeit (Richtwert 250 ms); keine Blockade des UI-Threads. |
| SF-10 | Ordnerwechsel aktualisiert den Abfragekontext; Filter gelten nicht stillschweigend für andere Ordner. Verhalten beim Wechsel (zurücksetzen oder je Ordner merken) ist vor Umsetzung festzulegen. |

### Vorgeschlagene Eingabesyntax

- Datum: `06.11.2026` (exakt), `>=01.11.2026` (ab), `01.11.2026..06.11.2026` (inklusiver Bereich).
- Text: `info@`, `Rechnung` (enthält).
- Optionale spätere Filter: Uhrzeit, Größe (`>5MB`), Anhang vorhanden, gelesen/ungelesen.

Syntax, lokale Datumsdarstellung, Zeitzonenbehandlung und Escape-Regeln werden im technischen Design eindeutig spezifiziert und getestet. Für einen Kalendertag sind die lokalen Tagesgrenzen korrekt in die Zeitbasis des Index zu übersetzen.

## Architekturleitlinien

Die Filterlogik darf nicht in der WinForms-Ansicht liegen:

1. **Presentation / MailListView:** Spalten, Eingabefelder, Anzeige von Validierungsfehlern und Ergebnissen.
2. **Application / Filtermodell und Controller:** typisierte Filterkriterien, Parser, Validierung, UND-Verknüpfung, Debouncing/Cancellation.
3. **Application / Query-Service:** filter- und sortierbare, paginierte Abfragen über einen definierten Vertrag.
4. **Persistence / Repository:** effiziente parametrisierte Abfragen gegen den lokalen Nachrichtenindex (SQLite); keine SQL-Stringverkettung aus Benutzereingaben.

Die Komponentenbezeichnungen sind konzeptionell; vorhandene Schichten und Namenskonventionen des Projekts haben Vorrang. Eine spätere IMAP-Suche kann über einen Adapter erfolgen, ohne die lokale Filtersemantik zu verändern. Kein erzwungener generischer Tabellenframework-Umbau in V1.

## Abnahmekriterien

1. Filter auf einen Tag zeigt ausschließlich Nachrichten dieses lokalen Kalendertags.
2. Kombination aus Datum, Absender und Betreff liefert genau die Schnittmenge.
3. Entfernen eines Kriteriums erweitert das Ergebnis korrekt; „Alle zurücksetzen“ zeigt den vollständigen aktuellen Ordner.
4. Sortierung bleibt bei aktiven Filtern stabil und korrekt; Pagination liefert keine Duplikate oder Auslassungen.
5. Ungültige Datums- und Bereichseingaben werden sichtbar zurückgewiesen.
6. Schnelles Tippen und Wechseln des Ordners erzeugen keine veralteten Ergebnisse oder UI-Blockaden.
7. Ergebniszähler stimmt mit den gefilterten und gesamten Nachrichten des aktuellen Ordners überein.
8. Automatisierte Tests decken Parser, Grenzwerte, kombinierte Filter und Repository-Abfragen ab.

## Spätere Ausbaustufen

- **Nach V1:** Größen-/Anhangs-/Statusfilter, gespeicherte Filter und virtuelle Ordner.
- **V2-Kandidaten:** komplexe UND/ODER-Gruppen, erweiterte Filterausdrücke und kontoübergreifende Suche.

Diese Ausbaustufen sind nicht Bestandteil der verbindlichen V1-Abnahme.
