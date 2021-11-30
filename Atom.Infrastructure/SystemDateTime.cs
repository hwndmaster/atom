using System.Diagnostics.CodeAnalysis;

namespace Genius.Atom.Infrastructure
{
    [ExcludeFromCodeCoverage]
    internal sealed class SystemDateTime : IDateTime
    {
        public DateTime Now => DateTime.Now;
    }
}
