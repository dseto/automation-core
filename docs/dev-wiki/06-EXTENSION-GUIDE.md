# 06 - Guia de Extensão

Este é um dos documentos mais importantes para desenvolvedores. Ele descreve como estender a plataforma com novas funcionalidades sem precisar alterar o código do `Core`.

---

## 🎯 Filosofia de Extensão

A plataforma foi projetada para ser extensível através de **composição e injeção de dependência**. A regra de ouro é: **"Aberto para extensão, fechado para modificação."**

Antes de modificar uma classe do `Core`, considere se a funcionalidade pode ser adicionada como um novo serviço ou um novo step.

---

##  сценарий 1: Adicionar um Novo Step Genérico

Este é o cenário de extensão mais comum.

**Exemplo:** Adicionar um step `Quando eu dou um duplo clique em {elementName}`.

1.  **Criar o Método de Serviço no `Core`:**
    *   Abra o `InteractionService.cs` no `Automation.Core`.
    *   Adicione um novo método que encapsula a lógica do WebDriver:
        ```csharp
        public async Task DoubleClickAsync(string cssSelector)
        {
            var element = _waitService.WaitForElementVisible(By.CssSelector(cssSelector));
            var actions = new Actions(_driver);
            actions.DoubleClick(element).Perform();
            await Task.CompletedTask; // Para manter o padrão async
        }
        ```

2.  **Adicionar a Interface (se necessário):**
    *   Se o `InteractionService` tiver uma interface (`IInteractionService`), adicione a assinatura do novo método a ela.

3.  **Criar o Step no `Reqnroll`:**
    *   Abra o `InteractionSteps.cs` no `Automation.Reqnroll`.
    *   Adicione um novo método de step que chama o serviço recém-criado:
        ```csharp
        [Quando(@"eu dou um duplo clique em \"(.*?)\"")]
        public async Task QuandoEuDouDuploCliqueEm(string elementRef)
        {
            var selector = _elementResolver.Resolve(elementRef);
            await _interactionService.DoubleClickAsync(selector);
        }
        ```

4.  **Documentar:**
    *   Adicione o novo step ao `05-step-catalog.md` no Spec Deck para que os QAs saibam que ele existe.

---

## сценарий 2: Adicionar uma Nova Estratégia de Dataset

**Exemplo:** Adicionar uma estratégia `shuffled` que embaralha a lista uma vez e depois a consome sequencialmente.

1.  **Criar a Classe de Estratégia:**
    *   No `Automation.Core`, crie uma nova classe que implementa uma interface comum (ex: `IDatasetStrategy`).
        ```csharp
        public class ShuffledStrategy : IDatasetStrategy
        {
            private List<string> _shuffledItems;
            private int _currentIndex = 0;

            public ShuffledStrategy(IEnumerable<string> items)
            {
                _shuffledItems = items.OrderBy(x => Guid.NewGuid()).ToList();
            }

            public string GetNext()
            {
                if (_currentIndex >= _shuffledItems.Count) _currentIndex = 0; // Reinicia
                return _shuffledItems[_currentIndex++];
            }
        }
        ```

2.  **Registrar a Estratégia na Fábrica:**
    *   Encontre a fábrica de estratégias (ex: `DatasetStrategyFactory.cs`).
    *   Adicione um novo `case` no `switch` para reconhecer a string `"shuffled"` e retornar uma instância da sua nova classe.
        ```csharp
        switch (strategyName.ToLower())
        {
            case "sequential": return new SequentialStrategy(items);
            case "random": return new RandomStrategy(items);
            case "shuffled": return new ShuffledStrategy(items); // <-- Novo
            default: throw new NotSupportedException(...);
        }
        ```

3.  **Atualizar o `DataMapValidator`:**
    *   Adicione `"shuffled"` à lista de estratégias válidas no `DataMapValidator.cs` para que o `Automation.Validator` não reporte um erro.

---

## сценарий 3: Adicionar um Novo Tipo de Resolução no `DataResolver`

**Atenção:** Este é um cenário avançado e deve ser feito com muito cuidado, pois afeta o coração da plataforma.

**Exemplo:** Adicionar um prefixo `#!` para executar um cálculo matemático simples.

1.  **Alterar o `DataResolver.cs`:**
    *   Adicione uma nova condição `if` na ordem de resolução determinística. A posição importa!
        ```csharp
        public object Resolve(string reference)
        {
            // ... outras resoluções ...

            // Novo: Tenta resolver como cálculo
            if (reference.StartsWith("#!"))
            {
                var expression = reference.Substring(2);
                // Lógica para calcular a expressão (ex: usando NCalc ou DataTable.Compute)
                return new DataTable().Compute(expression, null);
            }

            // ... resto da lógica ...
        }
        ```

2.  **Atualizar o `DataMapValidator`:**
    *   Adicione a lógica para validar a sintaxe da expressão matemática durante a validação estática.

3.  **Documentar Extensivamente:**
    *   Atualize o `03-contracts.md` e o `04-runtime-resolution.md` no Spec Deck para refletir o novo prefixo, sua sintaxe e comportamento.

---

**Próximo Documento:** [07 - Guia de Contribuição](07-CONTRIBUTION-GUIDE.md)
