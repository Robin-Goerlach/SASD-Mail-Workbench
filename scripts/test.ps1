[CmdletBinding()]
param([ValidateSet('Debug','Release')][string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    New-Item -ItemType Directory -Force -Path artifacts/test-results | Out-Null
    dotnet test Sasd.MailWorkbench.sln `
        --configuration $Configuration `
        --no-build `
        --logger "trx;LogFileName=test-results.trx" `
        --results-directory artifacts/test-results
} finally {
    Pop-Location
}
