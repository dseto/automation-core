using System;
using System.IO;
using System.Text.Json;
using Automation.Validator.Models;

namespace Automation.Validator.Validators
{
    public class ResolvedMetadataValidator
    {
        public ValidationResult Validate(string filePath)
        {
            var result = ValidationResult.Success();

            var resolvedPath = ResolvePath(filePath);
            if (resolvedPath == null)
            {
                result.AddError(new ValidationError("RESOLVED_NOT_FOUND", $"File not found: {filePath}", filePath));
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
                result.AddError(new ValidationError("RESOLVED_INVALID_JSON", ex.Message, filePath));
                return result;
            }

            var root = doc.RootElement;

            // Required fields (per schema)
            string[] required = new[] { "version", "generatedAt", "source", "steps" };
            foreach (var r in required)
            {
                if (!root.TryGetProperty(r, out _))
                    result.AddError(new ValidationError("RESOLVED_MISSING_FIELD", $"Missing field: {r}", filePath));
            }

            if (!root.TryGetProperty("steps", out var stepsEl) || stepsEl.ValueKind != JsonValueKind.Array)
                return result;

            if (!root.TryGetProperty("source", out var sourceEl) || sourceEl.ValueKind != JsonValueKind.Object || !sourceEl.TryGetProperty("draftFeaturePath", out var dfp) || string.IsNullOrWhiteSpace(dfp.GetString()))
                result.AddError(new ValidationError("RESOLVED_MISSING_FIELD", "Missing source.draftFeaturePath", filePath));

            int resolvedCount = 0, partialCount = 0, unresolvedCount = 0;

            foreach (var step in stepsEl.EnumerateArray())
            {
                if (!step.TryGetProperty("draftLine", out var dl) || dl.GetInt32() < 1)
                    result.AddError(new ValidationError("RESOLVED_STEP_MISSING_DRAFTLINE", "Step missing valid draftLine", filePath));

                var status = step.GetProperty("status").GetString();
                if (status is null || !(status == "resolved" || status == "partial" || status == "unresolved"))
                    result.AddError(new ValidationError("RESOLVED_INVALID_STATUS", $"Invalid status: {status}", filePath));

                if (status == "resolved")
                {
                    // resolved must have chosen
                    if (!step.TryGetProperty("chosen", out var chosen) || chosen.ValueKind == JsonValueKind.Null)
                        result.AddError(new ValidationError("RESOLVED_NO_CHOSEN", "Resolved step without chosen", filePath));
                    else
                    {
                        if (!chosen.TryGetProperty("pageKey", out var pk) || string.IsNullOrWhiteSpace(pk.GetString()) || !chosen.TryGetProperty("elementKey", out var ek) || string.IsNullOrWhiteSpace(ek.GetString()))
                            result.AddError(new ValidationError("RESOLVED_INVALID_CHOSEN", "Chosen missing pageKey/elementKey", filePath));
                    }
                    resolvedCount++;
                }
                else if (status == "partial")
                {
                    // partial must have candidates
                    if (!step.TryGetProperty("candidates", out var cand) || cand.ValueKind != JsonValueKind.Array || cand.GetArrayLength() == 0)
                        result.AddError(new ValidationError("RESOLVED_PARTIAL_NO_CANDIDATES", "Partial step without candidates", filePath));
                    partialCount++;
                }
                else if (status == "unresolved")
                {
                    unresolvedCount++;
                }

                // findings (if present) should be array of objects with severity/code/message
                if (step.TryGetProperty("findings", out var findings) && findings.ValueKind == JsonValueKind.Array)
                {
                    foreach (var f in findings.EnumerateArray())
                    {
                        if (f.ValueKind != JsonValueKind.Object)
                            result.AddError(new ValidationError("RESOLVED_FINDING_INVALID", "Finding must be object with severity/code/message", filePath));
                        else
                        {
                            if (!f.TryGetProperty("severity", out var sev) || (sev.GetString() != "error" && sev.GetString() != "warn" && sev.GetString() != "info"))
                                result.AddError(new ValidationError("RESOLVED_FINDING_INVALID_SEVERITY", "Invalid finding severity", filePath));
                            if (!f.TryGetProperty("code", out var code) || string.IsNullOrWhiteSpace(code.GetString()))
                                result.AddError(new ValidationError("RESOLVED_FINDING_MISSING_CODE", "Finding missing code", filePath));
                            if (!f.TryGetProperty("message", out var msg) || string.IsNullOrWhiteSpace(msg.GetString()))
                                result.AddError(new ValidationError("RESOLVED_FINDING_MISSING_MESSAGE", "Finding missing message", filePath));
                        }
                    }
                }
            }

            if (root.TryGetProperty("resolvedCount", out var rc) && rc.GetInt32() != resolvedCount)
                result.AddError(new ValidationError("RESOLVED_COUNT_MISMATCH", "resolvedCount mismatch", filePath));
            if (root.TryGetProperty("partialCount", out var pc) && pc.GetInt32() != partialCount)
                result.AddError(new ValidationError("RESOLVED_COUNT_MISMATCH", "partialCount mismatch", filePath));
            if (root.TryGetProperty("unresolvedCount", out var uc) && uc.GetInt32() != unresolvedCount)
                result.AddError(new ValidationError("RESOLVED_COUNT_MISMATCH", "unresolvedCount mismatch", filePath));

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
    }
}
