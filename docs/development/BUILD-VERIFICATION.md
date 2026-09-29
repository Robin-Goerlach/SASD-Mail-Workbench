# Build-Verifikation

## Transparenter Status

### Lokale Windows-Prüfung vom 23. September 2026

Die Hauptsolution wurde unter Windows mit .NET SDK 8.0.425 erfolgreich
wiederhergestellt, in Release gebaut und getestet:

- Locked-Restore erfolgreich,
- Release-Build erfolgreich,
- 21 Tests bestanden,
- 0 Tests fehlgeschlagen,
- 0 Tests übersprungen.

Verwendete Befehle:

```powershell
dotnet restore Sasd.MailWorkbench.sln --locked-mode
dotnet build Sasd.MailWorkbench.sln --configuration Release --no-restore --disable-build-servers -m:1
dotnet test Sasd.MailWorkbench.sln --configuration Release --no-build -m:1 --logger trx --results-directory artifacts/test-results
```

Die lokale Ausführungsrichtlinie blockierte unsignierte PowerShell-Skripte.
Deshalb wurden die CLI-Befehle direkt ausgeführt; die Richtlinie wurde nicht
verändert. Ein einzelner MSBuild-Knoten vermied die lokal beobachteten Probleme
beim Start paralleler Build-Prozesse.

### GitHub-Actions-Prüfung vom 29. September 2026

Der CI-Lauf für Commit
`7109775960ca1770602c2746f0bc2432c47fc635` wurde auf beiden Matrixsystemen
erfolgreich abgeschlossen:

- `windows-latest`: Restore, Release-Build, Tests und TRX-Upload erfolgreich,
- `ubuntu-latest`: Restore, Release-Build, Tests und TRX-Upload erfolgreich,
- 21 Tests pro Betriebssystem bestanden,
- 0 Tests fehlgeschlagen,
- 0 Tests übersprungen.

Damit sind die drei letzten Definition-of-Done-Punkte von Milestone 0.3.1
nachweisbar erfüllt: Lockfiles sind committed, der Windows-Build ist bestätigt
und GitHub Actions ist erfolgreich.

## Analyzer-Befunde

Der erfolgreiche CI-Build meldet weiterhin Analyzer-Warnungen, jedoch keine
Compilerfehler. Relevante Produktionscode-Befunde sind derzeit unter anderem:

- CA1512 in `ExtensionDescriptor`,
- CA1859 in `ImportStateRules`,
- CA1716 an `IApplicationLogger.Error`,
- CA1859 in `RecoverInterruptedImportsUseCase`,
- CA1859 in `SqliteMigrationRunner`,
- CA1001 in `SqliteMessageImportRepository`.

Zusätzlich meldet CA1707 Unterstriche in den bewusst verhaltensbeschreibenden
NUnit-Testnamen.

Diese Warnungen werden nicht pauschal unterdrückt. Jede Korrektur soll
ursachenbezogen und getrennt von fachlich unabhängigen Großänderungen erfolgen.

## Verbindliche lokale Prüfung

Da Lockfiles committed sind, ist der gesperrte Restore der Normalfall:

```powershell
./scripts/verify.ps1 -LockedMode
```

Alternativ:

```powershell
dotnet restore Sasd.MailWorkbench.sln --locked-mode
dotnet build Sasd.MailWorkbench.sln --configuration Release --no-restore
dotnet test Sasd.MailWorkbench.sln --configuration Release --no-build --logger trx --results-directory artifacts/test-results
```

## Abgrenzung Priority Mail Watcher

Das Verzeichnis `SASD-Mail-Workbench-Priority-Mail-Watcher/` ist weiterhin
ein separates Integrationspaket. Die erfolgreiche Hauptsolution-CI bestätigt
dessen Quellprojekte und Tests nicht. Vor einer Integration sind die
paketeigenen Integrationshinweise zu befolgen und die zusätzlichen Projekte
separat zu bauen und zu testen.
