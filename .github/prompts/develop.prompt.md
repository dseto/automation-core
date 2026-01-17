---
agent: builder
---
Define the task to achieve, including specific requirements, constraints, and success criteria.

# Tarefa — Implementar Automation Core (Spec-Driven)

## Objetivo
Completar o **MVP do Automation Core** usando o **Spec Deck (SSOT)** em `specs/` e o bootstrap em `src/`/`tests/`.

## SSOT obrigatório (leitura e aderência)
Siga rigorosamente:
- `specs/00-index.md`
- `specs/01-vision-and-scope.md`
- `specs/02-architecture.md`
- `specs/03-repository-structure.md`
- `specs/04-contracts-ui-map-and-gherkin.md`
- `specs/05-runtime-resolution.md`
- `specs/06-waits-and-angular-stability.md`
- `specs/07-preflight-validation.md`
- `specs/08-step-catalog.md`
- `specs/09-evidence-and-logging.md`
- `specs/10-debug-visual-local.md`
- `specs/11-extensions-and-innersource.md`
- `specs/13-testing-strategy.md`
- `specs/backlog.md`

## Restrições fixas
- **Edge only**
- **Selenium WebDriver**
- **Reqnroll + xUnit**
- Seletores: `data-testid` (CSS `[data-testid='x']`)
- **Runtime Resolution** (sem generation layer por app)
- `ui-map.yaml` é dicionário (Page → Friendly → testid)

## Critérios de sucesso (MVP)
1) Evidências em falha (sempre) em `Artifacts/{RunId}/{Feature}/{Scenario}/`:
   - `screenshot.png`, `page.html`, `metadata.json`, `steps.log`
2) Hook Reqnroll captura evidências automaticamente em falha (usar `ScenarioContext.TestError`).
3) StepTrace/log por step: step text, page context, url, friendly/testId/locator (quando aplicável), durationMs, angularFallback.
4) Waits:
   - DOM ready
   - Angular testability best-effort com hard timeout + fallback (sem travar)
   - Element waits com retry básico (stale element) no MVP
5) Validator:
   - `doctor`, `validate`, `plan` funcionam nos `samples/`
   - exit codes: 0 ok, 2 validação, 3 erro interno
6) Debug visual local:
   - `UI_DEBUG=true` força `HEADLESS=false`
   - slowmo/highlight/pauseOnFailure/pauseEachStep funcionais
   - debug visual é **local-only**

## Processo (determinístico)
- Siga `specs/backlog.md` e implemente **um item por vez**.
- A cada item, execute:
  1) `dotnet build`
  2) `dotnet test .\tests\Automation.Core.Tests\Automation.Core.Tests.csproj`
  3) `dotnet run --project .\src\Automation.Validator\Automation.Validator.csproj -- validate --ui-map .\samples\ui\ui-map.yaml --features .\samples\features`
  4) (se mexer em E2E) `RUN_E2E=true dotnet test .\tests\Automation.Acceptance.Tests\Automation.Acceptance.Tests.csproj`
- Corrija falhas antes de avançar.

## Prioridade inicial (primeira execução recomendada)
Implementar **Etapa 3 do backlog: Evidências + Logging (MVP)**:
- `steps.log` (append por step)
- `metadata.json` com contexto e flags
- Hook Reqnroll on-failure para capturar evidências
- Best-effort: erro de evidência/highlight não quebra o teste
