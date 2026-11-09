namespace Pos.Tests.Fakes;

/// <summary>Часы, которые всегда показывают заданное время, — для предсказуемых тестов.</summary>
public sealed class FixedTimeProvider : TimeProvider
{
    private DateTimeOffset now;
    public FixedTimeProvider(DateTimeOffset now)
    {
        this.now = now;
    }
    public override DateTimeOffset GetUtcNow() => now;
    public void Advance(TimeSpan delta) => now += delta;
}
