using System;
using Automation.Reqnroll.Runtime;
using Reqnroll;
using Xunit;

namespace Automation.Reqnroll.Steps;

/// <summary>
/// Steps de validação de elementos e estados da UI.
/// Suporta validação de visibilidade, texto, atributos, etc.
/// </summary>
[Binding]
public sealed class ValidationSteps
{
    private readonly AutomationRuntime _rt;

    public ValidationSteps(AutomationRuntime rt) => _rt = rt;

    #region Then - Navegação

    /// <summary>
    /// Valida que a URL atual contém a rota especificada.
    /// </summary>
    [Then(@"a rota deve ser ""(.*)""")]
    [Then(@"a URL deve conter ""(.*)""")]
    public void EntaoARotaDeveSer(string expectedRoute)
    {
        _rt.Debug.MaybePauseEachStep($"verify route {expectedRoute}");
        _rt.Waits.WaitForUrlContains(_rt.Driver, expectedRoute);
        
        Assert.Contains(expectedRoute, _rt.Driver.Url, StringComparison.OrdinalIgnoreCase);
        _rt.Debug.MaybeSlowMo();
    }

    #endregion

    #region Then - Visibilidade

    /// <summary>
    /// Valida que um elemento está visível na tela.
    /// Suporta nomes simples ("username") ou completos ("login.username").
    /// </summary>
    [Then(@"o elemento ""(.*)"" deve estar visível")]
    [Then(@"o elemento ""(.*)"" está visível")]
    [Given(@"o elemento ""(.*)"" deve estar visível")]
    [Given(@"o elemento ""(.*)"" está visível")]
    public void EntaoOElementoDeveEstarVisivel(string elementRef)
    {
        _rt.Debug.MaybePauseEachStep($"verify visible {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);
        _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        Assert.True(el.Displayed, $"Elemento '{elementRef}' deveria estar visível.");
        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Valida que um elemento NÃO está visível na tela.
    /// </summary>
    [Then(@"o elemento ""(.*)"" não deve estar visível")]
    [Then(@"o elemento ""(.*)"" não está visível")]
    public void EntaoOElementoNaoDeveEstarVisivel(string elementRef)
    {
        _rt.Debug.MaybePauseEachStep($"verify not visible {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);

        try
        {
            _rt.Waits.WaitElementNotVisible(_rt.Driver, rr.CssLocator, 3000);
        }
        catch
        {
            Assert.Fail($"Elemento '{elementRef}' deveria NÃO estar visível, mas está.");
        }

        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Valida que um elemento existe no DOM (pode estar oculto).
    /// </summary>
    [Then(@"o elemento ""(.*)"" deve existir")]
    public void EntaoOElementoDeveExistir(string elementRef)
    {
        _rt.Debug.MaybePauseEachStep($"verify exists {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var elements = _rt.Driver.FindElements(OpenQA.Selenium.By.CssSelector(rr.CssLocator));

        Assert.True(elements.Count > 0, $"Elemento '{elementRef}' deveria existir no DOM.");
        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Valida que um elemento NÃO existe no DOM.
    /// </summary>
    [Then(@"o elemento ""(.*)"" não deve existir")]
    public void EntaoOElementoNaoDeveExistir(string elementRef)
    {
        _rt.Debug.MaybePauseEachStep($"verify not exists {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var elements = _rt.Driver.FindElements(OpenQA.Selenium.By.CssSelector(rr.CssLocator));

        Assert.True(elements.Count == 0, $"Elemento '{elementRef}' NÃO deveria existir no DOM.");
        _rt.Debug.MaybeSlowMo();
    }

    #endregion

    #region Then - Texto

    /// <summary>
    /// Valida que um elemento contém um texto específico.
    /// </summary>
    [Then(@"o elemento ""(.*)"" deve conter o texto ""(.*)""")]
    [Then(@"o elemento ""(.*)"" contém o texto ""(.*)""")]
    public void EntaoOElementoDeveConterTexto(string elementRef, string expectedText)
    {
        _rt.Debug.MaybePauseEachStep($"verify text contains {expectedText} in {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitElementContainsText(_rt.Driver, rr.CssLocator, expectedText);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        Assert.Contains(expectedText, el.Text, StringComparison.OrdinalIgnoreCase);
        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Valida que um elemento tem exatamente o texto especificado.
    /// </summary>
    [Then(@"o elemento ""(.*)"" deve ter o texto ""(.*)""")]
    [Then(@"o texto de ""(.*)"" deve ser ""(.*)""")]
    public void EntaoOElementoDeveTerTexto(string elementRef, string expectedText)
    {
        _rt.Debug.MaybePauseEachStep($"verify text equals {expectedText} in {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        Assert.Equal(expectedText, el.Text.Trim());
        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Valida que um elemento NÃO contém um texto específico.
    /// </summary>
    [Then(@"o elemento ""(.*)"" não deve conter o texto ""(.*)""")]
    public void EntaoOElementoNaoDeveConterTexto(string elementRef, string text)
    {
        _rt.Debug.MaybePauseEachStep($"verify text not contains {text} in {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        Assert.DoesNotContain(text, el.Text, StringComparison.OrdinalIgnoreCase);
        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Valida que um elemento está vazio (sem texto).
    /// </summary>
    [Then(@"o elemento ""(.*)"" deve estar vazio")]
    public void EntaoOElementoDeveEstarVazio(string elementRef)
    {
        _rt.Debug.MaybePauseEachStep($"verify empty {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        var text = el.Text?.Trim() ?? "";
        var value = el.GetDomAttribute("value")?.Trim() ?? "";

        Assert.True(string.IsNullOrEmpty(text) && string.IsNullOrEmpty(value),
            $"Elemento '{elementRef}' deveria estar vazio, mas contém: text='{text}', value='{value}'");

        _rt.Debug.MaybeSlowMo();
    }

    #endregion

    #region Then - Atributos

    /// <summary>
    /// Valida que um elemento tem um atributo com valor específico.
    /// </summary>
    [Then(@"o atributo ""(.*)"" de ""(.*)"" deve ser ""(.*)""")]
    [Then(@"o atributo ""(.*)"" do elemento ""(.*)"" deve ser ""(.*)""")]
    public void EntaoOAtributoDeveSer(string attributeName, string elementRef, string expectedValue)
    {
        _rt.Debug.MaybePauseEachStep($"verify attribute {attributeName}={expectedValue} in {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitElementAttribute(_rt.Driver, rr.CssLocator, attributeName, expectedValue);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        var actualValue = el.GetDomAttribute(attributeName);
        Assert.Equal(expectedValue, actualValue);

        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Valida que um elemento tem um atributo que contém um valor.
    /// </summary>
    [Then(@"o atributo ""(.*)"" de ""(.*)"" deve conter ""(.*)""")]
    public void EntaoOAtributoDeveConter(string attributeName, string elementRef, string expectedValue)
    {
        _rt.Debug.MaybePauseEachStep($"verify attribute {attributeName} contains {expectedValue} in {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        var actualValue = el.GetDomAttribute(attributeName) ?? "";
        Assert.Contains(expectedValue, actualValue, StringComparison.OrdinalIgnoreCase);

        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Valida o valor de um campo de input.
    /// </summary>
    [Then(@"o valor de ""(.*)"" deve ser ""(.*)""")]
    [Then(@"o campo ""(.*)"" deve ter o valor ""(.*)""")]
    public void EntaoOValorDeveSer(string elementRef, string expectedValue)
    {
        EntaoOAtributoDeveSer("value", elementRef, expectedValue);
    }

    #endregion

    #region Then - Estado

    /// <summary>
    /// Valida que um elemento está habilitado.
    /// </summary>
    [Then(@"o elemento ""(.*)"" deve estar habilitado")]
    [Then(@"o elemento ""(.*)"" está habilitado")]
    public void EntaoOElementoDeveEstarHabilitado(string elementRef)
    {
        _rt.Debug.MaybePauseEachStep($"verify enabled {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        Assert.True(el.Enabled, $"Elemento '{elementRef}' deveria estar habilitado.");
        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Valida que um elemento está desabilitado.
    /// </summary>
    [Then(@"o elemento ""(.*)"" deve estar desabilitado")]
    [Then(@"o elemento ""(.*)"" está desabilitado")]
    public void EntaoOElementoDeveEstarDesabilitado(string elementRef)
    {
        _rt.Debug.MaybePauseEachStep($"verify disabled {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        Assert.False(el.Enabled, $"Elemento '{elementRef}' deveria estar desabilitado.");
        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Valida que um checkbox/radio está marcado.
    /// </summary>
    [Then(@"o elemento ""(.*)"" deve estar marcado")]
    [Then(@"o elemento ""(.*)"" está marcado")]
    public void EntaoOElementoDeveEstarMarcado(string elementRef)
    {
        _rt.Debug.MaybePauseEachStep($"verify checked {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        Assert.True(el.Selected, $"Elemento '{elementRef}' deveria estar marcado.");
        _rt.Debug.MaybeSlowMo();
    }

    /// <summary>
    /// Valida que um checkbox/radio NÃO está marcado.
    /// </summary>
    [Then(@"o elemento ""(.*)"" não deve estar marcado")]
    [Then(@"o elemento ""(.*)"" não está marcado")]
    public void EntaoOElementoNaoDeveEstarMarcado(string elementRef)
    {
        _rt.Debug.MaybePauseEachStep($"verify not checked {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        Assert.False(el.Selected, $"Elemento '{elementRef}' NÃO deveria estar marcado.");
        _rt.Debug.MaybeSlowMo();
    }

    #endregion

    #region Then - Contagem

    /// <summary>
    /// Valida a quantidade de elementos encontrados.
    /// </summary>
    [Then(@"devem existir (\d+) elementos? ""(.*)""")]
    public void EntaoDevemExistirElementos(int expectedCount, string elementRef)
    {
        _rt.Debug.MaybePauseEachStep($"verify count {expectedCount} of {elementRef}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(elementRef);
        var elements = _rt.Driver.FindElements(OpenQA.Selenium.By.CssSelector(rr.CssLocator));

        Assert.Equal(expectedCount, elements.Count);
        _rt.Debug.MaybeSlowMo();
    }

    #endregion
}
