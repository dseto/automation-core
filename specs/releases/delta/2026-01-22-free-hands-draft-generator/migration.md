# Delta 2 — Draft Generator
## Migration Guide

O delta exige ajustes no Recorder (Browser Capture) para garantir a qualidade dos `target.hint` gerados.

### Passos obrigatórios
1. Priorizar `data-testid` ao gerar `target.hint`.
2. Evitar ids dinâmicos (`mat-*`, `cdk-*`) como seletores.
3. Gerar fallback humano útil quando não houver `data-testid`.

### Caso canônico
```html
<input data-testid="page.login.username" id="mat-input-0">
```

Resultado esperado:

```
[target.hint]: [data-testid='page.login.username']
```
