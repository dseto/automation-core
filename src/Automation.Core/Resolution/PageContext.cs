namespace Automation.Core.Resolution;

public sealed class PageContext
{
    public string? CurrentPageName { get; private set; }
    public void SetPage(string pageName) => CurrentPageName = pageName;
}
