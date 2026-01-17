---
applyTo: '**'
---
Provide project context and coding guidelines that AI should follow when generating code, answering questions, or reviewing changes.

# Instruções do Projeto — Automation Core (Copilot)

## Contexto
Este repositório implementa um framework corporativo de automação de UI:
- C# / .NET 8
- Selenium WebDriver
- Microsoft Edge (único browser)
- Reqnroll (BDD) + xUnit
- Alvo frequente: Angular 17+ (wait best-effort via Angular Testability + fallback)

## SSOT (manda sempre)
O spec deck em `specs/` é a fonte única de verdade.
- Antes de alterar comportamento, localize o requisito no spec.
- Se precisar divergir, atualize o spec e registre decisão (ADR) — não inventar comportamento silenciosamente.

## Regras “não negociáveis”
- Edge-only (sem Playwright)
- Seletores por `data-testid` como padrão
- `ui-map.yaml` é dicionário (sem flows/lógica)
- Runtime Resolution (não gerar bindings por app)
- Angular wait best-effort com hard-timeout + fallback
- Debug visual local (headed/slowmo/highlight/pause) é requisito

## Guidelines de código
- Evitar `Thread.Sleep` fora de debug.
- Falhar rápido com mensagens claras (QA/DEV).
- Logging com `Microsoft.Extensions.Logging`.
- Evidências em falha: screenshot + html + metadata + steps.log (best-effort).
- Classes pequenas e coesas (SRP).

## Testes e validação
Após alterações, garantir:
- `dotnet build`
- `dotnet test .\tests\Automation.Core.Tests\Automation.Core.Tests.csproj`
- `dotnet run --project .\src\Automation.Validator\Automation.Validator.csproj -- validate --ui-map .\samples\ui\ui-map.yaml --features .\samples\features`

## Config (env vars)
Respeitar `RunSettings.FromEnvironment()`:
- UI_DEBUG, HEADLESS, SLOWMO_MS, HIGHLIGHT, PAUSE_ON_FAILURE, PAUSE_EACH_STEP
- WAIT_ANGULAR, ANGULAR_TIMEOUT_MS, STEP_TIMEOUT_MS
- BASE_URL, UI_MAP_PATH

## Anti-patterns
- XPath frágil / seletor por texto como padrão
- Sleeps para estabilidade
- Lógica de fluxo no YAML
- Aumentar escopo sem atualizar specs/tests
