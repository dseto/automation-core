# Run Settings (contrato de execução)

> Fonte de verdade: `Automation.Core/Configuration/RunSettings.cs` e `Automation.Reqnroll/Hooks/RuntimeHooks.cs`.

## Variáveis de ambiente suportadas (atuais)

### Execução / Ambiente
- `BASE_URL` (string, default: `""`)  
  Base URL do sistema sob teste. (Pode ser usado por steps de navegação / runtime)
- `TEST_ENV` (string, default: `"default"`)  
  Seleciona o contexto em `contexts[TEST_ENV]` no `data-map.yaml`.

### Browser / Driver
- `BROWSER` (string, default: `"chrome"`)  
  Suporta: `chrome` | `edge`. Valores desconhecidos caem em chrome.

### UI Map path
- `UI_MAP_PATH` (string, default: *auto-resolve*)  
  Se definido e existir no filesystem, é usado.
  Caso contrário, o runtime procura (subindo diretórios, até 10 níveis) por:
  - `ui/ui-map.yaml`
  - `samples/ui/ui-map.yaml`
  - `ui-map.yaml`
  Fallback final: `"ui-map.yaml"`.

### Headless / Debug
- `HEADLESS` (bool, default: `true`)  
  Controla headless do driver (via factories).
- `UI_DEBUG` (bool, default: `false`)  
  Habilita “modo debug” e afeta defaults abaixo.

### Ritmo / Interação (stability)
- `SLOWMO_MS` (int, default: **250** se `UI_DEBUG=true`, senão **0**)  
  Delay artificial entre ações.
- `HIGHLIGHT` (bool, default: **UI_DEBUG**)  
  Destaca elemento durante interações (quando implementado/ligado).
- `PAUSE_ON_FAILURE` (bool, default: **UI_DEBUG**)  
  Pausa ao falhar (quando implementado/ligado).

### Angular / Waits
- `WAIT_ANGULAR` (bool, default: `true`)  
  Tenta aguardar estabilidade do Angular quando aplicável.
- `ANGULAR_TIMEOUT_MS` (int, default: `5000`)  
  Timeout para “Angular stable”.

### Timeout de step
- `STEP_TIMEOUT_MS` (int, default: `20000`)  
  Timeout de ações/steps (quando aplicável).

### Recorder (FREE-HANDS)
- `AUTOMATION_RECORD` (bool, default: `false`)  
  Habilita gravação de sessão (session.json).
- `RECORD_OUTPUT_DIR` (string, default: `"artifacts/recorder"`)  
  Diretório de saída para o session.json.

## Observações importantes
- Defaults dependentes de `UI_DEBUG` estão implementados diretamente em `RunSettings.FromEnvironment()`.
- `BUILD_BUILDID`, `TF_BUILD`, `GITHUB_ACTIONS` podem existir no ambiente (CI), mas **não** são settings do produto.
