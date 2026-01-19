# Backlog Futuro

## v2.1 (Q2 2026)

### Automation.Validator CLI
Implementar ferramenta de validação de contratos com suporte a:
- Validação de UiMap (schema, unicidade, rotas).
- Validação de DataMap (schema, referências circulares).
- Validação de Gherkin (steps conhecidos, páginas existentes).
- Geração de relatório de cobertura.

**Prioridade:** Alta. **Esforço:** 3 sprints.

### Composição de Steps (User Steps)
Permitir que QAs criem novos steps combinando steps existentes sem código C#:
```yaml
# custom-steps.yaml
- name: "que eu realizei o login administrativo"
  steps:
    - "Dado que estou na página 'login'"
    - "Quando eu preencho os campos com os dados de 'user_admin'"
    - "E eu clico em 'submit' e aguardo a rota '/dashboard'"
```

**Prioridade:** Média. **Esforço:** 2 sprints.

### Dashboard de Resultados
Criar dashboard que agregue resultados de testes de todas as 100+ aplicações:
- Taxa de sucesso por aplicação.
- Testes mais flaky.
- Tempo de execução médio.
- Cobertura de funcionalidades.

**Prioridade:** Média. **Esforço:** 4 sprints.

## v3.0 (Q4 2026)

### Dados Dinâmicos (Dynamic DataMap)
Suportar dados que variam por regra, data, cotação, etc.:
```yaml
dynamic_data:
  cotacao_dolar:
    type: api_call
    url: "https://api.economia.com/USD-BRL"
    path: "bid"
  
  data_hoje:
    type: script
    command: "date +%d/%m/%Y"
```

**Prioridade:** Alta. **Esforço:** 5 sprints.

### Testes de API
Estender a plataforma para suportar testes de API REST:
- Steps para GET, POST, PUT, DELETE.
- Validação de response (status, body, headers).
- Integração com DataMap para dados de request.

**Prioridade:** Média. **Esforço:** 6 sprints.

### Testes de Performance
Adicionar suporte a testes de performance:
- Medição de tempo de carregamento de página.
- Medição de tempo de resposta de ações.
- Alertas se exceder threshold.

**Prioridade:** Baixa. **Esforço:** 4 sprints.

## v4.0 (2027)

### Mobile Testing
Estender suporte para aplicações mobile (iOS, Android):
- Integração com Appium.
- Steps específicos para mobile (tap, swipe, etc.).
- Suporte a múltiplos dispositivos em paralelo.

**Prioridade:** Baixa. **Esforço:** 8 sprints.

### AI-Powered Locator Generation
Usar IA para gerar locators automaticamente a partir de screenshots:
- Usuário tira screenshot da página.
- IA identifica elementos e gera testIds.
- Usuário valida e confirma.

**Prioridade:** Baixa. **Esforço:** 10 sprints.

## Critérios de Aceitação para Novas Features
1. Não quebra backward compatibility.
2. Mantém o princípio de "Zero Código".
3. Cobertura de testes >= 80%.
4. Documentação completa.
5. Aprovação de pelo menos 2 squads piloto.

## Roadmap de Adoção
- **Q1 2026:** Piloto em 5 aplicações. Feedback e iteração.
- **Q2 2026:** Rollout em 20 aplicações. Treinamento de squads.
- **Q3 2026:** Rollout em 50 aplicações. Estabilização.
- **Q4 2026:** Rollout em 100+ aplicações. Suporte contínuo.
