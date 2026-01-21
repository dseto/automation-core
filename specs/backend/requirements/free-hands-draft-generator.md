
# FREE-HANDS Draft Generator — Requisitos

⚠️ **Pré-condições obrigatórias**

Este delta assume que a Fase 1 (FREE-HANDS Recorder) está corretamente implementada, incluindo:
- RF00 — Modo exploratório
- Browser Capture Layer
- session.json contendo eventos reais (click/fill/etc)

Se o `session.json` contiver apenas eventos técnicos, o Draft Generator NÃO DEVE operar normalmente.

---

## RF00a — Contrato de qualidade do `target.hint` (depende do FREE-HANDS Recorder)

O `Draft Generator` depende diretamente da qualidade do `session.json`. Para que o Gherkin gerado seja útil
mesmo como **draft**, o FREE-HANDS Recorder **DEVE** produzir um `target.hint` que priorize `data-testid` e,
na ausência dele, gere o **melhor seletor/hint possível** para uso humano e para o Escape Hatch.

### Prioridade de captura (ordem obrigatória)

1) **`data-testid` (crítico)**
   - Se o elemento alvo (ou o elemento efetivamente interagido) possui `data-testid` não vazio, o `target.hint`
     **DEVE** ser gerado como seletor CSS baseado em `data-testid`:
     - Formato: `[data-testid='<valor>']`
     - Ex.: `page.login.username` → `[data-testid='page.login.username']`

2) **Fallback: seletor estável**
   - Se não houver `data-testid`, o Recorder **DEVE** tentar gerar um seletor CSS estável (prioridade sugerida):
     - `#id` (quando o `id` não parecer dinâmico)
     - `[name='<name>']`
     - `[formcontrolname='<formcontrolname>']`
     - `[aria-label='<aria-label>']` / `[placeholder='<placeholder>']`

3) **Fallback final: hint humano (para triagem/escape hatch)**
   - Se nenhum seletor estável for possível, o Recorder **DEVE** registrar um hint humano que ajude QAs/Devs a
     localizar o controle (ex.: `input (label='Usuário')`, `button ('Entrar')`, etc.).

### Anti-drift: tratamento de IDs dinâmicos

IDs gerados por bibliotecas/frameworks (ex.: Angular Material `mat-input-*`, `mat-option-*`, `cdk-*`) são
frequentemente instáveis. Nesses casos, o Recorder **NÃO DEVE** escolher `#id` como primeira opção se existir
qualquer alternativa mais estável (itens 1 e 2 acima).

### Critérios de aceite

- Dado um elemento como:
  - `<input ... data-testid="page.login.username" id="mat-input-0" ...>`
  o `target.hint` **DEVE** ser `[data-testid='page.login.username']` (e **NÃO** `#mat-input-0`).
- Quando não existir `data-testid`, o `target.hint` **DEVE** conter um seletor/hint melhor do que um `tagName`
  genérico (ex.: evitar `div` isolado), preferindo atributos estáveis ou um hint humano com texto/label.


---

## RF09 — Sanity check da sessão

Antes de gerar o draft, o sistema DEVE validar se:
- há mais de um evento relevante
- existe pelo menos um evento semântico (click/fill/select/submit)

Caso contrário:
- registrar warning
- marcar metadata como inválida OU abortar geração

---

## RF10 — Entrada (Normativo)
**O sistema DEVE aceitar um `session.json` válido como entrada.**

### Contrato mínimo de validade (schema lógico)
O `session.json` é considerado **válido** quando:
- Contém `sessionId` (string não vazia)
- Contém `startedAt` (timestamp ISO 8601) e `endedAt` (timestamp ISO 8601)
- Contém `events` como lista não vazia
- Cada `event` contém, no mínimo:
  - `type` (ex.: `click`, `fill`, `navigate`, `submit`, `unknown`)
  - `at` (timestamp ISO 8601)
  - `target.hint` (string). Pode ser vazia **somente** se houver `rawAction`

### Comportamento para sessão inválida
- DEVE emitir **warning** no log explicando o motivo
- DEVE gerar `draft.metadata.json` com `inputStatus = "invalid"`
- DEVE abortar a geração do `draft.feature`

**Exemplo (inválido):** `events` ausente → `inputStatus=invalid` + abort.

---

## RF11 — Agrupamento semântico (Normativo)
**O sistema DEVE agrupar eventos consecutivos e relacionados em “ações” de mais alto nível** para reduzir ruído no `draft.feature`
e aumentar a utilidade do output.

### Definições
- **Consecutivos:** eventos adjacentes no array, com diferença de tempo <= **2000ms**.
  - O valor **padrão oficial** é **2000ms** e DEVE ser definido em uma constante única (ex.: `ActionGrouperDefaults.MaxGapMs = 2000`).
- **Relacionados:** eventos que, após normalização mínima, tenham o mesmo `target.hint` **ou** onde o primeiro evento é “genérico” e o segundo é específico.

### Normalização mínima de `target.hint` (para equivalência)
Para comparar “hints equivalentes”, aplicar exatamente:
- `trim()` nas extremidades
- colapsar múltiplos espaços internos para 1
- normalizar aspas em seletor CSS: substituir `"` por `'` dentro de `[...]`
- manter case (case-sensitive). Atributos como `data-testid` permanecem como vierem do Recorder (não fazer lower-case geral).

> Observação: equivalência aqui é **determinística** (não fuzzy matching).

### Definição formal de “hint genérico”
Um `target.hint` é considerado **genérico** quando, após normalização:
- é exatamente um destes tokens: `div`, `main`, `body`, `html`
**OU**
- é um seletor CSS sem identificadores/atributos úteis, contendo apenas tag e/ou classes (sem `#id` e sem `[attr=]`):
  - exemplos: `div`, `div.container`, `mat-card`, `.container`

### Regras obrigatórias de agrupamento
1) **Focus + Fill**
   - Se houver `click` seguido de `fill` no mesmo alvo (mesmo `target.hint` após normalização) dentro de 2000ms,
     DEVE formar **um único grupo**.
   - O **PrimaryEvent** do grupo DEVE ser o `fill`.
   - O `click` pode ser descartado (ruído) **ou** mantido como evidência (ver regra de evidência).

2) **Click Submit + Submit**
   - Se houver `click` em alvo específico seguido de `submit` dentro de 2000ms,
     DEVE formar **um único grupo**.
   - O **PrimaryEvent** DEVE ser o `click` (ou `submit` apenas se não houver click).

3) **Genérico seguido de específico**
   - Se um evento tiver `target.hint` genérico e o próximo evento for mais específico dentro de 2000ms:
     - o genérico **NÃO PODE** ser PrimaryEvent.
     - o agrupador DEVE:
       - (a) descartar o evento genérico, **ou**
       - (b) mantê-lo como evidência no grupo, mas reordenar para que o específico seja PrimaryEvent.

### Navigate: política de agrupamento neste delta
- `navigate` **NÃO DEVE** ser agrupado com ações de alvo (click/fill) neste delta.
- `navigate` pode ser agrupado apenas com outros `navigate` consecutivos (ex.: redirects):
  - o **último navigate** deve ser PrimaryEvent
  - os anteriores devem ser evidência

### Regra de seleção do PrimaryEvent (ordem de preferência)
Dentro de um grupo, o evento primário DEVE ser o mais semântico conforme prioridade:
1. `fill`
2. `click` com seletor estável (ex.: `[data-testid='...']`)
3. `click` específico (id/name/aria/formcontrolname/placeholder)
4. `submit`
5. `navigate` (apenas em grupos de navigate)
6. eventos genéricos / desconhecidos

### Regra de evidência vs descarte
- Eventos genéricos usados apenas para foco/contexto **podem ser descartados** quando houver evento semântico no grupo.
- Se não descartados, devem constar em `EvidenceEvents` (ou estrutura equivalente) e **nunca** como PrimaryEvent.

### Exemplos (normativos)
**Antes (eventos):**
- `click` alvo `[data-testid="page.login.username"]`
- `fill`  alvo `[data-testid='page.login.username']` valor `"admin"`

**Depois (ação/grupo):**
- PrimaryEvent = `fill` em `[data-testid='page.login.username']` com `"admin"`
- Evidência inclui (opcionalmente) o `click`

**Anti-exemplo (proibido):**
- PrimaryEvent = `click div` quando existe `fill` em alvo específico no mesmo grupo.

### Critérios de aceite
- Para sessões reais, o `draft.feature` DEVE reduzir passos redundantes (ex.: “click para foco” + “fill” vira um só step).
- Determinismo: mesma entrada → mesmos grupos (ordem e seleção do PrimaryEvent estáveis).

---

## RF12 — Inferência de steps
Gerar steps Gherkin preliminares quando possível.


## RF12a — Formato Gherkin compatível com Automation.Reqnroll (obrigatório)

O `draft.feature` gerado **DEVE** ser imediatamente legível e executável (quando houver UIMap) no ecossistema
**Automation.Core/Automation.Reqnroll**, mesmo sendo um rascunho.

Regras de formatação:

1) **Idioma**
- Primeira linha: `#language: pt`
- Keywords em PT-BR:
  - `Funcionalidade:`
  - `Cenário:`
  - Steps: `Dado`, `Quando`, `Então`, `E`

2) **Padrão de steps (compatível com o catálogo atual do Automation.Reqnroll)**
- Navegação (quando houver `navigate`):
  - `Dado que estou na página "<path>"`
- Clique:
  - `Quando eu clico em "<elementRef>"`
- Preenchimento:
  - `Quando eu preencho "<elementRef>" com "<value>"`
- Observações:
  - `Then/Então` deve ser usado apenas quando a ação realmente for uma validação; caso contrário manter ações em `Quando/E`.
  - Para ações não suportadas pela inferência (ex.: select/toggle), aplicar RF13 (escape hatch).

3) **Regra de aspas (anti-quebra de step)**
Os steps do Automation.Reqnroll capturam parâmetros entre aspas duplas; portanto:
- O `<elementRef>` **NÃO PODE** conter aspas duplas (`"`).
- Se o gerador precisar serializar um seletor CSS que contenha aspas duplas (`"`), ele **DEVE** normalizar para aspas simples (`'`) dentro do seletor.
  - Ex.: `[data-testid="page.login.submit"]` → `[data-testid='page.login.submit']`

4) **Higiene do arquivo**
- Sem tabs; apenas espaços.
- Uma linha em branco entre `Funcionalidade:` e o primeiro `Cenário:`.
- Não gerar linhas Gherkin inválidas (ex.: `Feature:` em inglês).

Critério de aceite: o `draft.feature` deve passar no parser do Reqnroll (Gherkin) sem erros de sintaxe.


---
## RF13 — Escape hatch automático (Normativo)
**Quando não for possível inferir um step para uma ação/grupo, o sistema DEVE inserir TODO + RAW** no `draft.feature`,
preservando evidência suficiente para depuração.

### Quando aciona
A ação/grupo deve cair em escape hatch quando:
- O `PrimaryEvent` não é suportado pela inferência de steps **OU**
- `target.hint` está ausente/irrecuperável **OU**
- A ação seria ambígua conforme regra objetiva abaixo

### Regra objetiva de ambiguidade (mínima)
Considerar **ambíguo** quando:
- `target.hint` é vazio/nulo, **OU**
- `target.hint` é **genérico** (definição de RF11) e o step inferido exigiria alvo específico (ex.: click/fill).

### Formato obrigatório no `draft.feature`
- DEVE inserir um comentário TODO imediatamente antes da linha RAW, **sem linha em branco** entre eles.
- A linha RAW DEVE conter **JSON compacto em uma única linha** (minificado), representando no mínimo:
  - `type`
  - `at`
  - `target.hint` (se existir)
  - `value` (se existir)
  - `rawAction.script` (se existir) ou `rawAction` resumido

**Modelo (normativo):**
```gherkin
# TODO: revisar ação não inferida
# RAW: {"type":"select","at":"...","target":{"hint":"..."},"rawAction":{"script":"..."}}
```

### Preservação de script (sem quebrar Gherkin)
- `rawAction.script` DEVE ser incluído como string JSON com quebras de linha escapadas (`\n`), mantendo a saída em **uma linha**.
- Se `rawAction.script` exceder **500 caracteres**, DEVE ser truncado de forma determinística:
  - manter os primeiros 500 caracteres e adicionar sufixo `…(truncated)`
  - registrar warning no metadata: `RAW_SCRIPT_TRUNCATED`

### Critérios de aceite
- Nenhuma ação “desconhecida” pode desaparecer silenciosamente.
- O escape hatch não pode quebrar o Gherkin (RAW sempre em 1 linha).

---

## RF14 — Preservação de evidência (Normativo)
**`rawAction.script` DEVE aparecer no draft** quando presente na sessão.

### Regras
- Se a ação foi inferida normalmente, `rawAction.script` pode aparecer no metadata (ou comentário opcional),
  mas DEVE aparecer pelo menos em um lugar acessível.
- Se a ação caiu em escape hatch (RF13), `rawAction.script` DEVE aparecer no `# RAW:` correspondente
  (respeitando as regras de escape e truncamento de RF13).

---

## RF15 — Artefatos (Normativo)
**O sistema DEVE gerar:**
- `draft.feature`
- `draft.metadata.json`

### Conteúdo mínimo do metadata
`draft.metadata.json` DEVE conter pelo menos:
- `sessionId`
- `inputStatus` (`valid` | `invalid`)
- `generatedAt`
- contadores: `eventsCount`, `actionsCount`, `stepsInferredCount`, `escapeHatchCount`
- lista de `warnings` (pode ser vazia)

---

## RF16 — Não comprometer semântica (Normativo)
**O draft NÃO DEVE depender de UIMap nem DataMap e NÃO DEVE falhar por lacunas de mapeamento.**

### Regras
- O Draft Generator **não pode** tentar resolver `@refs` de UIMap ou chaves de DataMap.
- Se `target.hint` for fraco/ambíguo:
  - DEVE preferir escape hatch (RF13) + warnings em metadata
  - NÃO deve “inventar” um ref ou seletor inexistente
- A ausência de UIMap/DataMap **NÃO** pode impedir a geração do `draft.metadata.json`.

### Critérios de aceite
- Um `session.json` válido gera sempre `draft.metadata.json` (mesmo que o `draft.feature` tenha TODO/RAW).
- Não há dependências de assemblies/projetos de UIMap/DataMap no Delta 2.