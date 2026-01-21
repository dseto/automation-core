---
agent: spec-driven-implementer
---
Você é o spec-driven-implementer.

Objetivo: executar o fechamento da release de forma spec-driven.

Release alvo:
specs/releases/delta/2026-01-20-stabilization-framework/

Tarefas obrigatórias (siga nesta ordem):

────────────────────────────────────
1) Validação final (gates)
────────────────────────────────────
- Rode:
  dotnet restore
  dotnet build
  dotnet test
- Registre os resultados.
- Se algum falhar: pare, corrija e não prossiga.

────────────────────────────────────
2) Auditoria anti-drift
────────────────────────────────────
Verifique explicitamente:
- O código implementa tudo do README.md do delta pack.
- Tudo em changes.md está refletido no código.
- migration.md (se existir) é coerente com o estado atual.
- run-settings.md reflete o código.
- schemas e exemplos continuam válidos.
- validation docs batem com o Automation.Validator.

Liste qualquer divergência encontrada.
Se houver, proponha correção antes de continuar.

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
