namespace Automation.Core.Recorder.Draft;

public sealed class DraftGenerationResult
{
    public bool IsSuccess { get; init; }

    public string InputStatus { get; init; } = "";

    public string? Warning { get; init; }

    public string? DraftPath { get; init; }

    public string? MetadataPath { get; init; }
}
