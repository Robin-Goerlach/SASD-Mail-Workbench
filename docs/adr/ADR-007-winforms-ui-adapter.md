# ADR-007: WinForms als erster austauschbarer UI-Adapter

**Status:** Akzeptiert  
**Datum:** 12. Juli 2026  
**Gültig ab:** Planung 0.4.0

## Kontext

Eine zuverlässige, schnell realisierbare Windows-Oberfläche wird benötigt; spätere WPF- oder MAUI-Oberflächen sollen möglich bleiben.

## Entscheidung

WinForms enthält nur Fenster, Controls, Binding und UI-nahe Dienste. Fachverhalten und UI-Zustände liegen in Application und Presentation.Core. Ein eigener Host ist Composition Root.

## Konsequenzen

- keine MailKit- oder SQLite-Referenz im WinForms-Projekt
- kurze Eventhandler
- Designer.cs ohne Geschäftslogik
- spätere Oberfläche ersetzt nur Presentation/Host

## Verworfene oder verschobene Alternativen

- Geschäftslogik in Forms: blockiert Migration
- WPF sofort: höheres anfängliches Risiko
- MAUI sofort: für den Windows-first-Start unnötige Plattformkomplexität

## Rückverfolgbarkeit

Die Entscheidung ist bei Änderungen in Lastenheft, Pflichtenheft, Roadmap, Entwicklerhandbuch und Rückverfolgbarkeitsmatrix zu berücksichtigen.
