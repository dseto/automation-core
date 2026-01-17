param(
  [string]$ScenarioFilter = "Login_com_sucesso",
  [string]$BaseUrl = ""  # em apps reais, setar URL do ambiente; para demo usaremos file://
)

$env:UI_DEBUG="true"
$env:HEADLESS="false"
$env:SLOWMO_MS="250"
$env:HIGHLIGHT="true"
$env:PAUSE_ON_FAILURE="true"
$env:WAIT_ANGULAR="false"
if ($BaseUrl -ne "") { $env:BASE_URL=$BaseUrl }

Write-Host "DEBUG VISUAL ON (Edge headed)..." -ForegroundColor Yellow
Write-Host "Scenario filter: $ScenarioFilter" -ForegroundColor Yellow

# Para E2E local com demo HTML, habilite
$env:RUN_E2E="true"
dotnet test .\tests\Automation.Acceptance.Tests\Automation.Acceptance.Tests.csproj --filter "FullyQualifiedName~$ScenarioFilter"
exit $LASTEXITCODE
