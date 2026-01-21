using System;
using System.IO;
using System.Threading;
using Automation.Core.Configuration;
using Automation.Core.Driver;
using Automation.Core.Recorder;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Automation.RecorderTool;

/// <summary>
/// FREE-HANDS Recorder — Modo Exploratório (RF00)
/// 
/// Entrypoint standalone que permite abrir o browser e interagir livremente,
/// sem depender de .feature, cenário ou steps.
/// 
/// Ao encerrar (fechar browser ou CTRL+C), gera session.json.
/// </summary>
public class Program
{
    private static SessionRecorder? _recorder;
    private static IWebDriver? _driver;
    private static RunSettings? _settings;
    private static ILogger? _logger;
    private static readonly ManualResetEventSlim _exitEvent = new(false);

    public static int Main(string[] args)
    {
        // 1) Verificar se modo exploratório está ativado
        var recordEnabled = Environment.GetEnvironmentVariable("AUTOMATION_RECORD")
                            ?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

        if (!recordEnabled)
        {
            Console.WriteLine("[RecorderTool] AUTOMATION_RECORD não está definido como 'true'.");
            Console.WriteLine("[RecorderTool] Use: $env:AUTOMATION_RECORD='true' antes de executar.");
            return 1;
        }

        // 2) Configurar logging
        var loggerFactory = LoggerFactory.Create(b =>
        {
            b.AddSimpleConsole(o =>
            {
                o.SingleLine = true;
                o.TimestampFormat = "HH:mm:ss ";
            });
            b.SetMinimumLevel(LogLevel.Information);
        });
        _logger = loggerFactory.CreateLogger("RecorderTool");

        // 3) Carregar settings
        _settings = RunSettings.FromEnvironment();

        // 4) Garantir modo headed (exploratório requer UI visível)
        if (_settings.Headless)
        {
            _logger.LogWarning("Modo exploratório requer browser visível. Ignorando HEADLESS=true.");
            // Forçar headed via nova instância de settings
            _settings = _settings with { Headless = false };
        }

        // 5) Registrar handler para CTRL+C
        Console.CancelKeyPress += OnCancelKeyPress;

        // 6) Iniciar browser
        _logger.LogInformation("Iniciando browser em modo exploratório...");
        
        var browserType = Environment.GetEnvironmentVariable("BROWSER")?.ToLowerInvariant() ?? "chrome";
        _driver = browserType switch
        {
            "edge" => EdgeDriverFactory.Create(_settings),
            _ => ChromeDriverFactory.Create(_settings)
        };

        // 7) Iniciar recorder
        _recorder = new SessionRecorder();
        _recorder.Start();
        _logger.LogInformation("Recorder iniciado. SessionId: {SessionId}", _recorder.GetSession().SessionId);

        // 8) Navegar para URL inicial se configurada
        var baseUrl = Environment.GetEnvironmentVariable("BASE_URL");
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            _driver.Navigate().GoToUrl(baseUrl);
            _recorder.RecordNavigate(new Uri(baseUrl).AbsolutePath);
            _logger.LogInformation("Navegou para: {Url}", baseUrl);
        }

        // 9) Exibir instruções
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  FREE-HANDS Recorder — Modo Exploratório                       ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  Interaja livremente com a aplicação no browser.               ║");
        Console.WriteLine("║                                                                ║");
        Console.WriteLine("║  Para encerrar e gerar session.json:                           ║");
        Console.WriteLine("║    • Feche o browser manualmente, OU                           ║");
        Console.WriteLine("║    • Pressione CTRL+C neste terminal                           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // 10) Aguardar encerramento (browser fechado ou CTRL+C)
        WaitForExit();

        // 11) Finalizar
        return Shutdown();
    }

    private static void WaitForExit()
    {
        // Polling para detectar se o browser foi fechado
        while (!_exitEvent.IsSet)
        {
            try
            {
                // Tenta acessar o driver — se browser foi fechado, lança exceção
                _ = _driver?.WindowHandles;
                Thread.Sleep(500);
            }
            catch (WebDriverException)
            {
                // Browser foi fechado pelo usuário
                _logger?.LogInformation("Browser fechado pelo usuário.");
                break;
            }
        }
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true; // Evita término abrupto
        _logger?.LogInformation("CTRL+C detectado. Encerrando...");
        _exitEvent.Set();
    }

    private static int Shutdown()
    {
        try
        {
            // 1) Parar recorder
            _recorder?.Stop();

            // 2) Escrever session.json
            if (_recorder != null && _settings != null)
            {
                var writer = new SessionWriter();
                var outputDir = _settings.RecordOutputDir;
                var path = writer.Write(_recorder.GetSession(), outputDir);
                _logger?.LogInformation("session.json gerado: {Path}", path);

                // Exibir conteúdo
                Console.WriteLine();
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.WriteLine("  session.json:");
                Console.WriteLine("═══════════════════════════════════════════════════════════════");
                Console.WriteLine(File.ReadAllText(path));
            }

            // 3) Fechar driver (se ainda aberto)
            try { _driver?.Quit(); } catch { }
            try { _driver?.Dispose(); } catch { }

            return 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro ao encerrar recorder.");
            return 1;
        }
    }
}
