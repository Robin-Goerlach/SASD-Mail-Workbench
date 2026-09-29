# Build-Hinweis

Der aktuelle Hauptstand wurde statisch und durch automatisierte Builds geprüft.

Am 23. September 2026 wurden Locked-Restore, Release-Build und alle 21 Tests
unter Windows mit SDK 8.0.425 erfolgreich ausgeführt.

Am 29. September 2026 wurde zusätzlich GitHub Actions für Commit
`7109775960ca1770602c2746f0bc2432c47fc635` erfolgreich auf
`windows-latest` und `ubuntu-latest` ausgeführt. Beide Jobs haben Restore,
Release-Build und alle fünf Testprojekte erfolgreich abgeschlossen. Insgesamt
bestanden 21 Tests; es gab 0 Fehler und 0 übersprungene Tests.

Die NuGet-Lockdateien sind für alle Projekte der Hauptsolution committed.
CI verwendet deshalb künftig den gesperrten Restoremodus und schlägt fehl,
wenn Projektdateien, zentrale Paketversionen und Lockfiles nicht
übereinstimmen.

Der Build enthält weiterhin Analyzer-Warnungen. Diese sind als technische
Schulden dokumentiert und werden nicht durch globale Warnungsunterdrückung
verdeckt. Details stehen in `docs/development/BUILD-VERIFICATION.md`.

Lokale Verifikation:

```powershell
./scripts/verify.ps1 -LockedMode
```

Falls lokale PowerShell-Ausführungsrichtlinien die Skripte blockieren:

```powershell
dotnet restore Sasd.MailWorkbench.sln --locked-mode
dotnet build Sasd.MailWorkbench.sln --configuration Release --no-restore
dotnet test Sasd.MailWorkbench.sln --configuration Release --no-build --logger trx --results-directory artifacts/test-results
```
