
# Requisitos Funcionais — FREE-HANDS Recorder (Session Log)

Este documento é a **fonte normativa** do Recorder Session Log.
O runtime DEVE implementar exatamente o comportamento aqui descrito.

---

## RF00 — Modo exploratório (pré-condição estrutural)

**Descrição:**  
Quando `AUTOMATION_RECORD=true`, o sistema DEVE operar em **modo exploratório**:
- abrir o browser
- permitir o usuário interagir manualmente livremente com a aplicação
- gravar eventos (RF01–RF06)
- encerrar e gerar `session.json`

**Regras mandatórias:**
- O runtime DEVE iniciar e operar **mesmo se não existir nenhum arquivo `.feature`** no repositório.
- O runtime NÃO DEVE depender de:
  - carregamento de features/scenarios
  - execução de steps (Reqnroll/Gherkin)
  - ciclo de vida de testes (setup/teardown de cenário)
- Durante o modo exploratório, o framework NÃO “passa/falha” testes. Ele apenas grava.

**Critérios de aceite:**
- É possível executar um comando de gravação com `AUTOMATION_RECORD=true` em um workspace sem `.feature`, e o browser abre normalmente.
- Após o usuário encerrar a execução (fechar o browser ou comando de stop), um `session.json` é escrito em `RECORD_OUTPUT_DIR`.

---

## Regra Global — Eventos de baixo sinal
Eventos de baixo sinal (ex.: `mousemove`, `scroll`, `hover`, `keydown` isolado) DEVEM ser ignorados.

---

## Regra Global — Raw Action (JS executado)
Sempre que o runtime executar uma ação via JavaScript (ex.: `executeScript(...)`) para realizar uma interação,
o Recorder DEVE registrar `rawAction.kind="js"` e `rawAction.script="<script executado>"`.

---

## RF01 — Início e fim automático da sessão
Quando `AUTOMATION_RECORD=true`, iniciar a gravação antes da primeira interação relevante e encerrar ao final.

## RF02 — Navegação
Toda mudança de rota/página relevante gera `navigate` com `route`.

## RF03 — Click
Clique em elemento interativo gera `click` com `target`. Se realizado por JS, preencher `rawAction`.

## RF04 — Fill (consolidado)
Preenchimento consolidado por campo gera `fill` com `target` e `value`. Se realizado por JS, preencher `rawAction`.

## RF05 — Select / Toggle / Submit
Interações semânticas geram `select`/`toggle`/`submit`. Se realizado por JS, preencher `rawAction`.

## RF06 — Modal open / close
Quando detectável, gera `modal_open`/`modal_close`.
