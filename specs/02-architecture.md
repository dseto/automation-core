
# 02 — Arquitetura

## src/Automation.Core
- Configuration: RunSettings
- UiMap: UiMapLoader + UiMapModel
- Resolution: PageContext + ElementResolver + LocatorFactory
- Driver: EdgeDriverFactory
- Waits: DOM ready + Angular best-effort + element waits
- Evidence: EvidenceManager
- Debug: DebugController
- Diagnostics: StepTrace (evoluir para log estruturado por step)

## src/Automation.Reqnroll (Bindings BDD)
Bindings BDD usando **Reqnroll** (substitui SpecFlow).

- Hooks de runtime por cenário
- Steps PT-BR MVP (catálogo em Spec 08)
- Integração via DI (BoDi container)

## src/Automation.Validator
- doctor/validate/plan
- parser MVP de `.feature` (PT-BR) para validar `ui-map.yaml`
