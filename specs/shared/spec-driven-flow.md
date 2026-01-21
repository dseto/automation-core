# Fluxo Operacional — Spec-Driven Improvement

## Ciclo oficial

1. Criar delta pack a partir do template
2. Atualizar specs (SSOT) primeiro
3. Validar specs (schemas + exemplos + regras)
4. Implementar no código
5. Atualizar/alinhar Validator
6. Rodar testes (unit → contract → smoke)
7. Fechar delta pack
8. Merge + version bump

## Regra de ouro
Nenhuma melhoria é considerada pronta se:
- o spec não mudou
- ou o validator/docs não refletem o código
