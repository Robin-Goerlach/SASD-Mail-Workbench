# ADR-008: Extension Contracts vor dynamischem Plugin-Laden

**Status:** Akzeptiert  
**Datum:** 12. Juli 2026  
**Gültig ab:** Planung 0.4.0

## Kontext

Spätere Analyzer, Prozessoren und Exporter sollen ohne Totalumbau ergänzt werden.

## Entscheidung

Kernmodule verwenden stabile IDs, Versionen und kleine Schnittstellen aus ExtensionModel. V1 lädt keine unbekannten DLLs dynamisch und bietet keine offene UI-Plugin-API.

## Konsequenzen

- eingebaute Module testen die Erweiterungsarchitektur
- Ergebnisse werden als versionierte neutrale Daten gespeichert
- keine CLR-Typnamen als fachliche Datenbankkopplung

## Verworfene oder verschobene Alternativen

- Plugin-Lader sofort: zu frühe Sicherheits- und Versionskomplexität
- keine Erweiterungsschnittstellen: späterer Architekturbruch

## Rückverfolgbarkeit

Die Entscheidung ist bei Änderungen in Lastenheft, Pflichtenheft, Roadmap, Entwicklerhandbuch und Rückverfolgbarkeitsmatrix zu berücksichtigen.
