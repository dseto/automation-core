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
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    public UiMapModel LoadUiMap(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"UiMap não encontrado: {filePath}");

        var content = File.ReadAllText(filePath);
        var uiMap = _deserializer.Deserialize<UiMapModel>(content);
        
        return uiMap ?? new UiMapModel();
    }

    public DataMapModel LoadDataMap(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"DataMap não encontrado: {filePath}");

        var content = File.ReadAllText(filePath);
        var dataMap = _deserializer.Deserialize<DataMapModel>(content);
        
        return dataMap ?? new DataMapModel();
    }

    public string LoadGherkin(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Feature file não encontrado: {filePath}");

        return File.ReadAllText(filePath);
    }
}
