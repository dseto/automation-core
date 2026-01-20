# 07 - Guia de Contribuição

Este guia destina-se a desenvolvedores que desejam contribuir para o código-fonte da Automation Platform.

---

## 🚀 Processo de Contribuição

O processo segue um fluxo padrão de GitFlow/GitHub Flow:

1.  **Criar uma Issue:** Antes de começar a trabalhar, crie uma issue no repositório descrevendo o bug a ser corrigido ou a feature a ser adicionada. Isso permite a discussão com a equipe de arquitetura.
2.  **Criar um Fork (se externo) ou Branch:**
    *   **Contribuidores Externos:** Façam um fork do repositório principal.
    *   **Contribuidores Internos:** Criem uma nova branch a partir da `main` ou `develop`, seguindo o padrão `feature/nome-da-feature` ou `fix/nome-do-bug`.
3.  **Desenvolver:** Implemente a sua alteração. Siga os padrões de código abaixo.
4.  **Testar Localmente:**
    *   Garanta que o projeto compila sem erros (`dotnet build`).
    *   Se você alterou a lógica do Core, crie ou atualize os testes unitários.
    *   Execute todos os testes existentes para garantir que nada foi quebrado (`dotnet test`).
5.  **Validar Contratos:** Se sua alteração afeta os contratos, execute o `Automation.Validator` para garantir que tudo está correto.
6.  **Criar um Pull Request (PR):**
    *   Faça o push da sua branch.
    *   Abra um Pull Request para a branch `main` ou `develop`.
    *   Na descrição do PR, **referencie a issue original** (ex: `Resolves #123`).
    *   Descreva claramente *o que* foi feito e *por que*.
7.  **Revisão de Código:** Pelo menos um outro desenvolvedor deve revisar o PR. O revisor deve focar em lógica, padrões de código e impacto na arquitetura.
8.  **Merge:** Após a aprovação, o PR é "squashed and merged" para manter um histórico de commits limpo na branch principal.

---

## 📝 Padrões de Código

*   **Linguagem:** O código é escrito em C# 12 e utiliza os recursos mais recentes do .NET 8.
*   **Estilo:** Siga as convenções de estilo padrão do .NET (PascalCase para métodos e propriedades, camelCase para variáveis locais).
*   **Nullability:** O projeto usa `Nullable Reference Types` (`<Nullable>enable</Nullable>`). Evite ao máximo o uso do operador `!` (null-forgiving). Valide os inputs.
*   **Async/Await:** Use `async/await` em toda a I/O (interações com WebDriver, leitura de arquivos). Não misture código síncrono e assíncrono (`.Result` ou `.Wait()` são proibidos).
*   **Comentários:** Comente o *porquê* do código, não *o que* ele faz. O código deve ser autoexplicativo.
    ```csharp
    // Ruim: Incrementa o índice
    _currentIndex++;

    // Bom: Reinicia o índice para permitir que o dataset seja reutilizado no mesmo cenário
    if (_currentIndex >= _items.Count) _currentIndex = 0;
    ```
*   **Injeção de Dependência:** Sempre prefira obter dependências através do construtor.

---

## ✅ Definição de "Pronto" (Definition of Done)

Uma feature ou correção só é considerada "pronta" quando:

-   [ ] O código foi implementado e segue os padrões.
-   [ ] Os testes unitários (se aplicável) foram criados e estão passando.
-   [ ] Todos os testes de regressão estão passando.
-   [ ] A documentação relevante (Spec Deck e Wiki de Desenvolvedor) foi atualizada.
-   [ ] O Pull Request foi revisado e aprovado.
-   [ ] O código foi merged na branch principal.

---

Obrigado por contribuir para tornar a Automation Platform ainda melhor! 🎉 Automation Platform ainda melhor! 🚀
