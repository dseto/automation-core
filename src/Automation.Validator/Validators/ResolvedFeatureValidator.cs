using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using Automation.Validator.Models;

namespace Automation.Validator.Validators
{
    public class ResolvedFeatureValidator
    {
        private static readonly Regex UiGapCommentRegex = new(@"^#\s*UIGAP:\s*(UIGAP-\d{4})\s+([A-Z0-9_\-]+)", RegexOptions.Compiled);

        public ValidationResult Validate(string resolvedFeaturePath, string resolvedMetadataPath)
        {
            var result = ValidationResult.Success();

            if (!File.Exists(resolvedFeaturePath))
            {
                result.AddError(new ValidationError("RESOLVED_FEATURE_NOT_FOUND", $"File not found: {resolvedFeaturePath}", resolvedFeaturePath));
                return result;
            }

            if (!File.Exists(resolvedMetadataPath))
            {
                result.AddError(new ValidationError("RESOLVED_METADATA_NOT_FOUND", $"File not found: {resolvedMetadataPath}", resolvedMetadataPath));
                return result;
            }

            var resolvedLines = File.ReadAllLines(resolvedFeaturePath).ToList();

            JsonDocument metadataDoc;
            try
            {
                metadataDoc = JsonDocument.Parse(File.ReadAllText(resolvedMetadataPath));
            }
            catch (Exception ex)
            {
                result.AddError(new ValidationError("RESOLVED_METADATA_INVALID_JSON", ex.Message, resolvedMetadataPath));
                return result;
            }

            // Gate 1 header checks: starts with #language: pt and contains Funcionalidade and Cenário
            var firstNonEmpty = resolvedLines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l)) ?? string.Empty;
            if (!firstNonEmpty.StartsWith("#language: pt", StringComparison.OrdinalIgnoreCase))
                result.AddError(new ValidationError("RESOLVED_FEATURE_NO_LANGUAGE_PT", "Resolved feature must start with '#language: pt'", resolvedFeaturePath));

            var content = string.Join(Environment.NewLine, resolvedLines);
            if (!content.Contains("Funcionalidade:") || !content.Contains("Cenário:"))
                result.AddError(new ValidationError("RESOLVED_FEATURE_HEADER_LOCALE", "Resolved feature must use 'Funcionalidade:' and 'Cenário:' (pt) headers", resolvedFeaturePath));

            // Attempt to locate draft feature path from metadata (source.draftFeaturePath)
            string? draftPath = null;
            if (metadataDoc.RootElement.TryGetProperty("source", out var sourceEl) && sourceEl.ValueKind == JsonValueKind.Object && sourceEl.TryGetProperty("draftFeaturePath", out var dp) && dp.ValueKind == JsonValueKind.String)
            {
                draftPath = dp.GetString();
                // If relative, resolve relative to metadata file
                if (!Path.IsPathRooted(draftPath))
                {
                    var metaDir = Path.GetDirectoryName(resolvedMetadataPath) ?? Directory.GetCurrentDirectory();
                    draftPath = Path.GetFullPath(Path.Combine(metaDir, draftPath));
                }
            }

            // mapping from draftLine -> resolved.feature index (populated only when draft is available)
            var draftLineToResolvedIndex = new System.Collections.Generic.Dictionary<int, int>();

            if (string.IsNullOrWhiteSpace(draftPath) || !File.Exists(draftPath))
            {
                // Cannot fully validate step preservation without draft; add warning but continue other checks
                result.AddWarning(new ValidationWarning("RESOLVED_FEATURE_NO_DRAFT", "Draft feature not found; step-preservation checks skipped", resolvedFeaturePath));
            }
            else
            {
                var draftLines = File.ReadAllLines(draftPath).ToList();

                if (!metadataDoc.RootElement.TryGetProperty("steps", out var stepsEl) || stepsEl.ValueKind != JsonValueKind.Array)
                {
                    result.AddError(new ValidationError("RESOLVED_METADATA_NO_STEPS", "resolved.metadata.json missing 'steps' array", resolvedMetadataPath));
                }
                else
                {
                    // For each step in order, ensure the stepText appears in the resolved feature in the same order
                    int lastFoundIndex = -1;
                    foreach (var step in stepsEl.EnumerateArray())
                    {
                        var draftLine = step.GetProperty("draftLine").GetInt32();
                        var status = step.GetProperty("status").GetString() ?? "";
                        var stepText = step.GetProperty("stepText").GetString() ?? string.Empty;

                        string draftStepText = stepText.TrimEnd();

                        if (draftLine < 1 || draftLine > draftLines.Count)
                        {
                            result.AddError(new ValidationError("RESOLVED_METADATA_INVALID_DRAFTLINE", $"Invalid draftLine: {draftLine}", resolvedMetadataPath));
                            continue;
                        }

                        var expectedDraftText = draftLines[draftLine - 1].TrimEnd();
                        if (!string.Equals(expectedDraftText, draftStepText, StringComparison.Ordinal))
                        {
                            result.AddError(new ValidationError("RESOLVED_METADATA_STEPTEXT_MISMATCH", $"Step text in metadata for draftLine {draftLine} does not match draft file", resolvedMetadataPath));
                            continue;
                        }

                        // Find the draft step text in resolved lines after lastFoundIndex.
                        // Accept either the original draft step text OR a substituted navigation step that uses the resolved page key (e.g., Dado que estou na página "login").
                        bool found = false;
                        // build alternative draft text when chosen.pageKey exists (for navigation substitutions)
                        string? altDraftText = null;
                        if (step.TryGetProperty("chosen", out var chosenEl) && chosenEl.ValueKind == JsonValueKind.Object && chosenEl.TryGetProperty("pageKey", out var pageKeyEl) && pageKeyEl.ValueKind == JsonValueKind.String)
                        {
                            var pageKey = pageKeyEl.GetString();
                            if (!string.IsNullOrWhiteSpace(pageKey) && draftStepText.IndexOf("estou na página", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                // replace only the FIRST quoted string with the page key (preserve other quoted literals) — RF-SR-40
                                var regex = new System.Text.RegularExpressions.Regex("\"([^\"]+)\"");
                                altDraftText = regex.Replace(draftStepText, $"\"{pageKey}\"", 1);
                            }
                        }

                        for (int i = lastFoundIndex + 1; i < resolvedLines.Count; i++)
                        {
                            var candidate = resolvedLines[i].TrimEnd();
                            if (candidate.Equals(draftStepText, StringComparison.Ordinal) || (altDraftText != null && candidate.Equals(altDraftText, StringComparison.Ordinal)))
                            {
                                found = true;
                                lastFoundIndex = i;
                                // record mapping draftLine -> resolved index so comment check can locate the exact occurrence
                                draftLineToResolvedIndex[draftLine] = i;
                                break;
                            }
                        }

                        if (!found)
                        {
                            result.AddError(new ValidationError("RESOLVED_STEP_MISSING_OR_REORDERED", $"Step from draft line {draftLine} not found in resolved.feature or order changed: '{draftStepText}'", resolvedFeaturePath));
                            continue;
                        }

                        // If status is partial or unresolved, ensure the step text was not substituted (we already compared exact strings, so a substitution fails above)
                        if (status == "partial" || status == "unresolved")
                        {
                            // additional checks could be added (e.g., ensure braces or elementRefs not replaced)
                            // already ensured preservation by exact match; no-op
                        }
                    }
                }
            }

            // Check that for each error/warn finding in ui-gaps.report.json a comment exists immediately above the related step
            // Attempt to find ui-gaps report in same directory as metadata
            var reportPathCandidate = Path.Combine(Path.GetDirectoryName(resolvedMetadataPath) ?? string.Empty, "ui-gaps.report.json");
            if (!File.Exists(reportPathCandidate))
            {
                result.AddWarning(new ValidationWarning("RESOLVED_FEATURE_NO_UIGAPS_REPORT", "ui-gaps.report.json not found alongside metadata; comments presence not enforced", resolvedFeaturePath));
                return result;
            }

            JsonDocument reportDoc;
            try
            {
                reportDoc = JsonDocument.Parse(File.ReadAllText(reportPathCandidate));
            }
            catch (Exception ex)
            {
                result.AddError(new ValidationError("UIGAPS_INVALID_JSON", ex.Message, reportPathCandidate));
                return result;
            }

            var findings = reportDoc.RootElement.GetProperty("findings").EnumerateArray().Where(f => f.GetProperty("severity").GetString() == "error" || f.GetProperty("severity").GetString() == "warn").ToList();

            foreach (var f in findings)
            {
                var id = f.GetProperty("id").GetString() ?? "";
                var code = f.GetProperty("code").GetString() ?? "";
                var draftLine = f.GetProperty("draftLine").GetInt32();

                // locate step in resolved feature by matching draft step text
                if (draftPath == null || !File.Exists(draftPath)) continue; // already warned above

                var draftLines = File.ReadAllLines(draftPath).ToList();
                if (draftLine < 1 || draftLine > draftLines.Count) continue;
                var draftStepText = draftLines[draftLine - 1].TrimEnd();

                // find index in resolvedLines (prefer mapping from earlier validation to disambiguate repeated step texts)
                int idx;
                if (!draftLineToResolvedIndex.TryGetValue(draftLine, out idx))
                {
                    idx = resolvedLines.FindIndex(l => l.TrimEnd().Equals(draftStepText, StringComparison.Ordinal));
                }

                if (idx < 0)
                {
                    result.AddError(new ValidationError("UIGAPS_STEP_NOT_PRESENT_IN_RESOLVED", $"Step for finding {id} not present in resolved.feature: '{draftStepText}'", resolvedFeaturePath));
                    continue;
                }

                // check previous non-empty line for comment
                int prev = idx - 1;
                while (prev >= 0 && string.IsNullOrWhiteSpace(resolvedLines[prev])) prev--;
                if (prev < 0)
                {
                    result.AddError(new ValidationError("UIGAPS_COMMENT_MISSING", $"UIGAP comment for {id} missing above step at line {idx + 1}", resolvedFeaturePath));
                    continue;
                }

                var match = UiGapCommentRegex.Match(resolvedLines[prev].Trim());
                if (!match.Success)
                {
                    result.AddError(new ValidationError("UIGAPS_COMMENT_INVALID", $"UIGAP comment for {id} is missing or malformed: '{resolvedLines[prev].Trim()}'", resolvedFeaturePath));
                    continue;
                }

                var foundId = match.Groups[1].Value;
                var foundCode = match.Groups[2].Value;
                if (!string.Equals(foundId, id, StringComparison.Ordinal) || !string.Equals(foundCode, code, StringComparison.Ordinal))
                {
                    result.AddError(new ValidationError("UIGAPS_COMMENT_MISMATCH", $"UIGAP comment for {id} does not match expected id/code: found '{foundId}/{foundCode}'", resolvedFeaturePath));
                    continue;
                }
            }

            return result;
        }
    }
}
