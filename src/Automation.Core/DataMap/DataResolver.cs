using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Automation.Core.Configuration;

namespace Automation.Core.DataMap;

public class DataResolver
{
    private readonly DataMapModel _model;
    private readonly RunSettings _settings;
    private readonly Dictionary<string, int> _dataSetIndices = new();
    private readonly Random _random = new();

    public DataResolver(DataMapModel model, RunSettings settings)
    {
        _model = model;
        _settings = settings;
    }

    public object Resolve(string dataKey)
    {
        if (string.IsNullOrWhiteSpace(dataKey)) return null;

        // 1. Checa por referência de objeto (@)
        if (dataKey.StartsWith("@"))
            return ResolveObjectReference(dataKey);

        // 2. Checa por referência de dataset ({{...}})
        if (dataKey.StartsWith("{{") && dataKey.EndsWith("}}"))
            return ResolveDatasetReference(dataKey);

        // 3. Checa por variável de ambiente (${...})
        if (dataKey.StartsWith("${") && dataKey.EndsWith("}"))
            return ResolveEnvironmentVariable(dataKey);

        // 4. Se nenhum prefixo corresponde, trata como literal
        return dataKey;
    }

    private object ResolveObjectReference(string input)
    {
        var key = input.Substring(1);
        var env = _settings.EnvironmentName?.ToLower() ?? "default";

        if (_model.Contexts != null)
        {
            var contextObj = GetFromDictionary(_model.Contexts, env);
            if (contextObj is IDictionary context)
            {
                var result = GetFromDictionary(context, key);
                if (result != null) return result;
            }
        }

        if (env != "default" && _model.Contexts != null)
        {
            var defaultContextObj = GetFromDictionary(_model.Contexts, "default");
            if (defaultContextObj is IDictionary defaultContext)
            {
                var result = GetFromDictionary(defaultContext, key);
                if (result != null) return result;
            }
        }

        throw new InvalidOperationException($"Objeto '@{key}' não encontrado no DataMap.");
    }

    private object ResolveDatasetReference(string input)
    {
        var key = input.Substring(2, input.Length - 4);

        if (_model.Datasets == null)
            throw new InvalidOperationException($"Dataset '{{{{{key}}}}}' não encontrado: nenhum dataset definido.");

        var dataSetObj = GetFromDictionary(_model.Datasets, key);
        if (dataSetObj is not IDictionary dataSet)
            throw new InvalidOperationException($"Dataset '{{{{{key}}}}}' não encontrado no DataMap.");

        return ResolveFromDataSet(key, dataSet);
    }

    private object ResolveEnvironmentVariable(string input)
    {
        var key = input.Substring(2, input.Length - 3);
        var value = Environment.GetEnvironmentVariable(key);

        if (value == null)
            throw new InvalidOperationException($"Variável de ambiente '${{{key}}}' não encontrada. Defina antes de executar os testes.");

        return value;
    }

    private object GetFromDictionary(IDictionary dict, string key)
    {
        if (dict == null) return null;
        
        // Tenta busca direta
        if (dict.Contains(key)) return dict[key];
        
        // Busca case-insensitive e com tratamento de snake_case/camelCase
        var normalizedKey = key.Replace("_", "").ToLower();
        foreach (var k in dict.Keys)
        {
            var dictKey = k.ToString();
            if (string.Equals(dictKey, key, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dictKey.Replace("_", "").ToLower(), normalizedKey, StringComparison.OrdinalIgnoreCase))
            {
                return dict[k];
            }
        }
        
        return null;
    }

    private object ResolveFromDataSet(string key, IDictionary dataSet)
    {
        var itemsObj = GetFromDictionary(dataSet, "items");
        if (itemsObj is not IList items || items.Count == 0)
            return null;

        var strategyObj = GetFromDictionary(dataSet, "strategy");
        var strategy = strategyObj?.ToString()?.ToLower() ?? "sequential";

        switch (strategy)
        {
            case "random":
                return items[_random.Next(items.Count)];
            default:
                if (!_dataSetIndices.TryGetValue(key, out var index))
                    index = 0;
                var item = items[index % items.Count];
                _dataSetIndices[key] = index + 1;
                return item;
        }
    }
}
