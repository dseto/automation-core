# 03 - Detalhamento do Projeto `Automation.Core`

O `Automation.Core` é o motor da plataforma. Este documento detalha seus componentes mais importantes e como eles interagem.

---

## 🧩 Principais Namespaces e Classes

### `Automation.Core.Resolution`
Este é o namespace mais crítico. Contém a lógica que transforma as intenções declarativas em ações concretas.

| Classe | Responsabilidade |
|---|---|
| `DataResolver.cs` | **Resolve dados.** Implementa a **Sintaxe Explícita** (`@`, `{{}}`, `${}`). Recebe uma string do Gherkin e retorna o valor final a ser usado no teste, aplicando a ordem de resolução determinística. |
| `ElementResolver.cs` | **Resolve elementos.** Recebe um nome amigável (ex: "username") e, usando o `PageContext`, consulta o `UiMap` para retornar o seletor CSS correspondente (`[data-testid=...]`). |
| `PageContext.cs` | **Gerencia o estado da página.** Mantém uma referência à página atual (`CurrentPageName`). É responsável por validar o **Anchor Pattern** ao navegar, garantindo que a página correta foi carregada. |

### `Automation.Core.UiMap` e `Automation.Core.DataMap`
Estes namespaces contêm os modelos e os loaders para os contratos YAML.

| Classe | Responsabilidade |
|---|---|
| `UiMapModel.cs` | Define a estrutura de um `ui-map.yaml`, incluindo a hierarquia `pages > __meta > elements`. É aqui que a propriedade `Anchor` é definida. |
| `DataMapModel.cs` | Define a estrutura de um `data-map.yaml`, incluindo `contexts` e `datasets`. |
| `UiMapLoader.cs` / `DataMapLoader.cs` | Usam a biblioteca `YamlDotNet` para desserializar os arquivos YAML para os modelos C# correspondentes. |
| `UiMapValidator.cs` / `DataMapValidator.cs` | Contêm a lógica de validação que é consumida pelo `Automation.Validator`. Por exemplo, `UiMapValidator` verifica se páginas com rotas duplicadas possuem um `anchor`. |

### `Automation.Core.Driver`
Abstrai a criação e gerenciamento de instâncias do `WebDriver`.

| Classe | Responsabilidade |
|---|---|
| `DriverFactory.cs` | Uma fábrica que, com base em variáveis de ambiente (`BROWSER`, `HEADLESS`), cria a instância correta do driver (ChromeDriver, EdgeDriver). |

### `Automation.Core.Waits`
Fornece métodos de espera explícita para lidar com a natureza assíncrona de aplicações web.

| Classe | Responsabilidade |
|---|---|
| `WaitService.cs` | Oferece métodos como `WaitForElementVisible`, `WaitForUrlContains` e, crucialmente, `WaitPageAnchor`. Centraliza a lógica de `WebDriverWait`. |

---

## 💡 Como a Sintaxe Explícita Funciona no `DataResolver`

O método `Resolve(string reference)` é o coração da nova lógica.

```csharp
public object Resolve(string reference)
{
    // 1. Tenta resolver como Variável de Ambiente
    if (reference.StartsWith("${
") && reference.EndsWith("}"))
    {
        // Extrai o nome da variável e busca no ambiente
        return Environment.GetEnvironmentVariable(variableName);
    }

    // 2. Tenta resolver como Dataset
    if (reference.StartsWith("{{") && reference.EndsWith("}}"))
    {
        // Extrai o nome do dataset e retorna o próximo item
        return dataMap.GetNextDatasetItem(datasetName);
    }

    // 3. Tenta resolver como Objeto
    if (reference.StartsWith("@"))
    {
        // Extrai o nome do objeto e busca no contexto atual do DataMap
        return dataMap.GetDataObject(objectName);
    }

    // 4. Se nada der certo, é um Literal
    return reference;
}
```

---

## ⚓ Como o Anchor Pattern Funciona no `PageContext`

O `PageContext` trabalha em conjunto com o `NavigationSteps` para garantir a correta identificação da página.

1.  **`NavigationSteps`:** Quando o step `Dado que estou na página "minha_pagina"` é executado, ele chama um método no `PageContext` (ex: `SetCurrentPage`).
2.  **`PageContext.SetCurrentPage`:**
    *   Atualiza a propriedade `CurrentPageName`.
    *   Busca a definição da página no `UiMap`.
    *   Verifica se a propriedade `Anchor` está definida no `__meta` da página.
    *   Se estiver, chama `WaitService.WaitPageAnchor(driver, page.Anchor)`.
3.  **`WaitService.WaitPageAnchor`:** Aguarda explicitamente até que o elemento definido como `anchor` esteja visível na tela.

Se o anchor não aparecer dentro do timeout, uma exceção é lançada, indicando que a navegação falhou, mesmo que a URL esteja correta. Isso previne falsos positivos em SPAs.

---

**Próximo Documento:** [04 - Detalhamento do Projeto Reqnroll](04-REQNROLL-PROJECT.md)
