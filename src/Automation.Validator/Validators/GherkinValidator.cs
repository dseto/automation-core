using Automation.Validator.Models;
using System.Text.RegularExpressions;

namespace Automation.Validator.Validators;

/// <summary>
/// Valida Gherkin contra UiMap e DataMap (steps conhecidos, páginas existentes).
/// </summary>
public class GherkinValidator
{
    private static readonly string[] KnownSteps = new[]
    {
        "Dado que a aplicação está em",
        "Dado que estou na página",
        "Quando eu aguardo a rota",
        "Quando eu preencho",
        "Quando eu clico em",
        "Quando eu limpo o campo",
        "Quando eu seleciono",
        "Quando eu executo o script",
        "Então estou na página",
        "Então a rota deve ser",
        "Então o elemento",
        "Então o atributo"
    };

    public ValidationResult Validate(
        string gherkinContent,
        UiMapModel uiMap,
        DataMapModel dataMap,
        string filePath)
    {
        var result = ValidationResult.Success();
        var lines = gherkinContent.Split('\n');

        var currentPage = "";
        var lineNumber = 0;

        foreach (var line in lines)
        {
            lineNumber++;
            var trimmed = line.Trim();

            // Ignorar linhas vazias e comentários
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                continue;

            // Extrair página de "Dado que estou na página"
            if (trimmed.Contains("Dado que estou na página"))
            {
                var match = Regex.Match(trimmed, @"Dado que estou na página ""([^""]+)""");
                if (match.Success)
                {
                    currentPage = match.Groups[1].Value;
                    ValidatePage(currentPage, uiMap, filePath, lineNumber, result);
                }
            }

            // Validar step conhecido
            if (IsStepLine(trimmed))
            {
                ValidateStep(trimmed, uiMap, dataMap, currentPage, filePath, lineNumber, result);
            }
        }

        return result;
    }

    private bool IsStepLine(string line)
    {
        var stepKeywords = new[] { "Dado", "Quando", "Então", "E", "Mas" };
        return stepKeywords.Any(kw => line.StartsWith(kw + " "));
    }

    private void ValidatePage(
        string pageName,
        UiMapModel uiMap,
        string filePath,
        int lineNumber,
        ValidationResult result)
    {
        if (!uiMap.Pages.ContainsKey(pageName))
        {
            result.AddError(new ValidationError(
                "GHERKIN_PAGE_NOT_FOUND",
                $"Página '{pageName}' não encontrada no UiMap.",
                filePath,
                lineNumber
            ));
        }
    }

    private void ValidateStep(
        string stepLine,
        UiMapModel uiMap,
        DataMapModel dataMap,
        string currentPage,
        string filePath,
        int lineNumber,
        ValidationResult result)
    {
        // Validar que o step é conhecido
        var stepKnown = KnownSteps.Any(ks => stepLine.Contains(ks));
        if (!stepKnown)
        {
            result.AddWarning(new ValidationWarning(
                "GHERKIN_UNKNOWN_STEP",
                $"Step desconhecido: '{stepLine}'. Considere usar Escape Hatch ou propor novo step.",
                filePath
            ));
            return;
        }

        // Extrair referências de elemento (entre aspas)
        var elementMatches = Regex.Matches(stepLine, @"""([^""]+)""");
        foreach (Match match in elementMatches)
        {
            var reference = match.Groups[1].Value;

            // Validar elemento se estamos em uma página
            if (!string.IsNullOrEmpty(currentPage) && uiMap.Pages.TryGetValue(currentPage, out var page))
            {
                if (!page.Elements.ContainsKey(reference))
                {
                    result.AddError(new ValidationError(
                        "GHERKIN_ELEMENT_NOT_FOUND",
                        $"Elemento '{reference}' não encontrado na página '{currentPage}'.",
                        filePath,
                        lineNumber
                    ));
                }
            }

            // Validar dados se for referência de dados
            if (reference.Contains("_") && !reference.StartsWith("/"))
            {
                ValidateDataReference(reference, dataMap, filePath, lineNumber, result);
            }
        }
    }

    private void ValidateDataReference(
        string dataKey,
        DataMapModel dataMap,
        string filePath,
        int lineNumber,
        ValidationResult result)
    {
        // Verificar se é um token de dataset
        if (dataKey.StartsWith("{{") && dataKey.EndsWith("}}"))
        {
            var datasetName = dataKey.Trim('{', '}');
            if (!dataMap.Datasets.ContainsKey(datasetName))
            {
                result.AddError(new ValidationError(
                    "GHERKIN_DATASET_NOT_FOUND",
                    $"Dataset '{datasetName}' não encontrado no DataMap.",
                    filePath,
                    lineNumber
                ));
            }
        }
        else
        {
            // Verificar se é uma chave de contexto
            var defaultContext = dataMap.Contexts.TryGetValue("default", out var ctx) 
                ? ctx as IDictionary<object, object> 
                : null;
            
            if (defaultContext != null && !defaultContext.ContainsKey(dataKey))
            {
                result.AddWarning(new ValidationWarning(
                    "GHERKIN_DATA_KEY_NOT_FOUND",
                    $"Chave de dados '{dataKey}' não encontrada no contexto 'default'.",
                    filePath
                ));
            }
        }
    }
}
