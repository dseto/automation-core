
using Automation.Core.Configuration;
using Automation.Core.Driver;
using Automation.Core.UiMap;
using Automation.Reqnroll.Runtime;
using BoDi;
using Microsoft.Extensions.Logging;
using Reqnroll;

namespace Automation.Reqnroll.Hooks;

[Binding]
public sealed class RuntimeHooks
{
    private readonly IObjectContainer _container;
    private AutomationRuntime? _rt;

    public RuntimeHooks(IObjectContainer container) => _container = container;

    [BeforeScenario(Order = 0)]
    public void BeforeScenario()
    {
        using var loggerFactory = LoggerFactory.Create(b =>
        {
            b.AddSimpleConsole(o =>
            {
                o.SingleLine = true;
                o.TimestampFormat = "HH:mm:ss ";
            });
            b.SetMinimumLevel(LogLevel.Information);
        });
        var logger = loggerFactory.CreateLogger("Automation");

        var settings = RunSettings.FromEnvironment();
        var uiMapPath = Environment.GetEnvironmentVariable("UI_MAP_PATH") ?? @".\samples\ui\ui-map.yaml";
        var map = new UiMapLoader().LoadFromFile(uiMapPath);

        var driver = new EdgeDriverFactory(logger).Create(settings);

        _rt = new AutomationRuntime(settings, map, driver, logger);
        _rt.Debug.Banner();

        _container.RegisterInstanceAs(_rt);
    }

    [AfterScenario(Order = 100)]
    public void AfterScenario()
    {
        _rt?.Dispose();
    }
}
