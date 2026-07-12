# ADR-009: Gestufte Target-Framework- und IDE-Strategie

**Status:** Akzeptiert  
**Datum:** 12. Juli 2026  
**Ersetzt:** sofortige .NET-10-Migration zu Beginn von 0.4.0

## Entscheidung

1. 0.3.1 und 0.4.0 verwenden .NET 8, C# 12 und Visual Studio 2022.
2. Die Migration auf .NET 10 LTS wird spätestens in 0.5.0 abgeschlossen.
3. Wird das Supportende von .NET 8 vorher erreicht, wird die Migration vorgezogen.
4. Version 1.0 wird nur auf einer unterstützten LTS-Version veröffentlicht.
5. Vor der Migration werden keine .NET-10-/C#-14-exklusiven Kernfunktionen verwendet.

## Begründung

Die Stabilisierung von Build, Tests und Datenintegrität wird nicht mit einem gleichzeitigen IDE- und Runtimewechsel vermischt. Das Migrations-Gate verhindert dennoch ein Produktrelease auf einer nicht mehr unterstützten Plattform.
