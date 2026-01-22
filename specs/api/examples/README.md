# Examples (Canônicos)

Estes arquivos são exemplos **válidos** e **inválidos** para exercitar o Validator/CI.

## UiMap
- `uimap.example.yaml` (válido)
- `uimap.invalid-missing-testid.yaml` (inválido: elemento sem `test_id`)
- `uimap.invalid-bad-anchor.yaml` (inválido: `__meta.anchor` aponta para elemento inexistente)

## DataMap
- `datamap.example.yaml` (válido)
- `datamap.invalid-missing-default.yaml` (inválido: `contexts.default` ausente)
- `datamap.invalid-bad-strategy.yaml` (inválido: `strategy` fora do enum)


## Semantic Resolution (Delta 3)
- `resolved.feature.example.feature`
- `resolved.metadata.example.json`
- `ui-gaps.report.example.json`
- `ui-gaps.report.example.md`
