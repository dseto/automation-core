
# Draft Generator — Validation

- draft.feature sempre gerado quando input válida
- escape hatch quando necessário
- rawAction refletido
- metadata gerado

## Gate crítico — Validade da sessão de entrada
- Se session.json não tiver eventos semânticos:
  - warning obrigatório
  - metadata marcada como inválida OU abortar geração
- É falha gerar draft “normal” a partir de sessão vazia.


## Gate crítico — Qualidade do `target.hint` (RF00a)

O Draft Generator não tem como “adivinhar” o `data-testid`. Portanto, quando o HTML possui `data-testid`,
o Recorder deve ter produzido hints baseados em `data-testid`.

- Se existir `data-testid`, é falha registrar apenas `#mat-*`, `div` ou outro hint genérico.


## Gate — Formato Gherkin compatível (RF12a)

O `draft.feature` gerado DEVE:

- Começar com `#language: pt`
- Usar `Funcionalidade:` e `Cenário:` (não `Feature:`/`Scenario:`)
- Usar steps em PT-BR (`Dado/Quando/Então/E`)
- Não conter aspas duplas dentro do parâmetro de `<elementRef>` (ex.: **não** gerar `"[data-testid="x"]"`)

### Caso mínimo (exemplo)
Entrada com:
- navigate → "/login"
- fill → username/password
- click → submit
Deve produzir um `draft.feature` equivalente ao exemplo em `specs/api/examples/draft.feature.example.feature`.
