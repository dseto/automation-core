# Changes — 2026-01-20

## Fixed
- Testes duplicados (Reqnroll não é mais test project)
- Resolução incorreta de datasets
- Re-resolução indevida de valores literais
- Incompatibilidade de ui-map.yaml
- Erro NU1008 de build
- Conflito de EdgeDriver

## Changed
- GetTestIdOrThrow agora aceita testId e test_id
- DataResolver passa a receber valor completo
- Removida dependência de xUnit em Automation.Reqnroll
- Selenium Manager passa a ser padrão

## Removed
- PAUSE_EACH_STEP
- PAUSE_TIMEOUT_MS
- Selenium.WebDriver.MSEdgeDriver

## Added
- AssertHelper interno
- Método PreencherCampoComValorLiteral
- Padrão de debug visual via env vars

## Technical Notes
- Central Package Version Management passou a ser a única fonte de versão.