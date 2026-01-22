# changes.md

## Added
- Campo opcional `events[].waitMs` no `session.json` (Recorder).
- Parâmetro `RECORD_WAIT_LOG_THRESHOLD_SECONDS` (default 1.0).
- Step Gherkin `E eu espero <segundos> segundos` (aceita decimal).
- Geração automática do step de espera no `draft.feature` quando `waitMs` existir.

## Changed
- Exemplos normativos atualizados para demonstrar o fluxo completo (schema + session example + draft example + step catalog).

## Removed
- N/A

## Not Changed
- Campos obrigatórios do `session.json`.
- Semântica de eventos existentes (navigate/fill/click).

---

**Status:** Implemented and validated on 2026-01-21

## Compatibility
- Backward compatible: `waitMs` é opcional e pode ser ignorado por consumidores antigos.

## Notes
- O Recorder só emite `waitMs` quando `gapMs > thresholdMs` (evita ruído).
