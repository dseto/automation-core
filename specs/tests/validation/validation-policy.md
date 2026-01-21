# Validation Policy

> Base: `_legacy/07-validation-and-testing.md`

## Gates obrigatórios (PR / CI)
1. **Lint/format** dos specs (YAML/MD).
2. **Validação do ui-map**:
   - páginas/modais
   - elementos com `test_id`
3. **Validação do data-map**:
   - contexts por ambiente
   - datasets com strategy válida
4. **Validação do Gherkin**:
   - steps existentes no catálogo
   - parâmetros obrigatórios
5. **Smoke E2E** (opt-in por pipeline)
6. **Relatórios** gerados e anexados (quando aplicável)

## Conteúdo legado (referência)
# 07 - Validação de Contratos e Estratégia de Testes

## Validação de Contratos (Shift-Left)

### Automation.Validator
Ferramenta CLI que valida integridade de contratos antes da execução. Executa em CI/CD como pré-flight check.

**Comandos Disponíveis:**
- `validate`: Valida UiMap, DataMap e Feature Files
- `doctor`: Diagnóstico de problemas comuns no projeto
- `plan`: Planejar implementação de automação

**Exemplo de Uso:**
```bash
# Validação completa
automation-validator validate --ui-map ui-map.yaml --data-map data-map.yaml --features features/

# Diagnóstico do projeto
automation-validator doctor --path .

# Saída em JSON para CI/CD
automation-validator validate -u ui-map.yaml -d data-map.yaml -f features/ --json
```

### Checagens de UiMap
- Todas as páginas têm pelo menos um elemento
- Todos os testIds são únicos por página
- Todas as rotas são válidas (começam com "/")
- Não há elementos órfãos (sem página pai)
- **Novo:** Validação de Anchor Pattern (páginas com rotas ambíguas devem ter anchor)
- **Novo:** Detecção de SPAs e modais que precisam de anchor

### Checagens de DataMap
- Todos os contextos têm pelo menos um objeto de dados
- Todas as chaves de datasets são únicas
- Não há referências circulares (A referencia B que referencia A)
- Todos os valores são strings ou dicionários válidos
- **Novo:** Validação de Sintaxe Explícita (@objeto, {{dataset}}, ${envvar})
- **Novo:** Detecção de ambiguidade entre literais e referências

### Checagens de Gherkin
- Todas as páginas referenciadas existem no UiMap
- Todos os elementos referenciados existem na página
- Todas as chaves de dados existem no DataMap
- Não há steps desconhecidos (fora do catálogo)
- **Novo:** Validação de prefixos de sintaxe explícita (@, {{}}, ${})
- **Novo:** Detecção de referências inválidas ou ambíguas

### Execução
```bash
automation-validator validate --ui-map ui-map.yaml --data-map data-map.yaml --features features/
```

## Estratégia de Testes

### Pirâmide de Testes
A base da pirâmide são testes unitários do Core (validação de resolvers). O meio são testes de integração (Gherkin contra aplicação piloto). O topo são testes de fumaça em produção (validação de fluxos críticos).

### Tags para Organização
`@smoke`: Testes rápidos que cobrem fluxos críticos. `@regressao`: Testes que rodam em toda madrugada. `@positive`: Testes de sucesso. `@negative`: Testes de erro. `@ignore`: Testes em desenvolvimento (pulados).

### Execução Filtrada
```bash
# Apenas testes de fumaça
dotnet test --filter "Category=smoke"

# Apenas testes positivos
dotnet test --filter "Category=positive"

# Tudo exceto @ignore
dotnet test --filter "Category!=ignore"
```

## Critérios de Qualidade

### Cobertura Mínima
Cada página deve ter pelo menos um teste de sucesso. Cada fluxo crítico deve ter um teste positivo e um negativo. Cada elemento deve ser validado em pelo menos um teste.

### Tempo de Execução
Testes de fumaça devem rodar em menos de 5 minutos. Testes de regressão devem rodar em menos de 30 minutos. Se exceder, considere paralelização ou otimização.

### Taxa de Falha
Taxa de falha flaky (intermitente) deve ser menor que 2%. Se exceder, investigar waits e estabilidade. Falhas reais devem ser corrigidas no mesmo dia.

## Integração com CI/CD

### Pipeline Recomendado
1. **Checkout:** Clonar repositório.
2. **Restore:** `dotnet restore`.
3. **Validate:** `automation-validator validate`.
4. **Build:** `dotnet build`.
5. **Test:** `dotnet test --filter "Category=smoke"`.
6. **Report:** Gerar relatório de cobertura.

### Variáveis de Ambiente
`BASE_URL`: URL da aplicação. `BROWSER`: "chrome" ou "edge". `HEADLESS`: "true" ou "false". `ENVIRONMENT`: "default", "homolog", "prod".

### Artefatos
Logs de teste devem ser armazenados em `bin/Debug/net8.0/Logs/`. Screenshots em falha em `bin/Debug/net8.0/Evidence/`.

## Validação por JSON Schema
- UiMap: `api/schemas/uimap.schema.json`
- DataMap: `api/schemas/datamap.schema.json`

## Validação do Catálogo de Steps
- Fonte: `tests/gherkin/step-catalog.yaml`
- Regras: `tests/validation/step-catalog-validation.md`

## Nota sobre o Validator atual
- As regras em `tests/validation/*` refletem o código do `Automation.Validator`.
- A validação de steps em Gherkin atualmente usa `KnownStepPatterns` (hardcoded), não o `step-catalog.yaml`.
