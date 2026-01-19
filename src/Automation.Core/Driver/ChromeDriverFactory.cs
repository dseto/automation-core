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
        }

        options.AddArgument("--window-size=1365,768");
        options.AddArgument("--no-first-run");
        options.AddArgument("--no-default-browser-check");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--no-sandbox");

        return new ChromeDriver(options);
    }
}
