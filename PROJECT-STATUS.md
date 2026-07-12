# Projektstatus

**Produkt:** SASD Mail Workbench  
**Softwareversion:** 0.3.1  
**Dokumentstand:** 12. Juli 2026  
**Status:** Repository-Grundlage erstellt; externer Clean-Machine-Build ausstehend

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

- reale Build- und Testbestätigung auf Windows,
- committed `packages.lock.json` nach dem ersten Restore,
- POP3, SMTP oder IMAP,
- WinForms-Oberfläche,
- MIME-Parsing und Warnsystem.

## Nächster technischer Schritt

Repository unter Windows mit Visual Studio 2022 und .NET SDK 8.0.422 öffnen,
`./scripts/verify.ps1` ausführen, auftretende Compiler- oder Paketabweichungen
korrigieren und anschließend die erzeugten Lockfiles committen.
