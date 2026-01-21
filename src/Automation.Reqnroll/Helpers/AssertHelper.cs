using System;

namespace Automation.Reqnroll.Helpers;

/// <summary>
/// Helper interno para asserts sem expor dependência de xUnit.
/// Lança AssertionException em caso de falha.
/// </summary>
public static class AssertHelper
{
    /// <summary>
    /// Verifica se a condição é verdadeira.
    /// </summary>
    public static void True(bool condition, string? message = null)
    {
        if (!condition)
            throw new AssertionException(message ?? "Expected condition to be true, but was false.");
    }

    /// <summary>
    /// Verifica se a condição é falsa.
    /// </summary>
    public static void False(bool condition, string? message = null)
    {
        if (condition)
            throw new AssertionException(message ?? "Expected condition to be false, but was true.");
    }

    /// <summary>
    /// Verifica se dois valores são iguais.
    /// </summary>
    public static void Equal<T>(T expected, T actual, string? message = null)
    {
        if (!Equals(expected, actual))
            throw new AssertionException(message ?? $"Expected: {expected}, Actual: {actual}");
    }

    /// <summary>
    /// Verifica se uma string contém outra (case-insensitive por padrão).
    /// </summary>
    public static void Contains(string expectedSubstring, string actualString, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        if (actualString == null || !actualString.Contains(expectedSubstring, comparison))
            throw new AssertionException($"Expected string to contain '{expectedSubstring}', but was '{actualString ?? "(null)"}'");
    }

    /// <summary>
    /// Verifica se uma string NÃO contém outra (case-insensitive por padrão).
    /// </summary>
    public static void DoesNotContain(string notExpectedSubstring, string actualString, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        if (actualString != null && actualString.Contains(notExpectedSubstring, comparison))
            throw new AssertionException($"Expected string NOT to contain '{notExpectedSubstring}', but was '{actualString}'");
    }

    /// <summary>
    /// Falha incondicionalmente com uma mensagem.
    /// </summary>
    public static void Fail(string message)
    {
        throw new AssertionException(message);
    }
}

/// <summary>
/// Exceção lançada quando um assert falha.
/// </summary>
public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
    public AssertionException(string message, Exception innerException) : base(message, innerException) { }
}
