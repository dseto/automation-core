using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace Automation.Core.DataMap;

public class DataMapLoader
{
    private readonly IDeserializer _deserializer;

    public DataMapLoader()
    {
        _deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public DataMapModel Load(string filePath)
    {
        if (!File.Exists(filePath))
            return new DataMapModel();

        try
        {
            var yaml = File.ReadAllText(filePath);
            var rawData = _deserializer.Deserialize<object>(yaml) as IDictionary;
            
            var model = new DataMapModel();
            
            if (rawData != null)
            {
                foreach (DictionaryEntry entry in rawData)
                {
                    var key = entry.Key?.ToString()?.ToLower();
                    if (key == "contexts" && entry.Value is IDictionary contexts)
                    {
                        foreach (DictionaryEntry ctxEntry in contexts)
                        {
                            if (ctxEntry.Key != null)
                                model.Contexts[ctxEntry.Key.ToString()] = ctxEntry.Value;
                        }
                    }
                    else if (key == "datasets" && entry.Value is IDictionary datasets)
                    {
                        foreach (DictionaryEntry dsEntry in datasets)
                        {
                            if (dsEntry.Key != null)
                                model.Datasets[dsEntry.Key.ToString()] = dsEntry.Value;
                        }
                    }
                }
            }
            
            return model;
        }
        catch (Exception)
        {
            return new DataMapModel();
        }
    }
}
