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
} finally {
    Pop-Location
}
