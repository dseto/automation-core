# 🚀 Automation Platform - Wiki para Desenvolvedores e Arquitetos

**Bem-vindo à documentação técnica da Automation Platform!**

Esta Wiki é o guia central para desenvolvedores, arquitetos e contribuidores que desejam entender, estender e manter a plataforma de automação de testes de UI. Diferente da Wiki para QAs (focada em *uso*), esta documentação foca na **arquitetura, implementação e extensibilidade**.

---

## 🎯 Visão Geral da Arquitetura

A plataforma é construída sobre 3 pilares fundamentais que garantem **desacoplamento, escalabilidade e manutenibilidade**:

1.  **Contratos Declarativos (YAML):** A intenção do teste é declarada em arquivos `UiMap` e `DataMap`, separando o *o quê* do *como*.
2.  **Core de Resolução (C#):** Um motor de resolução (`Automation.Core`) interpreta os contratos em tempo de execução, lidando com a complexidade da interação com o navegador.
3.  **Steps Genéricos (Gherkin/Reqnroll):** Uma camada fina de steps (`Automation.Reqnroll`) conecta a linguagem natural do Gherkin aos serviços do Core, permitindo que QAs escrevam testes sem programar.

![Arquitetura da Plataforma](https://i.imgur.com/example.png)  *<- Placeholder para um diagrama de arquitetura*

---

## 📚 Estrutura da Wiki

Esta Wiki está organizada nos seguintes documentos:

| Documento | Propósito |
|-----------|----------|
| **[HOME.md](HOME.md)** | **Você está aqui.** Visão geral, princípios e mapa da Wiki. |
| **[01-ARCHITECTURE-OVERVIEW.md](01-ARCHITECTURE-OVERVIEW.md)** | Mergulho profundo na arquitetura, componentes, fluxo de dados e princípios de design. |
| **[02-PROJECT-STRUCTURE.md](02-PROJECT-STRUCTURE.md)** | Visão geral da estrutura dos 3 projetos principais: `Core`, `Reqnroll` e `Validator`. |
| **[03-CORE-PROJECT.md](03-CORE-PROJECT.md)** | Detalhamento do `Automation.Core`, o coração da plataforma. |
| **[04-REQNROLL-PROJECT.md](04-REQNROLL-PROJECT.md)** | Detalhamento do `Automation.Reqnroll`, a camada de steps Gherkin. |
| **[05-VALIDATOR-PROJECT.md](05-VALIDATOR-PROJECT.md)** | Detalhamento do `Automation.Validator`, a ferramenta CLI de Shift-Left. |
| **[06-EXTENSION-GUIDE.md](06-EXTENSION-GUIDE.md)** | **Leitura essencial.** Como estender a plataforma com novos steps, serviços e estratégias. |
| **[07-CONTRIBUTION-GUIDE.md](07-CONTRIBUTION-GUIDE.md)** | Como contribuir para o desenvolvimento da plataforma, padrões de código e processo de PR. |

---

## 💡 Princípios de Design

1.  **Zero-Code para QAs:** Analistas de QA devem focar em escrever Gherkin e YAML. Nenhuma lógica de programação deve ser exigida deles.
2.  **Contrato Forte, Implementação Flexível:** Os contratos (UiMap, DataMap) são a fonte da verdade. A implementação no Core pode ser otimizada, mas o contrato deve ser respeitado.
3.  **Shift-Left Testing:** Erros de contrato devem ser detectados o mais cedo possível. O `Automation.Validator` é a chave para isso, rodando em CI/CD antes mesmo da execução dos testes.
4.  **Determinismo e Previsibilidade:** A plataforma deve se comportar de forma consistente. A introdução da **Sintaxe Explícita** e do **Anchor Pattern** foram passos cruciais para garantir isso.
5.  **Extensibilidade:** A plataforma deve ser fácil de estender sem modificar o Core. O uso de injeção de dependência e interfaces claras é fundamental.

---

## 🚀 Como Começar

1.  **Arquitetos:** Comecem com **[01-ARCHITECTURE-OVERVIEW.md](01-ARCHITECTURE-OVERVIEW.md)** para entender a visão macro.
2.  **Desenvolvedores (Manutenção):** Mergulhem em **[03-CORE-PROJECT.md](03-CORE-PROJECT.md)** e **[04-REQNROLL-PROJECT.md](04-REQNROLL-PROJECT.md)** para entender o funcionamento interno.
3.  **Desenvolvedores (Extensão):** Leiam o **[06-EXTENSION-GUIDE.md](06-EXTENSION-GUIDE.md)** para aprender a adicionar novas funcionalidades.

---

**Próximo Documento:** [01 - Visão Geral da Arquitetura](01-ARCHITECTURE-OVERVIEW.md)
