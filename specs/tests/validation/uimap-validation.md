# UiMap Validation (espelho do Validator)

> Fonte de verdade: `Automation.Validator/Validators/UiMapValidator.cs`

## Regras e códigos (atuais)

### ERROS
- `UIMAP_EMPTY`  
  UiMap não contém páginas (`pages` vazio).
- `UIMAP_INVALID_ROUTE`  
  `route` existe, mas não começa com `/` e não contém `:`.
- `UIMAP_ELEMENT_NO_TESTID`  
  Elemento existe, mas `test_id` está vazio/nulo.
- `UIMAP_DUPLICATE_TESTID`  
  `test_id` duplicado **dentro da mesma página**.
- `UIMAP_GLOBAL_DUPLICATE_TESTID`  
  `test_id` duplicado **entre páginas diferentes**.
- `UIMAP_DUPLICATE_ROUTE`  
  `route` (normalizado) duplicado entre páginas.

### WARNINGS
- `UIMAP_PAGE_NO_ELEMENTS`  
  Página sem elementos.
- `UIMAP_PAGE_NO_ROUTE`  
  Página sem `route`.
- `UIMAP_PAGE_NO_ANCHOR`  
  Página sem `anchor`.
- `UIMAP_TESTID_PATTERN`  
  `test_id` não segue o padrão esperado (o validator sugere padrão por página).

## Observação sobre schema vs validator
- O JSON Schema (`api/schemas/uimap.schema.json`) formaliza estrutura.
- O Validator adiciona regras semânticas (unicidade, rotas, padrão de testId).
