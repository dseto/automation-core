# Automation.Validator

CLI para validação de contratos (UiMap, DataMap, Gherkin) da plataforma de automação de testes.

## Instalação

```bash
dotnet tool install --global Automation.Validator
```

Ou compilar localmente:
```bash
dotnet build
dotnet run
```

## Comandos

### validate
Valida integridade de contratos antes da execução.

```bash
automation-validator validate \
  --ui-map ui/ui-map.yaml \
  --data-map data/data-map.yaml \
  --features features/
```

**Opções:**
- `--ui-map, -u`: Caminho para o arquivo ui-map.yaml
- `--data-map, -d`: Caminho para o arquivo data-map.yaml
- `--features, -f`: Caminho para o diretório de features
- `--json, -j`: Gerar saída em JSON

**Checagens:**
- UiMap: Schema, unicidade de testIds, rotas válidas
- DataMap: Contextos, datasets, estratégias válidas
- Gherkin: Steps conhecidos, páginas existentes, elementos mapeados

**Exemplo de saída:**
```
============ VALIDAÇÃO DE CONTRATOS ============

✗ 2 erro(s) encontrado(s):

  [GHERKIN_ELEMENT_NOT_FOUND] [features/login.feature:5]
  → Elemento 'submit-btn' não encontrado na página 'login'

  [DATAMAP_NO_DEFAULT_CONTEXT] [data/data-map.yaml]
  → DataMap deve conter um contexto 'default'

⚠ 1 aviso(s):

  [UIMAP_PAGE_NO_ELEMENTS] [ui/ui-map.yaml]
  → Página 'dashboard' não contém nenhum elemento
```

### doctor
Diagnóstico de problemas comuns na estrutura do projeto.

```bash
automation-validator doctor --path .
```

**Verifica:**
- Existência de diretórios (features/, ui/, data/)
- Existência de arquivos (ui-map.yaml, data-map.yaml, reqnroll.json)
- Existência de arquivo .csproj

**Exemplo de saída:**
```
🔍 Executando diagnóstico...

✓ Diretório 'features/' existe
✓ Arquivo 'ui/ui-map.yaml' existe
✓ Arquivo 'data/data-map.yaml' existe
✗ Arquivo 'reqnroll.json' existe
✓ Arquivo '.csproj' existe

✗ Corrija os problemas acima
```

### plan
Gera plano de implementação para nova aplicação.

```bash
automation-validator plan --url https://minha-app.com
```

**Saída:**
```
📋 Plano de Implementação para https://minha-app.com

Passos recomendados:
1. Mapear todas as páginas da aplicação
2. Identificar elementos interativos
3. Criar ui-map.yaml
4. Definir dados de teste em data-map.yaml
5. Escrever cenários em Gherkin
6. Executar validação
7. Rodar testes
```

## Integração com CI/CD

### GitHub Actions
```yaml
- name: Validate Contracts
  run: |
    automation-validator validate \
      --ui-map ui/ui-map.yaml \
      --data-map data/data-map.yaml \
      --features features/ \
      --json > validation-report.json
```

### Azure Pipelines
```yaml
- task: PowerShell@2
  inputs:
    targetType: 'inline'
    script: |
      automation-validator validate `
        --ui-map ui/ui-map.yaml `
        --data-map data/data-map.yaml `
        --features features/
```

## Códigos de Erro

### UiMap
- `UIMAP_EMPTY`: UiMap não contém nenhuma página
- `UIMAP_PAGE_NO_ROUTE`: Página não possui rota
- `UIMAP_INVALID_ROUTE`: Rota não começa com "/"
- `UIMAP_ELEMENT_NO_TESTID`: Elemento não possui testId
- `UIMAP_DUPLICATE_TESTID`: TestId duplicado na página
- `UIMAP_DUPLICATE_ROUTE`: Rota mapeada para múltiplas páginas

### DataMap
- `DATAMAP_NO_CONTEXTS`: DataMap não contém contextos
- `DATAMAP_NO_DEFAULT_CONTEXT`: Falta contexto "default"
- `DATAMAP_INVALID_CONTEXT`: Contexto não é um dicionário válido
- `DATAMAP_EMPTY_DATASET`: Dataset não contém itens
- `DATAMAP_INVALID_STRATEGY`: Estratégia inválida (válidas: sequential, random, unique)

### Gherkin
- `GHERKIN_PAGE_NOT_FOUND`: Página referenciada não existe no UiMap
- `GHERKIN_ELEMENT_NOT_FOUND`: Elemento não existe na página
- `GHERKIN_DATASET_NOT_FOUND`: Dataset não existe no DataMap
- `GHERKIN_DATA_KEY_NOT_FOUND`: Chave de dados não existe
- `GHERKIN_UNKNOWN_STEP`: Step não está no catálogo

## Boas Práticas

1. **Executar antes de cada commit:** Adicione um pre-commit hook
   ```bash
   #!/bin/bash
   automation-validator validate --ui-map ui/ui-map.yaml --data-map data/data-map.yaml --features features/
   ```

2. **Executar em CI/CD:** Valide contratos antes de rodar testes

3. **Usar em desenvolvimento:** Rode `doctor` regularmente para verificar saúde do projeto

4. **Revisar avisos:** Avisos não bloqueiam, mas indicam problemas potenciais

## Troubleshooting

### "UiMap não encontrado"
Verifique o caminho do arquivo. Use caminhos relativos ao diretório atual.

### "Feature file não encontrado"
Verifique que o diretório contém arquivos `.feature`. Use `--features features/` para o diretório.

### "Validação passou mas testes falham"
Pode haver problemas em runtime (timing, elementos dinâmicos). Use `--json` para análise detalhada.

## Contribuindo

Para reportar bugs ou sugerir melhorias, abra uma issue no repositório da plataforma.
