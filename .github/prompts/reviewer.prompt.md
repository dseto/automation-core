---
agent: spec-driven-implementer
---
Você é o spec-driven-implementer.

Objetivo: Revisar a implementação das correções/melhorias da release de forma spec-driven.

Tarefas obrigatórias (siga nesta ordem):

────────────────────────────────────
1) Validação final (gates)
────────────────────────────────────
- Rode:
  dotnet restore
  dotnet build
  dotnet test

  - rode os smoke tests com este script e não pode falhar nenhum teste
  ui-tests\scripts\run-smoke.ps1

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
