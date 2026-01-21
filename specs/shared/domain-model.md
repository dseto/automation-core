# Domain Model

> Fonte: `specs/_legacy/02-architecture.md` + código em `Automation.Core`, `Automation.Reqnroll`, `Automation.Validator`.

## Conceitos centrais

### Spec Assets (SSOT operacional)
Artefatos de entrada que definem **o que executar** e **como resolver** referências:
- **Gherkin (.feature)**: descreve cenários e steps em linguagem natural.
- **ui-map.yaml**: mapeia nomes amigáveis (ex: `login.username`) → `test_id` (data-testid).
- **data-map.yaml**: define dados por contexto (`@objeto`), datasets (`{{dataset}}`) e variáveis de ambiente (`${ENV_VAR}`).
- **Run Settings**: configurações de execução (timeout, ambiente, etc.).

### Runtime Resolution (padrão)
A execução de um step **não depende de page objects estáticos**. Em vez disso, o sistema resolve tudo em runtime, a partir de:
- página atual (PageContext)
- ui-map (UiMapModel)
- data-map (DataMapModel)
- estratégia de espera (WaitService)

Resultado: evolução do app sob teste tende a impactar **apenas** os arquivos de spec (maps/dados) — reduzindo acoplamento.

## Blocos arquiteturais (bounded contexts)

### Validation Engine
- **Responsabilidade:** validar integridade de specs/arquivos antes da execução (shift-left).
- **Implementação:** projeto `Automation.Validator` (estado atual: biblioteca/validador; CLI mencionada como roadmap no spec book).
- **Saídas:** lista de erros/warnings acionáveis (ex: página inexistente no ui-map, chave de dados inválida).

### Core Engine
- **Responsabilidade:** resolver referências e prover serviços de execução (sem acoplar a framework BDD).
- **Implementação:** projeto `Automation.Core`.
- **Principais subsistemas:**
  - **UiMap**: `UiMapModel`, `UiPage` (carrega e consulta páginas/modais e `test_id`).
  - **Resolution**: `PageContext`, `ElementResolver`.
  - **DataMap**: `DataMapModel`, `DataResolver`.
  - **Waits/Evidence/Debug/Diagnostics**: serviços de estabilidade e rastreabilidade.

### Runtime Adapter
- **Responsabilidade:** conectar o Core Engine ao runtime BDD e ao WebDriver.
- **Implementação:** projeto `Automation.Reqnroll`.
- **Papéis-chave:**
  - inicialização por cenário (`RuntimeHooks`)
  - orquestração e DI (`AutomationRuntime`)
  - steps de alto nível (Navigation/Interaction/Validation steps)

## Invariantes (contratos arquiteturais)
- Um step só pode interagir com elemento se o **PageContext** estiver definido (ou se o elemento vier no formato `pagina.elemento`).
- Elementos são localizados por **data-testid** (CSS: `[data-testid='...']`).
- Resolução de dados segue precedência:
  1) `@objeto` (contextos do DataMap, com fallback para `default`)
  2) `{{dataset}}` (sequencial por default; random opcional)
  3) `${ENV_VAR}`
  4) literal
- O runtime deve ser **determinístico** sempre que possível (datasets sequenciais por padrão).

## Glossário mínimo (para detalhamento posterior)
- **Friendly name**: referência de elemento usada no Gherkin (ex: `username` ou `login.username`).
- **TestId**: valor real de `data-testid` no DOM.
- **Resolution Result**: tupla `{pageName, friendlyName, testId, cssLocator}`.
