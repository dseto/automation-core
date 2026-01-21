using System;
using System.IO;
using System.Linq;
using Automation.Core.Configuration;
using Automation.Core.Driver;
using Automation.Core.Recorder;
using Automation.Core.UiMap;
using Automation.Reqnroll.Runtime;
using Reqnroll.BoDi;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
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
        var loggerFactory = LoggerFactory.Create(b =>
        {
            b.AddSimpleConsole(o =>
            {
                o.SingleLine = true;
                o.TimestampFormat = "HH:mm:ss ";
            });
            b.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        });
        var logger = loggerFactory.CreateLogger("Automation");

        var settings = RunSettings.FromEnvironment();
        var uiMapPath = ResolveUiMapPath();
        var map = UiMapLoader.LoadFromFile(uiMapPath);

        // Suporta BROWSER=chrome ou BROWSER=edge (default: chrome)
        var browserType = Environment.GetEnvironmentVariable("BROWSER")?.ToLowerInvariant() ?? "chrome";
        IWebDriver driver = browserType switch
        {
            "edge" => EdgeDriverFactory.Create(settings),
            _ => ChromeDriverFactory.Create(settings)
        };

        _rt = new AutomationRuntime(settings, map, driver, logger);

        _rt.Recorder?.Start();
        
        _container.RegisterInstanceAs(_rt);
        _container.RegisterInstanceAs(_rt.PageContext);
        _container.RegisterInstanceAs(_rt.Waits);
        _container.RegisterInstanceAs(_rt.Resolver);
        _container.RegisterInstanceAs(_rt.Data);
        _container.RegisterInstanceAs(_rt.Driver);
        _container.RegisterInstanceAs(_rt.Settings);
        _container.RegisterInstanceAs(_rt.UiMap);
        _container.RegisterInstanceAs(logger);
    }

    [AfterScenario(Order = 100)]
    public void AfterScenario()
    {
        if (_rt?.Recorder != null)
        {
            _rt.Recorder.Stop();
            try
            {
                var writer = new SessionWriter();
                var path = writer.Write(_rt.Recorder.GetSession(), _rt.Settings.RecordOutputDir);
                _rt.Logger.LogInformation("Recorder session written: {Path}", path);
            }
            catch (Exception ex)
            {
                _rt.Logger.LogWarning(ex, "Failed to write recorder session.");
            }
        }

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

        return "ui-map.yaml";
    }
}
