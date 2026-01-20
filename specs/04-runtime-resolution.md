# 04 - Runtime Resolution

## Fluxo de Execução Completo

### 1. Parse Gherkin
O Reqnroll lê o step Gherkin (ex: "Quando eu preencho 'username' com 'admin'"). O step é mapeado para um método C# no InteractionSteps.

### 2. Resolução de Elemento
O ElementResolver recebe a referência "username". Consulta `PageContext.CurrentPageName` (ex: "login"). Busca a página no UiMap. Localiza o elemento "username" na página "login". Retorna o CSS locator `[data-testid='page.login.username']`.

### 3. Resolução de Dados (Sintaxe Explícita)
O DataResolver recebe o valor e aplica ordem determinística com prefixos:
1. Variável de Ambiente (`${...}`)
2. Dataset (`{{...}}`)
3. Objeto (`@...`)
4. Literal (sem prefixo)

Exemplos: `${BASE_URL}`, `{{cpfs_teste}}`, `@user_admin`, `admin`

### 4. Execução
O WebDriver usa o CSS locator para encontrar o elemento. Executa a ação (clique, preenchimento, etc.). Captura o resultado.

### 5. Wait
O WaitService aguarda estabilidade. Para navegação, aguarda a URL mudar. Para elementos, aguarda visibilidade. Para Angular, aguarda $http.pendingRequests. Timeout padrão é 20 segundos.

### 6. Captura de Evidências
Se houver sucesso, continua. Se houver falha, o RuntimeHooks captura screenshot e logs.

## Resolução de Elemento (ElementResolver)

### Algoritmo
```
function ResolveElement(friendlyName):
  page = PageContext.CurrentPageName
  if page is null:
    throw "PageContext not set"
  
  uiPage = UiMap.GetPage(page)
  if uiPage is null:
    throw "Page not found in UiMap"
  
  element = uiPage.GetElement(friendlyName)
  if element is null:
    throw "Element not found in page"
  
  return CssLocator(element.testId)
```

### Casos Especiais
Se o elemento não for encontrado na página atual, o resolver tenta buscar em "shared" (elementos globais). Se ainda não encontrar, lança exceção com sugestão de elementos disponíveis.

## Resolução de Dados (DataResolver - Sintaxe Explícita)

### Algoritmo Determinístico
A ordem de verificação é fixa:
1. Variável de Ambiente: `${...}`
2. Dataset: `{{...}}`
3. Objeto: `@...`
4. Literal: sem prefixo

Isso elimina ambiguidade e torna o comportamento previsível.

### Estratégias de Dataset
- Sequential: Mantém índice interno, incrementa a cada acesso
- Random: Seleciona aleatoriamente
- Unique: Garante que não há repetição dentro de um cenário

## Resolução de Rota (PageContext)

### Atualização Automática
Quando o WebDriver navega para uma nova URL, o PageContext é atualizado automaticamente. O novo nome da página é determinado pelo mapeamento rota → pageName no UiMap.

### Validação de Rota
O WaitService pode aguardar uma rota específica (ex: "/dashboard"). Verifica se a URL atual contém a substring esperada. Útil após cliques que disparam navegação.

## Tratamento de Erros

### Elemento Não Encontrado
Se o ElementResolver não encontrar um elemento, lança `ElementNotFoundException` com a lista de elementos disponíveis na página. Ajuda o QA a corrigir o nome.

### Dados Não Encontrados
Se o DataResolver não encontrar uma chave, lança `DataNotFoundException` com sugestão de chaves disponíveis no contexto.

### Timeout
Se o WaitService exceder o timeout, lança `WebDriverTimeoutException` com informações sobre o que estava aguardando.
