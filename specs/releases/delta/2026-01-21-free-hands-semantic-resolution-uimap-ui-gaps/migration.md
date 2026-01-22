# Delta 3 — Migration

## Quem precisa migrar
- Implementações do pipeline FREE-HANDS que já geram `draft.feature` (Delta 2) e agora precisam produzir a Fase 3.

## Passos (ordem fixa)
1) Garantir que existe `ui-map.yaml` em `UIMAP_PATH` (default: `specs/frontend/uimap.yaml`).
2) Implementar o Semantic Resolver conforme:
   - `specs/backend/requirements/free-hands-semantic-resolution.md`
   - `specs/backend/rules/semantic-resolution.md`
3) Gerar artefatos no diretório `SEMRES_OUTPUT_DIR`:
   - `resolved.feature`
   - `resolved.metadata.json`
   - `ui-gaps.report.json`
   - `ui-gaps.report.md`
4) Validar contratos:
   - `resolved.metadata.json` contra `specs/api/schemas/resolved.metadata.schema.json`
   - `ui-gaps.report.json` contra `specs/api/schemas/ui-gaps.report.schema.json`
5) Executar gates do Delta 3:
   - `specs/tests/validation/free-hands-semantic-resolution.md`

## Verificação pós-migração (mín. 3 checks)
- [ ] `resolved.feature` mantém ordem e não altera steps `partial/unresolved`.
- [ ] Todo step `unresolved` aparece como finding `error` no `ui-gaps.report.json`.
- [ ] Os exemplos canônicos são aceitos (schema + formato).

## O que NÃO fazer
- Não editar automaticamente `specs/frontend/uimap.yaml`.
- Não “adivinhar” o elemento correto quando houver ambiguidade.
- Não emitir apenas logs; os artefatos são obrigatórios.
