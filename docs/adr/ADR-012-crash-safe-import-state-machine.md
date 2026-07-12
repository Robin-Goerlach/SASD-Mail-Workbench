# ADR-012: Absturzsichere Import-Zustandsmaschine

**Status:** Akzeptiert

Der Import verwendet `Discovered`, `Downloading`, `Staged`, `RawStored`, `Parsed` und `Processed` sowie eigene Fehler-, Abbruch- und Reviewzustände. Startup-Recovery erkennt verwaiste Dateien und unvollständige Datenbankzustände. Ein bekannter externer Schlüssel bedeutet nicht automatisch, dass Parsing und Processing abgeschlossen sind.
