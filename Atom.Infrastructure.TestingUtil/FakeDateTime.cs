namespace Genius.Atom.Infrastructure.TestingUtil;

public sealed class FakeDateTime : IDateTime
{
    private DateTime _clock;

    public FakeDateTime()
    {
        SetClock(DateTime.Now);
    }

    public FakeDateTime(DateTime clock)
    {
        SetClock(clock);
    }

    public void SetClock(DateTime clock)
    {
        _clock = clock;
    }

    public DateTime Now => _clock;
    public DateTime NowUtc => _clock.ToUniversalTime();

    public DateTimeOffset NowOffset => new(_clock);

    public DateTimeOffset NowOffsetUtc => new(_clock.ToUniversalTime());
}
