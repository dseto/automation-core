---
applyTo: '**'
---
# Copilot Instructions — Spec Driven Automation Platform

Este repositório segue **Spec-Driven Design**.

## Fonte de verdade (SSOT)
Antes de alterar qualquer código, SEMPRE consulte:
- specs/releases/delta/* (entregas)
- specs/backend/rules/*
- specs/backend/implementation/run-settings.md
- specs/tests/validation/*
- specs/api/schemas/*
- specs/api/examples/*

## Fluxo obrigatório
1. Ler o delta pack da entrega.
2. Atualizar specs se necessário.
3. Só então alterar código.
4. Rodar restore/build/test a cada etapa.

## Regras
- Mudanças pequenas e rastreáveis.
- Não quebrar compatibilidade sem delta pack + migration.md.
- Validator deve refletir as regras do spec.
- Não adicionar dependência de framework de teste dentro de Automation.Reqnroll.

## Padrões importantes
- UiMap aceita testId e test_id.
- DataMap aceita {{dataset}} e valores literais não devem ser reprocessados.
- Selenium Manager é a fonte de drivers.
- RunSettings devem refletir specs/backend/implementation/run-settings.md.

## Organização de arquivos de teste
- Todos os testes devem ser criados dentro de `ui-tests/`
- Scripts PowerShell: `ui-tests/scripts/`
- Páginas HTML de teste: `ui-tests/pages/`
- Artifacts/evidências: `ui-tests/artifacts/` (configurar via `$env:RECORD_OUTPUT_DIR`)
- Features Gherkin: `ui-tests/features/`
- UI Maps: `ui-tests/ui/`

## Sempre entregar
- Lista de arquivos alterados.
- Resumo das mudanças.
- Como validar.


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
- `dotnet test .\ui-tests\UiTests.csproj`
- `dotnet run --project .\src\Automation.Validator\Automation.Validator.csproj -- validate --ui-map .\ui-tests\ui\ui-map.yaml --features .\ui-tests\features`

## Config (env vars)
Respeitar `RunSettings.FromEnvironment()`:
- UI_DEBUG, HEADLESS, SLOWMO_MS, HIGHLIGHT, PAUSE_ON_FAILURE
- WAIT_ANGULAR, ANGULAR_TIMEOUT_MS, STEP_TIMEOUT_MS
- BASE_URL, UI_MAP_PATH
- AUTOMATION_RECORD, RECORD_OUTPUT_DIR (padrão: `artifacts/recorder`)

## Anti-patterns
- XPath frágil / seletor por texto como padrão
- Sleeps para estabilidade
- Lógica de fluxo no YAML
- Aumentar escopo sem atualizar specs/tests
