param(
    [string]$SessionExample = "ui-tests\artifacts\seguro-sim\session.json",
    [string]$UiMap = "ui-tests\ui\ui-map-segurosim.yaml",
    [string]$OutputDir = "ui-tests\artifacts\semantic-resolution-segurosim",
    [string]$ScenarioName = "cenariosegurosim"
)

$ErrorActionPreference = "Stop"

# Resolve absolute paths
$root = (Get-Location).Path
$sessionPath = (Resolve-Path $SessionExample).Path
$uiMapPath = (Resolve-Path $UiMap).Path
$outDir = Join-Path $root $OutputDir
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }

Write-Host "Generating draft from session: $sessionPath"
# Build args for robust invocation: pass each argument as separate parameter to avoid quoting/concatenation issues
$dotnetArgs = @("--project", "src/Automation.RecorderTool", "generate-draft", "--input", $sessionPath, "--output", $outDir)
if (-not [string]::IsNullOrWhiteSpace($ScenarioName)) { $dotnetArgs += @("--scenario", $ScenarioName) }
Write-Host "dotnet run $($dotnetArgs -join ' ')"
$exit = & dotnet run @dotnetArgs
if ($LASTEXITCODE -ne 0) { Write-Host "generate-draft failed (exit $LASTEXITCODE)"; exit $LASTEXITCODE }

$draftPath = Join-Path $outDir "draft.feature"
$draftMeta = Join-Path $outDir "draft.metadata.json"
if (-not (Test-Path $draftPath) -or -not (Test-Path $draftMeta)) { Write-Host "Draft generation incomplete"; exit 1 }

Write-Host "Resolving draft with ui-map: $uiMapPath"
$resolvedOut = Join-Path $outDir "resolved"
if (-not (Test-Path $resolvedOut)) { New-Item -ItemType Directory -Path $resolvedOut | Out-Null }
$cmdResolve = "dotnet run --project src/Automation.RecorderTool resolve-draft --draft `"$draftPath`" --metadata `"$draftMeta`" --session `"$sessionPath`" --ui-map `"$uiMapPath`" --output `"$resolvedOut`""
Write-Host $cmdResolve
& dotnet run --project src/Automation.RecorderTool resolve-draft --draft "$draftPath" --metadata "$draftMeta" --session "$sessionPath" --ui-map "$uiMapPath" --output "$resolvedOut"
if ($LASTEXITCODE -ne 0) { Write-Host "resolve-draft failed (exit $LASTEXITCODE)"; exit $LASTEXITCODE }

$resolvedMeta = Join-Path $resolvedOut "resolved.metadata.json"
$uiGaps = Join-Path $resolvedOut "ui-gaps.report.json"
if (-not (Test-Path $resolvedMeta) -or -not (Test-Path $uiGaps)) { Write-Host "Resolver outputs missing"; exit 1 }

Write-Host "Validating outputs with Automation.Validator"
& dotnet run --project src/Automation.Validator validate --resolved "$resolvedMeta" --ui-gaps "$uiGaps"
if ($LASTEXITCODE -ne 0) { Write-Host "Validation failed (exit $LASTEXITCODE)"; exit $LASTEXITCODE }

Write-Host "Semantic resolution E2E succeeded. Outputs in: $resolvedOut" -ForegroundColor Green

# Cleanup temporary draft artifacts to avoid polluting workspace and test discovery filters
$draftPath = Join-Path $outDir "draft.feature"
$draftMeta = Join-Path $outDir "draft.metadata.json"
$draftCs = Join-Path $outDir "draft.feature.cs"
try {
    if (Test-Path $draftPath) { Remove-Item -Path $draftPath -Force; Write-Host "Removed temporary $draftPath" }
    if (Test-Path $draftMeta) { Remove-Item -Path $draftMeta -Force; Write-Host "Removed temporary $draftMeta" }
    if (Test-Path $draftCs) { Remove-Item -Path $draftCs -Force; Write-Host "Removed temporary $draftCs" }
} catch {
    Write-Host "Warning: failed to remove draft artifacts: $_" -ForegroundColor Yellow
}

exit 0
