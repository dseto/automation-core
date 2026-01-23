using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Automation.Core.UiMap;

public class UiMapModel
{
    public Dictionary<string, object> Pages { get; set; } = new();
    public Dictionary<string, object> Modals { get; set; } = new();

    public UiPage GetPageOrThrow(string pageName)
    {
        if (Pages.TryGetValue(pageName, out var pageObj) && pageObj is IDictionary pageDict)
            return new UiPage(pageDict);
            
        if (Modals.TryGetValue(pageName, out var modalObj) && modalObj is IDictionary modalDict)
            return new UiPage(modalDict);
        
        throw new ArgumentException($"Página ou Modal '{pageName}' não encontrada no ui-map.yaml.");
    }

    /// <summary>
    /// Retorna os nomes de páginas/modais que contêm o elemento fornecido.
    /// Usado como fallback quando um elemento simples não existe na página atual.
    /// </summary>
    public IEnumerable<string> FindPagesContainingElement(string elementName)
    {
        foreach (var kv in Pages)
        {
            if (kv.Value is IDictionary pageDict && pageDict.Contains(elementName))
                yield return kv.Key;
        }

        foreach (var kv in Modals)
        {
            if (kv.Value is IDictionary modalDict && modalDict.Contains(elementName))
                yield return kv.Key;
        }
    }
}

public class UiPage
{
    private readonly IDictionary _data;

    public UiPage(IDictionary data)
    {
        _data = data;
    }

    public string? Route
    {
        get
        {
            if (_data.Contains("__meta") && _data["__meta"] is IDictionary meta)
                return meta.Contains("route") ? meta["route"]?.ToString() : null;
            return null;
        }
    }

    public string? Anchor
    {
        get
        {
            if (_data.Contains("__meta") && _data["__meta"] is IDictionary meta)
                return meta.Contains("anchor") ? meta["anchor"]?.ToString() : null;
            return null;
        }
    }

    public bool HasAnchor => !string.IsNullOrWhiteSpace(Anchor);
    public bool HasRoute => !string.IsNullOrWhiteSpace(Route);
    public bool IsIdentifiable => HasRoute || HasAnchor;

    public string GetTestIdOrThrow(string elementName)
    {
        if (_data.Contains(elementName))
        {
            var element = _data[elementName];
            if (element is IDictionary elementDict)
            {
                // Suporta tanto testId quanto test_id (compatibilidade)
                if (elementDict.Contains("testId"))
                    return elementDict["testId"]?.ToString() ?? throw new ArgumentException($"Elemento '{elementName}' tem testId nulo.");
                if (elementDict.Contains("test_id"))
                    return elementDict["test_id"]?.ToString() ?? throw new ArgumentException($"Elemento '{elementName}' tem test_id nulo.");
            }
            
            // Suporte para formato simplificado se existir
            if (element is string testId) return testId;
        }

        throw new ArgumentException($"Elemento '{elementName}' não encontrado no mapeamento da página.");
    }
}
