# SASD Priority Mail Watcher

Der Priority Mail Watcher ist ein eigener Tray-Prozess innerhalb des Repositorys **SASD-Mail-Workbench**. Er bewertet ausschließlich normalisierte Mailbeobachtungen gegen explizite Beobachtungsregeln und benachrichtigt nicht pauschal über jede neue Nachricht.

## Dokumente

- [ARCHITECTURE.md](ARCHITECTURE.md) – Projektgrenzen, Abhängigkeiten und Laufzeitfluss
- [REQUIREMENTS.md](REQUIREMENTS.md) – technische Anforderungen dieses Integrationsstands
- [RULE-MODEL.md](RULE-MODEL.md) – Semantik des ersten Regelmodells
- [DEVELOPMENT.md](DEVELOPMENT.md) – Build, Tests und nächste technische Adapter

## Status

Dieser Stand etabliert die Architekturgrundlage und den Tray-Lebenszyklus. Produktive Mailprotokoll-, SQLite-, Credential- und Windows-App-Notification-Adapter sind noch nicht implementiert.
