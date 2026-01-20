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

Esta Wiki está organizada nos seguintes documentos:

| Documento | Propósito |
|-----------|----------|
| **[HOME.md](HOME.md)** | **Você está aqui.** Boas-vindas, princípios fundamentais e mapa da Wiki. |
| **[01-GHERKIN-GUIDE.md](01-GHERKIN-GUIDE.md)** | Como escrever cenários `.feature` em PT-BR, tags, estrutura de dados e a nova sintaxe de dados. |
| **[02-UIMAP-GUIDE.md](02-UIMAP-GUIDE.md)** | Como mapear elementos, definir páginas e usar o **Anchor Pattern** para SPAs e modais. |
| **[03-DATAMAP-GUIDE.md](03-DATAMAP-GUIDE.md)** | Como gerenciar dados de teste, contextos, objetos (`@`) e datasets (`{{}}`). |
| **[04-VALIDATION-GUIDE.md](04-VALIDATION-GUIDE.md)** | Como usar a ferramenta CLI `Automation.Validator` para validar seus arquivos antes de rodar os testes. |
| **[05-ESCAPE-HATCH-GUIDE.md](05-ESCAPE-HATCH-GUIDE.md)** | Guia sobre execução de JavaScript (Escape Hatch) como último recurso em cenários complexos. |
---

## 💡 O que há de Novo (Melhorias Críticas)

| Melhoria | Problema Resolvido | Onde Usar |
| :--- | :--- | :--- |
| **Anchor Pattern** | Ambiguidade de páginas em SPAs, modais e renderização condicional. | `ui-map.yaml` |
| **Sintaxe Explícita** | Ambiguidade entre valores literais e referências de dados. | Arquivos `.feature` |

**Seu sucesso na automação depende da correta aplicação destes novos padrões.**

---

## � Como Começar

1.  **Iniciante na plataforma:** Comece com [01-GHERKIN-GUIDE.md](01-GHERKIN-GUIDE.md) para aprender a escrever seus primeiros cenários.
2.  **Mapeamento de elementos:** Mergulhe em [02-UIMAP-GUIDE.md](02-UIMAP-GUIDE.md) para entender como mapear elementos com `data-testid`.
3.  **Gerenciar dados de teste:** Consulte [03-DATAMAP-GUIDE.md](03-DATAMAP-GUIDE.md) para aprender a estruturar dados.
4.  **Validação antes de rodar:** Sempre use [04-VALIDATION-GUIDE.md](04-VALIDATION-GUIDE.md) para validar seus arquivos com a CLI.
5.  **Casos avançados:** Consulte [05-ESCAPE-HATCH-GUIDE.md](05-ESCAPE-HATCH-GUIDE.md) apenas quando necessário usar JavaScript.

---

## 📞 Suporte

Em caso de dúvidas, consulte primeiro esta Wiki. Se o problema persistir, entre em contato com a equipe de Automação Core.

---

**Próximo Documento:** [01 - Guia Gherkin](01-GHERKIN-GUIDE.md)
