# 02 - Arquitetura Técnica

## Modelo Arquitetural
A plataforma segue o padrão **Runtime Resolution**, onde os contratos (YAML) são interpretados em tempo de execução. Não há geração de código por aplicação, diferenciando-se de frameworks tradicionais como SpecFlow.

## Componentes Principais

### Automation.Core
O motor central responsável pela execução. Contém:

**PageContext:** Mantém o estado da página atual (rota, nome). Atualizado automaticamente quando a URL muda. Usado pelo ElementResolver para localizar elementos corretos.

**ElementResolver:** Resolve `testId` a partir do `ui-map.yaml` usando PageContext. Suporta resolução por nome simples (ex: "username") ou qualificado (ex: "login.username"). Retorna CSS locator para o WebDriver.

**DataResolver:** Interpreta valores do `data-map.yaml`. Suporta três tipos: literais (strings diretas), objetos (dicionários de contexto), e tokens (datasets com estratégias de seleção).

**WaitService:** Gerencia waits de navegação, visibilidade de elementos e estabilidade Angular. Implementa retry logic com timeout configurável (padrão 20s).

**WebDriver Factory:** Cria instâncias de ChromeDriver ou EdgeDriver conforme `RunSettings.Browser`. Configura headless, slowmo e outras opções via variáveis de ambiente.

### Automation.Reqnroll
Camada de bindings que traduz Gherkin em comandos do Core.

**NavigationSteps:** Steps para navegação ("estou na página X", "aguardo a rota Y").

**InteractionSteps:** Steps para interação ("preencho campo", "clico botão", "executo script JS").

**ValidationSteps:** Steps para validação ("elemento visível", "texto contém", "atributo é").

**RuntimeHooks:** Inicializa WebDriver e contexto antes de cada cenário. Captura evidências em falha.

### Automation.Validator (Roadmap)
CLI que valida integridade de contratos antes da execução (Shift-Left).

## Fluxo de Execução
O fluxo começa quando o Reqnroll lê um step Gherkin. O ElementResolver busca o testId no ui-map.yaml usando o contexto da página atual. O DataResolver interpreta o valor (literal, contexto ou dataset). O WebDriver executa a ação. O WaitService aguarda estabilidade (URL, elemento, Angular). Se houver falha, o RuntimeHooks captura evidências.

## Estratégia de Isolamento
Projetos de teste dependem apenas do pacote NuGet `Automation.Reqnroll`, que encapsula o Core. Atualizações no motor não quebram testes existentes. Isso permite evolução da plataforma sem impacto nos 100+ squads.
