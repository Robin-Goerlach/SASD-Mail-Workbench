[CmdletBinding()]
param(
    [switch]$SkipVerification
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repositoryRoot 'Sasd.MailWorkbench.sln'

if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
    throw "Sasd.MailWorkbench.sln wurde nicht unter '$repositoryRoot' gefunden. Das Skript muss aus dem SASD-Mail-Workbench-Repository ausgeführt werden."
}

$projectPaths = @(
    'src/Sasd.MailWorkbench.Notification.Domain/Sasd.MailWorkbench.Notification.Domain.csproj',
    'src/Sasd.MailWorkbench.Notification.Contracts/Sasd.MailWorkbench.Notification.Contracts.csproj',
    'src/Sasd.MailWorkbench.Notification.Application/Sasd.MailWorkbench.Notification.Application.csproj',
    'src/Sasd.MailWorkbench.Notification.Infrastructure/Sasd.MailWorkbench.Notification.Infrastructure.csproj',
    'src/Sasd.MailWorkbench.Host.Tray/Sasd.MailWorkbench.Host.Tray.csproj',
    'tests/Sasd.MailWorkbench.Notification.Domain.Tests/Sasd.MailWorkbench.Notification.Domain.Tests.csproj',
    'tests/Sasd.MailWorkbench.Notification.Application.Tests/Sasd.MailWorkbench.Notification.Application.Tests.csproj',
    'tests/Sasd.MailWorkbench.Notification.Architecture.Tests/Sasd.MailWorkbench.Notification.Architecture.Tests.csproj'
)

foreach ($relativeProjectPath in $projectPaths) {
    $absoluteProjectPath = Join-Path $repositoryRoot $relativeProjectPath
    if (-not (Test-Path -LiteralPath $absoluteProjectPath -PathType Leaf)) {
        throw "Erwartetes Projekt fehlt: $relativeProjectPath"
    }
}

Push-Location $repositoryRoot
try {
    $solutionProjects = (& dotnet sln $solutionPath list 2>&1 | Out-String)
    if ($LASTEXITCODE -ne 0) {
        throw "Die vorhandene Solution konnte nicht gelesen werden.`n$solutionProjects"
    }

    # dotnet sln gibt je nach Betriebssystem unterschiedliche Pfadtrenner aus.
    # Die Normalisierung hält das Skript auch in Windows- und Linux-CI idempotent.
    $normalizedSolutionProjects = $solutionProjects.Replace('\', '/')

    foreach ($relativeProjectPath in $projectPaths) {
        if ($normalizedSolutionProjects -notmatch [regex]::Escape($relativeProjectPath)) {
            Write-Host "Füge Projekt hinzu: $relativeProjectPath"
            & dotnet sln $solutionPath add $relativeProjectPath
            if ($LASTEXITCODE -ne 0) {
                throw "Projekt konnte nicht zur Solution hinzugefügt werden: $relativeProjectPath"
            }
        }
        else {
            Write-Host "Projekt bereits enthalten: $relativeProjectPath"
        }
    }

    $adrIndexPath = Join-Path $repositoryRoot 'docs/adr/README.md'
    $adrRow = '| [ADR-016-priority-mail-watcher-companion-process](ADR-016-priority-mail-watcher-companion-process.md) | Priority Mail Watcher als separater Companion-Prozess | Akzeptiert |'
    if (Test-Path -LiteralPath $adrIndexPath -PathType Leaf) {
        $adrIndexContent = Get-Content -LiteralPath $adrIndexPath -Raw
        if ($adrIndexContent -notmatch 'ADR-016-priority-mail-watcher-companion-process') {
            Add-Content -LiteralPath $adrIndexPath -Value "`n$adrRow"
            Write-Host 'ADR-016 wurde im ADR-Index ergänzt.'
        }
    }

    if (-not $SkipVerification) {
        Write-Host 'Führe dotnet clean aus...'
        & dotnet clean $solutionPath
        if ($LASTEXITCODE -ne 0) { throw 'dotnet clean ist fehlgeschlagen.' }

        Write-Host 'Führe dotnet restore aus...'
        & dotnet restore $solutionPath
        if ($LASTEXITCODE -ne 0) { throw 'dotnet restore ist fehlgeschlagen.' }

        Write-Host 'Führe dotnet build aus...'
        & dotnet build $solutionPath --configuration Release --no-restore
        if ($LASTEXITCODE -ne 0) { throw 'dotnet build ist fehlgeschlagen.' }

        Write-Host 'Führe dotnet test aus...'
        & dotnet test $solutionPath --configuration Release --no-build
        if ($LASTEXITCODE -ne 0) { throw 'dotnet test ist fehlgeschlagen.' }
    }

    Write-Host 'Priority-Mail-Watcher-Integration abgeschlossen.'
}
finally {
    Pop-Location
}
