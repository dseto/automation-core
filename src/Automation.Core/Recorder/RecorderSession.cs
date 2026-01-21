using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Automation.Core.Recorder;

public sealed class RecorderSession
{
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = "";

    [JsonPropertyName("startedAt")]
    public DateTimeOffset StartedAt { get; set; }

    [JsonPropertyName("endedAt")]
    public DateTimeOffset? EndedAt { get; set; }

    [JsonPropertyName("events")]
    public List<RecorderEvent> Events { get; set; } = new();
}
