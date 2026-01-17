# 05 — Runtime Resolution

- PageContext é definido preferencialmente por step explícito
- FriendlyName -> TestId -> CSS locator
- Locator default: `[data-testid='{testId}']`
- Ambiguidade deve falhar com mensagem clara
