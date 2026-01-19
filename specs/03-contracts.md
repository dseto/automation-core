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

### Resolução em Runtime
O ElementResolver usa `PageContext.CurrentPageName` para localizar a página no UiMap. Busca o elemento pela chave (ex: "username") e retorna o CSS locator `[data-testid='page.login.username']`.

### Validação
O Validator verifica que todas as páginas têm pelo menos um elemento, que os testIds são únicos por página, e que as rotas são válidas.

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

### Resolução em Runtime
O DataResolver interpreta três tipos de referência. Literais (ex: "valor") retornam a string diretamente. Objetos (ex: "user_admin") retornam o dicionário completo. Tokens (ex: `{{cpfs_teste}}`) retornam o próximo item do dataset.

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
O Validator verifica que todas as páginas referenciadas existem no UiMap, que todos os elementos existem na página, que todas as chaves de dados existem no DataMap, e que não há referências circulares.
