using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Automation.Core.Recorder.Semantic.Models
{
    public sealed class ResolvedMetadata
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "0.1.0";

        [JsonPropertyName("generatedAt")]
        public string GeneratedAt { get; set; } = "";

        [JsonPropertyName("source")]
        public ResolvedSource Source { get; set; } = new ResolvedSource();

        [JsonPropertyName("steps")]
        public List<ResolvedStep> Steps { get; set; } = new();
    }

    public sealed class ResolvedSource
    {
        [JsonPropertyName("draftFeaturePath")]
        public string DraftFeaturePath { get; set; } = "";

        [JsonPropertyName("uiMapPath")]
        public string UiMapPath { get; set; } = "";

        [JsonPropertyName("sessionPath")]
        public string? SessionPath { get; set; }
    }

    public sealed class ResolvedStep
    {
        [JsonPropertyName("draftLine")]
        public int DraftLine { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "unresolved"; // resolved | partial | unresolved

        [JsonPropertyName("stepText")]
        public string StepText { get; set; } = "";

        [JsonPropertyName("chosen")]
        public ResolvedChosen? Chosen { get; set; }

        [JsonPropertyName("candidates")]
        public List<ResolvedCandidate> Candidates { get; set; } = new();

        [JsonPropertyName("findings")]
        public List<ResolvedFinding> Findings { get; set; } = new();
    }

    public sealed class ResolvedChosen
    {
        [JsonPropertyName("pageKey")]
        public string PageKey { get; set; } = "";

        [JsonPropertyName("elementKey")]
        public string ElementKey { get; set; } = "";

        [JsonPropertyName("testId")]
        public string? TestId { get; set; }
    }

    public sealed class ResolvedCandidate
    {
        [JsonPropertyName("pageKey")]
        public string PageKey { get; set; } = "";

        [JsonPropertyName("elementKey")]
        public string ElementKey { get; set; } = "";

        [JsonPropertyName("testId")]
        public string? TestId { get; set; }
    }

    public sealed class ResolvedFinding
    {
        [JsonPropertyName("severity")]
        public string Severity { get; set; } = "error"; // error | warn | info

        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
    }

    public sealed class UiGapsReport
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "0.1.0";

        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = "";

        [JsonPropertyName("generatedAt")]
        public string GeneratedAt { get; set; } = "";

        [JsonPropertyName("draftPath")]
        public string DraftPath { get; set; } = "";

        [JsonPropertyName("uimapPath")]
        public string UiMapPath { get; set; } = "";

        [JsonPropertyName("findings")]
        public List<UiGapFinding> Findings { get; set; } = new();

        [JsonPropertyName("stats")]
        public UiGapStats Stats { get; set; } = new();
    }

    public sealed class UiGapFinding
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("severity")]
        public string Severity { get; set; } = "error"; // error | warn | info

        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("draftLine")]
        public int DraftLine { get; set; }

        [JsonPropertyName("stepText")]
        public string StepText { get; set; } = "";

        [JsonPropertyName("route")]
        public string? Route { get; set; }

        [JsonPropertyName("page")]
        public string? Page { get; set; }

        [JsonPropertyName("element")]
        public string? Element { get; set; }

        [JsonPropertyName("inputRef")]
        public string? InputRef { get; set; }

        [JsonPropertyName("suggestedFix")]
        public object? SuggestedFix { get; set; }

        [JsonPropertyName("evidence")]
        public object? Evidence { get; set; }
    }

    public sealed class UiGapStats
    {
        [JsonPropertyName("errors")]
        public int Errors { get; set; }

        [JsonPropertyName("warnings")]
        public int Warnings { get; set; }

        [JsonPropertyName("infos")]
        public int Infos { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
