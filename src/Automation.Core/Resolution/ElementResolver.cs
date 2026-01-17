using Automation.Core.UiMap;

namespace Automation.Core.Resolution;

public sealed record ResolutionResult(string PageName, string FriendlyName, string TestId, string CssLocator);

public sealed class ElementResolver
{
    private readonly UiMapModel _map;
    private readonly PageContext _pageContext;

    public ElementResolver(UiMapModel map, PageContext pageContext)
    {
        _map = map;
        _pageContext = pageContext;
    }

    public ResolutionResult Resolve(string friendlyName)
    {
        var pageName = _pageContext.CurrentPageName;
        if (string.IsNullOrWhiteSpace(pageName))
            throw new InvalidOperationException("PageContext is not set. Use step: 'Dado que estou na tela \"PageName\"'.");

        var page = _map.GetPageOrThrow(pageName);
        var testId = page.GetTestIdOrThrow(friendlyName);
        var css = LocatorFactory.CssByTestId(testId);
        return new ResolutionResult(pageName, friendlyName, testId, css);
    }
}
