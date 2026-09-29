# Architektur

## Ziel

Der Watcher ist ein eigener Prozess im Benutzerkontext. Er bleibt von der geöffneten Mail-Workbench-Oberfläche unabhängig, teilt aber deren fachliche und technische Infrastruktur über stabile Schnittstellen.

## Projekte

| Projekt | Verantwortung | Darf nicht enthalten |
|---|---|---|
| `Notification.Domain` | Regeln, neutrale Mailbeobachtung, Matching | MailKit, SQLite, WinForms, Logging-Frameworks |
| `Notification.Contracts` | Ports und neutrale Ergebnisverträge | konkrete Adapter, Netzwerk- oder UI-Code |
| `Notification.Application` | Ablaufsteuerung, Deduplizierungsworkflow, Fehlerisolation | MailKit, SQLite, WinForms |
| `Notification.Infrastructure` | vorläufige Adapter und spätere technische Implementierungen | Tray-Oberfläche |
| `Host.Tray` | Composition Root, Tray-Lebenszyklus, Benutzerbenachrichtigung | Mailprotokoll- oder SQL-Implementierung |

## Abhängigkeitsrichtung

```text
Host.Tray ───────────────┐
                         v
Notification.Infrastructure --> Notification.Application
                                      |          |
                                      v          v
                         Notification.Contracts --> Notification.Domain
```

`Notification.Domain` besitzt keine ausgehenden Projektreferenzen. `Notification.Application` kennt ausschließlich Domain und Contracts. Der Host setzt konkrete Implementierungen über Constructor Injection zusammen.

## Laufzeitfluss

```text
Timer / später IMAP-IDLE-Signal
        ↓
PriorityMailWatcherCoordinator
        ↓
IPriorityMailRuleRepository
        ↓
IMailObservationSource
        ↓
PriorityMailEvaluationService
        ↓
PriorityMailRuleMatcher
        ↓
INotificationDispatchStore.TryReserveAsync
        ↓
IUserNotificationPublisher
        ↓
INotificationDispatchStore.MarkDeliveredAsync
```

Die Reservierung findet vor der sichtbaren Benachrichtigung statt. Damit kann ein späterer SQLite-Adapter Mehrfachzustellung auch zwischen Workbench- und Watcher-Prozess atomar verhindern. Schlägt ausschließlich das Persistieren des Zustellstatus nach einer bereits erfolgreichen Benutzerbenachrichtigung fehl, bleibt die Reservierung bestehen; eine unkontrollierte Doppelbenachrichtigung ist sicherheitsseitig schlechter als ein zunächst ungeklärter Zustellstatus.

## Mailzugriff

`IMailObservationSource` ist **kein neues IMAP- oder POP3-Client-Interface**. Es ist ein Watcher-spezifischer Read-Port für bereits normalisierte neue Mailereignisse. Der produktive Adapter muss die gemeinsamen Mail-Workbench-Abstraktionen verwenden.

## Mehrprozessbetrieb

Für den späteren SQLite-Adapter gelten mindestens:

- WAL-Modus und definierter Busy-Timeout,
- kurze explizite Transaktionen,
- Unique Constraint auf Regel-ID plus stabile Nachrichtenidentität,
- schemaweit koordinierte Migrationen,
- keine Secrets in SQLite,
- keine unkoordinierten parallelen Vollabrufe.

Ein lokaler Broker wird in diesem Stand nicht vorweggenommen. Er ist erst technisch zu rechtfertigen, wenn Locking- oder Ownership-Anforderungen mit SQLite und gemeinsamen Ports nicht zuverlässig erfüllbar sind.
