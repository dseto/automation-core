using System.Collections.Generic;
using System.Text.Json;
using Automation.Core.Recorder;

namespace Automation.Core.Recorder.Draft;

public sealed class StepInferenceEngine
{
    public IReadOnlyList<DraftStep> InferSteps(IReadOnlyList<DraftAction> actions)
    {
        var steps = new List<DraftStep>();

        foreach (var action in actions)
        {
            var ev = action.PrimaryEvent;
            if (ev == null) continue;

            var text = InferStepText(ev);
            if (string.IsNullOrWhiteSpace(text))
                continue;

            steps.Add(new DraftStep(text, action.EventIndexes, 0.7));
        }

        return steps;
    }

    private static string? InferStepText(RecorderEvent ev)
    {
        var target = TryGetHint(ev.Target);
        var literal = TryGetLiteral(ev.Value);

        return ev.Type switch
        {
            "navigate" => $"Given I am on \"{ev.Route ?? "/"}\"",
            "fill" when !string.IsNullOrWhiteSpace(target) =>
                $"When I fill \"{target}\" with \"{literal ?? ""}\"",
            "click" when !string.IsNullOrWhiteSpace(target) =>
                $"When I click \"{target}\"",
            "submit" when !string.IsNullOrWhiteSpace(target) =>
                $"Then I click \"{target}\"",
            _ => null
        };
    }

    private static string? TryGetHint(object? target)
    {
        if (target is Dictionary<string, object?> dict && dict.TryGetValue("hint", out var hint))
            return hint?.ToString();

        if (target is JsonElement json && json.ValueKind == JsonValueKind.Object)
        {
            if (json.TryGetProperty("hint", out var hintProp))
                return hintProp.GetString();
        }

        return null;
    }

    private static string? TryGetLiteral(object? value)
    {
        if (value is Dictionary<string, object?> dict && dict.TryGetValue("literal", out var literal))
            return literal?.ToString();

        if (value is JsonElement json && json.ValueKind == JsonValueKind.Object)
        {
            if (json.TryGetProperty("literal", out var literalProp))
                return literalProp.GetString();
        }

        return null;
    }
}
