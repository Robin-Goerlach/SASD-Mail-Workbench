Write-Host "==> Stelle NuGet-Pakete wieder her" -ForegroundColor Cyan

dotnet restore .\Sasd.MailClient.sln
if ($LASTEXITCODE -ne 0) {
    throw "Restore ist fehlgeschlagen."
}

Write-Host "==> Baue SASD MailClient Solution" -ForegroundColor Cyan

dotnet build .\Sasd.MailClient.sln --no-restore
if ($LASTEXITCODE -ne 0) {
    throw "Build ist fehlgeschlagen."
}

Write-Host "Build erfolgreich abgeschlossen." -ForegroundColor Green
