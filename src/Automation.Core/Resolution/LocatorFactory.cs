namespace Automation.Core.Resolution;

public static class LocatorFactory
{
    public static string CssByTestId(string testId) => $"[data-testid='{testId}']";
}
