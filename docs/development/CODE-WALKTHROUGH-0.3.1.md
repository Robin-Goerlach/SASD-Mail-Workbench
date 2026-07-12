# Code-Walkthrough Milestone 0.3.1

Dieses Dokument erklärt den Code in der Reihenfolge, in der er beim Import arbeitet.

## 1. Domain

`MessageFingerprint` enthält SHA-256 und Byte-Länge. Der Hash wird über die unveränderten Bytes gebildet. Eine Mail wird vor der Deduplizierung nicht als Text gelesen.

`ImportAttemptState` bildet eine Zustandsmaschine. `ImportStateRules` verhindert, dass Repository oder Oberfläche beliebige Zustände speichern.

## 2. Application

`ImportRawMessagesUseCase` orchestriert den Import, kennt aber weder SQLite noch POP3. Im inkrementellen Modus ist `SourceKey` der schnelle Vorfilter. Im vollständigen Modus wird jede sichtbare Mail erneut gestreamt und gehasht.

Ablauf:

1. Versuch als `Discovered` anlegen.
2. Auf `Downloading` wechseln.
3. Originalbytes in `staging/` schreiben und gleichzeitig hashen.
4. Als `Staged` speichern.
5. Nach Konto, SHA-256 und Länge auf Duplikate prüfen.
6. Neue Mail atomar nach `raw-mails/Jahr/Monat/` verschieben.
7. Datei und Katalog transaktional abschließen.

`RecoverInterruptedImportsUseCase` untersucht beim Start offene Versuche. Es rät niemals bei fehlenden Originaldaten, sondern markiert widersprüchliche Fälle zur manuellen Prüfung.

## 3. Infrastructure

`Sha256MessageFingerprintService` liest den Stream blockweise. Dadurch passen auch große Nachrichten in einen kleinen, konstanten Arbeitsspeicher.

`DirectoryRawMessageSource` simuliert den späteren POP3-Adapter mit lokalen EML-Dateien.

## 4. Persistence

`FileSystemRawMessageStore` hält Staging und Ziel auf demselben Dateisystem. Die endgültige Übernahme erfolgt mit `File.Move`, also als atomare Umbenennung.

`SqliteMigrationRunner` führt eingebettete SQL-Migrationen in Reihenfolge aus und prüft deren SHA-256-Prüfsumme. Bereits angewendete Migrationen dürfen nachträglich nicht verändert werden.

`SqliteMessageImportRepository` speichert Nachrichten, Quellenbeobachtungen und Importversuche. Der Katalogabschluss läuft in einer SQLite-Transaktion.

## 5. Tests

Die Tests prüfen insbesondere:

- CRLF und LF ergeben unterschiedliche Fingerprints,
- binäre Bytes werden unverändert gespeichert,
- exakte Duplikate werden automatisch verworfen,
- ein zweiter inkrementeller Lauf überspringt bekannte Quellschlüssel,
- ein unterbrochener Stagingimport wird wiederhergestellt,
- verwaiste Stagingdateien werden entfernt,
- unzulässige Schichtabhängigkeiten werden erkannt.
