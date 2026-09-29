# ADR-001: C# und modernes .NET als Plattform

**Status:** Akzeptiert  
**Datum:** 12. Juli 2026  
**Gültig ab:** Planung 0.4.0

## Kontext

Der Desktopclient benötigt robuste Netzwerk-, SQLite-, Test- und Windows-Integration.

## Entscheidung

C# mit modernem .NET wird als Kernplattform verwendet. Der vorhandene Prototyp bleibt Migrationsquelle; das Zielrelease wird auf .NET 10 LTS ausgerichtet.

## Konsequenzen

- gute Trennung in Projekte und Bibliotheken
- WinForms, WPF und MAUI können denselben fachlichen Kern nutzen
- NuGet-Abhängigkeiten und Runtime-Migrationen werden kontrolliert dokumentiert

## Verworfene oder verschobene Alternativen

- C++: höherer Implementierungs- und Sicherheitsaufwand
- Python: gut für Prototypen, aber nicht bevorzugt für den langfristigen Windows-Desktopclient
- Java: technisch möglich, aber weniger passend zum vorhandenen SASD-.NET-Portfolio

## Rückverfolgbarkeit

Die Entscheidung ist bei Änderungen in Lastenheft, Pflichtenheft, Roadmap, Entwicklerhandbuch und Rückverfolgbarkeitsmatrix zu berücksichtigen.
