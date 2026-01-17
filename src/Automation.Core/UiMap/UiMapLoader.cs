using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Automation.Core.UiMap;

public sealed class UiMapLoader
{
    private readonly IDeserializer _deserializer;

    public UiMapLoader()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public UiMapModel LoadFromFile(string path)
    {
        var yaml = File.ReadAllText(path);
        return LoadFromString(yaml);
    }

    public UiMapModel LoadFromString(string yaml)
    {
        var raw = _deserializer.Deserialize<Dictionary<string, Dictionary<string, object?>>>(yaml);
        var model = new UiMapModel();

        foreach (var (pageName, pageObj) in raw)
        {
            var page = new UiPage();
            foreach (var (key, value) in pageObj)
            {
                if (key.Equals("__meta", StringComparison.OrdinalIgnoreCase))
                {
                    if (value is Dictionary<object, object> metaDict)
                    {
                        var meta = new UiMeta();
                        if (metaDict.TryGetValue("route", out var r)) meta.Route = r?.ToString();
                        if (metaDict.TryGetValue("anchor", out var a)) meta.Anchor = a?.ToString();
                        page.Meta = meta;
                    }
                    continue;
                }

                page.Elements[key] = value?.ToString() ?? "";
            }

            model.Pages[pageName] = page;
        }

        return model;
    }
}
