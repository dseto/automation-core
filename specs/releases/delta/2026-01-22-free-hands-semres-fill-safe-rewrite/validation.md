# Validação

## V-SR-40
Passos de `fill` resolvidos DEVEM preservar os valores literais exatamente como foram digitados.

## V-RN-01
- Quando a `url` de um evento de navegação gravado for igual ao `BASE_URL` configurado, o resolvedor deve registrar `route: "/"` (root). Isso evita que o caminho base seja emitido nas gravações e quebre etapas subsequentes de geração de drafts.

## V-RC-01
- Eventos capturados no browser (click, submit, fill, navigate) DEVEM sobreviver a navegações completas. Eventos empurrados para o `buffer` de captura antes de `pagehide`/`beforeunload` devem ser persistidos e disponibilizados ao recorder na próxima carga de página (a implementação pode usar a chave de `localStorage` de curta duração `__fhRecorder_pending`).

## V-SR-41
- As features resolvidas DEVEM preferir referências `element-only` (por exemplo, `client-name`) quando possível. Se uma chave de elemento existir em múltiplas páginas, o resolvedor DEVE emitir um **aviso** `UIGAP_ELEMENT_AMBIGUOUS` para a linha correspondente do draft e manter a forma `element-only` no arquivo resolvido.
