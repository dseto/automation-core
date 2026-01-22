using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Automation.Core.Recorder.Draft;

public sealed class EscapeHatchRenderer
{
    private const int RawScriptMaxLength = 500;

    public EscapeHatchResult Render(DraftAction action)
    {
        var warnings = new List<string>();
        var primary = action.PrimaryEvent;

        var raw = new Dictionary<string, object?>
        {
            ["type"] = primary?.Type,
            ["at"] = primary?.T,
            ["target"] = primary?.Target,
            ["value"] = primary?.Value
        };

        if (!string.IsNullOrWhiteSpace(action.RawScript))
        {
            var script = action.RawScript;
            if (script.Length > RawScriptMaxLength)
            {
                script = script.Substring(0, RawScriptMaxLength) + "…(truncated)";
                warnings.Add("RAW_SCRIPT_TRUNCATED");
            }

            raw["rawAction"] = new Dictionary<string, object?>
            {
                ["script"] = script
            };
        }
        else if (primary?.RawAction != null)
        {
            raw["rawAction"] = primary.RawAction;
        }

        var rawJson = JsonSerializer.Serialize(raw);

        var lines = new List<string>
        {
            "# TODO: revisar ação não inferida",
            $"# RAW: {rawJson}"
        };

        // If any event in the action specifies a wait >= 1s, surface it as a draft line
        var waitMs = System.Linq.Enumerable.Where(action.Events.Select(e => e.WaitMs), w => w.HasValue && w.Value >= 1000)
            .Select(w => w!.Value)
            .FirstOrDefault();

        if (waitMs > 0)
        {
            var seconds = waitMs / 1000;
            lines.Insert(0, $"E eu espero {seconds} segundos");
        }

        return new EscapeHatchResult(lines, warnings);
    }
}

public sealed class EscapeHatchResult
{
    public EscapeHatchResult(IReadOnlyList<string> lines, IReadOnlyList<string> warnings)
    {
        Lines = lines;
        Warnings = warnings;
    }

    public IReadOnlyList<string> Lines { get; }

    public IReadOnlyList<string> Warnings { get; }
}
