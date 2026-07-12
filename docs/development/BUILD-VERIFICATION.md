# Build-Verifikation

## Transparenter Status

Der Repository-Inhalt wurde in einer Umgebung ohne installiertes .NET-SDK
erzeugt und konnte dort nicht kompiliert werden. XML-, Pfad-, Referenz- und
Repository-Prüfungen wurden statisch durchgeführt. Der erste reale Build ist
daher ein verpflichtendes Release-Gate und keine Formsache.

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
