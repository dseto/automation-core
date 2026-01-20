#language: pt-BR
Funcionalidade: Validação do Contrato de Dados (DataMap)

  Contexto:
    Dado que a aplicação está em "${BASE_URL}"

  Cenário: Login usando objeto de dados do DataMap
    Dado que estou na página "login"
    Quando eu preencho os campos com os dados de "@user_admin"
    E eu clico em "submit" e aguardo a rota "/dashboard"
    Então estou na página "dashboard"

  Cenário: Login com dados inválidos usando DataMap
    Dado que estou na página "login"
    Quando eu preencho os campos com os dados de "@invalid_user"
    E eu clico em "submit"
    Então o elemento "error" deve estar visível

  Cenário: Uso de coleções (DataSets) sequenciais
    Dado que estou na página "login"
    Quando eu preencho "username" com "{{cpfs_teste}}"
    E eu limpo o campo "username"
    E eu preencho "username" com "{{cpfs_teste}}"
    # O segundo preenchimento deve usar o próximo item da lista
