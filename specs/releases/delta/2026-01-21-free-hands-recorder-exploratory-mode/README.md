
# Delta Pack — 2026-01-21-free-hands-recorder-exploratory-mode

> Status: 🟢 **RELEASED** — v0.4.0 (2026-01-21)  
> Tipo: **Spec correction / scope hardening**  
> Alvo: corrigir desvio crítico — Recorder não pode depender de `.feature`/cenário/steps  
> Audit: [RF00-COMPLIANCE-AUDIT.md](./RF00-COMPLIANCE-AUDIT.md)

## Objetivo
Formalizar e tornar **normativo** o requisito estrutural do FREE-HANDS Recorder:

> Quando `AUTOMATION_RECORD=true`, o sistema DEVE abrir o browser em **modo exploratório**, permitindo interação manual livre, **sem depender de cenário de teste, arquivo `.feature` ou step definitions**.

Este delta corrige a ambiguidade que permitiu uma implementação acoplada ao pipeline de testes.

## Escopo
- Adição do requisito **RF00 — Modo exploratório** (pré-condição)
- Regras globais de **proibição de acoplamento** ao pipeline de testes (Reqnroll/Gherkin)
- Atualização da spec de implementação para definir o **fluxo de inicialização** do modo exploratório
- Atualização da validação (critérios e checks mínimos)

## Fora de escopo
- Novos tipos de evento, schemas, geração de draft, gaps, UIMap, Strict/Traction.

## Impacto
- **Não-breaking** para execução normal (AUTOMATION_RECORD=false).
- Pode exigir refactor interno para separar “runner de testes” vs “runner exploratório”.

## Arquivos incluídos
- specs/backend/requirements/free-hands-recorder-session.md (atualizado: RF00 + regras)
- specs/backend/implementation/free-hands-recorder-session.md (atualizado: fluxo exploratório)
- specs/tests/validation/recorder-session-validation.md (atualizado: critérios do RF00)

## Checklist de fechamento
- [x] RF00 implementado e testado localmente
- [x] Execução em modo exploratório funciona sem qualquer `.feature`
- [x] session.json é gerado ao encerrar o run
- [x] Modo normal (tests) permanece funcionando
- [x] Acoplamentos a testes removidos (RecorderHooks, RecorderSteps, feature file)
- [x] Standalone RecorderTool criado (sem dependência a Reqnroll)
- [x] Build verifica (0 errors)
- [x] RF00-COMPLIANCE-AUDIT.md gerado

## ✅ Implementação Completa

### Arquivos Criados
- ✅ `src/Automation.RecorderTool/Automation.RecorderTool.csproj`
- ✅ `src/Automation.RecorderTool/Program.cs` (entry point standalone)
- ✅ `ui-tests/scripts/run-free-hands.ps1` (PowerShell launcher)
- ✅ `specs/releases/delta/2026-01-21-free-hands-recorder-exploratory-mode/RF00-COMPLIANCE-AUDIT.md`

### Arquivos Removidos (Violações de RF00)
- ❌ `ui-tests/Steps/RecorderHooks.cs` (acoplado a BeforeScenario)
- ❌ `ui-tests/Steps/RecorderSteps.cs` (step definitions desnecessários)
- ❌ `ui-tests/features/recorder-session.feature` (requisito .feature violava RF00)

### Verificação Final
```
Build: ✅ 0 errors
Tests: ✅ Backward compatible
RF00:  ✅ Exploratory mode works WITHOUT .feature
Exit:  ✅ exit 0 on success
```

Ver: [RF00-COMPLIANCE-AUDIT.md](./RF00-COMPLIANCE-AUDIT.md)
