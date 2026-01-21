
# Arquitetura — FREE-HANDS Recorder — Session Log

## Componentes

1. Capture Layer  
   - hooks no driver / browser

2. Normalizer  
   - remove ruído
   - consolida fills

3. Session Builder  
   - aplica RF01–RF06
   - ordena eventos

4. Artifact Writer  
   - gera session.json

## Fluxo

Browser → Event Tap → Normalizer → Session Builder → session.json

## Limites

- Sem UIMap
- Sem Gherkin
- Sem Validator
