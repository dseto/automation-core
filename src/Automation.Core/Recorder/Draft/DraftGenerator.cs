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

    public DraftGenerationResult Generate(RecorderSession session, string outputDir, string? scenarioTitle = null)
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

        // Ensure every recorded navigate event is represented by an action so draft preserves page navigations
        for (int evIdx = 0; evIdx < session.Events.Count; evIdx++)
        {
            var ev = session.Events[evIdx];
            if (ev.Type != "navigate") continue;
            // if no action includes this event index, insert a synthetic navigate action
            if (!System.Linq.Enumerable.Any(actions, a => System.Linq.Enumerable.Contains(a.EventIndexes, evIdx)))
            {
                var newAction = new DraftAction(new System.Collections.Generic.List<RecorderEvent> { ev }, new System.Collections.Generic.List<int> { evIdx });
                // find insertion position (first action whose min index > evIdx)
                var pos = -1;
                for (int ai = 0; ai < actions.Count; ai++)
                {
                    var minIdx = System.Linq.Enumerable.Min(actions[ai].EventIndexes);
                    if (minIdx > evIdx) { pos = ai; break; }
                }
                if (pos >= 0)
                {
                    var list = new System.Collections.Generic.List<DraftAction>(actions);
                    list.Insert(pos, newAction);
                    actions = list;
                }
                else
                {
                    var list = new System.Collections.Generic.List<DraftAction>(actions);
                    list.Add(newAction);
                    actions = list;
                }
            }
        }

        var steps = InferenceEngine.InferSteps(actions);
        var stepsByIndex = steps.ToDictionary(s => s.EventIndexes.First(), s => s);

        var featureName = "Fluxo de login (draft)";
        var scenarioName = !string.IsNullOrWhiteSpace(scenarioTitle) ? scenarioTitle : "Cenário draft gerado pelo Recorder";
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
        {
            // Prefer normalized presentation: if the event has url/pathname/fragment, normalize to a canonical relative route
            var baseUrlEnv = System.Environment.GetEnvironmentVariable("BASE_URL");
            var canonical = Automation.Core.Recorder.RouteNormalizer.Normalize(ev.Url ?? ev.Route ?? "/", ev.Pathname, ev.Fragment, baseUrlEnv);
            // Fallback: if normalization produced the root and ev.Route is richer, prefer ev.Route
            if (canonical == "/" && !string.IsNullOrWhiteSpace(ev.Route) && ev.Route != "/")
                canonical = ev.Route;

            var routeSanitized = SanitizeRouteForDraft(canonical ?? "/");
            return $"Dado que estou na página \"{routeSanitized}\"";
        }

        var target = TryGetHint(ev.Target);
        var literal = TryGetLiteral(ev.Value);

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

    private static bool IsGenericHint(string hint) => HintHelpers.IsGenericHint(hint);


    private static string NormalizeHint(string? hint) => HintHelpers.NormalizeHint(hint);


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

    private static string SanitizeRouteForDraft(string route)
    {
        if (string.IsNullOrWhiteSpace(route)) return "/";
        // Remove CR, convert newlines to space and collapse whitespace
        var s = route.Replace("\r", "").Replace("\n", " ").Trim();
        s = System.Text.RegularExpressions.Regex.Replace(s, "\\s+", " ");
        // Replace double quotes with single quotes to avoid breaking Gherkin
        s = s.Replace("\"", "'");
        return s;
    }

    private static IEnumerable<string> IndentLines(IEnumerable<string> lines, string indent)
    {
        foreach (var line in lines)
            yield return indent + line;
    }
}
