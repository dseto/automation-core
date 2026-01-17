using System.Collections.Generic;

namespace Automation.Core.UiMap;

public sealed class UiMapModel
{
    public Dictionary<string, UiPage> Pages { get; } = new(StringComparer.OrdinalIgnoreCase);

    public UiPage GetPageOrThrow(string pageName)
    {
        if (!Pages.TryGetValue(pageName, out var page))
            throw new KeyNotFoundException($"Page '{pageName}' was not found in ui-map.");
        return page;
    }
}

public sealed class UiPage
{
    public UiMeta? Meta { get; set; }
    public Dictionary<string, string> Elements { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string GetTestIdOrThrow(string friendlyName)
    {
        if (!Elements.TryGetValue(friendlyName, out var testId))
            throw new KeyNotFoundException($"Element '{friendlyName}' was not found in ui-map for this page.");
        return testId;
    }
}

public sealed class UiMeta
{
    public string? Route { get; set; }
    public string? Anchor { get; set; }
}
