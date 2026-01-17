
# Delta Spec Pack — Migração LIMPA para Reqnroll (rename do módulo BDD)

Este delta realiza a migração “limpa”:
- Módulo `src/Automation.Reqnroll` como bindings BDD
- Namespaces e referências atualizados
- Docs/specs e instruções do Copilot atualizados
- Restante do Core inalterado

> Este delta inclui um arquivo `DELETE.md` com a lista de remoções.

## Como aplicar
1) Copie/sobrescreva os arquivos deste delta no repo (mesmos paths).
2) Delete os itens listados em `DELETE.md`.
3) Rode:
```powershell
dotnet restore
dotnet build
dotnet test .\tests\Automation.Core.Tests\Automation.Core.Tests.csproj
dotnet run --project .\src\Automation.Validator\Automation.Validator.csproj -- validate --ui-map .\samples\ui\ui-map.yaml --features .\samples\features
```
