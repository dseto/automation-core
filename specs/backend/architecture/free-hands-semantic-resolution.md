# Free Hands — Semantic Resolution (Arquitetura)

## Objetivo
Camada responsável por transformar um `draft.feature` em um `resolved.feature` com rastreabilidade e sem “adivinhação”.

## Entradas e saídas
### Entradas
- `draft.feature`
- `ui-map.yaml`
- opcional: `recorder.session.json`

### Saídas
- `resolved.feature`
- `resolved.metadata.json`
- `ui-gaps.report.json`
- `ui-gaps.report.md`

## Componentes lógicos
- **Draft Parser:** extrai steps e `draftLine`.
- **UIMap Index:** indexa elementos por:
  - `pageKey.elementKey`
  - `testId` (quando presente)
- **Resolver:** aplica regras (ver `backend/rules/semantic-resolution.md`)
- **Reporter:** gera report JSON/MD e injeta comentários `# UIGAP:` no `resolved.feature`.

## Notas de compatibilidade
- `UI_MAP_PATH` é canônico; `UIMAP_PATH` é alias.
- `SEMRES_MAX_CANDIDATES` evita explosão de output.
