using System;
using System.Collections.Generic;
using System.Text.Json;
using Automation.Core.Recorder;

namespace Automation.Core.Recorder.Draft;

public sealed class SessionSanityChecker
{
    public SanityCheckResult Check(RecorderSession session)
    {
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(session.SessionId))
            warnings.Add("SESSION_ID_MISSING");

        if (session.StartedAt == default)
            warnings.Add("STARTED_AT_MISSING");

        if (session.EndedAt == null)
            warnings.Add("ENDED_AT_MISSING");

        var events = session.Events ?? new List<RecorderEvent>();
        if (events.Count == 0)
            warnings.Add("EVENTS_EMPTY");

        foreach (var ev in events)
        {
            if (string.IsNullOrWhiteSpace(ev.Type))
                warnings.Add("EVENT_TYPE_MISSING");

            if (string.IsNullOrWhiteSpace(ev.T))
                warnings.Add("EVENT_AT_MISSING");

            var hint = GetHint(ev.Target);
            if (!IsNavigate(ev.Type) && string.IsNullOrWhiteSpace(hint) && ev.RawAction == null)
                warnings.Add("TARGET_HINT_MISSING");
        }

        var hasSemantic = events.Exists(e => SemanticTypes.Contains(e.Type));
        if (!hasSemantic)
            warnings.Add("NO_SEMANTIC_EVENTS");

        if (warnings.Count > 0)
        {
            return SanityCheckResult.InvalidWithWarning("Sessão inválida para geração de draft.", warnings);
        }

        return SanityCheckResult.Valid();
    }

    private static readonly HashSet<string> SemanticTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "click",
        "fill",
        "select",
        "submit"
    };

    private static bool IsNavigate(string? type)
        => string.Equals(type, "navigate", StringComparison.OrdinalIgnoreCase);

    private static string? GetHint(object? target)
    {
        if (target is Dictionary<string, object?> dict && dict.TryGetValue("hint", out var hint))
            return hint?.ToString();

        if (target is Dictionary<string, object> obj && obj.TryGetValue("hint", out var hintObj))
            return hintObj?.ToString();

        if (target is JsonElement json && json.ValueKind == JsonValueKind.Object)
        {
            if (json.TryGetProperty("hint", out var hintProp))
                return hintProp.GetString();
        }

        return null;
    }
}

public sealed class SanityCheckResult
{
    public bool IsValid { get; init; }

    public string Status { get; init; } = "";

    public string? Warning { get; init; }

    public string? Error { get; init; }

    public List<string> Warnings { get; init; } = new();

    public static SanityCheckResult Valid() => new()
    {
        IsValid = true,
        Status = "valid"
    };

    public static SanityCheckResult WithWarning(string message) => new()
    {
        IsValid = true,
        Status = "warning",
        Warning = message
    };

    public static SanityCheckResult InvalidWithWarning(string message, List<string>? warnings = null) => new()
    {
        IsValid = false,
        Status = "invalid",
        Warning = message,
        Error = message,
        Warnings = warnings ?? new List<string>()
    };

    public static SanityCheckResult Invalid(string message) => new()
    {
        IsValid = false,
        Status = "invalid",
        Error = message
    };

}
