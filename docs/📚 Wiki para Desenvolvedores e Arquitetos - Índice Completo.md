# 📚 Wiki para Desenvolvedores e Arquitetos - Índice Completo

**Versão:** 1.0  
**Data:** 20 de Janeiro de 2026  
**Plataforma:** Automation Platform  
**Público-alvo:** Desenvolvedores, Arquitetos de Solução, Contribuidores

---

## 📖 Documentos da Wiki

### 1. **HOME.md** - Introdução e Visão Geral
- **Propósito:** Bem-vindo e orientação inicial
- **Conteúdo:**
  - Visão geral de 3 pilares da arquitetura
  - Princípios de design (Zero-Code, Contrato Forte, Shift-Left, Determinismo, Extensibilidade)
  - Mapa da Wiki
  - Como começar (por role: Arquiteto, Desenvolvedor de Manutenção, Desenvolvedor de Extensão)

### 2. **01-ARCHITECTURE-OVERVIEW.md** - Visão Geral da Arquitetura
- **Propósito:** Entender a arquitetura macro e o fluxo de execução
- **Conteúdo:**
  - Componentes principais (Core, Reqnroll, Validator)
  - Fluxo de execução de um teste (diagrama sequencial)
  - Detalhamento do fluxo (7 passos)
  - Princípios de design aplicados (SRP, DIP, Declarative Approach, Shift-Left)
  - Inovações recentes (Anchor Pattern, Sintaxe Explícita)

### 3. **02-PROJECT-STRUCTURE.md** - Estrutura dos Projetos
- **Propósito:** Visão geral da estrutura de diretórios
- **Conteúdo:**
  - Estrutura geral dos 3 projetos
  - Detalhamento de cada projeto com diretórios e propósito
  - Árvore de diretórios para cada projeto

### 4. **03-CORE-PROJECT.md** - Detalhamento do Projeto Core
- **Propósito:** Mergulho profundo no coração da plataforma
- **Conteúdo:**
  - Principais namespaces: Resolution, UiMap, DataMap, Driver, Waits
  - Tabela de classes e responsabilidades
  - Como a Sintaxe Explícita funciona (código de exemplo)
  - Como o Anchor Pattern funciona (explicação detalhada)

### 5. **04-REQNROLL-PROJECT.md** - Detalhamento do Projeto Reqnroll
- **Propósito:** Entender a camada de BDD e steps
- **Conteúdo:**
  - Propósito e filosofia (steps genéricos, não criar novos)
  - Estrutura e classes principais (NavigationSteps, InteractionSteps, ValidationSteps, Hooks)
  - Exemplo de um step (código completo)
  - Injeção de dependência (código de exemplo)

### 6. **05-VALIDATOR-PROJECT.md** - Detalhamento do Projeto Validator
- **Propósito:** Entender a ferramenta CLI de validação
- **Conteúdo:**
  - Propósito e casos de uso
  - Estrutura e classes principais (Program.cs, Validators, Services)
  - Exemplo de fluxo de validação (passo a passo)

### 7. **06-EXTENSION-GUIDE.md** - Guia de Extensão
- **Propósito:** Como estender a plataforma sem modificar o Core
- **Conteúdo:**
  - Filosofia de extensão (Aberto para extensão, fechado para modificação)
  - Cenário 1: Adicionar um novo step genérico (passo a passo com código)
  - Cenário 2: Adicionar uma nova estratégia de dataset (passo a passo com código)
  - Cenário 3: Adicionar um novo tipo de resolução (avançado, com avisos)

### 8. **07-CONTRIBUTION-GUIDE.md** - Guia de Contribuição
- **Propósito:** Como contribuir para o desenvolvimento
- **Conteúdo:**
  - Processo de contribuição (8 passos)
  - Padrões de código (linguagem, estilo, nullability, async/await, comentários, DI)
  - Definição de "Pronto" (checklist de 6 itens)

---

## 🎯 Como Usar Esta Wiki

### Para Arquitetos de Solução
1. Comece com **HOME.md** para entender a visão geral
2. Leia **01-ARCHITECTURE-OVERVIEW.md** para entender o fluxo
3. Consulte **02-PROJECT-STRUCTURE.md** para ver como os projetos se organizam
4. Use **06-EXTENSION-GUIDE.md** para avaliar a extensibilidade da plataforma

### Para Desenvolvedores (Manutenção)
1. Comece com **HOME.md**
2. Leia **01-ARCHITECTURE-OVERVIEW.md**
3. Mergulhe em **03-CORE-PROJECT.md** e **04-REQNROLL-PROJECT.md** para entender o funcionamento interno
4. Consulte **07-CONTRIBUTION-GUIDE.md** antes de fazer alterações

### Para Desenvolvedores (Extensão)
1. Comece com **HOME.md**
2. Leia **06-EXTENSION-GUIDE.md** para aprender como estender
3. Consulte os projetos específicos (**03-CORE-PROJECT.md**, **04-REQNROLL-PROJECT.md**, **05-VALIDATOR-PROJECT.md**) conforme necessário
4. Siga o **07-CONTRIBUTION-GUIDE.md** ao fazer o PR

---

## 📊 Estrutura de Tópicos

| Tópico | Documentos |
|--------|-----------|
| **Arquitetura** | HOME, 01 |
| **Estrutura** | 02, 03, 04, 05 |
| **Extensão** | 06 |
| **Contribuição** | 07 |

---

## 🔍 Índice de Conceitos-Chave

| Conceito | Documento | Seção |
|----------|-----------|-------|
| **Anchor Pattern** | 01, 03 | "Inovações Recentes", "Como o Anchor Pattern Funciona" |
| **Sintaxe Explícita** | 01, 03 | "Inovações Recentes", "Como a Sintaxe Explícita Funciona" |
| **DataResolver** | 03 | "Principais Namespaces" |
| **ElementResolver** | 03 | "Principais Namespaces" |
| **PageContext** | 03 | "Principais Namespaces" |
| **Steps Genéricos** | 04, 06 | "Propósito e Filosofia", "Cenário 1" |
| **Injeção de Dependência** | 04 | "Injeção de Dependência" |
| **Shift-Left Testing** | 01, 05 | "Princípios de Design", "Propósito e Casos de Uso" |
| **Extensibilidade** | 06 | "Filosofia de Extensão" |

---

## 🚀 Próximos Passos

Após ler a Wiki:

1. **Clone o repositório** e explore o código-fonte
2. **Execute os testes** para entender o comportamento esperado
3. **Crie uma extensão simples** (ex: um novo step) para praticar
4. **Contribua** com melhorias ou correções

---

## 📝 Notas Importantes

- Esta Wiki assume conhecimento básico de C#, .NET e Selenium WebDriver
- Os exemplos de código são simplificados para clareza; consulte o código-fonte para implementações completas
- A Wiki é um documento vivo e deve ser atualizada conforme a plataforma evolui

---

**Versão da Plataforma:** 2.0 (com Anchor Pattern e Sintaxe Explícita)  
**Última Atualização:** 20 de Janeiro de 2026  
**Mantido por:** Equipe de Arquitetura da Automation Platform
