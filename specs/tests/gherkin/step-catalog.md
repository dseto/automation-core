# Step Catalog (Normativo)

> Base: `_legacy/05-step-catalog.md` + steps em `Automation.Reqnroll/Steps/*`.

## Objetivo
Este catálogo define quais steps são **contrato** (públicos) e sua semântica.

## Conteúdo legado (referência)
# 05 - Catálogo de Steps Genéricos

## Princípio
Todos os steps são genéricos e reutilizáveis. Não devem conter lógica específica de aplicação. A lógica reside nos contratos (UiMap, DataMap).

## NavigationSteps

| Step | Descrição |
|------|-----------|
| `Dado que a aplicação está em "${BASE_URL}"` | Define a URL base da aplicação (variável de ambiente). |
| `Dado que estou na página "{pageName}"` | Navega para a página especificada. Valida que a rota está correta. |
| `Quando eu aguardo a rota "{route}"` | Aguarda que a URL contenha a substring especificada. |

## InteractionSteps

| Step | Descrição |
|------|-----------|
| `Quando eu preencho "{element}" com "{value}"` | Preenche um campo de texto com o valor (literal ou referência de dados). |
| `Quando eu preencho os campos com os dados de "{dataKey}"` | Preenche múltiplos campos usando um objeto do DataMap. Mapeia automaticamente chaves para elementos. |
| `Quando eu clico em "{element}"` | Clica em um elemento. |
| `Quando eu clico em "{element}" e aguardo a rota "{route}"` | Clica e aguarda mudança de rota. |
| `Quando eu limpo o campo "{element}"` | Limpa o conteúdo de um campo de texto. |
| `Quando eu seleciono "{value}" em "{element}"` | Seleciona uma opção em um dropdown. |
| `Quando eu executo o script JS "{script}"` | Executa JavaScript genérico no contexto da página. |
| `Quando eu executo o script "{script}" no elemento "{element}"` | Executa JavaScript com o elemento como `arguments[0]`. |

## ValidationSteps

| Step | Descrição |
|------|-----------|
| `Então estou na página "{pageName}"` | Valida que a página atual é a esperada. |
| `Então a rota deve ser "{route}"` | Valida que a URL contém a substring. |
| `Então o elemento "{element}" deve estar visível` | Valida que o elemento está visível. |
| `Então o elemento "{element}" não deve estar visível` | Valida que o elemento não está visível. |
| `Então o elemento "{element}" deve conter "{text}"` | Valida que o texto do elemento contém a substring. |
| `Então o atributo "{attribute}" de "{element}" deve ser "{value}"` | Valida que um atributo do elemento tem o valor esperado. |
| `Então o elemento "{element}" deve estar habilitado` | Valida que o elemento não está desabilitado. |

## Escape Hatch (JavaScript)

Para casos complexos não cobertos pelos steps genéricos, use:

```gherkin
Quando eu executo o script JS "localStorage.clear()"
Quando eu executo o script "arguments[0].scrollIntoView()" no elemento "submit"
```

O script tem acesso ao elemento como `arguments[0]`. Use com moderação.

## Extensão de Steps

Se um step não existir, **não crie um novo**. Em vez disso:
1. Verifique se o Escape Hatch resolve o problema.
2. Se não, proponha o novo step para o Core via Pull Request.
3. Após aprovação, o step fica disponível para todos os 100+ squads.

Isso garante reutilização máxima e evita fragmentação.

## Diretrizes de compatibilidade
- Alterar texto/regex do step é potencial breaking change.
- Steps devem aceitar referências `element` e `page.element` quando aplicável.

## Contrato machine-readable
- `tests/gherkin/step-catalog.yaml` é a versão parseável (usada por validação automatizada).
