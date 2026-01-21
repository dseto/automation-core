# UI Contract (UiMap)

## SSOT
- `frontend/uimap.yaml` é a fonte única para mapear friendly names → testId.

## Requisitos mínimos
- `pages:` e/ou `modals:`
- `__meta:` opcional por page/modal
- `elements:` com `test_id` obrigatório

## Exemplo (ilustrativo)
```yaml
pages:
  login:
    __meta:
      route: /login
      anchor: form
    elements:
      username: { test_id: "login.form.username" }
      password: { test_id: "login.form.password" }
      submit:   { test_id: "login.form.submit" }
```

## Validações
- `tests/validation/uimap-validation.md`
