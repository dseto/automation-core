# Changes

## Summary
This release fixes unsafe semantic rewriting that could change literal fill values and normalizes route handling for initial navigations.

## Highlights
- **RF-SR-40 (Behavioral fix):** Semantic Resolver now preserves literal fill values by limiting replacement to the first quoted literal when appropriate.
- **RF-RN-01 (Bugfix):** Route Normalizer maps initial navigations equal to the configured `BASE_URL` to `route: "/"` to avoid emitting base paths in recorded routes.
- **Draft materialization:** `DraftGenerator` now preserves explicit navigation steps and qualifies unqualified targets to reduce UI gaps in generated drafts.
- **Reqnroll bindings & stability:** Added `Given` variants for interaction steps, improved wildcard-route handling to avoid invalid navigations, and added script improvements (`--scenario` support, draft cleanup, recorder-safety during debug runs).

## Implementation
Files modified include (non-exhaustive):
- `src/Automation.Core/Recorder/Semantic/SemanticResolver.cs`
- `src/Automation.Core/Recorder/Draft/DraftGenerator.cs`
- `src/Automation.Core/Driver/RouteNormalizer.cs`
- `src/Automation.Reqnroll/Steps/InteractionSteps.cs`
- `src/Automation.Reqnroll/Steps/NavigationSteps.cs`
- `src/Automation.RecorderTool/Program.cs`
- Updated scripts: `ui-tests/scripts/run-debug-segurosim.ps1`, `run-semantic-resolution_*.ps1`
- Tests: `SemanticResolutionTests`, `RouteNormalizerTests`, `ActionGrouperTests`, `SemanticE2ETests`

## Validation
- Unit and integration tests added/updated and executed locally.
- Manual debug run (`ui-tests/scripts/run-debug-segurosim.ps1`) executed and validated local success.

## Migration
No migration steps are required.
