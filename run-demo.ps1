Write-Host "==> Starte Demo-Maillauf" -ForegroundColor Cyan

dotnet run --project .\src\Sasd.MailClient.Bootstrap.Console\Sasd.MailClient.Bootstrap.Console.csproj
if ($LASTEXITCODE -ne 0) {
    throw "Demo-Lauf ist fehlgeschlagen."
}
