[CmdletBinding()]
param()
$root = Split-Path -Parent $PSScriptRoot
Get-ChildItem $root -Directory -Recurse -Force |
    Where-Object { $_.Name -in @('bin','obj','TestResults','runtime-data') } |
    Sort-Object FullName -Descending |
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$root/artifacts" -Recurse -Force -ErrorAction SilentlyContinue
