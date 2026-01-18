using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Automation.Core.UiMap;

public static class UiMapLoader
{
    public static UiMapModel LoadFromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("UiMap path is empty", nameof(path));

        if (!File.Exists(path))
            throw new FileNotFoundException($"ui-map.yaml not found at '{path}'", path);

        var yamlText = File.ReadAllText(path);

        var stream = new YamlStream();
        using (var reader = new StringReader(yamlText))
            stream.Load(reader);

        if (stream.Documents.Count == 0 || stream.Documents[0].RootNode is not YamlMappingNode root)
            throw new InvalidDataException("ui-map.yaml is empty or not a YAML mapping at the root.");

        // Supported schemas:
        //  A) root = { pages: {...}, modals: {...} }
        //  B) root = { LoginPage: {...}, DashboardPage: {...} }  (legacy/simple)
        //  C) root = { pages: { pages: {...}, modals: {...} } } (accidental wrapper) -> we normalize

        YamlMappingNode pagesNode;
        YamlMappingNode? modalsNode = null;

        if (TryGetMapping(root, "pages", out var explicitPages))
        {
            pagesNode = explicitPages;
            TryGetMapping(root, "modals", out modalsNode);

            // Normalize accidental wrapper: pages: { pages: {...}, modals: {...} }
            if (TryGetMapping(pagesNode, "pages", out var nestedPages) &&
                (pagesNode.Children.Count == 1 || pagesNode.Children.Keys.OfType<YamlScalarNode>().All(k =>
                    string.Equals(k.Value, "pages", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(k.Value, "modals", StringComparison.OrdinalIgnoreCase))))
            {
                pagesNode = nestedPages;
                if (TryGetMapping(explicitPages, "modals", out var nestedModals))
                    modalsNode = nestedModals;
            }
        }
        else
        {
            // Legacy/simple: treat root as pages mapping
            pagesNode = root;
            // no modals supported here
        }

        var uiMap = new UiMapModel();

        foreach (var (key, value) in pagesNode.Children)
        {
            var pageName = Scalar(key);
            if (value is not YamlMappingNode pageMap)
                throw new InvalidDataException($"Page '{pageName}' must be a YAML mapping.");
            uiMap.Pages[pageName] = ParsePage(pageMap);
        }

        if (modalsNode is not null)
        {
            foreach (var (key, value) in modalsNode.Children)
            {
                var modalName = Scalar(key);
                if (value is not YamlMappingNode modalMap)
                    throw new InvalidDataException($"Modal '{modalName}' must be a YAML mapping.");
                uiMap.Modals[modalName] = ParsePage(modalMap);
            }
        }

        UiMapValidator.Validate(uiMap, sourcePath: path);
        return uiMap;
    }

    private static UiPage ParsePage(YamlMappingNode pageNode)
    {
        var page = new UiPage();

        // meta (optional)
        if (TryGetMapping(pageNode, "__meta", out var metaNode) || TryGetMapping(pageNode, "meta", out metaNode))
        {
            page.Meta = new UiMeta
            {
                Route = TryGetScalarOrTestId(metaNode, "route"),
                Anchor = TryGetScalarOrTestId(metaNode, "anchor")
            };
        }
        else
        {
            // Back-compat: allow top-level route/anchor
            var route = TryGetScalarOrTestId(pageNode, "route");
            var anchor = TryGetScalarOrTestId(pageNode, "anchor");
            if (!string.IsNullOrWhiteSpace(route) || !string.IsNullOrWhiteSpace(anchor))
            {
                page.Meta = new UiMeta { Route = route, Anchor = anchor };
            }
        }

        // elements (legacy nested)
        if (TryGetMapping(pageNode, "elements", out var elNode))
        {
            AddElementsFromMapping(page.Elements, elNode);
        }

        // elements (inline: PageName -> FriendlyName -> testId)
        var hasTopLevelMeta = TryGetScalarOrTestId(pageNode, "route") is not null ||
                              TryGetScalarOrTestId(pageNode, "anchor") is not null;

        foreach (var (k, v) in pageNode.Children)
        {
            var name = Scalar(k);
            if (IsReservedPageKey(name))
                continue;
            if (hasTopLevelMeta && IsTopLevelMetaKey(name))
                continue;
            if (page.Elements.ContainsKey(name))
                continue;

            var testId = ExtractTestId(v);
            if (string.IsNullOrWhiteSpace(testId))
                continue;
            page.Elements[name] = testId;
        }

        return page;
    }

    private static void AddElementsFromMapping(Dictionary<string, string> elements, YamlMappingNode elNode)
    {
        foreach (var (k, v) in elNode.Children)
        {
            var name = Scalar(k);
            if (IsReservedPageKey(name))
                continue;
            var testId = ExtractTestId(v);
            if (string.IsNullOrWhiteSpace(testId))
                continue;
            elements[name] = testId;
        }
    }

    private static string? TryGetScalar(YamlMappingNode node, string key)
    {
        if (!node.Children.TryGetValue(new YamlScalarNode(key), out var value))
            return null;
        return value is YamlScalarNode s ? s.Value : value?.ToString();
    }

    private static string? TryGetScalarOrTestId(YamlMappingNode node, string key)
    {
        if (!node.Children.TryGetValue(new YamlScalarNode(key), out var value))
            return null;
        return ExtractTestId(value);
    }

    private static bool TryGetMapping(YamlMappingNode node, string key, out YamlMappingNode mapping)
    {
        if (node.Children.TryGetValue(new YamlScalarNode(key), out var value) && value is YamlMappingNode m)
        {
            mapping = m;
            return true;
        }
        mapping = null!;
        return false;
    }

    private static string Scalar(YamlNode node)
    {
        if (node is YamlScalarNode s && s.Value is not null) return s.Value;
        return node.ToString();
    }

    private static bool IsReservedPageKey(string key) =>
        string.Equals(key, "__meta", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, "meta", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, "elements", StringComparison.OrdinalIgnoreCase);

    private static bool IsTopLevelMetaKey(string key) =>
        string.Equals(key, "route", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, "anchor", StringComparison.OrdinalIgnoreCase);

    private static string? ExtractTestId(YamlNode node)
    {
        if (node is YamlScalarNode scalar)
            return scalar.Value;

        if (node is YamlMappingNode mapping)
        {
            var testId = TryGetScalar(mapping, "testId") ?? TryGetScalar(mapping, "dataTestId")
                         ?? TryGetScalar(mapping, "data-testid") ?? TryGetScalar(mapping, "data_testid");
            if (!string.IsNullOrWhiteSpace(testId))
                return testId;

            if (mapping.Children.Count == 1)
            {
                var only = mapping.Children.Values.FirstOrDefault();
                if (only is YamlScalarNode onlyScalar)
                    return onlyScalar.Value;
            }
        }

        return null;
    }
}
