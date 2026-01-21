# Vision & Scope

> Base: `_legacy/01-vision-and-scope.md`

# 01 - Visão e Escopo

## Visão
Plataforma de automação de testes de UI baseada em **Spec-Driven Development**, onde especificações (Gherkin) e contratos (YAML) são os únicos artefatos necessários. Elimina a necessidade de código C# nos projetos de teste.

## Princípios Fundamentais
1. **Zero Código:** Nenhuma linha de C# em projetos de teste para cenários padrão.
2. **Contratos Declarativos:** Inteligência reside em YAML (UiMap, DataMap).
3. **Runtime Resolution:** Interpretação de contratos em tempo de execução.
4. **Agnóstico:** Funciona para qualquer aplicação Web (Angular, React, etc.).

## Escopo
- **In-Scope:** Automação de UI Web, Massa de Dados Estática, Validação de Contratos, CI/CD Integration.
- **Out-of-Scope:** Performance Testing, API Testing, Massa Dinâmica Complexa (v3.0).

## Objetivos
- Setup de automação: 3 dias → 30 minutos.
- 100% dos QAs (sem conhecimento C#) podem criar automações.
- 95% reutilização de steps entre squads.
- Suportar 100+ aplicações do portfólio.

## Definição de Pronto (DoD)
- ✅ Automation.Validator valida UiMap e DataMap.
- ✅ Automation.Core resolve elementos e dados em runtime.
- ✅ Automation.Reqnroll executa steps genéricos em PT-BR.
- ✅ Evidências automáticas em falha.
- ✅ Debug visual local via env vars.
- ✅ Catálogo de 40+ steps cobrir 95% dos cenários.

## Tradução para contratos (normativo)
Esta seção conecta visão a decisões que viram **contratos** no deck:

- UI automation baseada em **data-testid** → contrato em `frontend/uimap.yaml` e regras em `backend/rules/element-resolution.md`.
- Execução BDD via Reqnroll → acoplamento permitido apenas no `Automation.Reqnroll` (ver `backend/architecture/system-architecture.md`).
- Shift-left validation → `Automation.Validator` + política em `tests/validation/validation-policy.md`.
