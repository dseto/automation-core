#language: pt-BR
@smoke @navigation
Funcionalidade: Navegação - Validação do Anchor Pattern e Rotas

  Contexto:
    Dado que a aplicação está em "${BASE_URL}"

  @anchor
  Cenário: Validar navegação para dashboard após login
    Dado que estou na página "login"
    Quando eu preencho os campos com os dados de "@user_admin"
    E eu clico em "submit" e aguardo a rota "/dashboard"
    Então estou na página "dashboard"
    E a rota deve ser "/dashboard"

  @anchor
  Cenário: Validar elementos do dashboard após login
    Dado que estou na página "login"
    Quando eu preencho os campos com os dados de "@user_admin"
    E eu clico em "submit" e aguardo a rota "/dashboard"
    Então estou na página "dashboard"
    E o elemento "stat-total" deve estar visível
    E o elemento "stat-active" deve estar visível
    E o elemento "stat-draft" deve estar visível
    E o elemento "stat-disabled" deve estar visível
