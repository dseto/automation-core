# 10 - Troubleshooting

## Problemas Comuns

### "Element not found in UiMap"
**Causa:** O elemento referenciado no Gherkin não existe no `ui-map.yaml` para a página atual.

**Solução:** Verifique que o elemento está mapeado no UiMap. Verifique que o nome do elemento está correto (case-sensitive). Use `automation-validator` para validar o UiMap.

### "Timed out after 20 seconds"
**Causa:** O WaitService aguardou mais de 20 segundos por uma ação (navegação, visibilidade, etc.).

**Solução:** Verifique que a aplicação está respondendo. Aumente o timeout se necessário (via `RunSettings.DefaultTimeoutMs`). Verifique que o elemento está realmente presente no DOM. Considere usar Escape Hatch para debug.

### "Data key not found in DataMap"
**Causa:** A chave de dados referenciada no Gherkin não existe no `data-map.yaml`.

**Solução:** Verifique que a chave existe no contexto correto. Verifique que o contexto está sendo selecionado corretamente (via `RunSettings.Environment`). Use `automation-validator` para validar o DataMap.

### "Page not found in UiMap"
**Causa:** A página referenciada no Gherkin não existe no `ui-map.yaml`.

**Solução:** Verifique que a página está mapeada no UiMap. Verifique que o nome da página está correto. Verifique que a rota da página está correta.

### "Teste passa localmente mas falha em CI/CD"
**Causa:** Diferença de ambiente (URL, dados, navegador, timing).

**Solução:** Verifique que as variáveis de ambiente estão configuradas corretamente em CI/CD. Verifique que o DataMap tem contextos para cada ambiente. Aumente o timeout em CI/CD (headless é mais lento). Verifique que o navegador está instalado em CI/CD.

### "Teste flaky (passa e falha aleatoriamente)"
**Causa:** Timing insuficiente, elemento não está pronto, ou aplicação é instável.

**Solução:** Aumente o timeout do WaitService. Use `Quando eu aguardo a rota` após navegação. Verifique que a aplicação está estável. Considere usar `slowmo` para debug (ex: `export SLOWMO_MS=1000`).

## Debug

### Modo Verbose
```bash
export HEADLESS=false
export SLOWMO_MS=2000
dotnet test --verbosity normal
```

Isso abre o navegador, executa lentamente e mostra logs detalhados.

### Captura de Screenshots
O RuntimeHooks captura screenshots automaticamente em falha. Verifique em `bin/Debug/net8.0/Evidence/`.

### Logs
Logs são salvos em `bin/Debug/net8.0/Logs/`. Procure por mensagens de erro ou warning.

### Escape Hatch para Debug
```gherkin
Quando eu executo o script JS "console.log(document.body.innerHTML)"
```

Isso imprime o HTML da página no console do navegador.

## Performance

### Paralelização
Por padrão, os testes rodam sequencialmente. Para paralelizar:
```bash
dotnet test --parallel
```

Cuidado: Isso pode causar conflito de dados se não estiver isolado.

### Otimização de Waits
Reduza timeouts para testes que não precisam de wait longo:
```csharp
// No InteractionSteps, passe timeout customizado
await _waitService.WaitForUrlContains(driver, "/dashboard", 5000);
```

## Contato e Escalação
Se o problema persistir após seguir este guia, abra uma issue no repositório da plataforma com:
1. Descrição do problema.
2. Logs completos.
3. Screenshot (se aplicável).
4. Versão da plataforma (`dotnet package list`).
5. Ambiente (Windows/Linux, navegador, .NET version).
