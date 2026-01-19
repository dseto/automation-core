using Microsoft.Extensions.Logging;
using Automation.Core.Configuration;
using OpenQA.Selenium;

namespace Automation.Core.Debug;

public class DebugController
{
    public DebugController(RunSettings settings, ILogger logger) { }
    public void Highlight(IWebElement element) { }
    public void TryHighlight(object? obj1, object? obj2 = null) { }
    public void MaybeSlowMo() { }
    public void MaybePauseEachStep(string? step = null) { }
}
