# Scripts de Teste

Conjunto de scripts PowerShell para executar testes de UI com diferentes configurações.

## ⚠️ Observação importante

Os scripts foram atualizados para serem executados a partir de **qualquer diretório**. Eles carregam automaticamente o arquivo de variáveis `_env.ps1` relativo à pasta `ui-tests\scripts` quando necessário.

Exemplos de execução a partir do diretório raiz do repositório:

```powershell
# Executar smoke (headless)
powershell -NoProfile -ExecutionPolicy Bypass -File .\ui-tests\scripts\run-smoke.ps1

# Executar debug visual com cenário específico
powershell -NoProfile -ExecutionPolicy Bypass -File .\ui-tests\scripts\run-debug.ps1 -Scenario "LoginComSucesso"
```

Ainda é possível usar o modo interativo (carregar `_env.ps1` manualmente) para customizar variáveis na sessão atual:

```powershell
cd ui-tests\scripts
. .\_env.ps1
```
## Scripts Disponíveis

### `run-smoke.ps1`
Executa testes smoke em modo headless (CI-like).

```powershell
. .\_env.ps1
.\run-smoke.ps1
```

**Configuração:**
- `HEADLESS=true`
- `UI_DEBUG=false`
- `SLOWMO_MS=0`

---

### `run-debug.ps1`
Executa testes em modo debug visual (headed, com slowmo).

```powershell
. .\_env.ps1
.\run-debug.ps1

# Ou com cenário específico:
.\run-debug.ps1 -Scenario "LoginComSucesso"
```

**Configuração:**
- `HEADLESS=false`
- `UI_DEBUG=true`
- `SLOWMO_MS=1000`
- `HIGHLIGHT=true`

---

### `run-recorder.ps1`
Executa testes do Recorder Session e exibe o `session.json` gerado.

```powershell
. .\_env.ps1
.\run-recorder.ps1
```

**Configuração:**
- `AUTOMATION_RECORD=true`
- `RECORD_OUTPUT_DIR=..\artifacts\recorder`
- `HEADLESS=false` (para visualizar gravação)
- `SLOWMO_MS=500`

**Output esperado:** `ui-tests\artifacts\recorder\session.json`

---

### `run-validate.ps1`
Valida ui-map.yaml e features antes de executar testes.

```powershell
. .\_env.ps1
.\run-validate.ps1
```

---

## Variáveis de Ambiente

As variáveis são definidas em `_env.ps1`:

| Variável | Default | Descrição |
|----------|---------|-----------|
| `BASE_URL` | Azure Static App | URL base da aplicação |
| `UI_MAP_PATH` | `../ui/ui-map.yaml` | Caminho do UI Map |
| `HEADLESS` | `true` | Executar browser em modo headless |
| `WAIT_ANGULAR` | `true` | Aguardar estabilização Angular |
| `ANGULAR_TIMEOUT_MS` | `1000` | Timeout para Angular (ms) |
| `STEP_TIMEOUT_MS` | `10000` | Timeout por step (ms) |
| `BROWSER` | `edge` | Browser (chrome/edge) |
| `AUTOMATION_RECORD` | `false` | Habilitar gravação de sessão |
| `RECORD_OUTPUT_DIR` | `artifacts/recorder` | Diretório de saída do recorder |

---

## Troubleshooting

### ❌ Erro: "Variável de ambiente BASE_URL não definida"
**Solução:** Execute `. .\_env.ps1` antes do script de teste.

### ❌ Erro: "Cannot find path 'ui-map.yaml'"
**Solução:** Verifique se está executando do diretório `ui-tests\scripts`.

### ❌ session.json não foi gerado
**Solução:** Verifique se `AUTOMATION_RECORD=true` está definido no script ou via `_env.ps1`.

---

## Exemplo de Uso Completo

```powershell
# 1. Navegar para diretório de scripts
cd C:\Projetos\automation-core\ui-tests\scripts

# 2. Carregar variáveis de ambiente
. .\_env.ps1

# 3. Executar testes
.\run-smoke.ps1      # CI-like
.\run-debug.ps1      # Visual debug
.\run-recorder.ps1   # Gravação de sessão
```
