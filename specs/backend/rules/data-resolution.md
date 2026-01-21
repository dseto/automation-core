# Data Resolution (DataMap)

## Inputs
- `dataKey` pode ser:
  - `@contextKey`
  - `{{datasetKey}}`
  - `${ENV_VAR}`
  - literal

## Precedência e fallback
1. `@contextKey`: buscar em `contexts[env]` com fallback para `contexts[default]`.
2. `{{datasetKey}}`: buscar em `datasets[datasetKey]`
   - `strategy: sequential` (default) ou `random` (opt-in)
3. `${ENV_VAR}`: variável de ambiente
4. literal

## Determinismo
- sequential mantém índice por dataset na execução.
- random deve permitir seed/config (quando habilitado).

## Implementação (referência)
- `Automation.Core/DataMap/DataResolver.cs`
