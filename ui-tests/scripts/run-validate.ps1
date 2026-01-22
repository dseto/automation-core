
param(
  # Caminho para o projeto Automation.Validator do Automation.Core
  [string]$ValidatorProject = "C:\Projetos\automation-core\src\Automation.Validator\Automation.Validator.csproj",
  # Validar apenas ui-map (sem validar features)
  [switch]$UiMapOnly,
  # Validar apenas data-map (sem validar features)
  [switch]$DataMapOnly
)

$ErrorActionPreference = "Stop"
if (-not $PSScriptRoot) { $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition } else { $ScriptDir = $PSScriptRoot }
. (Join-Path $ScriptDir "_env.ps1")

# Resolver paths com data-map se necessário
if (-not $env:DATA_MAP_PATH) { 
  $env:DATA_MAP_PATH = (Resolve-Path (Join-Path $ScriptDir "..\data\data-map.yaml") -ErrorAction SilentlyContinue).Path
}

Write-Host "== Validation with Automation.Validator =="
Write-Host "ValidatorProject: $ValidatorProject"
Write-Host "BASE_URL: $env:BASE_URL"
Write-Host "UI_MAP_PATH: $env:UI_MAP_PATH"
if ($env:DATA_MAP_PATH) { Write-Host "DATA_MAP_PATH: $env:DATA_MAP_PATH" }
Write-Host "FEATURES_PATH: $env:FEATURES_PATH"
Write-Host ""

# 1. Verificar se os arquivos base existem
if (-not (Test-Path $env:UI_MAP_PATH)) {
  Write-Host "[ERROR] ui-map.yaml nao encontrado em $env:UI_MAP_PATH"
  exit 1
}
Write-Host "[OK] ui-map.yaml encontrado"

if ($env:DATA_MAP_PATH -and -not (Test-Path $env:DATA_MAP_PATH)) {
  Write-Host "[ERROR] data-map.yaml nao encontrado em $env:DATA_MAP_PATH"
  exit 1
}
if ($env:DATA_MAP_PATH) { Write-Host "[OK] data-map.yaml encontrado" }

if (-not (Test-Path $env:FEATURES_PATH)) {
  Write-Host "[ERROR] features/ nao encontrado em $env:FEATURES_PATH"
  exit 1
}
Write-Host "[OK] features/ encontrado"
Write-Host ""

# 2. Validar ui-map.yaml com Automation.Validator
Write-Host "== Validating UI Map =="
& dotnet run --project $ValidatorProject -- validate --ui-map $env:UI_MAP_PATH
if ($LASTEXITCODE -ne 0) {
  Write-Host "[ERROR] UI Map validation failed"
  exit 1
}
Write-Host "[OK] UI Map validation passed"
Write-Host ""

# 3. Validar data-map.yaml se existir
if ($env:DATA_MAP_PATH -and -not $UiMapOnly) {
  Write-Host "== Validating Data Map =="
  & dotnet run --project $ValidatorProject -- validate --data-map $env:DATA_MAP_PATH
  if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Data Map validation failed"
    exit 1
  }
  Write-Host "[OK] Data Map validation passed"
  Write-Host ""
}

# 4. Validar features contra contratos (se não estiver em modo UiMapOnly)
if (-not $UiMapOnly -and -not $DataMapOnly) {
  Write-Host "== Validating Features =="
  $features = Get-ChildItem "$env:FEATURES_PATH\*.feature" -ErrorAction SilentlyContinue
  $featureCount = ($features | Measure-Object).Count
  
  if ($featureCount -eq 0) {
    Write-Host "[WARN] Nenhum arquivo .feature encontrado em $env:FEATURES_PATH"
  } else {
    Write-Host "[OK] $featureCount feature(s) encontrada(s)"
    
    # Validar cada feature contra os contratos
    foreach ($feature in $features) {
      Write-Host "  Validando: $($feature.Name)"
      $args = @("--project", $ValidatorProject, "--", "validate", "--gherkin", $feature.FullName, "--ui-map", $env:UI_MAP_PATH)
      
      if ($env:DATA_MAP_PATH) {
        $args += @("--data-map", $env:DATA_MAP_PATH)
      }
      
      & dotnet run @args
      if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERROR] Validation failed for $($feature.Name)"
        exit 1
      }
    }
    Write-Host "[OK] All features validated successfully"
  }
  Write-Host ""
}

Write-Host "== Validation completed successfully =="
exit 0

