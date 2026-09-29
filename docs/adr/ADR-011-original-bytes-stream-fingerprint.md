# ADR-011: Originalbytes und streambasierter Fingerprint

**Status:** Akzeptiert

Rohmails werden als Bytes beziehungsweise Streams behandelt. Download, temporäre Speicherung, Bytezählung und SHA-256-Berechnung erfolgen streambasiert. Die kanonische Rohmail bleibt unverändert. Die exakte Deduplizierung verwendet mindestens Konto-ID, SHA-256 und Byte-Länge.
