[CmdletBinding()]
param([switch]$LockedMode)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    if ($LockedMode) {
        dotnet restore Sasd.MailWorkbench.sln --locked-mode
    } else {
        dotnet restore Sasd.MailWorkbench.sln --use-lock-file
    }
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE." }
} finally {
    Pop-Location
}
