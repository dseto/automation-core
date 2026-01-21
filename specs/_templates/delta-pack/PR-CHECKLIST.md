# Pull Request Checklist (Spec-Driven)

## Spec
- [ ] Delta pack criado em `specs/releases/delta/YYYY-MM-DD`
- [ ] `README.md` preenchido
- [ ] `changes.md` atualizado
- [ ] `migration.md` (se breaking)

## Contratos
- [ ] JSON Schemas atualizados (se aplicável)
- [ ] Exemplos válidos criados/atualizados
- [ ] Exemplos inválidos criados/atualizados

## Regras
- [ ] `backend/rules/*` atualizado
- [ ] `tests/validation/*` espelha o Validator

## Steps (se aplicável)
- [ ] `step-catalog.md` atualizado
- [ ] `step-catalog.yaml` regenerado

## Código
- [ ] Core implementado sem acoplamento a runtime
- [ ] Validator reflete regras do spec
- [ ] Runtime apenas orquestra

## Qualidade
- [ ] Unit tests atualizados/criados
- [ ] Contract/spec validation passou
- [ ] Smoke test executado

## Governança
- [ ] Versioning avaliado
- [ ] Breaking change documentado
