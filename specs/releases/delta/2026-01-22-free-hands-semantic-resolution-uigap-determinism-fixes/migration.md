# migration — como aplicar

## Aplicação
1. Sobrescreva os arquivos do patch na raiz do repositório (mantendo o prefixo `specs/`).
2. Garanta que o runtime leia `UI_MAP_PATH` (ou `UIMAP_PATH` como alias).
3. Para habilitar o pipeline:
   - defina `SEMRES_ENABLED=true`
   - opcional: ajuste `SEMRES_MAX_CANDIDATES` e `SEMRES_OUTPUT_DIR`

## Compatibilidade
- `UI_MAP_PATH` permanece canônico.
- `UIMAP_PATH` é aceito como alias (não-breaking).
