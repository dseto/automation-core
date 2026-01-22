using System;
using System.IO;
using System.Text.Json;
using System.Text;
using Automation.Core.Recorder.Semantic.Models;

namespace Automation.Core.Recorder.Semantic
{
    public sealed class SemanticWriter
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public string WriteResolvedFeature(string content, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            var path = Path.Combine(outputDir, "resolved.feature");
            File.WriteAllText(path, content);
            return path;
        }

        public string WriteResolvedMetadata(ResolvedMetadata metadata, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            var path = Path.Combine(outputDir, "resolved.metadata.json");
            var json = JsonSerializer.Serialize(metadata, JsonOptions);
            File.WriteAllText(path, json);
            return path;
        }

        public string WriteUiGapsReport(UiGapsReport report, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            var jsonPath = Path.Combine(outputDir, "ui-gaps.report.json");
            var mdPath = Path.Combine(outputDir, "ui-gaps.report.md");

            var json = JsonSerializer.Serialize(report, JsonOptions);
            File.WriteAllText(jsonPath, json);

            var md = BuildMarkdown(report);
            File.WriteAllText(mdPath, md);

            return jsonPath;
        }

        private string BuildMarkdown(UiGapsReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# UI Gaps Report — Session {report.SessionId}");
            sb.AppendLine();
            sb.AppendLine($"Generated: {report.GeneratedAt}");
            sb.AppendLine();
            sb.AppendLine("## Stats");
            sb.AppendLine($"- Errors: {report.Stats.Errors}");
            sb.AppendLine($"- Warnings: {report.Stats.Warnings}");
            sb.AppendLine($"- Infos: {report.Stats.Infos}");
            sb.AppendLine($"- Total: {report.Stats.Total}");
            sb.AppendLine();
            sb.AppendLine("## Findings");
            foreach (var f in report.Findings)
            {
                sb.AppendLine($"- {f.Id} [{f.Severity}] {f.Code} — {f.Message} (line: {f.DraftLine})");
            }

            return sb.ToString();
        }
    }
}
