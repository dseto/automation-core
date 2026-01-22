# Backlog — follow-up items for Semantic Resolution

1. Recorder normalization: when draining browser events, prefer populating `attributes.data-testid` explicitly instead of only producing a `hint` string. This reduces parsing heuristics in downstream tools and makes session.json cleaner. — **DONE** (2026-01-22)
   - Owner: Recorder team
   - Priority: Medium
   - Files: `src/Automation.RecorderTool/Program.cs` (DrainBrowserEvents and TargetHintBuilder)

2. Add integration test to validate hint normalization at the recorder level. — **DONE** (2026-01-22)

3. Address nullable/analysis warnings found during build (non-blocking).

4. Consider tests for various hint formats (single/double quotes, extra whitespace). — **DONE** (2026-01-22) — Covered by integration tests added in `SemanticResolutionTests.cs`.
