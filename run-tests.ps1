Write-Host "==> Starte Testprojekt" -ForegroundColor Cyan

dotnet run --project .\tests\Sasd.MailClient.Tests\Sasd.MailClient.Tests.csproj
if ($LASTEXITCODE -ne 0) {
    throw "Mindestens ein Test ist fehlgeschlagen."
}
