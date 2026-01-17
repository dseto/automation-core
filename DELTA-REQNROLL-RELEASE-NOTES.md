# Delta Spec Pack — Migração SpecFlow -> Reqnroll (Automation Core)

## Motivação
Migrar do **SpecFlow** para **Reqnroll** (reboot/sucessor do SpecFlow), mantendo:
- .NET 8, xUnit, Selenium, Edge-only
- Gherkin + bindings BDD
- contratos do spec deck (SSOT) inalterados

## Escopo do delta
Substitui referências a SpecFlow por Reqnroll em:
- documentação (spec deck)
- dependências (NuGet)
- namespaces/atributos de bindings (Given/When/Then/Binding)
- integração com xUnit

## Como aplicar
1) Copie/sobrescreva os arquivos deste delta no repositório **na mesma estrutura de pastas**.
2) Rode:
```powershell
dotnet restore
dotnet build
dotnet test .\tests\Automation.Core.Tests\Automation.Core.Tests.csproj
```
3) Rode Validator (smoke):
```powershell
dotnet run --project .\src\Automation.Validator\Automation.Validator.csproj -- validate --ui-map .\samples\ui\ui-map.yaml --features .\samples\features
dotnet run --project .\src\Automation.Validator\Automation.Validator.csproj -- plan --ui-map .\samples\ui\ui-map.yaml --features .\samples\features
```
4) E2E (se usar):
```powershell
$env:RUN_E2E="true"
dotnet test .\tests\Automation.Acceptance.Tests\Automation.Acceptance.Tests.csproj
```

## Observações
- O projeto foi renomeado para `Automation.Reqnroll`.
- Migração principal:
  - pacotes `SpecFlow`/`SpecFlow.xUnit` -> `Reqnroll`/`Reqnroll.xUnit`
  - namespaces `TechTalk.SpecFlow` -> `Reqnroll`

## Checklist de aceite
- Build e testes unit verdes
- Bindings compilam com Reqnroll
- Validator continua operando (não depende de SpecFlow)
