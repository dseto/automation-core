
param(
  [string]$TestProject = "",
  [string]$Scenario = ""
)

$ErrorActionPreference = "Stop"

# Resolve script directory robustly so the script can be executed from any working directory
if (-not $PSScriptRoot) { $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition } else { $ScriptDir = $PSScriptRoot }

# Default test project path relative to this script (resolved to absolute)
if (-not $TestProject) {
  $TestProject = (Resolve-Path (Join-Path $ScriptDir "..\UiTests.csproj") -ErrorAction SilentlyContinue).Path
}

$env:BASE_URL = "https://gray-mushroom-0d87c190f.1.azurestaticapps.net"

# Debug visual local
$env:UI_DEBUG = "true"
$env:HEADLESS = "false"
$env:PAUSE_ON_FAILURE = "false"
$env:SLOWMO_MS = "1000"
$env:HIGHLIGHT = "true"
$env:BROWSER = "edge"
$env:UI_MAP_PATH = "C:\\Projetos\\automation-core\\ui-tests\\ui\\ui-map.yaml"

# Debug visual local
$env:UI_DEBUG = "true"
$env:HEADLESS = "false"
if (-not $env:SLOWMO_MS) { $env:SLOWMO_MS = "1000" }
if (-not $env:HIGHLIGHT) { $env:HIGHLIGHT = "true" }
if (-not $env:PAUSE_ON_FAILURE) { $env:PAUSE_ON_FAILURE = "false" }
$env:SLOWMO_MS = "1000"
$env:HIGHLIGHT = "true"
Write-Host "== Debug Visual (headed) =="
Write-Host "UI_DEBUG=$env:UI_DEBUG HEADLESS=$env:HEADLESS SLOWMO_MS=$env:SLOWMO_MS"
Write-Host "HIGHLIGHT=$env:HIGHLIGHT PAUSE_ON_FAILURE=$env:PAUSE_ON_FAILURE"

if ([string]::IsNullOrWhiteSpace($Scenario)) {
  dotnet test $TestProject --filter "Category=login" -- RunConfiguration.MaxCpuCount=1
} else {
  dotnet test $TestProject --filter "FullyQualifiedName~$Scenario" -- RunConfiguration.MaxCpuCount=1
}
exit $LASTEXITCODE
