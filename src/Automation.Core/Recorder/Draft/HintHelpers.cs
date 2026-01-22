using System.Text.RegularExpressions;

namespace Automation.Core.Recorder.Draft;

internal static class HintHelpers
{
    public static string NormalizeHint(string? hint)
    {
        if (hint == null) return string.Empty;

        var normalized = hint.Trim();
        normalized = Regex.Replace(normalized, "\\s+", " ");
        if (normalized.Contains('[') && normalized.Contains(']'))
            normalized = normalized.Replace('"', '\'');

        return normalized;
    }

    public static bool IsGenericHint(string hint)
    {
        if (string.IsNullOrWhiteSpace(hint)) return true;

        var normalized = NormalizeHint(hint);
        if (string.IsNullOrWhiteSpace(normalized)) return true;

        if (normalized is "div" or "main" or "body" or "html")
            return true;

        if (normalized.Contains("#") || normalized.Contains("["))
            return false;

        if (normalized.Contains("('") || normalized.Contains("(role=") || normalized.Contains("(label="))
            return false;

        // Treat data-testid-like tokens (e.g., page.login.username, login-username) as specific.
        if (Regex.IsMatch(normalized, "^[A-Za-z0-9_.:-]+$"))
            return false;

        return true;
    }
}
