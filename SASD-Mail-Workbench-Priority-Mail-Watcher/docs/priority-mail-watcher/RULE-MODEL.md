# Regelmodell Version 0.1

## Semantik

Eine Regel trifft nur, wenn sie aktiv ist und jede konfigurierte Kategorie erfüllt ist.

```text
Aktiv
UND optionales Konto
UND (exakter Absender ODER Absenderdomain)
UND (Betreff enthält Begriff A ODER Begriff B ...)
UND (Suchtext enthält Schlüsselwort A ODER Schlüsselwort B ...)
UND optionale Anhangsbedingung
```

Leere Kategorien werden ignoriert. Eine Regel ohne einziges fachliches Kriterium ist ungültig.

## Normalisierung

- Mailadressen und Domains werden kleingeschrieben.
- Ein führendes `@` einer Domain wird entfernt.
- Suchbegriffe werden getrimmt, aber nicht fachlich umgeschrieben.
- Vergleiche von Betreff und Suchtext sind ordinal und unabhängig von Groß-/Kleinschreibung.

## Stabile Nachrichtenidentität

`StableMessageId` ist eine technische Identität innerhalb eines Kontos. Für IMAP darf sie nicht nur aus dem sichtbaren `Message-Id`-Header bestehen. Der spätere Adapter soll mindestens Konto, Ordner, `UIDVALIDITY` und UID berücksichtigen.

## Deduplizierung

Der erste Schlüssel lautet:

```text
RuleId + AccountId + StableMessageId
```

Ein persistenter Adapter muss diesen Schlüssel mit einer eindeutigen Datenbankbedingung absichern. Erst danach darf die Benutzerbenachrichtigung versendet werden.

## Bewusste Auslassungen

Reply-To, Thread-Beobachtung, Ruhezeiten, einmalige Regeln, Priorität, Vertrauensstufen und DKIM-/DMARC-Auswertung sind noch nicht Teil dieses Regelmodells. Sie werden nicht als untypisierte Freitextoptionen vorweggenommen.
