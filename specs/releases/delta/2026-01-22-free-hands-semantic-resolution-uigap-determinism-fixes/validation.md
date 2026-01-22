# validation — como validar o delta

Use `specs/tests/validation/free-hands-semantic-resolution.md` como referência.

Checklist mínimo:
- Outputs gerados: `resolved.feature`, `resolved.metadata.json`, `ui-gaps.report.json`, `ui-gaps.report.md`
- Consistência por `draftLine` entre feature/metadata/report
- Ordenação e IDs determinísticos (`UIGAP-0001...`)
- No guessing: `partial/unresolved` não reescrevem step para candidato
- RunSettings: `UI_MAP_PATH` vence `UIMAP_PATH`
