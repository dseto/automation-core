# 4. Guia Validator: Validação Pré-Execução (Shift-Left)

A ferramenta de linha de comando `Automation.Validator` permite que você detecte erros de contrato e mapeamento **antes** de executar seus testes. Isso é o princípio **Shift-Left** em ação, economizando tempo de debug.

## 🛡️ Por que Validar?

| Problema | Antes do Validator | Com o Validator |
| :--- | :--- | :--- |
| Elemento não mapeado | Teste falha em runtime (demorado) | Validator avisa em build-time (rápido) |
| Referência de dado errada | Teste falha em runtime (demorado) | Validator avisa em build-time (rápido) |
| Ambiguidade de página (SPA) | Teste falha em runtime (demorado) | Validator avisa em build-time (rápido) |

---

## 💻 Comandos Principais

### 1. Validar UiMap (Contrato de Elementos)

Este comando verifica se o seu `ui-map.yaml` está bem formado e segue as regras de Anchor.

```bash
dotnet run --project Automation.Validator -- validate --ui-map [caminho/para/seu/ui-map.yaml]
```

**O que ele verifica:**
*   **Sintaxe YAML:** Se o arquivo é um YAML válido.
*   **Unicidade:** Se todos os `testId` são únicos por página.
*   **Regras de Anchor:**
    *   Se páginas com a mesma `route` têm `anchor` definido.
    *   Se o `anchor` é um `data-testid` válido.

### 2. Validar DataMap (Contrato de Dados)

Este comando verifica se o seu `data-map.yaml` está bem formado e segue as regras de Sintaxe Explícita.

```bash
dotnet run --project Automation.Validator -- validate --data-map [caminho/para/seu/data-map.yaml]
```

**O que ele verifica:**
*   **Sintaxe YAML:** Se o arquivo é um YAML válido.
*   **Contextos:** Se o contexto `default` existe.
*   **Datasets:** Se as estratégias (`sequential`, `random`, `unique`) são válidas.
*   **Nomes de Objeto:** Se os nomes de objeto não contêm prefixos de referência (`@`, `{{`, `${}`).

### 3. Validar Gherkin (Contrato de Uso)

Este comando é o mais poderoso, pois verifica se as referências no seu arquivo `.feature` existem nos seus arquivos de contrato.

```bash
dotnet run --project Automation.Validator validate --gherkin [caminho/para/seu/cenario.feature] \
    --ui-map [caminho/para/ui-map.yaml] \
    --data-map [caminho/para/data-map.yaml]
```

**O que ele verifica:**
*   **Páginas:** Se todas as páginas referenciadas (`Dado que estou na página "login"`) existem no UiMap.
*   **Elementos:** Se todos os elementos referenciados (`Quando eu preencho "username"`) existem na página.
*   **Referências de Objeto (`@`):** Se o objeto referenciado (`@user_admin`) existe no DataMap.
*   **Referências de Dataset (`{{}}`):** Se o dataset referenciado (`{{cpfs_teste}}`) existe no DataMap.
*   **Sintaxe Explícita:** Se a sintaxe de prefixos (`@`, `{{}}`, `${}`) está correta.

---

## 🚨 Exemplos de Erro e Solução

| Erro do Validator | Causa Provável | Solução |
| :--- | :--- | :--- |
| `Página 'cadastro' não encontrada no UiMap` | Erro de digitação no Gherkin ou página não mapeada. | Corrigir o nome da página no `.feature` ou mapear no `ui-map.yaml`. |
| `Elemento 'submit' não encontrado na página 'login'` | Elemento não mapeado na página `login`. | Adicionar o elemento `submit` na seção `login` do `ui-map.yaml`. |
| `Ambiguidade de rota: '/app/settings' é usada por 'aba_seguranca' e 'aba_perfil'` | Páginas compartilham a mesma URL. | Adicionar um `anchor` único para cada página no `ui-map.yaml`. |
| `Objeto '@user_admin' não encontrado no DataMap` | Objeto não existe ou está no contexto errado. | Verificar o `data-map.yaml` e o contexto de execução. |
| `Dataset '{{cpfs_teste}}' não encontrado` | Dataset não existe. | Adicionar o dataset `cpfs_teste` na seção `datasets` do `data-map.yaml`. |
| `Prefixo '@' deve estar no início da string` | Tentativa de usar `@` no meio de um valor. | Corrigir a sintaxe no `.feature` ou usar um valor literal. |

**Regra:** Use o Validator como seu primeiro passo de debug. Se o Validator passar, o problema é de execução (runtime), não de contrato (build-time).
