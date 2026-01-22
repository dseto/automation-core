# migration.md

## Quem migra
- Implementadores do Recorder (session log)
- Implementadores do Draft Generator
- Implementadores do runner/steps (Reqnroll)

## Passos
1. Atualizar `recorder.session.schema.json` para aceitar `events[].waitMs` (opcional, integer >= 0).
2. No Recorder:
   - Calcular `gapMs` entre eventos consecutivos.
   - Se `gapMs > RECORD_WAIT_LOG_THRESHOLD_SECONDS*1000`, emitir `waitMs = gapMs` no evento atual.
3. No Draft Generator:
   - Se `event.waitMs` existir, inserir `E eu espero <segundos> segundos` imediatamente antes do step do evento.
4. No catálogo de steps:
   - Garantir que o step `eu espero ([0-9]+(?:\.[0-9]+)?) segundos?` exista e esteja documentado.

## Verificação pós-migração
- `recorder.session.login.example.json` valida no schema.
- `draft.feature.example.feature` contém step de espera antes do evento correspondente.
- Step catalog contém o novo step e aceita decimal.

**Status:** Migration steps executed and validated on 2026-01-21

## O que NÃO fazer
- NÃO inventar espera no Draft Generator.
- NÃO tornar `waitMs` obrigatório no schema.
