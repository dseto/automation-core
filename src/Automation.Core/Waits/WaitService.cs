using Automation.Core.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Automation.Core.Waits;

/// <summary>
/// Serviço de esperas inteligentes para automação de UI.
/// Inclui waits para DOM, Angular, navegação e elementos.
/// </summary>
public sealed class WaitService
{
    private readonly RunSettings _settings;
    private readonly ILogger _logger;

    public WaitService(RunSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    #region DOM Waits

    /// <summary>
    /// Aguarda o DOM estar completamente carregado (document.readyState === 'complete').
    /// </summary>
    public void WaitDomReady(IWebDriver driver)
    {
        var wait = CreateWait(driver, _settings.StepTimeoutMs);
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

    #endregion

    #region Angular Waits

    /// <summary>
    /// Tenta aguardar a estabilidade do Angular (zone.js).
    /// Retorna true se Angular estável, false se timeout ou não-Angular.
    /// </summary>
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

            var wait = CreateWait(driver, _settings.AngularTimeoutMs);
            wait.Until(_ =>
            {
                var r = js.ExecuteScript(script);
                if (r is Dictionary<string, object> dict && dict.TryGetValue("ok", out var ok) && ok is bool okb && !okb)
                    return true;
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

    #endregion

    #region Navigation Waits

    /// <summary>
    /// Aguarda a URL conter uma substring específica.
    /// </summary>
    public void WaitForUrlContains(IWebDriver driver, string substring, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _settings.StepTimeoutMs;
        var wait = CreateWait(driver, timeout);

        _logger.LogDebug("Waiting for URL to contain: {Substring}", substring);

        wait.Until(d =>
        {
            try
            {
                return d.Url.Contains(substring, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        });

        _logger.LogDebug("URL now contains: {Substring}. Current URL: {Url}", substring, driver.Url);
    }

    /// <summary>
    /// Aguarda a URL mudar de um valor anterior.
    /// </summary>
    public void WaitForUrlChange(IWebDriver driver, string previousUrl, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _settings.StepTimeoutMs;
        var wait = CreateWait(driver, timeout);

        _logger.LogDebug("Waiting for URL to change from: {PreviousUrl}", previousUrl);

        wait.Until(d =>
        {
            try
            {
                return !string.Equals(d.Url, previousUrl, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        });

        _logger.LogDebug("URL changed. New URL: {Url}", driver.Url);
    }

    /// <summary>
    /// Executa uma ação (ex: clique) e aguarda a navegação completar.
    /// Detecta automaticamente mudança de URL e aguarda DOM + Angular.
    /// </summary>
    public void WaitForNavigation(IWebDriver driver, Action action, int? timeoutMs = null)
    {
        var previousUrl = driver.Url;
        _logger.LogDebug("Executing action and waiting for navigation. Current URL: {Url}", previousUrl);

        action();

        // Aguarda URL mudar
        WaitForUrlChange(driver, previousUrl, timeoutMs);

        // Aguarda DOM e Angular estabilizarem
        WaitDomReady(driver);
        TryWaitAngularStable(driver, out _);

        _logger.LogDebug("Navigation complete. New URL: {Url}", driver.Url);
    }

    /// <summary>
    /// Executa uma ação e aguarda a rota conter um valor específico.
    /// Útil para SPAs onde a navegação é por rota.
    /// </summary>
    public void WaitForNavigationToRoute(IWebDriver driver, Action action, string expectedRoute, int? timeoutMs = null)
    {
        _logger.LogDebug("Executing action and waiting for route: {Route}", expectedRoute);

        action();

        // Aguarda um pouco para a navegação iniciar
        System.Threading.Thread.Sleep(500);

        // Aguarda rota esperada
        WaitForUrlContains(driver, expectedRoute, timeoutMs);

        // Aguarda DOM e Angular estabilizarem
        WaitDomReady(driver);
        TryWaitAngularStable(driver, out _);

        _logger.LogDebug("Navigation to route complete. Current URL: {Url}", driver.Url);
    }

    #endregion

    #region Element Waits

    /// <summary>
    /// Aguarda um elemento estar visível pelo seletor CSS.
    /// </summary>
    public IWebElement WaitVisibleByCss(IWebDriver driver, string css, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _settings.StepTimeoutMs;
        var wait = CreateWait(driver, timeout);

        var el = wait.Until<IWebElement?>(d =>
        {
            try
            {
                var found = d.FindElement(By.CssSelector(css));
                return found.Displayed ? found : null;
            }
            catch { return null; }
        });

        return el ?? throw new WebDriverTimeoutException(
            $"Elemento '{css}' não ficou visível dentro do timeout de {timeout}ms.");
    }

    /// <summary>
    /// Aguarda um elemento estar clicável pelo seletor CSS.
    /// </summary>
    public IWebElement WaitClickableByCss(IWebDriver driver, string css, int? timeoutMs = null)
    {
        var el = WaitVisibleByCss(driver, css, timeoutMs);
        if (!el.Enabled)
            throw new InvalidOperationException(
                $"Elemento '{css}' está visível mas não está habilitado para clique.");
        return el;
    }

    /// <summary>
    /// Aguarda um elemento desaparecer ou não existir.
    /// </summary>
    public void WaitElementNotVisible(IWebDriver driver, string css, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _settings.StepTimeoutMs;
        var wait = CreateWait(driver, timeout);

        wait.Until(d =>
        {
            try
            {
                var elements = d.FindElements(By.CssSelector(css));
                return elements.Count == 0 || elements.All(e => !e.Displayed);
            }
            catch { return true; }
        });
    }

    /// <summary>
    /// Aguarda um elemento conter um texto específico.
    /// </summary>
    public IWebElement WaitElementContainsText(IWebDriver driver, string css, string text, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _settings.StepTimeoutMs;
        var wait = CreateWait(driver, timeout);

        var el = wait.Until<IWebElement?>(d =>
        {
            try
            {
                var found = d.FindElement(By.CssSelector(css));
                return found.Text.Contains(text, StringComparison.OrdinalIgnoreCase) ? found : null;
            }
            catch { return null; }
        });

        return el ?? throw new WebDriverTimeoutException(
            $"Elemento '{css}' não contém o texto '{text}' dentro do timeout de {timeout}ms.");
    }

    /// <summary>
    /// Aguarda um elemento ter um atributo com valor específico.
    /// </summary>
    public IWebElement WaitElementAttribute(IWebDriver driver, string css, string attribute, string value, int? timeoutMs = null)
    {
        var timeout = timeoutMs ?? _settings.StepTimeoutMs;
        var wait = CreateWait(driver, timeout);

        var el = wait.Until<IWebElement?>(d =>
        {
            try
            {
                var found = d.FindElement(By.CssSelector(css));
                var attrValue = found.GetAttribute(attribute);
                return string.Equals(attrValue, value, StringComparison.OrdinalIgnoreCase) ? found : null;
            }
            catch { return null; }
        });

        return el ?? throw new WebDriverTimeoutException(
            $"Elemento '{css}' não tem atributo '{attribute}' = '{value}' dentro do timeout de {timeout}ms.");
    }

    #endregion

    #region Anchor Waits

    /// <summary>
    /// Aguarda o anchor de uma página estar visível.
    /// Útil para confirmar que uma página foi carregada.
    /// </summary>
    public IWebElement WaitPageAnchor(IWebDriver driver, string anchorTestId, int? timeoutMs = null)
    {
        var css = $"[data-testid='{anchorTestId}']";
        _logger.LogDebug("Waiting for page anchor: {Anchor}", anchorTestId);

        var el = WaitVisibleByCss(driver, css, timeoutMs);

        _logger.LogDebug("Page anchor visible: {Anchor}", anchorTestId);
        return el;
    }

    #endregion

    #region Helpers

    private static WebDriverWait CreateWait(IWebDriver driver, int timeoutMs)
    {
        return new WebDriverWait(
            new SystemClock(),
            driver,
            TimeSpan.FromMilliseconds(timeoutMs),
            TimeSpan.FromMilliseconds(200));
    }

    #endregion
}
