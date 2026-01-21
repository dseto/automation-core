using System.Collections.Generic;

namespace Automation.Core.Recorder.Draft;

public sealed class DraftFeature
{
    public string FeatureName { get; set; } = "Draft feature";

    public string ScenarioName { get; set; } = "Draft scenario from recorder";

    public List<string> Steps { get; set; } = new();
}
