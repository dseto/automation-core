---
agent: spec-driven-implementer
---
Implemente o plano etapa a etapa.

Regras:
- Em cada etapa: faça as mudanças mínimas necessárias, depois rode:
  dotnet restore
  dotnet build
  dotnet test
- Se algo falhar, corrija antes de seguir.
- Garanta compatibilidade com ui-map usando testId e test_id.
- Remova o que foi removido no delta pack (PAUSE_EACH_STEP, PAUSE_TIMEOUT_MS, MSEdgeDriver).
- Garanta que Automation.Reqnroll NÃO é test project e não depende de Xunit.
- Garanta que dataset {{...}} e literais com @ não sejam reprocessados.
- No final, liste todos os arquivos alterados e confirme os comandos e resultados.

Comece pela etapa 1 do plano.
