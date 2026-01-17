using Automation.Core.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Automation.Core.Debug;

public sealed class DebugController
{
    private readonly RunSettings _settings;
    private readonly ILogger _logger;

    public DebugController(RunSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public void Banner()
    {
        if (_settings.UiDebug)
            _logger.LogWarning("DEBUG VISUAL ON (UI_DEBUG=true). Headless forced off.");
    }

    public void MaybePauseEachStep(string stepText)
    {
        if (!_settings.UiDebug || !_settings.PauseEachStep) return;
        Console.WriteLine($">> STEP: {stepText}");
        Console.Write("Press ENTER to continue...");
        Console.ReadLine();
    }

    public void MaybeSlowMo()
    {
        if (!_settings.UiDebug) return;
        if (_settings.SlowMoMs > 0) Thread.Sleep(_settings.SlowMoMs);
    }

    public void TryHighlight(IWebDriver driver, IWebElement el)
    {
        if (!_settings.UiDebug || !_settings.Highlight) return;
        try
        {
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].style.outline='3px solid #ff0'; arguments[0].style.backgroundColor='rgba(255,255,0,0.15)';", el);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Highlight failed (best-effort).");
        }
    }

    public void PauseOnFailureIfEnabled()
    {
        if (!_settings.UiDebug || !_settings.PauseOnFailure) return;
        Console.WriteLine("Test failed. Browser kept open for inspection. Press ENTER to close...");
        Console.ReadLine();
    }
}
