# Initializes local secret files from templates (never commits real secrets).
$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionRoot = Resolve-Path (Join-Path $scriptRoot "../..")

$targets = @(
    @{
        Example = "src/BanquetHallManagement.HttpApi.Host/appsettings.secrets.json.example"
        Secrets = "src/BanquetHallManagement.HttpApi.Host/appsettings.secrets.json"
    },
    @{
        Example = "src/BanquetHallManagement.DbMigrator/appsettings.secrets.json.example"
        Secrets = "src/BanquetHallManagement.DbMigrator/appsettings.secrets.json"
    },
    @{
        Example = "test/BanquetHallManagement.HttpApi.Client.ConsoleTestApp/appsettings.secrets.json.example"
        Secrets = "test/BanquetHallManagement.HttpApi.Client.ConsoleTestApp/appsettings.secrets.json"
    }
)

foreach ($target in $targets) {
    $examplePath = Join-Path $solutionRoot $target.Example
    $secretsPath = Join-Path $solutionRoot $target.Secrets

    if (-not (Test-Path $examplePath)) {
        Write-Warning "Example not found: $examplePath"
        continue
    }

    if (Test-Path $secretsPath) {
        Write-Host "Keeping existing secrets file: $secretsPath"
        continue
    }

    Copy-Item $examplePath $secretsPath
    Write-Host "Created $secretsPath from example. Edit it or use user secrets / environment variables."
}

Write-Host ""
Write-Host "Optional: load HttpApi.Host user secrets from template:"
Write-Host "  dotnet user-secrets set --project src/BanquetHallManagement.HttpApi.Host < etc/secrets/user-secrets.host.template.json"
Write-Host ""
Write-Host "Environment variable examples:"
Write-Host "  ConnectionStrings__Default"
Write-Host "  AuthServer__CertificatePassPhrase"
Write-Host "  StringEncryption__DefaultPassPhrase"
Write-Host "  Seed__AbpAdmin__Password"
Write-Host "  Seed__ApplicationAdmin__Password"
