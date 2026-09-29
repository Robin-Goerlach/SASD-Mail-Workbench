# Import, Deduplizierung und Recovery

## Inkrementeller Modus

Ein bereits beobachteter Quellschlüssel wird übersprungen. Später entspricht
dieser Schlüssel typischerweise einer POP3-UIDL.

## Vollständige Verifikation

Jede sichtbare Nachricht wird erneut gestreamt. Während des Schreibens in eine
temporäre Datei werden SHA-256 und Byte-Länge berechnet. Danach entscheidet der
Katalog anhand von Konto, Hash und Länge:

- unbekannt: atomar in den Rohmail-Speicher übernehmen,
- bekannt: nur die temporäre Kopie löschen und die Quellenbeobachtung erneuern.

Die Nachricht auf dem Mailserver wird dabei niemals gelöscht.

## Zustände

```text
Discovered → Downloading → Staged → Committing → Completed
                         ↘ DuplicateDiscarded
```

Fehler- und Abbruchzustände werden getrennt gespeichert. Startup-Recovery
untersucht `Downloading`, `Staged` und `Committing`.

## Absturzfenster

Vor dem Dateiverschieben wird der Zielpfad in SQLite gespeichert. Liegt nach
einem Absturz bereits die Zieldatei vor, kann der Katalogabschluss fortgesetzt
werden. Liegt nur die Stagingdatei vor, wird sie atomar verschoben. Fehlen beide
Dateien, wird keine Vermutung angestellt; der Datensatz erhält
`RequiresManualReview`.
