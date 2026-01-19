# 08 - Segurança e Conformidade

## Gestão de Secrets

### Variáveis de Ambiente
Credenciais nunca devem estar hardcoded. Use variáveis de ambiente:
```bash
export TEST_USER="admin"
export TEST_PASS="ChangeMe123!"
```

### DataMap com Secrets
Para dados sensíveis no DataMap, use referências a variáveis de ambiente:
```yaml
contexts:
  default:
    user_admin:
      username: ${TEST_USER}
      password: ${TEST_PASS}
```

O Core resolve `${VAR_NAME}` para o valor da variável de ambiente.

### CI/CD Secrets
Em pipelines CI/CD, use o gerenciador de secrets da plataforma (GitHub Secrets, Azure Key Vault, etc.). Nunca comite secrets no repositório.

## Isolamento de Dados

### Contextos por Ambiente
Use contextos diferentes para cada ambiente:
```yaml
contexts:
  default:
    user_admin:
      username: "admin_dev"
  
  homolog:
    user_admin:
      username: "admin_hml"
  
  prod:
    user_admin:
      username: "admin_prod"
```

Selecione o contexto via `RunSettings.Environment`.

### Limpeza de Dados
Após cada cenário, o RuntimeHooks deve limpar dados sensíveis (localStorage, sessionStorage). Use o Escape Hatch se necessário:
```gherkin
Então eu executo o script JS "localStorage.clear()"
```

## Conformidade

### Auditoria
Todos os testes devem ser rastreáveis. Use tags para categorizar (ex: `@gdpr`, `@pci`). Logs devem ser armazenados com timestamp.

### Retenção de Evidências
Screenshots e logs devem ser retidos por 90 dias. Após esse período, devem ser deletados automaticamente.

### Conformidade com LGPD/GDPR
Não use dados reais de usuários em testes. Use dados fictícios ou anonimizados. Se precisar de dados reais, obtenha consentimento explícito.

## Boas Práticas

### Princípio do Menor Privilégio
Use contas de teste com permissões mínimas. Não use contas administrativas para testes padrão.

### Rotação de Credenciais
Credenciais de teste devem ser rotacionadas regularmente (ex: a cada 90 dias).

### Monitoramento
Monitore tentativas de acesso falhadas. Se houver múltiplas falhas, bloquear a conta temporariamente.

### Documentação
Documente todas as políticas de segurança em um arquivo `SECURITY.md` no repositório.
