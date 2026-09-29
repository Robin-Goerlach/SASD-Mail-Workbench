# Dokumentationsplan

**Dokumentversion:** 1.0  
**Stand:** 12. Juli 2026

## 1. Zweck

Der Dokumentationsplan legt fest, welche Unterlagen den Code begleiten, wann sie aktualisiert werden und welche Dokumente vor einem Release verbindlich sind.

## 2. Dokumenthierarchie

### Projektweit verbindlich

- Lastenheft V1.0
- Pflichtenheft V1.0
- `PROJECT-STATUS.md`
- `ROADMAP.md`
- ADR-Sammlung
- `TRACEABILITY.md`

### Versionsbezogen

- Release Notes
- Entwicklerhandbuch
- Testhandbuch
- Migrationshinweise
- Known Issues

### Funktionsbezogen

- Sicherheitskonzept und Threat Model
- Datenbankhandbuch
- UI-/UX-Spezifikation
- Mailprotokoll- und Synchronisationskonzept
- Backup-/Restore-Runbook
- Diagnose- und Anonymisierungskonzept
- Extension-/Plugin-Konzept

### Vor 1.0 zusätzlich

- Benutzerhandbuch
- Installationshandbuch
- Administratorhandbuch
- Troubleshooting-Handbuch
- Datenschutz- und Datenflussdokumentation
- Software Bill of Materials und Drittanbieter-Lizenzen

## 3. Pflegepflicht

Ein Pull Request oder Commit ist dokumentationspflichtig, wenn er:

- öffentliche Schnittstellen ändert,
- Persistenzschema oder Migrationen ändert,
- einen Statusautomaten erweitert,
- Sicherheitsstandardwerte verändert,
- ein neues externes Paket einführt,
- eine Lastenheftanforderung implementiert,
- Bedienabläufe oder Screens verändert,
- Testausführung oder Buildvoraussetzungen verändert.

## 4. Verantwortliche Dokumente je Meilenstein

| Meilenstein | Pflichtaktualisierungen |
|---|---|
| 0.4.0 | Projektstatus, Roadmap, ADRs, Solution Architecture, Entwicklerhandbuch, Testhandbuch, POP3-/Get-all-Konzept |
| 0.5.0 | SMTP-/Outbox-/APPEND-Konzept, Zustandsmodelle, Entwickler- und Testhandbuch |
| 0.6.0 | Sicherheitskonzept, HTML-Rendering-Konzept, Warnsystem, Message-Laboratory-Spezifikation |
| 0.7.0 | UI-/UX-Spezifikation, Datenbankhandbuch, Backup-/Restore-Runbook, Benutzerhandbuch-Entwurf |
| 0.8.0 | Regel-, Datensatz-, Pseudonymisierungs- und Extension-Konzept |
| 0.9.0 | Installations-, Administrator-, Troubleshooting-, Datenschutz- und Release-Dokumentation |
| 1.0.0 | freigegebene Gesamtdokumentation, SBOM, Lizenzen, finale Release Notes |

## 5. Änderungsnachweis

Jedes größere Dokument erhält:

- Dokumentversion,
- zugehörige Softwareversion,
- Datum,
- Status (Entwurf, geprüft, freigegeben),
- Änderungsverlauf,
- Verweise auf ADRs und Anforderungen.

## 6. Keine doppelte Wahrheit

- Lastenheft: **Was** wird benötigt?
- Pflichtenheft: **Wie** wird es technisch erfüllt?
- ADR: **Warum** wurde eine tragende Entscheidung getroffen?
- Projektstatus: **Was ist heute wirklich vorhanden?**
- Roadmap: **Wann** wird der Rest geplant?
- Traceability: **Wo** befinden sich Code und Tests für jede Anforderung?
