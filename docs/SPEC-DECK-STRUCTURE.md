# Estrutura do Spec Deck 📁

Este documento mostra, de forma resumida e legível, a estrutura de pastas e arquivos do *spec deck* que fica em `specs/`.

---

## Visão geral
- Local: `specs/`
- Propósito: fonte única de verdade (SSOT) para requisitos, contratos, regras, exemplos e releases do projeto.

---

## Estrutura (visão em árvore)

```
specs/
├─ 00-index.md
├─ _legacy/
│  ├─ 00-index.md
│  ├─ 01-vision-and-scope.md
│  └─ ...
├─ api/
│  ├─ contracts.md
│  ├─ openapi.yaml
│  └─ examples/
│     ├─ datamap.example.yaml
│     └─ ...
├─ backend/
│  ├─ architecture/
│  ├─ implementation/
│  ├─ requirements/
│  └─ rules/
├─ frontend/
│  ├─ README.md
│  └─ uimap.yaml
├─ ops/
│  ├─ ci-gates.md
│  └─ runbooks/
├─ releases/
│  ├─ README.md
│  └─ delta/
├─ shared/
│  ├─ architecture-principles.md
│  └─ README.md
├─ tests/
│  ├─ README.md
│  └─ validation/
└─ ...
```

> Nota: dentro de `releases/delta/` ficam os *delta packs* (entregas) com mudanças e migrações, por exemplo `releases/delta/2026-01-20/`.

---

## Descrição por diretório e arquivos 🔎
Abaixo há uma descrição das subpastas e dos arquivos presentes em `specs/` (lista atualizada).

### Raiz
- `00-index.md` — índice principal do spec deck, aponta para seções e conteúdos relevantes.
- `SPEC-DECK-STRUCTURE.md` — este arquivo (mapa da árvore e descrições).

### `_legacy/` (histórico)
Arquivos mais antigos mantidos para referência:
```
_legacy/
├─ 00-index.md            — índice legado
├─ 01-vision-and-scope.md — visão e escopo históricos
├─ 02-architecture.md     — arquitetura (legado)
├─ 03-contracts.md        — contratos (legado)
├─ 04-runtime-resolution.md
├─ 05-step-catalog.md
├─ 06-escape-hatch.md
├─ 07-validation-and-testing.md
├─ 08-security-and-compliance.md
├─ 09-implementation-guide.md
├─ 10-troubleshooting.md
└─ backlog.md
```
Use `_legacy/` quando precisar consultar decisões e documentos previos.

### `api/`
- `contracts.md` — resumo dos contratos de API e decisões de versão.
- `openapi.yaml` — especificação OpenAPI usada para validação e geração de clientes.
- `versioning/versioning-policy.md` — política de versionamento das APIs/specs.

`api/examples/` — exemplos de payloads e casos de uso:
```
api/examples/
├─ datamap.example.yaml                        — exemplo válido de DataMap
├─ datamap.invalid-bad-strategy.yaml           — exemplo inválido (estratégia incorreta)
├─ datamap.invalid-missing-default.yaml        — exemplo inválido (missing default)
├─ draft.feature.example.feature               — exemplo de feature Gherkin (draft)
├─ README.md                                   — explicação sobre os exemplos
├─ recorder.session.login.example.json         — sessão de recorder (login) de exemplo
├─ recorder.session.manual.example.json        — sessão manual de recorder (exemplo)
├─ uimap.example.yaml                          — exemplo de UiMap
├─ uimap.invalid-bad-anchor.yaml               — exemplo inválido (anchor errado)
└─ uimap.invalid-missing-testid.yaml           — exemplo inválido (missing testid)
```

`api/schemas/` — esquemas JSON usados para validar arquivos:
```
api/schemas/
├─ datamap.schema.json          — schema para DataMap
├─ draft.metadata.schema.json   — schema para metadados de drafts
├─ recorder.session.schema.json — schema para sessões do recorder
├─ uimap.schema.json            — schema para UiMap
└─ README.md                    — instruções sobre schemas
```

### `backend/`
Organiza arquitetura, requisitos, implementação e regras:
```
backend/
├─ architecture/
│  ├─ free-hands-draft-generator.md
│  ├─ free-hands-recorder-browser-capture.md
│  ├─ free-hands-recorder-session.md
│  └─ system-architecture.md
├─ implementation/
│  ├─ free-hands-draft-generator.md
│  ├─ free-hands-recorder-browser-capture.md
│  ├─ free-hands-recorder-session.md
│  ├─ README.md
│  └─ run-settings.md            — run-settings e valores de ambiente (ex. WAIT_ANGULAR)
├─ requirements/
│  ├─ free-hands-draft-generator.md
│  ├─ free-hands-recorder-browser-capture.md
│  └─ free-hands-recorder-session.md
└─ rules/
   ├─ data-resolution.md
   ├─ element-resolution.md
   ├─ escape-hatch.md
   ├─ implementation-guide.md
   ├─ resolution-flow.md
   └─ runtime-resolution.md
```
- `architecture/` — decisões arquiteturais por subcomponente.
- `implementation/` — guias de implementação e `run-settings.md` (valores e variáveis de runtime).
- `requirements/` — requisitos funcionais por feature.
- `rules/` — regras e fluxos para resolução de elementos e dados.

### `frontend/`
- `README.md` — notas gerais do frontend/specs para captura e recorder.
- `uimap.yaml` — exemplo/definição do UiMap padrão do projeto.
- `notes/free-hands-recorder.injected-script.js` — script usado pela captura no recorder (quando aplicável).

### `ops/`
Documentação operacional:
```
ops/
├─ ci-gates.md        — critérios para gates de CI
├─ compliance.md      — requisitos de compliance
├─ observability.md   — métricas e logs necessários
├─ security.md        — considerações de segurança
├─ troubleshooting.md — runbooks gerais
└─ runbooks/
   └─ troubleshooting.md — procedimentos de troubleshooting
```

### `releases/`
- `README.md` — orientações sobre entregas e delta packs.
- `delta/` — pasta com *delta packs* (entregas). Exemplo de um delta pack:
```
releases/delta/2026-01-21-free-hands-recorder-browser-capture-mvp/
├─ changes.md   — lista de mudanças da entrega
├─ migration.md — passos de migração/impacto
└─ README.md    — notas da entrega
```

### `shared/`
Recursos e convenções reutilizáveis:
```
shared/
├─ architecture-principles.md
├─ domain-model.md
├─ glossary.md
├─ naming-conventions.md
├─ nfr.md                      — requisitos não funcionais
├─ README.md
├─ spec-driven-flow.md
├─ ui-field-catalog.md
└─ vision.md
```

### `tests/`
Guides e validações relacionadas aos specs:
```
tests/
├─ README.md
├─ gherkin/
│  ├─ escape-hatch.md
│  ├─ step-catalog.md
│  └─ step-catalog.yaml
└─ validation/
   ├─ datamap-validation.md
   ├─ free-hands-draft-generator.md
   ├─ gherkin-validation.md
   ├─ recorder-browser-capture-mvp.md
   ├─ recorder-session-validation.md
   ├─ step-catalog-validation.md
   ├─ uimap-validation.md
   └─ validation-policy.md
```
- `tests/gherkin/` contém o catálogo de steps e features de exemplo.
- `tests/validation/` descreve casos de validação e políticas aplicadas pelo validador.

---

## Como validar 🔍
No PowerShell, rode:

```powershell
Get-ChildItem -Recurse -Force .\specs | Format-Table -AutoSize
```

Ou use o utilitário `tree` para uma visão hierárquica:

```powershell
tree specs /F
```

---

## Observações finais ✅
- Mantenha este arquivo atualizado sempre que a estrutura de `specs/` mudar.
- Para alterações de comportamento, consulte os arquivos em `specs/releases/delta/` antes de codificar.

---

Arquivo gerado automaticamente para facilitar navegação e documentação interna.