# Validierungsbericht des Integrationspakets

**Stand:** 24. Juli 2026  
**Paket:** SASD Priority Mail Watcher – Architecture Foundation

## Durchgeführte statische Prüfungen

- XML-Parsing aller acht neuen Projektdateien,
- Auflösung sämtlicher relativer `ProjectReference`-Pfade innerhalb des Integrationspakets,
- Prüfung der vorgesehenen Abhängigkeitsrichtung,
- Suche nach verbotenen WinForms-, MailKit-, MimeKit- und SQLite-Imports im fachlichen Kern,
- Prüfung auf absolute lokale Pfade, eingebettete Zugangsdaten und erzeugte Build-Artefakte,
- strukturelle Klammer- und Zeichenkettenprüfung aller C#-Quelldateien,
- Integritätsmanifest mit SHA-256 für alle Paketdateien.

## Nicht in der Erzeugungsumgebung ausführbar

In der zur Paketerzeugung verfügbaren Laufzeit war kein .NET-SDK installiert. Daher wurde kein erfolgreicher Compiler- oder Testlauf behauptet. Die verbindliche Verifikation erfolgt nach dem Entpacken im echten Repository durch:

```powershell
./scripts/integrate-priority-mail-watcher.ps1
```

Das Skript führt `dotnet clean`, `dotnet restore`, `dotnet build --configuration Release` und `dotnet test --configuration Release` aus und bricht bei jedem Fehler ab.

## Erwartete Folgeartefakte

Der erste Restore erzeugt aufgrund der Repository-Einstellung `RestorePackagesWithLockFile` Lockfiles für die neuen Testprojekte. Sie sind nach erfolgreicher Prüfung zu committen.
