
# Automation Core — Spec Deck + Pack de Implementação (MVP)

**Stack:** C# + .NET 8 • **Reqnroll** • xUnit • Selenium WebDriver • Microsoft Edge • Windows • Azure DevOps Server 2020 (on-prem)

Módulo BDD: `src/Automation.Reqnroll` (bindings/hooks/steps).

## 🆕 FREE-HANDS Recorder (RF00)

**NEW:** Modo exploratório para gravação de interações manualmente.

```powershell
# Iniciar recorder (sem .feature, sem teste)
cd ui-tests/scripts
. .\\_env.ps1
.\run-free-hands.ps1 -Url "https://app.com"

# Resultado: session.json gerado
# Documentação: docs/qa-wiki/06-RECORDER-GUIDE.md
# Audit: specs/releases/delta/2026-01-21-free-hands-recorder-exploratory-mode/RF00-COMPLIANCE-AUDIT.md
```

**Status:** ✅ RF00 (Exploratory Mode) fully implemented and verified.

---

## Comandos rápidos
```powershell
dotnet restore
dotnet build
dotnet test .\tests\Automation.Core.Tests\Automation.Core.Tests.csproj

dotnet run --project .\src\Automation.Validator\Automation.Validator.csproj -- doctor
dotnet run --project .\src\Automation.Validator\Automation.Validator.csproj -- validate --ui-map .\samples\ui\ui-map.yaml --features .\samples\features
dotnet run --project .\src\Automation.Validator\Automation.Validator.csproj -- plan --ui-map .\samples\ui\ui-map.yaml --features .\samples\features

# Recorder (new)
dotnet run --project .\src\Automation.RecorderTool\Automation.RecorderTool.csproj
```
