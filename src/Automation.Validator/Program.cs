using Automation.Validator.Models;
using Automation.Validator.Services;
using Automation.Validator.Validators;

// Parse command line arguments
var commandArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();

if (commandArgs.Length == 0 || commandArgs[0] == "help" || commandArgs[0] == "--help" || commandArgs[0] == "-h")
{
    PrintHelp();
    return 0;
}

var command = commandArgs[0];

try
{
    return command switch
    {
        "validate" => await ValidateCommand(commandArgs.Skip(1).ToArray()),
        "doctor" => await DoctorCommand(commandArgs.Skip(1).ToArray()),
        "plan" => await PlanCommand(commandArgs.Skip(1).ToArray()),
        _ => HandleUnknownCommand(command)
    };
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ Erro: {ex.Message}");
    Console.ResetColor();
    return 1;
}

// Implementação dos comandos

async Task<int> ValidateCommand(string[] cmdArgs)
{
    string? uiMapPath = null;
    string? dataMapPath = null;
    string? featuresPath = null;
    bool jsonOutput = false;

    // Parse arguments
    for (int i = 0; i < cmdArgs.Length; i++)
    {
        if ((cmdArgs[i] == "--ui-map" || cmdArgs[i] == "-u") && i + 1 < cmdArgs.Length)
            uiMapPath = cmdArgs[++i];
        else if ((cmdArgs[i] == "--data-map" || cmdArgs[i] == "-d") && i + 1 < cmdArgs.Length)
            dataMapPath = cmdArgs[++i];
        else if ((cmdArgs[i] == "--features" || cmdArgs[i] == "-f") && i + 1 < cmdArgs.Length)
            featuresPath = cmdArgs[++i];
        else if (cmdArgs[i] == "--json" || cmdArgs[i] == "-j")
            jsonOutput = true;
    }

    try
    {
        var loader = new YamlLoader();
        var reportService = new ReportService();
        var combinedResult = ValidationResult.Success();

        // Validar UiMap
        if (!string.IsNullOrEmpty(uiMapPath))
        {
            var uiMap = loader.LoadUiMap(uiMapPath);
            var uiValidator = new UiMapValidator();
            var uiResult = uiValidator.Validate(uiMap, uiMapPath);
            
            foreach (var error in uiResult.Errors)
                combinedResult.AddError(error);
            foreach (var warning in uiResult.Warnings)
                combinedResult.AddWarning(warning);
        }

        // Validar DataMap
        if (!string.IsNullOrEmpty(dataMapPath))
        {
            var dataMap = loader.LoadDataMap(dataMapPath);
            var dataValidator = new DataMapValidator();
            var dataResult = dataValidator.Validate(dataMap, dataMapPath);
            
            foreach (var error in dataResult.Errors)
                combinedResult.AddError(error);
            foreach (var warning in dataResult.Warnings)
                combinedResult.AddWarning(warning);
        }

        // Validar Features
        if (!string.IsNullOrEmpty(featuresPath) && !string.IsNullOrEmpty(uiMapPath) && !string.IsNullOrEmpty(dataMapPath))
        {
            var uiMap = loader.LoadUiMap(uiMapPath);
            var dataMap = loader.LoadDataMap(dataMapPath);
            var gherkinValidator = new GherkinValidator();

            var featureFiles = Directory.GetFiles(featuresPath, "*.feature", SearchOption.AllDirectories);
            foreach (var featureFile in featureFiles)
            {
                var gherkinContent = loader.LoadGherkin(featureFile);
                var gherkinResult = gherkinValidator.Validate(gherkinContent, uiMap, dataMap, featureFile);
                
                foreach (var error in gherkinResult.Errors)
                    combinedResult.AddError(error);
                foreach (var warning in gherkinResult.Warnings)
                    combinedResult.AddWarning(warning);
            }
        }

        if (jsonOutput)
        {
            var json = reportService.GenerateJsonReport(combinedResult);
            Console.WriteLine(json);
        }
        else
        {
            reportService.PrintConsoleReport(combinedResult, "VALIDAÇÃO DE CONTRATOS");
        }

        return combinedResult.IsValid ? 0 : 1;
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Erro na validação: {ex.Message}");
        Console.ResetColor();
        return 1;
    }
}

async Task<int> DoctorCommand(string[] cmdArgs)
{
    string projectPath = ".";

    // Parse arguments
    for (int i = 0; i < cmdArgs.Length; i++)
    {
        if ((cmdArgs[i] == "--path" || cmdArgs[i] == "-p") && i + 1 < cmdArgs.Length)
            projectPath = cmdArgs[++i];
    }

    Console.WriteLine("\n🔍 Executando diagnóstico...\n");

    var checks = new List<(string name, bool passed, string message)>();

    // Verificar estrutura de diretórios
    var hasFeatures = Directory.Exists(Path.Combine(projectPath, "features"));
    checks.Add(("Diretório 'features/' existe", hasFeatures, hasFeatures ? "✓" : "✗ Crie o diretório 'features/'"));

    var hasUiMap = File.Exists(Path.Combine(projectPath, "ui", "ui-map.yaml"));
    checks.Add(("Arquivo 'ui/ui-map.yaml' existe", hasUiMap, hasUiMap ? "✓" : "✗ Crie o arquivo 'ui/ui-map.yaml'"));

    var hasDataMap = File.Exists(Path.Combine(projectPath, "data", "data-map.yaml"));
    checks.Add(("Arquivo 'data/data-map.yaml' existe", hasDataMap, hasDataMap ? "✓" : "✗ Crie o arquivo 'data/data-map.yaml'"));

    var hasReqnroll = File.Exists(Path.Combine(projectPath, "reqnroll.json"));
    checks.Add(("Arquivo 'reqnroll.json' existe", hasReqnroll, hasReqnroll ? "✓" : "✗ Crie o arquivo 'reqnroll.json'"));

    var hasCsproj = Directory.GetFiles(projectPath, "*.csproj").Any();
    checks.Add(("Arquivo '.csproj' existe", hasCsproj, hasCsproj ? "✓" : "✗ Crie um projeto .NET"));

    foreach (var (name, passed, message) in checks)
    {
        var color = passed ? ConsoleColor.Green : ConsoleColor.Red;
        Console.ForegroundColor = color;
        Console.WriteLine($"{message} {name}");
        Console.ResetColor();
    }

    var allPassed = checks.All(c => c.passed);
    Console.WriteLine($"\n{(allPassed ? "✓ Projeto pronto!" : "✗ Corrija os problemas acima")}");

    return allPassed ? 0 : 1;
}

async Task<int> PlanCommand(string[] cmdArgs)
{
    string appUrl = "";

    // Parse arguments
    for (int i = 0; i < cmdArgs.Length; i++)
    {
        if ((cmdArgs[i] == "--url" || cmdArgs[i] == "-u") && i + 1 < cmdArgs.Length)
            appUrl = cmdArgs[++i];
    }

    Console.WriteLine($"\n📋 Plano de Implementação para {appUrl}\n");
    Console.WriteLine("Passos recomendados:");
    Console.WriteLine("1. Mapear todas as páginas da aplicação");
    Console.WriteLine("2. Identificar elementos interativos (inputs, buttons, etc.)");
    Console.WriteLine("3. Criar ui-map.yaml com páginas e elementos");
    Console.WriteLine("4. Definir dados de teste em data-map.yaml");
    Console.WriteLine("5. Escrever cenários em Gherkin (features/)");
    Console.WriteLine("6. Executar 'automation-validator validate' para validar");
    Console.WriteLine("7. Rodar testes com 'dotnet test'");
    Console.WriteLine();

    return 0;
}

int HandleUnknownCommand(string command)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"⚠️  Comando desconhecido: '{command}'");
    Console.ResetColor();
    PrintHelp();
    return 1;
}

void PrintHelp()
{
    Console.WriteLine("\n🤖 Automation.Validator - Validador de Contratos para Testes de UI\n");
    Console.WriteLine("Uso: automation-validator <comando> [opções]\n");
    Console.WriteLine("Comandos:");
    Console.WriteLine("  validate    Valida UiMap, DataMap e Feature Files");
    Console.WriteLine("  doctor      Diagnóstico de problemas comuns");
    Console.WriteLine("  plan        Planejar implementação de automação");
    Console.WriteLine("  help        Exibe esta mensagem de ajuda\n");
    Console.WriteLine("Exemplos:");
    Console.WriteLine("  automation-validator validate --ui-map ui-map.yaml --data-map data-map.yaml --features features/");
    Console.WriteLine("  automation-validator validate -u ui-map.yaml -d data-map.yaml -f features/ --json");
    Console.WriteLine("  automation-validator doctor --path .");
    Console.WriteLine("  automation-validator plan --url https://app.example.com\n");
}
