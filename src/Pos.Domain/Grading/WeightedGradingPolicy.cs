using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Grading;

/// <summary>
/// Итоговая оценка: взвешенная доля по всем критериям рубрикатора.
/// Невыставленный критерий считается нулём — как в итоговой ведомости.
/// </summary>
public sealed class WeightedGradingPolicy : IGradingPolicy
{
    public string Code => "weighted";

    public string Title => "Итоговая (все критерии, пропуск = 0)";

    public decimal Calculate(Rubric rubric, IReadOnlyCollection<Evaluation> evaluations)
    {
        if (rubric.IsEmpty)
        {
            return 0m;
        }

        var byCriterion = evaluations.ToDictionary(e => e.CriterionId);
        var weighted = rubric.Criteria
            .Where(c => byCriterion.ContainsKey(c.Id))
            .Sum(c => c.Weight.Value * byCriterion[c.Id].Score.Ratio);

        return FinalGrade.MaxValue * weighted / rubric.TotalWeight;
    }
}
