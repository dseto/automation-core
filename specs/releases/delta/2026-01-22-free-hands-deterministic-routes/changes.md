
# Changes

## Summary
Entrega normativa que torna determinístico o tratamento de rotas no pipeline FREE-HANDS (Recorder → Draft Generator → Semantic Resolver).

## Implementado
- Recorder: contrato de `session.json` com campos `url`, `pathname`, `fragment` e `route` (normalização determinística).
- RouteNormalizer: nova utilidade para canonicalizar rotas gravadas (remove caminhos locais, preserva fragmentos, respeita BASE_URL).
- Draft Generator: utiliza rota normalizada e aplica sanitização para emitir navegações válidas em uma única linha (evita quebras de Gherkin).
- Semantic Resolver: regras determinísticas de mapeamento de rota, preferência por rota do evento de sessão quando disponível, emissão de `UIGAP_ROUTE_NOT_MAPPED` como info para rotas não mapeadas, reescrita de steps de navegação para `pageKey` e inserção de passo `BASE_URL` quando necessário.
- Correções: prevenção de substituições agressivas que trocavam literais de preenchimento por referências de elemento; correção no alinhamento de EventIndexes do `ActionGrouper`.
- Testes: novos testes unitários cobrindo normalização de rota, geração de draft, resolução semântica e preservação de literais.

## Impacto
- Não é breaking: compatibilidade com sessões legadas mantida (comportamento de fallback definido em `migration.md`).

## Status
- Finalizado: alterações implementadas, testadas e validadas localmente.
