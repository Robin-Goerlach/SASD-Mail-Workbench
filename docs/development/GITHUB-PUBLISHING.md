# Veröffentlichung bei GitHub

Empfohlen wird zunächst ein **privates** Repository.

## Mit GitHub CLI

```powershell
gh auth login
./scripts/publish-to-github.ps1
```

## Manuell über SSH

```powershell
git remote add origin git@github.com:SASD-Sysadmin/sasd-mail-workbench.git
git branch -M main
git push -u origin main
```

Vor dem Push prüfen:

```powershell
git status
git log --oneline --decorate -10
./scripts/verify.ps1
```

Das Repository besitzt bewusst keine Open-Source-Lizenz. Vor einer öffentlichen
Freigabe ist `docs/legal/LICENSING-DECISION.md` zu bearbeiten.
