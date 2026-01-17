# 04 — Contratos (MVP)

## data-testid
- obrigatório em elementos interativos e âncoras
- estável, sem IDs dinâmicos

## ui-map.yaml
- PageName -> FriendlyName -> testid
- `__meta.route` e `__meta.anchor` permitidos

## Steps suportados no MVP (validator + reqnroll)
- Dado que estou na tela "{PageName}"
- Quando eu preencho o campo "{Element}" com "{Value}"
- Quando eu clico no botão "{Element}"
- Então a rota deve ser "{Route}"
- Então o elemento "{Element}" deve estar visível
