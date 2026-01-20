# 08 - Mecanismo de Escape-Hatch: Execução de JavaScript

Este documento detalha a implementação e as considerações arquiteturais do mecanismo de Escape-Hatch, que permite a execução de JavaScript a partir de steps Gherkin.

---

## 🎯 Propósito Arquitetural

O Escape-Hatch é uma concessão pragmática. A arquitetura da plataforma é projetada para abstrair completamente o desenvolvedor de testes (QA) da necessidade de programar. No entanto, reconhece-se que o mundo real das aplicações web é complexo e, por vezes, imprevisível.

O objetivo do Escape-Hatch é fornecer uma **válvula de escape controlada** para cenários onde a interação padrão do WebDriver/Selenium falha ou é insuficiente, sem comprometer a estrutura geral da plataforma.

**Princípio Chave:** O mecanismo deve ser implementado na camada mais alta possível (a camada de Steps), com o mínimo de envolvimento do `Core`, para reforçar a ideia de que é uma exceção e não uma funcionalidade central.

---

## ⚙️ Implementação

A funcionalidade é implementada inteiramente dentro da classe `InteractionSteps.cs` no projeto `Automation.Reqnroll`.

### `InteractionSteps.cs`

Dois métodos de step públicos fornecem a funcionalidade:

1.  **`QuandoEuExecutoOScriptJS(string script)`**
    *   **Gherkin:** `Quando eu executo o script JS "..."`
    *   **Implementação:**
        ```csharp
        public void QuandoEuExecutoOScriptJS(string script)
        {
            var jsExecutor = (IJavaScriptExecutor)_rt.Driver;
            jsExecutor.ExecuteScript(script);
        }
        ```
    *   **Análise:** Este método simplesmente pega a string do Gherkin e a passa diretamente para o `IJavaScriptExecutor` do Selenium. É uma passagem direta, sem envolvimento do `Automation.Core`.

2.  **`QuandoEuExecutoOScriptNoElemento(string script, string elementRef)`**
    *   **Gherkin:** `Quando eu executo o script "..." no elemento "..."`
    *   **Implementação:**
        ```csharp
        public void QuandoEuExecutoOScriptNoElemento(string script, string elementRef)
        {
            // 1. Resolve o elemento usando o Core
            var selector = _elementResolver.Resolve(elementRef);
            var element = _waitService.WaitForElementVisible(By.CssSelector(selector));

            // 2. Executa o script com o elemento como argumento
            var jsExecutor = (IJavaScriptExecutor)_rt.Driver;
            jsExecutor.ExecuteScript(script, element);
        }
        ```
    *   **Análise:** Este método faz uso do `Automation.Core` para uma única tarefa: resolver a referência do elemento (`elementRef`) para um objeto `IWebElement`. Após a resolução, a execução do script ainda é de responsabilidade direta do step, passando o elemento encontrado como o `arguments[0]` para o script.

---

## ⚖️ Decisões de Design e Trade-offs

*   **Localização na Camada de Steps:** A decisão de manter a lógica do Escape-Hatch exclusivamente no `Automation.Reqnroll` foi intencional. Colocá-la no `Automation.Core` (ex: em um `JavaScriptService`) a legitimaria como uma funcionalidade central, o que vai contra a filosofia da plataforma. Mantê-la nos steps a posiciona corretamente como uma ferramenta de conveniência para casos extremos.

*   **Segurança:** A execução de scripts arbitrários introduz um vetor de risco. No contexto de testes de UI, onde o ambiente é controlado, o risco é baixo. No entanto, os scripts são executados com os mesmos privilégios da página, o que deve ser um ponto de atenção se os testes forem executados contra ambientes de produção.

*   **Fragilidade vs. Poder:** O principal trade-off é a fragilidade. Um teste que depende de um script JS está acoplado à estrutura do DOM e ao JavaScript da página. Uma pequena refatoração no front-end pode quebrar o teste. Em contrapartida, ele oferece o poder de contornar bloqueios que, de outra forma, impediriam a automação.

---

## 🚀 Extensibilidade Futura

Embora o objetivo seja limitar o uso do Escape-Hatch, algumas extensões poderiam ser consideradas se a necessidade se provar recorrente:

*   **Scripts Pré-definidos:** Criar um catálogo de scripts comuns (ex: `scroll_to_bottom`, `force_click`) que poderiam ser chamados por nome, em vez de passar o código JS inteiro no Gherkin. Isso reduziria a duplicação e o risco de erros de sintaxe.
    ```gherkin
    Quando eu executo o script nomeado "force_click" no elemento "botao_submit"
    ```

*   **Retorno de Valores:** Modificar os steps para retornar o valor da execução do script para uma variável de cenário do Reqnroll, permitindo validações sobre o resultado.
    ```gherkin
    Quando eu executo o script "return localStorage.getItem("token");" e salvo o resultado como "user_token"
    Então a variável "user_token" não deve ser nula
    ```

Qualquer extensão nesse sentido deve ser avaliada cuidadosamente para não incentivar o uso excessivo do mecanismo.
