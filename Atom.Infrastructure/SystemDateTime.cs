using System.Diagnostics.CodeAnalysis;

namespace Genius.Atom.Infrastructure
{
    [ExcludeFromCodeCoverage]
    internal sealed class SystemDateTime : IDateTime
    {
        public DateTime Now => DateTime.Now;
        public DateTime NowUtc => DateTime.UtcNow;

        public DateTimeOffset NowOffset => DateTimeOffset.Now;

        public DateTimeOffset NowOffsetUtc => DateTimeOffset.UtcNow;
    }
}
