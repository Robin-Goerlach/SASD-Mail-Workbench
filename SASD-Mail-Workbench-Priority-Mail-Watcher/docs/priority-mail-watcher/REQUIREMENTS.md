# Technische Anforderungen des Integrationsstands

## Erfüllt

| ID | Anforderung | Nachweis |
|---|---|---|
| PNW-ARC-001 | Der Watcher besitzt einen eigenen ausführbaren Tray-Host. | `Sasd.MailWorkbench.Host.Tray` |
| PNW-ARC-002 | Domain, Application, Infrastruktur und Host sind getrennt. | Projektstruktur und Architekturtests |
| PNW-ARC-003 | Der fachliche Kern referenziert weder WinForms noch MailKit oder SQLite. | Architekturtests |
| PNW-RUL-001 | Regeln unterstützen Konto, Absenderadresse, Domain, Betreff, Schlüsselwörter und Anhänge. | Domain-Modell und Unit Tests |
| PNW-RUL-002 | Werte einer Kategorie sind ODER-, Kategorien untereinander UND-verknüpft. | `PriorityMailRuleMatcher` |
| PNW-RUL-003 | Regeln können deaktiviert, zeitlich aktiviert und abgelaufen sein. | `PriorityMailRule.IsActiveAt` |
| PNW-NOT-001 | Regel, Konto und stabile Nachrichtenidentität bilden den Deduplizierungsschlüssel. | `NotificationDispatchKey` |
| PNW-NOT-002 | Benachrichtigungen werden vor Versand atomar reserviert. | `INotificationDispatchStore` und Application-Tests |
| PNW-RUN-001 | Überlappende Prüfläufe werden verhindert. | `PriorityMailWatcherCoordinator` |
| PNW-RUN-002 | Der Tray-Prozess startet ohne sichtbares Hauptfenster und ohne erhöhte Rechte. | ApplicationContext und Manifest |

## Noch offen

| ID | Technische Lücke |
|---|---|
| PNW-MAIL-001 | Produktiver Adapter auf gemeinsame IMAP-Abstraktionen einschließlich IDLE-Lifecycle |
| PNW-MAIL-002 | IMAP-Polling-Fallback und später POP3-UIDL-Fallback |
| PNW-PER-001 | SQLite-Repositories für Regeln, Cursor und Benachrichtigungszustände |
| PNW-SEC-001 | Zugriff auf gemeinsame Credential References ohne Secret-Duplikation |
| PNW-NOT-003 | Windows App Notifications mit Aktivierung und Workbench-Deep-Link |
| PNW-SET-001 | Einstellungs- und Regeloberfläche |
| PNW-OPS-001 | Autostartverwaltung, strukturierte Dateilogs und Diagnoseansicht |
