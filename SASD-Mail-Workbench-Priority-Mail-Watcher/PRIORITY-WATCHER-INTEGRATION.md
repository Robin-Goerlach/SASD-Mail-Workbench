# SASD Priority Mail Watcher – Integrationspaket

Dieses Paket ergänzt das bestehende Repository **SASD-Mail-Workbench** um die technische Grundlage des **SASD Priority Mail Watcher**.

## Inhalt

Das Paket fügt folgende Projekte hinzu:

```text
src/
├── Sasd.MailWorkbench.Notification.Domain
├── Sasd.MailWorkbench.Notification.Contracts
├── Sasd.MailWorkbench.Notification.Application
├── Sasd.MailWorkbench.Notification.Infrastructure
└── Sasd.MailWorkbench.Host.Tray

tests/
├── Sasd.MailWorkbench.Notification.Domain.Tests
├── Sasd.MailWorkbench.Notification.Application.Tests
└── Sasd.MailWorkbench.Notification.Architecture.Tests
```

Außerdem werden die technische Dokumentation unter `docs/priority-mail-watcher/` sowie ADR-016 ergänzt.

## Integration

1. Den Inhalt dieses ZIP-Archivs in das Wurzelverzeichnis von `SASD-Mail-Workbench` entpacken.
2. PowerShell im Repository-Wurzelverzeichnis öffnen.
3. Das Integrationsskript ausführen:

```powershell
./scripts/integrate-priority-mail-watcher.ps1
```

Das Skript trägt die neuen Projekte idempotent in `Sasd.MailWorkbench.sln` ein und führt anschließend standardmäßig Restore, Build und Tests aus.

Da das Hauptrepository zentrale Paketversionen und Lockfiles verwendet, entstehen beim ersten erfolgreichen Restore zusätzliche `packages.lock.json`-Dateien in den neuen Testprojekten. Diese Dateien müssen geprüft und zusammen mit der Integration committed werden.

Nur die Solution aktualisieren:

```powershell
./scripts/integrate-priority-mail-watcher.ps1 -SkipVerification
```

## Wichtige technische Abgrenzung

Dieser Stand ist bewusst eine **kompilierbare Architekturgrundlage**, noch keine produktiv nutzbare Mailüberwachung.

Noch nicht enthalten sind:

- produktiver IMAP-IDLE- oder Polling-Adapter,
- POP3-Fallback,
- SQLite-Persistenz der Regeln und Benachrichtigungszustände,
- Credential-Zugriff,
- echte Windows App Notifications,
- Einstellungsdialog und Regelverwaltung.

Der Tray-Host verwendet vorerst einen leeren Mailbeobachtungsadapter, In-Memory-Repositories und eine Balloon-Benachrichtigung als klar gekennzeichneten UI-Fallback. Es wird keine zweite Mailprotokollimplementierung eingeführt.

## Framework-Hinweis

Das vorhandene Repository verwendet aktuell .NET 8 und Visual Studio 2022. Deshalb erbt dieser Integrationsstand für die Bibliotheken `net8.0`; der Tray-Host verwendet `net8.0-windows`.

Die spätere Migration auf .NET 10 muss als eigener technischer Commit erfolgen. .NET 10 wird von Visual Studio 2022 nicht offiziell als Target Framework unterstützt; dafür ist Visual Studio 2026 oder ein CLI-basierter Build mit dem .NET-10-SDK erforderlich. Eine Frameworkmigration wurde absichtlich nicht mit der Watcher-Architektur vermischt.
