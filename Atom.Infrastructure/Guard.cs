using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Genius.Atom.Infrastructure;

public static class Guard
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNull<T>([NotNull] T? value,
        string? message = null,
        [CallerArgumentExpression(parameterName: nameof(value))] string? parameterName = null)
    {
        if (value is null)
            throw new ArgumentNullException(parameterName, message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNullOrEmpty(string value,
        [CallerArgumentExpression(parameterName: nameof(value))] string? parameterName = null)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(parameterName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNullOrWhitespace(string value,
        [CallerArgumentExpression(parameterName: nameof(value))] string? parameterName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(parameterName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNegative(int value, string? parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName);
    }
}

public static class GuardExtensions
{
    [return: NotNull]
    public static T NotNull<T>(this T value,
        string? message = null,
        [CallerArgumentExpression(parameterName: nameof(value))] string? parameterName = null)
    {
        Guard.NotNull(value, message, parameterName);

        return value!;
    }
}
