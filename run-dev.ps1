$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$rootDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiUrl = "http://localhost:5079"
$frontendUrl = "http://localhost:5173"
$frontendDir = Join-Path $rootDir "frontend"

function Resolve-CommandPath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Name
    )

    $cmd = Get-Command $Name -ErrorAction SilentlyContinue
    if (-not $cmd) {
        throw "Comando '$Name' nao encontrado no PATH."
    }

    return $cmd.Source
}

$dotnetCmd = Resolve-CommandPath -Name "dotnet"
$npmCmd = Get-Command "npm.cmd" -ErrorAction SilentlyContinue
if (-not $npmCmd) {
    $npmCmd = Get-Command "npm" -ErrorAction SilentlyContinue
}
if (-not $npmCmd) {
    throw "Comando 'npm' nao encontrado no PATH."
}
$npmCmd = $npmCmd.Source

$apiProcess = $null
$frontendProcess = $null
$stopRequested = $false

$cancelHandler = [ConsoleCancelEventHandler]{
    param($sender, $eventArgs)
    $script:stopRequested = $true
    $eventArgs.Cancel = $true
    Write-Host "`nEncerrando processos..." -ForegroundColor Yellow
}

[Console]::add_CancelKeyPress($cancelHandler)

try {
    Write-Host "[1/2] Iniciando API em $apiUrl" -ForegroundColor Cyan
    $apiProcess = Start-Process `
        -FilePath $dotnetCmd `
        -ArgumentList @("run", "--project", "src/ComprarProgramada.API", "--urls", $apiUrl) `
        -WorkingDirectory $rootDir `
        -NoNewWindow `
        -PassThru

    Write-Host "[2/2] Iniciando frontend em $frontendUrl" -ForegroundColor Cyan
    if (-not (Test-Path (Join-Path $frontendDir "node_modules"))) {
        Write-Host "Instalando dependencias do frontend..." -ForegroundColor Yellow
        Push-Location $frontendDir
        try {
            & $npmCmd install
        }
        finally {
            Pop-Location
        }
    }

    $env:VITE_API_BASE_URL = $apiUrl
    $frontendProcess = Start-Process `
        -FilePath $npmCmd `
        -ArgumentList @("run", "dev", "--", "--host", "localhost", "--port", "5173") `
        -WorkingDirectory $frontendDir `
        -NoNewWindow `
        -PassThru

    Write-Host ""
    Write-Host "Aplicacao em execucao:" -ForegroundColor Green
    Write-Host "- Swagger:  $apiUrl/swagger"
    Write-Host "- Frontend: $frontendUrl"
    Write-Host ""
    Write-Host "Pressione Ctrl+C para encerrar ambos os processos."

    while (-not $stopRequested) {
        if (($apiProcess -and $apiProcess.HasExited) -or ($frontendProcess -and $frontendProcess.HasExited)) {
            break
        }

        Start-Sleep -Seconds 1
    }
}
finally {
    [Console]::remove_CancelKeyPress($cancelHandler)

    foreach ($proc in @($frontendProcess, $apiProcess)) {
        if ($proc -and -not $proc.HasExited) {
            Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
        }
    }

    Remove-Item Env:VITE_API_BASE_URL -ErrorAction SilentlyContinue
}
