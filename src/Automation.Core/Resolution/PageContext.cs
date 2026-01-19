using System;
using System.Collections.Generic;
using Automation.Core.UiMap;
using OpenQA.Selenium;

namespace Automation.Core.Resolution;

public sealed class PageContext
{
    private readonly Stack<string> _pageHistory = new();
    private readonly UiMapModel _uiMap;
    private readonly IWebDriver _driver;

    public string? CurrentPageName { get; private set; }

    public PageContext(UiMapModel uiMap, IWebDriver driver)
    {
        _uiMap = uiMap;
        _driver = driver;
    }

    public void SetPage(string pageName)
    {
        if (!string.IsNullOrWhiteSpace(CurrentPageName))
            _pageHistory.Push(CurrentPageName);

        CurrentPageName = pageName;
    }

    public void SetCurrentPage(string pageName) => SetPage(pageName);

    public void GoBack()
    {
        if (_pageHistory.Count > 0)
            CurrentPageName = _pageHistory.Pop();
    }

    public bool HasPage => !string.IsNullOrWhiteSpace(CurrentPageName);

    public void Clear()
    {
        CurrentPageName = null;
        _pageHistory.Clear();
    }

    public string GetCurrentPageOrThrow()
    {
        if (string.IsNullOrWhiteSpace(CurrentPageName))
            throw new InvalidOperationException(
                "Nenhuma página foi definida no contexto. " +
                "Use o step 'Dado que estou na página \"NomeDaPagina\"' antes de interagir com elementos.");

        return CurrentPageName;
    }

    public UiPage GetPageDefinition(string pageName)
    {
        return _uiMap.GetPageOrThrow(pageName);
    }

    public void NavigateTo(string url)
    {
        _driver.Navigate().GoToUrl(url);
    }
}
