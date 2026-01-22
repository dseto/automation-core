
param(
  [string]$TestProject = ""
)

$ErrorActionPreference = "Stop"

if (-not $PSScriptRoot) { $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition } else { $ScriptDir = $PSScriptRoot }
. (Join-Path $ScriptDir "_env.ps1")

# Habilitar gravação de sessão
$env:AUTOMATION_RECORD = "true"
if (-not $TestProject) { $TestProject = (Resolve-Path (Join-Path $ScriptDir "..\UiTests.csproj") -ErrorAction SilentlyContinue).Path }
$env:RECORD_OUTPUT_DIR = (Join-Path $ScriptDir "..\artifacts\recorder") | Resolve-Path -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Path

# Debug visual para ver o que está sendo gravado
$env:HEADLESS = "false"
$env:UI_DEBUG = "true"
$env:SLOWMO_MS = "500"
$env:HIGHLIGHT = "true"

Write-Host "== Recorder Session Test =="
Write-Host "AUTOMATION_RECORD=$env:AUTOMATION_RECORD"
Write-Host "RECORD_OUTPUT_DIR=$env:RECORD_OUTPUT_DIR"
Write-Host "Session.json será gerado em: $env:RECORD_OUTPUT_DIR"
Write-Host ""

dotnet test $TestProject --filter "FullyQualifiedName~RecorderSession" -- RunConfiguration.MaxCpuCount=1

if ($LASTEXITCODE -eq 0) {
  Write-Host ""
  Write-Host "Teste concluído. Verificando session.json..." -ForegroundColor Green
  $sessionFile = Join-Path $env:RECORD_OUTPUT_DIR "session.json"
  if (Test-Path $sessionFile) {
    Write-Host "session.json gerado:" -ForegroundColor Green
    Get-Content $sessionFile | Write-Host
  } else {
    Write-Host "session.json NÃO foi gerado!" -ForegroundColor Red
  }
}

exit $LASTEXITCODE
