[CmdletBinding()]
param([switch]$LockedMode)
$ErrorActionPreference = 'Stop'
& "$PSScriptRoot/restore.ps1" -LockedMode:$LockedMode
& "$PSScriptRoot/build.ps1" -Configuration Release
& "$PSScriptRoot/test.ps1" -Configuration Release
Write-Host 'Restore, Build und Tests wurden erfolgreich abgeschlossen.' -ForegroundColor Green
