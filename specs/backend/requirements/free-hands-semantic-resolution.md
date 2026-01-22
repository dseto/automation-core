# Free Hands — Semantic Resolution (Delta 3)

## RF30 — Pipeline de Semantic Resolution (Normativo)
**Intenção:** transformar um `draft.feature` em `resolved.feature` sem “adivinhar” elementos, registrando gaps de forma auditável.

**Regras objetivas**
- Entrada mínima:
  - `draft.feature` (texto Gherkin)
  - `ui-map.yaml` (UI Map)
  - opcional: `recorder.session.json` (quando existir)
- Saídas:
  - `resolved.feature` (Gherkin)
  - `resolved.metadata.json` (metadados de resolução)
  - `ui-gaps.report.json` e `ui-gaps.report.md` (gaps acionáveis)
- A resolução **NÃO** pode:
  - inventar pages/elements inexistentes
  - substituir referências em steps quando houver ambiguidade (ver RF34)

**Exemplo normativo**
- Entrada: step com `elementRef="login.username"`
- UIMap: possui `login.username`
- Saída: `resolved.feature` mantém o step e `resolved.metadata.json` marca o step como `resolved`.

---

## RF31 — Resolução via UIMap key (Normativo)
**Intenção:** mapear refs do draft para elementos do UIMap por chave estável.

**Regras objetivas**
- Quando um step referenciar `pageKey.elementKey`, o resolver deve procurar essa key no UIMap.
- Se existir match exato: status `resolved`.
- Se não existir: gerar finding `UI_MAP_KEY_NOT_FOUND` e status `unresolved`.

**Exemplo normativo**
- Entrada: `checkout.payButton`
- UIMap não contém `checkout.payButton`
- Saída: `ui-gaps.report.json` inclui finding `UI_MAP_KEY_NOT_FOUND` para o step.

---

## RF32 — Fallback por testId quando houver Session (Normativo)
**Intenção:** permitir refinar candidatos usando `target.testId` quando `recorder.session.json` estiver disponível.

**Regras objetivas**
- Se `recorder.session.json` existir e o step possuir `target.testId`, o resolver pode:
  - localizar no UIMap o elemento cujo `testId` seja igual ao `target.testId`
  - **somente** quando isso produzir um match único e consistente
- Se gerar múltiplos candidatos: status `partial` + candidates listados (ver RF34)

**Exemplo normativo**
- Entrada: step com `target.testId="btn-login"`
- UIMap: dois elementos com `testId="btn-login"`
- Saída: `partial` com `candidates` (limitados por RF38) e findings explicando ambiguidade.

---

## RF33 — Determinismo do relatório de gaps (Normativo)
**Intenção:** garantir reprodutibilidade e rastreabilidade (diffs limpos em PR).

**Regras objetivas**
- O `ui-gaps.report.json` deve ser ordenado por:
  1) `draftLine` (asc)
  2) `severity` (error > warn > info)
  3) `code` (asc)
- IDs de gaps devem ser determinísticos:
  - formato: `UIGAP-0001`, `UIGAP-0002`, ...
  - atribuídos sequencialmente após a ordenação definida acima
- É proibido usar GUID/aleatório para IDs.

**Exemplo normativo**
- Dois runs com a mesma entrada devem produzir os mesmos IDs para os mesmos findings.

---

## RF34 — Não “adivinhar” e não substituir refs em ambiguidade (Normativo)
**Intenção:** evitar falsos positivos e testes instáveis.

**Regras objetivas**
- Em qualquer cenário ambíguo:
  - o step em `resolved.feature` **não** pode ser reescrito para um elementRef “escolhido”
  - o step é marcado como `partial`
  - `candidates[]` e `findings[]` devem explicar as opções e o motivo da ambiguidade
- Em `unresolved`, o step permanece como está e os findings devem ser suficientes para ação humana.

**Exemplo normativo**
- UIMap contém 2 candidatos plausíveis para a mesma ref
- Saída: step preservado + `partial` + `candidates`.

---

## RF35 — Comentários UIGAP no resolved.feature (Normativo)
**Intenção:** tornar gaps visíveis no próprio arquivo resolvido, no local certo.

**Regras objetivas**
- Para cada finding do tipo error/warn associado a um step, o `resolved.feature` deve conter um comentário imediatamente acima do step:
  - `# UIGAP: <id> <code> — <message>`
- O comentário deve referenciar o ID determinístico do `ui-gaps.report.json`.

**Exemplo normativo**
- `# UIGAP: UIGAP-0001 UI_MAP_KEY_NOT_FOUND — Element key não encontrada no UIMap`

---

## RF36 — Metadados de resolução por step (Normativo)
**Intenção:** permitir auditoria e tooling.

**Regras objetivas**
- `resolved.metadata.json` deve listar, por step:
  - `draftLine`
  - `status`: `resolved` | `partial` | `unresolved`
  - `chosen` (quando resolved)
  - `candidates` (quando partial)
  - `findings` (sempre que houver gaps)
- Deve existir correlação por `draftLine` entre `resolved.feature`, metadata e report.

---

## RF37 — Nomenclatura e compatibilidade de RunSettings (Normativo)
**Intenção:** evitar breaking change em automações e documentação.

**Regras objetivas**
- `UI_MAP_PATH` é canônico.
- `UIMAP_PATH` é alias aceito.
- Precedência: `UI_MAP_PATH` vence `UIMAP_PATH`.
- Defaults devem ser estáveis e documentados.

---

## RF38 — Limites de performance (Normativo)
**Intenção:** impedir explosão de candidatos e manter run previsível.

**Regras objetivas**
- `SEMRES_MAX_CANDIDATES` define o limite máximo de candidatos retornados por finding/step.
- Ao exceder o limite:
  - truncar candidatos
  - registrar finding `CANDIDATES_TRUNCATED` com `totalCandidates` e `returnedCandidates`


## RF39 — Resolução determinística de rotas de navegação (Normativo)

### Intenção
Quando um step do draft representa navegação (`Dado que estou na página "<route>"`), o Semantic Resolver deve mapear
a rota para uma página do UiMap de forma **determinística**, sem heurísticas.

### Regras objetivas
1) Fonte única:
   - O Semantic Resolver **DEVE** usar o valor do parâmetro `<route>` do step (que veio de `event.route`) como entrada.
   - O Semantic Resolver **NÃO DEVE** “corrigir” rotas (ex.: concatenar segmentos, remover subpaths, tentar adivinhar `.html`).

2) Algoritmo de mapeamento (ordem obrigatória):
   A) **Match por page key (nome da página)**  
      - Se `<route>` for exatamente igual ao `pageKey` de uma página no UiMap, resolver para essa página.
   B) **Match por `__meta.route` (comparação exata)**  
      - Se `<route>` contém `#`, extrair o fragmento (inclui `#`), por exemplo `#/dashboard`.
      - Derivar `routeKey`:
        - Se o fragmento começa com `#/`, então `routeKey = fragmento.Substring(1)` → `/dashboard`
        - Caso contrário, `routeKey = fragmento` sem o `#` inicial, mantendo o restante.
      - Resolver para a página cujo `__meta.route` seja exatamente `routeKey`.
   C) **Fallback por pathname completo**
      - Se `<route>` não contém `#`, ou se o passo B não encontrar match:
        - Tentar match exato de `<route>` contra `__meta.route`.
        - (Isto cobre apps que usam rotas “full path” como `/app.html` no UiMap.)

3) Resultado quando não mapear:
   - Se nenhum match ocorrer, o resolver **DEVE** emitir um finding:
     - `code`: `UIGAP_ROUTE_NOT_MAPPED`
     - `severity`: `info`
   - O step permanece **unresolved** em nível de página (sem `chosen.pageKey`).

### Exemplo normativo (UiMap + route → resolução)

**Input (step do draft)**
```gherkin
Dado que estou na página "/app.html#/dashboard"
```

**UiMap (trecho)**
```yaml
pages:
  dashboard:
    __meta:
      route: "/dashboard"
```

**Output (metadados de resolução)**
- `chosen.pageKey = "dashboard"`
- `confidence = 1.0` (determinístico)
- `status = "resolved"`

### Critérios de aceite
- [ ] Se o UiMap tem `__meta.route="/dashboard"`, o step `/app.html#/dashboard` resolve para `dashboard`.
- [ ] Se não houver match, é emitido `UIGAP_ROUTE_NOT_MAPPED` (info) e nenhuma página é escolhida.
- [ ] Não existem tentativas de “adivinhar” (ex.: `/login.html` → `/login` sem regra explícita).

### Anti-exemplo (proibido)
- Resolver `/login.html` para uma página com `__meta.route="/login"` **sem** match exato ou regra normativa adicional.

