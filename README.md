
# Automation Core — Spec Deck + Pack de Implementação (MVP)

**Stack:** C# + .NET 8 • **Reqnroll** • xUnit • Selenium WebDriver • Microsoft Edge • Windows • Azure DevOps Server 2020 (on-prem)

Módulo BDD: `src/Automation.Reqnroll` (bindings/hooks/steps).

## Comandos rápidos
```powershell
dotnet restore
dotnet build
dotnet test .\tests\Automation.Core.Tests\Automation.Core.Tests.csproj

dotnet run --project .\src\Automation.Validator\Automation.Validator.csproj -- doctor
dotnet run --project .\src\Automation.Validator\Automation.Validator.csproj -- validate --ui-map .\samples\ui\ui-map.yaml --features .\samples\features
dotnet run --project .\src\Automation.Validator\Automation.Validator.csproj -- plan --ui-map .\samples\ui\ui-map.yaml --features .\samples\features
```
