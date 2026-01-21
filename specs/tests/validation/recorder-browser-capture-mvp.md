
# Validation — Browser Capture MVP

## Manual smoke test (obrigatório)
1) Iniciar modo exploratório (`AUTOMATION_RECORD=true`) sem `.feature`
2) Interagir manualmente:
   - navegar (pelo menos 1 mudança de rota)
   - preencher 2 campos
   - clicar 2 vezes
   - submeter
3) Encerrar

**Esperado:** session.json contém eventos click e fill (não apenas navigate).

## Contract checks
- JS injeta `window.__fhRecorder`
- Polling retorna eventos
- Mapper gera tipos aceitos pelo schema
