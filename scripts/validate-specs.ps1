
Write-Host "=== SPEC DRIVEN VALIDATION PIPELINE ==="

Write-Host "`n[1/4] dotnet restore"
dotnet restore
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`n[2/4] dotnet build"
dotnet build
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`n[3/4] dotnet test"
dotnet test
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`n[4/4] (placeholder) Spec validation"
Write-Host "TODO: JSON Schema + step-catalog validation"

Write-Host "`nALL CHECKS PASSED"
