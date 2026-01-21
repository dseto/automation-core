# DataMap Validation (espelho do Validator)

> Fonte de verdade: `Automation.Validator/Validators/DataMapValidator.cs`

## Regras e códigos (atuais)

### ERROS
- `DATAMAP_NO_CONTEXTS`  
  DataMap não contém nenhum contexto.
- `DATAMAP_NO_DEFAULT_CONTEXT`  
  DataMap deve conter contexto `default`.
- `DATAMAP_INVALID_CONTEXT`  
  Contexto não é um dicionário válido.
- `DATAMAP_INVALID_STRATEGY`  
  `strategy` não é válida. Válidas: `sequential`, `random`, `unique`.

- `DATAMAP_EMPTY_DATASET`  
  Dataset sem itens válidos.
- `DATAMAP_INVALID_CONTEXT` / `DATAMAP_EMPTY_CONTEXT`  
  Contextos vazios/invalidáveis (ver mensagens do validator).

### WARNINGS
- `DATAMAP_EMPTY_CONTEXT`  
  Contexto existe mas está vazio.
- `DATAMAP_KEY_CONFLICT`  
  Nome de dataset conflita com chave em contexto.
- `DATAMAP_DUPLICATE_ITEMS`  
  Quando `strategy == unique`, itens duplicados geram warning (o runtime atual trata como sequential; ver nota).

## Nota importante (realidade do runtime)
- O runtime (`Automation.Core/DataMap/DataResolver.cs`) implementa:
  - `random` (random item)
  - qualquer outra estratégia → comportamento **sequential**
- Portanto, hoje `unique` é **aceita/validada**, mas o comportamento em runtime é equivalente a sequential.
