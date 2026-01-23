# JSON Schemas (SSOT)

Estes schemas formalizam os contratos de arquivos do produto.

- `uimap.schema.json` — contrato do `frontend/uimap.yaml`
- `datamap.schema.json` — contrato do `data/data-map.yaml`

## Como usar (exemplos)
- Em CI, validar YAML convertido para JSON contra o schema correspondente.
- Em tooling local, usar um validador JSON Schema compatível com Draft 2020-12.

## Observações
- O código atual aceita formato de elemento simplificado (string = test_id) e formato completo `{ test_id: ... }`.
- O schema permite campos extras dentro de páginas/elementos para extensões futuras.

## Exemplos
Ver `api/examples/` para YAMLs válidos e inválidos.

## Estratégias de dataset
O schema de DataMap aceita `sequential`, `random` e `unique` (espelho do Validator).


## Semantic Resolution (Delta 3)
- `resolved.metadata.schema.json`
- `ui-gaps.report.schema.json`

> Nota: Schemas ajustados e validados como parte do delta `2026-01-22-free-hands-semres-fill-safe-rewrite` (RELEASED v0.6.2 em 2026-01-23).
