using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

/// <summary>
/// Критерий — за что ставят балл (Entity внутри агрегата Rubric).
/// Создаётся только через рубрикатор, поэтому «критерий вне рубрикатора» не появится.
/// </summary>
public sealed class Criterion
{
    internal Criterion(CriterionId id, CriterionName name, decimal maxScore, Weight weight)
    {
        if (maxScore <= 0)
        {
            throw new DomainException($"Максимальный балл критерия должен быть положительным, получено {maxScore}.");
        }
        Id = id;
        Name = name;
        MaxScore = maxScore;
        Weight = weight;
    }
    public CriterionId Id { get; }
    public CriterionName Name { get; }
    public decimal MaxScore { get; }
    public Weight Weight { get; }

    /// <summary>Балл по этому критерию; вне диапазона [0; MaxScore] — DomainException.</summary>
    public Score ScoreOf(decimal value) => new(value, MaxScore);
}
