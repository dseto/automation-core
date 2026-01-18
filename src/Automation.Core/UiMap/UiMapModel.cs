using System.Collections.Generic;

namespace Automation.Core.UiMap;

/// <summary>
/// In-memory representation of ui-map.yaml.
/// </summary>
public sealed class UiMapModel
{
    public Dictionary<string, UiPage> Pages { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, UiPage> Modals { get; } = new(StringComparer.OrdinalIgnoreCase);

    public UiPage GetPageOrThrow(string pageName)
    {
        if (Pages.TryGetValue(pageName, out var page)) return page;
        if (Modals.TryGetValue(pageName, out var modal)) return modal;
        throw new KeyNotFoundException($"Page '{pageName}' was not found in ui-map.");
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
