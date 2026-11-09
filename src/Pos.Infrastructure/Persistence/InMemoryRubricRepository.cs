using Pos.Domain.Rubrics;

namespace Pos.Infrastructure.Persistence;

/// <summary>Адаптер порта IRubricRepository: один рубрикатор курса в памяти.</summary>
public sealed class InMemoryRubricRepository : IRubricRepository
{
    private Rubric rubric = new();
    public Rubric Get() => rubric;
    public void Save(Rubric rubric)
    {
        this.rubric = rubric ?? throw new ArgumentNullException(nameof(rubric));
    }
}
