#language: pt-BR
@smoke
Funcionalidade: Login - Acesso à aplicação

  Contexto:
    Dado que a aplicação está em "${BASE_URL}"

  @positive
  Cenário: Login com credenciais válidas
    Dado que estou na página "login"
    Quando eu preencho os campos com os dados de "@user_admin"
    E eu clico em "submit" e aguardo a rota "/dashboard"
    Então estou na página "dashboard"
    E o elemento "stat-total" deve estar visível

  @negative
  Cenário: Login com credenciais inválidas
    Dado que estou na página "login"
    Quando eu preencho "username" com "usuario-invalido"
    E eu preencho "password" com "senha-invalida"
    E eu clico em "submit"
    Então o elemento "error" deve estar visível
    E a rota deve ser "/login"

  @interaction
  Cenário: Toggle de visibilidade de senha
    Dado que estou na página "login"
    Quando eu preencho "password" com "senha123"
    E eu clico em "toggle-password"
    Então o atributo "type" de "password" deve ser "text"
    Quando eu clico em "toggle-password"
    Então o atributo "type" de "password" deve ser "password"

  @interaction
  Cenário: Login com esperas explícitas
    Dado que estou na página "login"
    E eu aguardo 1 segundos
    Quando eu preencho "username" com "admin"
    E eu aguardo 1 segundos
    Quando eu preencho "password" com "ChangeMe123!"
    E eu aguardo 1 segundos
    E eu clico em "submit" e aguardo a rota "/dashboard"
    Então estou na página "dashboard"
    E o elemento "stat-total" deve estar visível
