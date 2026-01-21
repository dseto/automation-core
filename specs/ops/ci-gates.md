# CI Gates (Recomendação)

## Objetivo
Evitar drift e garantir que specs continuam executáveis.

## Pipeline sugerido
1. Validate YAML syntax (ui-map, data-map, step-catalog).
2. JSON Schema validation:
   - ui-map.yaml → `api/schemas/uimap.schema.json`
   - data-map.yaml → `api/schemas/datamap.schema.json`
3. Step catalog checks:
   - patterns únicos
   - handlers consistentes (quando aplicável)
4. Gherkin validation:
   - steps usados existem no catálogo
5. Unit tests (Core/Validator)
6. Smoke E2E (opt-in)

## Artefatos
- publicar relatório de validação + logs
- anexar evidências (quando habilitadas)
