# Changes

## Resumo
Esta entrega corrige uma reescrita semântica insegura que poderia alterar valores literais de preenchimento (`fill`) e normaliza o tratamento de rotas iniciais para evitar emissão de caminhos base nas gravações.

## Destaques
- **RF-SR-40 (Correção comportamental):** O `SemanticResolver` agora preserva valores literais em passos de preenchimento substituindo apenas a primeira ocorrência entre aspas quando aplicável.
- **RF-RN-01 (Correção):** `RouteNormalizer` mapeia navegações iniciais cujo `url` seja igual ao `BASE_URL` para `route: "/"` (root), evitando rotas base no `session.json`.
- **Draft materialization:** `DraftGenerator` preserva passos de navegação explícitos e qualifica alvos não qualificados para reduzir gaps em drafts gerados.
- **Resolução semântica (referências de elemento):** As features resolvidas preferem referências `element-only` (ex.: `client-name`) quando possível; se a mesma chave de elemento existir em várias páginas, o resolvedor emite um aviso `UIGAP_ELEMENT_AMBIGUOUS` e mantém a forma `element-only` para concisão.
- **Estabilidade e bindings:** Adicionados variantes `Given` para steps de interação, melhor tratamento de rotas wildcard, suporte a `--scenario` nos scripts, limpeza de drafts temporários e medidas de segurança para evitar sobrescrita de `session.json` durante runs de debug.
- **Recorder (robustez de captura):** Persistência do buffer de captura no `localStorage` (`__fhRecorder_pending`) para preservar eventos que ocorrem imediatamente antes de uma navegação completa.

## Implementação (arquivos principais)
- `src/Automation.Core/Recorder/Semantic/SemanticResolver.cs`
- `src/Automation.Core/Recorder/Draft/DraftGenerator.cs`
- `src/Automation.Core/Driver/RouteNormalizer.cs`
- `src/Automation.Reqnroll/Steps/InteractionSteps.cs`
- `src/Automation.Reqnroll/Steps/NavigationSteps.cs`
- `src/Automation.RecorderTool/Program.cs`
- Scripts atualizados: `ui-tests/scripts/run-debug-segurosim.ps1`, `ui-tests/scripts/run-semantic-resolution_*.ps1`
- Testes: `SemanticResolutionTests`, `RouteNormalizerTests`, `ActionGrouperTests`, `SemanticE2ETests`

## Validação
- Testes unitários e de integração adicionados/atualizados e executados localmente.
- Execução manual de debug (`ui-tests/scripts/run-debug-segurosim.ps1`) validou o fluxo com sucesso.

