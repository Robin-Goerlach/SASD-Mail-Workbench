[CmdletBinding()]
param(
    [string]$Organization = 'SASD-Sysadmin',
    [string]$Repository = 'sasd-mail-workbench',
    [switch]$Public
)
$ErrorActionPreference = 'Stop'
$visibility = if ($Public) { '--public' } else { '--private' }
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    gh auth status
    gh repo create "$Organization/$Repository" $visibility --source . --remote origin --push
} finally {
    Pop-Location
}
