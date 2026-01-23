using System;
using Automation.Core.Resolution;
using Automation.Core.Waits;
using Automation.Reqnroll.Runtime;
using Reqnroll;
using OpenQA.Selenium;
using Microsoft.Extensions.Logging;

namespace Automation.Reqnroll.Steps;

[Binding]
public class NavigationSteps
{
    private readonly AutomationRuntime _rt;
    private readonly PageContext _pageContext;
    private readonly WaitService _waitService;
    private readonly ScenarioContext _scenarioContext;
    private readonly IWebDriver _driver;

    public NavigationSteps(AutomationRuntime rt, PageContext pageContext, WaitService waitService, ScenarioContext scenarioContext, IWebDriver driver)
    {
        _rt = rt;
        _pageContext = pageContext;
        _waitService = waitService;
        _scenarioContext = scenarioContext;
        _driver = driver;
    }

    [Given(@"que a aplicação está em ""(.*)""")]
    public void DadoQueAAplicacaoEstaEm(string url)
    {
        if (url.Contains("${BASE_URL}"))
        {
            var baseUrl = Environment.GetEnvironmentVariable("BASE_URL");
            if (string.IsNullOrEmpty(baseUrl))
                throw new InvalidOperationException("Variável de ambiente BASE_URL não definida.");
            
            url = url.Replace("${BASE_URL}", baseUrl.TrimEnd('/'));
        }
        
        _scenarioContext["BASE_URL"] = url;
    }

    [Given(@"que estou na página ""(.*)""")]
    public void DadoQueEstouNaPagina(string pageName)
    {
        var page = _pageContext.GetPageDefinition(pageName);
        if (page == null)
            throw new ArgumentException($"Página '{pageName}' não encontrada no UiMap.");

        if (!_scenarioContext.TryGetValue("BASE_URL", out string baseUrl))
            throw new InvalidOperationException("BASE_URL não definida. Use o step 'Dado que a aplicação está em...' primeiro.");

        var route = page.Route ?? "";

        // If route is a wildcard or not specified, do not attempt navigation — it represents a page fragment/component
        if (string.IsNullOrWhiteSpace(route) || route.Trim() == "*" || route.Contains("*"))
        {
            // If an anchor is defined for this page, wait for it as a heuristic that the page/fragment is available
            if (!string.IsNullOrWhiteSpace(page.Anchor))
            {
                try
                {
                    // Try a short wait for anchor; don't fail the scenario if it's not present (wildcard pages may be present across routes)
                    _waitService.WaitPageAnchor(_driver, page.Anchor, 2000);
                }
                catch (OpenQA.Selenium.WebDriverTimeoutException)
                {
                    _rt?.Logger?.LogWarning("Anchor '{Anchor}' not found within short timeout for page '{Page}'. Continuing without navigation.", page.Anchor, pageName);
                }
            }

            _pageContext.SetCurrentPage(pageName);
            return;
        }

        var fullUrl = $"{baseUrl.TrimEnd('/')}/{route.TrimStart('/')}";

        _pageContext.NavigateTo(fullUrl);
        _pageContext.SetCurrentPage(pageName);
        _rt.Recorder?.RecordNavigate(route);
    }

    [Then(@"estou na página ""(.*)""")]
    public void EntaoEstouNaPagina(string pageName)
    {
        var page = _pageContext.GetPageDefinition(pageName);
        if (page == null)
            throw new ArgumentException($"Página '{pageName}' não encontrada no UiMap.");

        var route = page.Route ?? "";
        _waitService.WaitForUrlContains(_driver, route);
        _pageContext.SetCurrentPage(pageName);
    }
}
