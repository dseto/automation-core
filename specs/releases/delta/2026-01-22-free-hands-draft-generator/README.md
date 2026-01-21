
# Delta Pack — 2026-01-22-free-hands-draft-generator (v2)

> Status: RELEASED  
> Versão: v0.5.0  
> Data de fechamento: 2026-01-21  
> Fase: FREE-HANDS Recorder — Delta 2 (Draft Generator)

## Objetivo
Introduzir a **fase semântica inicial** do FREE-HANDS Recorder: transformar `session.json` (log factual) em um **rascunho operacional** (`draft.feature`), pronto para refinamento humano.

Este delta **não substitui o humano**. Ele gera um draft, não um teste final.

## Pré-requisito crítico
Este delta pressupõe que o FREE-HANDS Recorder já esteja capturando interações manuais reais no browser.

Se o `session.json` não contiver eventos como click e fill,
o Draft Generator não deve ser usado para mascarar falhas de captura.

Entrada pobre ⇒ saída inválida.

## Dependências
- Delta 1 — Recorder Session + Browser Capture MVP
- RF00 — Modo exploratório

## Escopo
- Leitura de `session.json`
- Sanity check da sessão
- Agrupamento de eventos em ações
- Inferência inicial de steps Gherkin
- Geração de `draft.feature`
- Geração de `draft.metadata.json`
- **Escape hatch automático**
- Preservação de `rawAction.script`

## Fora de escopo
- UIMap
- Gaps
- Strict/Traction completos
- Geração de cenário final

## Checklist
- [x] session.json → draft.feature
- [x] escape hatch presente
- [x] JS refletido no draft
- [x] sanity check ativo


## Nota importante — Padrão de Gherkin (RF12a)
Mesmo sendo draft, o `draft.feature` **segue** o padrão do Automation.Reqnroll:
- `#language: pt`
- Keywords PT-BR (`Funcionalidade/Cenário/Dado/Quando/Então/E`)
- `<elementRef>` sem aspas duplas internas (normalizar seletores para `'").

