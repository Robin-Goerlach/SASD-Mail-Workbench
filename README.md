# SASD Mail Workbench

SASD Mail Workbench ist eine lokal arbeitende, technisch nachvollziehbare
Mail- und Analyseplattform. Das Projekt wird Windows-first entwickelt und
beginnt mit .NET 8. Die erste grafische Oberfläche folgt in Milestone 0.4.0
als Windows-Forms-Adapter; Domain, Application und Persistenz bleiben davon
unabhängig.

## Aktueller Stand

**Version 0.3.1 – Stabilisierung und Build-Basis**

Dieser Repository-Stand implementiert noch keinen POP3-, SMTP- oder
IMAP-Zugriff. Er schafft zuerst die belastbare Grundlage:

- streambasierte Verarbeitung unveränderter Rohmail-Bytes,
- SHA-256 plus Byte-Länge als exakter Fingerprint,
- temporäres Staging und atomare Dateiübernahme,
- SQLite-Katalog mit versionierten Migrationen,
- Import-Zustandsmaschine und Wiederanlauf,
- relative Speicherpfade,
- stabile Processor-/Analyzer-Verträge,
- reguläre NUnit-Testprojekte,
- GitHub-Actions-Workflow und vollständige Projektdokumentation.

## Geplante Alleinstellungsmerkmale

- POP3 `Get all` mit automatischer Deduplizierung,
- unverändertes lokales Rohmailarchiv,
- SMTP-Versand und IMAP-Upload in den Gesendet-Ordner,
- lokales, erklärbares Warnsystem für Links, Header und Anhänge,
- Message Laboratory, Provenienz und reproduzierbare Analysis Runs,
- spätere Erweiterbarkeit ohne UI- oder Plugin-Zwang im Kern.

## Voraussetzungen

- Visual Studio 2022 17.14 oder aktueller mit Workload **.NET-Desktopentwicklung**
- .NET 8 SDK 8.0.422
- PowerShell 7 empfohlen

## Schnellstart

```powershell
./scripts/restore.ps1
./scripts/build.ps1
./scripts/test.ps1
./scripts/run-demo.ps1
```

Alternativ:

```powershell
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build --logger "trx;LogFileName=test-results.trx"
```

Beim ersten Restore werden `packages.lock.json`-Dateien erzeugt. Diese sollen
anschließend committed werden. Danach kann `./scripts/verify.ps1 -LockedMode`
für einen vollständig gesperrten Restore verwendet werden.

## Solution-Struktur

```text
src/
  Sasd.MailWorkbench.Domain
  Sasd.MailWorkbench.Application.Contracts
  Sasd.MailWorkbench.Application
  Sasd.MailWorkbench.ExtensionModel
  Sasd.MailWorkbench.Infrastructure
  Sasd.MailWorkbench.Persistence
  Sasd.MailWorkbench.Bootstrap.Console

tests/
  Sasd.MailWorkbench.Domain.Tests
  Sasd.MailWorkbench.Application.Tests
  Sasd.MailWorkbench.Infrastructure.Tests
  Sasd.MailWorkbench.Persistence.Tests
  Sasd.MailWorkbench.Architecture.Tests
```

## Wichtige Grenzen

- Keine Zugangsdaten im Repository.
- Keine Servermail wird durch lokale Deduplizierung gelöscht.
- Rohmails werden nicht als Text normalisiert, bevor ihr kanonischer Hash
  berechnet wird.
- Das aktuelle Repository ist nicht unter einer Open-Source-Lizenz freigegeben.
- Der generierte Stand benötigt einen ersten Build auf einem Rechner mit dem
  angegebenen .NET-SDK; Details stehen in `docs/development/BUILD-VERIFICATION.md`.

## Dokumentation

- [Projektstatus](PROJECT-STATUS.md)
- [Roadmap](ROADMAP.md)
- [Milestone 0.3.1](docs/milestones/MILESTONE-0.3.1.md)
- [Architektur](docs/architecture/ARCHITECTURE.md)
- [Datenintegrität und Recovery](docs/architecture/IMPORT-AND-RECOVERY.md)
- [Code-Walkthrough 0.3.1](docs/development/CODE-WALKTHROUGH-0.3.1.md)
- [Build-Hinweis](BUILD-NOTICE.md)
- [Qualitätsreview](docs/quality/QUALITY-REVIEW-2026-07-12.md)
- [GitHub-Veröffentlichung](docs/development/GITHUB-PUBLISHING.md)

## Lizenzstatus

Der Repository-Stand ist derzeit für die private Entwicklung der SASD GmbH
vorbereitet. Siehe [LICENSE](LICENSE).
