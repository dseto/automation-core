
# Migration

## Decision Table

| session.json contains route | Behavior |
|-----------------------------|----------|
| Yes | Use route directly |
| No | Emit UIGAP_ROUTE_NOT_MAPPED (info) |
| Legacy format | Accepted without inference |

## Compatibility
- Esta entrega é **compatível** com versões anteriores: não introduz breaking changes.
- Não há necessidade de migração para consumidores existentes; comportamentos de fallback (quando `route` faltante) são explícitos e documentados.
