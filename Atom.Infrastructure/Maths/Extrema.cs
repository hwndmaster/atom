using System.Numerics;

namespace Genius.Atom.Infrastructure.Maths;

/// <summary>
///   Represents an extreme values of a range, which also known as Minima and Maxima.
/// </summary>
/// <param name="Minimum">The lower value of the range.</param>
/// <param name="Maximum">The higher value of the range.</param>
/// <typeparam name="T">The type of the value.</typeparam>
public record Extrema<T>(T Minimum, T Maximum)
    where T: IComparable<T>;

/// <summary>
///   Represents an extreme values of a range, which also known as Minima and Maxima.
///   Also includes a count of the values.
/// </summary>
/// <param name="Minimum">The lower value of the range.</param>
/// <param name="Maximum">The higher value of the range.</param>
/// <param name="Count">The count of the values.</param>
/// <typeparam name="T">The type of the value.</typeparam>
public record ExtremaWithCount<T>(T Minimum, T Maximum, int Count) : Extrema<T>(Minimum, Maximum)
    where T: IComparable<T>;

public static class ExtremaExtensions
{
    public static Extrema<TSource> Extrema<TSource>(this IEnumerable<TSource> source)
        where TSource : struct, IComparable<TSource>
    {
        Guard.NotNull(source);

        TSource? min = null, max = null;
        foreach (var item in source)
        {
            if (min is null || min.Value.CompareTo(item) > 0)
            {
                min = item;
            }
            if (max is null || max.Value.CompareTo(item) < 0)
            {
                max = item;
            }
        }

        if (min is null || max is null)
        {
            throw new InvalidOperationException("Source contains no elements");
        }

        return new Extrema<TSource>(min.Value, max.Value);
    }

    public static ExtremaWithCount<TSource> ExtremaAndCount<TSource>(this IEnumerable<TSource> source)
        where TSource : struct, IComparable<TSource>
    {
        Guard.NotNull(source);

        int count = 0;
        TSource? min = null, max = null;
        foreach (var item in source)
        {
            if (min is null || min.Value.CompareTo(item) > 0)
                min = item;
            if (max is null || max.Value.CompareTo(item) < 0)
                max = item;

            count++;
        }

        if (min is null || max is null)
        {
            throw new InvalidOperationException("Source contains no elements");
        }

        return new ExtremaWithCount<TSource>(min.Value, max.Value, count);
    }
}
