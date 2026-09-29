$runtimeDirectory = Join-Path $PSScriptRoot 'runtime-data'

if (Test-Path $runtimeDirectory) {
    Write-Host "Loesche $runtimeDirectory" -ForegroundColor Yellow
    Remove-Item $runtimeDirectory -Recurse -Force
}
else {
    Write-Host "Kein runtime-data Verzeichnis vorhanden." -ForegroundColor DarkYellow
}
