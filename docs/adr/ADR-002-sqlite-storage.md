# ADR-002: SQLite für Metadaten und Arbeitszustände

**Status:** Akzeptiert  
**Datum:** 12. Juli 2026  
**Gültig ab:** Planung 0.4.0

## Kontext

Der Client benötigt lokale, transaktionale, durchsuchbare und sicherbare Metadatenhaltung.

## Entscheidung

SQLite wird für Kontometadaten, Nachrichtenzustände, Provenienz, Analyseergebnisse, Queues, Tags und Suchindex verwendet.

## Konsequenzen

- lokaler Betrieb ohne Server
- Migrationen und Transaktionen sind Pflicht
- Rohmailbytes werden nicht ausschließlich als Datenbank-BLOB behandelt

## Verworfene oder verschobene Alternativen

- JSON-Dateien als Hauptkatalog: unzureichend für Transaktionen und Suche
- externer Datenbankserver: unnötige Betriebsabhängigkeit

## Rückverfolgbarkeit

Die Entscheidung ist bei Änderungen in Lastenheft, Pflichtenheft, Roadmap, Entwicklerhandbuch und Rückverfolgbarkeitsmatrix zu berücksichtigen.
