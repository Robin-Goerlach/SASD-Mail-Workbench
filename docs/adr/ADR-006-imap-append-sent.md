# ADR-006: IMAP APPEND für gesendete Nachrichten

**Status:** Akzeptiert  
**Datum:** 12. Juli 2026  
**Gültig ab:** Planung 0.4.0

## Kontext

Andere parallel verwendete IMAP-Clients sollen lokal gesendete Nachrichten im Serverordner Gesendet sehen.

## Entscheidung

Nach SMTP-Erfolg wird die exakt versendete MIME-Nachricht lokal archiviert und anschließend per IMAP APPEND in den ermittelten Gesendet-Ordner übertragen.

## Konsequenzen

- SMTP- und APPEND-Erfolg bleiben getrennt
- UploadUncertain verhindert blindes Wiederholen
- Zielordner wird über Special-Use, Providerprofil oder Nutzerauswahl ermittelt

## Verworfene oder verschobene Alternativen

- nur lokale Gesendet-Ablage: für andere Clients unsichtbar
- BCC an sich selbst: falsche Semantik und unzuverlässige Ordnerzuordnung

## Rückverfolgbarkeit

Die Entscheidung ist bei Änderungen in Lastenheft, Pflichtenheft, Roadmap, Entwicklerhandbuch und Rückverfolgbarkeitsmatrix zu berücksichtigen.
