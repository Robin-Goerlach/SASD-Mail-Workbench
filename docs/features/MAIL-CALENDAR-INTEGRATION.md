# Mail-zentrierter Kalender, Wiedervorlage und Terminplanung

Status: Konsolidiertes Produkt- und Architekturkonzept; noch keine verbindliche V1-Abnahmeforderung.  
Stand: 2026-11-06

## Zweck dieses Dokuments

Dieses Dokument bündelt die bisherigen Überlegungen zu Kalender-, Termin-, Frist- und Wiedervorlagefunktionen der SASD Mail Workbench. Es trennt bewusst zwischen der bereits festgelegten strategischen Richtung und noch offenen Produkt- und Architekturentscheidungen.

Der Kalender soll **kein Selbstzweck und kein Nachbau von Outlook oder Thunderbird** werden. Sein Wert für die Mail Workbench entsteht dort, wo zeitbezogene Arbeit unmittelbar aus Nachrichten hervorgeht.

Leitbild:

```text
Mail -> erkennen -> bewerten -> planen -> erinnern -> erledigen
```

## Strategische Produktgrenze

Die SASD Mail Workbench bleibt eine spezialisierte Mail-Workbench. Ein vollständiger PIM-Kalender gehört nicht automatisch zum Produktumfang, nur weil klassische Mailprogramme einen solchen enthalten.

Der Kalenderbereich soll deshalb zuerst die mailbezogenen Arbeitsabläufe abdecken:

- aus einer Mail einen Termin ableiten,
- aus einer Mail eine Frist ableiten,
- eine Mail auf Wiedervorlage setzen,
- einen erwarteten Follow-up-Zeitpunkt festhalten,
- eine Termineinladung aus einer Mail verarbeiten,
- eine zeitbezogene Erinnerung wieder in den Arbeitsfluss bringen.

Eine Monats-, Wochen- oder Tagesansicht kann später sinnvoll sein, ist aber nicht der primäre fachliche Kern.

## Fachliche Unterscheidung

Zeitbezogene Objekte sollen nicht undifferenziert als „Kalendereintrag“ behandelt werden.

### Termin

Ein Ereignis findet zu einem bestimmten Zeitpunkt oder in einem Zeitraum statt.

Beispiel: Besprechung am 27. August um 14:00 Uhr.

### Frist / Deadline

Etwas muss spätestens bis zu einem bestimmten Zeitpunkt erledigt sein.

Beispiel: Angebot bis 31. August beantworten.

### Wiedervorlage

Eine Nachricht oder ein Vorgang soll zu einem bestimmten Zeitpunkt erneut Aufmerksamkeit bekommen.

Beispiel: Diese Mail am Montagmorgen erneut anzeigen.

### Follow-up

Zu einem Vorgang wird zu einem späteren Zeitpunkt eine Reaktion oder Kontrolle erwartet.

Beispiel: Wenn bis Freitag keine Antwort eingegangen ist, den Vorgang erneut anzeigen.

Diese Begriffe sind als fachliche Arbeitsgrundlage gedacht. Das endgültige Domainmodell wird vor Implementierungsbeginn festgelegt.

## Mail als Ausgangspunkt

Der wesentliche Mehrwert liegt in der direkten Verbindung zwischen Nachricht und zeitbezogener Planung.

```text
E-MAIL
  |
  +-- enthält Termin? -----------> Termin
  |
  +-- enthält Frist? ------------> Deadline
  |
  +-- später bearbeiten? --------> Wiedervorlage
  |
  +-- Antwort erwartet? ---------> Follow-up
  |
  +-- Einladung enthalten? ------> Calendar Event
  |
  +-- Aufgabe enthalten? --------> Task-Kandidat
                                     |
                                     v
                              Zeitbezogene Planung
                                     |
                                     v
                             Notification Service
```

Automatische Erkennung kann später unterstützen, darf aber eine explizite Benutzerentscheidung nicht unbemerkt ersetzen.

## Erinnerung und Benachrichtigung

Kalenderobjekt und Benachrichtigung sind unterschiedliche fachliche Konzepte.

Ein Termin, eine Frist oder eine Wiedervorlage beschreibt **was wann relevant wird**. Ein Notification Service beschreibt **wie der Benutzer darüber informiert wird**.

Zielarchitektur:

```text
Mail / Watcher / Kalender
          |
          v
   Reminder wird fällig
          |
          v
 Notification Service
          |
          +--> Compact Notification
          |
          +--> Detailed Notification
```

Damit soll vermieden werden, dass Kalender, Mail Watcher und andere Workbench-Komponenten jeweils eigene Erinnerungsmechanismen implementieren.

## Wiedervorlage / Remind later

Die bereits diskutierte Aktion `RemindLater` des Priority-Mail-/Watcher-Konzepts passt fachlich unmittelbar in diese Architektur.

Langfristig soll es keinen getrennten „Watcher-Reminder“ und „Kalender-Reminder“ geben. Beide sollten denselben fachlichen Mechanismus verwenden, soweit ihre Anforderungen kompatibel sind.

Mögliche Zustände/Aktionen:

- Erinnerung planen,
- Erinnerung verschieben,
- Erinnerung erledigen,
- Vorgang stummschalten, soweit fachlich zulässig,
- ursprüngliche Nachricht bzw. den zugehörigen Vorgang öffnen.

Die genaue Zustandsmaschine ist noch festzulegen.

## Quiet Hours

Bereits vorgesehene Ruhezeiten für Benachrichtigungen müssen auch bei zeitbezogenen Workbench-Ereignissen berücksichtigt werden.

Dabei ist zwischen Ereignistypen zu unterscheiden. Eine normale Wiedervorlage kann beispielsweise bis zum Ende der Quiet Hours warten, während andere Meldungsklassen andere Regeln benötigen können.

Der Kalender soll dafür keine separate, konkurrierende Ruhezeitenlogik erhalten.

## Local-first

Der Kalender soll zur bestehenden Local-first-Ausrichtung der Mail Workbench passen.

Grundsatz:

> Der lokale Funktionsumfang darf nicht von einem einzelnen Cloud-Anbieter abhängen.

Ein lokaler Kalender bzw. eine lokale zeitbezogene Planung muss deshalb grundsätzlich funktionsfähig bleiben. Externe Kalender können später als Synchronisationsquellen oder -ziele ergänzt werden.

## Externe Kalender und Provider

Als mögliche spätere Integrationen wurden bzw. werden betrachtet:

- Google Calendar,
- Microsoft-365-/Outlook-Kalender,
- CalDAV,
- lokale Kalenderdaten,
- iCalendar/ICS als Austauschformat.

Die Mail Workbench soll fachlich nicht von einem einzelnen Provider abhängig werden. Eine mögliche Zielrichtung ist eine providerunabhängige Kalenderdomäne mit Adaptern.

```text
Google Calendar ----+
Microsoft 365 ------+
CalDAV -------------+--> SASD Calendar Domain
lokaler Kalender ---+
Mail Workbench -----+
PIMS ---------------+
```

Diese Provider sind **noch keine verbindliche Implementierungsanforderung**.

## Ereignisbasierte Integration

Für externe Systeme ist langfristig eine ereignisbasierte Integration sinnvoll, beispielsweise:

```text
Provider
   |
   v
Webhook / Synchronisationsadapter
   |
   v
SASD-Integrationsschicht
   |
   v
Calendar Domain
```

Eine solche Integrationsschicht sollte providerabhängige Ereignisse normalisieren, statt deren Besonderheiten in die Mail-UI oder die Kalenderdomäne zu tragen.

## Grenze zwischen Mail Workbench und SASD PIMS

Eine zentrale offene Architekturentscheidung ist die fachliche Eigentümerschaft des allgemeinen Kalenders.

Mögliche Zielaufteilung:

```text
SASD PIMS
    |
    +-- allgemeiner persönlicher Kalender
    +-- allgemeine Termine / Aufgaben / Erinnerungen
            ^
            |
SASD Mail Workbench
    |
    +-- Mail -> Termin
    +-- Mail -> Frist
    +-- Mail -> Wiedervorlage
    +-- Mail -> Follow-up
    +-- Einladung aus Mail
            ^
            |
Priority Mail Watcher
    |
    +-- Remind later / Follow-up
```

Eine attraktive Zielrichtung ist, dass PIMS und Mail Workbench **nicht zwei konkurrierende Kalenderkerne** entwickeln. Die Mail Workbench würde dann eine starke Mail-zu-Kalender-Integration bereitstellen, während ein gemeinsamer Kalenderkern bzw. PIMS den allgemeinen Kalender besitzt.

Diese Zuständigkeitsgrenze ist vor einer größeren Kalenderimplementierung verbindlich festzulegen.

## Noch nicht beschlossene Funktionen

Folgende Punkte sind ausdrücklich offen und dürfen nicht als bereits genehmigte Anforderungen interpretiert werden:

- konkrete Tages-, Wochen-, Monats- oder Jahresansicht,
- Agenda-/Listenansicht,
- mehrere Kalender und Kalenderfarben,
- Kategorien,
- Serientermine,
- Geburtstage und Feiertagskalender,
- Ressourcen- und Raumkalender,
- Terminplanung mit mehreren Teilnehmern,
- Free/Busy,
- konkrete Google-Calendar-Anbindung,
- Microsoft Graph / Exchange,
- CalDAV,
- ICS-Import und -Export,
- Verarbeitung von `text/calendar`,
- automatische Erkennung von Termineinladungen,
- RSVP (`Accept`, `Tentative`, `Decline`),
- Versand eigener Termineinladungen,
- Zeitzonenmodell,
- Erinnerungsintervalle und Snooze-Regeln,
- Konflikterkennung,
- gemeinsam genutzte Kalender,
- endgültige Zuständigkeitsgrenze PIMS vs. Mail Workbench.

## Architekturleitlinien

Für eine spätere Umsetzung gelten bereits jetzt folgende Leitplanken:

1. **Keine Kalenderlogik in der WinForms-Ansicht.** Darstellung und fachliche Zeitplanung bleiben getrennt.
2. **Mailreferenzen bleiben nachvollziehbar.** Ein aus einer Nachricht erzeugter Termin, eine Frist oder Wiedervorlage muss zur Ursprungsnachricht zurückführen können.
3. **Local-first.** Cloud-Provider sind Adapter, keine Voraussetzung für die Kernfunktion.
4. **Gemeinsame Reminder-Infrastruktur.** Watcher, Wiedervorlage und Kalender sollen nicht unnötig getrennte Benachrichtigungssysteme besitzen.
5. **Providerneutralität.** Google-, Microsoft- oder CalDAV-spezifische Modelle dürfen die zentrale Kalenderdomäne nicht dominieren.
6. **Persistente, nachvollziehbare Zustände.** Erinnerungen, Verschiebungen und Erledigungen müssen nach Neustarts erhalten bleiben.
7. **Keine stillen Aktionen.** Automatisch erkannte Termine oder Fristen werden nicht ohne nachvollziehbare Benutzerentscheidung verbindlich angelegt.
8. **Zeitzonen und lokale Tagesgrenzen werden explizit modelliert.** Implizite `DateTime`-Annahmen sind zu vermeiden.
9. **Architektur vor Vollausbau.** Zuerst Domainmodell und Zuständigkeitsgrenzen festlegen, danach UI- und Providerumfang.

## Sinnvolle erste Ausbaustufe

Vor einem vollständigen Kalender wäre eine kleine mailzentrierte Slice sinnvoll:

1. Nachricht auswählen.
2. „Wiedervorlage…“ wählen.
3. Datum und Uhrzeit festlegen.
4. Wiedervorlage lokal persistent speichern.
5. Zum Fälligkeitszeitpunkt über den gemeinsamen Notification Service erinnern.
6. Aus der Erinnerung die Originalnachricht öffnen.
7. Erinnerung als erledigt markieren oder erneut verschieben.

Diese Slice würde den Kernnutzen validieren, ohne bereits einen vollständigen PIM-Kalender zu bauen.

### Mögliche Abnahmekriterien dieser späteren Slice

- Wiedervorlage bleibt nach Neustart erhalten.
- Fällige Wiedervorlagen werden genau einmal als aktiver Vorgang präsentiert.
- Originalnachricht ist aus der Erinnerung erreichbar.
- Verschieben erzeugt keinen doppelten Reminder.
- Erledigte Wiedervorlagen werden nicht erneut ausgelöst.
- Quiet-Hours-Regeln werden zentral berücksichtigt.
- Tests decken Persistenz, Fälligkeit, Snooze und Neustart ab.

Diese Kriterien beschreiben **noch keinen freigegebenen V1-Scope**, sondern einen möglichen ersten Implementierungsschritt.

## Zusammenfassung

Der Kalender der SASD Mail Workbench ist primär eine **Mail-zu-Zeit-Integration**:

```text
Nachricht
   |
   +--> Termin
   +--> Frist
   +--> Wiedervorlage
   +--> Follow-up
   +--> Einladung
            |
            v
      gemeinsame Zeitplanung
            |
            v
      Notification Service
```

Die Workbench soll damit nicht zu einem Outlook-Klon werden. Ihr Mehrwert ist, dass Nachrichten zuverlässig in zeitbezogene Arbeitsvorgänge überführt und später wieder in den richtigen Kontext gebracht werden können.

Vor einer größeren Implementierung ist insbesondere zu entscheiden, welche Teile einem gemeinsamen SASD-Kalenderkern bzw. SASD PIMS gehören und welche ausschließlich Mail-Workbench-spezifisch bleiben.
