using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Automation.Core.Configuration;
using Automation.Core.Driver;
using Automation.Core.Recorder;
using Automation.Core.Recorder.Draft;
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
      if (args.Length > 0 && args[0].Equals("generate-draft", StringComparison.OrdinalIgnoreCase))
        return RunGenerateDraft(args.Skip(1).ToArray());

      if (args.Length > 0 && args[0].Equals("resolve-draft", StringComparison.OrdinalIgnoreCase))
        return RunResolveDraft(args.Skip(1).ToArray());

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
        
        var browserType = (_settings?.Browser ?? "chrome").ToLowerInvariant();
        _driver = browserType switch
        {
          "edge" => EdgeDriverFactory.Create(_settings),
          _ => ChromeDriverFactory.Create(_settings)
        };

        // 7) Iniciar recorder
        _recorder = new SessionRecorder(_settings?.RecordWaitLogThresholdSeconds ?? 1.0);
        _recorder.Start();
        _logger.LogInformation("Recorder iniciado. SessionId: {SessionId}", _recorder.GetSession().SessionId);

        // 8) Navegar para URL inicial se configurada
        var baseUrl = Environment.GetEnvironmentVariable("BASE_URL");
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            _driver.Navigate().GoToUrl(baseUrl);
            var ab = new Uri(baseUrl);
            // Normalize the initial route using RouteNormalizer so that baseUrl itself maps to '/'
            var canonical = Automation.Core.Recorder.RouteNormalizer.Normalize(ab.AbsoluteUri, ab.AbsolutePath, string.IsNullOrEmpty(ab.Fragment) ? null : ab.Fragment, baseUrl);
            _recorder.RecordNavigate(canonical, ab.AbsoluteUri, ab.AbsolutePath, string.IsNullOrEmpty(ab.Fragment) ? null : ab.Fragment);
            _logger.LogInformation("Navegou para: {Url}", baseUrl);
            
            // 8.1) Injetar script de captura de browser (RC01)
            InjectBrowserCaptureScript();
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

    private static int RunGenerateDraft(string[] args)
    {
      var input = GetArgValue(args, "--input");
      var output = GetArgValue(args, "--output");
      var scenario = GetArgValue(args, "--scenario") ?? GetArgValue(args, "--name");

      if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(output))
      {
        Console.WriteLine("Uso: generate-draft --input <session.json> --output <output-dir> [--scenario <name>]");
        return 1;
      }

      try
      {
        var reader = new SessionReader();
        var session = reader.Read(input);

        var generator = new DraftGenerator(
          new SessionSanityChecker(),
          new ActionGrouper(),
          new StepInferenceEngine(),
          new EscapeHatchRenderer(),
          new DraftWriter());

        var result = generator.Generate(session, output, scenario);
        if (!result.IsSuccess)
        {
          Console.WriteLine("[DraftGenerator] Sessão inválida. Draft não gerado.");
          if (!string.IsNullOrWhiteSpace(result.Warning))
            Console.WriteLine($"[DraftGenerator] Aviso: {result.Warning}");

          if (!string.IsNullOrWhiteSpace(result.MetadataPath))
            Console.WriteLine($"[DraftGenerator] Metadata: {result.MetadataPath}");

          return 1;
        }

        Console.WriteLine($"[DraftGenerator] draft.feature: {result.DraftPath}");
        Console.WriteLine($"[DraftGenerator] draft.metadata.json: {result.MetadataPath}");
        return 0;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[DraftGenerator] Erro: {ex.Message}");
        return 1;
      }
    }

    private static int RunResolveDraft(string[] args)
    {
      string? draftPath = null;
      string? metadataPath = null;
      string? uimapPath = null;
      string? sessionPath = null;
      string? output = null;

      for (int i = 0; i < args.Length; i++)
      {
        if ((args[i] == "--draft") && i + 1 < args.Length) draftPath = args[++i];
        else if ((args[i] == "--metadata") && i + 1 < args.Length) metadataPath = args[++i];
        else if ((args[i] == "--ui-map" || args[i] == "--uimap") && i + 1 < args.Length) uimapPath = args[++i];
        else if ((args[i] == "--session") && i + 1 < args.Length) sessionPath = args[++i];
        else if ((args[i] == "--output") && i + 1 < args.Length) output = args[++i];
      }

      // SEMRES feature toggle: if SEMRES_ENABLED is explicitly set and not 'true' or '1', disable the resolution run
      var semresFlag = Environment.GetEnvironmentVariable("SEMRES_ENABLED");
      if (!string.IsNullOrWhiteSpace(semresFlag) && !(semresFlag.Equals("true", StringComparison.OrdinalIgnoreCase) || semresFlag == "1"))
      {
        Console.WriteLine($"[ResolveDraft] SEMRES_ENABLED is set to '{semresFlag}'. Semantic resolution disabled.");
        return 1;
      }

      if (string.IsNullOrWhiteSpace(draftPath) || string.IsNullOrWhiteSpace(metadataPath))
      {
        Console.WriteLine("Uso: resolve-draft --draft <draft.feature> --metadata <draft.metadata.json> [--session <session.json>] [--ui-map <ui-map.yaml>] [--output <dir>]");
        return 1;
      }

      try
      {
        var draftContent = File.ReadAllText(draftPath);
        var metadataJson = File.ReadAllText(metadataPath);
        var draftMetadata = System.Text.Json.JsonSerializer.Deserialize<Automation.Core.Recorder.Draft.DraftMetadata>(metadataJson);

        if (draftMetadata == null) throw new Exception("draft.metadata.json inválido");

        // Load UiMap using YamlLoader from Automation.Validator
        var loader = new Automation.Validator.Services.YamlLoader();
        var validatorUiMap = loader.LoadUiMap(uimapPath ?? System.Environment.GetEnvironmentVariable("UI_MAP_PATH") ?? "specs/frontend/uimap.yaml");
        var uiMap = ConvertValidatorUiMapToCore(validatorUiMap);

        Automation.Core.Recorder.RecorderSession? session = null;
        if (!string.IsNullOrWhiteSpace(sessionPath))
        {
          var sr = new SessionReader();
          session = sr.Read(sessionPath);
        }

        var resolver = new Automation.Core.Recorder.Semantic.SemanticResolver(uiMap, session, System.Environment.GetEnvironmentVariable("SEMRES_MAX_CANDIDATES") is string s && int.TryParse(s, out var mi) ? mi : 5, draftPath, uimapPath ?? "specs/frontend/uimap.yaml");
        var writer = new Automation.Core.Recorder.Semantic.SemanticWriter();

        // Optional debug: if SEMRES_DEBUG is set to 'true', emit mapping diagnostics to help trace data-testid extraction
        var semresDebug = (Environment.GetEnvironmentVariable("SEMRES_DEBUG") ?? "").Equals("true", StringComparison.OrdinalIgnoreCase);
        if (semresDebug && session != null && draftMetadata != null)
        {
            Console.WriteLine("[ResolveDraft][DEBUG] Mapping eventIndex -> extractedTestId -> candidatesCount");
            var getTestIdMi = typeof(Automation.Core.Recorder.Semantic.SemanticResolver).GetMethod("TryGetTestIdFromSession", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var buildIndexMi = typeof(Automation.Core.Recorder.Semantic.SemanticResolver).GetMethod("BuildTestIdIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
                var idx = (System.Collections.IDictionary)buildIndexMi.Invoke(null, new object[] { uiMap })!;
            foreach (var m in draftMetadata.Mappings)
            {
                var evIdx = m.EventIndex;
                var extracted = (string?)getTestIdMi.Invoke(resolver, new object[] { evIdx });
                var candidatesCount = 0;
                if (!string.IsNullOrWhiteSpace(extracted))
                {
                    if (idx.Contains(extracted)) candidatesCount = ((System.Collections.IList)idx[extracted]).Count;
                }
                Console.WriteLine($"[ResolveDraft][DEBUG] eventIndex={evIdx}, draftLine={m.DraftLine}, extracted='{extracted}', candidates={candidatesCount}");
            }
        }

        var (resolvedMeta, report, resolvedFeature) = resolver.Resolve(draftContent, draftMetadata);

        var outDir = output ?? System.Environment.GetEnvironmentVariable("SEMRES_OUTPUT_DIR") ?? "artifacts/semantic-resolution";
        Directory.CreateDirectory(outDir);

        var rf = writer.WriteResolvedFeature(resolvedFeature, outDir);
        var rm = writer.WriteResolvedMetadata(resolvedMeta, outDir);
        var rg = writer.WriteUiGapsReport(report, outDir);

        Console.WriteLine($"[ResolveDraft] resolved.feature: {rf}");
        Console.WriteLine($"[ResolveDraft] resolved.metadata.json: {rm}");
        Console.WriteLine($"[ResolveDraft] ui-gaps.report.json: {rg}");

        return 0;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[ResolveDraft] Erro: {ex.Message}");
        return 1;
      }
    }

    private static Automation.Core.UiMap.UiMapModel ConvertValidatorUiMapToCore(Automation.Validator.Models.UiMapModel vmap)
    {
      var core = new Automation.Core.UiMap.UiMapModel();
      foreach (var (pageName, page) in vmap.Pages)
      {
        var pageDict = new System.Collections.Generic.Dictionary<string, object>();
        var meta = new System.Collections.Generic.Dictionary<string, object>();
        if (!string.IsNullOrWhiteSpace(page.Route)) meta["route"] = page.Route;
        if (!string.IsNullOrWhiteSpace(page.Anchor)) meta["anchor"] = page.Anchor;
        if (meta.Count > 0) pageDict["__meta"] = meta;

        foreach (var (elementName, element) in page.Elements)
        {
          var elementDict = new System.Collections.Generic.Dictionary<string, object>();
          if (!string.IsNullOrWhiteSpace(element.TestId)) elementDict["testId"] = element.TestId;
          pageDict[elementName] = elementDict;
        }

        core.Pages[pageName] = pageDict;
      }
      return core;
    }

    private static string? GetArgValue(string[] args, string key)
    {
      for (var i = 0; i < args.Length - 1; i++)
      {
        if (args[i].Equals(key, StringComparison.OrdinalIgnoreCase))
          return args[i + 1];
      }

      return null;
    }

    private static void WaitForExit()
    {
        // Polling para detectar se o browser foi fechado E drenar eventos de browser capture
        while (!_exitEvent.IsSet)
        {
            try
            {
                // Tenta acessar o driver — se browser foi fechado, lança exceção
                _ = _driver?.WindowHandles;
                
                // RC06: Drenar eventos do buffer JS
                DrainBrowserEvents();
                
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

    /// <summary>
    /// RC01: Injeta script JS no browser que captura eventos DOM (click, fill, submit, navigate)
    /// Baseado em specs/frontend/notes/free-hands-recorder.injected-script.js
    /// </summary>
    private static void InjectBrowserCaptureScript()
    {
        if (_driver == null) return;

        // Embed do script de captura (70 linhas ref implementation)
        const string injectedScript = """
(function() {
  if (window.__fhRecorder) return; // Já injetado

  const buffer = [];
  const fillTimers = new WeakMap();
  const startTime = Date.now();

  function shortText(el) {
    const text = (el.textContent || '').trim();
    return text.length > 80 ? text.substring(0, 77) + '...' : text;
  }

  function attrs(el) {
    const result = {};
    if (el.id) result.id = el.id;
    if (el.name) result.name = el.name;
    if (el.type) result.type = el.type;
    if (el.getAttribute) {
      ['role', 'aria-label', 'data-testid', 'formcontrolname', 'placeholder'].forEach(attr => {
        const val = el.getAttribute(attr);
        if (val) result[attr] = val;
      });
    }
    return result;
  }

  function isDynamicId(id) {
    return /^mat-input-/.test(id) || /^mat-option-/.test(id) || /^cdk-/.test(id);
  }

  function cssPath(el) {
    if (el.getAttribute && el.getAttribute('data-testid')) {
      return "[data-testid='" + el.getAttribute('data-testid') + "']";
    }
    if (el.id && !isDynamicId(el.id)) return '#' + el.id;
    if (el.name) return "[name='" + el.name + "']";
    return el.tagName.toLowerCase();
  }

  function push(kind, targetEl, value) {
    buffer.push({
      kind: kind,
      ts: Date.now() - startTime,
      target: {
        tag: targetEl.tagName.toLowerCase(),
        text: shortText(targetEl),
        css: cssPath(targetEl),
        attributes: attrs(targetEl)
      },
      value: value || {}
    });
  }

  // RC05: Hook History API (SPA navigation)
  const origPushState = history.pushState;
  const origReplaceState = history.replaceState;
  history.pushState = function() {
    origPushState.apply(this, arguments);
    push('navigate', document.body, { literal: arguments[2] || location.pathname, url: location.href, pathname: location.pathname, fragment: (location.hash || null) });
  };
  history.replaceState = function() {
    origReplaceState.apply(this, arguments);
    push('navigate', document.body, { literal: arguments[2] || location.pathname, url: location.href, pathname: location.pathname, fragment: (location.hash || null) });
  };
  window.addEventListener('popstate', () => {
    push('navigate', document.body, { literal: location.pathname, url: location.href, pathname: location.pathname, fragment: (location.hash || null) });
  });

  // RC02: Capture clicks (bubbling phase)
  document.addEventListener('click', (e) => {
    const target = e.target.closest('button, a, [role="button"], input[type="button"], input[type="submit"]') || e.target;
    push('click', target);
  }, true);

  // RC03: Capture fills (input/change with debounce)
  document.addEventListener('input', (e) => {
    if (!e.target.matches('input, textarea')) return;
    const el = e.target;
    if (fillTimers.has(el)) clearTimeout(fillTimers.get(el));
    fillTimers.set(el, setTimeout(() => {
      push('fill', el, { literal: el.value });
      fillTimers.delete(el);
    }, 400));
  }, true);

  document.addEventListener('change', (e) => {
    if (!e.target.matches('select, input[type="checkbox"], input[type="radio"]')) return;
    push('fill', e.target, { literal: e.target.value || (e.target.checked ? 'checked' : 'unchecked') });
  }, true);

  // RC04: Capture submit
  document.addEventListener('submit', (e) => {
    push('submit', e.target);
  }, true);

  // RC06: Expose drain API
  window.__fhRecorder = {
    version: 'mvp-1',
    drain: () => buffer.splice(0, buffer.length)
  };

  // Emit initial navigate event for current document (helps capture full navigations)
  try { push('navigate', document.body, { literal: location.pathname + (location.hash || ''), url: location.href, pathname: location.pathname, fragment: (location.hash || null) }); } catch(e) { }
})();
""";

        try
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript(injectedScript);
            _logger?.LogInformation("[BrowserCapture] Script injetado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "[BrowserCapture] Falha ao injetar script. Captura de eventos pode não funcionar.");
        }
    }

    /// <summary>
    /// RC06: Polling - drena eventos do buffer JS e converte para SessionRecorder
    /// </summary>
    private static void DrainBrowserEvents()
    {
        if (_driver == null || _recorder == null) return;

        try
        {
            // Garantir que o script de captura esteja presente na página atual (tratamento de full navigations)
            var hasRecorderObj = ((IJavaScriptExecutor)_driver).ExecuteScript("return typeof window.__fhRecorder !== 'undefined';");
            if (!(hasRecorderObj is bool has && has))
            {
                _logger?.LogInformation("[BrowserCapture] __fhRecorder não encontrado. Tentando injetar script...");
                InjectBrowserCaptureScript();
                // Pequeno delay para o script ser executado no contexto da página
                Thread.Sleep(100);
            }

            var result = ((IJavaScriptExecutor)_driver).ExecuteScript(
                "return window.__fhRecorder?.drain?.() ?? [];"
            );

            if (result is not System.Collections.IEnumerable enumerable) return;

            var events = enumerable.Cast<object>().ToList();
            if (events.Count == 0) return;

            // Converter eventos JS para SessionRecorder
            foreach (var ev in events)
            {
                if (ev is not Dictionary<string, object> dict) continue;

                var kind = dict.GetValueOrDefault("kind")?.ToString();
                var target = dict.GetValueOrDefault("target") as Dictionary<string, object>;
                var value = dict.GetValueOrDefault("value") as Dictionary<string, object>;

                if (target == null) continue;

                var hint = TargetHintBuilder.BuildHint(target);

                // Mapear kind para SessionRecorder (conforme specs/backend/implementation/free-hands-recorder-browser-capture.md)
                switch (kind)
                {
                    case "navigate":
                        var rawRoute = value?.GetValueOrDefault("literal")?.ToString() ?? "/";
                        var url = value?.GetValueOrDefault("url")?.ToString();
                        var pathname = value?.GetValueOrDefault("pathname")?.ToString();
                        var fragment = value?.GetValueOrDefault("fragment")?.ToString();

                        // Normalize route to canonical relative form (pathname + fragment, prefer .html tail)
                        var baseUrlEnv = Environment.GetEnvironmentVariable("BASE_URL");
                        var canonical = Automation.Core.Recorder.RouteNormalizer.Normalize(url ?? rawRoute, pathname, fragment, baseUrlEnv);

                        _recorder.RecordNavigate(canonical, url, pathname, fragment);
                        break;

                    case "click":
                        _recorder.RecordClick(hint);
                        break;

                    case "fill":
                        var literal = value?.GetValueOrDefault("literal")?.ToString() ?? "";
                        // TODO: Mascarar password fields (defer para próxima entrega)
                        _recorder.RecordFill(hint, literal);
                        break;

                    case "submit":
                        _recorder.RecordSubmit(hint);
                        break;
                }
            }
        }
        catch (WebDriverException)
        {
            // Browser pode ter sido fechado durante polling — ignora
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "[BrowserCapture] Erro ao drenar eventos.");
        }
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
