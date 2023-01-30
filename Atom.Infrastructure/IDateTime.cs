namespace Genius.Atom.Infrastructure;

public interface IDateTime
{
    DateTime Now { get; }
    DateTime NowUtc { get; }
    DateTimeOffset NowOffset { get; }
    DateTimeOffset NowOffsetUtc { get; }
}
