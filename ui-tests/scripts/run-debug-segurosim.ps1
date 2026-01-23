
param(
  [string]$TestProject = "",
  [string]$Scenario = "cenariosegurosim"
)

$ErrorActionPreference = "Stop"

# Resolve script directory robustly so the script can be executed from any working directory
if (-not $PSScriptRoot) { $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition } else { $ScriptDir = $PSScriptRoot }

# Default test project path relative to this script (resolved to absolute)
if (-not $TestProject) {
  $TestProject = (Resolve-Path (Join-Path $ScriptDir "..\UiTests.csproj") -ErrorAction SilentlyContinue).Path
}

$env:BASE_URL = "http://localhost/insurance-quote-spa-static"

# Debug visual local
$env:UI_DEBUG = "true"
$env:HEADLESS = "false"
$env:PAUSE_ON_FAILURE = "false"
$env:SLOWMO_MS = "100"
$env:HIGHLIGHT = "true"
$env:BROWSER = "edge"
$env:UI_MAP_PATH = "C:\\Projetos\\automation-core\\ui-tests\\ui\\ui-map-segurosim.yaml"
Write-Host "== Debug Visual (headed) =="
Write-Host "UI_DEBUG=$env:UI_DEBUG HEADLESS=$env:HEADLESS SLOWMO_MS=$env:SLOWMO_MS"
Write-Host "HIGHLIGHT=$env:HIGHLIGHT PAUSE_ON_FAILURE=$env:PAUSE_ON_FAILURE"

# NOTE: For safety, tests invoked by this debug script will temporarily disable recorder (AUTOMATION_RECORD)
# while running `dotnet test`. This prevents accidental overwrites of sample session files (e.g., ui-tests/artifacts/seguro-sim/session.json)
# in case the environment or CI has recording enabled globally.
# We restore the original AUTOMATION_RECORD value after the test completes.

if ([string]::IsNullOrWhiteSpace($Scenario)) {
  $resolvedFeature = "C:\\Projetos\\automation-core\\ui-tests\\artifacts\\semantic-resolution-segurosim\\resolved\\resolved.feature"
  if (Test-Path $resolvedFeature) {
    Write-Host "== Running scenarios from $resolvedFeature =="
    $scenarios = Select-String -Path $resolvedFeature -Pattern '^\s*Scenario(?: Outline)?:\s*(.+)$' | ForEach-Object { $_.Matches[0].Groups[1].Value.Trim() } | Where-Object { $_ -ne "" }
    if ($scenarios -and $scenarios.Count -gt 0) {
      $filterParts = $scenarios | ForEach-Object { "FullyQualifiedName~`"$_`"" }
      $filter = $filterParts -join '|'
      Write-Host "Filter: $filter"
      $__oldAutomationRecord = $env:AUTOMATION_RECORD
      try {
        $env:AUTOMATION_RECORD = "false"
        dotnet test $TestProject --filter "$filter" -- RunConfiguration.MaxCpuCount=1
      } finally {
        if ($null -eq $__oldAutomationRecord) { Remove-Item env:AUTOMATION_RECORD -ErrorAction SilentlyContinue } else { $env:AUTOMATION_RECORD = $__oldAutomationRecord }
      }
    } else {
      Write-Host "No scenarios found in $resolvedFeature. Running default Category=login"
      $__oldAutomationRecord = $env:AUTOMATION_RECORD
      try {
        $env:AUTOMATION_RECORD = "false"
        dotnet test $TestProject --filter "Category=login" -- RunConfiguration.MaxCpuCount=1
      } finally {
        if ($null -eq $__oldAutomationRecord) { Remove-Item env:AUTOMATION_RECORD -ErrorAction SilentlyContinue } else { $env:AUTOMATION_RECORD = $__oldAutomationRecord }
      }
    }
  } else {
    Write-Host "Resolved feature not found at $resolvedFeature. Running default Category=login"
    $__oldAutomationRecord = $env:AUTOMATION_RECORD
    try {
      $env:AUTOMATION_RECORD = "false"
      dotnet test $TestProject --filter "Category=login" -- RunConfiguration.MaxCpuCount=1
    } finally {
      if ($null -eq $__oldAutomationRecord) { Remove-Item env:AUTOMATION_RECORD -ErrorAction SilentlyContinue } else { $env:AUTOMATION_RECORD = $__oldAutomationRecord }
    }
  }
} else {
  $__oldAutomationRecord = $env:AUTOMATION_RECORD
  try {
    $env:AUTOMATION_RECORD = "false"
    dotnet test $TestProject --filter "FullyQualifiedName~$Scenario" -- RunConfiguration.MaxCpuCount=1
  } finally {
    if ($null -eq $__oldAutomationRecord) { Remove-Item env:AUTOMATION_RECORD -ErrorAction SilentlyContinue } else { $env:AUTOMATION_RECORD = $__oldAutomationRecord }
  }
}
exit $LASTEXITCODE
