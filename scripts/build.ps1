[CmdletBinding()]
param([ValidateSet('Debug','Release')][string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    dotnet build Sasd.MailWorkbench.sln --configuration $Configuration --no-restore
} finally {
    Pop-Location
}
