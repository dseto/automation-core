using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using Automation.Core.Configuration;

namespace Automation.Core.Driver;

public static class EdgeDriverFactory
{
    public static IWebDriver Create(RunSettings settings)
    {
        var options = new EdgeOptions();
        if (settings.Headless)
        {
            options.AddArgument("--headless");
        }
        else
        {
            // Start browser maximized in headed/debug sessions
            options.AddArgument("--start-maximized");
        }

        var driver = new EdgeDriver(options);

        // Best-effort maximize the window after creation (some drivers ignore startup flags)
        if (!settings.Headless)
        {
            try { driver.Manage().Window.Maximize(); } catch { }
        }

        return driver;
    }
}
