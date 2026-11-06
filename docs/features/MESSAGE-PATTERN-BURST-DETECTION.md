# Message Pattern & Burst Detection

Status: Konsolidiertes Produkt- und Architekturkonzept; noch keine verbindliche V1-Abnahmeforderung.  
Stand: 2026-11-06

## Zweck dieses Dokuments

Dieses Dokument beschreibt die Erkennung gehäufter, identischer oder stark ähnlicher Nachrichten in der SASD Mail Workbench. Ausgangspunkt ist die Beobachtung, dass viele Mails mit gleichem oder ähnlichem Betreff, demselben Absender oder nahezu identischem Inhalt ein wichtiges Signal darstellen können.

Ein solches Muster kann Spam bedeuten, muss es aber nicht. Ebenso möglich sind:

- legitime Newsletter oder andere Bulk-Mail,
- automatische GitHub-/CI-/Ticket-Benachrichtigungen,
- Monitoring- und Alarmierungsstürme,
- mehrfach zugestellte identische Nachrichten,
- schnelle echte Konversationen,
- Fehlkonfigurationen eines sendenden Systems,
- eine Spam- oder Phishing-Kampagne,
- ein kompromittiertes Absenderkonto.

Die Workbench soll solche Häufungen daher zunächst **erkennen, gruppieren und erklärbar beschreiben**. Eine Spam- oder Sicherheitsbewertung ist ein nachgelagerter fachlicher Schritt.

Leitgrundsatz:

> Häufigkeit und Ähnlichkeit sind starke Signale, aber kein alleiniger Beweis für Spam.

## Abgrenzung zur exakten Deduplizierung

ADR-005 und die Anforderungen LF-DUP-* behandeln die beweisbare Identität von Rohmails. Diese Deduplizierung dient der Datenintegrität und verhindert insbesondere, dass identische temporäre Kopien bei `Get all` als zusätzliche kanonische Nachrichten übernommen werden.

Message Pattern & Burst Detection verfolgt einen anderen Zweck:

- **exakte Deduplizierung:** Sind zwei Rohmails bytegenau dieselbe Nachricht?
- **Pattern Detection:** Gehören unterschiedliche Nachrichten wahrscheinlich zu demselben wiederkehrenden Muster?
- **Burst Detection:** Tritt ein solches Muster in ungewöhnlich kurzer Zeit besonders häufig auf?
- **Security/Spam Classification:** Welche Bedeutung hat dieses Muster?

Diese Ebenen dürfen nicht vermischt werden.

## Grundmodell

Eine einzelne Nachricht kann mehrere neutrale Analysebefunde besitzen. Ein Gruppierungsbefund beschreibt zunächst nur die beobachtete Struktur.

Beispiel:

```text
17 Nachrichten
gleicher Absender
ähnlicher normalisierter Betreff
innerhalb von 6 Minuten
92 % Textähnlichkeit

=> PatternType: RepetitiveBurst
=> Count: 17
=> Confidence: 0.97
```

Wichtig:

```text
RepetitiveBurst != Spam
```

Erst ein Spam-, Security-, Monitoring- oder anderes Fachmodul interpretiert den Befund.

## Relevante Signale

Die Erkennung soll kombinierbare Signale verwenden können. Dazu gehören insbesondere:

- From-Absender,
- Sender und Reply-To,
- Envelope-/Return-Path,
- Absender-Domain,
- normalisierter Betreff,
- Message-ID,
- In-Reply-To und References,
- vorhandene Thread-Zuordnung,
- Empfangszeit und Zeitabstände,
- Empfängerliste,
- normalisierter Plaintext,
- normalisiertes HTML,
- Text-/Strukturähnlichkeit,
- Links und Ziel-Domains,
- Anhänge und Anhangshashes,
- Nachrichtengröße,
- SPF-, DKIM- und DMARC-Ergebnisse, sofern vorhanden,
- bekannte Rollen eines Absenders, z. B. Monitoring, Newsletter oder automatischer Dienst.

Kein einzelnes Signal soll allein eine Spam-Klassifikation erzwingen.

## Betreffnormalisierung

Betreffzeilen sollen für die Mustererkennung normalisiert werden können.

Beispiel:

```text
RE: Rechnung 4711
AW: Rechnung 4711
Re: Re: Rechnung 4711
[EXTERN] Rechnung 4711
```

kann für Vergleichszwecke auf ein gemeinsames Grundmuster reduziert werden:

```text
rechnung 4711
```

Die Normalisierung darf den ursprünglichen Betreff nicht ersetzen. Originaldaten bleiben unverändert erhalten; die normalisierte Form ist ausschließlich ein abgeleitetes Analysemerkmal.

Später kann zusätzlich eine vorsichtige Erkennung dynamischer Bestandteile sinnvoll sein, etwa bei:

```text
Backup failed on gps01 at 22:01
Backup failed on gps01 at 22:06
Backup failed on gps01 at 22:11
```

Hier kann ein gemeinsames Muster vorliegen, obwohl sich Zeitstempel unterscheiden.

## Zeitfenster und Burst-Erkennung

Die Anzahl ähnlicher Mails ist ohne Zeitbezug nur begrenzt aussagekräftig.

Beispiel:

```text
100 ähnliche Mails in 5 Jahren
```

ist grundsätzlich anders zu bewerten als:

```text
100 ähnliche Mails in 90 Sekunden
```

Die Analyse soll deshalb konfigurierbare bzw. fachlich geeignete Zeitfenster unterstützen, beispielsweise:

- 1 Minute,
- 5 Minuten,
- 15 Minuten,
- 1 Stunde,
- 24 Stunden.

Das endgültige Modell für Fenster, Schwellenwerte und Persistenz wird vor der Implementierung festgelegt.

## Typische Fallklassen

### Wahrscheinliche Spam- oder Phishing-Kampagne

Beispielbefund:

- sehr viele Nachrichten in kurzer Zeit,
- unbekannter oder unerwarteter Absender,
- nahezu identischer Inhalt,
- ungewöhnliche oder wechselnde Ziel-Domains,
- fehlgeschlagene oder widersprüchliche Authentifizierung,
- keine bestehende Korrespondenz.

Ein solcher Burst darf den Spam-/Security-Score deutlich erhöhen, soll aber seine Gründe einzeln erklärbar machen.

### Newsletter / legitime Bulk-Mail

Beispielbefund:

- bekannter Absender,
- regelmäßiges Muster,
- passende Authentifizierung,
- typische Bulk-/List-Header,
- gegebenenfalls List-Unsubscribe.

Das Ergebnis kann als BulkMail oder Newsletter gruppiert werden, ohne automatisch eine Sicherheitswarnung zu erzeugen.

### Monitoring- oder Alarmierungssturm

Beispiel:

```text
monitoring@example.org
"Host gps01 unreachable"
37 Nachrichten in 18 Minuten
```

Das ist möglicherweise kein Spam, aber ein wichtiges operatives Ereignis. Die Workbench soll daraus eine zusammengefasste Darstellung ermöglichen, statt den Benutzer 37-mal unabhängig zu benachrichtigen.

### Automatischer Dienst

Dienste wie GitHub, CI/CD-Systeme, Shops, Versandplattformen oder Ticket-Systeme können legitimerweise viele ähnliche Nachrichten versenden.

Langfristig kann ein Absender- oder Dienstprofil die Interpretation verbessern:

```text
SenderRole = AutomatedService
```

### Echte Konversation

Ein schneller Mailwechsel darf nicht als Spam verschwinden. Threadmerkmale wie In-Reply-To, References, wechselnde Absender und inhaltliche Unterschiede sind deshalb wichtige Gegenindikatoren.

## Exakte Mehrfachzustellung

Mehrfach zugestellte, inhaltlich exakt gleiche Nachrichten sind zusätzlich von semantisch ähnlichen Nachrichten zu unterscheiden.

Für beweisbare Rohmailidentität gilt weiterhin ADR-005 mit SHA-256 über Originalbytes, Konto-ID und Länge.

Für Diagnose und UI kann dennoch relevant sein, dass eine identische Nachricht mehrfach zugestellt oder beobachtet wurde. Solche Zustellungen sollen nicht unbemerkt gelöscht werden, wenn dadurch diagnostische Information verloren ginge.

## Message Groups

Als fachliche Abstraktion wird ein späteres `MessageGroup`-Modell vorgesehen.

Beispiel:

```text
MessageGroup
    Id
    GroupType
    FirstSeen
    LastSeen
    MessageCount
    RepresentativeMessage
    Pattern
    Confidence
    RiskScore?          // nur falls von Security geliefert
```

Mögliche Gruppentypen:

```text
Thread
Newsletter
AutomatedNotification
MonitoringIncident
DuplicateDelivery
RepetitiveBurst
SuspectedSpamCampaign
Unknown
```

Die konkrete Persistenzstruktur ist noch keine verbindliche Architekturentscheidung.

## UI-Verhalten

Die Nachrichtenliste soll zusammengehörige oder stark repetitive Gruppen später verdichtet darstellen können.

Beispiel:

```text
Database unavailable — 23 ähnliche Meldungen seit 22:14
```

Neue passende Nachrichten erhöhen den Gruppenzähler, statt zwingend eine neue gleichwertige sichtbare Warnung zu erzeugen.

Wichtig ist, dass der Benutzer die Gruppe jederzeit aufklappen und alle zugrunde liegenden Nachrichten sehen kann.

Die Gruppierung darf keine Rohmail verbergen oder vernichten.

## Integration mit Priority Mail Watcher

Der Priority Mail Watcher soll einen erkannten Burst als gemeinsamen Vorgang behandeln können.

Statt:

```text
23 x "WICHTIGE MAIL"
```

soll beispielsweise möglich sein:

```text
CRITICAL: Database unavailable
23 ähnliche Meldungen seit 22:14
```

Eine neue Nachricht kann den Zähler, die letzte Beobachtungszeit und gegebenenfalls den Risikostatus aktualisieren.

Der Watcher soll die Pattern Detection nutzen, aber keine eigene konkurrierende Ähnlichkeitslogik implementieren.

## Integration mit Todo / Aufgaben

Ein Burst darf nicht automatisch viele identische Aufgaben erzeugen.

Aus:

```text
Mail 1: Backup fehlgeschlagen
Mail 2: Backup fehlgeschlagen
...
Mail 36: Backup fehlgeschlagen
```

soll fachlich beispielsweise eine Aufgabe entstehen können:

```text
Backupfehler auf gps01 untersuchen

Betroffene Nachrichten: 36
Erster Fehler: 22:01
Letzter Fehler: 23:17
Status: weiterhin aktiv
```

Die Aufgabe referenziert die Gruppe bzw. deren Nachrichten. Die Entscheidung, ob überhaupt eine Aufgabe angelegt wird, liegt beim Todo-/Workflow-Modul und nicht beim Pattern Detector.

## Integration mit Notifications

Notification-Logik soll repetitive Ereignisse zusammenfassen können. Pattern Detection liefert dafür einen neutralen Befund.

Mögliche Strategien:

- erste Nachricht normal benachrichtigen,
- weitere ähnliche Nachrichten in einem kurzen Zeitfenster zusammenfassen,
- Zähler und letzte Beobachtungszeit aktualisieren,
- bei Eskalation eine neue Benachrichtigung erzeugen,
- Quiet-Hours-Regeln weiterhin zentral beachten.

Damit soll verhindert werden, dass ein Alarmsturm selbst einen Benachrichtigungssturm erzeugt.

## Integration mit Security und Spam-Erkennung

Pattern Detection ist eine Eingangsquelle für Security, nicht deren Ersatz.

Zielrichtung:

```text
Mail Intake
     |
     v
Normalization
     |
     v
Message Fingerprint
     |
     v
Pattern / Burst Detector
     |
     +-------------> Spam / Security
     |
     +-------------> Priority Watcher
     |
     +-------------> Todo
     |
     +-------------> Notifications
     |
     +-------------> UI Grouping
```

Security kann zusätzlich SPF, DKIM, DMARC, Domainabweichungen, Linkanalyse, Markenprofile, Attachment-Befunde und weitere Indikatoren berücksichtigen.

## Erweiterungsarchitektur

Die Funktion soll zu ADR-008 passen: kleine stabile Erweiterungsverträge vor einem offenen dynamischen Plugin-System.

Ein Pattern Detector kann später einen neutralen, versionierten Befund veröffentlichen, beispielsweise konzeptionell:

```text
MailPatternDetected
    PatternType
    MessageIds
    RepresentativeMessageId
    FirstSeen
    LastSeen
    Count
    Confidence
```

Andere Module interpretieren diesen Befund unabhängig.

Grundsatz:

> Ein Modul erkennt eine Tatsache; ein anderes Modul entscheidet über die fachliche Konsequenz.

Dadurch können Security, Todo, Watcher und Notifications dasselbe Analyseergebnis nutzen, ohne sich hart gegenseitig zu referenzieren.

## Spätere Anomalieerkennung

Als spätere Ausbaustufe kann das System historische Basiswerte pro Absender, Domain oder Dienst berücksichtigen.

Beispiel:

```text
Normal:
2-5 Nachrichten pro Woche
bekannte Betreffmuster
bekannte Link-Domains

Aktuell:
73 Nachrichten in 3 Minuten
neues HTML-Muster
neue Ziel-Domain
```

Eine solche Baseline-/Anomalieerkennung ist ausdrücklich **kein V1-Zwang**. Für eine erste Implementierung sind deterministische Regeln, Zeitfenster und nachvollziehbare Ähnlichkeitsmerkmale vorzuziehen.

## Sicherheits- und Datenintegritätsregeln

Folgende Leitplanken gelten:

1. Eine Mail wird nicht allein deshalb als Spam eingestuft, weil viele ähnliche Nachrichten vorliegen.
2. Viele ähnliche Nachrichten dürfen nicht allein aufgrund ihrer Häufigkeit automatisch gelöscht werden.
3. Automatisches Löschen erfordert wesentlich stärkere Evidenz oder eine ausdrückliche Benutzerregel.
4. Rohmails und ursprüngliche Header/Betreffzeilen bleiben unverändert.
5. Normalisierung, Ähnlichkeitswerte, Gruppen und Scores sind abgeleitete Daten.
6. Jeder sicherheitsrelevante Befund muss seine wesentlichen Gründe nachvollziehbar anzeigen können.
7. Gruppen müssen auf die zugrunde liegenden Nachrichten zurückführbar bleiben.
8. Pattern Detection darf die bestehende exakte Deduplizierungslogik nicht ersetzen.

## Empfohlene erste Implementierungs-Slice

Eine spätere erste Slice sollte bewusst klein bleiben:

1. normalisierten Absender und Betreff ableiten,
2. Nachrichten in definierten Zeitfenstern vergleichen,
3. einfache Gruppen für gleichen Absender + ähnlichen Betreff bilden,
4. FirstSeen, LastSeen und Count persistieren,
5. Gruppen in einer Analyse-/Diagnoseansicht sichtbar machen,
6. keine automatische Löschung und zunächst keine selbständige Spamentscheidung,
7. Tests für legitime Threads, Newsletter, Monitoring-Bursts und verdächtige Massenmails.

Erst danach sollten Body-Similarity, historische Baselines oder automatische Security-Gewichtungen ergänzt werden.

## Offene Entscheidungen

Vor einer verbindlichen Umsetzung sind mindestens zu klären:

- Welche Pattern-Typen gehören ins Domainmodell?
- Welche Normalisierungsregeln sind sprach- und providerunabhängig belastbar?
- Welche Ähnlichkeitsverfahren genügen für die erste Version?
- Welche Zeitfenster und Schwellenwerte sind Standardwerte und welche konfigurierbar?
- Wie werden Message Groups persistiert und aktualisiert?
- Wie interagieren Gruppen mit Threading?
- Welche Befunde darf Security automatisch in einen Spam-Score übernehmen?
- Welche Benutzeraktionen gibt es für falsch gruppierte Nachrichten?
- Wann wird aus einem laufenden Burst ein geschlossener Incident?
- Welche Capability-/Event-Verträge werden für Watcher, Todo und Notifications benötigt?

## Einordnung in die Roadmap

Message Pattern & Burst Detection ist derzeit ein **Konzeptkandidat mit offenem Scope** und keine verbindliche V1-Abnahmeforderung.

Technische Grundlagen liegen bereits in mehreren geplanten Bereichen:

- exakte Deduplizierung und Fingerprints,
- sichere MIME-/Headeranalyse,
- lokales Warnsystem,
- Regeln,
- stabile Extension Contracts.

Eine Aufnahme in einen konkreten Meilenstein sollte erst nach Festlegung des minimalen Domainmodells und der ersten Implementierungs-Slice erfolgen.
