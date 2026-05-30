using System.Diagnostics.CodeAnalysis;

namespace Genius.Atom.Infrastructure;

public static class Randomizer
{
    private static readonly Random _rnd = new();

    public static bool RandomBool()
        => _rnd.NextDouble() >= 0.5;

    public static int RandomInt(int from, int to)
        => _rnd.Next(from, to);

    extension<T>(IList<T>? list)
    {
        [return: MaybeNull]
        public T TakeRandom()
        {
            if (list is null)
            {
                return default;
            }
            if (list.Count == 0)
                return default;
            return list[_rnd.Next(0, list.Count - 1)];
        }
    }
}
