
param(
  [string]$TestProject = "C:\Projetos\metrics-simple-frontend\ui-tests\UiTests.csproj",
  [string]$Scenario = ""
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\_env.ps1"

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
