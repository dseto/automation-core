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

        var env = _settings.EnvironmentName?.ToLower() ?? "default";
        
        // 1. Tentar resolver do contexto atual
        if (_model.Contexts != null)
        {
            var contextObj = GetFromDictionary(_model.Contexts, env);
            if (contextObj is IDictionary context)
            {
                var result = GetFromDictionary(context, dataKey);
                if (result != null) return result;
            }
        }

        // Fallback para 'default'
        if (env != "default" && _model.Contexts != null)
        {
            var defaultContextObj = GetFromDictionary(_model.Contexts, "default");
            if (defaultContextObj is IDictionary defaultContext)
            {
                var result = GetFromDictionary(defaultContext, dataKey);
                if (result != null) return result;
            }
        }

        // 2. Tentar resolver de um DataSet
        if (_model.Datasets != null)
        {
            var dataSetObj = GetFromDictionary(_model.Datasets, dataKey);
            if (dataSetObj is IDictionary dataSet)
                return ResolveFromDataSet(dataKey, dataSet);
        }

        return dataKey;
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
