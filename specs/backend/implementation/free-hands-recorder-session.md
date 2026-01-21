
# FREE-HANDS Recorder — Session Log (Fase 1)

## Fonte de requisitos (SSOT)
Implementa RF00–RF06 conforme:
`specs/backend/requirements/free-hands-recorder-session.md`

---

## Fluxo de inicialização (MANDATÓRIO)

### Modo exploratório (Recorder)
Quando `AUTOMATION_RECORD=true`, o runtime DEVE:
1) iniciar browser e anexar hooks do Recorder  
2) navegar para a URL inicial (se configurado)  
3) **aguardar interação manual do usuário** (sem carregar `.feature`, sem executar steps)  
4) encerrar (fechar browser / comando stop)  
5) persistir `session.json` em `RECORD_OUTPUT_DIR`  
6) encerrar processo com sucesso (exit 0), salvo erro técnico

### Modo normal (tests)
Quando `AUTOMATION_RECORD=false`, segue o fluxo normal de testes (Reqnroll/Gherkin).

---

## Não permitido neste delta
- Requerer `.feature` para iniciar Recorder
- Executar steps durante Recorder
