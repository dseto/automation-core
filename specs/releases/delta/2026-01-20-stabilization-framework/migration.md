# Migration Guide — 2026-01-20

## Quando aplicar
Se você:
- usava PAUSE_EACH_STEP
- dependia de xUnit dentro de Automation.Reqnroll
- possui ui-map.yaml legado
- possui scripts de setup de driver

## O que quebrou
- PAUSE_EACH_STEP e PAUSE_TIMEOUT_MS removidos
- Automation.Reqnroll não expõe mais Xunit.Assert
- Driver Edge não é mais gerenciado manualmente

## Como migrar

### 1. Debug
Remover PAUSE_EACH_STEP e usar:
- SLOWMO_MS
- HIGHLIGHT
- Breakpoints no VS Code

### 2. Assert
Substituir:
Xunit.Assert → Automation.Reqnroll.Helpers.Assert

### 3. UI Map
Ambos funcionam:

Novo:
username:
  testId: page.login.username

Legado:
username:
  test_id: page.login.username

### 4. Ambiente
- Remover C:\selenium\edgedriver_win64 do PATH
- Garantir Selenium Manager ativo

## Checklist
- [x] Build ok
- [x] Cenários sem duplicação
- [x] Debug visual validado
- [x] Specs atualizadas