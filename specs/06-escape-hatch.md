# 06 - Escape Hatch: Execução de JavaScript

## Propósito
O Escape Hatch permite executar JavaScript arbitrário quando os steps genéricos não cobrem um caso. Mantém o "Zero Código" porque não requer C#, apenas Gherkin.

## Casos de Uso
Manipulação de localStorage/sessionStorage, Disparo de eventos customizados, Modificação direta de DOM, Execução de funções globais da aplicação.

## Sintaxe

### Execução Global
```gherkin
Quando eu executo o script JS "localStorage.clear()"
Quando eu executo o script JS "window.myCustomFunction()"
```

O script é executado no contexto da janela. Tem acesso a `window`, `document`, `localStorage`, etc.

### Execução em Elemento
```gherkin
Quando eu executo o script "arguments[0].click()" no elemento "submit"
Quando eu executo o script "arguments[0].value = 'novo valor'" no elemento "username"
```

O elemento é passado como `arguments[0]`. Útil para interações que o Selenium não consegue fazer nativamente.

## Limitações
O script é executado de forma síncrona. Se precisar de async, use `Promise` ou `async/await`. O script tem acesso apenas ao contexto da página, não a variáveis do teste. Erros no script causam falha do teste.

## Boas Práticas
Use Escape Hatch com moderação. Prefira sempre os steps genéricos. Se usar frequentemente, considere propor um novo step para o Core. Documente o motivo da execução de JS nos comentários do Gherkin.

## Exemplos

### Limpar Dados Locais
```gherkin
Quando eu executo o script JS "localStorage.clear()"
E eu executo o script JS "sessionStorage.clear()"
```

### Disparar Evento Customizado
```gherkin
Quando eu executo o script JS "document.dispatchEvent(new Event('custom-event'))"
```

### Modificar Atributo Diretamente
```gherkin
Quando eu executo o script "arguments[0].setAttribute('disabled', 'false')" no elemento "submit"
```

## Segurança
O Escape Hatch não deve ser usado para contornar autenticação ou segurança. Se precisar fazer isso, é um sinal de que o teste está mal projetado. Sempre valide que o script é seguro antes de usar em produção.
