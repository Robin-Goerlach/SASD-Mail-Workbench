# Anforderung: IMAP-Backup, lokale Archivierung, Serverbereinigung und Restore

Status: **Entwurf / Architekturvorschlag**, noch nicht implementiert  
Stand: 2026-11-06

## 1. Ziel und Abgrenzung

Die SASD Mail Workbench soll vollständige Postfächer von Anbietern wie IONOS und GMX lokal sichern, nach überprüfter Sicherung optional serverseitig bereinigen und Nachrichten später in dasselbe oder ein anderes IMAP-Postfach zurückspielen können. Dies ist eine **geplante Funktion**, keine Aussage über den aktuellen Implementierungsstand (siehe README und ROADMAP).

**Grundsatz:** Restore ist ein IMAP-Upload mittels `APPEND`, **kein** erneuter SMTP-Versand. POP3 kann zum Abruf dienen, bietet aber keine vollständige Ordner-/Flag-Wiederherstellung und keinen Upload.

## 2. Betriebsarten

1. **Synchronisation:** Server bleibt führend, lokale Offlinekopie.
2. **Archivierung (Standardempfehlung):** lokal dauerhaft archivieren, ausgewählte verifizierte Serverkopien optional entfernen.
3. **Cold Storage (erweitert):** Serverpostfach weitgehend leeren, lokales Archiv wird primärer Bestand; erhöhte Anforderungen an Backup und Wiederherstellung.

Serverlöschung ist immer eine **explizite, gesondert bestätigte** Aktion. Ein lokaler Import oder eine Deduplizierung darf nie automatisch eine Serverlöschung auslösen.

## 3. Fachliche Anforderungen

- Kontenübergreifende Sicherung inklusive Ordnerhierarchie, Posteingang, Gesendet, Entwürfen und benutzerdefinierten Ordnern, soweit der Server Zugriff gewährt.
- Rohmail (RFC 5322/MIME) **bytegenau** und unveränderlich sichern; keine Header-/Body-Normalisierung, kein Umkodieren von Anhängen.
- Pro Nachricht speichern: interne lokale ID, Konto, Ordnerpfad, UID und UIDVALIDITY der Quelle, INTERNALDATE, Größe, Standardflags und unterstützte Keywords, Message-ID-Header, SHA-256 der Originalbytes, Sicherungs-/Prüfstatus.
- Ordner- und Special-Use-Mapping für unterschiedliche Provider; vor Migration Vorschau und editierbare Zielzuordnung.
- Einzelne Nachricht, Auswahl, Ordner oder gesamtes Konto wiederherstellen; auch Anbieterwechsel ermöglichen.
- IMAP-Capabilities dynamisch ermitteln, keine providerspezifischen Features voraussetzen; Quota und Upload-Limits soweit verfügbar vorab prüfen.
- Vor Restore Konflikt- und Duplikatprüfung; gleiche Message-ID allein reicht nicht als Identitätsnachweis. Der Rohmail-Hash ist für bytegleiche Inhalte maßgeblich, jedoch können legitime Kopien in verschiedenen Ordnern vorkommen.
- Fortschritt, Abbruch, Wiederanlauf, Fehlerbericht, Prüfbericht und nachvollziehbares Auditjournal ohne Geheimnisse oder Rohmailinhalte im Log.

## 4. Sicherheitskritischer Workflow: Backup und Löschen

1. IMAP-Ordner und Nachrichten inventarisieren, Quelle mit UIDVALIDITY und UID erfassen.
2. Nachrichten vollständig herunterladen und zunächst in Staging-Dateien schreiben.
3. Nach erfolgreichem Schreiben atomar in das Archiv übernehmen; SHA-256 und Byte-Länge speichern.
4. Gespeicherte Bytes erneut lesen und Integrität prüfen; MIME-Struktur optional validieren (MIME-Prüfung ersetzt keinen Bytevergleich).
5. Zweite, unabhängige und verschlüsselte Sicherung erstellen und **tatsächlich verifizieren** (beispielsweise Stichproben plus vollständige Manifestprüfung); Backupstatus getrennt vom Downloadstatus führen.
6. Erst dann Kandidaten für eine Serverbereinigung anbieten; Auswahl, Umfang, Anzahl, Speicherplatz und Risiken anzeigen.
7. Optional zunächst in einen Quarantäneordner verschieben; dies ist nur eine zusätzliche Rückfallstufe, **kein Ersatz** für ein unabhängiges Backup. Quarantäne kann weiterhin auf die Serverquota angerechnet werden.
8. Nach expliziter Freigabe löschen; vor jedem Eingriff UIDVALIDITY und Nachrichtenidentität erneut prüfen, um zwischenzeitliche Änderungen zu berücksichtigen. Keine unsichere pauschale `EXPUNGE`-Operation, die fremde als gelöscht markierte Nachrichten entfernt; `UID EXPUNGE` nur bei nachgewiesener Unterstützung und sicherer Zielmenge.
9. Serverzustand nachprüfen und Ergebnis pro Nachricht journalisieren. Bei Unsicherheit stoppen und manuelle Prüfung verlangen.

**Nicht zulässig:** `Download erfolgreich → sofort löschen`.

### Zustandsmodell (Vorschlag)

`Discovered → Downloading → Downloaded → Verified → BackupConfirmed → DeletionPending → DeletedFromServer`

Für Restore gesondert: `RestorePending → Uploading → Restored → RestoreVerified`; Fehlerzustände und Wiederanlauf müssen explizit modelliert werden. Zustandsübergänge sind idempotent beziehungsweise wiederaufnehmbar zu implementieren.

## 5. Restore und Migration

1. Zielkonto, Zielordner und Mapping bestätigen.
2. Zielordner bei Bedarf anlegen; verfügbare Quota und Größenlimits berücksichtigen.
3. Vor Upload nach vorhandenen Nachrichten suchen und Konflikte anzeigen; keine automatische Löschung von vermeintlichen Duplikaten.
4. Unveränderte RFC-5322-Rohmail per IMAP `APPEND` mit unterstützten Flags und ursprünglichem INTERNALDATE übertragen.
5. Falls verfügbar `APPENDUID` aus `UIDPLUS` auswerten; sonst Zielnachricht anhand mehrerer Merkmale suchen. Neue UID/UIDVALIDITY als **neue** Identität speichern.
6. Upload nachprüfen (Rücklesen/Bytevergleich, soweit Server die Nachricht unverändert zurückliefert; serverseitige Transformationen gesondert ausweisen). Flags, Datum und Ordnerzuordnung prüfen.
7. Transaktionsjournal und Prüfbericht aktualisieren. Nach Verbindungsabbruch vor Wiederholung auf bereits erfolgten Upload prüfen, um Duplikate zu vermeiden.

**Grenzen:** IMAP-UIDs und UIDVALIDITY lassen sich nicht konservieren. Providerinterne Spam-Klassifikationen, Filter, Kontakte, Kalender, serverseitige Regeln und manche Keywords sind nicht Bestandteil der Rohmail und können verloren gehen. DKIM-Header bleiben bei bytegleicher Sicherung enthalten, ihre Gültigkeit ist nicht garantiert. Upload-Limits, Speicherquoten und Serveränderungen können Restore verhindern.

## 6. Datenhaltung und Sicherheit

- Originalbytes getrennt von Metadaten speichern; Hash = SHA-256 über die **gesamte unveränderte Rohmail**, nicht über normalisierte Header.
- SQLite-Katalog mit versionierten Migrationen, relativen Pfaden, transaktionalem Journal und konsistenter Referenzierung des Rohmailarchivs.
- Keine Passwörter oder Tokens im Klartext; OS-Credential-Store beziehungsweise geeigneter Secret-Provider.
- Verschlüsselter Datenträger und verschlüsselte unabhängige Backups; Wiederherstellung aus Backup regelmäßig testen.
- Least Privilege, keine Secrets oder Nachrichteninhalte im Log; bei Lösch- und Restoreaktionen vollständige, datensparsame Auditspur.
- Archivdateien nicht stillschweigend überschreiben; Integritätsfehler als harte Sperre für serverseitige Löschung behandeln.

## 7. Architekturvorschlag

Die bestehende Trennung Domain / Application / Infrastructure / Persistence beibehalten. Keine neuen eigenständigen Projekte erzwingen, bevor Schnittstellen und Verantwortlichkeiten geklärt sind.

- **Domain:** `ArchiveEntry`, `FolderMapping`, `BackupManifest`, `TransferState`, `RestoreResult`, Löschfreigabe-Regeln.
- **Application:** `BackupAccount`, `VerifyBackup`, `PrepareServerCleanup`, `DeleteVerifiedServerCopies`, `RestoreMessages`, `MigrateAccount`.
- **Infrastructure:** IMAP-Adapter mit Capability-Erkennung, Streaming, APPEND und UID-bezogenen Operationen; optional POP3-Importadapter.
- **Persistence:** unveränderliche Rohmailobjekte, SQLite-Metadaten, Journal, Manifeste.
- **UI:** Assistenten mit Vorschau, Fortschritt, Abbruch, Wiederanlauf, Konfliktansicht und separater Bestätigung destruktiver Aktionen.
- **ExtensionModel:** Provider-Mapping und spätere Plugins dürfen den Sicherheitskern nicht umgehen; keine Plugin-Berechtigung zur ungeprüften Löschung.

## 8. Abnahmekriterien

- [ ] IMAP-Ordnerhierarchie und Rohmails eines Testkontos vollständig lokal gesichert.
- [ ] SHA-256 und Byte-Länge nach erneutem Lesen identisch; manipulierte Archivdatei wird erkannt.
- [ ] Serverlöschung ohne bestätigte unabhängige Zweitsicherung technisch gesperrt.
- [ ] Abbruch und Wiederanlauf in Download-, Lösch- und Restorephase ohne Datenverlust.
- [ ] Geänderte UIDVALIDITY beziehungsweise veränderte Quelle verhindert unsichere Löschung.
- [ ] IMAP-APPEND stellt Rohmail, ursprüngliches INTERNALDATE und unterstützte Flags wieder her.
- [ ] Wiederholter Restore nach unklarem Upload-Ergebnis führt nicht stillschweigend zu Duplikaten.
- [ ] Restore von Konto A nach Konto B mit Ordner-Mapping und Konfliktbericht getestet.
- [ ] Fehlende Servererweiterungen und Quota-/Größenfehler werden sicher behandelt.
- [ ] Logs enthalten weder Zugangsdaten noch vollständige Nachrichteninhalte.
- [ ] POP3-only-Konten werden nicht als vollständig wiederherstellbar ausgewiesen.
- [ ] Kein produktives Postfach wird in automatisierten Tests gelöscht.

## 9. Priorisierung und offene Entscheidungen

**Phase A:** Nur lesendes IMAP-Inventar, vollständiger Download, lokales Archiv, Integritätsprüfung und Export.  
**Phase B:** IMAP-Restore und Migration in Testpostfächer, Konfliktprüfung, idempotenter Wiederanlauf.  
**Phase C:** Zweitbackup-Verifikation, sichere Serverbereinigung mit ausdrücklicher Freigabe und Audit.  
**Später:** zeitgesteuerte Archivierungsregeln, Cold-Storage-Modus, providerübergreifende Komfortfunktionen.

Vor der Implementierung festlegen: Speicherlayout (einzelne .eml-Dateien oder Content-Addressed Store), Backupmanifestformat, Verschlüsselungsgrenze, Umgang mit serverseitig veränderten Nachrichten, Quarantäne-Aufbewahrungsdauer, Ordner-Mapping und UX für Löschfreigaben.

## 10. Protokollreferenzen

- RFC 9051 – IMAP4rev2 (APPEND, Flags, INTERNALDATE)
- RFC 3501 – IMAP4rev1
- RFC 4315 – UIDPLUS
- RFC 6851 – IMAP MOVE
- RFC 7889 – APPENDLIMIT
- RFC 1939 – POP3
- RFC 5322 – Internet Message Format

Diese Anforderung erweitert die bestehende Rohmail-/Import- und Recovery-Architektur; sie behauptet **keine** bereits implementierte IMAP-, POP3- oder SMTP-Funktion.
