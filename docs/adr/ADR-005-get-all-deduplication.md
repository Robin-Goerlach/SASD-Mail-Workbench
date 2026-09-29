# ADR-005: Get all und exakte SHA-256-Deduplizierung

**Status:** Akzeptiert  
**Datum:** 12. Juli 2026  
**Gültig ab:** Planung 0.4.0

## Kontext

Parallel genutzte POP3-Clients können dazu führen, dass im lokalen Bestand Nachrichten fehlen oder Clients Duplikate anlegen.

## Entscheidung

Get all lädt jede auf dem Server sichtbare Mail vollständig in einen temporären Speicher. SHA-256 über Originalbytes, Konto-ID und Länge bestimmen, ob sie neu ist. Bekannte temporäre Kopien werden verworfen; kanonische lokale und Serverkopien bleiben erhalten.

## Konsequenzen

- langsam, aber bewusst vollständig
- streambasierter Download
- jeder Vorgang wird protokolliert
- späterer Bestandsprüflauf nutzt dieselbe Evidenz

## Verworfene oder verschobene Alternativen

- nur UIDL: nicht dauerhaft providerübergreifend belastbar
- nur Message-ID: kann fehlen oder doppelt sein
- automatisches Serverlöschen: zu hohes Datenverlustrisiko

## Rückverfolgbarkeit

Die Entscheidung ist bei Änderungen in Lastenheft, Pflichtenheft, Roadmap, Entwicklerhandbuch und Rückverfolgbarkeitsmatrix zu berücksichtigen.
