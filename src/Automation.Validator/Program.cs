using System.CommandLine;
using System.Text.RegularExpressions;
using Automation.Core.UiMap;
using Automation.Core.Resolution;

var root = new RootCommand("Automation Validator (doctor/validate/plan)");

var uiMapOpt = new Option<string>("--ui-map", description: "Path to ui-map.yaml") { IsRequired = false };
var featuresOpt = new Option<string>("--features", description: "Path to folder with .feature files") { IsRequired = false };

root.AddCommand(BuildDoctor());
root.AddCommand(BuildValidate(uiMapOpt, featuresOpt));
root.AddCommand(BuildPlan(uiMapOpt, featuresOpt));

return await root.InvokeAsync(args);

static Command BuildDoctor()
{
    var cmd = new Command("doctor", "Checks local prerequisites (Edge/driver/env vars).");
    cmd.SetHandler(() =>
    {
        try
        {
            Console.WriteLine("Doctor report:");
            Console.WriteLine($"- OS: {Environment.OSVersion}");
            Console.WriteLine($"- BASE_URL: {Environment.GetEnvironmentVariable("BASE_URL") ?? "(not set)"}");
            Console.WriteLine($"- UI_MAP_PATH: {Environment.GetEnvironmentVariable("UI_MAP_PATH") ?? "(not set)"}");
            Console.WriteLine();
            Console.WriteLine("NOTE: For E2E you need msedgedriver compatible with Edge available (PATH or Selenium Manager).");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Doctor error: {ex.Message}");
            Environment.Exit(3);
        }
    });
    return cmd;
}

static Command BuildValidate(Option<string> uiMapOpt, Option<string> featuresOpt)
{
    var cmd = new Command("validate", "Validates ui-map + features using MVP parser.");
    cmd.AddOption(uiMapOpt);
    cmd.AddOption(featuresOpt);

    cmd.SetHandler((string? uiMap, string? features) =>
    {
        try
        {
            var (mapPath, featuresPath) = Normalize(uiMap, features);
            var map = new UiMapLoader().LoadFromFile(mapPath);

            var report = Validator.Validate(map, featuresPath, explainPlan: false);
            report.PrintToConsole();

            Environment.Exit(report.HasErrors ? 2 : 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Validation error: {ex.Message}");
            Environment.Exit(3);
        }
    }, uiMapOpt, featuresOpt);

    return cmd;
}

static Command BuildPlan(Option<string> uiMapOpt, Option<string> featuresOpt)
{
    var cmd = new Command("plan", "Prints explain plan (resolution) for features.");
    cmd.AddOption(uiMapOpt);
    cmd.AddOption(featuresOpt);

    cmd.SetHandler((string? uiMap, string? features) =>
    {
        try
        {
            var (mapPath, featuresPath) = Normalize(uiMap, features);
            var map = new UiMapLoader().LoadFromFile(mapPath);

            var report = Validator.Validate(map, featuresPath, explainPlan: true);
            report.PrintToConsole();

            Environment.Exit(report.HasErrors ? 2 : 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Plan error: {ex.Message}");
            Environment.Exit(3);
        }
    }, uiMapOpt, featuresOpt);

    return cmd;
}

static (string mapPath, string featuresPath) Normalize(string? uiMap, string? features)
{
    var mapPath = uiMap ?? ".\\\\samples\\\\ui\\\\ui-map.yaml";
    var featuresPath = features ?? ".\\\\samples\\\\features";
    if (!File.Exists(mapPath)) throw new FileNotFoundException("ui-map not found", mapPath);
    if (!Directory.Exists(featuresPath)) throw new DirectoryNotFoundException($"features folder not found: {featuresPath}");
    return (mapPath, featuresPath);
}

static class Validator
{
    private static readonly Regex GivenPage   = new(@"^\s*(Dado|Given)\s+que\s+estou\s+na\s+tela\s+""(?<page>[^""]+)""", RegexOptions.IgnoreCase);
    private static readonly Regex WhenFill    = new(@"^\s*(Quando|When|E|And)\s+eu\s+preencho\s+o\s+campo\s+""(?<el>[^""]+)""\s+com\s+""(?<val>[^""]*)""", RegexOptions.IgnoreCase);
    private static readonly Regex WhenClick   = new(@"^\s*(Quando|When|E|And)\s+eu\s+clico\s+no\s+bot[aã]o\s+""(?<el>[^""]+)""", RegexOptions.IgnoreCase);
    private static readonly Regex ThenVisible = new(@"^\s*(Então|Then|E|And)\s+o\s+elemento\s+""(?<el>[^""]+)""\s+deve\s+estar\s+vis[ií]vel", RegexOptions.IgnoreCase);
    private static readonly Regex ThenRoute   = new(@"^\s*(Então|Then|E|And)\s+a\s+rota\s+deve\s+ser\s+""(?<route>[^""]+)""", RegexOptions.IgnoreCase);

    public static ValidationReport Validate(UiMapModel map, string featuresFolder, bool explainPlan)
    {
        var report = new ValidationReport();

        foreach (var file in Directory.EnumerateFiles(featuresFolder, "*.feature", SearchOption.AllDirectories))
        {
            var lines = File.ReadAllLines(file);
            string? currentPage = null;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var mPage = GivenPage.Match(line);
                if (mPage.Success)
                {
                    currentPage = mPage.Groups["page"].Value;
                    if (!map.Pages.ContainsKey(currentPage))
                        report.AddError(file, i + 1, $"Page '{currentPage}' not found in ui-map.");
                    else if (explainPlan)
                        report.AddInfo(file, i + 1, $"[PLAN] PageContext = {currentPage}");
                    continue;
                }

                string RequirePage()
                {
                    if (string.IsNullOrWhiteSpace(currentPage))
                    {
                        report.AddError(file, i + 1, "PageContext not set. Add: Dado que estou na tela \"PageName\"");
                        return "";
                    }
                    return currentPage;
                }

                var mFill = WhenFill.Match(line);
                if (mFill.Success)
                {
                    var page = RequirePage();
                    var el = mFill.Groups["el"].Value;
                    if (!string.IsNullOrWhiteSpace(page) && map.Pages.TryGetValue(page, out var p))
                    {
                        if (!p.Elements.ContainsKey(el))
                            report.AddError(file, i + 1, $"Element '{el}' not found in page '{page}'.");
                        else if (explainPlan)
                        {
                            var testId = p.Elements[el];
                            report.AddInfo(file, i + 1, $"[PLAN] {page}.{el} -> {testId} -> {LocatorFactory.CssByTestId(testId)}");
                        }
                    }
                    continue;
                }

                var mClick = WhenClick.Match(line);
                if (mClick.Success)
                {
                    var page = RequirePage();
                    var el = mClick.Groups["el"].Value;
                    if (!string.IsNullOrWhiteSpace(page) && map.Pages.TryGetValue(page, out var p))
                    {
                        if (!p.Elements.ContainsKey(el))
                            report.AddError(file, i + 1, $"Element '{el}' not found in page '{page}'.");
                        else if (explainPlan)
                        {
                            var testId = p.Elements[el];
                            report.AddInfo(file, i + 1, $"[PLAN] {page}.{el} -> {testId} -> {LocatorFactory.CssByTestId(testId)}");
                        }
                    }
                    continue;
                }

                var mVisible = ThenVisible.Match(line);
                if (mVisible.Success)
                {
                    var page = RequirePage();
                    var el = mVisible.Groups["el"].Value;
                    if (!string.IsNullOrWhiteSpace(page) && map.Pages.TryGetValue(page, out var p))
                    {
                        if (!p.Elements.ContainsKey(el))
                            report.AddError(file, i + 1, $"Element '{el}' not found in page '{page}'.");
                        else if (explainPlan)
                        {
                            var testId = p.Elements[el];
                            report.AddInfo(file, i + 1, $"[PLAN] {page}.{el} -> {testId} -> {LocatorFactory.CssByTestId(testId)}");
                        }
                    }
                    continue;
                }

                var mRoute = ThenRoute.Match(line);
                if (mRoute.Success)
                {
                    if (explainPlan)
                        report.AddInfo(file, i + 1, $"[PLAN] Expect route contains: {mRoute.Groups["route"].Value}");
                    continue;
                }

                if (line.StartsWith("Dado", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("Quando", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("Então", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("E", StringComparison.OrdinalIgnoreCase))
                {
                    report.AddError(file, i + 1, $"Unsupported step (MVP): {line}");
                }
            }
        }

        return report;
    }
}

sealed class ValidationReport
{
    private readonly List<ValidationItem> _items = new();

    public bool HasErrors => _items.Any(i => i.Level == "ERROR");

    public void AddError(string file, int line, string message) => _items.Add(new("ERROR", file, line, message));
    public void AddInfo(string file, int line, string message) => _items.Add(new("INFO", file, line, message));

    public void PrintToConsole()
    {
        foreach (var i in _items)
            Console.WriteLine($"{i.Level} {i.File}:{i.Line} - {i.Message}");

        if (!_items.Any())
            Console.WriteLine("OK (no findings).");
    }
}

sealed record ValidationItem(string Level, string File, int Line, string Message);
