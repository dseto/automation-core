
# Ajuste aqui 1 vez e todo o kit usa.

# URL base do app (local ou ambiente DEV)
if (-not $env:BASE_URL) { $env:BASE_URL = "https://gray-mushroom-0d87c190f.1.azurestaticapps.net" }

# Paths (relativos a ui-tests)
if (-not $env:UI_MAP_PATH) { $env:UI_MAP_PATH = (Resolve-Path "$PSScriptRoot\..\ui\ui-map.yaml").Path }
if (-not $env:FEATURES_PATH) { $env:FEATURES_PATH = (Resolve-Path "$PSScriptRoot\..\features").Path }

# Flags default (CI-like)
if (-not $env:HEADLESS) { $env:HEADLESS = "true" }
if (-not $env:WAIT_ANGULAR) { $env:WAIT_ANGULAR = "true" }
if (-not $env:ANGULAR_TIMEOUT_MS) { $env:ANGULAR_TIMEOUT_MS = "1000" }
if (-not $env:STEP_TIMEOUT_MS) { $env:STEP_TIMEOUT_MS = "10000" }
# Browser (chrome ou edge)
if (-not $env:BROWSER) { $env:BROWSER = "edge" }

# Onde salvar evidências (se o Core usar Artifacts/ por env var, ajuste aqui conforme seu Core)
if (-not $env:ARTIFACTS_DIR) {
  $artifactsPath = Join-Path $PSScriptRoot "..\artifacts"
  if (-not (Test-Path $artifactsPath)) {
    New-Item -ItemType Directory -Path $artifactsPath -Force | Out-Null
  }
  $env:ARTIFACTS_DIR = (Resolve-Path $artifactsPath).Path
}

# Recorder output (session.json e artefatos do free-hands)
if (-not $env:RECORD_OUTPUT_DIR) {
  $scriptDir = Split-Path -Parent $PSCommandPath
  if (-not $scriptDir) { $scriptDir = $PSScriptRoot }
  if (-not $scriptDir) { $scriptDir = (Get-Location).Path }
  
  $recorderPath = Join-Path (Split-Path -Parent $scriptDir) "artifacts\recorder"
  if (-not (Test-Path $recorderPath)) {
    New-Item -ItemType Directory -Path $recorderPath -Force | Out-Null
  }
  $env:RECORD_OUTPUT_DIR = $recorderPath
}

# (Opcional) credenciais se você tiver bypass login local
if (-not $env:TEST_USER) { $env:TEST_USER = "admin" }
if (-not $env:TEST_PASS) { $env:TEST_PASS = "ChangeMe123!" }

# Selenium WebDriver mirror (oficial Microsoft endpoint)
if (-not $env:SE_DRIVER_MIRROR_URL) { $env:SE_DRIVER_MIRROR_URL = "https://msedgedriver.microsoft.com" }
