# 01 — Visão e Escopo (Automation Core)

## Objetivo
Framework corporativo para testes automatizados de UI em escala (100+ apps):
- BDD (**Reqnroll**)
- `data-testid` como contrato
- `ui-map.yaml` como dicionário
- Runtime Resolution (sem generation layer por app)
- Pré-flight validation
- Observabilidade (logs + evidências)
- Debug visual local (QA/DEV)

## DoD (MVP)
- `Automation.Validator doctor/validate/plan` funciona em `samples/`
- `Automation.Core` resolve locators via `[data-testid='x']`
- Bindings BDD via **Reqnroll** executam steps mínimos em PT-BR
- evidências em falha
- debug visual local via env vars
