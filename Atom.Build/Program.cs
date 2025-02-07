using System;
using System.Reflection;

namespace Genius.Atom.Build;

static class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version?.ToString());
    }
}
