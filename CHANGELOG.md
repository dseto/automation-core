# Changelog
- 0.1.0: bootstrap do Core + spec deck + implementação MVP inicial
- v0.5.0 (2026-01-21): Adiciona Draft Generator (session.json → draft.feature), escape hatch automático, preservação de rawAction e metadata; RF11/RF13/RF15 implementados.
- v0.5.1 (2026-01-21): Corrige interpretação de literais no DataMap: valores que começam com '@' agora são tratados como literais quando não existe uma referência de contexto correspondente. Adicionado testes unitários para o `DataResolver`. ✅
- v0.5.2 (2026-01-22): Correções e melhorias no Draft Generator / Semantic Resolver: tratar `data-testid` com pontos como identificadores específicos, unificação das heurísticas de normalização (`HintHelpers`), evitar falsos `UI_MAP_KEY_NOT_FOUND` ao resolver testIds, e adição de testes de integração e agrupamento.
- v0.6.0 (2026-01-21): Recorder emits `waitMs` for gaps above configured threshold; Draft Generator materializes waits as `E eu espero <segundos> segundos`; Step catalog accepts decimal waits. Added unit tests for `SessionRecorder` and integration validation for draft generation. ✅
