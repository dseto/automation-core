# Implementation Guide (Backend)

> Base: `_legacy/09-implementation-guide.md`

## Organização
- `backend/rules/` — normas de comportamento (SSOT técnico).
- `backend/implementation/` — como implementar/estender o código respeitando as regras.

## Conteúdo legado (referência)
# 09 - Guia de Implementação

## Pré-requisitos
.NET 8.0 SDK instalado. Chrome ou Edge navegador. Conhecimento básico de Gherkin (PT-BR). Acesso ao repositório da plataforma.

## Passo 1: Criar Novo Projeto de Testes
```bash
dotnet new classlib -n MeuApp.UiTests
cd MeuApp.UiTests
```

## Passo 2: Adicionar Referência ao Automation.Reqnroll
```bash
dotnet add package Automation.Reqnroll --version 2.0.0
```

## Passo 3: Criar Estrutura de Diretórios
```
MeuApp.UiTests/
├── features/
│   └── exemplo.feature
├── ui/
│   └── ui-map.yaml
├── data/
│   └── data-map.yaml
├── reqnroll.json
└── MeuApp.UiTests.csproj
```

## Passo 4: Configurar reqnroll.json
```json
{
  "language": "pt-BR",
  "stepAssemblies": [
    {
      "assembly": "Automation.Reqnroll"
    }
  ]
}
```

## Passo 5: Criar UiMap
Mapear todos os elementos da aplicação no `ui/ui-map.yaml`:
```yaml
pages:
  login:
    __meta:
      route: /login
      anchor: page.login
    username:
      testId: page.login.username
    password:
      testId: page.login.password
```

## Passo 6: Criar DataMap
Definir dados de teste no `data/data-map.yaml`:
```yaml
contexts:
  default:
    user_admin:
      username: "admin"
      password: "senha123"
```

## Passo 7: Criar Feature File
Escrever testes em `features/exemplo.feature`:
```gherkin
#language: pt-BR
@smoke
Funcionalidade: Login

  Contexto:
    Dado que a aplicação está em "${BASE_URL}"

  Cenário: Login com sucesso
    Dado que estou na página "login"
    Quando eu preencho os campos com os dados de "user_admin"
    E eu clico em "submit" e aguardo a rota "/dashboard"
    Então estou na página "dashboard"
```

## Passo 8: Configurar .csproj
Adicionar cópia de arquivos YAML:
```xml
<ItemGroup>
  <None Update="ui/ui-map.yaml">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Update="data/data-map.yaml">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

## Passo 9: Executar Testes
```bash
# Definir variáveis de ambiente
export BASE_URL="https://minha-app.com"
export BROWSER="chrome"
export HEADLESS="true"

# Executar testes
dotnet test
```

## Passo 10: Validar Contratos (Opcional)
```bash
automation-validator validate \
  --ui-map ui/ui-map.yaml \
  --data-map data/data-map.yaml \
  --features features/
```

## Checklist de Implementação
- ✅ Projeto criado com referência ao Automation.Reqnroll.
- ✅ Estrutura de diretórios criada.
- ✅ reqnroll.json configurado.
- ✅ UiMap mapeado (todas as páginas e elementos).
- ✅ DataMap criado (contextos e datasets).
- ✅ Feature files escritos (cobrindo fluxos críticos).
- ✅ .csproj configurado para copiar YAML.
- ✅ Testes executados com sucesso.
- ✅ Contratos validados.

## Tempo Estimado
Seguindo este guia, a implementação deve levar **30 minutos** para uma aplicação simples (5-10 páginas).

## Suporte
Se encontrar problemas, consulte o arquivo `10-troubleshooting.md`.
