using Automation.Core.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Automation.Core.Driver;

public static class ChromeDriverFactory
{
    public static IWebDriver Create(RunSettings settings)
    {
        var options = new ChromeOptions();

        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--disable-gpu");
            // Keep a deterministic size in headless mode
            options.AddArgument("--window-size=1365,768");
        }
        else
        {
            // Start browser maximized in headed/debug sessions
            options.AddArgument("--start-maximized");
        }

        options.AddArgument("--no-first-run");
        options.AddArgument("--no-default-browser-check");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--no-sandbox");

        var driver = new ChromeDriver(options);

        // Best-effort maximize the window after creation (some drivers ignore startup flags)
        if (!settings.Headless)
        {
            try { driver.Manage().Window.Maximize(); } catch { }
        }

        return driver;
    }
}
