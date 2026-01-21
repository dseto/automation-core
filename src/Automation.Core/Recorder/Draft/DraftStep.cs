using System.Collections.Generic;

namespace Automation.Core.Recorder.Draft;

public sealed class DraftStep
{
    public DraftStep(string text, IReadOnlyList<int> eventIndexes, double confidence)
    {
        Text = text;
        EventIndexes = eventIndexes;
        Confidence = confidence;
    }

    public string Text { get; }

    public IReadOnlyList<int> EventIndexes { get; }

    public double Confidence { get; }
}
