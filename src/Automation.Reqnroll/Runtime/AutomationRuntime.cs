
using Automation.Core.Configuration;
using Automation.Core.Debug;
using Automation.Core.Driver;
using Automation.Core.Evidence;
using Automation.Core.Resolution;
using Automation.Core.UiMap;
using Automation.Core.Waits;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Automation.Reqnroll.Runtime;

public sealed class AutomationRuntime : IDisposable
{
    public RunSettings Settings { get; }
    public UiMapModel UiMap { get; }
    public PageContext PageContext { get; } = new();
    public ElementResolver Resolver { get; }
    public IWebDriver Driver { get; }
    public WaitService Waits { get; }
    public EvidenceManager Evidence { get; }
    public DebugController Debug { get; }
    public ILogger Logger { get; }

    public string RunId { get; } = Guid.NewGuid().ToString("n");

    public AutomationRuntime(RunSettings settings, UiMapModel map, IWebDriver driver, ILogger logger)
    {
        Settings = settings;
        UiMap = map;
        Driver = driver;
        Logger = logger;

        Waits = new WaitService(settings, logger);
        Evidence = new EvidenceManager(logger);
        Debug = new DebugController(settings, logger);

        Resolver = new ElementResolver(map, PageContext);
    }

    public void Dispose()
    {
        try { Driver.Quit(); } catch { }
        try { Driver.Dispose(); } catch { }
    }
}
