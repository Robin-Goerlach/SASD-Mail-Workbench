# Changelog

Alle wesentlichen Änderungen werden in diesem Dokument festgehalten.

## [Unreleased]

### Fixed

- interne Berichtszähler für die Application-Assembly freigegeben,
- mehrdeutige NUnit-Delegates für den .NET-8-Build explizit typisiert,
- Restore-, Build- und Testskripte brechen bei fehlgeschlagenen dotnet-Aufrufen ab,
- separate TRX-Dateien statt überschriebener Testergebnisse.

### Added

- vorbereitete GitHub-Repository-Struktur,
- CI-Workflow und GitHub-Vorlagen.

## [0.3.1] - 2026-07-12

### Added

- konsolidierte Solution unter `Sasd.MailWorkbench.*`,
- streambasierter SHA-256-Fingerprint,
- Rohmail-Staging und atomare Übernahme,
- SQLite-Migrationen und Importkatalog,
- Import- und Recovery-Use-Cases,
- stabile Extension Contracts,
- NUnit-Testprojekte und Architekturtests,
- Projekt-, Architektur- und Entwicklungsdokumentation.

### Changed

- .NET 8 bleibt Übergangsbasis bis einschließlich 0.4.0.
- Produktname wurde auf SASD Mail Workbench vereinheitlicht.

### Removed

- eigener, nicht standardisierter Test-Runner aus dem aktiven Projektstand.
