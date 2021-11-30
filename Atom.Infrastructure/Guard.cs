using System.Diagnostics.CodeAnalysis;

namespace Genius.Atom.Infrastructure;

public static class Guard
{
    public static void AgainstNull([NotNull] object? parameterValue, string? parameterName = null)
    {
        if (parameterValue is null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }
}

public static class GuardExtensions
{
    [return: NotNull]
    public static T NotNull<T>(this T parameterValue, string? parameterName = null)
    {
        Guard.AgainstNull(parameterValue, parameterName);

        return parameterValue;
    }
}
