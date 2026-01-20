
param(
  # Caminho para o projeto de testes (xUnit + Reqnroll bindings)
  [string]$TestProject = "C:\Projetos\metrics-simple-frontend\ui-tests\UiTests.csproj"
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\_env.ps1"

# CI-like (explícito, sem operador ??)
$env:HEADLESS = "true"
$env:UI_DEBUG = "false"
$env:SLOWMO_MS = "0"
$env:HIGHLIGHT = "false"
$env:PAUSE_ON_FAILURE = "false"
$env:PAUSE_EACH_STEP = "false"

Write-Host "== Smoke (headless) =="
dotnet test $TestProject --filter "Category=smoke" -- RunConfiguration.MaxCpuCount=1
exit $LASTEXITCODE
