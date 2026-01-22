# Delta — FREE-HANDS Deterministic Routes (Draft Generator & Semantic Resolution)

- Data: 2026-01-22
- Slug: `2026-01-22-free-hands-deterministic-routes`
- Versão: v0.6.1
- Status: **RELEASED**
- Data de fechamento: 2026-01-22

## Objetivo

Tornar **determinísticas e explícitas** as rotas usadas no pipeline FREE-HANDS:

- O Recorder grava eventos `navigate` com campos normalizados (`url`, `pathname`, `fragment`, `route`).
- O Draft Generator transcreve `event.route` de forma segura ao `draft.feature` (1 linha, sem quebras de Gherkin).
- O Semantic Resolver mapeia rotas para páginas do UiMap com regras **explícitas e determinísticas** e, quando não mapear, emite `UIGAP_ROUTE_NOT_MAPPED` (info).

## Escopo

Inclui:
- Contrato do `session.json` (navigate) com `url/pathname/fragment/route`
- Política normativa de sanitização de rota/hints/RAW para evitar quebra de Gherkin
- Política normativa de mapeamento de rota (fragment → `__meta.route`) sem “guessing”

Fora de escopo:
- Qualquer heurística de correção de rotas
- Alterações na semântica de resolução de elementos (targets)

## RFs cobertos

- Recorder Session: RF02 (atualizado) — Navegação determinística
- Draft Generator: RF12b (novo) — Determinismo de rotas e sanitização 1 linha
- Semantic Resolution: RF39 (novo) — Resolução determinística de rotas de navegação

## Artefatos impactados

- `session.json` (contrato e exemplo)
- `draft.feature` (geração de navegação e higiene 1 linha)
- `resolved.metadata.json` / `ui-gaps.report.json` (finding `UIGAP_ROUTE_NOT_MAPPED` quando não mapear)

## Links SSOT (fonte única)

- `specs/backend/requirements/free-hands-recorder-session.md` (RF02)
- `specs/backend/requirements/free-hands-draft-generator.md` (RF12b)
- `specs/backend/requirements/free-hands-semantic-resolution.md` (RF39)
- `specs/backend/rules/semantic-resolution.md` (política de mapeamento)
- `specs/api/schemas/recorder.session.schema.json`
- `specs/api/examples/recorder.session.*.example.json`
- `specs/tests/validation/free-hands-draft-generator.md`
- `specs/tests/validation/free-hands-semantic-resolution.md`
