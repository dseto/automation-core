#language: pt

Funcionalidade: Fluxo de login (draft)

Cenário: Cenário draft gerado pelo Recorder

  Dado que estou na página "/login"
  E eu espero 2.1 segundos
  Quando eu preencho "username" com "admin"
  E eu preencho "password" com "***"
  # TODO: revisar ação não inferida
  # RAW: {"type":"unknown","at":"2026-01-22T00:00:00Z","target":{"hint":"[aria-label='toggle password']"},"rawAction":{"script":"document.querySelector(\"[aria-label='toggle password']\").click();"}}
  E eu clico em "submit"
