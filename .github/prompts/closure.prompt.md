---
agent: spec-driven-implementer
---
Você é o spec-driven-implementer.

Objetivo: executar o fechamento da release de forma spec-driven.

Tarefas obrigatórias (siga nesta ordem):

────────────────────────────────────
3) Fechamento do delta pack
────────────────────────────────────
Atualize no README.md do delta pack:
- Status: RELEASED
- Versão: <sugerir versão seguindo semver>
- Data de fechamento: <data de hoje>

Revise e normalize:
- README.md (objetivo, escopo, checklist 100% marcado)
- changes.md (sem TODO, linguagem factual final)
- migration.md (se existir)

────────────────────────────────────
4) Atualização dos índices
────────────────────────────────────
- Atualize specs/releases/README.md:
  - incluir esta release na lista “Releases registradas”.
- Se aplicável, atualizar:
  - specs/api/schemas/README.md
  - specs/api/examples/README.md

────────────────────────────────────
5) Versionamento
────────────────────────────────────
- Sugira a versão final (ex: v0.8.0) com justificativa.
- Indique exatamente:
  - onde atualizar a versão no código
  - qual tag git deve ser criada.

NÃO crie a tag automaticamente.
Apenas prepare tudo para o versionamento.

────────────────────────────────────
6) Entregáveis finais
────────────────────────────────────
Ao final, produza:

A) Resumo de fechamento da release:
   - o que foi entregue
   - impacto
   - breaking changes (se houver)
   - como validar

B) Checklist final de fechamento (todos marcados).

C) Lista completa de arquivos alterados.

D) Comandos executados e seus resultados.

Regra de ouro:
- Não assumir nada.
- Não pular etapas.
- Não prosseguir se algo estiver inconsistente.
