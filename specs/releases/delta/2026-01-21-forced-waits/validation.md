# validation.md

## Gate WAIT-01 — Schema + exemplo do Recorder
**Objetivo:** Garantir que `waitMs` é aceito no schema e exemplos validam.  
**Input:** `specs/api/schemas/recorder.session.schema.json`, `specs/api/examples/recorder.session.login.example.json`  
**Expected output:** Validação PASS.  
**Sinais de falha:** `waitMs` rejeitado ou exemplo inválido.

## Gate WAIT-02 — Threshold de emissão
**Objetivo:** Evitar ruído de `waitMs` em gaps pequenos.  
**Input:** Sessão com gaps 900ms e 2100ms, threshold=1.0s.  
**Expected output:** Apenas 2100ms aparece como `waitMs`.  
**Sinais de falha:** `waitMs` aparece em gap <= threshold ou não aparece em gap > threshold.

## Gate WAIT-03 — Draft materializa espera
**Objetivo:** Garantir que `waitMs` vira step explícito.  
**Input:** `session.json` com `waitMs`.  
**Expected output:** `draft.feature` contém `E eu espero <segundos> segundos` antes do step do evento.  
**Sinais de falha:** Step ausente ou em posição errada.

## Gate WAIT-04 — Step catalog
**Objetivo:** Garantir contrato do step de espera.  
**Input:** `specs/tests/gherkin/step-catalog.yaml` e `.md`  
**Expected output:** Step `eu espero ... segundos` presente e documentado.  
**Sinais de falha:** Step ausente ou não aceita decimal.
