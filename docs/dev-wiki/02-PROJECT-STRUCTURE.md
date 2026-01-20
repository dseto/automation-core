# 02 - Estrutura dos Projetos

Este documento fornece uma visão geral da estrutura de diretórios e do propósito de cada um dos três projetos principais da Automation Platform.

---

## 📂 Estrutura Geral

A solução está organizada em três projetos principais, localizados no diretório `src/`:

```
automation-platform/
└── src/
    ├── Automation.Core/
    ├── Automation.Reqnroll/
    └── Automation.Validator/
```

---

## 🚀 `Automation.Core`

O coração da plataforma. Contém toda a lógica de negócio, serviços e modelos para interação com o navegador e resolução de contratos.

**Propósito:** Isolar a complexidade do WebDriver e fornecer uma API de serviços coesa para a camada de steps.

```
Automation.Core/
├── DataMap/           # Lógica para carregar e resolver o data-map.yaml
│   ├── DataMapLoader.cs
│   ├── DataMapModel.cs
│   └── DataResolver.cs    # <-- Contém a lógica da Sintaxe Explícita
├── Diagnostics/       # Serviços para logging e tracing
│   └── StepTrace.cs
├── Driver/            # Fábricas para criar instâncias do WebDriver (Chrome, Edge)
│   └── ...
├── Resolution/        # Lógica central de resolução
│   ├── ElementResolver.cs # Resolve nomes amigáveis para seletores
│   └── PageContext.cs     # <-- Mantém o estado da página atual e valida o Anchor
├── UiMap/             # Lógica para carregar e validar o ui-map.yaml
│   ├── UiMapLoader.cs
│   ├── UiMapModel.cs      # <-- Contém a definição do Anchor Pattern
│   └── UiMapValidator.cs
├── Waits/             # Serviços de espera explícita
│   └── WaitService.cs
└── Automation.Core.csproj
```

---

## 🎭 `Automation.Reqnroll`

A camada de BDD (Behavior-Driven Development). Conecta os steps Gherkin escritos em português aos serviços do `Automation.Core`.

**Propósito:** Fornecer uma biblioteca de steps genéricos e reutilizáveis, permitindo que QAs escrevam testes sem precisar de código C#.

```
Automation.Reqnroll/
├── Hooks/               # Lógica que executa antes/depois de cenários (ex: screenshots)
│   └── RuntimeHooks.cs
├── Steps/               # Definição dos steps Gherkin
│   ├── InteractionSteps.cs  # Passos de interação (clicar, preencher)
│   ├── NavigationSteps.cs   # Passos de navegação (ir para página, aguardar rota)
│   └── ValidationSteps.cs   # Passos de validação (verificar texto, visibilidade)
└── Automation.Reqnroll.csproj
```

---

## ✅ `Automation.Validator`

Uma ferramenta de linha de comando (CLI) para validação estática dos contratos.

**Propósito:** Implementar o princípio de "Shift-Left Testing", permitindo que desenvolvedores e pipelines de CI/CD validem os arquivos YAML e Gherkin antes da execução, pegando erros mais cedo.

```
Automation.Validator/
├── Commands/            # Lógica para cada comando da CLI (validate, doctor, plan)
├── Models/              # Modelos de dados para os resultados da validação
├── Services/            # Serviços auxiliares (ex: carregar YAML, gerar relatórios)
├── Validators/          # Lógica de validação para cada tipo de contrato
│   ├── DataMapValidator.cs
│   ├── GherkinValidator.cs
│   └── UiMapValidator.cs
├── Program.cs           # Ponto de entrada da aplicação CLI
└── Automation.Validator.csproj
```

---

**Próximo Documento:** [03 - Detalhamento do Projeto Core](03-CORE-PROJECT.md)
