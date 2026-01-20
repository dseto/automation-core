# 🏠 Wiki de Automação - Guia para QAs e BAs

## Bem-vindo(a) à Plataforma Zero Code!

Esta Wiki é o seu guia essencial para criar testes de interface de usuário (UI) automatizados de forma rápida e robusta, sem a necessidade de escrever uma única linha de código C#.

Nossa plataforma segue o princípio **"Zero Code"** e utiliza **BDD (Behavior-Driven Development)** com a sintaxe **Gherkin** (em Português do Brasil) e arquivos de configuração **YAML**.

---

## 🎯 Princípios Fundamentais

| Princípio | Descrição | Ferramenta Chave |
| :--- | :--- | :--- |
| **Zero Code** | Você escreve apenas o **"O Quê"** (o comportamento esperado), e a plataforma cuida do **"Como"** (a execução técnica). | Gherkin + YAML |
| **BDD** | Testes escritos em linguagem natural, focados no comportamento do negócio. | Arquivos `.feature` |
| **Contrato** | A aplicação deve seguir um contrato de elementos (`data-testid`) para que a automação funcione. | `ui-map.yaml` |
| **Determinismo** | A plataforma garante que cada instrução é resolvida de forma inequívoca. | **Anchor Pattern** e **Sintaxe Explícita do DataResolver** |
| **Shift-Left** | Problemas de mapeamento e dados são detectados antes da execução do teste. | `Automation.Validator` CLI |

---

## 🗺️ Mapa da Wiki

| Seção | Foco | Arquivos |
| :--- | :--- | :--- |
| **1. Guia Gherkin** | Como escrever cenários `.feature` em PT-BR, tags e a nova sintaxe de dados. | `01-GHERKIN-GUIDE.md` |
| **2. Guia UiMap** | Como mapear elementos, definir páginas e usar o **Anchor Pattern** para SPAs. | `02-UIMAP-GUIDE.md` |
| **3. Guia DataMap** | Como gerenciar dados de teste, contextos, objetos (`@`) e datasets (`{{}}`). | `03-DATAMAP-GUIDE.md` |
| **4. Guia Validator** | Como usar a ferramenta CLI para validar seus arquivos antes de rodar os testes. | `04-VALIDATION-GUIDE.md` |

---

## 💡 O que há de Novo (Melhorias Críticas)

| Melhoria | Problema Resolvido | Onde Usar |
| :--- | :--- | :--- |
| **Anchor Pattern** | Ambiguidade de páginas em SPAs, modais e renderização condicional. | `ui-map.yaml` |
| **Sintaxe Explícita** | Ambiguidade entre valores literais e referências de dados. | Arquivos `.feature` |

**Seu sucesso na automação depende da correta aplicação destes novos padrões.**

---

## 📞 Suporte

Em caso de dúvidas, consulte primeiro esta Wiki. Se o problema persistir, entre em contato com a equipe de Automação Core.
