# Architecture Principles (Spec-Driven)

## Princípios

1. **Specs são SSOT**
   - comportamento, mapeamento e dados vivem em arquivos de spec e são versionados junto do projeto.

2. **Runtime Resolution por padrão**
   - reduzir page objects e acoplamento a implementações específicas do app sob teste.
   - resolução deve ser rastreável (input → output).

3. **Shift-left validation**
   - validar mapas/dados/features antes de executar (falhar cedo com mensagens acionáveis).

4. **Determinismo primeiro**
   - escolhas aleatórias devem ser opt-in e controladas.
   - defaults devem ser reprodutíveis.

5. **Erros com contexto**
   - falhas de resolução/validação devem indicar: arquivo, página, campo, chave, sugestão de correção.

6. **Separação de responsabilidades**
   - `Core` não depende do runtime BDD.
   - `Runtime Adapter` não reimplementa resolução; apenas orquestra.
   - `Validator` nunca executa browser; apenas valida contratos.

## Anti-padrões a evitar
- Step definitions “sabendo demais” sobre a estrutura do DOM além do `data-testid`.
- Duplicação de mapeamentos (o ui-map deve ser a fonte única).
- Dados hardcoded nos steps quando poderiam estar no data-map.
