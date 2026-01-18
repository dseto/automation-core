
using Automation.Core.Configuration;
using Automation.Core.Driver;
using Automation.Core.UiMap;
using Automation.Reqnroll.Runtime;
using Reqnroll.BoDi;
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
        var uiMapPath = ResolveUiMapPath();
        var map = UiMapLoader.LoadFromFile(uiMapPath);

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

    private static string ResolveUiMapPath()
    {
        // 1) explicit override
        var env = Environment.GetEnvironmentVariable("UI_MAP_PATH");
        if (!string.IsNullOrWhiteSpace(env) && File.Exists(env))
            return env;

        // 2) common relative locations (project root)
        var candidates = new[]
        {
            Path.Combine("ui", "ui-map.yaml"),
            Path.Combine("samples", "ui", "ui-map.yaml"),
            "ui-map.yaml"
        };

        // 3) search upwards from base directory and current directory
        var starts = new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var start in starts)
        {
            var dir = new DirectoryInfo(start);
            for (var i = 0; i < 10 && dir != null; i++, dir = dir.Parent)
            {
                foreach (var rel in candidates)
                {
                    var full = Path.GetFullPath(Path.Combine(dir.FullName, rel));
                    if (File.Exists(full))
                        return full;
                }
            }
        }

        // 4) last resort: keep previous default so error message stays familiar
        return @".\samples\ui\ui-map.yaml";
    }
}
