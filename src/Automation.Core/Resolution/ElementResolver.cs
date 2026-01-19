using System;
using Automation.Core.UiMap;

namespace Automation.Core.Resolution;

/// <summary>
/// Resultado da resolução de um elemento.
/// </summary>
public sealed record ResolutionResult(
    string PageName, 
    string FriendlyName, 
    string TestId, 
    string CssLocator);

/// <summary>
/// Resolve nomes amigáveis de elementos para seletores CSS.
/// Suporta dois formatos:
/// - Nome simples: "username" (usa página do contexto atual)
/// - Nome completo: "login.username" (especifica página explicitamente)
/// </summary>
public sealed class ElementResolver
{
    private readonly UiMapModel _map;
    private readonly PageContext _pageContext;

    public ElementResolver(UiMapModel map, PageContext pageContext)
    {
        _map = map;
        _pageContext = pageContext;
    }

    /// <summary>
    /// Resolve um nome amigável para um seletor CSS.
    /// Suporta:
    /// - "username" → usa página do contexto atual
    /// - "login.username" → usa página "login" explicitamente
    /// - "anchor" → resolve para o anchor da página atual
    /// </summary>
    public ResolutionResult Resolve(string elementRef)
    {
        if (string.IsNullOrWhiteSpace(elementRef))
            throw new ArgumentException("Nome do elemento não pode ser vazio.", nameof(elementRef));

        string pageName;
        string friendlyName;

        // Verifica se é formato "page.element"
        if (elementRef.Contains('.'))
        {
            var parts = elementRef.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new ArgumentException(
                    $"Formato de elemento inválido: '{elementRef}'. " +
                    "Use 'elemento' ou 'pagina.elemento'.");

            pageName = parts[0];
            friendlyName = parts[1];
        }
        else
        {
            // Nome simples: usa contexto de página atual
            pageName = _pageContext.GetCurrentPageOrThrow();
            friendlyName = elementRef;
        }

        var page = _map.GetPageOrThrow(pageName);
        string testId;

        // Verifica se é o anchor da página
        if (string.Equals(friendlyName, "anchor", StringComparison.OrdinalIgnoreCase))
        {
            testId = page.Anchor 
                ?? throw new InvalidOperationException(
                    $"Página '{pageName}' não possui anchor definido no ui-map.yaml.");
        }
        else
        {
            testId = page.GetTestIdOrThrow(friendlyName);
        }

        var css = $"[data-testid='{testId}']";
        return new ResolutionResult(pageName, friendlyName, testId, css);
    }

    /// <summary>
    /// Tenta resolver um elemento, retornando null se não encontrado.
    /// </summary>
    public ResolutionResult? TryResolve(string elementRef)
    {
        try
        {
            return Resolve(elementRef);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Verifica se um elemento existe no ui-map.
    /// </summary>
    public bool Exists(string elementRef)
    {
        return TryResolve(elementRef) != null;
    }
}
