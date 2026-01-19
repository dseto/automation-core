using System;
using System.Collections.Generic;

namespace Automation.Core.DataMap;

public class DataMapModel
{
    public Dictionary<string, object> Contexts { get; set; } = new();
    public Dictionary<string, object> Datasets { get; set; } = new();
}
