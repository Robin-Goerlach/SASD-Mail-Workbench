# Milestone 0.3.1 – Stabilisierung und Build-Basis

## Definition of Done

- [x] Solution und Namespaces konsolidiert
- [x] .NET-8-SDK gepinnt
- [x] zentrale Paketversionen vorbereitet
- [x] NUnit-Testprojekte angelegt
- [x] Rohmails werden bytegenau gestreamt
- [x] Staging und atomare Dateiübernahme implementiert
- [x] SQLite-Migrationen implementiert
- [x] Importzustände und Recovery implementiert
- [x] relative Speicherpfade verwendet
- [x] Extension Contracts vorbereitet
- [x] CI und Dokumentation angelegt
- [x] erster Restore erzeugt und committed Lockfiles
- [x] Release-Build und alle Tests auf Windows erfolgreich
- [x] GitHub Actions erfolgreich

## Abschlussnachweis

Milestone 0.3.1 ist technisch abgeschlossen.

Die Hauptsolution enthält committed `packages.lock.json`-Dateien. Ein lokaler
Windows-Lauf vom 23. September 2026 bestätigte Locked-Restore, Release-Build
und 21 erfolgreiche Tests. Der GitHub-Actions-Lauf für Commit
`7109775960ca1770602c2746f0bc2432c47fc635` bestätigte am 29. September 2026
Restore, Release-Build und alle 21 Tests zusätzlich auf `windows-latest` und
`ubuntu-latest`.

Noch vorhandene Analyzer-Warnungen sind dokumentierte technische Schulden und
kein offener Punkt dieser Definition of Done.
