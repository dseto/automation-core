# Step Catalog Validation

## Objetivo
Validar que:
- `tests/gherkin/step-catalog.yaml` está consistente
- patterns são únicos (sem colisão perigosa)
- handler aponta para implementação existente (quando rodado em repo completo)
- cada step usado em `.feature` existe no catálogo

## Regras mínimas
- `version` obrigatório
- `steps[*].bdd` ∈ {Given, When, Then}
- `steps[*].patterns` deve ter ao menos 1 pattern
