# Delta Pack — 2026-01-20 — Estabilização do Framework e Debug Visual

**Status:** RELEASED  
**Versão sugerida:** v0.3.0  
**Data de fechamento:** 2026-01-21

## Objetivo
Estabilizar o framework de automação após execução real em ambiente local, corrigindo problemas de build, runtime, compatibilidade de specs e melhorando a experiência de debug visual.

## Tipo de mudança
- [x] Runtime (Core / Reqnroll)
- [x] Validator / governança implícita
- [x] Contrato (ui-map / datamap)
- [x] Documentação / governança

## Escopo
- Correção de testes duplicados
- Correção de resolução de valores literais
- Correção de parsing de datasets
- Compatibilidade de ui-map.yaml
- Build e drivers
- Novo padrão de debug visual

## Impacto
- [ ] Não breaking
- [x] Breaking change (ver migration.md)

## Como validar
- Build compila sem erros
- 1 cenário = 1 teste
- {{dataset}} resolve corretamente
- Valores com @ funcionam
- ui-map com testId funciona
- Debug visual com SLOWMO + highlight