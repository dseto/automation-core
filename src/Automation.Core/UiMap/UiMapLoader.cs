using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Automation.Core.UiMap;

public static class UiMapLoader
{
    private static readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public static UiMapModel LoadFromFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"ui-map.yaml não encontrado em '{path}'", path);

        var yaml = File.ReadAllText(path);
        return _deserializer.Deserialize<UiMapModel>(yaml) ?? new UiMapModel();
    }
}
