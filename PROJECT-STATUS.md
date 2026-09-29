# Projektstatus

**Produkt:** SASD Mail Workbench  
**Softwareversion:** 0.3.1  
**Dokumentstand:** 29. September 2026

**Status:** Milestone 0.3.1 technisch abgeschlossen; Retrieval Foundation 0.4.0 ist der nächste Entwicklungsschritt.

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
- committed `packages.lock.json` für alle Projekte der Hauptsolution,
- Windows-Release-Build und 21 Tests lokal erfolgreich,
- GitHub Actions auf `windows-latest` und `ubuntu-latest` erfolgreich,
- Dokumentation für Milestone 0.3.1.

Der GitHub-Actions-Lauf für Commit
`7109775960ca1770602c2746f0bc2432c47fc635` war am 29. September 2026 auf
Windows und Ubuntu erfolgreich. Beide Jobs haben Restore, Release-Build und
alle fünf Testprojekte erfolgreich ausgeführt. Insgesamt bestanden 21 Tests;
es gab 0 fehlgeschlagene und 0 übersprungene Tests.

## Bekannte technische Schulden

Der Release-Build ist erfolgreich, enthält aber weiterhin Analyzer-Warnungen.
Dazu gehören insbesondere CA1001, CA1512, CA1716, CA1859 sowie CA1707 in
Testmethodennamen. Diese Befunde sind getrennt von Compilerfehlern zu behandeln
und blockieren die Definition of Done von 0.3.1 nicht.

Das separate Priority-Mail-Watcher-Integrationspaket ist noch nicht Bestandteil
der Hauptsolution und wurde durch diesen CI-Lauf nicht validiert.

## Noch nicht erreicht

- POP3-, SMTP- oder IMAP-Produktivadapter,
- WinForms-Oberfläche,
- MIME-Parsing und Warnsystem,
- produktive Konto- und Credential-Verwaltung.

## Nächster technischer Schritt

Milestone 0.4.0 beginnt mit der Mail Retrieval Foundation. Dabei wird die
bestehende providerneutrale Importpipeline weiterverwendet; ein POP3-Adapter
darf weder die bytegenaue Speicherung noch die vorhandene Deduplizierungs- und
Recoverylogik umgehen.
