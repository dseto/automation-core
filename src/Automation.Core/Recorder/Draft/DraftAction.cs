using System.Collections.Generic;
using Automation.Core.Recorder;

namespace Automation.Core.Recorder.Draft;

public sealed class DraftAction
{
    public DraftAction(IReadOnlyList<RecorderEvent> events, IReadOnlyList<int> eventIndexes, string? rawScript = null)
    {
        Events = events;
        EventIndexes = eventIndexes;
        RawScript = rawScript;
    }

    public IReadOnlyList<RecorderEvent> Events { get; }

    public IReadOnlyList<int> EventIndexes { get; }

    public string? RawScript { get; }

    public RecorderEvent? PrimaryEvent => Events.Count > 0 ? Events[0] : null;
}
