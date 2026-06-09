$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionRoot = Join-Path $scriptRoot "../../"

function Get-CertificatePassPhrase {
    if ($env:AuthServer__CertificatePassPhrase) {
        return $env:AuthServer__CertificatePassPhrase
    }

    if ($env:AUTH_SERVER_CERTIFICATE_PASSPHRASE) {
        return $env:AUTH_SERVER_CERTIFICATE_PASSPHRASE
    }

    $secretsPath = Join-Path $solutionRoot "src/BanquetHallManagement.HttpApi.Host/appsettings.secrets.json"
    if (Test-Path $secretsPath) {
        $secrets = Get-Content $secretsPath -Raw | ConvertFrom-Json
        if ($secrets.AuthServer.CertificatePassPhrase) {
            return $secrets.AuthServer.CertificatePassPhrase
        }
    }

    throw "Certificate passphrase is not configured. Set AuthServer__CertificatePassPhrase or create appsettings.secrets.json from appsettings.secrets.json.example."
}

Write-Host "Building the solution..."
Set-Location $solutionRoot
dotnet build

if ($LASTEXITCODE -ne 0) {
    [Console]::Error.WriteLine("dotnet build FAILED with exit code $LASTEXITCODE")
    exit -1
}

$certPassPhrase = Get-CertificatePassPhrase
$jobs = @()

$jobs += Start-Job -Name "InstallLibs" -ScriptBlock {
    $ErrorActionPreference = "Stop"
    Set-Location (Join-Path $using:scriptRoot "../../")
    abp install-libs

    if ($LASTEXITCODE -ne 0) {
        throw "abp install-libs exited with code $LASTEXITCODE"
    }
}

$jobs += Start-Job -Name "DbMigrator" -ScriptBlock {
    $ErrorActionPreference = "Stop"
    Set-Location (Join-Path $using:scriptRoot "../../src/BanquetHallManagement.DbMigrator")
    dotnet run
    dotnet run

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet run (DbMigrator) exited with code $LASTEXITCODE"
    }
}

$jobs += Start-Job -Name "DevCert" -ScriptBlock {
    $ErrorActionPreference = "Stop"
    Set-Location (Join-Path $using:scriptRoot "../../src/BanquetHallManagement.HttpApi.Host")
    dotnet dev-certs https -v -ep openiddict.pfx -p $using:certPassPhrase

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet dev-certs exited with code $LASTEXITCODE"
    }
}

Wait-Job $jobs | Out-Null
$jobs | Receive-Job

$failed = $jobs | Where-Object { $_.State -eq 'Failed' }
$hasError = $failed.Count -gt 0

if ($hasError) {
    foreach ($job in $failed) {
        [Console]::Error.WriteLine("Job '$($job.Name)' FAILED")
    }

    Remove-Job $jobs | Out-Null
    exit -1
}

Remove-Job $jobs | Out-Null
exit 0
