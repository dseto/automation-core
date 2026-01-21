using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Automation.Core.Recorder;

public static class TargetHintBuilder
{
    public static string BuildHint(IDictionary<string, object> target)
    {
        var dataTestId = GetAttribute(target, "data-testid");
        if (!string.IsNullOrWhiteSpace(dataTestId))
            return CssAttribute("data-testid", dataTestId);

        var id = GetAttribute(target, "id");
        if (!string.IsNullOrWhiteSpace(id) && !IsDynamicId(id))
            return CssAttribute("id", id);

        var name = GetAttribute(target, "name");
        if (!string.IsNullOrWhiteSpace(name))
            return CssAttribute("name", name);

        var formControlName = GetAttribute(target, "formcontrolname");
        if (!string.IsNullOrWhiteSpace(formControlName))
            return CssAttribute("formcontrolname", formControlName);

        var ariaLabel = GetAttribute(target, "aria-label");
        if (!string.IsNullOrWhiteSpace(ariaLabel))
            return CssAttribute("aria-label", ariaLabel);

        var placeholder = GetAttribute(target, "placeholder");
        if (!string.IsNullOrWhiteSpace(placeholder))
            return CssAttribute("placeholder", placeholder);

        var role = GetAttribute(target, "role");
        var tag = GetTargetValue(target, "tag") ?? "element";
        var text = GetTargetValue(target, "text");

        if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(role))
            return $"{tag} (role='{NormalizeValue(role)}' text='{NormalizeValue(text)}')";

        if (!string.IsNullOrWhiteSpace(text))
            return $"{tag} ('{NormalizeValue(text)}')";

        if (!string.IsNullOrWhiteSpace(role))
            return $"{tag} (role='{NormalizeValue(role)}')";

        return tag;
    }

    private static string CssAttribute(string name, string value)
    {
        return $"[{name}='{NormalizeValue(value)}']";
    }

    private static string NormalizeValue(string value)
    {
        return value.Replace("'", "\\'");
    }

    private static string? GetAttribute(IDictionary<string, object> target, string name)
    {
        if (!target.TryGetValue("attributes", out var attributesObj))
            return null;

        if (attributesObj is IDictionary<string, object> dict && dict.TryGetValue(name, out var raw))
            return GetString(raw);

        if (attributesObj is JsonElement json && json.ValueKind == JsonValueKind.Object)
        {
            if (json.TryGetProperty(name, out var prop))
                return prop.GetString();
        }

        return null;
    }

    private static string? GetTargetValue(IDictionary<string, object> target, string name)
    {
        if (!target.TryGetValue(name, out var raw))
            return null;

        return GetString(raw);
    }

    private static string? GetString(object? raw)
    {
        return raw switch
        {
            null => null,
            string s => s,
            JsonElement json when json.ValueKind == JsonValueKind.String => json.GetString(),
            _ => raw.ToString()
        };
    }

    private static bool IsDynamicId(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return true;

        return id.StartsWith("mat-input-", StringComparison.OrdinalIgnoreCase)
               || id.StartsWith("mat-option-", StringComparison.OrdinalIgnoreCase)
               || id.StartsWith("cdk-", StringComparison.OrdinalIgnoreCase);
    }
}
