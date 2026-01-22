# Delta: 2026-01-21-forced-waits (v0.6.0)

**Status:** RELEASED

**Data de fechamento:** 2026-01-21

## Classificação
- Normativo: RF07 (Recorder Session), RF17 (Draft Generator), Step Catalog (Gherkin)
- Direcional: N/A

## Objetivo
Adicionar suporte a **esperas explícitas** no pipeline:
1) Recorder registra espera relevante entre eventos em `session.json` (`waitMs`)
2) Draft Generator materializa a espera em `draft.feature` via step `E eu espero X segundos`
3) Step Catalog inclui o step e contrato para execução no runner

## Outputs afetados
- `specs/api/schemas/recorder.session.schema.json` (schema)
- `specs/api/examples/recorder.session.login.example.json` (exemplo)
- `specs/api/examples/draft.feature.example.feature` (exemplo)
- `specs/backend/requirements/free-hands-recorder-session.md` (RF07)
- `specs/backend/requirements/free-hands-draft-generator.md` (RF17)
- `specs/tests/gherkin/step-catalog.yaml` (step)
- `specs/tests/gherkin/step-catalog.md` (documentação do step)

## Defaults (Copilot-safe)
- `RECORD_WAIT_LOG_THRESHOLD_SECONDS = 1.0`

---

## Checklist de validação (executado)
- [x] WAIT-01 — Schema + exemplo do Recorder (schema aceita `waitMs` e exemplo valida)
- [x] WAIT-02 — Threshold de emissão (`waitMs` emitido apenas quando gap > threshold) — testado com unit tests
- [x] WAIT-03 — Draft materializa espera (draft contém `E eu espero <segundos> segundos`) — validado via DraftGenerator behavior and example
- [x] WAIT-04 — Step catalog (step `eu espero ... segundos` presente e aceita decimal)

---

## Observações de compatibilidade
- Backward compatible: `waitMs` é opcional. Consumidores antigos devem ignorar o campo se não o reconhecerem.
- Nenhum breaking change identificado.
