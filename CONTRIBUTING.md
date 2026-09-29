# Mitwirken

Das Repository ist zunächst für die private Entwicklung der SASD GmbH
vorbereitet. Externe Beiträge werden erst nach einer Lizenzentscheidung
angenommen.

## Entwicklungsregeln

1. Ein Commit behandelt genau eine nachvollziehbare Änderung.
2. Öffentliche Typen und nichttriviale Methoden erhalten XML-Kommentare.
3. Domain und Application dürfen keine UI- oder SQLite-Abhängigkeiten erhalten.
4. Fehlerbehebungen benötigen einen reproduzierenden Test.
5. Secrets, echte Mailadressen und produktive E-Mails dürfen nicht committed werden.

## Commitstil

```text
feat(storage): add atomic raw-message commit
fix(import): preserve cancelled state during staging
test(recovery): cover missing target and staging files
docs(adr): define exact fingerprint policy
```
