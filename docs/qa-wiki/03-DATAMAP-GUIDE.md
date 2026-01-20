# 3. Guia DataMap: Gerenciamento de Dados de Teste

O `data-map.yaml` é o seu repositório de dados de teste. Ele garante que os dados sejam centralizados, reutilizáveis e fáceis de manter.

## 📝 Estrutura Básica do DataMap

```yaml
contexts:
  default:
    user_admin:
      username: "admin"
      password: "ChangeMe123!"
  
  homolog:
    user_admin:
      username: "admin_hml"
      password: "SenhaHml123!"

datasets:
  cpfs_teste:
    strategy: sequential
    items:
      - "123.456.789-00"
      - "987.654.321-11"
```

---

## 💡 Sintaxe Explícita do DataResolver (CRÍTICO)

A nova sintaxe elimina a ambiguidade entre valores literais e referências de dados. **Você deve usar o prefixo correto para que a plataforma saiba onde buscar o valor.**

### 1. Referência de Objeto (`@`)

Use o prefixo `@` para referenciar um objeto completo do DataMap.

| Tipo | Sintaxe | Exemplo Gherkin | Resultado |
| :--- | :--- | :--- | :--- |
| **Objeto** | `@nome_do_objeto` | `Quando eu preencho os campos com os dados de "@user_admin"` | Retorna o dicionário completo (`{username: "admin", ...}`) |

**Regra:** O prefixo `@` é obrigatório para buscar um objeto no `contexts`.

### 2. Referência de Dataset (`{{...}}`)

Use a sintaxe `{{...}}` para referenciar um Dataset. A plataforma retornará o próximo item da lista, seguindo a `strategy` definida.

| Tipo | Sintaxe | Exemplo Gherkin | Resultado |
| :--- | :--- | :--- | :--- |
| **Dataset** | `{{nome_do_dataset}}` | `Quando eu preencho "cpf" com "{{cpfs_teste}}"` | Retorna o próximo item do `items` (ex: "123.456.789-00") |

### 3. Variável de Ambiente (`${...}`)

Use a sintaxe `${...}` para referenciar variáveis de ambiente do sistema operacional.

| Tipo | Sintaxe | Exemplo Gherkin | Resultado |
| :--- | :--- | :--- | :--- |
| **Env Var** | `${NOME_VARIAVEL}` | `Dado que a aplicação está em "${BASE_URL}"` | Retorna o valor da variável `BASE_URL` |

### 4. Valor Literal (Sem Prefixo)

Qualquer valor que **não** comece com `@`, `{{` ou `${` é tratado como uma string literal.

| Tipo | Sintaxe | Exemplo Gherkin | Resultado |
| :--- | :--- | :--- | :--- |
| **Literal** | `valor_qualquer` | `Quando eu preencho "username" com "user_admin"` | Retorna a string `"user_admin"` |

**Cuidado:** Se você quer usar o objeto `user_admin`, mas esquece o `@`, a plataforma preencherá o campo com a string literal `"user_admin"`, e não com os dados do objeto.

---

## 🌍 Contextos

Contextos permitem que você use o mesmo nome de objeto (`user_admin`) com valores diferentes, dependendo do ambiente de execução.

| Contexto | Uso |
| :--- | :--- |
| `default` | Usado por padrão. Deve conter dados válidos para a maioria dos testes. |
| `homolog` | Usado quando o teste é executado no ambiente de homologação. |
| `production` | Usado para testes de smoke ou monitoramento em produção (com cautela). |

**Regra:** O nome do objeto deve ser o mesmo em todos os contextos para que o Gherkin não precise mudar.

---

## 🔢 Datasets e Estratégias

Datasets são listas de valores para testes parametrizados.

| Strategy | Descrição | Uso |
| :--- | :--- | :--- |
| `sequential` | Retorna os itens na ordem, reiniciando ao final. | Padrão, ideal para a maioria dos casos. |
| `random` | Retorna um item aleatório da lista. | Testes de carga ou amostragem. |
| `unique` | Retorna um item aleatório, garantindo que não se repita no mesmo cenário. | Testes de criação de usuário único. |

**Regra:** Se você usar um dataset, certifique-se de que o número de chamadas `{{dataset}}` no cenário não exceda o número de itens, a menos que a estratégia permita.
