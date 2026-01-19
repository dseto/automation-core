using System.CommandLine;
using Automation.Validator.Models;
using Automation.Validator.Services;
using Automation.Validator.Validators;

var rootCommand = new RootCommand("Automation.Validator - Validador de Contratos para Testes de UI");

// Comando: validate
var validateCommand = new Command("validate", "Valida UiMap, DataMap e Feature Files");

var uiMapOption = new Option<string>(
    new[] { "--ui-map", "-u" },
    "Caminho para o arquivo ui-map.yaml"
);

var dataMapOption = new Option<string>(
    new[] { "--data-map", "-d" },
    "Caminho para o arquivo data-map.yaml"
);

var featuresOption = new Option<string>(
    new[] { "--features", "-f" },
    "Caminho para o diretório de features (*.feature)"
);

var jsonOutputOption = new Option<bool>(
    new[] { "--json", "-j" },
    "Gerar saída em JSON"
);

validateCommand.AddOption(uiMapOption);
validateCommand.AddOption(dataMapOption);
validateCommand.AddOption(featuresOption);
validateCommand.AddOption(jsonOutputOption);

validateCommand.SetHandler(async (uiMapPath, dataMapPath, featuresPath, jsonOutput) =>
{
    await ValidateCommand(uiMapPath, dataMapPath, featuresPath, jsonOutput);
}, uiMapOption, dataMapOption, featuresOption, jsonOutputOption);

// Comando: doctor
var doctorCommand = new Command("doctor", "Diagnóstico de problemas comuns");

var projectPathOption = new Option<string>(
    new[] { "--path", "-p" },
    "Caminho do projeto (padrão: diretório atual)"
) { IsRequired = false };

doctorCommand.AddOption(projectPathOption);

doctorCommand.SetHandler(async (projectPath) =>
{
    await DoctorCommand(projectPath ?? ".");
}, projectPathOption);

// Comando: plan
var planCommand = new Command("plan", "Planejar implementação de automação");

var appUrlOption = new Option<string>(
    new[] { "--url", "-u" },
    "URL da aplicação para análise"
);

planCommand.AddOption(appUrlOption);

planCommand.SetHandler(async (appUrl) =>
{
    await PlanCommand(appUrl);
}, appUrlOption);

rootCommand.AddCommand(validateCommand);
rootCommand.AddCommand(doctorCommand);
rootCommand.AddCommand(planCommand);

await rootCommand.InvokeAsync(args);

// Implementação dos comandos

async Task ValidateCommand(string? uiMapPath, string? dataMapPath, string? featuresPath, bool jsonOutput)
{
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

        Environment.Exit(combinedResult.IsValid ? 0 : 1);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Erro: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
}

async Task DoctorCommand(string projectPath)
{
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
}

async Task PlanCommand(string appUrl)
{
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
}
