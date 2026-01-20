# 04 - Detalhamento do Projeto `Automation.Reqnroll`

O `Automation.Reqnroll` serve como uma ponte entre a linguagem natural do Gherkin e a lógica de execução do `Automation.Core`. É a camada mais próxima do Analista de QA.

---

## 🎯 Propósito e Filosofia

O objetivo principal deste projeto é fornecer um **catálogo fixo e genérico de steps**. A filosofia é que os QAs não devem precisar escrever código C#. Eles devem ser capazes de testar qualquer aplicação combinando os steps pré-existentes.

**A criação de novos steps deve ser uma exceção, não a regra.** Se um novo step é necessário, isso geralmente indica uma oportunidade para generalizar uma funcionalidade no `Automation.Core`.

---

## 🧩 Estrutura e Classes Principais

### `Automation.Reqnroll.Steps`
Este namespace contém as classes que definem os bindings do Gherkin.

| Classe | Responsabilidade |
|---|---|
| `NavigationSteps.cs` | Contém os steps relacionados à navegação, como `Dado que estou na página {pageName}` e `E eu aguardo a rota {routeName}`. É aqui que o `PageContext` e a validação do **Anchor Pattern** são invocados. |
| `InteractionSteps.cs` | Define os steps de interação com elementos, como `Quando eu clico em {elementName}` e `Quando eu preencho {elementName} com {dataKey}`. Esta classe utiliza o `ElementResolver` e o `DataResolver` extensivamente. |
| `ValidationSteps.cs` | Fornece os steps para asserções e validações, como `Então o texto {text} deve estar visível` e `Então o elemento {elementName} não deve existir`. |

### Exemplo de um Step em `InteractionSteps.cs`

```csharp
[Quando(@"eu preencho o campo \"(.*?)\" com os dados de \"(.*?)\"")]
public async Task QuandoEuPreenchoCom(string elementRef, string dataKey)
{
    // 1. Resolve o elemento para obter o seletor
    var selector = _elementResolver.Resolve(elementRef);

    // 2. Resolve os dados para obter o valor
    var dataValue = _dataResolver.Resolve(dataKey);

    // 3. Chama o serviço de interação no Core
    await _interactionService.FillAsync(selector, dataValue.ToString());
}
```

Este exemplo demonstra o padrão: o step na camada `Reqnroll` é muito "magro". Ele apenas orquestra chamadas para os serviços do `Core`, onde a lógica de negócio realmente reside.

### `Automation.Reqnroll.Hooks`
Este namespace contém a lógica que é executada em eventos específicos do ciclo de vida do teste.

| Classe | Responsabilidade |
|---|---|
| `RuntimeHooks.cs` | Define métodos com atributos como `[BeforeScenario]` e `[AfterScenario]`. É usado para inicializar o WebDriver no início de um teste e para capturar screenshots e logs em caso de falha no final. |

---

## 💉 Injeção de Dependência

O `Reqnroll` utiliza injeção de dependência para obter instâncias dos serviços do `Core`. O framework de BDD gerencia o ciclo de vida desses objetos.

```csharp
public class InteractionSteps : StepsBase
{
    private readonly ElementResolver _elementResolver;
    private readonly DataResolver _dataResolver;

    // O Reqnroll injeta as dependências automaticamente no construtor
    public InteractionSteps(ElementResolver elementResolver, DataResolver dataResolver)
    {
        _elementResolver = elementResolver;
        _dataResolver = dataResolver;
    }

    // ... steps ...
}
```

Isso desacopla os steps das implementações concretas, facilitando a manutenção e os testes unitários da camada de steps (se necessário).

---

**Próximo Documento:** [05 - Detalhamento do Projeto Validator](05-VALIDATOR-PROJECT.md)
