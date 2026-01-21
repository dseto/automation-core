
# Delta Pack — 2026-01-21-free-hands-recorder-browser-capture-mvp

> Status: DRAFT  
> Tipo: **Bugfix / Critical enablement**  
> Alvo: garantir captura de interações manuais no modo exploratório (RF00)

## Problema
O `session.json` está registrando apenas `navigate` inicial (abrir browser) e **não captura cliques/digitações manuais**.
Isso indica que a captura está acoplada ao pipeline de testes/steps, e não ao browser.

## Objetivo
Implementar um **Browser Capture Layer** (instrumentação no DOM) para capturar eventos reais do usuário:
- click
- input/change (fill consolidado)
- submit
- navegação (history.pushState/replaceState/popstate)

Sem depender de `.feature`, steps, UIMap ou cenários.

## Dependências
- RF00 (modo exploratório) — deve existir e permanecer válido.
- Session schema da fase 1 (pode ser estendido neste delta de forma compatível).

## Escopo
- Injetar um script JS no browser ao iniciar o modo Recorder
- Coletar eventos em um buffer no `window` e permitir polling pelo runtime
- Mapear eventos JS → eventos do `session.json`
- Consolidar fill (debounce por elemento)
- Persistir `rawAction.script` quando o runtime usar `executeScript`

## Fora de escopo
- Draft Generator
- UIMap / gaps
- Último metro / Strict/Traction completos

## Checklist
- [ ] Em modo exploratório, interações manuais geram eventos (click/fill/submit)
- [ ] session.json deixa de ser “quase vazio”
- [ ] Nenhuma dependência de `.feature`
