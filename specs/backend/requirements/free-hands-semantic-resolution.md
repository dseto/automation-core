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
