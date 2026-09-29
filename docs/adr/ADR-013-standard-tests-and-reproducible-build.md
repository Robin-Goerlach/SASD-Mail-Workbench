# ADR-013: Standardisierte Tests und reproduzierbarer Build

**Status:** Akzeptiert

Ab 0.3.1 werden NUnit, Microsoft.NET.Test.Sdk und NUnit Adapter eingesetzt. `global.json`, zentrale Paketversionen und Lockfiles sichern die Reproduzierbarkeit. `dotnet test` und Visual Studio Test Explorer sind verbindliche Ausführungswege.
