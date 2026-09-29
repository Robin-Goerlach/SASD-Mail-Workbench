# POP3 Streaming Evaluation

**Stand:** 29. September 2026  
**Zielmeilenstein:** 0.4.0 – Mail Retrieval Foundation  
**Status:** technische Entscheidungsvorlage, keine Architekturänderung

## Ausgangslage

Die bestehende Importpipeline erwartet über `IRawMessageSource.OpenReadAsync`
einen lesbaren Stream und verarbeitet ihn anschließend bytegenau. ADR-011
verlangt, dass Download, temporäre Speicherung, Bytezählung und SHA-256-
Berechnung streambasiert erfolgen. Die kanonische Rohmail darf vor der
Fingerprint-Bildung nicht als Text oder MIME-Nachricht normalisiert werden.

Für 0.4.0 ist MailKit als bevorzugte Protokollbibliothek vorgesehen.

## Verifizierter MailKit-Befund

Die aktuelle MailKit-Version 4.18.1 wurde gegen den Upstream-Quellcode geprüft.

`Pop3Client.GetStreamAsync(...)` verwendet intern
`DownloadStreamContext.ParseAsync(...)`. Dieser Pfad liest die vollständige
POP3-Nachricht und schreibt sie in einen `MimeKit.IO.MemoryBlockStream`.
Erst nach Abschluss des Downloads wird dieser Stream an den Aufrufer
zurückgegeben.

Damit ist `GetStreamAsync(...)` zwar eine Stream-API nach außen, aber kein
durchgängiger Netzwerk-zu-Staging-Stream. Eine große Rohmail befindet sich
vollständig in einem speicherbasierten Zwischenstream, bevor unsere
`IRawMessageStore.StageAsync(...)`-Pipeline sie erhält.

Der Befund betrifft die öffentliche POP3-Rohstream-API. UIDL-Ermittlung über
MailKit ist davon unabhängig und technisch geeignet.

## Konflikt mit unseren Invarianten

Ein produktiver Adapter, der einfach `Pop3Client.GetStreamAsync(...)`
durchreicht, würde zwei bereits gesetzte Anforderungen abschwächen:

1. ADR-011: Download und Fingerprint sollen streambasiert erfolgen.
2. NF-PER-003: Große Nachrichten dürfen nicht vollständig im RAM liegen müssen.

Da Datenintegrität, Reproduzierbarkeit und Korrektheit Vorrang vor einer
schnellen Adapterimplementierung haben, wird diese Abweichung nicht
stillschweigend eingeführt.

## Technische Alternativen

### A – MailKit-POP3 unverändert verwenden

Vorteile:

- kleinster Implementierungsaufwand,
- ausgereifte POP3-/TLS-/Authentifizierungsbehandlung,
- UIDL und Providerkompatibilität direkt verfügbar.

Nachteile:

- vollständige Rohmail wird vor unserer Stagingpipeline speicherbasiert
  gepuffert,
- widerspricht der gesetzten Streaming-Anforderung für große Nachrichten.

Diese Variante erfordert eine bewusste Änderung der bisherigen
Produkt-/Architekturvorgabe.

### B – Eng begrenzter eigener Streaming-RETR-Transport

MailKit kann weiterhin dort verwendet werden, wo seine öffentliche API die
Anforderungen erfüllt. Für den eigentlichen `RETR`-Datenstrom würde ein
kleiner POP3S-Transport die dot-unstufften Nachrichtendaten direkt als
forward-only Stream an `IRawMessageSource` liefern.

Vorteile:

- echte Netzwerk-zu-Staging-Verarbeitung,
- vorhandene Import-, Hash-, Deduplizierungs- und Recoverylogik bleibt
  unverändert,
- kein vollständiger Nachrichtenpuffer im Arbeitsspeicher erforderlich.

Nachteile:

- eigener POP3-Protokollcode erhöht Test-, Security- und Wartungsaufwand,
- TLS, Authentifizierung, Timeouts und Protokollfehler müssen sorgfältig
  abgedeckt werden,
- darf nicht zu einer zweiten parallelen Mailarchitektur anwachsen.

### C – MailKit erweitern beziehungsweise einen Upstream-Patch anstreben

Ziel wäre eine öffentliche MailKit-API, die POP3-Nutzdaten direkt in einen
bereitgestellten Zielstream schreibt oder einen echten laufenden
Nachrichtenstream liefert.

Vorteile:

- Protokollhandling bleibt bei MailKit,
- Streaming-Invariante kann erhalten bleiben.

Nachteile:

- zusätzlicher Fork-/Patch-Aufwand,
- unklarer Upstream-Zeitplan,
- vor einer Übernahme ist die Wartungs- und Lizenzstrategie festzulegen.

## Nicht empfohlene Zwischenlösung

Ein Größen-Schwellwert nach dem Muster „kleine Mails über MailKit puffern,
große Mails anders laden“ würde zwei unterschiedliche Abrufpfade mit
unterschiedlicher Fehlersemantik erzeugen. Das erhöht die Komplexität gerade
in dem Bereich, in dem Recovery und Provenienz besonders stabil sein müssen.

## Unabhängig weiter bearbeitbarer technischer Schritt

Die bestehende Fingerprint-/Stagingpipeline wird durch einen Test gegen einen
nicht seekbaren, stark fragmentierten forward-only Stream abgesichert. Damit
ist nachweisbar, dass der Anwendungskern keinen seekbaren oder vollständig
gepufferten Quellstream verlangt.

Zusätzlich schützen Architekturtests Domain, Application und ExtensionModel
explizit vor direkten MailKit-/MimeKit-Referenzen. Eine spätere
Protokollbibliothek bleibt damit an der Adaptergrenze.

## Offene Entscheidung

Vor dem produktiven POP3-Adapter muss entschieden werden, ob Variante A, B
oder C verbindlich verfolgt wird. Bis dahin wird keine schwächere
Streaming-Semantik als scheinbar fertige Retrieval-Implementierung committed.
