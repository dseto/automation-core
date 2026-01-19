using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using Automation.Core.Configuration;

namespace Automation.Core.Driver;

public static class EdgeDriverFactory
{
    public static IWebDriver Create(RunSettings settings)
    {
        var options = new EdgeOptions();
        if (settings.Headless) options.AddArgument("--headless");
        return new EdgeDriver(options);
    }
}
