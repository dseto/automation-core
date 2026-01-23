# Delta: Semantic Resolution – Safe Fill Value Preservation

- **Status:** RELEASED
- **Version:** v0.6.2
- **Data de fechamento:** 2026-01-23

## Objetivo
Corrigir comportamento de reescrita semântica que poderia alterar valores literais de preenchimento (`fill`). Garantir que valores literais sejam preservados verbatim nos passos gerados e que a resolução semântica não introduza regressões em drafts/resolution.

## Escopo
- Ajustes no `SemanticResolver` para substituir apenas a primeira ocorrência entre aspas quando aplicável.
- Normalização de rota inicial (quando `url == BASE_URL` -> `route: "/"`).
- Ajustes no `DraftGenerator`/`ActionGrouper` para preservar navegations explícitas e qualificar targets.
- Correções em bindings Reqnroll e scripts de execução para estabilidade (incl. tratamento de rotas wildcard, suporte a `--scenario`).

## Checklist de validação
- [x] Implementação de código concluída
- [x] Tests unitários adicionados e atualizados
- [x] Testes de integração/local executados (run-debug)
- [x] Specs (`changes.md`, `migration.md`, `validation.md`) atualizados
- [x] Documentação do delta pack normalizada
- [x] Pacote fechado e marcado como RELEASED

## Artefatos e evidências
- Diretório de saída: `SEMRES_OUTPUT_DIR/semantic-resolution-*`
- Artifacts gerados: `artifacts/semantic-resolution-*` (draft/resolved)
- Testes relevantes: `SemanticResolutionTests`, `RouteNormalizerTests`, `ActionGrouperTests`, `SemanticE2ETests`
