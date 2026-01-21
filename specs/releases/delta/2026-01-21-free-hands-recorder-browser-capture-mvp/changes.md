
# Changes — Browser Capture MVP

## Added
- Browser Capture Layer (JS injected)
- Captura de click/input/submit + navegação por History API
- Buffer `window.__fhRecorder` com polling determinístico

## Changed
- Session event model: `target` passa a aceitar `css`, `tag`, `text`, `attributes` (opcional)

## Fixed
- session.json vazio ao interagir manualmente
- [BUG-2026-01-21] Ordem errada de argumentos em `push()` ao capturar navegações SPA via History API
  - **Sintoma**: `TypeError: Cannot read properties of null (reading 'tagName')` em apps Angular quando executando `history.pushState()`
  - **Causa**: Chamadas `push(document.body, null, {...})` passavam `kind=null` e `targetEl=document.body` (ordem invertida)
  - **Correção**: Alterado para `push('navigate', document.body, {...})` em 3 hooks (pushState, replaceState, popstate)
  - **Arquivos**: `src/Automation.RecorderTool/Program.cs` (InjectBrowserCaptureScript, linhas ~172-179)
  - **Validação**: Testado com aplicação Angular 17 (Azure Static App) — 23 eventos capturados incluindo 5 navegações SPA
- [BUG-2026-01-21] Timestamps em session.json gravados em UTC ao invés do fuso horário local
  - **Sintoma**: `startedAt` e `endedAt` com offset `+00:00` (UTC) ao invés de `-03:00` (GMT-3/horário local)
  - **Causa**: `SessionRecorder` usava `DateTimeOffset.UtcNow` para gravar timestamps
  - **Correção**: Alterado para `DateTimeOffset.Now` (respeita fuso horário do sistema)
  - **Arquivos**: `src/Automation.Core/Recorder/SessionRecorder.cs` (Start, Stop)
  - **Validação**: Timestamps agora refletem fuso local (ex: `-03:00` para Brasília)
