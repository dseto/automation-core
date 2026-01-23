// Auto-generated patch: UiMapValidator with reflection-based validation (no dependency on UiMapDocument types)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Automation.Core.UiMap
{
    /// <summary>
    /// Validates the in-memory UI Map object loaded from ui-map.yaml.
    /// This implementation intentionally uses reflection so it does not depend on a specific model type
    /// (e.g., UiMapDocument) and remains compatible as the model evolves.
    /// </summary>
    public static class UiMapValidator
    {
        public static void Validate(object? uiMap, string? sourcePath = null)
        {
            if (uiMap is null)
                throw new UiMapValidationException($"UiMap is null. Source: {sourcePath ?? "<unknown>"}");

            // Expect something like:
            // uiMap.Pages : IDictionary<string, PageDefinition>
            // uiMap.Modals: IDictionary<string, ModalDefinition> (optional)
            var pagesObj = GetProp(uiMap, "Pages") ?? GetProp(uiMap, "pages");
            var modalsObj = GetProp(uiMap, "Modals") ?? GetProp(uiMap, "modals"); // optional

            if (pagesObj is null)
            {
                throw new UiMapValidationException(
                    $"UiMap does not expose a Pages dictionary/property. " +
                    $"Found type: {uiMap.GetType().FullName}. Source: {sourcePath ?? "<unknown>"}");
            }

            var pages = AsStringKeyDictionary(pagesObj);
            if (pages is null)
            {
                throw new UiMapValidationException(
                    $"UiMap.Pages is not a string-key dictionary. Actual: {pagesObj.GetType().FullName}. " +
                    $"Source: {sourcePath ?? "<unknown>"}");
            }

            if (pages.Count == 0)
            {
                // Common failure when YAML is malformed (e.g., duplicated 'pages:' nesting) and only top-level keys were parsed.
                var topKeys = TryGetTopLevelKeys(uiMap);
                var hint = topKeys.Any()
                    ? $"Top-level keys: {string.Join(", ", topKeys)}. "
                    : string.Empty;

                throw new UiMapValidationException(
                    $"UiMap.Pages is empty. This usually means the YAML structure is wrong (e.g., a nested 'pages:' inside 'pages:'). " +
                    hint +
                    $"Source: {sourcePath ?? "<unknown>"}");
            }

            // Validate each page contains an Elements dictionary with at least 1 element
            var errors = new List<string>();

            foreach (var (pageName, pageDef) in pages)
            {
                if (string.IsNullOrWhiteSpace(pageName))
                {
                    errors.Add("A page entry has an empty key.");
                    continue;
                }

                if (pageDef is null)
                {
                    errors.Add($"Page '{pageName}' definition is null.");
                    continue;
                }

                var elementsObj = GetProp(pageDef, "Elements") ?? GetProp(pageDef, "elements");
                if (elementsObj is null)
                {
                    // Some teams may model pages directly as a dictionary of elements (PageName -> { ElementName: testid })
                    // In that case, the pageDef itself is a dictionary.
                    var fallbackElements = AsStringKeyDictionary(pageDef);
                    if (fallbackElements is null)
                    {
                        errors.Add($"Page '{pageName}' has no Elements dictionary/property.");
                        continue;
                    }

                    ValidateElementsDictionary(pageName, fallbackElements, errors);
                    continue;
                }

                var elements = AsStringKeyDictionary(elementsObj);
                if (elements is null)
                {
                    errors.Add($"Page '{pageName}'.Elements is not a string-key dictionary. Actual: {elementsObj.GetType().FullName}");
                    continue;
                }

                ValidateElementsDictionary(pageName, elements, errors);
            }

            // Optional: validate modals if present
            if (modalsObj is not null)
            {
                var modals = AsStringKeyDictionary(modalsObj);
                if (modals is null)
                {
                    errors.Add($"UiMap.Modals is not a string-key dictionary. Actual: {modalsObj.GetType().FullName}");
                }
                else
                {
                    foreach (var (modalName, modalDef) in modals)
                    {
                        if (string.IsNullOrWhiteSpace(modalName))
                        {
                            errors.Add("A modal entry has an empty key.");
                            continue;
                        }

                        if (modalDef is null)
                        {
                            errors.Add($"Modal '{modalName}' definition is null.");
                            continue;
                        }

                        var elementsObj = GetProp(modalDef, "Elements") ?? GetProp(modalDef, "elements");
                        if (elementsObj is null)
                        {
                            var fallbackElements = AsStringKeyDictionary(modalDef);
                            if (fallbackElements is null)
                            {
                                errors.Add($"Modal '{modalName}' has no Elements dictionary/property.");
                                continue;
                            }

                            ValidateElementsDictionary($"modal:{modalName}", fallbackElements, errors);
                            continue;
                        }

                        var elements = AsStringKeyDictionary(elementsObj);
                        if (elements is null)
                        {
                            errors.Add($"Modal '{modalName}'.Elements is not a string-key dictionary. Actual: {elementsObj.GetType().FullName}");
                            continue;
                        }

                        ValidateElementsDictionary($"modal:{modalName}", elements, errors);
                    }
                }
            }

            // Enforce policy: UiMap must define a root route '/'
            bool hasRootRoute = false;
            foreach (var (pageName, pageDef) in pages)
            {
                var route = GetRouteFromPageDef(pageDef);
                if (!string.IsNullOrWhiteSpace(route) && string.Equals(route.Trim(), "/", StringComparison.Ordinal))
                {
                    hasRootRoute = true;
                    break;
                }
            }

            if (!hasRootRoute)
            {
                errors.Add("UiMap must include a page with __meta.route = '/'. This repository chooses the policy that '/' should be explicitly mapped.");
            }

            if (errors.Count > 0)
            {
                var msg =
                    $"UiMap validation failed ({errors.Count} issue(s)). Source: {sourcePath ?? "<unknown>"}\n" +
                    string.Join("\n", errors.Select(e => "- " + e));
                throw new UiMapValidationException(msg);
            }
        }

        private static void ValidateElementsDictionary(string scopeName, IDictionary<string, object?> elements, List<string> errors)
        {
            var filtered = elements
                .Where(kv => !IsMetaKey(kv.Key))
                .ToList();

            if (filtered.Count == 0)
            {
                errors.Add($"'{scopeName}' has 0 elements mapped.");
                return;
            }

            foreach (var (elementKey, elementValue) in filtered)
            {
                if (string.IsNullOrWhiteSpace(elementKey))
                {
                    errors.Add($"'{scopeName}' has an element with empty key.");
                    continue;
                }

                // Element keys MUST NOT contain '.' to avoid ambiguity with page.element notation
                if (elementKey.Contains('.'))
                {
                    errors.Add($"'{scopeName}.{elementKey}' contains invalid character '.' in element key. Element keys MUST NOT contain '.'.");
                    continue;
                }

                // elementValue can be:
                // - string testid
                // - object with TestId/DataTestId property
                if (elementValue is null)
                {
                    errors.Add($"'{scopeName}.{elementKey}' mapping is null.");
                    continue;
                }

                if (elementValue is string s)
                {
                    if (string.IsNullOrWhiteSpace(s))
                        errors.Add($"'{scopeName}.{elementKey}' maps to an empty test id.");
                    continue;
                }

                var testId = GetProp(elementValue, "TestId") ?? GetProp(elementValue, "testId")
                            ?? GetProp(elementValue, "DataTestId") ?? GetProp(elementValue, "dataTestId")
                            ?? GetProp(elementValue, "DataTestid") ?? GetProp(elementValue, "dataTestid");

                if (testId is string ts)
                {
                    if (string.IsNullOrWhiteSpace(ts))
                        errors.Add($"'{scopeName}.{elementKey}' TestId is empty.");
                }
                else if (testId is null)
                {
                    // Not necessarily fatal, but in this framework we expect stable locator contract.
                    errors.Add($"'{scopeName}.{elementKey}' is not a string and has no TestId/DataTestId property.");
                }
                else
                {
                    errors.Add($"'{scopeName}.{elementKey}' TestId/DataTestId is not a string (actual: {testId.GetType().FullName}).");
                }
            }
        }

        private static object? GetProp(object obj, string propName)
        {
            var t = obj.GetType();
            var p = t.GetProperty(propName);
            return p?.GetValue(obj);
        }

        private static IDictionary<string, object?>? AsStringKeyDictionary(object obj)
        {
            // IDictionary<string, T>
            var t = obj.GetType();

            // Quick path: non-generic IDictionary
            if (obj is IDictionary nonGeneric)
            {
                var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (DictionaryEntry entry in nonGeneric)
                {
                    if (entry.Key is null) continue;
                    dict[entry.Key.ToString() ?? string.Empty] = entry.Value;
                }
                return dict;
            }

            // Generic IDictionary<,>
            var idict = t.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

            if (idict is null) return null;

            var args = idict.GetGenericArguments();
            if (args.Length != 2) return null;

            // Key must be string (or convertible)
            if (args[0] != typeof(string)) return null;

            // enumerate via IEnumerable<KeyValuePair<string, TValue>>
            var enumerable = obj as IEnumerable;
            if (enumerable is null) return null;

            var dictOut = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in enumerable)
            {
                if (item is null) continue;
                var itemType = item.GetType();
                var keyProp = itemType.GetProperty("Key");
                var valProp = itemType.GetProperty("Value");
                if (keyProp is null || valProp is null) continue;

                var k = keyProp.GetValue(item) as string;
                var v = valProp.GetValue(item);

                if (k is not null)
                    dictOut[k] = v;
            }

            return dictOut;
        }

        private static IEnumerable<string> TryGetTopLevelKeys(object uiMap)
        {
            // Try: if uiMap itself is a dictionary (bad parse scenario)
            var dict = AsStringKeyDictionary(uiMap);
            return dict?.Keys ?? Array.Empty<string>();
        }

        private static bool IsMetaKey(string key) =>
            string.Equals(key, "__meta", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(key, "meta", StringComparison.OrdinalIgnoreCase);

        private static string? GetRouteFromPageDef(object pageDef)
        {
            // 1) Try to read Route property (if the page is deserialized into a typed object)
            var routeProp = GetProp(pageDef, "Route");
            if (routeProp is string s1) return s1;

            // 2) Try __meta.route via dictionary
            var pageDict = AsStringKeyDictionary(pageDef);
            if (pageDict != null && pageDict.TryGetValue("__meta", out var metaObj))
            {
                // meta can be dictionary-like or typed object
                if (metaObj is IDictionary metaDict)
                {
                    if (metaDict.Contains("route") && metaDict["route"] is string metaRoute)
                        return metaRoute;
                }

                var metaRouteProp = GetProp(metaObj, "route") ?? GetProp(metaObj, "Route");
                if (metaRouteProp is string s2) return s2;
            }

            return null;
        }
    }

    public sealed class UiMapValidationException : Exception
    {
        public UiMapValidationException(string message) : base(message) { }
    }
}
