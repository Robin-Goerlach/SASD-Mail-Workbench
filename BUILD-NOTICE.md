# Build-Hinweis

Der Quellstand wurde statisch auf Projektverweise, XML-Struktur, Pfade und offensichtliche Syntaxprobleme geprüft.

Am 23. September 2026 wurden Locked-Restore, Release-Build und alle 21 Tests
unter Windows mit SDK 8.0.425 erfolgreich ausgeführt. Analyzer-Warnungen sowie
die unabhängige Clean-Machine-/CI-Prüfung bleiben offen. Prüfbefehle und
Umgebungsgrenzen stehen in `docs/development/BUILD-VERIFICATION.md`.

Auf einem Windows-Rechner mit Visual Studio 2022 und dem in `global.json` angegebenen .NET-8-SDK bitte ausführen:

```powershell
./scripts/verify.ps1
```

Beim ersten Restore werden die NuGet-Lockdateien erzeugt. Danach:

```powershell
git add "**/packages.lock.json"
git commit -m "build: lock restored NuGet dependency graph"
./scripts/verify.ps1 -LockedMode
```
