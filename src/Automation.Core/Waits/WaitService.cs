using Automation.Core.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Automation.Core.Waits;

public sealed class WaitService
{
    private readonly RunSettings _settings;
    private readonly ILogger _logger;

    public WaitService(RunSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public void WaitDomReady(IWebDriver driver)
    {
        var wait = new WebDriverWait(new SystemClock(), driver, TimeSpan.FromMilliseconds(_settings.StepTimeoutMs), TimeSpan.FromMilliseconds(200));
        wait.Until(d =>
        {
            try
            {
                var js = (IJavaScriptExecutor)d;
                var state = js.ExecuteScript("return document.readyState")?.ToString();
                return state == "complete";
            }
            catch { return false; }
        });
    }

    public bool TryWaitAngularStable(IWebDriver driver, out bool fallbackUsed)
    {
        fallbackUsed = false;
        if (!_settings.WaitAngular) return true;

        try
        {
            var js = (IJavaScriptExecutor)driver;
            var script = @"
                try {
                  const f = window.getAllAngularTestabilities;
                  if (!f) return { ok:false, reason:'missing' };
                  const ts = f();
                  const stable = ts.every(t => t.isStable());
                  return { ok:true, stable: stable };
                } catch(e) { return { ok:false, reason:'error', msg: e.toString() }; }
            ";

            var wait = new WebDriverWait(new SystemClock(), driver, TimeSpan.FromMilliseconds(_settings.AngularTimeoutMs), TimeSpan.FromMilliseconds(200));
            wait.Until(_ =>
            {
                var r = js.ExecuteScript(script);
                if (r is Dictionary<string, object> dict && dict.TryGetValue("ok", out var ok) && ok is bool okb && !okb)
                    return true; // missing/error -> stop waiting and fallback
                if (r is Dictionary<string, object> dict2 && dict2.TryGetValue("stable", out var st) && st is bool stb)
                    return stb;
                return true;
            });

            var r2 = js.ExecuteScript(script);
            if (r2 is Dictionary<string, object> dict3 && dict3.TryGetValue("ok", out var ok2) && ok2 is bool okb2 && !okb2)
            {
                fallbackUsed = true;
                _logger.LogDebug("Angular testability missing/error; using fallback element waits.");
            }

            return true;
        }
        catch (WebDriverTimeoutException)
        {
            fallbackUsed = true;
            _logger.LogWarning("Angular wait timed out after {Ms}ms. Falling back to element waits.", _settings.AngularTimeoutMs);
            return false;
        }
        catch (Exception ex)
        {
            fallbackUsed = true;
            _logger.LogDebug(ex, "Angular wait failed; falling back.");
            return false;
        }
    }

    public IWebElement WaitVisibleByCss(IWebDriver driver, string css)
    {
        var wait = new WebDriverWait(new SystemClock(), driver, TimeSpan.FromMilliseconds(_settings.StepTimeoutMs), TimeSpan.FromMilliseconds(200));
        var el = wait.Until<IWebElement?>(d =>
        {
            try
            {
                var found = d.FindElement(By.CssSelector(css));
                return found.Displayed ? found : null;
            }
            catch { return null; }
        });

        return el ?? throw new WebDriverTimeoutException($"Element '{css}' was not visible within timeout.");
    }

    public IWebElement WaitClickableByCss(IWebDriver driver, string css)
    {
        var el = WaitVisibleByCss(driver, css);
        if (!el.Enabled) throw new InvalidOperationException($"Element '{css}' is visible but not enabled.");
        return el;
    }
}
