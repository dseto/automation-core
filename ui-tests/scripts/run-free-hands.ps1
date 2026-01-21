# FREE-HANDS Recorder — Modo Exploratório (RF00)
#
# Este script implementa o modo exploratório PURO:
# - Browser abre
# - Usuário interage livremente
# - SEM dependência de .feature, cenários ou step definitions
# - Ao encerrar, gera session.json
#
# Uso:
#   . .\_env.ps1
#   .\run-free-hands.ps1
#   .\run-free-hands.ps1 -Url "https://meuapp.com" -OutputDir ".\artifacts\recorder-custom"

param(
    [string]$Url = "",
    [string]$OutputDir = ""
)

$ErrorActionPreference = "Stop"

# 1) Carregar variáveis de ambiente base
. "$PSScriptRoot\_env.ps1"

# 2) Configurar recorder (via env vars — sem hooks)
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
} else {
    if (-not $env:BASE_URL) {
        Write-Host "[ERRO] BASE_URL não definida. Use: .\run-free-hands.ps1 -Url 'https://...' ou configure em _env.ps1" -ForegroundColor Red
        exit 1
    }
}

# 5) Garantir modo headed (exploratório requer visualização)
$env:HEADLESS = "false"
$env:UI_DEBUG = "true"

# 6) Criar diretório de saída se não existir
$outputPath = [System.IO.Path]::GetFullPath($env:RECORD_OUTPUT_DIR)
if (-not (Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
}

# 7) Exibir configuração
Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  FREE-HANDS Recorder — Modo Exploratório (RF00)                ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuração:"
Write-Host "  BASE_URL:          $env:BASE_URL"
Write-Host "  RECORD_OUTPUT_DIR: $outputPath"
Write-Host "  BROWSER:           $env:BROWSER"
Write-Host ""
Write-Host "Quando terminar:"
Write-Host "  • Feche o browser, OU"
Write-Host "  • Pressione CTRL+C"
Write-Host ""

# 8) Rodar o RecorderTool (entrypoint standalone, sem dependência de testes)
$projectPath = "$PSScriptRoot\..\..\src\Automation.RecorderTool\Automation.RecorderTool.csproj"

if (-not (Test-Path $projectPath)) {
    Write-Host "[ERRO] Projeto RecorderTool não encontrado em: $projectPath" -ForegroundColor Red
    exit 1
}

dotnet run --project $projectPath

exit $LASTEXITCODE
