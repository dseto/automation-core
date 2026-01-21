# FREE-HANDS Recorder — Guia Prático (RF00)

**Status:** 🟢 Ready for Production  
**Última atualização:** 2026-01-21  
**Público:** QAs, BAs, Testers

---

## O que é o FREE-HANDS Recorder?

O **FREE-HANDS Recorder** é uma ferramenta para **exploração manual de aplicações web**. Você:

1. Abre o browser
2. Interage com a aplicação livremente (cliques, preenchimento de campos, navegação)
3. Ao encerrar, o sistema gera um arquivo `session.json` com todas as interações capturadas

**Não requer:** cenários de teste, `.feature` files, ou conhecimento de BDD.

---

## Modo de Uso — Passo a Passo

### Pré-requisitos
- PowerShell 5.1+
- .NET SDK 8.0+
- Projeto clonado em `C:\Projetos\automation-core\`

### 1. Abrir PowerShell
```powershell
cd C:\Projetos\automation-core\ui-tests\scripts
```

### 2. Carregar Configurações Base
```powershell
. .\\_env.ps1
```

Isso carrega variáveis como:
- `BASE_URL`: URL padrão da aplicação
- `BROWSER`: Tipo de browser (Edge, Chrome)
- Timeouts e configurações de debug

### 3. Iniciar o Recorder

**Forma Simples (usa BASE_URL):**
```powershell
.\run-free-hands.ps1
```

**Com URL Customizada:**
```powershell
.\run-free-hands.ps1 -Url "https://meuapp.com/dashboard"
```

**Com Diretório de Saída Customizado:**
```powershell
.\run-free-hands.ps1 -Url "https://meuapp.com" -OutputDir ".\artifacts\recorder-custom"
```

### 4. O Browser Abre
```
╔════════════════════════════════════════════════════════════════╗
║  FREE-HANDS Recorder — Modo Exploratório (RF00)                ║
╚════════════════════════════════════════════════════════════════╝

Configuração:
  BASE_URL:          https://meuapp.com
  RECORD_OUTPUT_DIR: C:\Projetos\automation-core\artifacts\recorder
  BROWSER:           ChromeDriver

Quando terminar:
  • Feche o browser, OU
  • Pressione CTRL+C
```

**O browser abre** em modo "headed" (com visualização). Você pode:
- Navegar para diferentes URLs
- Clicar em elementos
- Preencher formulários
- Submeter dados
- Abrir/fechar modais
- Voltar/avançar páginas

**Interações são registradas automaticamente** conforme você navega.

### 5. Encerrar o Recorder

**Opção A:** Fechar o browser normalmente
- Clique no X da janela do browser

**Opção B:** CTRL+C no PowerShell
```
[INFO] CTRL+C detectado. Encerrando...
[INFO] Session.json escrito em: C:\Projetos\automation-core\artifacts\recorder\session.json
exit 0
```

---

## Saída: session.json

Após encerrar, um arquivo `session.json` é gerado com toda a sessão de interação.

**Estrutura:**
```json
{
  "sessionId": "e37e5a2ce7c346a2b3b8882fe163ff89",
  "startedAt": "2026-01-21T04:45:42.1397166+00:00",
  "endedAt": "2026-01-21T04:45:50.531941+00:00",
  "events": [
    {
      "t": "00:00.173",
      "type": "navigate",
      "route": "/",
      "title": "Example Domain"
    },
    {
      "t": "00:02.456",
      "type": "click",
      "selector": "[data-testid=\"login-btn\"]",
      "tagName": "button",
      "text": "Sign In"
    },
    {
      "t": "00:03.100",
      "type": "fill",
      "selector": "[data-testid=\"email-input\"]",
      "tagName": "input",
      "value": "user@example.com"
    },
    {
      "t": "00:03.500",
      "type": "fill",
      "selector": "[data-testid=\"password-input\"]",
      "tagName": "input",
      "value": "password123"
    },
    {
      "t": "00:04.100",
      "type": "submit",
      "selector": "[data-testid=\"login-form\"]",
      "tagName": "form"
    }
  ]
}
```

**Campos:**
- `sessionId`: Identificador único da sessão
- `startedAt` / `endedAt`: Timestamps (ISO 8601)
- `events`: Array de eventos capturados

**Tipos de Evento:**
- `navigate`: Navegação para URL
- `click`: Clique em elemento
- `fill`: Preenchimento de campo
- `select`: Mudança de seleção (dropdown, radio)
- `toggle`: Mudança de checkbox/switch
- `submit`: Envio de formulário
- `modal-open` / `modal-close`: Abertura/fechamento de modal

---

## Casos de Uso

### 1. Exploração Inicial de Aplicação
Você recebeu uma aplicação nova e precisa entender fluxos. Use o recorder para:
- Navegar pelas funcionalidades
- Documentar cliques, preenchimentos e comportamentos
- Gerar session.json para análise posterior

```powershell
.\run-free-hands.ps1 -Url "https://nova-app.com"
# Explore, depois CTRL+C
# Arquivo gerado: artifacts/recorder/session.json
```

### 2. Reprodução de Bug
QA encontrou bug complexo. Use o recorder para:
- Replicar manualmente as interações que causam o bug
- Capturar exatamente o caminho até a falha
- Gerar session.json com timestamp preciso de cada etapa

```powershell
.\run-free-hands.ps1 -Url "https://app.com/problematic-page" -OutputDir ".\artifacts\bug-reproduction"
# Reproduza o bug passo a passo
# Arquivo: artifacts/bug-reproduction/session.json
```

### 3. Coleta de Dados para Automação
Dev precisa criar testes automatizados. Use o recorder para:
- Coletar seletores (`data-testid`)
- Capturar sequência de interações
- Usar session.json como referência para step definitions

```powershell
.\run-free-hands.ps1 -Url "https://app.com/login"
# Execute fluxo de login completo
# Use events para informar step_definitions
```

---

## Troubleshooting

### ❌ "BASE_URL não definida"
**Solução:** Configure em `_env.ps1` ou passe via parâmetro:
```powershell
.\run-free-hands.ps1 -Url "https://app.com"
```

### ❌ "Projeto RecorderTool não encontrado"
**Solução:** Verifique caminho do projeto:
```powershell
Test-Path "C:\Projetos\automation-core\src\Automation.RecorderTool\Automation.RecorderTool.csproj"
```

### ❌ Browser não abre
**Solução:** Verifique se ChromeDriver/EdgeDriver está disponível:
```powershell
$env:HEADLESS = "false"
$env:UI_DEBUG = "true"
.\run-free-hands.ps1 -Url "https://app.com"
```

### ❌ session.json não gerado
**Solução:** Verifique diretório de saída:
```powershell
# Padrão
Get-ChildItem -Recurse -Filter "session.json" C:\Projetos\automation-core\artifacts\

# Custom
Get-ChildItem -Recurse -Filter "session.json" C:\Projetos\automation-core\artifacts\recorder-custom\
```

---

## Dicas & Boas Práticas

### ✅ Do
- **Use `data-testid`** sempre que possível (seletores mais estáveis)
- **Testes em ambiente limpo** (clear cache, logout, start fresh)
- **Interações deliberadas** (não clique aleatoriamente, seja intencional)
- **Documente o objetivo** (antes de gravar, saiba o que quer reproduzir)
- **Verifique session.json** após encerrar (valide que eventos foram capturados)

### ❌ Don't
- ❌ **Não use modo headless** (HEADLESS=true) — você não verá o que está acontecendo
- ❌ **Não invente seletores** — use os que o dev informou
- ❌ **Não misture múltiplos fluxos** — uma gravação = um objetivo
- ❌ **Não pressione CTRL+C abruptamente** — aguarde o log de encerramento
- ❌ **Não edite session.json manualmente** — é saída automatizada, não para edição

---

## Integração com Testes (Opcional)

Se você deseja **gravar também durante execução de testes** (diferente de modo exploratório):

```powershell
$env:AUTOMATION_RECORD = "true"
dotnet test .\ui-tests\UiTests.csproj
```

Isso:
- Executa testes normalmente (via Reqnroll)
- Gera session.json **para cada cenário** durante a execução
- Mantém compatibilidade com modo exploratório

**Mas RF00 (modo exploratório puro) funciona INDEPENDENTE dessa opção.**

---

## Referências

- [RF00 Compliance Audit](../../specs/releases/delta/2026-01-21-free-hands-recorder-exploratory-mode/RF00-COMPLIANCE-AUDIT.md)
- [Session Recording Specification](../../specs/backend/requirements/free-hands-recorder-session.md)
- [PowerShell Scripts Reference](./05-ESCAPE-HATCH-GUIDE.md)

---

**Pronto para usar!** 🚀

Se tiver dúvidas, consulte a [Documentação para Desenvolvedores](../dev-wiki/HOME.md).
