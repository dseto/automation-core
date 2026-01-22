# Plano: Determinismo de Rotas — Draft Generator & Semantic Resolution 🔧

Contexto Funcional (para QA / Analistas) 🧭

- O que está acontecendo no produto: durante sessões exploratórias gravadas pelo Recorder, o sistema gera um rascunho (`draft.feature`) que descreve a sequência de páginas e ações do usuário (ex.: abrir login → preencher campos → navegar para o dashboard).
- Sintoma percebido pelo QA: ao executar os cenários gerados automaticamente, o navegador pode abrir uma URL incorreta (ex.: `.../login.html/login`), resultar em erro 404 ou falhas intermitentes nos testes. Isso ocorre quando a informação de rota é ambígua ou está fragmentada no `session.json` e o motor tenta "adivinhar" o destino.
- Impacto no fluxo de QA/BA: relatórios e evidências geradas ficam inconsistentes e os ciclos de validação são mais lentos, pois é preciso corrigir manualmente rotas e steps que deveriam ser reproduzíveis automaticamente.
- O que queremos do ponto de vista funcional: que as rotas sejam explícitas e determinísticas no `session.json` (ex.: `/login.html`, `/app.html#/dashboard`) para que o rascunho e o `resolved.feature` reflitam *precisamente* a navegação do usuário, sem tentativas de inferência automatizada.

**Resumo**

O objetivo deste plano é garantir que as rotas usadas pelo pipeline FREE-HANDS sejam determinísticas e explícitas: o `session.json` deve conter rotas normalizadas e o `DraftGenerator` deve transcrever essas rotas sem heurísticas ou "adivinhação" para o `draft.feature`/`resolved.feature`. O `SemanticResolver` deve então mapear essas rotas para páginas do `UiMap` com regras claras e determinísticas.

---

## 1) Problema observado 🔎

- O `draft.feature` contém linhas onde seletores/textos têm quebras de linha internas, quebrando o parser Gherkin.
- Eventos de navegação gravam caminhos locais (ex.: `C:/Projetos/.../app.html#/dashboard`) ou rotas incompletas; o motor tinha que "adivinhar" a route, levando a inconsistências e falhas (p.ex. `file:///.../login.html/login`).

> Requisito: **rotas determinísticas** — o Recorder grava rotas com formato padrão e o Draft/Semantic usam exatamente esse valor para gerar/interpretar steps.

---

## 2) Requisitos de alto nível ✅

- `session.json` deve conter, para eventos `navigate`, campos **determinísticos** (ex.: `url`, `route`, `pathname`, `fragment`).
- `DraftGenerator` deve usar `event.route` diretamente para gerar `Dado que estou na página "<route>"` (apenas normalização para Gherkin: colapsar whitespace, trocar aspas internas etc.).
- `SemanticResolver` deve mapear `route` para páginas do `UiMap` com regras explícitas e não-heurísticas (fragment → `__meta.route`, pathname → comparação direta).
- Em caso de não-mapeamento, emitir `UIGAP_ROUTE_NOT_MAPPED` (severity `info`) — sem tentativas de "adivinhar".

---

## 3) Exemplos contratuais (ilustrativos) 💡

session.json (navigate event):

```json
{
  "t": "00:00.000",
  "type": "navigate",
  "url": "http://localhost/insurance-quote-spa-static/app.html#/dashboard",
  "route": "/app.html#/dashboard",
  "pathname": "/app.html",
  "fragment": "#/dashboard"
}
```

Geração no feature:

```gherkin
Dado que estou na página "/app.html#/dashboard"
```

`SemanticResolver` (regra):
- se `route` contém `#`, extrair `fragment` (`#/dashboard`) -> mapear para `__meta.route` == `/dashboard` (comparação exata);
- senão comparar `pathname` (`/app.html`) com possíveis rotas do `UiMap` ou page key equivalentes;
- se não mapear, emitir finding `UIGAP_ROUTE_NOT_MAPPED` (info).

---

## 4) Arquivos-alvo e mudanças propostas 🔧

- Recorder (normalização de captura)
  - `src/Automation.Core/Recorder/RecorderEvent.cs` + código do capturador do navegador
  - Objetivo: garantir `url`, `route`, `pathname`, `fragment` com formato padronizado (route = path + fragment). Atualizar schema (`specs/api/schemas/recorder.session.schema.json`) se necessário.

- Draft Generator (produção do feature)
  - `src/Automation.Core/Recorder/Draft/DraftGenerator.cs`
  - Objetivo: gerar `Dado que estou na página "<route>"` usando `event.route` (aplicar apenas sanitização para Gherkin: colapsar whitespace, remover quebras internas, substituir `"` por `'`).

- Semantic Resolver (mapeamento)
  - `src/Automation.Core/Recorder/Semantic/SemanticResolver.cs`
  - Objetivo: implementar mapeamento determinístico descrito acima (fragment -> `__meta.route`, pathname -> comparações diretas). Não fazer guessing além das regras normativas.

- Helpers / Normalização
  - `src/Automation.Core/Recorder/Draft/HintHelpers.cs` e `EscapeHatchRenderer.cs`
  - Objetivo: garantir que seletores e RAW JSON no `# RAW:` sejam uma linha (JSON compacto) e que hints multi-line sejam compactados.

- Esquemas e Docs
  - `specs/api/schemas/recorder.session.schema.json` — declarar formato esperado de `route`/`url`/`fragment`.
  - Atualizar `specs/releases/delta/.../changes.md` e `migration.md` para documentar regra de determinismo de rotas.

- Testes
  - `src/Automation.Core.Tests/DraftGeneratorTests.cs` — teste: event.route com `#/dashboard` gera step em 1 linha; sanitização de hints multiline.
  - `src/Automation.Core.Tests/SemanticResolutionTests.cs` — teste: `/app.html#/dashboard` resolve para página `dashboard` quando UiMap tem `__meta.route: /dashboard`.

---

## 5) Passos detalhados / ordem de execução 🧭

1. Atualizar schema: adicionar campo `route` (se necessário) e documentar formato (path + optional fragment).
2. Escrever testes unitários (DraftGenerator & SemanticResolver).  
3. Implementar sanitização de hints e RAW JSON (EscapeHatchRenderer) para evitar quebras de linha no `.feature`.
4. Fazer `DraftGenerator` usar `event.route` diretamente; adicionar sanitização leve (colapsar `\s+`).
5. Implementar mapeamento determinístico no `SemanticResolver` (fragment -> page route; pathname -> route matching).  
6. Rodar integração E2E: gerar `draft.feature` e `resolved.feature` a partir de `session.json` real e executar `dotnet test` dos `UiTests` para garantir ausência de parse errors.
7. Atualizar docs/migration e checklist do delta.

---

## 6) Checklist de validação ✅

- [ ] `recorder.session.schema.json` documenta `route`/`url`/`fragment` (exemplos + formato).
- [ ] Teste-unit: `DraftGenerator` gera `Dado que estou na página "<route>"` sem quebras internas.
- [ ] Teste-unit: `SemanticResolver` resolve `/app.html#/dashboard` para página `dashboard` quando UiMap contém `__meta.route: /dashboard`.
- [ ] Teste-integração: gerar draft/resolved com `session.json` real e executar `dotnet test` — sem erros de parse de Gherkin.
- [ ] Documentação (migration.md) atualizada com regras e steps de validação.

---

## 7) Riscos & Mitigação ⚠️

- Risco: escolher igualdade `pathname` vs `__meta.route` (ex.: `/login.html` vs `/login`).
  - Mitigação: definir política clara (preferência por fragment -> route; para pathname, documentar regra de normalização — ex.: strip `.html` se e somente se UiMap usar `/login`). Incluir testes que explicitem a decisão.

- Risco: mudanças de normalização alteram textos de draft para o usuário.
  - Mitigação: manter escape hatch (`# RAW:`) com o rawAction e avisos; documentar mudanças e retrocompatibilidade no migration.md.

---

## 8) Próximos passos (operacionais) ▶️

- Se desejar, posso preparar um conjunto de commits propostos (1) testes, (2) implementação, (3) docs, para revisão.  
- Alternativamente, posso criar um checklist de pull request com os testes e comandos para validação.

---

> Nota: este documento foi gerado como plano técnico e de validação. Ele não altera código — serve como base para a execução controlada das mudanças.

---

Se quiser, gero também os snippets de código sugeridos para cada arquivo (ex.: função de extração de fragment em `SemanticResolver`) para acelerar a implementação.

---

## Glossário (o que cada arquivo / artefato significa) 📚

- `src/Automation.Core/Recorder/RecorderEvent.cs` — Modelo dos eventos capturados pelo Recorder (cada clique, navegação ou preenchimento). Contém campos como `route`/`url` que descrevem **o que** e **onde** aconteceu.

- `src/Automation.Core/Recorder/Draft/DraftGenerator.cs` — Código que transforma a sequência de eventos (`session.json`) em um rascunho legível (`draft.feature`). Responsável por escrever os passos Gherkin que descrevem a sessão.

- `src/Automation.Core/Recorder/Draft/HintHelpers.cs` — Utilitários para normalizar sugestões de seletores/hints (ex.: transformar seletores em formas consistentes para uso no draft).

- `src/Automation.Core/Recorder/Draft/EscapeHatchRenderer.cs` — Gera blocos `# TODO` e `# RAW:` quando uma ação não pôde ser inferida; garante que o conteúdo bruto seja preservado para análise humana.

- `src/Automation.Core/Recorder/Semantic/SemanticResolver.cs` — Lógica que pega o `draft.feature` e tenta resolver referências contra o `UiMap`, produzindo `resolved.feature`, `ui-gaps.report.json` e `resolved.metadata.json` (ou seja, aponta gaps e mapeamentos encontrados).

- `specs/api/schemas/recorder.session.schema.json` — Esquema JSON que define o formato esperado de `session.json` (valida campos como `route`/`url`/`fragment`). Útil para garantir contratos entre Recorder e os componentes que consomem a sessão.

- `specs/releases/delta/.../changes.md` e `migration.md` — Documentos do delta que explicam o que mudou, o impacto e os passos de migração para adaptações necessárias (útil para QA/BA planejar validação e rollout).

- `src/Automation.Core.Tests/DraftGeneratorTests.cs` — Testes unitários que garantem que o Draft Generator infere corretamente steps (ex.: espera, uso de `data-testid`, normalizações).

- `src/Automation.Core.Tests/SemanticResolutionTests.cs` — Testes que verificam como o Semantic Resolver mapeia refs e gera reports de gaps (determinismo, candidatos, etc.).

- `draft.feature` — Arquivo gerado pelo Draft Generator; é o rascunho legível da sessão (a ser revisado por humanos).

- `resolved.feature` — Versão do feature com comentários de gaps/decisões após o Semantic Resolver; é o artefato usado para executar cenários ou para revisão de gaps.

- `ui-gaps.report.json` / `resolved.metadata.json` — Artefatos JSON que documentam os problemas de mapeamento (gaps) e metadados da resolução. Importante para auditoria e para automações que validam a qualidade da saída.

---

> Use este glossário ao revisar o plano ou quando precisar explicar o fluxo para stakeholders não técnicos; se quiser, posso transformar isso em uma página breve de FAQ para o time de QA.