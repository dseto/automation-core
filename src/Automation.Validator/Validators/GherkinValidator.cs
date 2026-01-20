using Automation.Validator.Models;
using System.Text.RegularExpressions;

namespace Automation.Validator.Validators;

/// <summary>
/// Valida Gherkin contra UiMap e DataMap (steps conhecidos, páginas existentes).
/// </summary>
public class GherkinValidator
{
    // Padrões de steps conhecidos (regex patterns)
    private static readonly string[] KnownStepPatterns = new[]
    {
        // Navigation Steps
        @"que a aplicação está em",
        @"que estou na página",
        @"estou na página",
        @"a rota deve ser",
        @"eu aguardo a rota",
        @"eu aguardo \d+ segundos?",
        
        // Interaction Steps
        @"eu preencho ""[^""]+"" com",
        @"eu preencho os campos com os dados de",
        @"eu clico em ""[^""]+""",
        @"eu clico em ""[^""]+"" e aguardo a rota",
        @"eu limpo o campo",
        @"eu seleciono",
        @"eu executo o script",
        
        // Validation Steps
        @"o elemento ""[^""]+"" deve estar visível",
        @"o elemento ""[^""]+"" não deve estar visível",
        @"o atributo ""[^""]+"" de ""[^""]+"" deve ser",
        @"o texto de ""[^""]+"" deve ser",
        @"o texto de ""[^""]+"" deve conter"
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

            // Ignorar linhas de estrutura Gherkin
            if (IsStructureLine(trimmed))
                continue;

            // Extrair página de "Dado que estou na página" ou "Então estou na página"
            var pageMatch = Regex.Match(trimmed, @"(?:que estou na página|estou na página) ""([^""]+)""");
            if (pageMatch.Success)
            {
                currentPage = pageMatch.Groups[1].Value;
                ValidatePage(currentPage, uiMap, filePath, lineNumber, result);
                continue;
            }

            // Validar step conhecido
            if (IsStepLine(trimmed))
            {
                ValidateStep(trimmed, uiMap, dataMap, currentPage, filePath, lineNumber, result);
            }
        }

        return result;
    }

    private bool IsStructureLine(string line)
    {
        var structureKeywords = new[] { 
            "Funcionalidade:", "Feature:", 
            "Cenário:", "Scenario:", 
            "Contexto:", "Background:",
            "Esquema do Cenário:", "Scenario Outline:",
            "Exemplos:", "Examples:",
            "@" // Tags
        };
        return structureKeywords.Any(kw => line.StartsWith(kw));
    }

    private bool IsStepLine(string line)
    {
        var stepKeywords = new[] { "Dado", "Quando", "Então", "E", "Mas", "Given", "When", "Then", "And", "But" };
        return stepKeywords.Any(kw => line.StartsWith(kw + " "));
    }

    private bool IsKnownStep(string stepLine)
    {
        return KnownStepPatterns.Any(pattern => Regex.IsMatch(stepLine, pattern, RegexOptions.IgnoreCase));
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
        if (!IsKnownStep(stepLine))
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
        var references = elementMatches.Select(m => m.Groups[1].Value).ToList();
        
        // Para steps de preenchimento, o primeiro parâmetro é o elemento, o segundo é o valor
        // Para steps de clique, o parâmetro é o elemento
        // Para steps de validação de atributo, primeiro é atributo, segundo é elemento, terceiro é valor
        
        foreach (var reference in references)
        {
            // Ignorar referências especiais
            if (IsSpecialReference(reference))
                continue;

            // Validar referências de dados
            ValidateDataReference(reference, dataMap, filePath, lineNumber, result);
        }
        
        // Validar elementos específicos baseado no tipo de step
        ValidateElementsInStep(stepLine, references, uiMap, currentPage, filePath, lineNumber, result);
    }

    private void ValidateElementsInStep(
        string stepLine,
        List<string> references,
        UiMapModel uiMap,
        string currentPage,
        string filePath,
        int lineNumber,
        ValidationResult result)
    {
        if (string.IsNullOrEmpty(currentPage) || !uiMap.Pages.TryGetValue(currentPage, out var page))
            return;

        // Determinar qual referência é um elemento baseado no tipo de step
        string? elementRef = null;
        
        if (Regex.IsMatch(stepLine, @"eu preencho ""([^""]+)"" com"))
        {
            // Primeiro parâmetro é o elemento
            elementRef = references.FirstOrDefault();
        }
        else if (Regex.IsMatch(stepLine, @"eu clico em ""([^""]+)"""))
        {
            // Primeiro parâmetro é o elemento
            elementRef = references.FirstOrDefault();
        }
        else if (Regex.IsMatch(stepLine, @"o elemento ""([^""]+)"" deve"))
        {
            // Primeiro parâmetro é o elemento
            elementRef = references.FirstOrDefault();
        }
        else if (Regex.IsMatch(stepLine, @"o atributo ""[^""]+"" de ""([^""]+)"""))
        {
            // Segundo parâmetro é o elemento
            elementRef = references.Skip(1).FirstOrDefault();
        }
        else if (Regex.IsMatch(stepLine, @"eu limpo o campo ""([^""]+)"""))
        {
            // Primeiro parâmetro é o elemento
            elementRef = references.FirstOrDefault();
        }
        
        // Validar se o elemento existe na página atual
        if (elementRef != null && !IsSpecialReference(elementRef))
        {
            if (!page.Elements.ContainsKey(elementRef))
            {
                // Verificar se o elemento existe em qualquer página
                var foundInPage = uiMap.Pages
                    .Where(p => p.Value.Elements.ContainsKey(elementRef))
                    .Select(p => p.Key)
                    .FirstOrDefault();
                
                if (foundInPage != null)
                {
                    result.AddWarning(new ValidationWarning(
                        "GHERKIN_ELEMENT_WRONG_PAGE",
                        $"Elemento '{elementRef}' pertence à página '{foundInPage}', não à página atual '{currentPage}'.",
                        filePath
                    ));
                }
                else
                {
                    result.AddWarning(new ValidationWarning(
                        "GHERKIN_ELEMENT_NOT_FOUND",
                        $"Elemento '{elementRef}' não encontrado em nenhuma página do UiMap.",
                        filePath
                    ));
                }
            }
        }
    }

    private bool IsSpecialReference(string reference)
    {
        // Rotas
        if (reference.StartsWith("/")) return true;
        
        // Variáveis de ambiente
        if (reference.StartsWith("${") && reference.EndsWith("}")) return true;
        
        // Referências de objeto
        if (reference.StartsWith("@")) return true;
        
        // Datasets
        if (reference.StartsWith("{{") && reference.EndsWith("}}")) return true;
        
        // Valores literais comuns (tipos de input, valores de atributo)
        var literalValues = new[] { "text", "password", "submit", "button", "checkbox", "radio", "hidden", "email", "number" };
        if (literalValues.Contains(reference.ToLower())) return true;
        
        // Valores que parecem ser dados de teste (não elementos)
        if (reference.Contains("-") && reference.Split('-').All(p => p.All(char.IsLetter))) return false;
        
        // Valores que parecem ser literais de teste
        if (reference.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.') && 
            !reference.Contains("-") && reference.Length > 20) return true;
        
        return false;
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
        // Verificar se é uma referência de objeto (@)
        else if (dataKey.StartsWith("@"))
        {
            var objectName = dataKey.Substring(1);
            var found = false;
            
            foreach (var ctx in dataMap.Contexts.Values)
            {
                if (ctx is IDictionary<object, object> dict && dict.ContainsKey(objectName))
                {
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                result.AddWarning(new ValidationWarning(
                    "GHERKIN_OBJECT_NOT_FOUND",
                    $"Objeto '{dataKey}' não encontrado em nenhum contexto do DataMap.",
                    filePath
                ));
            }
        }
        // Variáveis de ambiente são sempre válidas (resolvidas em runtime)
        else if (dataKey.StartsWith("${") && dataKey.EndsWith("}"))
        {
            // Variáveis de ambiente são válidas - não validar
        }
    }
}
