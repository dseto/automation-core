using Automation.Validator.Models;

namespace Automation.Validator.Validators;

/// <summary>
/// Valida integridade do UiMap (schema, unicidade, rotas).
/// </summary>
public class UiMapValidator
{
    public ValidationResult Validate(UiMapModel uiMap, string filePath)
    {
        var result = ValidationResult.Success();

        // Validar que existe pelo menos uma página
        if (uiMap.Pages.Count == 0)
        {
            result.AddError(new ValidationError(
                "UIMAP_EMPTY",
                "UiMap não contém nenhuma página.",
                filePath
            ));
            return result;
        }

        var testedPages = new HashSet<string>();

        foreach (var (pageName, page) in uiMap.Pages)
        {
            ValidatePage(pageName, page, filePath, result);
            testedPages.Add(pageName);
        }

        // Validar referências cruzadas
        ValidateCrossReferences(uiMap, filePath, result);

        return result;
    }

    private void ValidatePage(string pageName, UiPage page, string filePath, ValidationResult result)
    {
        // Validar que a página tem pelo menos um elemento
        if (page.Elements.Count == 0)
        {
            result.AddWarning(new ValidationWarning(
                "UIMAP_PAGE_NO_ELEMENTS",
                $"Página '{pageName}' não contém nenhum elemento.",
                filePath
            ));
        }

        // Validar rota
        if (string.IsNullOrWhiteSpace(page.Route))
        {
            result.AddError(new ValidationError(
                "UIMAP_PAGE_NO_ROUTE",
                $"Página '{pageName}' não possui rota definida.",
                filePath
            ));
        }
        else if (!page.Route.StartsWith("/"))
        {
            result.AddError(new ValidationError(
                "UIMAP_INVALID_ROUTE",
                $"Página '{pageName}' tem rota inválida: '{page.Route}'. Deve começar com '/'.",
                filePath
            ));
        }

        // Validar elementos
        var testedElements = new HashSet<string>();
        foreach (var (elementName, element) in page.Elements)
        {
            ValidateElement(pageName, elementName, element, filePath, result);
            testedElements.Add(elementName);
        }

        // Validar unicidade de testIds
        var testIds = new HashSet<string>();
        foreach (var element in page.Elements.Values)
        {
            if (!string.IsNullOrWhiteSpace(element.TestId))
            {
                if (!testIds.Add(element.TestId))
                {
                    result.AddError(new ValidationError(
                        "UIMAP_DUPLICATE_TESTID",
                        $"TestId duplicado na página '{pageName}': '{element.TestId}'",
                        filePath
                    ));
                }
            }
        }
    }

    private void ValidateElement(string pageName, string elementName, UiElement element, string filePath, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(element.TestId))
        {
            result.AddError(new ValidationError(
                "UIMAP_ELEMENT_NO_TESTID",
                $"Elemento '{elementName}' na página '{pageName}' não possui testId.",
                filePath
            ));
        }
        else
        {
            // Validar padrão de testId (deve ser page.pageName.elementName)
            var expectedPattern = $"page.{pageName}.{elementName}";
            if (element.TestId != expectedPattern)
            {
                result.AddWarning(new ValidationWarning(
                    "UIMAP_TESTID_PATTERN",
                    $"TestId '{element.TestId}' não segue padrão esperado: '{expectedPattern}'",
                    filePath
                ));
            }
        }
    }

    private void ValidateCrossReferences(UiMapModel uiMap, string filePath, ValidationResult result)
    {
        // Validar que não há rotas duplicadas
        var routes = new Dictionary<string, string>();
        foreach (var (pageName, page) in uiMap.Pages)
        {
            if (!string.IsNullOrWhiteSpace(page.Route))
            {
                if (routes.TryGetValue(page.Route, out var existingPage))
                {
                    result.AddError(new ValidationError(
                        "UIMAP_DUPLICATE_ROUTE",
                        $"Rota '{page.Route}' está mapeada para múltiplas páginas: '{existingPage}' e '{pageName}'",
                        filePath
                    ));
                }
                else
                {
                    routes[page.Route] = pageName;
                }
            }
        }
    }
}
