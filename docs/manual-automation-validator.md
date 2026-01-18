# Manual de Uso — Automation.Validator (UiMap Validator)

## 1. Objetivo do Automation.Validator
O Automation.Validator é uma ferramenta de linha de comando (CLI) responsável por validar o ui-map.yaml antes da execução dos testes.

Ele garante que:
- A estrutura do YAML está correta
- Não há erros comuns de modelagem (ex: pages duplicado)
- Todas as páginas possuem elementos válidos
- O Automation.Core não irá falhar em runtime por erro de configuração

Importante: o Validator não executa testes, apenas valida configuração.

## 2. Quando usar o Validator
Use o Automation.Validator sempre que:
- Um ui-map.yaml for criado ou alterado
- Um novo frontend for preparado para automação
- Antes de commitar mudanças no ui-map.yaml
- Em pipelines CI (opcional, mas recomendado)

## 3. Estrutura esperada do ui-map.yaml
```yaml
pages:
  login:
    username:
      testId: login-username
    password:
      testId: login-password
    submit:
      testId: login-submit

  dashboard:
    anchor:
      testId: dashboard-anchor
    stat-total:
      testId: dashboard-stat-total
```

Regras estruturais:
- Deve existir a chave raiz pages
- pages deve ser um dicionário
- Cada página deve conter ao menos um elemento
- Cada elemento deve possuir testId
- Estruturas como pages.pages.login são inválidas

## 4. Executando o Validator localmente
### Pré-requisitos
- .NET SDK 8
- Projeto automation-core compilado
- Arquivo ui-map.yaml existente

### Execução
```bash
dotnet run --project src/Automation.Validator -- --ui-map "C:\Projetos\metrics-simple-frontend\ui-tests\ui-map.yaml"
```

## 5. Saídas
### Sucesso
Exit code 0, com mensagens de validação concluída.

### Erro
Exit code diferente de 0, com descrição clara do problema e caminho do arquivo.

## 6. Integração com CI
Pode ser usado como gate em pipelines para evitar que configurações inválidas cheguem à execução de testes.

## 7. Relação com Automation.Core
O Validator antecipa erros que apareceriam apenas em runtime no Automation.Core + Reqnroll, garantindo fail-fast.

## 8. Boas práticas
- Rodar o Validator antes de dotnet test
- Commitar apenas ui-map.yaml válido
- Usar como gate em PRs

## 9. O que o Validator não faz
- Não valida DOM
- Não abre browser
- Não executa testes
