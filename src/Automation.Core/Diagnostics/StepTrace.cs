using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Automation.Core.Diagnostics;

public sealed record StepTraceEntry(
    string StepText,
    string? Page,
    string? Url,
    string? FriendlyName,
    string? TestId,
    string? CssLocator,
    long DurationMs,
    bool? AngularFallback,
    bool? Success,
    string? Error,
    DateTimeOffset TimestampUtc);

public sealed class StepTrace
{
    private readonly ILogger _logger;
    private string? _stepsLogPath;

    public StepTrace(ILogger logger) => _logger = logger;

    public void Initialize(string scenarioFolder)
    {
        Directory.CreateDirectory(scenarioFolder);
        _stepsLogPath = Path.Combine(scenarioFolder, "steps.log");
    }

    public void LogStep(StepTraceEntry entry)
    {
        if (_stepsLogPath is null) return;

        try
        {
            var line = JsonSerializer.Serialize(entry);
            File.AppendAllText(_stepsLogPath, line + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to write steps.log (best-effort)." );
        }
    }
}
