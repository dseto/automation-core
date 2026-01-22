# Delta 3 — Validation

## Gate 1 — Formato do `resolved.feature`
- Objetivo: Gherkin válido, preservando o draft.
- Input: `resolved.feature`
- Expected output:
  - `#language: pt`
  - ordem de steps preservada
  - comentários de gaps: `# UIGAP: <id> <code>`
- Falha se:
  - steps `partial/unresolved` forem alterados

## Gate 2 — Contrato do `resolved.metadata.json`
- Objetivo: Schema + consistência.
- Input: `resolved.metadata.json`
- Expected output:
  - valida contra `specs/api/schemas/resolved.metadata.schema.json`
  - `resolvedCount + partialCount + unresolvedCount == stepsCount`
- Falha se:
  - houver `findingId` que não existe no `ui-gaps.report.json`

## Gate 3 — Contrato do `ui-gaps.report.json`
- Objetivo: Schema + stats coerentes.
- Input: `ui-gaps.report.json`
- Expected output:
  - valida contra `specs/api/schemas/ui-gaps.report.schema.json`
  - `stats.total == findings.length`
- Falha se:
  - IDs não forem determinísticos (`UIGAP-0001...`)

## Gate 4 — No Silent Failure
- Objetivo: cada target não resolvido vira finding explícito.
- Input: `resolved.metadata.json` + `ui-gaps.report.json`
- Expected output:
  - todo `unresolved` → >= 1 finding `error`
  - todo `partial` → >= 1 finding `warning`
- Falha se:
  - existir step `unresolved` sem finding

## Gate 5 — Spec Sync
- Objetivo: evitar drift entre RF ↔ schema ↔ exemplos ↔ manifest.
- Input:
  - `impact-manifest.md`
  - schemas e exemplos citados
- Expected output:
  - todos os paths do manifest existem
  - schemas/exemplos refletem RF30–RF38
- Falha se:
  - houver arquivo alterado fora do delta e ausente no manifest
