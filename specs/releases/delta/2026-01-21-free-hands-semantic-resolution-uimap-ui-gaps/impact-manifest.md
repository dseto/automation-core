# Delta 3 — Impact Manifest

Arquivos SSOT impactados (fora do delta pack):

1) specs/backend/requirements/free-hands-semantic-resolution.md
- Tipo: requirements
- Motivo: adicionar RF30–RF38 (Semantic Resolution + UI Gaps)
- RFs: RF30–RF38

2) specs/backend/rules/semantic-resolution.md
- Tipo: rules
- Motivo: consolidar regras e catálogo de findings
- RFs: RF30–RF38

3) specs/backend/architecture/free-hands-semantic-resolution.md
- Tipo: architecture
- Motivo: documentar o estágio Delta 3 no pipeline FREE-HANDS
- RFs: RF30–RF38

4) specs/backend/implementation/run-settings.md
- Tipo: implementation
- Motivo: adicionar settings do Semantic Resolution (defaults oficiais)
- RFs: RF30, RF33

5) specs/api/schemas/resolved.metadata.schema.json
- Tipo: schema
- Motivo: formalizar contrato do `resolved.metadata.json`
- RFs: RF35

6) specs/api/schemas/ui-gaps.report.schema.json
- Tipo: schema
- Motivo: formalizar contrato do `ui-gaps.report.json`
- RFs: RF36, RF38

7) specs/api/examples/resolved.feature.example.feature
- Tipo: example
- Motivo: exemplo canônico de `resolved.feature`
- RFs: RF34

8) specs/api/examples/resolved.metadata.example.json
- Tipo: example
- Motivo: exemplo canônico de `resolved.metadata.json`
- RFs: RF35

9) specs/api/examples/ui-gaps.report.example.json
- Tipo: example
- Motivo: exemplo canônico de `ui-gaps.report.json`
- RFs: RF36, RF38

10) specs/api/examples/ui-gaps.report.example.md
- Tipo: example
- Motivo: exemplo canônico de `.md` derivado do JSON
- RFs: RF36

11) specs/api/examples/README.md
- Tipo: other
- Motivo: indexar novos exemplos canônicos do Delta 3
- RFs: RF34–RF36

12) specs/tests/validation/free-hands-semantic-resolution.md
- Tipo: validation
- Motivo: adicionar gates normativos do Delta 3
- RFs: RF34–RF38
