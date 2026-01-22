
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

## RF02 — Navegação (Normativo)

### Intenção
Garantir que toda navegação capturada pelo Recorder produza **rotas determinísticas e explícitas** no `session.json`,
evitando qualquer inferência/“adivinhação” posterior no Draft Generator e no Semantic Resolver.

### Regras objetivas
1) Todo evento com `"type": "navigate"` **DEVE** incluir:
   - `url` (string): URL absoluta observada pelo browser (ex.: `http://localhost/app.html#/dashboard`).
   - `pathname` (string): caminho do documento (ex.: `/app.html`).
   - `fragment` (string|null): fragmento (inclui `#`), ou `null` quando não houver (ex.: `#/dashboard`).
   - `route` (string): **forma canônica determinística** = `pathname` + (`fragment` se existir).
     - Ex.: `pathname=/app.html` e `fragment=#/dashboard` → `route=/app.html#/dashboard`
     - Ex.: `pathname=/login.html` e `fragment=null` → `route=/login.html`

2) Normalização obrigatória (sem heurística):
   - `route` **DEVE** começar com `/`.
   - `route` **NÃO PODE** conter prefixos locais/FS (ex.: `file:///`, `C:\`, `/Users/...`).
   - `route` **NÃO PODE** conter whitespace (inclui `\r`, `\n`, `\t`). Caso a fonte contenha whitespace, o Recorder **DEVE** colapsar para um único espaço e então remover espaços à esquerda/direita (trim).

3) Compatibilidade:
   - O Recorder **PODE** continuar emitindo `"route"` para consumidores antigos; porém, após este delta, o contrato normativo passa a exigir também `url`, `pathname` e `fragment` para `navigate`.

### Exemplo normativo (entrada → saída)

**Entrada observada (runtime/browser)**
- `window.location.href = "http://localhost/insurance-quote-spa-static/app.html#/dashboard"`
- `window.location.pathname = "/insurance-quote-spa-static/app.html"` *(exemplo quando o app está em subpath)*
- `window.location.hash = "#/dashboard"`

**Evento `navigate` (session.json)**
```json
{
  "t": "00:00.000",
  "type": "navigate",
  "url": "http://localhost/insurance-quote-spa-static/app.html#/dashboard",
  "pathname": "/insurance-quote-spa-static/app.html",
  "fragment": "#/dashboard",
  "route": "/insurance-quote-spa-static/app.html#/dashboard"
}
```

### Critérios de aceite
- [ ] Para qualquer `navigate`, `route == pathname + fragment` (quando `fragment` != null) ou `route == pathname` (quando `fragment` == null).
- [ ] `route` não contém `file:///` nem caminhos locais e não possui `\r`/`\n`.
- [ ] O `session.json` gerado valida contra `specs/api/schemas/recorder.session.schema.json`.

### Anti-exemplo (proibido)
- `route: "file:///C:/Projetos/app.html#/dashboard"` (prefixo local/FS).
- `route: "/login.html/login"` (concatenado por inferência).
- `route` contendo `\n` (quebra o parser Gherkin quando transcrito).
## RF03 — Click
Clique em elemento interativo gera `click` com `target`. Se realizado por JS, preencher `rawAction`.

## RF04 — Fill (consolidado)
Preenchimento consolidado por campo gera `fill` com `target` e `value`. Se realizado por JS, preencher `rawAction`.

## RF05 — Select / Toggle / Submit
Interações semânticas geram `select`/`toggle`/`submit`. Se realizado por JS, preencher `rawAction`.

## RF06 — Modal open / close
Quando detectável, gera `modal_open`/`modal_close`.
## RF07 — Registrar espera entre eventos no `session.json` (Normativo)

### Intenção
Preservar o tempo real de inatividade entre eventos capturados, para permitir geração de espera explícita no draft e execução determinística.

### Regras objetivas
1. Para cada evento `events[i]` (i > 0), o Recorder DEVE calcular `gapMs = t(i) - t(i-1)` usando a mesma base temporal já usada em `t`.
2. Deve existir o parâmetro configurável `RECORD_WAIT_LOG_THRESHOLD_SECONDS` (float) com default **1.0**.
3. Defina `thresholdMs = RECORD_WAIT_LOG_THRESHOLD_SECONDS * 1000`.
4. Se `gapMs > thresholdMs`, o Recorder DEVE incluir no evento atual: `waitMs = gapMs` (inteiro em milissegundos, >= 0).
5. Se `gapMs <= thresholdMs`, o campo `waitMs` NÃO DEVE ser emitido.
6. `waitMs` é opcional e consumidores que não suportarem DEVEM ignorar o campo.

### Exemplo normativo
```json
{
  "t": "00:02.100",
  "type": "fill",
  "waitMs": 2100,
  "target": { "hint": "username" },
  "value": { "literal": "admin" }
}
```

### Critérios de aceite
- Com `RECORD_WAIT_LOG_THRESHOLD_SECONDS=1.0`, um gap de 2100ms DEVE gerar `waitMs: 2100`.
- Um gap de 900ms NÃO DEVE gerar `waitMs`.

### Anti-exemplo
- ❌ Emitir `waitMs` em todos os eventos (gera ruído e drift no draft).
- ❌ Emitir `waitSeconds` como string sem contrato (o contrato é `waitMs` inteiro).
