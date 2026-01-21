# Testing (SSOT)

## Estrutura
- `gherkin/` — contrato de steps e diretrizes.
- `testdata/` — dados determinísticos.
- `mapping/` — mapeamentos auxiliares (quando aplicável).
- `validation/` — políticas e regras de validação.

## Como evoluir
- Novos steps devem:
  - entrar no catálogo
  - ter documentação + exemplos
  - ter testes automatizados (unit/integration) no código
