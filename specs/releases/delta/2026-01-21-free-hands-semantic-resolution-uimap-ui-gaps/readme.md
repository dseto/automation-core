# Delta 3 — Semantic Resolution (UI Mapping & UI Gaps)

## Objetivo
Introduzir a Fase 3 do pipeline FREE-HANDS: **resolver semanticamente** refs do `draft.feature` contra o **UIMap** e produzir relatórios de **gaps de UI** (mapeamento ausente, rota ambígua, ausência de evidência de `data-testid`, etc).

## Escopo
Inclui:
- Regras normativas de resolução (UIMap key + fallback por `target.testId`)
- Geração de artefatos:
  - `resolved.feature`
  - `resolved.metadata.json`
  - `ui-gaps.report.json`
  - `ui-gaps.report.md`
- Schemas e exemplos canônicos
- Gates de validação (Delta 3)

Não inclui:
- Alterações automáticas no `ui-map.yaml`
- Resolução de DataMap
- Replay/execução determinística (Delta 5)

## Requisitos (RFs)
Fonte normativa (SSOT):
- `specs/backend/requirements/free-hands-semantic-resolution.md` (RF30–RF38)

Regras consolidadas:
- `specs/backend/rules/semantic-resolution.md`

## Status
Draft

## Versão
v1

## O que NÃO fazer
- NÃO inventar páginas/elementos.
- NÃO substituir refs em steps `partial`/`unresolved`.
- NÃO gerar gaps apenas em texto: o JSON é o artefato primário (`ui-gaps.report.json`).
