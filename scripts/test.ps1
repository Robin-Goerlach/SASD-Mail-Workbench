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
        --logger trx `
        --results-directory artifacts/test-results
    if ($LASTEXITCODE -ne 0) { throw "dotnet test failed with exit code $LASTEXITCODE." }
} finally {
    Pop-Location
}
