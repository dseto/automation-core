# Element Resolution (UiMap + PageContext)

## Inputs
- `elementRef` pode ser:
  - `element` (usa PageContext)
  - `page.element` (página explícita)
  - referência a `modal.element` (se modelado como modal)
- `PageContext.CurrentPageName` deve estar definido para refs simples.

## Lookup
- procurar em `ui-map.yaml`:
  - `pages[pageName].elements[elementName]`
  - fallback: `modals[pageName].elements[elementName]`
- suportar `__meta`:
  - `route` (para navegação/verificação)
  - `anchor` (ponto de sincronização)

## Output
- `ResolutionResult(pageName, friendlyName, testId, cssLocator)`

## Erros (acionáveis)
- `E_PAGE_CONTEXT_REQUIRED`: ref simples sem PageContext.
- `E_PAGE_NOT_FOUND`: página/modal inexistente.
- `E_ELEMENT_NOT_FOUND`: elemento inexistente.
- `E_TESTID_MISSING`: elemento existe mas sem test_id.

## Implementação (referência)
- `Automation.Core` (UiMapModel, PageContext, ElementResolver).
