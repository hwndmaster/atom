namespace Genius.Atom.Infrastructure.Linq;

// NOTE: Originally taken from https://raw.githubusercontent.com/fsateler/MoreLINQ/master/MoreLinq/FullJoin.cs

public static class FullJoinExtensions
{
    /// <summary>
    ///   Performs a full outer join on two homogeneous sequences.
    ///   Additional arguments specify key selection functions and result projection functions.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="first">The first sequence to join fully.</param>
    /// <param name="second">The second sequence to join fully.</param>
    /// <param name="keySelector">Function that projects the key given an element of one of the sequences to join.</param>
    /// <param name="firstSelector">
    ///   Function that projects the result given just an element from
    ///   <paramref name="first"/> where there is no corresponding element in <paramref name="second"/>.
    /// </param>
    /// <param name="secondSelector">
    ///   Function that projects the result given just an element from
    ///   <paramref name="second"/> where there is no corresponding element in <paramref name="first"/>.
    /// </param>
    /// <param name="bothSelector">
    ///   Function that projects the result given an element from
    ///   <paramref name="first"/> and an element from <paramref name="second"/> that match on a common key.
    /// </param>
    /// <returns>A sequence containing results projected from a full outer join of the two input sequences.</returns>
    public static IEnumerable<TResult> FullJoin<TSource, TKey, TResult>(
        this IEnumerable<TSource> first,
        IEnumerable<TSource> second,
        Func<TSource, TKey> keySelector,
        Func<TSource, TResult> firstSelector,
        Func<TSource, TResult> secondSelector,
        Func<TSource, TSource, TResult> bothSelector)
    {
        return first.FullJoin(second, keySelector.NotNull(),
            firstSelector, secondSelector, bothSelector,
            null);
    }

    /// <summary>
    ///   Performs a full outer join on two homogeneous sequences.
    ///   Additional arguments specify key selection functions, result projection functions and a key comparer.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the key selector function.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="first">The first sequence to join fully.</param>
    /// <param name="second">The second sequence to join fully.</param>
    /// <param name="keySelector">Function that projects the key given an element of one of the sequences to join.</param>
    /// <param name="firstSelector">
    ///   Function that projects the result given just an element from
    ///   <paramref name="first"/> where there is no corresponding element in <paramref name="second"/>.
    /// </param>
    /// <param name="secondSelector">
    ///   Function that projects the result given just an element from
    ///   <paramref name="second"/> where there is no corresponding element in <paramref name="first"/>.
    /// </param>
    /// <param name="bothSelector">
    ///   Function that projects the result given an element from
    ///   <paramref name="first"/> and an element from <paramref name="second"/> that match on a common key.
    /// </param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> instance used to compare keys for equality.</param>
    /// <returns>A sequence containing results projected from a full outer join of the two input sequences.</returns>
    public static IEnumerable<TResult> FullJoin<TSource, TKey, TResult>(
        this IEnumerable<TSource> first,
        IEnumerable<TSource> second,
        Func<TSource, TKey> keySelector,
        Func<TSource, TResult> firstSelector,
        Func<TSource, TResult> secondSelector,
        Func<TSource, TSource, TResult> bothSelector,
        IEqualityComparer<TKey>? comparer)
    {
        Guard.NotNull(keySelector);

        return first.FullJoin(second,
            keySelector, keySelector,
            firstSelector, secondSelector, bothSelector,
            comparer);
    }

    /// <summary>
    ///   Performs a full outer join on two heterogeneous sequences.
    ///   Additional arguments specify key selection functions and result
    ///   projection functions.
    /// </summary>
    /// <typeparam name="TFirst">The type of elements in the first sequence.</typeparam>
    /// <typeparam name="TSecond">The type of elements in the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="first">The first sequence to join fully.</param>
    /// <param name="second">The second sequence to join fully.</param>
    /// <param name="firstKeySelector">Function that projects the key given an element from <paramref name="first"/>.</param>
    /// <param name="secondKeySelector">Function that projects the key given an element from <paramref name="second"/>.</param>
    /// <param name="firstSelector">
    ///   Function that projects the result given just an element from
    ///   <paramref name="first"/> where there is no corresponding element in <paramref name="second"/>.
    /// </param>
    /// <param name="secondSelector">
    ///   Function that projects the result given just an element from
    ///   <paramref name="second"/> where there is no corresponding element in <paramref name="first"/>.
    /// </param>
    /// <param name="bothSelector">
    ///   Function that projects the result given an element from
    ///   <paramref name="first"/> and an element from <paramref name="second"/> that match on a common key.
    /// </param>
    /// <returns>A sequence containing results projected from a full outer join of the two input sequences.</returns>
    public static IEnumerable<TResult> FullJoin<TFirst, TSecond, TKey, TResult>(
        this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second,
        Func<TFirst, TKey> firstKeySelector,
        Func<TSecond, TKey> secondKeySelector,
        Func<TFirst, TResult> firstSelector,
        Func<TSecond, TResult> secondSelector,
        Func<TFirst, TSecond, TResult> bothSelector) =>
        first.FullJoin(second,
            firstKeySelector, secondKeySelector,
            firstSelector, secondSelector, bothSelector,
            null);

    /// <summary>
    ///   Performs a full outer join on two heterogeneous sequences.
    ///   Additional arguments specify key selection functions, result projection functions and a key comparer.
    /// </summary>
    /// <typeparam name="TFirst">The type of elements in the first sequence.</typeparam>
    /// <typeparam name="TSecond">The type of elements in the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="first">The first sequence to join fully.</param>
    /// <param name="second">The second sequence to join fully.</param>
    /// <param name="firstKeySelector">Function that projects the key given an element from <paramref name="first"/>.</param>
    /// <param name="secondKeySelector">Function that projects the key given an element from <paramref name="second"/>.</param>
    /// <param name="firstSelector">
    ///   Function that projects the result given just an element from
    ///   <paramref name="first"/> where there is no corresponding element in <paramref name="second"/>.
    /// </param>
    /// <param name="secondSelector">
    ///   Function that projects the result given just an element from
    ///   <paramref name="second"/> where there is no corresponding element in <paramref name="first"/>.
    /// </param>
    /// <param name="bothSelector">
    ///   Function that projects the result given an element from
    ///   <paramref name="first"/> and an element from <paramref name="second"/> that match on a common key.
    /// </param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> instance used to compare keys for equality.</param>
    /// <returns>A sequence containing results projected from a full outer join of the two input sequences.</returns>
    public static IEnumerable<TResult> FullJoin<TFirst, TSecond, TKey, TResult>(
        this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second,
        Func<TFirst, TKey> firstKeySelector,
        Func<TSecond, TKey> secondKeySelector,
        Func<TFirst, TResult> firstSelector,
        Func<TSecond, TResult> secondSelector,
        Func<TFirst, TSecond, TResult> bothSelector,
        IEqualityComparer<TKey>? comparer)
    {
        Guard.NotNull(first);
        Guard.NotNull(second);
        Guard.NotNull(firstKeySelector);
        Guard.NotNull(secondKeySelector);
        Guard.NotNull(firstSelector);
        Guard.NotNull(secondSelector);
        Guard.NotNull(bothSelector);

        var seconds = second.Select(e => new KeyValuePair<TKey, TSecond>(secondKeySelector(e), e)).ToArray();
        var secondLookup = seconds.ToLookup(e => e.Key, e => e.Value, comparer);
        var firstKeys = new HashSet<TKey>(comparer);

        foreach (var fe in first)
        {
            var key = firstKeySelector(fe);
            firstKeys.Add(key);

            using var se = secondLookup[key].GetEnumerator();
            if (se.MoveNext())
            {
                do { yield return bothSelector(fe, se.Current); }
                while (se.MoveNext());
            }
            else
            {
                yield return firstSelector(fe);
            }
        }

        foreach (var se in seconds)
        {
            if (!firstKeys.Contains(se.Key))
                yield return secondSelector(se.Value);
        }
    }
}
