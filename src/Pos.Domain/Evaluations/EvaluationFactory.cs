using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Domain.Evaluations;

/// <summary>
/// Factory (порождающий GoF): единственная точка создания оценки.
/// Проверяет оба правила домена: критерий есть в рубрикаторе и балл в его диапазоне.
/// </summary>
public sealed class EvaluationFactory
{
    private readonly TimeProvider clock;

    public EvaluationFactory(TimeProvider clock)
    {
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public Evaluation Create(Student student, Rubric rubric, CriterionId criterionId, decimal value, string? comment = null)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(rubric);

        var criterion = rubric.GetCriterion(criterionId);
        var score = criterion.ScoreOf(value);

        return new Evaluation(EvaluationId.New(), student.Id, criterion.Id, score, clock.GetUtcNow(), comment);
    }
}
