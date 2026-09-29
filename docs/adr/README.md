# Architecture Decision Records

**Stand:** 12. Juli 2026

ADRs dokumentieren tragende Entscheidungen, ihren Kontext und ihre Konsequenzen. Sie ersetzen weder Lasten- noch Pflichtenheft.

| ADR | Titel | Status |
|---|---|---|
| [ADR-001-csharp-dotnet](ADR-001-csharp-dotnet.md) | C# und modernes .NET als Plattform | Akzeptiert |
| [ADR-002-sqlite-storage](ADR-002-sqlite-storage.md) | SQLite für Metadaten und Arbeitszustände | Akzeptiert |
| [ADR-003-raw-message-storage](ADR-003-raw-message-storage.md) | Unveränderter Rohmail-Dateispeicher | Akzeptiert |
| [ADR-004-pop3-hybrid-access](ADR-004-pop3-hybrid-access.md) | POP3-zentrierter Abruf mit SMTP und gezieltem IMAP | Akzeptiert |
| [ADR-005-get-all-deduplication](ADR-005-get-all-deduplication.md) | Get all und exakte SHA-256-Deduplizierung | Akzeptiert |
| [ADR-006-imap-append-sent](ADR-006-imap-append-sent.md) | IMAP APPEND für gesendete Nachrichten | Akzeptiert |
| [ADR-007-winforms-ui-adapter](ADR-007-winforms-ui-adapter.md) | WinForms als erster austauschbarer UI-Adapter | Akzeptiert |
| [ADR-008-extension-contracts](ADR-008-extension-contracts.md) | Extension Contracts vor dynamischem Plugin-Laden | Akzeptiert |
| [ADR-009-target-framework](ADR-009-target-framework.md) | .NET 8 als Übergang; Migration vor Produktrelease | Akzeptiert |
| [ADR-010-local-warning-system](ADR-010-local-warning-system.md) | Lokales, erklärbares Warnsystem | Akzeptiert |


## Statuswerte

- **Vorgeschlagen:** noch nicht verbindlich
- **Akzeptiert:** verbindliche Grundlage
- **Ersetzt:** durch eine neuere ADR abgelöst
- **Verworfen:** nicht umgesetzt

Neue Entscheidungen werden nicht rückwirkend aus alten ADRs gelöscht. Stattdessen verweist eine neue ADR auf die ersetzte Entscheidung.

| [ADR-011-original-bytes-stream-fingerprint](ADR-011-original-bytes-stream-fingerprint.md) | Originalbytes und Stream-Hashing | Akzeptiert |
| [ADR-012-crash-safe-import-state-machine](ADR-012-crash-safe-import-state-machine.md) | Import-Zustandsmaschine und Recovery | Akzeptiert |
| [ADR-013-standard-tests-and-reproducible-build](ADR-013-standard-tests-and-reproducible-build.md) | NUnit und reproduzierbarer Build | Akzeptiert |
| [ADR-014-stable-processing-identities](ADR-014-stable-processing-identities.md) | Stabile Processing- und Analyzer-IDs | Akzeptiert |
