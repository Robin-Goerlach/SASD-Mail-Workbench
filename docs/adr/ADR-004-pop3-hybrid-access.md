# ADR-004: POP3-zentrierter Abruf mit SMTP und gezieltem IMAP

**Status:** Akzeptiert  
**Datum:** 12. Juli 2026  
**Gültig ab:** Planung 0.4.0

## Kontext

Der Anwender bevorzugt POP3 und möchte lokale Vollständigkeit, zugleich sollen gesendete Mails für andere Clients serverseitig sichtbar sein.

## Entscheidung

Eingang wird primär über POP3S abgerufen. SMTP versendet. IMAP wird zunächst gezielt für Gesendet-Ordner-Ermittlung und APPEND eingesetzt.

## Konsequenzen

- POP3 bleibt einfacher Eingangskanal
- vollständige IMAP-Eingangssynchronisation bleibt spätere Erweiterung
- drei Protokollzustände werden getrennt modelliert

## Verworfene oder verschobene Alternativen

- nur POP3: kann nichts hochladen
- vollständiges IMAP zuerst: entspricht nicht dem bevorzugten Arbeitsmodell

## Rückverfolgbarkeit

Die Entscheidung ist bei Änderungen in Lastenheft, Pflichtenheft, Roadmap, Entwicklerhandbuch und Rückverfolgbarkeitsmatrix zu berücksichtigen.
