# Architektur

## Ziel

Die Oberfläche ist ein austauschbarer Adapter. Domain, Application,
Persistenz, Mailprotokolle und Analyse dürfen keine WinForms-Abhängigkeit
enthalten.

## Abhängigkeiten

```text
Domain
  ↑
Application.Contracts
  ↑
Application
  ↑                 ↑
Infrastructure    Persistence

ExtensionModel bleibt unabhängig.
Bootstrap setzt konkrete Implementierungen zusammen.
```

## Projekte

- **Domain:** Zustände, Value Objects und fachliche Regeln.
- **Application.Contracts:** Requests, Reports und UI-neutrale Ergebnisse.
- **Application:** Use Cases und Ports.
- **ExtensionModel:** kleine stabile Verträge; kein Plugin-Lader.
- **Infrastructure:** Zeit, Logging, Hashing und Demoquelle.
- **Persistence:** SQLite, Migrationen und Rohmail-Dateispeicher.
- **Bootstrap.Console:** ausführbare Demonstration und Composition Root.

## Spätere UI

Ab 0.4.0 werden `Presentation.Core`, `Presentation.WinForms` und
`Host.WinForms` ergänzt. Forms enthalten keine MailKit-, SQLite- oder
Geschäftslogik.
