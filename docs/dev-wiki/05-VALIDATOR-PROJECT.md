# 05 - Detalhamento do Projeto `Automation.Validator`

O `Automation.Validator` é uma ferramenta CLI crucial para a estratégia de **Shift-Left Testing** da plataforma. Ele permite a validação estática dos contratos, encontrando erros antes que um único teste seja executado.

---

## 🎯 Propósito e Casos de Uso

-   **Desenvolvedores Locais:** Antes de comitar, um desenvolvedor pode rodar o validador para garantir que suas alterações nos arquivos YAML ou Gherkin não quebraram nenhum contrato.
-   **Pipeline de CI/CD:** O validador é executado como um passo obrigatório no pipeline. Se a validação falhar, o build é interrompido, prevenindo a execução de testes fadados ao fracasso e economizando tempo de máquina.

---

## 🧩 Estrutura e Classes Principais

### `Program.cs`
O ponto de entrada da aplicação CLI. É responsável por:
-   Analisar os argumentos da linha de comando (ex: `validate`, `--ui-map`, etc.).
-   Invocar o comando correspondente.
-   Retornar um código de saída (`exit code`) apropriado (0 para sucesso, 1 para falha), que é essencial para a integração com pipelines de CI/CD.

### `Automation.Validator.Validators`
Este namespace contém a lógica de validação para cada tipo de artefato.

| Classe | Responsabilidade |
|---|---|
| `UiMapValidator.cs` | Valida o `ui-map.yaml`. Verifica a unicidade de `testId`s, a validade das rotas e, mais importante, a correta aplicação do **Anchor Pattern** (ex: se duas páginas compartilham a mesma rota, ambas devem ter um `anchor`). |
| `DataMapValidator.cs` | Valida o `data-map.yaml`. Verifica a existência de contextos, a unicidade de chaves e a correta sintaxe das referências da **Sintaxe Explícita** (ex: um valor não pode começar com `@` se não for uma referência válida). |
| `GherkinValidator.cs` | Valida os arquivos `.feature`. Faz a "ligação" entre os contratos, verificando se as páginas e elementos referenciados no Gherkin existem no `UiMap` e se as chaves de dados existem no `DataMap`. |

### `Automation.Validator.Services`

| Classe | Responsabilidade |
|---|---|
| `YamlLoader.cs` | Um serviço compartilhado para carregar e desserializar os arquivos YAML, reutilizando a lógica já presente no `Automation.Core`. |
| `ReportService.cs` | Formata os resultados da validação para exibição no console ou para saída em formato JSON (usando a flag `--json`). |

---

## ⚙️ Exemplo de Fluxo de Validação

Quando o comando `automation-validator validate --ui-map ui.yaml --data-map data.yaml --features features/` é executado:

1.  **`Program.cs`** parseia os argumentos e invoca o `ValidateCommand`.
2.  O `ValidateCommand` instancia os três validadores (`UiMapValidator`, `DataMapValidator`, `GherkinValidator`).
3.  Ele usa o `YamlLoader` para carregar os arquivos `ui.yaml` e `data.yaml` em memória.
4.  **`UiMapValidator.Validate()`** é chamado, executando todas as checagens de UiMap (incluindo as regras do Anchor Pattern).
5.  **`DataMapValidator.Validate()`** é chamado, executando as checagens de DataMap (incluindo as regras da Sintaxe Explícita).
6.  **`GherkinValidator.Validate()`** é chamado. Ele recebe os modelos de UiMap e DataMap já carregados e parseia todos os arquivos `.feature` para validar as referências cruzadas.
7.  Todos os erros e avisos são coletados.
8.  O `ReportService` formata a saída para o console.
9.  Se houver algum erro, `Program.cs` retorna o código de saída `1`, falhando o passo no pipeline de CI/CD.

---

**Próximo Documento:** [06 - Guia de Extensão](06-EXTENSION-GUIDE.md)
