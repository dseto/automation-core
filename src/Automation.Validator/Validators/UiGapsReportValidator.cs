using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Automation.Validator.Models;

namespace Automation.Validator.Validators
{
    public class UiGapsReportValidator
    {
        private static readonly string[] AllowedCodes = new[] { "UIGAP_ROUTE_NOT_MAPPED", "UIGAP_ROUTE_AMBIGUOUS", "UIGAP_PAGE_CONTEXT_MISSING", "UIGAP_ELEMENT_NOT_MAPPED", "UIGAP_ELEMENT_AMBIGUOUS", "UIGAP_TESTID_MISSING", "UIGAP_TESTID_AMBIGUOUS", "UIGAP_TESTID_NOT_FOUND", "UI_MAP_KEY_NOT_FOUND", "TESTID_NOT_FOUND_IN_UIMAP", "AMBIGUOUS_MATCH", "CANDIDATES_TRUNCATED" };

        public ValidationResult Validate(string filePath)
        {
            var result = ValidationResult.Success();

            var resolvedPath = ResolvePath(filePath);
            if (resolvedPath == null)
            {
                result.AddError(new ValidationError("UIGAPS_NOT_FOUND", $"File not found: {filePath}", filePath));
                return result;
            }

            filePath = resolvedPath;
            string content = File.ReadAllText(filePath);
            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(content);
            }
            catch (Exception ex)
            {
                result.AddError(new ValidationError("UIGAPS_INVALID_JSON", ex.Message, filePath));
                return result;
            }

            var root = doc.RootElement;
            if (!root.TryGetProperty("version", out var ver) || ver.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(ver.GetString()))
                result.AddError(new ValidationError("UIGAPS_MISSING_VERSION", "Missing or invalid 'version' field", filePath));

            if (!root.TryGetProperty("findings", out var findingsEl) || findingsEl.ValueKind != JsonValueKind.Array)
                result.AddError(new ValidationError("UIGAPS_NO_FINDINGS", "Missing findings array", filePath));

            if (!root.TryGetProperty("stats", out var statsEl) || statsEl.ValueKind != JsonValueKind.Object)
                result.AddError(new ValidationError("UIGAPS_NO_STATS", "Missing stats object", filePath));

            int count = findingsEl.GetArrayLength();
            var total = statsEl.GetProperty("total").GetInt32();
            if (count != total)
                result.AddError(new ValidationError("UIGAPS_STATS_TOTAL_MISMATCH", $"stats.total ({total}) != findings.length ({count})", filePath));

            // Validate each finding and check ordering and ids
            var items = findingsEl.EnumerateArray().ToList();

            // Validate fields and codes
            for (int i = 0; i < items.Count; i++)
            {
                var f = items[i];
                if (!f.TryGetProperty("id", out var id) || id.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(id.GetString()))
                    result.AddError(new ValidationError("UIGAPS_FINDING_NO_ID", "Finding missing id", filePath));

                if (!f.TryGetProperty("severity", out var severity) || (severity.GetString() != "error" && severity.GetString() != "warn" && severity.GetString() != "info"))
                    result.AddError(new ValidationError("UIGAPS_FINDING_INVALID_SEVERITY", "Invalid severity", filePath));

                if (!f.TryGetProperty("code", out var code) || string.IsNullOrWhiteSpace(code.GetString()) || !AllowedCodes.Contains(code.GetString()))
                    result.AddError(new ValidationError("UIGAPS_FINDING_INVALID_CODE", $"Invalid code: {code.GetString()}", filePath));

                if (!f.TryGetProperty("message", out var msg) || string.IsNullOrWhiteSpace(msg.GetString()))
                    result.AddError(new ValidationError("UIGAPS_FINDING_MISSING_MESSAGE", "Finding missing message", filePath));

                if (!f.TryGetProperty("draftLine", out var dl) || dl.GetInt32() < 1)
                    result.AddError(new ValidationError("UIGAPS_FINDING_MISSING_DRAFTLINE", "Invalid draftLine", filePath));

                if (!f.TryGetProperty("stepText", out var st) || st.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(st.GetString()))
                    result.AddError(new ValidationError("UIGAPS_FINDING_MISSING_STEPTEXT", "Finding missing stepText", filePath));
            }

            // Check ordering: by draftLine asc, severity (error>warn>info), code asc
            var expectedOrder = items.OrderBy(x => x.GetProperty("draftLine").GetInt32())
                                     .ThenBy(x => SeverityRank(x.GetProperty("severity").GetString() ?? ""))
                                     .ThenBy(x => x.GetProperty("code").GetString() ?? "", StringComparer.Ordinal)
                                     .ToList();

            // Ensure severity uses 'warn' naming as normative


            bool orderingOk = true;
            for (int i = 0; i < items.Count; i++)
            {
                var actualId = items[i].GetProperty("id").GetString();
                var expectedId = expectedOrder[i].GetProperty("id").GetString();
                if (actualId != expectedId) { orderingOk = false; break; }
            }

            if (!orderingOk)
                result.AddError(new ValidationError("UIGAPS_ORDERING_INVALID", "Findings are not ordered as required", filePath));

            // Check IDs format and sequential assignment
            for (int i = 0; i < items.Count; i++)
            {
                var idStr = items[i].GetProperty("id").GetString();
                var expected = $"UIGAP-{(i + 1).ToString("D4")}";
                if (idStr != expected)
                    result.AddError(new ValidationError("UIGAPS_ID_NOT_SEQUENTIAL", $"Finding id '{idStr}' is not '{expected}'", filePath));
            }

            return result;
        }

        private static string? ResolvePath(string filePath)
        {
            if (Path.IsPathRooted(filePath) && File.Exists(filePath)) return filePath;
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, filePath);
                if (File.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }
            return null;
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
    }
}
