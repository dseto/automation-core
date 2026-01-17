param(
  [string]$UiMapPath = ".\samples\ui\ui-map.yaml",
  [string]$FeaturesPath = ".\samples\features"
)

Write-Host "Running Validator validate..." -ForegroundColor Cyan
dotnet run --project .\src\Automation.Validator\Automation.Validator.csproj -- validate --ui-map $UiMapPath --features $FeaturesPath
exit $LASTEXITCODE
