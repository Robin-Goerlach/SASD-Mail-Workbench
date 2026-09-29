# ADR-016: Priority Mail Watcher als separater Companion-Prozess

**Status:** Akzeptiert

## Kontext

Der Priority Mail Watcher muss unabhängig von einem geöffneten Workbench-Fenster laufen, darf die gemeinsame Mailverarbeitung jedoch weder duplizieren noch behindern. Windows-Dienste sind für benutzergebundene Tray- und Benachrichtigungsinteraktion ungeeignet.

## Entscheidung

Der Watcher wird im Repository `SASD-Mail-Workbench` als eigener WinForms-Tray-Host mit getrennten Notification-Projekten implementiert. Domain und Application bleiben frei von WinForms, MailKit und SQLite. Produktive Mailzugriffe erfolgen ausschließlich über Adapter auf gemeinsame Mail-Workbench-Abstraktionen.

Der Host läuft mit `asInvoker` im Benutzerkontext. Der erste Integrationsstand verwendet Constructor Injection und eine explizite Composition Root; ein DI-Container wird erst ergänzt, wenn dessen Nutzen über reine Objektkonstruktion hinausgeht.

## Konsequenzen

- Workbench und Watcher können unabhängig gestartet werden.
- Gemeinsamer Code wird ohne Paket- und Versionssynchronisation zwischen Repositorys entwickelt.
- Ein späteres Auslagern bleibt durch die Projektgrenzen möglich.
- Deduplizierung und gemeinsame Persistenz benötigen atomare, prozessübergreifend belastbare Adapter.
- Der Tray-Host darf keine eigene IMAP-, POP3- oder SQLite-Geschäftslogik enthalten.

## Verworfene Alternativen

- **Funktion im Workbench-Hauptfenster:** arbeitet bei geschlossenem Fenster nicht zuverlässig.
- **Windows-Dienst:** erschwert Tray- und Benutzerbenachrichtigungen durch Session-Trennung.
- **Sofort separates Repository:** erzeugt vorzeitig Paketierung, Versionierung und Synchronisation gemeinsamer Verträge.
