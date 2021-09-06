using System;
using System.Collections.Generic;

namespace Genius.Atom.Infrastructure
{
    public static class Utils
    {
        private static readonly Random _rnd = new();

        public static bool RandomBool()
            => _rnd.NextDouble() >= 0.5;

        public static int RandomInt(int from, int to)
            => _rnd.Next(from, to);

        public static T TakeRandom<T>(this IList<T> list)
        {
            if (list.Count == 0)
                return default;
            return list[_rnd.Next(0, list.Count - 1)];
        }
    }
}