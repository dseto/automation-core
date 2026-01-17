
using Automation.Reqnroll.Runtime;
using Reqnroll;
using Xunit;

namespace Automation.Reqnroll.Steps;

[Binding]
public sealed class BasicSteps
{
    private readonly AutomationRuntime _rt;

    public BasicSteps(AutomationRuntime rt) => _rt = rt;

    [Given(@"que estou na tela ""(.*)""")]
    public void DadoQueEstouNaTela(string pageName)
    {
        _rt.PageContext.SetPage(pageName);

        var page = _rt.UiMap.GetPageOrThrow(pageName);
        if (!string.IsNullOrWhiteSpace(page.Meta?.Route))
        {
            var baseUrl = (_rt.Settings.BaseUrl ?? "").TrimEnd('/');
            var route = page.Meta.Route.StartsWith("/") ? page.Meta.Route : "/" + page.Meta.Route;
            var url = string.IsNullOrWhiteSpace(baseUrl) ? route : baseUrl + route;
            _rt.Driver.Navigate().GoToUrl(url);
        }
    }

    [When(@"eu preencho o campo ""(.*)"" com ""(.*)""")]
    public void QuandoEuPreenchoOCampo(string element, string value)
    {
        _rt.Debug.MaybePauseEachStep($"fill {element}");
        _rt.Waits.WaitDomReady(_rt.Driver);
        _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);

        var rr = _rt.Resolver.Resolve(element);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        el.Clear();
        el.SendKeys(value);

        _rt.Debug.MaybeSlowMo();
    }

    [When(@"eu clico no botão ""(.*)""")]
    public void QuandoEuClicoNoBotao(string element)
    {
        _rt.Debug.MaybePauseEachStep($"click {element}");
        _rt.Waits.WaitDomReady(_rt.Driver);
        _rt.Waits.TryWaitAngularStable(_rt.Driver, out _);

        var rr = _rt.Resolver.Resolve(element);
        var el = _rt.Waits.WaitClickableByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        el.Click();

        _rt.Debug.MaybeSlowMo();
    }

    [Then(@"a rota deve ser ""(.*)""")]
    public void EntaoARotaDeveSer(string route)
    {
        _rt.Debug.MaybePauseEachStep($"route {route}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        Assert.Contains(route, _rt.Driver.Url);
        _rt.Debug.MaybeSlowMo();
    }

    [Then(@"o elemento ""(.*)"" deve estar visível")]
    public void EntaoOElementoDeveEstarVisivel(string element)
    {
        _rt.Debug.MaybePauseEachStep($"visible {element}");
        _rt.Waits.WaitDomReady(_rt.Driver);

        var rr = _rt.Resolver.Resolve(element);
        var el = _rt.Waits.WaitVisibleByCss(_rt.Driver, rr.CssLocator);
        _rt.Debug.TryHighlight(_rt.Driver, el);

        Assert.True(el.Displayed);
        _rt.Debug.MaybeSlowMo();
    }
}
