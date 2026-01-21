# Delta 2 — Draft Generator
## Changes (Requirements Patch)

Este patch normaliza e fortalece o contrato do `free-hands-draft-generator`, detalhando requisitos e regras de implementação para reduzir divergências e garantir comportamentos determinísticos.

---

## Implementado
- RF11 — Agrupamento semântico
  - Janela temporal para eventos consecutivos
  - Regras de merge: focus+fill, click+submit, genérico→específico
  - Critério determinístico para seleção de `PrimaryEvent`
  - Exemplos e anti-exemplos codificados em testes

- RF13 — Escape hatch automático
  - `# TODO` + `# RAW:` com JSON compacto em única linha
  - Preservação de `rawAction.script` com truncamento determinístico (500 chars)
  - Warnings em metadata (`RAW_SCRIPT_TRUNCATED`) quando truncado

- RF16 — Não comprometer semântica
  - Implementação evita dependência de UIMap/DataMap ao inferir steps
  - Ambiguidades resultam em escape hatch (RF13)

## Clarificações aplicadas
- RF10, RF14, RF15 receberam redação normativa e exemplos de validação.

## Compatibilidade
- Mudanças são retro-compatíveis com implementações que já obedecem ao contrato.
- Implementações fora do contrato agora falharão nas auditorias de spec (intencional).

---

> Nota: todos os pontos acima foram implementados no código e cobertos por testes unitários e integração leve (geração de `draft.feature` example).