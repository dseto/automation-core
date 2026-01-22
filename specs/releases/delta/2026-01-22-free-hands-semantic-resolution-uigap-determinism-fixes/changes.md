# changes — Semantic Resolution (Delta 3)

## Added
- SSOT: requisitos, regras e arquitetura de Semantic Resolution
- API: schemas e exemplos (resolved.metadata, ui-gaps.report)
- Tests: validação de determinismo, consistência por draftLine e no-guessing

## Changed
- RunSettings: adicionados parâmetros SEMRES_* e alias `UIMAP_PATH` com precedência definida

## Notes
- IDs de gaps são **determinísticos** (proibido GUID).
