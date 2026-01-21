# Spec Deck (Spec-Driven) — Índice

Este spec deck é **SSOT** do produto. Ele substitui o formato sequencial (spec book) por uma estrutura por domínios:
- `shared/` — vocabulário, modelo de domínio, princípios e convenções.
- `api/` — contratos externos (OpenAPI/JSON Schemas) e política de versionamento.
- `backend/` — regras de runtime e arquitetura técnica.
- `frontend/` — contratos de UI (ui-map) e convenções.
- `tests/` — critérios executáveis (Gherkin, test data) e política de validação.
- `ops/` — segurança, compliance, observabilidade e runbooks.
- `releases/` — delta packs por release.

> Rastreabilidade: os arquivos originais estão em `_legacy/`.

## Onde cada capítulo antigo foi parar (resumo)
- 01 Vision → `shared/vision.md`
- 02 Architecture → `shared/domain-model.md` e `backend/architecture/system-architecture.md`
- 03 Contracts → `api/` e `tests/validation/`
- 04 Runtime Resolution → `backend/rules/`
- 05 Step Catalog → `tests/gherkin/step-catalog.md`
- 06 Escape Hatch → `tests/gherkin/escape-hatch.md` + `backend/rules/escape-hatch.md`
- 07 Validation & Testing → `tests/` + `tests/validation/`
- 08 Security & Compliance → `ops/security.md` + `ops/compliance.md`
- 09 Implementation Guide → `backend/implementation/`
- 10 Troubleshooting → `ops/runbooks/`
- Backlog → `releases/delta/`
