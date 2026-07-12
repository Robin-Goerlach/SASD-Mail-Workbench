[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    dotnet run --project src/Sasd.MailWorkbench.Bootstrap.Console --configuration Release
} finally {
    Pop-Location
}
