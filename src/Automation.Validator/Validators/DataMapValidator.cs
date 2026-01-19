using Automation.Validator.Models;

namespace Automation.Validator.Validators;

/// <summary>
/// Valida integridade do DataMap (schema, referências, estratégias).
/// </summary>
public class DataMapValidator
{
    public ValidationResult Validate(DataMapModel dataMap, string filePath)
    {
        var result = ValidationResult.Success();

        // Validar contextos
        if (dataMap.Contexts.Count == 0)
        {
            result.AddError(new ValidationError(
                "DATAMAP_NO_CONTEXTS",
                "DataMap não contém nenhum contexto.",
                filePath
            ));
        }
        else
        {
            ValidateContexts(dataMap.Contexts, filePath, result);
        }

        // Validar datasets
        ValidateDatasets(dataMap.Datasets, filePath, result);

        // Validar referências cruzadas
        ValidateCrossReferences(dataMap, filePath, result);

        return result;
    }

    private void ValidateContexts(Dictionary<string, object> contexts, string filePath, ValidationResult result)
    {
        // Validar que existe contexto "default"
        if (!contexts.ContainsKey("default"))
        {
            result.AddError(new ValidationError(
                "DATAMAP_NO_DEFAULT_CONTEXT",
                "DataMap deve conter um contexto 'default'.",
                filePath
            ));
        }

        foreach (var (contextName, contextValue) in contexts)
        {
            if (contextValue is not IDictionary<object, object> contextDict)
            {
                result.AddError(new ValidationError(
                    "DATAMAP_INVALID_CONTEXT",
                    $"Contexto '{contextName}' não é um dicionário válido.",
                    filePath
                ));
                continue;
            }

            if (contextDict.Count == 0)
            {
                result.AddWarning(new ValidationWarning(
                    "DATAMAP_EMPTY_CONTEXT",
                    $"Contexto '{contextName}' está vazio.",
                    filePath
                ));
            }
        }
    }

    private void ValidateDatasets(Dictionary<string, DataSet> datasets, string filePath, ValidationResult result)
    {
        foreach (var (datasetName, dataset) in datasets)
        {
            // Validar estratégia
            var validStrategies = new[] { "sequential", "random", "unique" };
            if (!validStrategies.Contains(dataset.Strategy))
            {
                result.AddError(new ValidationError(
                    "DATAMAP_INVALID_STRATEGY",
                    $"Dataset '{datasetName}' tem estratégia inválida: '{dataset.Strategy}'. Válidas: {string.Join(", ", validStrategies)}",
                    filePath
                ));
            }

            // Validar items
            if (dataset.Items.Count == 0)
            {
                result.AddError(new ValidationError(
                    "DATAMAP_EMPTY_DATASET",
                    $"Dataset '{datasetName}' não contém itens.",
                    filePath
                ));
            }

            // Validar unicidade para estratégia "unique"
            if (dataset.Strategy == "unique")
            {
                var uniqueItems = new HashSet<string>(dataset.Items);
                if (uniqueItems.Count != dataset.Items.Count)
                {
                    result.AddWarning(new ValidationWarning(
                        "DATAMAP_DUPLICATE_ITEMS",
                        $"Dataset '{datasetName}' com estratégia 'unique' contém itens duplicados.",
                        filePath
                    ));
                }
            }
        }
    }

    private void ValidateCrossReferences(DataMapModel dataMap, string filePath, ValidationResult result)
    {
        // Validar que não há referências circulares (simplificado)
        // Em uma implementação real, isso seria mais complexo
        var allKeys = new HashSet<string>();
        foreach (var context in dataMap.Contexts.Values)
        {
            if (context is IDictionary<object, object> contextDict)
            {
                foreach (var key in contextDict.Keys)
                {
                    allKeys.Add(key.ToString() ?? "");
                }
            }
        }

        // Validar que chaves de datasets não conflitam com contextos
        foreach (var datasetName in dataMap.Datasets.Keys)
        {
            if (allKeys.Contains(datasetName))
            {
                result.AddWarning(new ValidationWarning(
                    "DATAMAP_KEY_CONFLICT",
                    $"Dataset '{datasetName}' tem o mesmo nome de uma chave em contexto.",
                    filePath
                ));
            }
        }
    }
}
