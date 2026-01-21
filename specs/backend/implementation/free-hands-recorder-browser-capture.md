
# Implementação — Browser Capture Layer (MVP)

Implementa RC01–RC06.

## Estratégia recomendada (robusta e simples)
1. Injetar script no início do modo exploratório (após abrir página)
2. Script acumula eventos em `window.__fhRecorder.buffer`
3. Runtime faz polling (ex.: a cada 250–500ms) e transforma em `RecorderEvent` do session.json

## Por que polling (e não console.log)
- Evita dependência de logs do driver
- Funciona em mais stacks
- Mantém controle determinístico

## Mapeamento JS → session.json
- kind navigate → type "navigate" + route
- kind click → type "click" + target (tag/text/attributes/css)
- kind fill → type "fill" + target + value.literal (ou masked)
- kind submit → type "submit"

## Segurança / PII
- Runtime pode mascarar inputs por heurística (name/id contém "pass", type=password etc.)
