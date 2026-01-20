using Microsoft.Extensions.Logging;
using Automation.Core.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Automation.Core.Debug;

/// <summary>
/// Controlador de debug visual para testes (highlight, slowmo, pause).
/// </summary>
public class DebugController
{
    private readonly RunSettings _settings;
    private readonly ILogger _logger;

    public DebugController(RunSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Destaca um elemento no navegador com borda amarela.
    /// </summary>
    public void Highlight(IWebElement element)
    {
        if (!_settings.Highlight || element == null)
            return;

        try
        {
            var driver = ((IWrapsDriver)element).WrappedDriver;
            var originalStyle = element.GetDomAttribute("style") ?? "";
            
            // Adicionar borda amarela
            var script = @"
                arguments[0].style.border = '3px solid yellow';
                arguments[0].style.backgroundColor = 'rgba(255, 255, 0, 0.2)';
            ";
            
            ((IJavaScriptExecutor)driver).ExecuteScript(script, element);
            
            // Restaurar estilo original após 500ms
            System.Threading.Thread.Sleep(500);
            ((IJavaScriptExecutor)driver).ExecuteScript(
                $"arguments[0].style.cssText = '{originalStyle}';", 
                element
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Erro ao destacar elemento: {ex.Message}");
        }
    }

    /// <summary>
    /// Tenta destacar um elemento (seguro para objetos que podem não ser elementos).
    /// </summary>
    public void TryHighlight(object? obj1, object? obj2 = null)
    {
        if (!_settings.Highlight)
            return;

        try
        {
            // Se obj1 é um elemento, destacar
            if (obj1 is IWebElement element1)
            {
                Highlight(element1);
                return;
            }

            // Se obj2 é um elemento, destacar
            if (obj2 is IWebElement element2)
            {
                Highlight(element2);
                return;
            }

            // Se obj1 é um driver, não fazer nada (não é um elemento)
            // Se obj1 é um string, não fazer nada (é um locator)
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Erro ao tentar destacar: {ex.Message}");
        }
    }

    /// <summary>
    /// Aplica slowmo (delay) entre ações para debug visual.
    /// </summary>
    public void MaybeSlowMo()
    {
        if (_settings.SlowMoMs <= 0)
            return;

        _logger.LogInformation($"⏸️  SlowMo: aguardando {_settings.SlowMoMs}ms");
        System.Threading.Thread.Sleep(_settings.SlowMoMs);
    }

    /// <summary>
    /// Pausa antes de cada step se configurado, com opção de continuar.
    /// </summary>
    public void MaybePauseEachStep(string? step = null)
    {
        if (!_settings.PauseEachStep)
            return;

        var stepInfo = string.IsNullOrWhiteSpace(step) ? "" : $" [{step}]";
        _logger.LogInformation($"⏸️  Pausa antes do step{stepInfo}. Pressione ENTER para continuar...");
        
        // Se for headless ou CI, não pausar
        if (_settings.Headless)
            return;

        // Ler entrada do console (se disponível)
        try
        {
            Console.ReadLine();
        }
        catch
        {
            // Se não conseguir ler do console (ex: CI environment), continuar
        }

        MaybeSlowMo();
    }

    /// <summary>
    /// Pausa se houver falha e estiver configurado.
    /// </summary>
    public void MaybePauseOnFailure(string? failureMessage = null)
    {
        if (!_settings.PauseOnFailure)
            return;

        var msg = string.IsNullOrWhiteSpace(failureMessage) 
            ? "Teste falhou" 
            : $"Teste falhou: {failureMessage}";
        
        _logger.LogError($"❌ {msg}. Pressione ENTER para continuar...");
        
        // Se for headless ou CI, não pausar
        if (_settings.Headless)
            return;

        try
        {
            Console.ReadLine();
        }
        catch
        {
            // Se não conseguir ler do console, continuar
        }
    }
}
