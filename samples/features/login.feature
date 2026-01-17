@smoke
Funcionalidade: Login

Cenário: Login com sucesso
  Dado que estou na tela "LoginPage"
  Quando eu preencho o campo "Usuario" com "admin"
  E eu preencho o campo "Senha" com "123"
  E eu clico no botão "Entrar"
  Então a rota deve ser "/home"
  Dado que estou na tela "DashboardPage"
  E o elemento "TituloBoasVindas" deve estar visível
