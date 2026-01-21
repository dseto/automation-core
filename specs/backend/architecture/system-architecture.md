# System Architecture

> Este documento detalha a topologia e os fluxos conforme o código atual.

## Topologia da solution

- `Automation.Core` — núcleo (resolução, leitura de mapas, waits, evidências, debug).
- `Automation.Reqnroll` — adaptador de runtime (hooks, DI, steps BDD).
- `Automation.Validator` — validação (governança de specs; CLI como evolução).

## Component diagram (alto nível)

```
            +------------------------------+
            |          Spec Assets         |
            |  .feature / ui-map / data    |
            +--------------+---------------+
                           |
                           v
                +----------+-----------+
                |  Automation.Validator |
                |  (validate contracts) |
                +----------+-----------+
                           |
                           v
+--------------------------+---------------------------+
|                    Automation.Core                  |
|  UiMapModel / PageContext / ElementResolver         |
|  DataMapModel / DataResolver / WaitService          |
|  DebugController / EvidenceManager / Diagnostics    |
+--------------------------+---------------------------+
                           |
                           v
                +----------+-----------+
                | Automation.Reqnroll  |
                | Hooks + Runtime +    |
                | Steps (BDD binding)  |
                +----------+-----------+
                           |
                           v
                     WebDriver (Selenium)
```

## Fluxo de execução (real)

1. **BeforeScenario** (`Automation.Reqnroll/Hooks/RuntimeHooks.cs`)
   - carrega `RunSettings.FromEnvironment()`
   - resolve path do `ui-map.yaml` (env `UI_MAP_PATH` ou busca em locais comuns)
   - cria WebDriver (env `BROWSER=chrome|edge`, default: chrome)
   - constrói `AutomationRuntime` e registra no container (DI do Reqnroll)

2. **AutomationRuntime** (`Automation.Reqnroll/Runtime/AutomationRuntime.cs`)
   - mantém `RunId`
   - carrega `data/data-map.yaml` a partir do BaseDirectory
   - cria serviços: `DataResolver`, `WaitService`, `EvidenceManager`, `DebugController`, `PageContext`, `ElementResolver`

3. **Step binding** (`Automation.Reqnroll/Steps/*.cs`)
   - cada step chama `WaitDomReady` + `TryWaitAngularStable`
   - resolve elemento via `ElementResolver.Resolve(elementRef)`:
     - `username` → usa página atual do `PageContext`
     - `login.username` → página explícita
     - `anchor` → usa anchor definido em `__meta.anchor`
   - converte `testId` em CSS: `[data-testid='...']`
   - resolve dados via `DataResolver.Resolve(dataKey)`
   - interage/valida no DOM
   - opcionalmente aplica debug (highlight, slow-mo, pause)

4. **AfterScenario**
   - dispose do runtime (fecha driver)

## Subsystem: Runtime Resolution

### PageContext
- mantém `CurrentPageName`
- mantém histórico de navegação (`_pageHistory`)
- é a fonte de verdade da “página atual” para resolução de elementos

### UiMapModel / UiPage
- suporta `Pages` e `Modals`
- cada page possui `__meta.route` e/ou `__meta.anchor`
- elementos podem ser:
  - formato completo `{ test_id: "..." }`
  - formato simplificado `"..."`

### ElementResolver
- input: `elementRef`
- output: `ResolutionResult(pageName, friendlyName, testId, cssLocator)`
- regra: lança erro claro quando:
  - página não definida e ref é simples
  - página/modoal não existe
  - elemento não existe
  - anchor não existe

### DataResolver
- `@key` resolve em `contexts[env]` com fallback `contexts[default]`
- `{{dataset}}` resolve em `datasets[key]`:
  - `strategy=random` ou `sequential` (default)
  - mantém índice por dataset (determinismo)
- `${ENV_VAR}` resolve por Environment Variable
- literal caso contrário

## Estratégia de isolamento
- Projetos de teste dependem apenas do pacote `Automation.Reqnroll` (NuGet).
- Core e Validator evoluem sem impacto direto nos projetos consumidores (desde que contratos permaneçam compatíveis).
