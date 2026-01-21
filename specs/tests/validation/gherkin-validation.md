# Gherkin Validation (espelho do Validator)

> Fonte de verdade: `Automation.Validator/Validators/GherkinValidator.cs`

## O que o validator faz hoje
- Usa uma lista interna `KnownStepPatterns` (regex) para reconhecer steps.
- Valida páginas referenciadas (`GHERKIN_PAGE_NOT_FOUND`) contra `uiMap.Pages`.
- Valida datasets `{{...}}` (`GHERKIN_DATASET_NOT_FOUND`) contra `dataMap.Datasets`.
- Valida objetos `@...` (`GHERKIN_OBJECT_NOT_FOUND`) verificando se existe em algum contexto.
- Valida elementos referenciados entre aspas para *alguns tipos de steps*:
  - se não existir na página atual: `GHERKIN_ELEMENT_WRONG_PAGE` (warning) se existir em outra
  - se não existir em nenhuma: `GHERKIN_ELEMENT_NOT_FOUND` (warning)

## Códigos e severidade (atuais)

### ERROS
- `GHERKIN_PAGE_NOT_FOUND` (com lineNumber)
- `GHERKIN_DATASET_NOT_FOUND` (com lineNumber)

### WARNINGS
- `GHERKIN_UNKNOWN_STEP`
- `GHERKIN_ELEMENT_WRONG_PAGE`
- `GHERKIN_ELEMENT_NOT_FOUND`
- `GHERKIN_OBJECT_NOT_FOUND`

## Relação com `tests/gherkin/step-catalog.yaml`
- O `step-catalog.yaml` foi gerado do código real (`Automation.Reqnroll/Steps/*.cs`) e é o **SSOT desejado**.
- Porém, **o validator atual ainda NÃO usa** o step-catalog.yaml; ele usa `KnownStepPatterns` hardcoded.
- TODO recomendado: trocar/estender o validator para validar `.feature` contra `step-catalog.yaml` (anti-drift real).
