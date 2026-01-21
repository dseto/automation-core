# API & Contracts

> Base: `_legacy/03-contracts.md` + código.  
> Nota: este produto é principalmente uma **plataforma de automação**; contratos aqui incluem formatos de arquivos e APIs internas/CLI.

## Contratos suportados (formatos)

### ui-map.yaml (UI contract)
- Define pages/modals, `__meta` e elementos.
- Regras de resolução: ver `backend/rules/element-resolution.md`.

### data-map.yaml (data contract)
- Define `contexts` e `datasets`.
- Regras de resolução: ver `backend/rules/data-resolution.md`.

### Gherkin (.feature) (behaviour contract)
- Steps válidos e suas assinaturas: ver `tests/gherkin/step-catalog.md`.

### Run Settings (execution contract)
- Variáveis de ambiente e defaults: ver `backend/implementation/run-settings.md`.

## Conteúdo legado (referência)
# 03 - Contratos: UiMap, DataMap e Gherkin

## Princípio Fundamental
Contratos são a interface entre teste e aplicação. Devem ser versionados, validados e tratados como código. Eles definem o "contrato" que a aplicação deve cumprir.

## UiMap Contract (ui-map.yaml)

### Estrutura
```yaml
pages:
  login:
    __meta:
      route: /login
      anchor: page.login
    username:
      testId: page.login.username
    password:
      testId: page.login.password
    submit:
      testId: page.login.submit
```

### Semântica
A chave de página (ex: "login") deve corresponder à rota (ex: "/login"). Cada elemento possui um testId que segue o padrão `page.{pageName}.{elementName}`. O campo `__meta` contém metadados da página (rota, anchor para validação).

**Anchor Pattern (Novo):**
O campo `anchor` identifica um elemento único que confirma que a página foi carregada. Essencial em SPAs onde a URL não muda. Exemplo: `anchor: page.login` busca `[data-testid='page.login']` para validar que a página está presente.

**Quando usar Anchor:**
- SPAs (Single Page Applications) com navegação sem mudança de URL
- Modais e diálogos que abrem sem navegação
- Renderização condicional de páginas
- Qualquer página onde a URL não é suficiente para identificação

### Resolução em Runtime
O ElementResolver usa `PageContext.CurrentPageName` para localizar a página no UiMap. Busca o elemento pela chave (ex: "username") e retorna o CSS locator `[data-testid='page.login.username']`.

**Validação de Anchor em Runtime:**
Ao navegar para uma página com `anchor` definido, o PageContext aguarda o elemento anchor estar visível (WaitPageAnchor). Isso garante que a página foi realmente carregada antes de interagir com elementos.

### Validação
O Validator verifica que:
- Todas as páginas têm pelo menos um elemento
- Os testIds são únicos por página
- As rotas são válidas
- **Novo:** Páginas com rotas ambíguas têm `anchor` definido
- **Novo:** Não há conflito entre páginas (mesma rota sem anchor)

## DataMap Contract (data-map.yaml)

### Estrutura
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

### Contextos
Contextos representam diferentes ambientes ou perfis de usuário. O contexto "default" é sempre usado. Contextos adicionais (homolog, prod) são selecionados via `RunSettings.Environment`. Cada contexto contém objetos de dados (ex: user_admin) com propriedades (username, password).

### Datasets
Datasets são coleções de valores. A estratégia "sequential" itera na ordem. "random" seleciona aleatoriamente. "unique" garante sem repetição. Tokens no Gherkin (ex: `{{cpfs_teste}}`) resolvem para o próximo item do dataset.

### Resolução em Runtime (Sintaxe Explícita)
O DataResolver interpreta **quatro tipos de referência com prefixos determinísticos:**

| Tipo | Prefixo | Exemplo | Resolução |
|------|---------|---------|----------|
| **Objeto** | `@` | `@user_admin` | Busca no DataMap (contexto atual) |
| **Dataset** | `{{...}}` | `{{cpfs_teste}}` | Próximo item do dataset |
| **Variável de Ambiente** | `${...}` | `${BASE_URL}` | Variável do SO ou RunSettings |
| **Literal** | Nenhum | `user_admin` | String diretamente (sem resolução) |

**Benefícios da Sintaxe Explícita:**
- Elimina ambiguidade entre literal e referência
- Determinístico: ordem de verificação é fixa
- Validável em build-time (shift-left)
- Sem surpresas em runtime

**Exemplos:**
```yaml
user_admin: "admin"              # Literal: string "admin"
@user_admin: ...                  # Erro: @ é prefixo, não valor
username: "@user_admin"           # Literal: string "@user_admin"
username: @user_admin             # Referência: busca objeto user_admin
```

## Gherkin em PT-BR

### Padrão
```gherkin
#language: pt-BR
@smoke @positive
Funcionalidade: Login - Acesso à aplicação

  Contexto:
    Dado que a aplicação está em "${BASE_URL}"

  Cenário: Login com credenciais válidas
    Dado que estou na página "login"
    Quando eu preencho os campos com os dados de "user_admin"
    E eu clico em "submit" e aguardo a rota "/dashboard"
    Então estou na página "dashboard"
```

### Regras
Tags (`@smoke`, `@regressao`) organizam os testes. O Contexto define setup comum. Passos devem usar steps genéricos do Core (não criar novos). Variáveis de ambiente (ex: `${BASE_URL}`) são resolvidas pelo Core.

### Validação
O Validator verifica que:
- Todas as páginas referenciadas existem no UiMap
- Todos os elementos existem na página
- Todas as chaves de dados existem no DataMap
- Não há referências circulares
- **Novo:** Todos os prefixos de sintaxe explícita (@, {{}}, ${}) são válidos
- **Novo:** Não há ambiguidade entre literais e referências
