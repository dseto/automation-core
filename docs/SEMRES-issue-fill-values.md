# Problema: valores de `fill` substituídos durante a resolução semântica ❌

## Descrição funcional (para Business Analysts) 💬

O sistema captura interações reais de usuários (cliques e preenchimentos) e gera automaticamente um rascunho de teste em formato Gherkin para facilitar a criação de cenários de teste. Durante a etapa de "resolução semântica" — que mapeia esses passos para elementos e páginas conhecidos (UiMap) — alguns valores digitados pelo usuário (por exemplo, senhas, nomes) foram trocados indevidamente pela referência do elemento (por exemplo, `login.user`).

Impacto prático:
- Cenários gerados automaticamente tornam-se inválidos para execução automática, exigindo intervenção manual da equipe de QA.
- O pipeline de validação (Automation.Validator) sinaliza erros, impedindo integração automática.
- Perda de confiança e aumento de esforço manual para transformar gravações em testes executáveis.

## Resumo (rápido) 🔍
Ao executar o pipeline de resolução semântica (`generate-draft` → `resolve-draft`) o `resolved.feature` ficou com os valores de preenchimento trocados. Exemplo observado:

- Draft original:
  - `E eu preencho "login.user" com "admin"`
- Resolved gerado (incorreto):
  - `E eu preencho "login.user" com "login.user"`

Isso causou falha na validação (Automation.Validator) com erro: `RESOLVED_STEP_MISSING_OR_REORDERED` — porque o resolved não contém o passo textual esperado pelo draft.

---

## Reproduzir 🧭
1. Ter uma sessão válida com eventos de click/fill (ex.: `ui-tests\artifacts\seguro-sim\session.json`).
2. Executar: `ui-tests\scripts\run-semantic-resolution_segurosim.ps1`.
3. Observar o `ui-tests\artifacts\semantic-resolution-segurosim\resolved\resolved.feature` e `ui-tests\artifacts\semantic-resolution-segurosim\resolved\resolved.metadata.json`.
4. Notar que valores literais (ex.: `"admin"`) foram substituídos indevidamente.

---

## Causa raiz (detalhada) 🧠
- O `SemanticResolver` faz substituições textuais nas linhas do draft para trocar referências ambíguas por referências resolvidas (p.ex., `login.pass.label` → `login.pass-label`).
- A implementação atual usa uma chamada genérica de Regex.Replace que substitui **todas** as ocorrências entre aspas na linha:

```csharp
// código problemático (resumido)
lines[idx] = Regex.Replace(lines[idx], "\"([^\"]+)\"", $"\"{newRef}\"");
```

- Essa abordagem substitui também os literais de valor (por exemplo, o segundo parâmetro do step `E eu preencho ... com "valor"`), trocando `"admin"` por `"login.user"`.

---

## Correção proposta ✅
- Ajustar a substituição para ser precisa e segura, alterando somente a referência do elemento e preservando o valor literal:

Opções viáveis (preferência 1 → 2):

1. Substituir usando regex que case especificamente o padrão de `fill` (apenas altera o *elementRef*, não o *value*):

```csharp
// exemplo de regex alvo (C#)
var pattern = "^(\\s*E eu preencho)\\s+\"(?<elem>[^\"]+)\"\\s+com\\s+\"(?<val>[^\"]+)\"";
lines[idx] = Regex.Replace(lines[idx], pattern, m => $"{m.Groups[1].Value} \"{newRef}\" com \"{m.Groups[2].Value}\"");
```

2. (Mais simples) Usar `Regex.Replace` com contador (`count = 1`) para substituir apenas a primeira ocorrência entre aspas, combinado com verificação do contexto da linha para certificar que a primeira ocorrência corresponde ao elemento e não ao valor.

- Garantir que a substituição preserve a ordem e o texto do passo (exceto pela substituição controlada do elemento), para que o `ResolvedFeatureValidator` encontre o passo original ou uma equivalência aceitável.

---

## Testes a adicionar / atualizar 🧪
- Unit tests em `SemanticResolutionTests`:
  - Caso: `Quando eu clico em "login.user"` + `E eu preencho "login.user" com "admin"` → após resolver, `resolved.feature` deve ter `E eu preencho "login.user" com "admin"` (valor preservado) e referência de elemento compatível com runtime.
  - Caso: substituição de `login.pass.label` → `login.pass-label` (ou equivalente) **mas** valor permanece inalterado.
- Integração E2E:
  - Executar `ui-tests\scripts\run-semantic-resolution_segurosim.ps1` com uma sessão válida e validar com `Automation.Validator`.

---

## Riscos e mitigação ⚠️
- Risco: regex muito permissiva pode continuar alterando literais (regredir). Mitigação: usar regex estrita que case o formato do step, não todas as strings.
- Risco: duplicar linhas para preservar original (se feito) pode gerar artefatos que confundam humanos. Mitigação: preferir reescrita *segura* (somente a parte necessária) em vez de duplicação quando possível.
- Risco: testes frágeis para variantes de step (ex.: variações de espaçamento/idioma). Mitigação: incluir testes com variações e normalização via helpers (NormalizeStepText já existe, usar similar).

---

## Checklist de validação (para quando a correção for aplicada) ✅
- [ ] Implementar substituição específica (ver acima).
- [ ] Adicionar/ajustar unit tests e E2E tests.
- [ ] dotnet build → OK
- [ ] dotnet test → todos os testes passam
- [ ] Executar: `ui-tests\scripts\run-semantic-resolution_segurosim.ps1` → Automation.Validator passa (exit 0)
- [ ] Executar: `ui-tests\scripts\run-debug-segurosim.ps1` → cenário executa sem erros (nenhuma falha de mapeamento/valor alterado)

---

## Locais do código a revisar
- src/Automation.Core/Recorder/Semantic/SemanticResolver.cs — local da substituição por regex (corrigir aqui)
- src/Automation.Core/Recorder/Draft/StepInferenceEngine.cs — revisar se a inferência está agrupando ou alterando passos de forma inesperada
- src/Automation.Core.Tests/SemanticResolutionTests.cs — adicionar testes novos
- ui-tests scripts & validators: verificar comportamento de validação (Automation.Validator/ResolvedFeatureValidator)

## Glossário 📚
- `draft.feature`: Gherkin gerado a partir da sessão (Recorder). Contém os passos inferidos diretamente das interações do usuário (click, fill, navigate).
- `resolved.feature`: Versão do `draft.feature` após a resolução semântica. Substitui referências por chaves do UiMap e insere comentários de *findings* (UIGAPs), tornando o texto mais alinhado ao UiMap.
- `draft.metadata.json`: Metadados do draft contendo *mappings* entre eventos, ações e linhas do draft, e contagem de steps/actions.
- `resolved.metadata.json`: Metadados da resolução contendo o status de cada step (resolved/partial/unresolved), *chosen* (pageKey/elementKey/testId) e *findings* por step.
- `ui-gaps.report.json`: Relatório de gaps (findings) gerado pela resolução, usado pelo validator e para sugerir correções no UiMap.
- `session.json`: Sessão gravada pelo Recorder com a lista de eventos brutos (cada evento tem tipo, target, value, route etc.).
- `ui-map-segurosim.yaml`: UiMap do aplicativo em teste — mapeia `pageKey` → `elementKey` → `testId` e define `__meta.route` para páginas.
- `BASE_URL`: Variável de ambiente que representa o host base do aplicativo; usada para construir URLs completas ao navegar.
- `pageKey` / `elementKey` / `testId`: termos do UiMap: `pageKey` é a chave da página (ex.: `login`), `elementKey` é a chave do elemento dentro da página (ex.: `pass-label`) e `testId` é o identificador real usado no HTML (ex.: `login.pass.label`).

---

Se quiser, eu posso preparar o snippet de patch exato e os testes sugeridos para aplicar (posso implementar e rodar os testes), ou apenas entregar o patch sugerido para você revisar e aplicar. Qual opção prefere? (A) Implementar e testar; (B) Entregar patch + testes sugeridos para revisão.