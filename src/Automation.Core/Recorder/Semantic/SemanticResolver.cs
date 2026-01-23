using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Automation.Core.Recorder.Draft;
using Automation.Core.Recorder;
using Automation.Core.Recorder.Semantic.Models;
using Automation.Core.UiMap;

namespace Automation.Core.Recorder.Semantic
{
    /// <summary>
    /// Implements semantic resolution rules (RF30–RF38 and Confidence Calculation).
    /// Deterministic and follows SSOT rules: exact UIMap key -> confidence 1.0, unique testId -> 1.0, multiple candidates -> 1/n, zero -> 0.0.
    /// Produces ResolvedMetadata, UiGapsReport and resolved feature content (inserts UIGAP comments).
    /// </summary>
    public class SemanticResolver
    {
        private readonly UiMapModel _uiMap;
        private readonly RecorderSession? _session;
        private readonly int _maxCandidates;
        private readonly string _draftPath;
        private readonly string _uiMapPath;

        public SemanticResolver(UiMapModel uiMap, RecorderSession? session = null, int maxCandidates = 5, string draftPath = "draft.feature", string uiMapPath = "ui-map.yaml")
        {
            _uiMap = uiMap ?? throw new ArgumentNullException(nameof(uiMap));
            _session = session;
            _maxCandidates = Math.Max(1, maxCandidates);
            _draftPath = draftPath;
            _uiMapPath = uiMapPath;
        }

        public (ResolvedMetadata metadata, UiGapsReport report, string resolvedFeature) Resolve(string draftContent, DraftMetadata draftMetadata)
        {
            if (draftContent == null) throw new ArgumentNullException(nameof(draftContent));
            if (draftMetadata == null) throw new ArgumentNullException(nameof(draftMetadata));

            var lines = ReadLines(draftContent).ToList();

            var metadata = new ResolvedMetadata
            {
                Version = "0.1.0",
                GeneratedAt = DateTimeOffset.UtcNow.ToString("O"),
                Source = new ResolvedSource
                {
                    DraftFeaturePath = _draftPath,
                    UiMapPath = _uiMapPath,
                    SessionPath = null
                }
            };

            var report = new UiGapsReport
            {
                Version = "0.1.0",
                SessionId = draftMetadata.SessionId,
                GeneratedAt = metadata.GeneratedAt,
                DraftPath = _draftPath,
                UiMapPath = _uiMapPath
            };

            // Build testId index
            var testIdIndex = BuildTestIdIndex(_uiMap);

            var findings = new List<UiGapFinding>();

            foreach (var mapping in draftMetadata.Mappings.OrderBy(m => m.DraftLine))
            {
                var stepLine = mapping.DraftLine;
                var stepText = GetLine(lines, stepLine);
                var inputRef = ExtractQuoted(stepText) ?? string.Empty;

                var step = new ResolvedStep
                {
                    DraftLine = stepLine,
                    StepText = stepText
                };

                // 1) Try UIMap key (page.element) — be strict: avoid treating hints like "[data-testid='...']" as page.element
                if (!string.IsNullOrWhiteSpace(inputRef) && inputRef.Contains('.') && !inputRef.Contains('[') && !inputRef.Contains(']'))
                {
                    // only accept simple page.element formats (e.g., "login.username")
                    var parts = inputRef.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        var page = parts[0];
                        var element = parts[1];
                        // If page exists, try page.element first, but do NOT emit error immediately if element not found.
                        if (_uiMap.Pages.ContainsKey(page))
                        {
                            try
                            {
                                var uiPage = _uiMap.GetPageOrThrow(page);
                                var testId = uiPage.GetTestIdOrThrow(element);

                                step.Status = "resolved";
                                step.Chosen = new ResolvedChosen { PageKey = page, ElementKey = element, TestId = testId };

                                metadata.Steps.Add(step);
                                continue;
                            }
                            catch
                            {
                                // Element not found in page — do NOT add a finding here. Fall through and try to resolve the inputRef as a testId (see below) or other strategies.
                            }
                        }

                        // If we reach here, page.element did not resolve — try treating the entire inputRef as a testId
                        if (testIdIndex.TryGetValue(inputRef, out var candidatesList) && candidatesList.Count > 0)
                        {
                            var totalCandidates = candidatesList.Count;

                            if (totalCandidates == 1)
                            {
                                var chosen = candidatesList[0];
                                var resParts = chosen.ResolvedRef.Split('.', 2);
                                step.Status = "resolved";
                                step.Chosen = new ResolvedChosen { PageKey = resParts[0], ElementKey = resParts[1], TestId = chosen.TestId };
                                metadata.Steps.Add(step);
                                continue;
                            }

                            // multiple candidates -> partial
                            step.Status = "partial";

                            var returned = candidatesList.Take(_maxCandidates).ToList();
                            foreach (var c in returned)
                            {
                                var parts2 = c.ResolvedRef.Split('.', 2);
                                step.Candidates.Add(new ResolvedCandidate { PageKey = parts2[0], ElementKey = parts2[1], TestId = c.TestId });
                            }

                            if (totalCandidates > _maxCandidates)
                            {
                                var infoFinding = new UiGapFinding
                                {
                                    Severity = "info",
                                    Code = "CANDIDATES_TRUNCATED",
                                    Message = $"Total candidatos: {totalCandidates}. Retornados: {_maxCandidates}.",
                                    DraftLine = stepLine,
                                    InputRef = inputRef,
                                    StepText = stepText
                                };
                                findings.Add(infoFinding);
                            }

                            var warnFinding = new UiGapFinding
                            {
                                Severity = "warn",
                                Code = "AMBIGUOUS_MATCH",
                                Message = $"Ambiguidade: {totalCandidates} candidatos encontrados para testId '{inputRef}'.",
                                DraftLine = stepLine,
                                InputRef = inputRef,
                                StepText = stepText
                            };
                            findings.Add(warnFinding);

                            metadata.Steps.Add(step);
                            continue;
                        }

                        // else: fall through to route mapping / session-based resolution
                    }
                }

                // 2) Special-case: navigate steps (route mapping)
                // If the draft step appears to be a navigation (e.g., "Dado que estou na página \"...\"")
                // then try to resolve the route to a UiMap page (by page key or page.__meta.route).
                // If not found, emit an info finding UIGAP_ROUTE_NOT_MAPPED instead of an error.
                if (!string.IsNullOrWhiteSpace(stepText) && stepText.IndexOf("estou na página", StringComparison.OrdinalIgnoreCase) >= 0 && !string.IsNullOrWhiteSpace(inputRef))
                {
                    // Prefer the route recorded in the session event (deterministic) when available; otherwise use the draft literal
                    var route = inputRef.Trim();
                    if (_session != null)
                    {
                        if (mapping.EventIndex >= 0 && mapping.EventIndex < _session.Events.Count)
                        {
                            var ev = _session.Events[mapping.EventIndex];
                            // Prefer normalized route presentation from the session (url/pathname/fragment) when possible
                            var baseUrlEnv = System.Environment.GetEnvironmentVariable("BASE_URL");
                            var normalized = Automation.Core.Recorder.RouteNormalizer.Normalize(ev.Url ?? ev.Route ?? route, ev.Pathname, ev.Fragment, baseUrlEnv);
                            if (!string.IsNullOrWhiteSpace(normalized) && normalized != "/")
                                route = normalized.Trim();
                            else if (!string.IsNullOrWhiteSpace(ev.Route))
                                route = ev.Route.Trim();
                        }
                    }

                    var routeNoSlash = route.StartsWith("/") ? route.Substring(1) : route;

                    // Try page key match
                    var pageKeyMatch = _uiMap.Pages.Keys.FirstOrDefault(k => string.Equals(k, route, StringComparison.OrdinalIgnoreCase) || string.Equals(k, routeNoSlash, StringComparison.OrdinalIgnoreCase));
                    if (pageKeyMatch != null)
                    {
                        // Route maps to a page in the UI Map — treat as resolved for navigation context.
                        step.Status = "resolved";
                        step.Chosen = new ResolvedChosen { PageKey = pageKeyMatch, ElementKey = "__page__", TestId = null };
                        metadata.Steps.Add(step);
                        continue;
                    }

                    // Try matching page __meta.route
                    var routeMatched = false;
                    foreach (var kv in _uiMap.Pages)
                    {
                        if (kv.Value is System.Collections.IDictionary pageDict)
                        {
                            var uiPage = new Automation.Core.UiMap.UiPage(pageDict);
                            var pageRoute = uiPage.Route;
                            if (!string.IsNullOrWhiteSpace(pageRoute))
                            {
                                var pageRouteNoSlash = pageRoute.StartsWith("/") ? pageRoute.Substring(1) : pageRoute;
                                if (string.Equals(pageRoute, route, StringComparison.OrdinalIgnoreCase) || string.Equals(pageRouteNoSlash, routeNoSlash, StringComparison.OrdinalIgnoreCase))
                                {
                                    // Route maps to a page via __meta.route — treat as resolved for navigation context.
                                    step.Status = "resolved";
                                    step.Chosen = new ResolvedChosen { PageKey = kv.Key, ElementKey = "__page__", TestId = null };
                                    metadata.Steps.Add(step);
                                    routeMatched = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (routeMatched)
                        continue;

                    // Not mapped — produce an info finding (UIGAP_ROUTE_NOT_MAPPED)
                    var routeInfo = new UiGapFinding
                    {
                        Severity = "info",
                        Code = "UIGAP_ROUTE_NOT_MAPPED",
                        Message = $"Rota '{route}' não mapeada no UiMap.",
                        DraftLine = stepLine,
                        InputRef = inputRef,
                        StepText = stepText,
                        Route = route
                    };
                    findings.Add(routeInfo);
                    metadata.Steps.Add(step);
                    continue;
                }

                // 2) Fallback: if session exists and event carries a testId, use testId lookup
                var evTestId = TryGetTestIdFromSession(mapping.EventIndex);
                if (!string.IsNullOrWhiteSpace(evTestId))
                {
                    if (!testIdIndex.TryGetValue(evTestId, out var candidatesList) || candidatesList.Count == 0)
                    {
                        step.Status = "unresolved";
                        var f = new UiGapFinding
                        {
                            Severity = "error",
                            Code = "TESTID_NOT_FOUND_IN_UIMAP",
                            Message = $"testId '{evTestId}' não encontrado no UiMap.",
                            DraftLine = stepLine,
                            InputRef = inputRef,
                            StepText = stepText
                        };
                        findings.Add(f);
                        metadata.Steps.Add(step);
                        continue;
                    }

                    var totalCandidates = candidatesList.Count;

                    if (totalCandidates == 1)
                    {
                        var chosen = candidatesList[0];
                        var resParts = chosen.ResolvedRef.Split('.', 2);
                        step.Status = "resolved";
                        step.Chosen = new ResolvedChosen { PageKey = resParts[0], ElementKey = resParts[1], TestId = chosen.TestId };
                        metadata.Steps.Add(step);
                        continue;
                    }

                    // multiple candidates -> partial
                    step.Status = "partial";

                    // add candidates (truncate to max)
                    var returned = candidatesList.Take(_maxCandidates).ToList();
                    foreach (var c in returned)
                    {
                        var parts = c.ResolvedRef.Split('.', 2);
                        step.Candidates.Add(new ResolvedCandidate { PageKey = parts[0], ElementKey = parts[1], TestId = c.TestId });
                    }

                    if (totalCandidates > _maxCandidates)
                    {
                        var infoFinding = new UiGapFinding
                        {
                            Severity = "info",
                            Code = "CANDIDATES_TRUNCATED",
                            Message = $"Total candidatos: {totalCandidates}. Retornados: {_maxCandidates}.",
                            DraftLine = stepLine,
                            InputRef = inputRef,
                            StepText = stepText
                        };
                        findings.Add(infoFinding);
                    }

                    var warnFinding = new UiGapFinding
                    {
                        Severity = "warn",
                        Code = "AMBIGUOUS_MATCH",
                        Message = $"Ambiguidade: {totalCandidates} candidatos encontrados para testId '{evTestId}'.",
                        DraftLine = stepLine,
                        InputRef = inputRef,
                        StepText = stepText
                    };
                    findings.Add(warnFinding);

                    metadata.Steps.Add(step);
                    continue;
                }

                // 3) No candidate available -> unresolved
                step.Status = "unresolved";
                var f2 = new UiGapFinding
                {
                    Severity = "error",
                    Code = "UI_MAP_KEY_NOT_FOUND",
                    Message = $"Referência não resolvida (sem context/testId): '{inputRef}'",
                    DraftLine = stepLine,
                    InputRef = inputRef,
                    StepText = stepText
                };
                findings.Add(f2);
                metadata.Steps.Add(step);
            }

            // Assign deterministic IDs and sort as specified in SSOT
            var ordered = findings
                .OrderBy(f => f.DraftLine)
                .ThenBy(f => SeverityRank(f.Severity)) // error > warn > info
                .ThenBy(f => f.Code, StringComparer.Ordinal)
                .ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                ordered[i].Id = $"UIGAP-{(i + 1).ToString("D4")}";
            }

            // Build report and stats
            report.Findings = ordered;
            report.Stats = new UiGapStats
            {
                Errors = ordered.Count(f => f.Severity == "error"),
                Warnings = ordered.Count(f => f.Severity == "warn"),
                Infos = ordered.Count(f => f.Severity == "info"),
                Total = ordered.Count
            };

            // Map findings back to metadata (attach findings objects to step entries)
            foreach (var f in ordered)
            {
                var step = metadata.Steps.FirstOrDefault(s => s.DraftLine == f.DraftLine);
                if (step != null)
                {
                    step.Findings.Add(new ResolvedFinding { Severity = f.Severity, Code = f.Code, Message = f.Message });
                }
            }

            // Rewrite navigation step lines to use pageKey when the step was resolved to a page
            foreach (var s in metadata.Steps)
            {
                if (s != null && s.Status == "resolved" && s.Chosen != null && !string.IsNullOrWhiteSpace(s.Chosen.PageKey) && !string.IsNullOrWhiteSpace(s.StepText) && s.StepText.IndexOf("estou na página", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var idx = s.DraftLine - 1;
                    if (idx >= 0 && idx < lines.Count)
                    {
                        // replace only the FIRST quoted string with the page key (keeps other parts intact) — RF-SR-40
                        var regex = new System.Text.RegularExpressions.Regex("\"([^\"]+)\"");
                        lines[idx] = regex.Replace(lines[idx], $"\"{s.Chosen.PageKey}\"", 1);
                    }
                }
            }

            // Rewrite element references for steps resolved to page.element so runtime can resolve them (e.g., login.pass.label -> login.pass-label)
            foreach (var s in metadata.Steps)
            {
                if (s != null && s.Status == "resolved" && s.Chosen != null && !string.IsNullOrWhiteSpace(s.StepText) && s.Chosen.ElementKey != "__page__")
                {
                    var quoted = ExtractQuoted(s.StepText);
                    if (!string.IsNullOrWhiteSpace(quoted) && quoted.Contains('.') )
                    {
                        var parts = quoted.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);
                        var newRef = $"{s.Chosen.PageKey}.{s.Chosen.ElementKey}";
                        var idx = s.DraftLine - 1;

                        // Case A: quoted is in form "page.element.*" (e.g., "login.pass.label") — rewrite when page matches
                        if (parts.Length == 2 && string.Equals(parts[0], s.Chosen.PageKey, StringComparison.OrdinalIgnoreCase))
                        {
                            if (idx >= 0 && idx < lines.Count)
                            {
                                // replace only the FIRST quoted string (the reference) so literal values in subsequent quoted strings are preserved — RF-SR-40
                                var regex = new System.Text.RegularExpressions.Regex("\"([^\"]+)\"");
                                lines[idx] = regex.Replace(lines[idx], $"\"{newRef}\"", 1);
                            }
                        }
                        // Case B: quoted contains the resolved testId (e.g., "[data-testid='page.login.username']"), rewrite to element ref
                        else if (!string.IsNullOrWhiteSpace(s.Chosen.TestId) && quoted.Contains(s.Chosen.TestId, StringComparison.Ordinal))
                        {
                            if (idx >= 0 && idx < lines.Count)
                            {
                                var regex = new System.Text.RegularExpressions.Regex("\"([^\"]+)\"");
                                lines[idx] = regex.Replace(lines[idx], $"\"{newRef}\"", 1);
                            }
                        }
                    }
                }
            }

            // If there are page-level navigation steps resolved, ensure the resolved feature contains a leading BASE_URL step
            var hasPageNav = metadata.Steps.Any(s => s.Status == "resolved" && s.Chosen != null && s.Chosen.ElementKey == "__page__");
            System.Collections.Generic.List<int>? insertedPositions = null;
            if (hasPageNav)
            {
                // Do not duplicate if the resolved already contains an app base step
                var existsBaseStep = lines.Any(l => l.IndexOf("Dado que a aplicação está em", StringComparison.OrdinalIgnoreCase) >= 0);
                if (!existsBaseStep)
                {
                    // Find scenario header and insert after it (after the blank line following the header)
                    var scenarioIdx = -1;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].TrimStart().StartsWith("Cenário:", StringComparison.OrdinalIgnoreCase))
                        {
                            scenarioIdx = i;
                            break;
                        }
                    }

                    var insertAt = (scenarioIdx >= 0) ? Math.Min(scenarioIdx + 2, lines.Count) : 0;
                    var baseStep = "  Dado que a aplicação está em \"${BASE_URL}\"";
                    lines.Insert(insertAt, baseStep);

                    // Record insertion position so we can map draftLine -> resolvedLine when inserting comments
                    var insertLineNumber = insertAt + 1; // 1-based
                    insertedPositions = new System.Collections.Generic.List<int> { insertLineNumber };
                    // Note: We do NOT modify the DraftLine values in the findings (they must remain draft-file-based). Instead
                    // InsertFindingsComments will map draftLine -> resolvedLine accounting for these insertions.
                }
            }

            // Add comments into resolved feature (comments for error/warn)
            var commented = InsertFindingsComments(lines, ordered, insertedPositions);

            return (metadata, report, string.Join(Environment.NewLine, commented) + Environment.NewLine);
        }

        private static int SeverityRank(string severity)
        {
            return severity switch
            {
                "error" => 0,
                "warn" => 1,
                "info" => 2,
                _ => 3
            };
        }

        private static IEnumerable<string> ReadLines(string content)
        {
            using var reader = new System.IO.StringReader(content ?? string.Empty);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        private static string GetLine(IList<string> lines, int lineNumber)
        {
            if (lineNumber <= 0 || lineNumber > lines.Count) return string.Empty;
            return lines[lineNumber - 1];
        }

        private static string? ExtractQuoted(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            var first = text.IndexOf('"');
            if (first < 0) return null;
            var second = text.IndexOf('"', first + 1);
            if (second <= first) return null;
            return text.Substring(first + 1, second - first - 1);
        }

        private string? TryGetTestIdFromSession(int eventIndex)
        {
            if (_session == null) return null;
            if (eventIndex < 0 || eventIndex >= _session.Events.Count) return null;
            var ev = _session.Events[eventIndex];
            // Attempt to extract data-testid from target
            if (ev.Target == null) return null;

            // Target might be Dictionary<string,object> or JsonElement
            if (ev.Target is IDictionary<string, object> dict)
            {
                // 1) attributes.data-testid
                if (dict.TryGetValue("attributes", out var attributesObj))
                {
                    if (attributesObj is IDictionary<string, object> a && a.TryGetValue("data-testid", out var dt) && dt is string s)
                        return s;
                    if (attributesObj is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        if (je.TryGetProperty("data-testid", out var prop) && prop.ValueKind == System.Text.Json.JsonValueKind.String)
                            return prop.GetString();
                    }
                }

                // 2) Some events may carry testId directly
                if (dict.TryGetValue("testId", out var tid) && tid is string ts)
                    return ts;

                // 3) hint (string) may contain data-testid="..." or data-testid='...'
                if (dict.TryGetValue("hint", out var hintObj) && hintObj is string hintStr)
                {
                    var extracted = ExtractTestIdFromHint(hintStr);
                    if (!string.IsNullOrWhiteSpace(extracted)) return extracted;
                }
            }

            if (ev.Target is System.Text.Json.JsonElement json && json.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                if (json.TryGetProperty("attributes", out var attrs) && attrs.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (attrs.TryGetProperty("data-testid", out var dtv) && dtv.ValueKind == System.Text.Json.JsonValueKind.String)
                        return dtv.GetString();
                }

                if (json.TryGetProperty("testId", out var tidv) && tidv.ValueKind == System.Text.Json.JsonValueKind.String)
                    return tidv.GetString();

                if (json.TryGetProperty("hint", out var hintv) && hintv.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var hintStr = hintv.GetString() ?? string.Empty;
                    var extracted = ExtractTestIdFromHint(hintStr);
                    if (!string.IsNullOrWhiteSpace(extracted)) return extracted;
                }
            }

            return null;
        }

        private static string? ExtractTestIdFromHint(string hint)
        {
            if (string.IsNullOrWhiteSpace(hint)) return null;
            var m = System.Text.RegularExpressions.Regex.Match(hint, "data-testid\\s*=\\s*[\'\"](?<id>[^\'\"]+)[\'\"]", System.Text.RegularExpressions.RegexOptions.Compiled);
            if (m.Success) return m.Groups["id"].Value;
            return null;
        }

        private static Dictionary<string, List<(string ResolvedRef, string TestId)>> BuildTestIdIndex(UiMapModel uiMap)
        {
            var dict = new Dictionary<string, List<(string ResolvedRef, string TestId)>>(StringComparer.Ordinal);
            foreach (var kv in uiMap.Pages)
            {
                var pageName = kv.Key;
                if (kv.Value is IDictionary pageDict)
                {
                    foreach (DictionaryEntry entry in pageDict)
                    {
                        var key = (entry.Key ?? string.Empty).ToString();
                        if (key == "__meta") continue;
                        var elementObj = entry.Value;
                        if (elementObj is IDictionary elementDict)
                        {
                            string? testId = null;
                            if (elementDict.Contains("testId")) testId = elementDict["testId"]?.ToString();
                            if (string.IsNullOrWhiteSpace(testId) && elementDict.Contains("test_id")) testId = elementDict["test_id"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(testId))
                            {
                                var resolvedRef = $"{pageName}.{key}";
                                if (!dict.TryGetValue(testId, out var list)) { list = new List<(string, string)>(); dict[testId] = list; }
                                list.Add((resolvedRef, testId));
                            }
                        }
                        else if (entry.Value is string s)
                        {
                            var resolvedRef = $"{pageName}.{key}";
                            var testId = s;
                            if (!dict.TryGetValue(testId, out var list)) { list = new List<(string, string)>(); dict[testId] = list; }
                            list.Add((resolvedRef, testId));
                        }
                    }
                }
            }

            return dict;
        }

        private static List<string> InsertFindingsComments(IList<string> lines, List<UiGapFinding> orderedFindings, System.Collections.Generic.List<int>? insertedPositions)
        {
            // We need to insert comments above the step lines for error and warning findings.
            // Findings use DraftLine (referencing draft.feature). Since we may have inserted additional lines into the
            // resolved feature (e.g., a BASE_URL step), we must map draftLine -> resolvedLine by applying the shift
            // introduced by insertions occurring at or before the draft line.

            // Build mapping draftLine -> resolvedLine
            var byResolvedLine = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<UiGapFinding>>();
            foreach (var f in orderedFindings.Where(f => f.Severity == "error" || f.Severity == "warn"))
            {
                var shift = 0;
                if (insertedPositions != null && insertedPositions.Count > 0)
                {
                    shift = insertedPositions.Count(p => p <= f.DraftLine);
                }
                var resolvedLine = f.DraftLine + shift;
                if (!byResolvedLine.TryGetValue(resolvedLine, out var list))
                {
                    list = new System.Collections.Generic.List<UiGapFinding>();
                    byResolvedLine[resolvedLine] = list;
                }
                list.Add(f);
            }

            var outLines = new List<string>();
            for (int i = 0; i < lines.Count; i++)
            {
                int lineNumber = i + 1;
                if (byResolvedLine.TryGetValue(lineNumber, out var findings))
                {
                    // insert comment(s)
                    foreach (var f in findings)
                    {
                        outLines.Add($"# UIGAP: {f.Id} {f.Code} — {f.Message}");
                    }
                }

                outLines.Add(lines[i]);
            }

            return outLines;
        }
    }
}
