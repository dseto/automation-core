# Scripts de Teste - Como Usar

## Problema: Múltiplas Janelas Abertas

O problema foi que o xUnit (padrão) executa testes em **paralelo**, abrindo múltiplas janelas do navegador simultaneamente. Para debug visual, precisamos de execução **sequencial**.

## Solução Implementada

Adicionado `-- RunConfiguration.MaxCpuCount=1` em todos os scripts, forçando execução sequencial (1 teste por vez).

## Scripts Disponíveis

### 1. `run-debug.ps1` (Recomendado - Smoke Completo)
Executa todos os cenários smoke **sequencialmente** com debug visual.

```bash
cd C:\Projetos\metrics-simple-frontend\ui-tests
.\scripts\run-debug.ps1
```

**O que acontece:**
- Abre 1 janela por vez
- Executa os 3 cenários sequencialmente:
  1. Login com credenciais válidas
  2. Login com credenciais inválidas
  3. Toggle de visibilidade de senha
- Cada passo tem pausa de 2.5s
- Mostra destaque visual dos elementos

### 2. `run-debug-single.ps1` (Novo - Um Cenário)
Executa apenas **um cenário** por vez (ideal para debug individual).

```bash
cd C:\Projetos\metrics-simple-frontend\ui-tests
.\scripts\run-debug-single.ps1 -Scenario "Login com credenciais válidas"
```

**Opções de Cenários:**
```bash
.\scripts\run-debug-single.ps1 -Scenario "Login com credenciais válidas"
.\scripts\run-debug-single.ps1 -Scenario "Login com credenciais inválidas"
.\scripts\run-debug-single.ps1 -Scenario "Toggle de visibilidade de senha"
```

### 3. `run-smoke.ps1` (CI/Produção)
Executa testes em modo **headless** (sem interface gráfica) com paralelismo controlado.

```bash
cd C:\Projetos\metrics-simple-frontend\ui-tests
.\scripts\run-smoke.ps1
```

**Configuração:**
- ✅ Headless (sem abrir navegador)
- ✅ Sem pauses (velocidade máxima)
- ✅ Sequencial (MaxCpuCount=1)

## Comparação de Comportamento

| Feature | run-debug.ps1 | run-debug-single.ps1 | run-smoke.ps1 |
|---------|---------|---------|---------|
| Modo | Visual | Visual | Headless |
| Paralelismo | Sequencial (1 janela) | Sequencial (1 janela) | Sequencial |
| SLOWMO_MS | 2500ms | 2500ms | 0ms |
| HEADLESS | false | false | true |
| Highlight | true | true | false |
| Ideal para | Debug geral | Debug específico | CI/Testes rápidos |

## Configurações de Ambiente

Todos os scripts usam estas variáveis (em `_env.ps1`):

```powershell
# Debug visual
$env:UI_DEBUG = "true"              # Ativa destaque de elementos
$env:HEADLESS = "false"             # Abre navegador visível
$env:SLOWMO_MS = "2500"             # Pausa entre steps (ms)
$env:HIGHLIGHT = "true"             # Borda ao redor de elementos

# URLs
$env:BASE_URL = "http://localhost:4200"  # Para desenvolvimento local
$env:TEST_USER = "admin"
$env:TEST_PASS = "ChangeMe123!"
```

## Solução Implementada

### Arquivo: `run-debug.ps1`
```powershell
dotnet test $TestProject --filter "TestCategory=smoke|Category=smoke" -- RunConfiguration.MaxCpuCount=1
```

### Arquivo: `run-smoke.ps1`
```powershell
dotnet test $TestProject --filter "TestCategory=smoke|Category=smoke" -- RunConfiguration.MaxCpuCount=1
```

### Novo Arquivo: `run-debug-single.ps1`
Permite rodar um cenário por vez com:
```powershell
.\scripts\run-debug-single.ps1 -Scenario "Seu Cenário"
```

## Resultado

✅ **Antes**: 3 janelas abertas simultaneamente
✅ **Depois**: 1 janela por vez, execução sequencial

Agora você consegue ver cada step claramente sem confusão de múltiplas janelas!
