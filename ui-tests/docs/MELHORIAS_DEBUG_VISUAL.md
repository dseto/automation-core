# Melhorias de Debug Visual - Pauses e Waits entre Steps

## Resumo das Mudanças

Foi implementado um sistema de pauses e esperas explícitas para o debug visual dos testes E2E, permitindo visualizar melhor cada step da automação.

## O que foi feito

### 1. **Melhorias no Step "Quando eu clico em"**
   - Adicionada pausa de 500ms após o clique para permitir início da transição
   - Adicionada espera por DOM estar pronto após clique
   - Adicionada espera por Angular estabilizar
   - Chamada de `MaybeSlowMo()` após as esperas para respeitar `SLOWMO_MS`

**Arquivo**: `Steps/PilotSteps.cs` - Método `QuandoEuClicoEm()`

```csharp
[When(@"eu clico em ""(.*)""")]
public void QuandoEuClicoEm(string elementRef)
{
    _rt.Waits.WaitDomReady(_rt.Driver);
    _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);

    var css = CssByElementRef(elementRef);
    var el = _rt.Waits.WaitClickableByCss(_rt.Driver, css);
    _rt.Debug.TryHighlight(_rt.Driver, el);

    el.Click();

    // Aguardar transição/navegação
    _rt.Debug.MaybeSlowMo();
    
    // Aguardar DOM estar pronto após clique
    Thread.Sleep(500); // Pequena pausa para iniciar transição
    _rt.Waits.WaitDomReady(_rt.Driver);
    _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);
    _rt.Debug.MaybeSlowMo();
}
```

### 2. **Adição de Step "Given o elemento deve estar visível"**
   - Permite validar que elementos estão visíveis como parte do contexto (Given)
   - Espera explícita por DOM pronto
   - Destaque visual do elemento

**Arquivo**: `Steps/PilotSteps.cs` - Método `DadoOElementoDeveEstarVisivel()`

```csharp
[Given(@"o elemento ""(.*)"" deve estar visível")]
public void DadoOElementoDeveEstarVisivel(string elementRef)
{
    _rt.Waits.WaitDomReady(_rt.Driver);
    var css = CssByElementRef(elementRef);
    var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, css);
    _rt.Debug.TryHighlight(_rt.Driver, el);
}
```

### 3. **Remoção de Ambiguidades**
   - Removidos steps duplicados com `BasicSteps` do Automation.Reqnroll
   - Mantidos apenas os steps específicos do piloto
   - Deixado que a biblioteca base valide "a rota deve ser" (com waits internos)

## Como Usar no Debug Visual

### Script: `run-debug.ps1`

```powershell
# Configurações ativas:
$env:UI_DEBUG = "true"              # Ativa debug visual
$env:HEADLESS = "false"             # Abre navegador visível
$env:SLOWMO_MS = "2500"             # Pausa de 2.5s entre steps (pode ajustar)
$env:PAUSE_EACH_STEP = "true"       # Pausa entre cada step (gerenciado pelo Core)
```

### Executar com Debug Visual

```bash
cd C:\Projetos\metrics-simple-frontend\ui-tests
.\scripts\run-debug.ps1
```

### Resultado Esperado

1. **Navegador abre** em modo visível (não headless)
2. **Cada step é destacado** com bordas visuais no elemento
3. **Pausa de 2.5s** após cada interação (preenchimento, clique)
4. **Espera automática** após cliques por:
   - 500ms inicial para transição iniciar
   - DOM estar pronto
   - Angular estabilizar
   - Mais 2.5s de slowmo para visualizar

## Fluxo do Teste "Login com credenciais válidas"

1. ✅ Aplicação em execução → Navega para BASE_URL
2. ✅ Vai para página "login" → Navega e aguarda
3. ✅ Elemento "login.anchor" deve estar visível → Espera e destaca
4. ⏸️ Preenche username → 2.5s pausa
5. ⏸️ Preenche password → 2.5s pausa
6. ⏸️ **CLICA em login.submit** → 500ms + DOM ready + Angular stable + 2.5s = total ~5-6s
7. ⏸️ **Espera rota ser "/dashboard"** → Até 10s esperando navegação (from BasicSteps)
8. ✅ Elemento "dashboard.anchor" deve estar visível → Espera e destaca
9. ✅ Elemento "dashboard.stat-total" deve estar visível → Espera e destaca

## Variáveis de Ambiente Ajustáveis

| Variável | Valor Atual | Descrição |
|----------|------------|-----------|
| `SLOWMO_MS` | `2500` | Milissegundos de pausa entre steps (para visualização) |
| `UI_DEBUG` | `true` | Ativa elementos destacados com bordas |
| `HEADLESS` | `false` | Abre navegador visível |
| `PAUSE_EACH_STEP` | `true` | Pausa (gerenciada pelo Core) |
| `BASE_URL` | Variável | URL da aplicação (http://localhost:4200 para desenvolvimento local) |

## Para Ajustar a Velocidade

- **Mais lento**: `$env:SLOWMO_MS = "5000"` (5 segundos)
- **Mais rápido**: `$env:SLOWMO_MS = "1000"` (1 segundo)
- **Sem pausa**: `$env:SLOWMO_MS = "0"`

## Próximos Passos

Se ainda houver necessidade de melhorias:

1. Aumentar timeout de rota em `EntaoARotaDeveSer` se navegação for lenta
2. Ajustar `Thread.Sleep(500)` no clique se transição for mais lenta
3. Adicionar logs customizados para diagnosticar fluxos lentos
4. Criar métodos de espera para elementos específicos (modais, dropdowns)
