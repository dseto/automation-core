
# Requisitos — FREE-HANDS Recorder — Browser Capture Layer (MVP)

Este delta habilita o comportamento esperado do RF00 (modo exploratório) capturando interações manuais diretamente no browser.

## Non-Goals / Anti-Patterns (MANDATÓRIO)

❌ NÃO é aceitável implementar o Recorder capturando apenas:
- chamadas de steps do Reqnroll (ex.: InteractionSteps.cs)
- wrappers do driver usados apenas durante execução automatizada
- hooks do lifecycle de cenário (BeforeScenario/AfterScenario)

✅ O Recorder FREE-HANDS deve capturar interações MANUAIS do usuário diretamente no browser,
via instrumentação (JS injected / CDP) e coleta pelo runtime.

Se o usuário clicar/digitar manualmente e isso não gerar eventos, a implementação está incorreta.


## Gate RF00 + Browser Capture (PASS/FAIL)

Condição:
- NÃO existir nenhum arquivo .feature no workspace (ou não ser carregado)
- AUTOMATION_RECORD=true

Ações manuais:
- navegar para /login
- digitar em 2 campos
- clicar em 2 elementos
- submeter
- navegar para outra rota/tela

PASS se:
- session.json contém >= 5 eventos
- contém pelo menos 1 click e 1 fill
- não contém apenas navigate inicial

---

## RC01 — Injeção do Recorder Script
Quando `AUTOMATION_RECORD=true`, o runtime DEVE injetar um script JS no `document` que:
- inicializa `window.__fhRecorder`
- registra listeners de eventos
- armazena eventos em um buffer (fila)

## RC02 — Captura de click
Ao ocorrer um click, o script DEVE registrar um evento com:
- kind: "click"
- ts: timestamp (ms)
- target: informações do elemento (mínimo: tag + texto curto + atributos úteis)

## RC03 — Captura de fill (input/change consolidado)
Ao ocorrer `input`/`change`, o script DEVE registrar eventos consolidados por elemento:
- deve usar debounce (ex.: 300–600ms) por elemento
- deve registrar um único evento final contendo o valor final

## RC04 — Captura de submit
Ao ocorrer submit (form submit ou Enter detectável), registrar kind: "submit".

## RC05 — Captura de navegação (SPA)
Hookar History API:
- pushState
- replaceState
- popstate
E registrar kind: "navigate" com `route = location.pathname + location.search + location.hash`.

## RC06 — Buffer + polling
O script DEVE expor:
- `window.__fhRecorder.drain()` → retorna e limpa eventos
- `window.__fhRecorder.version`

O runtime DEVE fazer polling periódico via `executeScript("return window.__fhRecorder?.drain?.() ?? []")`.

## RC07 — Timestamps com fuso horário local
O SessionRecorder DEVE gravar timestamps (`startedAt`, `endedAt`) respeitando o fuso horário do sistema (local timezone), NÃO em UTC.

**Razão**: Facilita correlação de logs/evidências com horário de execução real do QA/DEV.

**Implementação**: Usar `DateTimeOffset.Now` ao invés de `DateTimeOffset.UtcNow`.

**Validação**: O offset no JSON deve refletir o fuso local (ex: `-03:00` para GMT-3, não `+00:00`).

## Critérios de aceite
Em modo exploratório:
1. Abrir /login
2. Digitar em 2 campos
3. Clicar em 2 botões
4. Submeter
Resultado: session.json contém > 5 eventos incluindo click e fill.
