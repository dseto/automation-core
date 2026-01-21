# Naming Conventions

## Objetivo
Garantir consistência entre Gherkin, ui-map, data-map, logs e relatórios.

## UI (Friendly names)
- Preferir `page.element` quando houver ambiguidade.
- `page` deve bater com a chave em `frontend/uimap.yaml` (pages/modals).
- `element` deve bater com a chave do elemento dentro da página.

## data-testid (TestId)
- Deve ser **estável** e **semântico** (não baseado em posição/estilo).
- Evitar IDs gerados automaticamente.
- Padrão sugerido: `page.component.element` (ex: `login.form.username`).

## Data keys
- Context keys: `@<nome>` (ex: `@usuario`)
- Dataset keys: `{{<nome>}}` (ex: `{{usuarioValido}}`)
- Env keys: `${VAR}` (ex: `${BASE_URL}`)

## Arquivos
- `frontend/uimap.yaml`
- `tests/testdata/*`
- `tests/gherkin/*.feature`
