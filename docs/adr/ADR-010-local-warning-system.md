# ADR-010: Lokales, erklärbares Warnsystem

**Status:** Akzeptiert  
**Datum:** 12. Juli 2026  
**Gültig ab:** Planung 0.4.0

## Kontext

Mails können täuschende Absender, Buttons, Links, Domains und Anhänge enthalten. Ein einfacher Domainvergleich erzeugt jedoch Fehlalarme.

## Entscheidung

Header, Authentication-Results, sichtbare Texte, tatsächliche Ziele, IDN/Punycode, Markenprofile und Anhänge werden lokal durch versionierte Analyzer geprüft. Jede Warnung zeigt Evidenz und Gründe. Das System behauptet niemals absolute Sicherheit.

## Konsequenzen

- datenschutzfreundlich und offlinefähig
- Remote-Reputation nur nach ausdrücklicher Aktivierung
- Markenprofile erlauben legitime Dienstleister
- Analyzer-Schnittstellen bleiben erweiterbar

## Verworfene oder verschobene Alternativen

- ein einzelner Blackbox-Score: nicht nachvollziehbar
- automatisches Öffnen/Online-Prüfen aller Links: Datenschutz- und Sicherheitsrisiko

## Rückverfolgbarkeit

Die Entscheidung ist bei Änderungen in Lastenheft, Pflichtenheft, Roadmap, Entwicklerhandbuch und Rückverfolgbarkeitsmatrix zu berücksichtigen.
