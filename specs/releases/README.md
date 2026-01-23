# 📦 Releases & Delta Packs — Spec Deck

Este diretório contém o **histórico oficial de entregas** do framework, organizado em **delta packs**.

Cada delta pack representa **uma entrega lógica** (feature, estabilização, refactor, breaking change, etc.) e é o **artefato primário de governança** entre:

- especificação (spec deck)
- código
- validação
- consumidores do framework

---

# 🧭 Estrutura

```
specs/releases/
  README.md                ← este arquivo
  delta/
    YYYY-MM-DD-<slug>/
      README.md
      changes.md
      migration.md (se aplicável)
```

Exemplo real:

```
specs/releases/delta/
  2026-01-20-spec-deck-refactor/
  2026-01-20-stabilization-framework/
```

---

# 🏷️ Convenção de nomenclatura

## Formato da pasta

```
YYYY-MM-DD-<slug-semantico>
```

Onde o `slug` deve indicar claramente o tipo de entrega, por exemplo:

- `spec-deck-refactor`
- `stabilization-framework`
- `gherkin-validation`
- `runtime-hardening`
- `ui-map-contract-v2`

👉 Mesmo dia pode ter **múltiplas entregas**, desde que o `slug` seja diferente.

---

# 📄 Conteúdo obrigatório de um delta pack

Todo delta pack **deve conter**:

### `README.md`
Resumo funcional da entrega:
- objetivo
- escopo
- impacto (breaking ou não)
- checklist de validação

### `changes.md`
Changelog técnico estruturado:
- Added / Changed / Fixed / Removed / Deprecated

### `migration.md` (obrigatório se breaking)
Guia de migração:
- o que quebrou
- quem é impactado
- como adaptar specs, testes e runtime

---

# 🔁 Fluxo oficial de release

1. Criar pasta em `specs/releases/delta/`
2. Copiar templates de `_templates/delta-pack/`
3. Atualizar specs (SSOT) primeiro
4. Implementar no código
5. Validar (unit → contract → smoke)
6. Fechar delta pack
7. Promover entrega (merge / tag / version bump)

Referência completa:  
👉 `specs/shared/spec-driven-flow.md`

---

# 🏛️ Regras de governança

- 📁 **Uma pasta = uma entrega**
- 🔁 A pasta pode evoluir enquanto a entrega estiver em progresso
- 🏁 Ao publicar, o delta pack é considerado **congelado**
- 🚨 Breaking change exige `migration.md`
- 📜 O delta pack é a **fonte de verdade histórica** (não o commit, não o PR)

---

# 📚 Objetivo deste diretório

Este diretório existe para garantir:

- rastreabilidade entre spec e código  
- histórico de decisões arquiteturais  
- governança de breaking changes  
- previsibilidade para QAs e usuários do framework  
- base formal para versionamento e auditoria

---

# 📌 Dica operacional

Sempre que alguém perguntar:

> “O que mudou nessa versão?”

👉 a resposta deve estar **primeiro** aqui, e **depois** no código.

## Anti-Pattern (crítico)
Não capturar apenas ações originadas de steps/Reqnroll. FREE-HANDS deve capturar eventos manuais no browser.

---

# 📜 Releases registradas

A lista abaixo representa as entregas já criadas no spec deck.

- 2026-01-20-stabilization-framework — Estabilização do runtime, build e debug (RELEASED v0.3.0 em 2026-01-21)
- 2026-01-21-free-hands-recorder-session — RF01–RF06 session log (RELEASED v0.4.0 em 2026-01-21)
- 2026-01-21-free-hands-recorder-exploratory-mode — RF00 exploratory mode (RELEASED v0.4.0 em 2026-01-21)
- 2026-01-22-free-hands-draft-generator — Draft Generator (RELEASED v0.5.2 em 2026-01-22)
- 2026-01-22-free-hands-deterministic-routes — Deterministic Routes (RELEASED v0.6.1 em 2026-01-22)
- 2026-01-22-free-hands-semres-fill-safe-rewrite — Semantic resolution: Safe fill-value preservation (RELEASED v0.6.2 em 2026-01-23)
- 2026-01-21-forced-waits — Recorder forced waits + Draft materialization (RELEASED v0.6.0 em 2026-01-21)

