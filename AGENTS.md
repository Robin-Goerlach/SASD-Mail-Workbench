# Repository Guidelines

## Project Structure & Module Organization

`Sasd.MailWorkbench.sln` is the main solution. `src/` contains Domain, Application.Contracts, Application, ExtensionModel, Infrastructure, Persistence, and Bootstrap.Console projects. Matching test projects live in `tests/`, including architecture dependency checks. Keep Domain and Application independent of UI and SQLite dependencies.

Use `docs/` for architecture, ADRs, development notes, and image assets; `samples/` for sample inputs; and `artifacts/test-results/` for generated test reports. `SASD-Mail-Workbench-Priority-Mail-Watcher/` contains a separate integration bundle with its own source, tests, and integration instructions; read `PRIORITY-WATCHER-INTEGRATION.md` before changing its integration.

## Build, Test, and Development Commands

Use the .NET 8 SDK specified in `global.json` (8.0.422, allowing later patches); PowerShell 7 is recommended. Run from the repository root:

- `./scripts/restore.ps1`: restore dependencies and generate package lock files.
- `./scripts/build.ps1`: build the main solution in Release mode without restoring.
- `./scripts/test.ps1`: run the previously built Release tests and write TRX reports.
- `./scripts/verify.ps1`: run restore, build, and tests in sequence.
- `./scripts/run-demo.ps1`: run the console bootstrap demonstration.

Commit generated `packages.lock.json` files; once present, use `./scripts/verify.ps1 -LockedMode`. CI verifies the main solution on Windows and Ubuntu.

## Coding Style & Naming Conventions

Follow `.editorconfig`: UTF-8, LF endings, four-space C# indentation, and two spaces for Markdown, JSON, XML, and YAML. Use C# 12, file-scoped namespaces, braces on new lines, explicit types, and System imports first. Nullable reference types and .NET analyzers are enabled. Use PascalCase for types and methods, and add XML comments to public types and nontrivial methods.

## Testing Guidelines

Use NUnit with `NUnit3TestAdapter`. Name fixtures `<Type>Tests` and methods descriptively, such as `Constructor_RejectsInvalidHash`. Every bug fix needs a reproducing test. Cover affected behavior and preserve architecture checks; no numeric coverage threshold is configured.

## Commit & Pull Request Guidelines

History mixes short messages with `chore:` commits. Follow `CONTRIBUTING.md` for new commits: `feat(storage): add atomic raw-message commit`. Keep each commit focused on one change.

Complete the PR template with purpose, validation, risks, and rollback details. Confirm build success, tests or justification, architecture boundaries, and relevant documentation/ADR updates.

## Security & Data Integrity

Never commit credentials, real email addresses, or production emails. Preserve raw-mail bytes before canonical hashing; local deduplication must never delete server mail.
