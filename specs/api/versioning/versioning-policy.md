# Versioning Policy

## O que versiona
- Pacote(s) NuGet do runtime (`Automation.Reqnroll`).
- Contratos de arquivo: ui-map, data-map, catálogo de steps.

## Regras
- Mudanças que quebram compatibilidade de contrato (breaking):
  - renomear página/elemento em ui-map sem alias
  - remover step existente ou alterar assinatura
  - alterar comportamento padrão de resolução (ex: sequential → random)
- Breaking changes exigem:
  - incremento MAJOR no pacote
  - nota de migração
  - delta pack em `releases/delta/`

## Deprecação
- manter aliases por ao menos 1 MINOR release antes de remover.
