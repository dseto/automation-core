using Automation.Core.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace Automation.Core.Driver;

public sealed class EdgeDriverFactory
{
    private readonly ILogger _logger;

    public EdgeDriverFactory(ILogger logger) => _logger = logger;

    public IWebDriver Create(RunSettings settings)
    {
        var options = new EdgeOptions();

        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--disable-gpu");
        }

        options.AddArgument("--window-size=1365,768");
        options.AddArgument("--no-first-run");
        options.AddArgument("--no-default-browser-check");
        options.AddArgument("--disable-extensions");

        _logger.LogInformation("Creating Edge driver. Headless={Headless} UiDebug={UiDebug}", settings.Headless, settings.UiDebug);

        // Requires msedgedriver compatible with Edge (PATH or Selenium Manager support).
        return new EdgeDriver(options);
    }
}
