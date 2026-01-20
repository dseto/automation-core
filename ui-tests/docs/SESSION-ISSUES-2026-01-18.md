# Relatório de Problemas - Sessão de Testes UI E2E

**Data:** 18/01/2026  
**Projeto:** metrics-simple-frontend/ui-tests  
**Dependências:** Automation.Core, Automation.Reqnroll

---

## Resumo Executivo

Durante a sessão de configuração e execução dos testes UI E2E, diversos problemas foram identificados que exigiram **contornos locais (workarounds)** no projeto de testes. Estes problemas indicam oportunidades de melhoria nos projetos `Automation.Core` e `Automation.Reqnroll` para reduzir a necessidade de customizações em projetos dependentes.

**Objetivo:** Projetos de testes devem conter apenas:
- Mapeamento de elementos (ui-map.yaml)
- Arquivos de features (.feature)
- Configuração mínima de ambiente

---

## Problemas Identificados

### 1. ❌ Falta de Contexto de Página nos Steps Básicos

**Problema:**  
O `BasicSteps.EntaoOElementoDeveEstarVisivel()` não tem conhecimento do contexto de página atual. Quando o feature file usa `"stat-total"` (nome simples), o step não sabe em qual página procurar o elemento.

**Erro observado:**
```
Element 'stat-total' was not found in ui-map for this page.
```

**Workaround local:**
```csharp
// PilotSteps.cs - Campo para rastrear página atual
private string? _currentPageName;

[Given(@"que estou na página ""(.*)""")]
public void DadoQueEstouNaPagina(string pageName)
{
    _currentPageName = pageName;  // Rastreia contexto
    // ...
}

private string CssByElementRef(string elementRef)
{
    // Suporta "username" (simples) e "login.username" (completo)
    if (!elementRef.Contains('.'))
    {
        pageName = _currentPageName 
            ?? throw new InvalidOperationException("Nenhuma página definida no contexto");
    }
    // ...
}
```

**Recomendação para o Core:**
- `AutomationRuntime` ou `PageContext` deve manter o estado da página atual
- `ElementResolver.Resolve()` deve aceitar nomes simples e inferir a página do contexto
- Steps básicos devem usar esse contexto automaticamente

---

### 2. ❌ Ambiguidade de Step Definitions

**Problema:**  
Ao criar um step local `[Then(@"o elemento ""(.*)"" deve estar visível")]` para ter contexto de página, houve conflito com o step de mesmo nome no `BasicSteps`.

**Erro observado:**
```
Ambiguous step definitions found for step 'Then o elemento "stat-total" deve estar visível':
- UiTests:PilotSteps.EntaoOElementoDeveEstarVisivel
- Automation.Reqnroll:BasicSteps.EntaoOElementoDeveEstarVisivel
```

**Workaround local:**
Criar step com texto diferente para evitar conflito:
```csharp
[Then(@"o elemento ""(.*)"" está visível")]  // "está" ao invés de "deve estar"
public void EntaoOElementoEstaVisivel(string elementRef)
```

E atualizar o feature file:
```gherkin
E o elemento "stat-total" está visível  # Antes: "deve estar visível"
```

**Recomendação para o Core:**
- Implementar mecanismo de **override de steps** (Reqnroll não suporta nativamente)
- Ou: Steps básicos devem delegar para um resolver injetável que projetos podem customizar
- Ou: Steps básicos devem verificar se há contexto de página antes de resolver elementos

---

### 3. ❌ Timing Insuficiente Após Navegação

**Problema:**  
O step de clique (`QuandoEuClicoEm`) tinha apenas 500ms de espera após o clique. Em modo headless (mais rápido), a verificação de rota falhava porque a navegação ainda não havia completado.

**Erro observado:**
```
Assert.Contains() Failure: Sub-string not found
String:    "https://gray-mushroom-0d87c190f.1.azurest"···
Not found: "/dashboard"
```

**Comportamento inconsistente:**
- ✅ Passava com `UI_DEBUG=true` e `SLOWMO_MS=2500`
- ❌ Falhava com `HEADLESS=true` e `SLOWMO_MS=0`

**Workaround local:**
```csharp
[When(@"eu clico em ""(.*)""")]
public void QuandoEuClicoEm(string elementRef)
{
    // ... clique ...
    
    Thread.Sleep(2000);  // Aumentado de 500ms para 2000ms
    _rt.Waits.WaitDomReady(_rt.Driver);
    _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);
}
```

**Recomendação para o Core:**
- Implementar `WaitForUrlChange()` ou `WaitForRouteContains(string route)`
- Implementar `WaitForNavigation()` que detecta mudança de URL automaticamente
- Steps de clique que causam navegação devem ter opção de aguardar mudança de rota
- Considerar retry com polling ao invés de sleep fixo

---

### 4. ❌ Steps de Credenciais Não Existem no Core

**Problema:**  
Não havia steps para preencher campos com credenciais de variáveis de ambiente (`TEST_USER`, `TEST_PASS`).

**Workaround local:**
```csharp
[When(@"eu preencho ""(.*)"" com as credenciais de usuário")]
public void QuandoEuPreenchoComCredenciaisDeUsuario(string elementRef)
{
    var user = Environment.GetEnvironmentVariable("TEST_USER") ?? "admin";
    QuandoEuPreenchoCom(elementRef, user);
}

[When(@"eu preencho ""(.*)"" com as credenciais de senha")]
public void QuandoEuPreenchoComCredenciaisDeSenha(string elementRef)
{
    var pass = Environment.GetEnvironmentVariable("TEST_PASS") ?? "admin";
    QuandoEuPreenchoCom(elementRef, pass);
}
```

**Recomendação para o Core:**
- Adicionar steps genéricos para credenciais:
  - `Quando eu preencho "{element}" com a variável de ambiente "{ENV_VAR}"`
  - `Quando eu preencho "{element}" com as credenciais de usuário`
  - `Quando eu preencho "{element}" com as credenciais de senha`
- Suportar placeholders em valores: `"${TEST_USER}"` expandido automaticamente

---

### 5. ❌ Duplicação de Testes por Cópia de .feature para bin/

**Problema:**  
O `.csproj` tinha configuração para copiar arquivos `.feature` para o output directory. O Reqnroll descobria testes tanto da pasta source quanto do bin/, resultando em 6 testes ao invés de 3.

**Sintoma:**
```
Total de testes: 6  (deveria ser 3)
Duas janelas de browser abrindo simultaneamente
```

**Workaround local:**
Remover a cópia do `.feature` do `.csproj`:
```xml
<!-- REMOVIDO -->
<None Update="features\*.feature">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

**Recomendação para o Core:**
- Documentar claramente que arquivos `.feature` NÃO devem ser copiados para output
- Fornecer template de `.csproj` correto
- Considerar adicionar analyzer/warning se detectar essa configuração

---

### 6. ❌ Step "que estou na página" vs "estou na página" (Then)

**Problema:**  
O step `Given que estou na página "{page}"` existe, mas não havia equivalente `Then estou na página "{page}"` para validação após navegação.

**Workaround local:**
```csharp
[Then(@"estou na página ""(.*)""")]
public void EntaoEstouNaPagina(string pageName)
{
    _currentPageName = pageName;  // Atualiza contexto
    var page = _rt.UiMap.GetPageOrThrow(pageName);
    // Valida que a página existe no ui-map
}
```

**Recomendação para o Core:**
- Adicionar step `Then estou na página "{page}"` que:
  - Atualiza o contexto de página
  - Opcionalmente valida se a URL contém a rota esperada
  - Opcionalmente aguarda o anchor da página estar visível

---

### 7. ⚠️ Filtro de Testes com OR Operator

**Problema:**  
O filtro `--filter "TestCategory=smoke|Category=smoke"` usava OR, fazendo com que testes aparecessem duplicados na descoberta (não era o problema principal, mas contribuía para confusão).

**Workaround local:**
Simplificar para apenas `--filter "Category=smoke"`.

**Recomendação para o Core:**
- Documentar padrões de filtro corretos
- Padronizar uso de `Category` vs `TestCategory`

---

## Arquivos Modificados Localmente

| Arquivo | Tipo de Mudança |
|---------|-----------------|
| `Steps/PilotSteps.cs` | +150 linhas de steps customizados |
| `features/login-smoke.feature` | Alteração de texto de steps |
| `UiTests.csproj` | Remoção de cópia de .feature |
| `scripts/run-debug.ps1` | Ajuste de filtros |
| `scripts/_env.ps1` | Credenciais de teste |

---

## Matriz de Priorização de Melhorias

| Melhoria | Impacto | Esforço | Prioridade |
|----------|---------|---------|------------|
| Contexto de página nos steps | Alto | Médio | 🔴 Alta |
| WaitForNavigation/WaitForUrlChange | Alto | Baixo | 🔴 Alta |
| Steps de credenciais genéricos | Médio | Baixo | 🟡 Média |
| Mecanismo de override de steps | Médio | Alto | 🟡 Média |
| Step "Then estou na página" | Baixo | Baixo | 🟢 Baixa |
| Documentação de .csproj | Baixo | Baixo | 🟢 Baixa |

---

## Proposta de API para Contexto de Página

```csharp
// Automation.Core - Nova interface
public interface IPageContext
{
    string? CurrentPage { get; }
    void SetPage(string pageName);
    string ResolveElement(string elementRef); // Aceita "username" ou "login.username"
}

// Automation.Reqnroll - Steps usando contexto
[Given(@"que estou na página ""(.*)""")]
public void GivenQueEstouNaPagina(string pageName)
{
    _pageContext.SetPage(pageName);
    // navegação...
}

[Then(@"o elemento ""(.*)"" deve estar visível")]
public void ThenOElementoDeveEstarVisivel(string elementRef)
{
    var css = _pageContext.ResolveElement(elementRef); // Usa contexto automaticamente
    // validação...
}
```

---

## Proposta de API para Waits de Navegação

```csharp
// Automation.Core - Novos métodos em Waits
public interface IWaits
{
    // Existentes
    void WaitDomReady(IWebDriver driver);
    bool TryWaitAngularStable(IWebDriver driver, out string? error);
    
    // NOVOS
    void WaitForUrlContains(IWebDriver driver, string substring, int timeoutMs = 10000);
    void WaitForUrlChange(IWebDriver driver, string previousUrl, int timeoutMs = 10000);
    void WaitForNavigation(IWebDriver driver, Action clickAction, int timeoutMs = 10000);
}

// Uso em step de clique com navegação
[When(@"eu clico em ""(.*)"" e aguardo navegação")]
public void QuandoEuClicoEAguardoNavegacao(string elementRef)
{
    var currentUrl = _driver.Url;
    // clique...
    _waits.WaitForUrlChange(_driver, currentUrl);
}
```

---

## Conclusão

Os testes agora funcionam com **3 cenários passando**, mas exigiram ~150 linhas de código customizado no projeto de testes. O objetivo é que esse código migre para o `Automation.Core` e `Automation.Reqnroll`, permitindo que projetos de testes contenham apenas:

1. `ui-map.yaml` - Mapeamento de elementos
2. `*.feature` - Cenários de teste
3. `_env.ps1` - Configuração de ambiente
4. Mínimo de steps customizados para lógica específica do negócio

---

*Documento gerado durante sessão de debug em 18/01/2026*
