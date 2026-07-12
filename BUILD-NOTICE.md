# Build-Hinweis

Der Quellstand wurde statisch auf Projektverweise, XML-Struktur, Pfade und offensichtliche Syntaxprobleme geprüft.

In der Erstellungsumgebung war kein .NET-SDK installiert. Deshalb konnten `dotnet restore`, `dotnet build` und `dotnet test` dort nicht ausgeführt werden.

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
