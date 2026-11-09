using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Grading;

/// <summary>
/// Текущая оценка: взвешенная доля только по уже выставленным критериям.
/// Удобно посреди семестра, когда часть критериев ещё не наступила.
/// </summary>
public sealed class EvaluatedOnlyGradingPolicy : IGradingPolicy
{
    public string Code => "current";

    public string Title => "Текущая (только выставленные критерии)";

    public decimal Calculate(Rubric rubric, IReadOnlyCollection<Evaluation> evaluations)
    {
        var byCriterion = evaluations.ToDictionary(e => e.CriterionId);
        var graded = rubric.Criteria.Where(c => byCriterion.ContainsKey(c.Id)).ToList();
        var totalWeight = graded.Sum(c => c.Weight.Value);
        if (totalWeight == 0)
        {
            return 0m;
        }

        var weighted = graded.Sum(c => c.Weight.Value * byCriterion[c.Id].Score.Ratio);
        return FinalGrade.MaxValue * weighted / totalWeight;
    }
}
