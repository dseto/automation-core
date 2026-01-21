using System;
using System.IO;
using Automation.Core.Configuration;
using Automation.Core.DataMap;
using Automation.Core.Debug;
using Automation.Core.Evidence;
using Automation.Core.Recorder;
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
    public DataMapModel DataMap { get; }
    public PageContext PageContext { get; }
    public ElementResolver Resolver { get; }
    public DataResolver Data { get; }
    public IWebDriver Driver { get; }
    public WaitService Waits { get; }
    public EvidenceManager Evidence { get; }
    public DebugController Debug { get; }
    public SessionRecorder? Recorder { get; }
    public ILogger Logger { get; }
    public string RunId { get; } = Guid.NewGuid().ToString("n");

    public AutomationRuntime(RunSettings settings, UiMapModel map, IWebDriver driver, ILogger logger)
    {
        Settings = settings;
        UiMap = map;
        Driver = driver;
        Logger = logger;

        // Carregar DataMap
        var dataMapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "data-map.yaml");
        DataMap = new DataMapLoader().Load(dataMapPath);
        Data = new DataResolver(DataMap, settings);

        Waits = new WaitService(settings, logger);
        Evidence = new EvidenceManager(logger);
        Debug = new DebugController(settings, logger);
        PageContext = new PageContext(map, driver);
        Resolver = new ElementResolver(map, PageContext);

        if (settings.RecordEnabled)
            Recorder = new SessionRecorder();
    }

    public void Dispose()
    {
        try { Driver.Quit(); } catch { }
        try { Driver.Dispose(); } catch { }
    }
}
