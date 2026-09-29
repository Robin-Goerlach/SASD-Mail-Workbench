# Entwicklung und Verifikation

## Integration

```powershell
./scripts/integrate-priority-mail-watcher.ps1
```

Das Skript ergänzt die Projekte idempotent in der vorhandenen Solution und führt aus:

```powershell
dotnet clean
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

## Debugging des Tray-Hosts

Startprojekt:

```text
Sasd.MailWorkbench.Host.Tray
```

Der Host startet zunächst pausiert. Im Tray-Menü kann die Überwachung gestartet oder ein Prüflauf ausgelöst werden. Da der Bootstrap-Adapter keine Nachrichten liefert und noch keine Regeln persistiert, sind in diesem Stand keine echten Mailtreffer zu erwarten.

## Nächste technische Implementierung

Die nächste größere Implementierung sollte erst nach Prüfung der vorhandenen Mail-Workbench-Abstraktionen beginnen. Zu klären sind insbesondere:

1. neutrale Darstellung eines IMAP-Neuereignisses,
2. Ownership einer langlebigen IMAP-Verbindung,
3. Koordination mit normalem Abruf und Workbench-Prozess,
4. persistenter Cursor aus Ordner, UIDVALIDITY und UID,
5. atomare SQLite-Reservierung der Benachrichtigung.

Erst danach wird ein produktiver `IMailObservationSource` implementiert.

## Qualitätsregeln

- Keine MailKit-Typen außerhalb des konkreten Mailadapters.
- Keine SQL-Typen außerhalb des SQLite-Adapters.
- Keine WinForms-Typen im fachlichen Kern.
- Keine Secrets in Datenbank, Konfiguration, Logs oder Tests.
- Jede neue Regelbedingung benötigt Domain- und Application-Tests.
- Fehlercodes und Zustände müssen stabil und protokollierbar bleiben.
