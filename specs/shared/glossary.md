# Glossary

> Base: `_legacy/02-architecture.md`, `_legacy/04-runtime-resolution.md`, código-fonte.

## Termos
- **Spec Assets**: conjunto de arquivos que governam execução (feature, ui-map, data-map, settings).
- **Friendly name**: nome lógico referenciado no Gherkin (ex: `username` ou `login.username`).
- **TestId**: valor real do atributo `data-testid` no DOM.
- **PageContext**: fonte de verdade da página atual para resolução de elementos.
- **Resolution Result**: resultado de resolução `{pageName, friendlyName, testId, cssLocator}`.
- **DataMap**: mapa de dados com contextos (`@key`) e datasets (`{{dataset}}`).
- **Escape Hatch**: bypass controlado de resolução/validação (quando inevitável).

## TODO
Completar extraindo termos e aliases do `_legacy/*`.
