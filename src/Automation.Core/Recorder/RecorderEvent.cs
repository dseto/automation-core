using System.Text.Json.Serialization;

namespace Automation.Core.Recorder;

public sealed class RecorderEvent
{
    [JsonPropertyName("t")]
    public string T { get; init; } = "";

    [JsonPropertyName("type")]
    public string Type { get; init; } = "";

    [JsonPropertyName("route")]
    public string? Route { get; init; }

    [JsonPropertyName("target")]
    public object? Target { get; init; }

    [JsonPropertyName("value")]
    public object? Value { get; init; }

    [JsonPropertyName("rawAction")]
    public object? RawAction { get; init; }

    [JsonPropertyName("waitMs")]
    public int? WaitMs { get; init; }
}
