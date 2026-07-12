# ADR-003: Unveränderter Rohmail-Dateispeicher

**Status:** Akzeptiert  
**Datum:** 12. Juli 2026  
**Gültig ab:** Planung 0.4.0

## Kontext

Parsing und Analyzer müssen wiederholbar sein, ohne Originale zu verändern oder neu abzurufen.

## Entscheidung

Originale RFC-822-/MIME-Bytes werden kanonisch im Dateisystem gespeichert; SQLite enthält Referenz, Hash, Größe und Provenienz.

## Konsequenzen

- Reprocessing möglich
- Backup muss Datenbank und Rohmailbestand konsistent sichern
- abgeleitete Texte dürfen das Original nie überschreiben

## Verworfene oder verschobene Alternativen

- nur geparste Felder speichern: Informationsverlust
- Rohmails nur in SQLite-BLOBs: erschwert Diagnose und große Dateien

## Rückverfolgbarkeit

Die Entscheidung ist bei Änderungen in Lastenheft, Pflichtenheft, Roadmap, Entwicklerhandbuch und Rückverfolgbarkeitsmatrix zu berücksichtigen.
