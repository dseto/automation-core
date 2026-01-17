
# Automation Platform — Contexto Consolidado

## 1. Visão Geral

Este projeto define e implementa uma **plataforma corporativa de automação de testes de UI** para aplicações web (principalmente **Angular 17+**), usando uma abordagem **spec-driven**, com foco em:

- Escala (100+ aplicações)
- Padronização
- Baixa fragilidade
- Debug visual local
- Governança e InnerSource

Atualmente, o **Automation.Core já está implementado**, utilizando **Reqnroll** (sucessor do SpecFlow). O foco agora passa a ser:
- Pilotos reais
- Evolução do Core
- Sustentação, governança e expansão.

---

## 2. Stack Tecnológica

### Plataforma
- C# / .NET 8
- Selenium WebDriver
- Microsoft Edge (único browser)
- Reqnroll (BDD)
- xUnit
- Windows agents
- Azure DevOps Server 2020 on‑prem

### Frontends alvo
- Angular 17+ (SPA com rotas)
- data-testid como contrato obrigatório

---

## 3. Fundamentos Arquiteturais (não negociáveis)

- **data-testid como contrato de automação**
- **ui-map.yaml** como dicionário de elementos (sem lógica)
- **Runtime resolution** (sem geração de código por aplicação)
- **BDD via Reqnroll**
- **Pré‑flight validation** (doctor / validate / plan)
- **Debug visual local obrigatório**
- **Edge‑only**
- **IA como acelerador, não dependência**

---

## 4. Componentes da Plataforma

### Automation.Core
Responsável por:
- Gerenciamento de driver (Edge)
- Esperas (DOM ready, Angular best‑effort + hard timeout)
- Resolução de elementos via ui-map.yaml
- Evidências e logs
- Debug visual (highlight, slow‑mo, pause)
- Configuração por variáveis de ambiente

### Automation.Reqnroll
- Hooks de cenário
- Injeção de runtime
- Step definitions genéricas (PT‑BR)
- Integração com xUnit

### Automation.Validator
- doctor
- validate (features x ui-map)
- plan (plano de execução)

---

## 5. Padrão de automação

### UI
- Todo elemento testável deve ter `data-testid`
- Cada tela deve ter um elemento âncora
- Rotas previsíveis
- Sem dependência de estrutura de DOM

### ui-map.yaml
- Estrutura: Página → Elemento → testid
- Metadados: route, anchor
- Nenhuma lógica ou fluxo

### Features
- Escritas em PT‑BR
- Fluxos definidos no Gherkin
- Steps genéricos
- Tags para smoke, regressão etc.

---

## 6. Execução

### Local (QA / DEV)
- Scripts PowerShell
- VS Code Tasks
- Modos:
  - validate
  - smoke headless
  - debug visual

### CI (Azure DevOps)
- Windows agent
- Edge
- Headless
- Publicação de evidências

---

## 7. Debug visual local

Requisito obrigatório.

Capacidades:
- Execução headed
- Highlight de elementos
- Slow motion
- Pause por step
- Pause em falha
- Logs detalhados

Uso exclusivo local (não CI).

---

## 8. Preparação de aplicações piloto

Checklist:
- Inserir data-testid nos elementos
- Criar âncoras de tela
- Garantir bypass de login (quando aplicável)
- Garantir massa previsível
- Criar ui-tests com:
  - ui-map.yaml
  - features
  - scripts
  - tasks

---

## 9. Governança e InnerSource

- Automation.Core é produto corporativo
- Versionamento controlado
- Contribuições via PR (InnerSource)
- Guidelines formais para:
  - novos steps
  - waits
  - debug
  - padrões de selector
- Core não pode virar gargalo
- Squads podem propor steps e melhorias

---

## 10. Objetivo atual

- Rodar pilotos reais
- Estabilizar waits e debug
- Refinar governança
- Evoluir ferramentas auxiliares:
  - automação de data-testid
  - geração assistida de ui-map
  - validação de UI

---

## 11. Frentes futuras mapeadas

- CLI local para suporte a QA
- Recorder assistido (gera ui-map + feature)
- Integração com APIs para setup de massa
- Dashboards de execução
- Pacotes internos versionados
- Templates corporativos por tipo de aplicação

---

## 12. Estado atual

- Automation.Core implementado com Reqnroll
- Kit local criado para pilotos
- Processo definido para escala

Próximo passo: **executar pilotos, validar arquitetura, evoluir o Core.**
