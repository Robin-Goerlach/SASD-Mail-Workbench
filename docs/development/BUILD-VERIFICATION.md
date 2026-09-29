# Build-Verifikation

## Transparenter Status

Am 23. September 2026 wurde die Hauptsolution unter Windows mit .NET SDK
8.0.425 erfolgreich wiederhergestellt, in Release gebaut und getestet:
21 Tests bestanden, keine fehlgeschlagen oder übersprungen. Der Build enthält
noch Analyzer-Warnungen. Dies war kein Clean-Machine-Test; Linux/CI und das
separate Priority-Mail-Watcher-Paket wurden nicht validiert.

Verwendete Befehle:

```powershell
dotnet restore Sasd.MailWorkbench.sln --locked-mode
dotnet build Sasd.MailWorkbench.sln --configuration Release --no-restore --disable-build-servers -m:1
dotnet test Sasd.MailWorkbench.sln --configuration Release --no-build -m:1 --logger trx --results-directory artifacts/test-results
```

Die lokale Ausführungsrichtlinie blockiert unsignierte PowerShell-Skripte.
Deshalb wurden die CLI-Befehle direkt ausgeführt; die Skriptkette wurde hier
nicht zur Laufzeit geprüft. Die Richtlinie wurde nicht verändert.
Ein einzelner MSBuild-Knoten vermeidet die lokal beobachteten Probleme beim
Start paralleler Build-Prozesse. TRX erhält automatisch eindeutige Dateinamen,
damit die fünf Testprojekte ihre Ergebnisse nicht gegenseitig überschreiben.

## Windows-Prüfung

```powershell
dotnet --info
./scripts/verify.ps1
```

Danach:

1. alle erzeugten `packages.lock.json` prüfen und committen,
2. `./scripts/verify.ps1 -LockedMode` ausführen,
3. GitHub Actions prüfen,
4. etwaige Compilerbefunde in kleinen Korrekturcommits beheben.
