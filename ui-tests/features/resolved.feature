#language: pt

Funcionalidade: Fluxo de login (draft)

  Contexto:
    Dado que a aplicação está em "${BASE_URL}"

  @positive
  Cenário: cenariosegurosim
    Dado que estou na página "login"
    E eu preencho "user" com "admin"
    E eu preencho "pass" com "admin"
    E eu clico em "submit" e aguardo a rota "/dashboard"
    Então estou na página "dashboard"
    E o elemento "toast" deve estar visível