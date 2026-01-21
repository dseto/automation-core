using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Automation.Core.Recorder.Draft;

public sealed class DraftMetadata
{
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = "";

    [JsonPropertyName("inputStatus")]
    public string InputStatus { get; set; } = "";

    [JsonPropertyName("generatedAt")]
    public string GeneratedAt { get; set; } = "";

    [JsonPropertyName("eventsCount")]
    public int EventsCount { get; set; }

    [JsonPropertyName("actionsCount")]
    public int ActionsCount { get; set; }

    [JsonPropertyName("stepsInferredCount")]
    public int StepsInferredCount { get; set; }

    [JsonPropertyName("escapeHatchCount")]
    public int EscapeHatchCount { get; set; }

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = new();

    [JsonPropertyName("mappings")]
    public List<DraftMapping> Mappings { get; set; } = new();
}

public sealed class DraftMapping
{
    [JsonPropertyName("eventIndex")]
    public int EventIndex { get; set; }

    [JsonPropertyName("actionIndex")]
    public int ActionIndex { get; set; }

    [JsonPropertyName("draftLine")]
    public int DraftLine { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}
