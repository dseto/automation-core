# FREE-HANDS Recorder - Modo Exploratorio (RF00)
# Uso:
#   . .\_env.ps1
#   .\run-free-hands.ps1 -Url "https://example.com" -OutputDir ".\artifacts\recorder-custom"

param(
    [string]$Url = "http://localhost/insurance-quote-spa-static",
    [string]$OutputDir = "C:\Projetos\automation-core\ui-tests\artifacts\seguro-sim"
)

$ErrorActionPreference = "Stop"

# 1) Carregar variaveis base
if (-not $PSScriptRoot) { $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition } else { $ScriptDir = $PSScriptRoot }

# Debug visual local
$env:UI_DEBUG = "true"
$env:HEADLESS = "false"
$env:PAUSE_ON_FAILURE = "false"
$env:SLOWMO_MS = "1000"
$env:HIGHLIGHT = "true"
$env:BROWSER = "edge"
$env:UI_MAP_PATH = "C:\\Projetos\\automation-core\\ui-tests\\ui\\ui-map-segurosim.yaml"

# 2) Habilitar recorder (modo exploratorio)
$env:AUTOMATION_RECORD = "true"

# 3) Diretório de saída (resolver para caminho absoluto e garantir existencia)
if ($OutputDir -ne "") {
    if ([System.IO.Path]::IsPathRooted($OutputDir)) { $env:RECORD_OUTPUT_DIR = $OutputDir } else { $env:RECORD_OUTPUT_DIR = Join-Path (Get-Location) $OutputDir }
} elseif (-not $env:RECORD_OUTPUT_DIR) {
    $env:RECORD_OUTPUT_DIR = Join-Path (Join-Path $ScriptDir "..\..") "artifacts\recorder"
}
if (-not (Test-Path $env:RECORD_OUTPUT_DIR)) { New-Item -ItemType Directory -Path $env:RECORD_OUTPUT_DIR -Force | Out-Null }
$env:RECORD_OUTPUT_DIR = (Resolve-Path -Path $env:RECORD_OUTPUT_DIR).Path

# 4) URL inicial
if ($Url -ne "") {
    $env:BASE_URL = $Url
} elseif (-not $env:BASE_URL) {
    Write-Host "[ERRO] BASE_URL nao definida. Use: .\run-free-hands.ps1 -Url 'https://...' ou configure em _env.ps1" -ForegroundColor Red
    exit 1
}

# 5) Modo headed para exploracao
$env:HEADLESS = "false"
$env:UI_DEBUG = "true"

# 6) Garantir diretorio de saida
$outputPath = [System.IO.Path]::GetFullPath($env:RECORD_OUTPUT_DIR)
if (-not (Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
}

# 7) Info
Write-Host ""
Write-Host "FREE-HANDS Recorder (RF00)"
Write-Host "BASE_URL:          $env:BASE_URL"
Write-Host "RECORD_OUTPUT_DIR: $outputPath"
Write-Host "BROWSER:           $env:BROWSER"
Write-Host ""
Write-Host "Para encerrar: feche o browser ou CTRL+C"
Write-Host ""

# 8) Rodar RecorderTool
$projectPath = Join-Path $PSScriptRoot "..\..\src\Automation.RecorderTool\Automation.RecorderTool.csproj"
if (-not (Test-Path $projectPath)) {
    Write-Host ("[ERRO] Projeto RecorderTool nao encontrado em: " + $projectPath) -ForegroundColor Red
    exit 1
}

dotnet run --project $projectPath

exit $LASTEXITCODE
