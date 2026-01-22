
param(
  # Caminho para o projeto de testes (xUnit + Reqnroll bindings)
  [string]$TestProject = ""
)

$ErrorActionPreference = "Stop"
if (-not $PSScriptRoot) { $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition } else { $ScriptDir = $PSScriptRoot }
. (Join-Path $ScriptDir "_env.ps1")
if (-not $TestProject) { $TestProject = (Resolve-Path (Join-Path $ScriptDir "..\UiTests.csproj") -ErrorAction SilentlyContinue).Path }

# CI-like (explícito, sem operador ??)
$env:HEADLESS = "true"
$env:UI_DEBUG = "false"
$env:SLOWMO_MS = "0"
$env:HIGHLIGHT = "false"
$env:PAUSE_ON_FAILURE = "false"

Write-Host "== Smoke (headless) =="
dotnet test $TestProject --filter "Category=smoke" -- RunConfiguration.MaxCpuCount=1
exit $LASTEXITCODE
