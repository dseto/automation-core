using Automation.Core.Configuration;
using Automation.Core.Driver;
using Automation.Core.Resolution;
using Automation.Core.UiMap;
using Automation.Core.Waits;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Automation.Acceptance.Tests;

public class Login_com_sucesso
{
    [Fact]
    public void Runs_demo_flow_when_RUN_E2E_true()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_E2E"), "true", StringComparison.OrdinalIgnoreCase))
            return;

        using var loggerFactory = LoggerFactory.Create(b => b.AddSimpleConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger("E2E");

        var uiMapPath = Path.GetFullPath(@".\samples\ui\ui-map.yaml");
        var map = UiMapLoader.LoadFromFile(uiMapPath);

        var settings = RunSettings.FromEnvironment() with { WaitAngular = false, Headless = true };

        var driver = new EdgeDriverFactory(logger).Create(settings);
        try
        {
            var waits = new WaitService(settings, logger);
            var ctx = new PageContext();
            ctx.SetPage("LoginPage");
            var resolver = new ElementResolver(map, ctx);

            var demo = Path.Combine(AppContext.BaseDirectory, "Assets", "demo.html");
            driver.Navigate().GoToUrl(new Uri(demo).AbsoluteUri);

            waits.WaitDomReady(driver);

            waits.WaitVisibleByCss(driver, resolver.Resolve("Usuario").CssLocator).SendKeys("admin");
            waits.WaitVisibleByCss(driver, resolver.Resolve("Senha").CssLocator).SendKeys("123");
            waits.WaitClickableByCss(driver, resolver.Resolve("Entrar").CssLocator).Click();

            waits.WaitDomReady(driver);

            var welcome = waits.WaitVisibleByCss(driver, resolver.Resolve("TituloBoasVindas").CssLocator);
            Assert.True(welcome.Displayed);
        }
        finally
        {
            try { driver.Quit(); } catch { }
            driver.Dispose();
        }
    }
}
