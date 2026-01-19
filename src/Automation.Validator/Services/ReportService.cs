using Automation.Validator.Models;

namespace Automation.Validator.Services;

/// <summary>
/// Gera relatórios de validação em diferentes formatos.
/// </summary>
public class ReportService
{
    public void PrintConsoleReport(ValidationResult result, string title)
    {
        Console.WriteLine($"\n{'='} {title} {'='}\n");

        if (result.IsValid && result.Warnings.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Validação passou com sucesso!");
            Console.ResetColor();
            return;
        }

        if (result.Errors.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ {result.Errors.Count} erro(s) encontrado(s):\n");
            Console.ResetColor();

            foreach (var error in result.Errors)
            {
                PrintError(error);
            }
        }

        if (result.Warnings.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n⚠ {result.Warnings.Count} aviso(s):\n");
            Console.ResetColor();

            foreach (var warning in result.Warnings)
            {
                PrintWarning(warning);
            }
        }
    }

    private void PrintError(ValidationError error)
    {
        var location = error.File != null && error.Line.HasValue
            ? $" [{error.File}:{error.Line}]"
            : error.File != null
                ? $" [{error.File}]"
                : "";

        Console.WriteLine($"  [{error.Code}]{location}");
        Console.WriteLine($"  → {error.Message}\n");
    }

    private void PrintWarning(ValidationWarning warning)
    {
        var location = warning.File != null ? $" [{warning.File}]" : "";
        Console.WriteLine($"  [{warning.Code}]{location}");
        Console.WriteLine($"  → {warning.Message}\n");
    }

    public string GenerateJsonReport(ValidationResult result)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            isValid = result.IsValid,
            errorCount = result.Errors.Count,
            warningCount = result.Warnings.Count,
            errors = result.Errors.Select(e => new
            {
                code = e.Code,
                message = e.Message,
                file = e.File,
                line = e.Line
            }),
            warnings = result.Warnings.Select(w => new
            {
                code = w.Code,
                message = w.Message,
                file = w.File
            })
        }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

        return json;
    }

    public CoverageReport CalculateCoverage(UiMapModel uiMap, List<string> testedPages)
    {
        var totalPages = uiMap.Pages.Count;
        var totalElements = uiMap.Pages.Values.Sum(p => p.Elements.Count);
        var testedPageSet = new HashSet<string>(testedPages);
        var testedPageCount = testedPageSet.Count;
        var testedElementCount = uiMap.Pages
            .Where(p => testedPageSet.Contains(p.Key))
            .Sum(p => p.Value.Elements.Count);

        var coverage = totalElements > 0 
            ? (testedElementCount / (double)totalElements) * 100 
            : 0;

        var untestedPages = uiMap.Pages.Keys
            .Where(p => !testedPageSet.Contains(p))
            .ToList();

        return new CoverageReport(
            totalPages,
            totalElements,
            testedPageCount,
            testedElementCount,
            coverage,
            untestedPages
        );
    }
}
