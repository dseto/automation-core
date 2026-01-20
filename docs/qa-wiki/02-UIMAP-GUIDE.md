# 2. Guia UiMap: Mapeamento de Elementos e Anchor Pattern

O `ui-map.yaml` é o contrato de elementos da sua aplicação. Ele mapeia nomes amigáveis (friendly names) usados no Gherkin para os identificadores técnicos (`data-testid`) na aplicação.

## 📝 Estrutura Básica do UiMap

```yaml
pages:
  login:
    __meta:
      route: /login
      anchor: page.login.container
    username:
      testId: page.login.username
    password:
      testId: page.login.password
    submit:
      testId: page.login.submit
  
  dashboard:
    __meta:
      route: /dashboard
      anchor: page.dashboard.container
    welcome_message:
      testId: page.dashboard.welcome
```

### Regras de Ouro

1.  **Nome Amigável:** A chave do elemento (`username`, `submit`) é o nome que você usa no Gherkin.
2.  **`testId`:** O valor deve ser o `data-testid` exato que o desenvolvedor implementou na aplicação.
3.  **Padrão de Nomenclatura:** O `testId` deve seguir o padrão `page.[nome_pagina].[nome_elemento]`.

---

## ⚓ Anchor Pattern: Identificação de Páginas em SPAs (NOVO)

O **Anchor Pattern** resolve o problema de identificar uma página de forma única, mesmo quando a URL não muda (Single Page Applications - SPAs).

### O Problema

Em SPAs, modais, abas e renderização condicional, a URL pode ser a mesma, mas o conteúdo da tela é diferente.

| Cenário | URL | Conteúdo | Ambiguidade |
| :--- | :--- | :--- | :--- |
| Página de Cadastro | `/app/users` | Formulário de Cadastro | Sim |
| Modal de Edição | `/app/users` | Modal de Edição | Sim |

### A Solução: O Campo `anchor`

O campo `anchor` em `__meta` define um `data-testid` que **deve estar presente** na tela para que a plataforma considere que você está naquela página.

```yaml
pages:
  cadastro:
    __meta:
      route: /app/users
      anchor: page.cadastro.form
    ...
  
  modal_edicao:
    __meta:
      # Rota é a mesma, mas o Anchor é diferente
      route: /app/users
      anchor: modal.edicao.container
    ...
```

### Algoritmo de Identificação de Página

A plataforma usa a seguinte lógica para saber onde você está:

1.  **Verifica a Rota:** Se a URL atual corresponde à `route` da página.
2.  **Verifica o Anchor:** Se o elemento definido em `anchor` está visível na tela.

**Resultado:** A página só é considerada "ativa" se **ambas** as condições forem atendidas (se o campo `anchor` estiver preenchido).

---

## 📝 Regras de Uso do Anchor

| Cenário | `route` | `anchor` | Exemplo de Uso |
| :--- | :--- | :--- | :--- |
| **Página Simples** | `/login` | Opcional | `Dado que estou na página "login"` |
| **SPA com Modais** | `/app/users` | `page.users.list` | `Então estou na página "lista_usuarios"` |
| **Modal** | `/app/users` | `modal.user.edit` | `Então estou na página "modal_edicao_usuario"` |
| **Renderização Condicional** | `/app/settings` | `tab.security.container` | `Então estou na página "aba_seguranca"` |

### Validação em Build-Time

O `Automation.Validator` agora verifica as regras de Anchor:

*   **Páginas com a mesma `route`** **DEVEM** ter um `anchor` definido para evitar ambiguidade.
*   O `anchor` deve ser um `data-testid` único e visível.

**Ação do QA:** Se o Validator reclamar de ambiguidade de rota, adicione um `anchor` único para cada página/modal que compartilha a mesma URL.
