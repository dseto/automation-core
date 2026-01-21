
# FREE-HANDS Draft Generator — Implementation Spec

Pipeline:
session.json
  → SanityChecker
  → ActionGrouper
  → StepInferenceEngine
  → EscapeHatchRenderer
  → DraftWriter

Regras:
- Draft nunca é final.
- Nunca inventar steps.
- Sempre preservar evidência.

Dependência explícita:
- O Draft Generator assume o **contrato RF00a** (qualidade do `target.hint`).
  - Se o hint vier como `[data-testid='...']`, o engine deve preservar isso como `elementRef`.
  - Se o hint não for um seletor estável (ex.: `div` isolado), o Draft Generator deve manter o output em modo
    conservador, gerando **TODO + RAW** (RF13/RF14) ao invés de "inventar" um elementRef.


## DraftWriter — Regras de serialização (Gherkin PT-BR)

O DraftWriter é responsável por garantir compatibilidade com Automation.Reqnroll:

- Primeira linha sempre `#language: pt`
- Cabeçalho: `Funcionalidade: <título> (draft)`
- Cenários: `Cenário: <título>` (título determinístico)
- Steps:
  - `Dado que estou na página "<path>"` para `navigate`
  - `Quando eu clico em "<elementRef>"`
  - `Quando eu preencho "<elementRef>" com "<value>"`
  - Use `E` para steps subsequentes do mesmo tipo (ação), preservando ordem original.
- Escape hatch (RF13/RF14):
  - Inserir bloco comentado `# TODO:` + `# RAW:` com o script.
- Anti-quebra de aspas:
  - Antes de escrever `<elementRef>`, substituir `"` por `'` em seletores CSS (ex.: data-testid).
  - Garantir que o texto final não contenha `"` dentro de `<elementRef>`.

Checklist: o arquivo gerado deve ser parseável por Reqnroll sem erros de sintaxe.
