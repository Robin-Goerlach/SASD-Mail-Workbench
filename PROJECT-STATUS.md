# Projektstatus

**Produkt:** SASD Mail Workbench  
**Softwareversion:** 0.3.1  
**Dokumentstand:** 23. September 2026

**Status:** Windows-Release-Build und 21 Tests erfolgreich; externer Clean-Machine-Build ausstehend

## Erreicht

- konsolidierter Produkt- und Namespace-Name,
- .NET-8-SDK-Pinning,
- zentrale Paketversionen,
- reguläre NUnit-Testprojekte,
- bytegenauer Rohmail-Speicher,
- SQLite-Migrationen,
- Importversuche, Fingerprints und Quellbeobachtungen,
- Startup-Recovery-Use-Case,
- Architekturtests und GitHub Actions,
- Dokumentation für Milestone 0.3.1.

## Noch nicht erreicht

- unabhängige Clean-Machine- und CI-Bestätigung,
- committed `packages.lock.json` nach dem ersten Restore,
- POP3, SMTP oder IMAP,
- WinForms-Oberfläche,
- MIME-Parsing und Warnsystem.

## Nächster technischer Schritt

Build-Korrekturen und vorhandene Lockfiles prüfen, anschließend Clean-Machine-
und CI-Verifikation abschließen. Danach folgt die Retrieval Foundation aus
Milestone 0.4.0. Lokale Prüfbefehle und Grenzen stehen in
`docs/development/BUILD-VERIFICATION.md`.
