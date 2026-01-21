# FREE-HANDS Recorder - Modo Exploratorio (RF00)
# Uso:
#   . .\_env.ps1
#   .\run-free-hands.ps1 -Url "https://example.com" -OutputDir ".\artifacts\recorder-custom"

param(
    [string]$Url = "",
    [string]$OutputDir = ""
)

$ErrorActionPreference = "Stop"

# 1) Carregar variaveis base
. "$PSScriptRoot\_env.ps1"

# 2) Habilitar recorder (modo exploratorio)
$env:AUTOMATION_RECORD = "true"

# 3) Diretório de saída
if ($OutputDir -ne "") {
    $env:RECORD_OUTPUT_DIR = (Resolve-Path $OutputDir -Relative)
} elseif (-not $env:RECORD_OUTPUT_DIR) {
    $env:RECORD_OUTPUT_DIR = (Resolve-Path "$PSScriptRoot\..\..\artifacts\recorder" -Relative)
}

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
