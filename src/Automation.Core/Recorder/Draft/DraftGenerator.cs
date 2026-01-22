using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Automation.Core.Recorder;

namespace Automation.Core.Recorder.Draft;

public sealed class DraftGenerator
{
    public DraftGenerator(
        SessionSanityChecker sanityChecker,
        ActionGrouper grouper,
        StepInferenceEngine inferenceEngine,
        EscapeHatchRenderer escapeHatchRenderer,
        DraftWriter writer)
    {
        SanityChecker = sanityChecker;
        Grouper = grouper;
        InferenceEngine = inferenceEngine;
        EscapeHatchRenderer = escapeHatchRenderer;
        Writer = writer;
    }

    public SessionSanityChecker SanityChecker { get; }

    public ActionGrouper Grouper { get; }

    public StepInferenceEngine InferenceEngine { get; }

    public EscapeHatchRenderer EscapeHatchRenderer { get; }

    public DraftWriter Writer { get; }

    public DraftGenerationResult Generate(RecorderSession session, string outputDir)
    {
        var sanity = SanityChecker.Check(session);
        if (!sanity.IsValid)
        {
            var metadata = new DraftMetadata
            {
                SessionId = session.SessionId,
                InputStatus = sanity.Status,
                GeneratedAt = DateTimeOffset.UtcNow.ToString("O"),
                EventsCount = session.Events.Count,
                ActionsCount = 0,
                StepsInferredCount = 0,
                EscapeHatchCount = 0,
                Warnings = new List<string>(sanity.Warnings)
            };

            var metadataPath = Writer.WriteMetadata(metadata, outputDir);
            return new DraftGenerationResult
            {
                IsSuccess = false,
                InputStatus = sanity.Status,
                Warning = sanity.Warning ?? sanity.Error,
                MetadataPath = metadataPath
            };
        }

        var actions = Grouper.Group(session);
        var steps = InferenceEngine.InferSteps(actions);
        var stepsByIndex = steps.ToDictionary(s => s.EventIndexes.First(), s => s);

        var featureName = "Fluxo de login (draft)";
        var scenarioName = "Cenário draft gerado pelo Recorder";
        var lines = new List<string>
        {
            "#language: pt",
            "",
            $"Funcionalidade: {featureName}",
            "",
            $"Cenário: {scenarioName}",
            ""
        };

        var mappings = new List<DraftMapping>();
        var warnings = new List<string>();
        var escapeHatchCount = 0;
        string? lastKeyword = null;

        const string indent = "  ";
        for (var i = 0; i < actions.Count; i++)
        {
            var actionKey = actions[i].EventIndexes.FirstOrDefault();
            if (stepsByIndex.TryGetValue(actionKey, out var step))
            {
                var text = BuildStepText(actions[i]);
                if (string.IsNullOrWhiteSpace(text))
                {
                    var escape = EscapeHatchRenderer.Render(actions[i]);
                    lines.AddRange(IndentLines(escape.Lines, indent));
                    warnings.AddRange(escape.Warnings);
                    escapeHatchCount++;
                    lastKeyword = null;
                    continue;
                }

                text = NormalizeStepText(text, lastKeyword, out var keyword);
                lastKeyword = keyword;

                var primaryEvent = actions[i].PrimaryEvent;
                if (primaryEvent?.WaitMs != null)
                {
                    var secondsInt = primaryEvent.WaitMs.Value / 1000;
                    if (secondsInt >= 1)
                    {
                        lines.Add(indent + $"E eu espero {secondsInt} segundos");
                    }
                }

                lines.Add(indent + text);

                mappings.Add(new DraftMapping
                {
                    EventIndex = step.EventIndexes.FirstOrDefault(),
                    ActionIndex = i,
                    DraftLine = lines.Count,
                    Confidence = step.Confidence
                });

                continue;
            }

            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                lines.Add("");

            var escapeResult = EscapeHatchRenderer.Render(actions[i]);

            var primaryEventForEscape = actions[i].PrimaryEvent;
            if (primaryEventForEscape?.WaitMs != null)
            {
                var secondsInt = primaryEventForEscape.WaitMs.Value / 1000;
                if (secondsInt >= 1)
                {
                    lines.Add(indent + $"E eu espero {secondsInt} segundos");
                }
            }

            lines.AddRange(IndentLines(escapeResult.Lines, indent));
            warnings.AddRange(escapeResult.Warnings);
            escapeHatchCount++;

            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                lines.Add("");

            lastKeyword = null;
        }

        var content = string.Join(Environment.NewLine, lines).TrimEnd() + Environment.NewLine;
        var draftPath = Writer.WriteFeature(content, outputDir);

        var draftMetadata = new DraftMetadata
        {
            SessionId = session.SessionId,
            InputStatus = "valid",
            GeneratedAt = DateTimeOffset.UtcNow.ToString("O"),
            EventsCount = session.Events.Count,
            ActionsCount = actions.Count,
            StepsInferredCount = mappings.Count,
            EscapeHatchCount = escapeHatchCount,
            Warnings = warnings,
            Mappings = mappings
        };

        var metadataOut = Writer.WriteMetadata(draftMetadata, outputDir);

        return new DraftGenerationResult
        {
            IsSuccess = true,
            InputStatus = "valid",
            DraftPath = draftPath,
            MetadataPath = metadataOut
        };
    }

    private static string NormalizeStepText(string text, string? lastKeyword, out string? resultingKeyword)
    {
        var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            resultingKeyword = lastKeyword;
            return text;
        }

        var keyword = parts[0];
        if (lastKeyword != null && keyword.Equals(lastKeyword, StringComparison.OrdinalIgnoreCase) && !keyword.Equals("E", StringComparison.OrdinalIgnoreCase))
        {
            resultingKeyword = lastKeyword;
            return parts.Length > 1 ? $"E {parts[1]}" : "E";
        }

        if (keyword.Equals("E", StringComparison.OrdinalIgnoreCase))
        {
            resultingKeyword = lastKeyword ?? keyword;
            return text;
        }

        resultingKeyword = keyword;
        return text;
    }

    private static string? BuildStepText(DraftAction action)
    {
        var ev = action.PrimaryEvent;
        if (ev == null) return null;

        if (ev.Type == "navigate")
            return $"Dado que estou na página \"{ev.Route ?? "/"}\"";

        var target = NormalizeElementRef(TryGetHint(ev.Target));
        var literal = TryGetLiteral(ev.Value);

        if (string.IsNullOrWhiteSpace(target) || IsGenericHint(target))
            return null;

        return ev.Type switch
        {
            "fill" when !string.IsNullOrWhiteSpace(target) =>
                $"Quando eu preencho \"{target}\" com \"{literal ?? ""}\"",
            "click" when !string.IsNullOrWhiteSpace(target) =>
                $"Quando eu clico em \"{target}\"",
            "submit" when !string.IsNullOrWhiteSpace(target) =>
                $"Quando eu clico em \"{target}\"",
            _ => null
        };
    }

    private static string? NormalizeElementRef(string? elementRef)
    {
        if (string.IsNullOrWhiteSpace(elementRef)) return elementRef;
        return elementRef.Replace('"', '\'');
    }

    private static bool IsGenericHint(string hint)
    {
        var normalized = NormalizeHint(hint);
        if (string.IsNullOrWhiteSpace(normalized)) return true;

        if (normalized is "div" or "main" or "body" or "html")
            return true;

        if (normalized.Contains("#") || normalized.Contains("["))
            return false;

        if (normalized.Contains("('") || normalized.Contains("(role=") || normalized.Contains("(label="))
            return false;

        // Treat data-testid-like tokens (e.g., page.login.username, login-username) as specific.
        // Accept alphanumeric and common separators as identifiers.
        if (System.Text.RegularExpressions.Regex.IsMatch(normalized, "^[A-Za-z0-9_.:-]+$"))
            return false;

        return true;
    }

    private static string NormalizeHint(string? hint)
    {
        if (hint == null) return string.Empty;

        var normalized = hint.Trim();
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "\\s+", " ");
        if (normalized.Contains('[') && normalized.Contains(']'))
            normalized = normalized.Replace('"', '\'');

        return normalized;
    }

    private static string? TryGetHint(object? target)
    {
            // Prefer attributes.data-testid when available
            if (target is Dictionary<string, object?> dict)
            {
                if (dict.TryGetValue("attributes", out var attrsObj) && attrsObj is Dictionary<string, object?> attrsDict && attrsDict.TryGetValue("data-testid", out var dt) && dt != null)
                {
                    return dt.ToString();
                }

                if (dict.TryGetValue("hint", out var hint))
                {
                    var hintStr = hint?.ToString();
                    // try extract data-testid from hint like [data-testid='x']
                    var m = System.Text.RegularExpressions.Regex.Match(hintStr ?? string.Empty, "data-testid\\s*=\\s*[\'\"](?<id>[^\'\"]+)[\'\"]", System.Text.RegularExpressions.RegexOptions.Compiled);
                    if (m.Success) return m.Groups["id"].Value;
                    return hintStr;
                }
            }

            if (target is System.Text.Json.JsonElement json && json.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                if (json.TryGetProperty("attributes", out var attrs) && attrs.ValueKind == System.Text.Json.JsonValueKind.Object && attrs.TryGetProperty("data-testid", out var dtv) && dtv.ValueKind == System.Text.Json.JsonValueKind.String)
                    return dtv.GetString();

                if (json.TryGetProperty("hint", out var hintProp) && hintProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var hintStr = hintProp.GetString();
                    var m = System.Text.RegularExpressions.Regex.Match(hintStr ?? string.Empty, "data-testid\\s*=\\s*[\'\"](?<id>[^\'\"]+)[\'\"]", System.Text.RegularExpressions.RegexOptions.Compiled);
                    if (m.Success) return m.Groups["id"].Value;
                    return hintStr;
                }
            }

        return null;
    }

    private static string? TryGetLiteral(object? value)
    {
        if (value is Dictionary<string, object?> dict && dict.TryGetValue("literal", out var literal))
            return literal?.ToString();

        if (value is System.Text.Json.JsonElement json && json.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            if (json.TryGetProperty("literal", out var literalProp))
                return literalProp.GetString();
        }

        return null;
    }

    private static IEnumerable<string> IndentLines(IEnumerable<string> lines, string indent)
    {
        foreach (var line in lines)
            yield return indent + line;
    }
}
