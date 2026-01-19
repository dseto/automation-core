# Spec Deck - Automation Platform v2.0

## Visão Geral
Especificação técnica completa da plataforma de automação de testes de UI baseada em **Spec-Driven Development (SDD)**. Todos os artefatos são versionados e tratados como código.

## Documentos

### Fundação
1. **01-vision-and-scope.md** - Visão, princípios e escopo da plataforma.
2. **02-architecture.md** - Arquitetura técnica e componentes principais.

### Contratos (SSOT)
3. **03-contracts.md** - UiMap, DataMap e Gherkin como contratos formais.
4. **04-runtime-resolution.md** - Fluxo de resolução em runtime (Element, Data, Wait).

### Implementação
5. **05-step-catalog.md** - Catálogo completo de steps genéricos.
6. **06-escape-hatch.md** - Execução de JavaScript para casos complexos.

### Qualidade
7. **07-validation-and-testing.md** - Validação de contratos e estratégia de testes.
8. **08-security-and-compliance.md** - Segurança, secrets e conformidade.

### Operação
9. **09-implementation-guide.md** - Passo a passo para implementar em nova aplicação.
10. **10-troubleshooting.md** - Guia de diagnóstico e resolução de problemas.

### Futuro
11. **backlog.md** - Features futuras e roadmap.

## Convenções
- **YAML:** Contratos (UiMap, DataMap) em YAML com validação de schema.
- **Gherkin:** Testes em PT-BR com tags para organização.
- **C#:** Apenas no Core e Reqnroll, nunca em projetos de teste.
- **LLM-Ready:** Documentos estruturados para consumo por modelos de linguagem.
