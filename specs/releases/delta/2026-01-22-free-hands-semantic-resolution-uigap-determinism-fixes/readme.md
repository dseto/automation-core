# Delta — 2026-01-22-free-hands-semantic-resolution-uigap-determinism-fixes

## Objetivo
Entregar o **Delta 3** de *Semantic Resolution* com foco nos pain points críticos:
- gerar **UI Gaps Report** com **IDs determinísticos**
- evitar “adivinhação” em resolução (ambiguidade vira `partial` com candidates)
- garantir compatibilidade de RunSettings (`UI_MAP_PATH` canônico + `UIMAP_PATH` alias)
- permitir validação sem depender de engine JSON Schema (checks explícitos)

## Escopo (Normativo)
Inclui:
- novos requisitos e regras de *Semantic Resolution*
- schemas e exemplos canônicos para:
  - `resolved.metadata.json`
  - `ui-gaps.report.json`
- validação (gates) para determinismo, consistência e no-guessing
- update incremental em `backend/implementation/run-settings.md`

Fora de escopo:
- heurísticas de ML/IA para resolução
- “auto-fix” de UIMap
