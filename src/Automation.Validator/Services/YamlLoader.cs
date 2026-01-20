using Automation.Validator.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Automation.Validator.Services;

/// <summary>
/// Carrega e desserializa arquivos YAML (UiMap, DataMap).
/// </summary>
public class YamlLoader
{
    private readonly IDeserializer _deserializer;

    public YamlLoader()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public UiMapModel LoadUiMap(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"UiMap não encontrado: {filePath}");

        var content = File.ReadAllText(filePath);
        
        // Deserialize como dicionário genérico primeiro
        var rawDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();
        
        var rawData = rawDeserializer.Deserialize<Dictionary<string, object>>(content);
        
        var uiMap = new UiMapModel();
        
        if (rawData != null && rawData.TryGetValue("pages", out var pagesObj) && pagesObj is Dictionary<object, object> pages)
        {
            foreach (var pageEntry in pages)
            {
                var pageName = pageEntry.Key.ToString() ?? "";
                var pageData = pageEntry.Value as Dictionary<object, object>;
                
                if (pageData == null) continue;
                
                var uiPage = new UiPage();
                
                foreach (var entry in pageData)
                {
                    var key = entry.Key.ToString() ?? "";
                    
                    if (key == "__meta" && entry.Value is Dictionary<object, object> meta)
                    {
                        if (meta.TryGetValue("route", out var route))
                            uiPage.Route = route?.ToString();
                        if (meta.TryGetValue("anchor", out var anchor))
                            uiPage.Anchor = anchor?.ToString();
                    }
                    else if (entry.Value is Dictionary<object, object> elementData)
                    {
                        var element = new UiElement();
                        if (elementData.TryGetValue("test_id", out var testId))
                            element.TestId = testId?.ToString();
                        uiPage.Elements[key] = element;
                    }
                }
                
                uiMap.Pages[pageName] = uiPage;
            }
        }
        
        return uiMap;
    }

    public DataMapModel LoadDataMap(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"DataMap não encontrado: {filePath}");

        var content = File.ReadAllText(filePath);
        
        // Deserialize como dicionário genérico primeiro
        var rawDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();
        
        var rawData = rawDeserializer.Deserialize<Dictionary<string, object>>(content);
        
        var dataMap = new DataMapModel();
        
        if (rawData != null)
        {
            // Processar contexts
            if (rawData.TryGetValue("contexts", out var contextsObj) && contextsObj is Dictionary<object, object> contexts)
            {
                foreach (var ctx in contexts)
                {
                    dataMap.Contexts[ctx.Key.ToString() ?? ""] = ctx.Value ?? new object();
                }
            }
            
            // Processar datasets
            if (rawData.TryGetValue("datasets", out var datasetsObj) && datasetsObj is Dictionary<object, object> datasets)
            {
                foreach (var ds in datasets)
                {
                    var dsName = ds.Key.ToString() ?? "";
                    var dsData = ds.Value as Dictionary<object, object>;
                    
                    if (dsData != null)
                    {
                        var dataSet = new DataSet();
                        
                        if (dsData.TryGetValue("strategy", out var strategy))
                            dataSet.Strategy = strategy?.ToString() ?? "sequential";
                        
                        if (dsData.TryGetValue("items", out var items) && items is List<object> itemList)
                            dataSet.Items = itemList.Select(i => i?.ToString() ?? "").ToList();
                        
                        dataMap.Datasets[dsName] = dataSet;
                    }
                }
            }
        }
        
        return dataMap;
    }

    public string LoadGherkin(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Feature file não encontrado: {filePath}");

        return File.ReadAllText(filePath);
    }
}
