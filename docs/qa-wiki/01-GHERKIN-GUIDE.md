# 1. Guia Gherkin: Escrevendo Cenários em PT-BR

O Gherkin é a linguagem de domínio específico (DSL) que usamos para descrever o comportamento do software.

## 📝 Estrutura Básica

Todo arquivo de cenário deve seguir a estrutura:

```gherkin
#language: pt-BR
@tag1 @tag2
Funcionalidade: [Nome da Funcionalidade] - [Breve Descrição]

  Contexto:
    Dado que [condição inicial comum a todos os cenários]

  Cenário: [Nome do Cenário]
    Dado que [condição inicial]
    Quando eu [ação do usuário]
    Então [resultado esperado]
    E [outro resultado esperado]
```

### Palavras-Chave Obrigatórias (PT-BR)

| Palavra-Chave | Uso |
| :--- | :--- |
| `Funcionalidade` | Define o escopo do arquivo (ex: Login, Cadastro). |
| `Contexto` | Define os passos de *setup* que se aplicam a todos os cenários. |
| `Cenário` | Define um caso de teste específico. |
| `Dado` | Condição inicial (o que já existe). |
| `Quando` | Ação do usuário (o que o usuário faz). |
| `Então` | Resultado esperado (o que o sistema deve fazer). |
| `E` | Continuação de `Dado`, `Quando` ou `Então`. |

---

## 🏷️ Tags de Cenário

As tags são usadas para organizar e filtrar a execução dos testes.

| Tag | Uso | Exemplo |
| :--- | :--- | :--- |
| `@smoke` | Testes críticos que validam a funcionalidade básica. | `dotnet test --filter Category=smoke` |
| `@regressao` | Testes de regressão mais detalhados. | `dotnet test --filter Category=regressao` |
| `@negativo` | Cenários que testam o comportamento esperado em caso de erro. | `@negativo @login` |
| `@funcionalidade` | Nome da funcionalidade (ex: `@login`, `@cadastro`). | `@login` |

**Regra:** Sempre use pelo menos uma tag de escopo (`@smoke`, `@regressao`) e uma tag de funcionalidade.

---

## 💡 Sintaxe Explícita do DataResolver (NOVO)

Para eliminar a ambiguidade entre valores literais e referências de dados, agora usamos **prefixos obrigatórios** para referências.

| Tipo de Dado | Prefixo | Exemplo Gherkin | O que Resolve |
| :--- | :--- | :--- | :--- |
| **Valor Literal** | Nenhum | `"admin"` | String `"admin"` |
| **Referência de Objeto** | `@` | `"@user_admin"` | Objeto completo do DataMap |
| **Referência de Dataset** | `{{...}}` | `"{{cpfs_teste}}"` | Próximo item do Dataset |
| **Variável de Ambiente** | `${...}` | `"${BASE_URL}"` | Valor da variável de ambiente |

### Exemplos Práticos

| Intenção | Gherkin (Correto) | Gherkin (Incorreto - Ambíguo) |
| :--- | :--- | :--- |
| Usar dados de um objeto | `Quando eu preencho os campos com os dados de "@user_admin"` | `Quando eu preencho os campos com os dados de "user_admin"` |
| Preencher com a string "admin" | `Quando eu preencho "username" com "admin"` | N/A (sempre literal) |
| Preencher com o email | `Quando eu preencho "email" com "user@corp.com"` | N/A (sempre literal) |
| Usar um CPF do dataset | `Quando eu preencho "cpf" com "{{cpfs_teste}}"` | N/A (dataset já usava `{{}}`) |

**Regra:** Se você quer que a plataforma **busque** algo no DataMap, **USE O PREFIXO `@`**. Caso contrário, será tratado como um valor literal.

---

## 📚 Steps Genéricos Mais Usados

| Step | Uso | Exemplo |
| :--- | :--- | :--- |
| `Dado que estou na página "[nome_pagina]"` | Define o contexto da página. | `Dado que estou na página "login"` |
| `Quando eu preencho "[elemento]" com "[valor]"` | Preenche um campo. | `Quando eu preencho "username" com "@user_admin"` |
| `Quando eu clico em "[elemento]"` | Clica em um botão ou link. | `Quando eu clico em "submit"` |
| `Quando eu preencho os campos com os dados de "[@objeto]"` | Preenche múltiplos campos com um objeto do DataMap. | `Quando eu preencho os campos com os dados de "@user_admin"` |
| `Então estou na página "[nome_pagina]"` | Verifica se a página atual é a esperada (usa Anchor se necessário). | `Então estou na página "dashboard"` |
| `Então o campo "[elemento]" deve conter "[valor]"` | Valida o conteúdo de um campo. | `Então o campo "username" deve conter "admin"` |
| `Então o elemento "[elemento]" deve estar visível` | Valida a visibilidade de um elemento. | `Então o elemento "modal_sucesso" deve estar visível` |

**Regra:** Sempre use os steps genéricos do Core. Evite criar steps customizados.
