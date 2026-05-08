$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionRoot = Join-Path $scriptRoot "../../"

Write-Host "Building the solution..."
Set-Location $solutionRoot
dotnet build

if ($LASTEXITCODE -ne 0) {
    [Console]::Error.WriteLine("dotnet build FAILED with exit code $LASTEXITCODE")
    exit -1
}

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
    dotnet dev-certs https -v -ep openiddict.pfx -p ed0653a5-a03d-435c-a129-cb0e2fb15ffa

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