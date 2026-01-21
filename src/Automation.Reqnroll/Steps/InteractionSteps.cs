using System;
using System.Collections;
using System.Collections.Generic;
using Automation.Reqnroll.Runtime;
using OpenQA.Selenium;
using Reqnroll;

namespace Automation.Reqnroll.Steps;

/// <summary>
/// Steps de interação com elementos da UI.
/// Suporta cliques, preenchimento de campos, seleção, etc.
/// </summary>
[Binding]
public sealed class InteractionSteps
{
    private readonly AutomationRuntime _rt;

    public InteractionSteps(AutomationRuntime rt) => _rt = rt;

    #region When - Preenchimento de Campos

    /// <summary>
    /// Preenche um campo com um valor resolvido pelo DataResolver.
    /// Suporta literais, variáveis de ambiente ou chaves do DataMap.
    /// </summary>
    [When(@"eu preencho ""(.*)"" com ""(.*)""")]
    [When(@"eu preencho o campo ""(.*)"" com ""(.*)""")]
    [When(@"eu digito ""(.*)"" em ""(.*)""")]
    public void QuandoEuPreenchoCom(string elementRef, string dataKey)
    {
        _rt.Waits.WaitDomReady(_rt.Driver);
        _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        var value = ResolveValue(dataKey);
        el.Clear();
        el.SendKeys(value);

        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Preenche múltiplos campos a partir de um objeto de dados do DataMap.
    /// O objeto deve ter chaves que correspondam aos nomes amigáveis no UiMap.
    /// </summary>
    [When(@"eu preencho os campos com os dados de ""(.*)""")]
    public void QuandoEuPreenchoCamposComDadosDe(string dataKey)
    {
        
        var data = _rt.Data.Resolve(dataKey);
        if (data is IDictionary dataMap)
        {
            foreach (DictionaryEntry entry in dataMap)
            {
                var fieldName = entry.Key.ToString();
                var fieldValue = entry.Value?.ToString() ?? "";
                QuandoEuPreenchoCom(fieldName, fieldValue);
            }
        }
        else
        {
            throw new InvalidOperationException($"A chave de dados '{dataKey}' não resolveu para um objeto/dicionário válido.");
        }
    }

    /// <summary>
    /// Preenche um campo com o valor de uma variável de ambiente.
    /// </summary>
    [When(@"eu preencho ""(.*)"" com a variável ""(.*)""")]
    [When(@"eu preencho o campo ""(.*)"" com a variável ""(.*)""")]
    public void QuandoEuPreenchoComVariavel(string elementRef, string envVarName)
    {
        var value = Environment.GetEnvironmentVariable(envVarName) ?? "";
        QuandoEuPreenchoCom(elementRef, value);
    }

    /// <summary>
    /// Preenche um campo com as credenciais de usuário (variável TEST_USER).
    /// </summary>
    [When(@"eu preencho ""(.*)"" com as credenciais de usuário")]
    [When(@"eu preencho o campo ""(.*)"" com as credenciais de usuário")]
    public void QuandoEuPreenchoComCredenciaisDeUsuario(string elementRef)
    {
        var user = Environment.GetEnvironmentVariable("TEST_USER") ?? "admin";
        QuandoEuPreenchoCom(elementRef, user);
    }

    /// <summary>
    /// Preenche um campo com as credenciais de senha (variável TEST_PASS).
    /// </summary>
    [When(@"eu preencho ""(.*)"" com as credenciais de senha")]
    [When(@"eu preencho o campo ""(.*)"" com as credenciais de senha")]
    public void QuandoEuPreenchoComCredenciaisDeSenha(string elementRef)
    {
        var pass = Environment.GetEnvironmentVariable("TEST_PASS") ?? "admin";
        QuandoEuPreenchoCom(elementRef, pass);
    }

    /// <summary>
    /// Preenche um campo com um valor literal (NÃO resolve via DataMap).
    /// Use quando o valor deve ser usado exatamente como digitado.
    /// </summary>
    [When(@"eu preencho ""(.*)"" com valor literal ""(.*)""")]
    [When(@"eu preencho o campo ""(.*)"" com valor literal ""(.*)""")]
    public void QuandoEuPreenchoComValorLiteral(string elementRef, string literalValue)
    {
        _rt.Waits.WaitDomReady(_rt.Driver);
        _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        el.Clear();
        el.SendKeys(literalValue);

        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Limpa o conteúdo de um campo.
    /// </summary>
    [When(@"eu limpo o campo ""(.*)""")]
    [When(@"eu limpo ""(.*)""")]
    public void QuandoEuLimpoOCampo(string elementRef)
    {
        _rt.Waits.WaitDomReady(_rt.Driver);
        _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        el.Clear();

        _rt.Debug.MaybeSlowMo();
    }

    #endregion

    #region When - Cliques

    /// <summary>
    /// Clica em um elemento (botão, link, etc).
    /// Aguarda automaticamente a navegação se houver mudança de URL.
    /// </summary>
    [When(@"^eu clico em ""([^""]+)""$")]
    [When(@"eu clico no botão ""(.*)""")]
    [When(@"eu clico no elemento ""(.*)""")]
    public void QuandoEuClicoEm(string elementRef)
    {
        _rt.Waits.WaitDomReady(_rt.Driver);
        _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitClickableByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        var previousUrl = _rt.Driver.Url;
        el.Click();

        // Aguarda estabilização após clique
        _rt.Debug.MaybeSlowMo();

        // Verifica se houve navegação
        try
        {
            _rt.Waits.WaitForUrlChange(_rt.Driver, previousUrl, 2000);
            // Se URL mudou, aguarda DOM e Angular
            _rt.Waits.WaitDomReady(_rt.Driver);
            _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);
        }
        catch (WebDriverTimeoutException)
        {
            // URL não mudou - clique sem navegação (ex: toggle, modal)
            // Apenas aguarda DOM e Angular
            _rt.Waits.WaitDomReady(_rt.Driver);
            _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);
        }

        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Clica em um elemento e aguarda navegação para uma rota específica.
    /// </summary>
    [When(@"eu clico em ""(.*)"" e aguardo a rota ""(.*)""")]
    [When(@"eu clico no botão ""(.*)"" e aguardo a rota ""(.*)""")]
    public void QuandoEuClicoEAguardoRota(string elementRef, string expectedRoute)
    {
        _rt.Waits.WaitDomReady(_rt.Driver);
        _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitClickableByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        _rt.Waits.WaitForNavigationToRoute(_rt.Driver, () => el.Click(), expectedRoute);

        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Duplo clique em um elemento.
    /// </summary>
    [When(@"eu dou duplo clique em ""(.*)""")]
    public void QuandoEuDouDuploCliqueEm(string elementRef)
    {
        _rt.Waits.WaitDomReady(_rt.Driver);
        _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitClickableByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        var actions = new OpenQA.Selenium.Interactions.Actions(_rt.Driver);
        actions.DoubleClick(el).Perform();

        _rt.Debug.MaybeSlowMo();
    }

    #endregion

    #region When - Seleção

    /// <summary>
    /// Seleciona uma opção em um dropdown pelo texto visível.
    /// </summary>
    [When(@"eu seleciono ""(.*)"" em ""(.*)""")]
    [When(@"eu seleciono a opção ""(.*)"" em ""(.*)""")]
    public void QuandoEuSelecionoEm(string optionText, string elementRef)
    {
        _rt.Waits.WaitDomReady(_rt.Driver);
        _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        var select = new OpenQA.Selenium.Support.UI.SelectElement(el);
        select.SelectByText(optionText);

        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Seleciona uma opção em um dropdown pelo valor.
    /// </summary>
    [When(@"eu seleciono o valor ""(.*)"" em ""(.*)""")]
    public void QuandoEuSelecionoValorEm(string value, string elementRef)
    {
        _rt.Waits.WaitDomReady(_rt.Driver);
        _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        var select = new OpenQA.Selenium.Support.UI.SelectElement(el);
        select.SelectByValue(value);

        _rt.Debug.MaybeSlowMo();
    }

    #endregion

    #region When - Teclado

    /// <summary>
    /// Pressiona uma tecla especial (Enter, Tab, Escape, etc).
    /// </summary>
    [When(@"eu pressiono a tecla ""(.*)""")]
    [When(@"eu pressiono ""(.*)""")]
    public void QuandoEuPressionoTecla(string keyName)
    {

        var key = keyName.ToUpperInvariant() switch
        {
            "ENTER" => Keys.Enter,
            "TAB" => Keys.Tab,
            "ESCAPE" or "ESC" => Keys.Escape,
            "BACKSPACE" => Keys.Backspace,
            "DELETE" => Keys.Delete,
            "SPACE" or "ESPAÇO" => Keys.Space,
            "UP" or "CIMA" => Keys.ArrowUp,
            "DOWN" or "BAIXO" => Keys.ArrowDown,
            "LEFT" or "ESQUERDA" => Keys.ArrowLeft,
            "RIGHT" or "DIREITA" => Keys.ArrowRight,
            "HOME" => Keys.Home,
            "END" => Keys.End,
            "PAGEUP" => Keys.PageUp,
            "PAGEDOWN" => Keys.PageDown,
            "F1" => Keys.F1,
            "F2" => Keys.F2,
            "F3" => Keys.F3,
            "F4" => Keys.F4,
            "F5" => Keys.F5,
            _ => throw new ArgumentException($"Tecla não reconhecida: {keyName}")
        };

        var actions = new OpenQA.Selenium.Interactions.Actions(_rt.Driver);
        actions.SendKeys(key).Perform();

        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Pressiona uma tecla em um elemento específico.
    /// </summary>
    [When(@"eu pressiono ""(.*)"" em ""(.*)""")]
    public void QuandoEuPressionoTeclaEm(string keyName, string elementRef)
    {
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        var key = keyName.ToUpperInvariant() switch
        {
            "ENTER" => Keys.Enter,
            "TAB" => Keys.Tab,
            "ESCAPE" or "ESC" => Keys.Escape,
            _ => keyName
        };

        el.SendKeys(key);

        _rt.Debug.MaybeSlowMo();
    }

    #endregion

    #region When - Esperas Explícitas

    /// <summary>
    /// Aguarda um número específico de segundos.
    /// Use com moderação - prefira waits implícitos.
    /// </summary>
    [When(@"eu aguardo (\d+) segundos?")]
    public void QuandoEuAguardoSegundos(int seconds)
    {
        System.Threading.Thread.Sleep(seconds * 1000);
        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Aguarda um elemento estar visível.
    /// </summary>
    [When(@"eu aguardo o elemento ""(.*)"" estar visível")]
    public void QuandoEuAguardoElementoVisivel(string elementRef)
    {
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Aguarda um elemento desaparecer.
    /// </summary>
    [When(@"eu aguardo o elemento ""(.*)"" desaparecer")]
    public void QuandoEuAguardoElementoDesaparecer(string elementRef)
    {

        var rr = _rt.Resolver.Resolve(elementRef);
        _rt.Waits.WaitElementNotVisible(_rt.Driver, rr.CssLocator);

        _rt.Debug.MaybeSlowMo();
    }

    #endregion

    #region Escape Hatch - JavaScript Execution

    /// <summary>
    /// Executa um script JavaScript genérico no contexto da página.
    /// Útil para manipulação de localStorage, sessionStorage ou eventos globais.
    /// </summary>
    [When(@"eu executo o script JS ""(.*)""")]
    public void QuandoEuExecutoOScriptJS(string script)
    {
        _rt.Waits.WaitDomReady(_rt.Driver);
        
        try
        {
            var js = (OpenQA.Selenium.IJavaScriptExecutor)_rt.Driver;
            js.ExecuteScript(script);
            _rt.Debug.MaybeSlowMo();
        }
        catch (System.Exception ex)
        {
            throw new System.InvalidOperationException($"Falha ao executar script JS global: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Executa um script JavaScript em um elemento específico.
    /// O elemento é passado como o primeiro argumento (arguments[0]) para o script.
    /// Exemplo: Quando eu executo o script "arguments[0].click()" no elemento "submit"
    /// </summary>
    [When(@"eu executo o script ""(.*)"" no elemento ""(.*)""")]
    public void QuandoEuExecutoOScriptNoElemento(string script, string elementRef)
    {
        _rt.Waits.WaitDomReady(_rt.Driver);
        
        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        try
        {
            var js = (OpenQA.Selenium.IJavaScriptExecutor)_rt.Driver;
            js.ExecuteScript(script, el);
            _rt.Debug.MaybeSlowMo();
        }
        catch (System.Exception ex)
        {
            throw new System.InvalidOperationException($"Falha ao executar script no elemento '{elementRef}': {ex.Message}", ex);
        }
    }

    #endregion

    #region Helpers

    private string ResolveValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        // 1. Suporta ${VAR_NAME} (Variáveis de Ambiente)
        if (value.StartsWith("${") && value.EndsWith("}"))
        {
            var varName = value[2..^1];
            return Environment.GetEnvironmentVariable(varName) ?? "";
        }

        // 2. Suporta {{DATA_KEY}} (DataMap)
        if (value.StartsWith("{{") && value.EndsWith("}}"))
        {
            var dataKey = value[2..^1];
            return _rt.Data.Resolve(dataKey)?.ToString() ?? "";
        }

        // 3. Suporta @OBJECT_KEY (Referência de objeto no DataMap)
        if (value.StartsWith("@"))
        {
            return _rt.Data.Resolve(value)?.ToString() ?? "";
        }

        // 4. Valor literal - NÃO re-resolver via DataResolver
        return value;
    }

    #endregion
}
