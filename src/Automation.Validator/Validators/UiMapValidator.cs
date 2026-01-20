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
        // Determinar se é uma página ou modal baseado no anchor
        var isModal = page.Anchor?.StartsWith("modal.") == true;
        var prefix = isModal ? "modal" : "page";
        
        // Validar que a página tem pelo menos um elemento
        if (page.Elements.Count == 0)
        {
            result.AddWarning(new ValidationWarning(
                "UIMAP_PAGE_NO_ELEMENTS",
                $"Página '{pageName}' não contém nenhum elemento.",
                filePath
            ));
        }

        // Validar rota (modais podem não ter rota)
        if (string.IsNullOrWhiteSpace(page.Route) && !isModal)
        {
            result.AddWarning(new ValidationWarning(
                "UIMAP_PAGE_NO_ROUTE",
                $"Página '{pageName}' não possui rota definida. Se for um modal, defina anchor com prefixo 'modal.'",
                filePath
            ));
        }
        else if (!string.IsNullOrWhiteSpace(page.Route) && !page.Route.StartsWith("/") && !page.Route.Contains(":"))
        {
            result.AddError(new ValidationError(
                "UIMAP_INVALID_ROUTE",
                $"Página '{pageName}' tem rota inválida: '{page.Route}'. Deve começar com '/'.",
                filePath
            ));
        }

        // Validar anchor
        if (string.IsNullOrWhiteSpace(page.Anchor))
        {
            result.AddWarning(new ValidationWarning(
                "UIMAP_PAGE_NO_ANCHOR",
                $"Página '{pageName}' não possui anchor definido. Recomendado para SPAs.",
                filePath
            ));
        }

        // Validar elementos
        var testedElements = new HashSet<string>();
        foreach (var (elementName, element) in page.Elements)
        {
            ValidateElement(pageName, elementName, element, prefix, filePath, result);
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

    private void ValidateElement(string pageName, string elementName, UiElement element, string prefix, string filePath, ValidationResult result)
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
            // Validar padrão de testId (deve ser page.pageName.elementName ou modal.pageName.elementName)
            var expectedPattern = $"{prefix}.{pageName}.{elementName}";
            
            // Aceitar também testIds com sufixo (para elementos dinâmicos como page.dashboard.process-)
            var testIdBase = element.TestId.TrimEnd('-');
            var expectedBase = expectedPattern.TrimEnd('-');
            
            if (element.TestId != expectedPattern && testIdBase != expectedBase && !element.TestId.StartsWith($"{prefix}.{pageName}."))
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
        // Validar que não há rotas duplicadas (exceto rotas vazias para modais)
        var routes = new Dictionary<string, string>();
        foreach (var (pageName, page) in uiMap.Pages)
        {
            if (!string.IsNullOrWhiteSpace(page.Route))
            {
                // Normalizar rotas com parâmetros (ex: /processes/:id -> /processes/:id)
                var normalizedRoute = page.Route;
                
                if (routes.TryGetValue(normalizedRoute, out var existingPage))
                {
                    result.AddError(new ValidationError(
                        "UIMAP_DUPLICATE_ROUTE",
                        $"Rota '{page.Route}' está mapeada para múltiplas páginas: '{existingPage}' e '{pageName}'",
                        filePath
                    ));
                }
                else
                {
                    routes[normalizedRoute] = pageName;
                }
            }
        }

        // Validar unicidade global de testIds
        var globalTestIds = new Dictionary<string, (string page, string element)>();
        foreach (var (pageName, page) in uiMap.Pages)
        {
            foreach (var (elementName, element) in page.Elements)
            {
                if (!string.IsNullOrWhiteSpace(element.TestId))
                {
                    if (globalTestIds.TryGetValue(element.TestId, out var existing))
                    {
                        result.AddError(new ValidationError(
                            "UIMAP_GLOBAL_DUPLICATE_TESTID",
                            $"TestId '{element.TestId}' duplicado: usado em '{existing.page}.{existing.element}' e '{pageName}.{elementName}'",
                            filePath
                        ));
                    }
                    else
                    {
                        globalTestIds[element.TestId] = (pageName, elementName);
                    }
                }
            }
        }
    }
}
