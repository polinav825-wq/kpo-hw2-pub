using Pos.Domain.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Domain.Grading;

/// <summary>
/// Domain Service: итог зависит сразу от студента, рубрикатора и набора оценок —
/// это правило не принадлежит ни одной сущности. Итог всегда считается из текущих оценок
/// и нигде не хранится, поэтому не может «разойтись» с ними.
/// </summary>
public sealed class FinalGradeCalculator
{
    public FinalGrade Calculate(Student student, Rubric rubric, IEnumerable<Evaluation> evaluations, IGradingPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(rubric);
        ArgumentNullException.ThrowIfNull(evaluations);
        ArgumentNullException.ThrowIfNull(policy);

        var own = evaluations.Where(e => e.StudentId == student.Id).ToList();

        if (own.Any(e => !rubric.Contains(e.CriterionId)))
        {
            throw new DomainException($"У студента {student.Name} есть оценка по критерию вне рубрикатора.");
        }
        if (own.GroupBy(e => e.CriterionId).Any(g => g.Count() > 1))
        {
            throw new DomainException($"У студента {student.Name} несколько оценок по одному критерию.");
        }
        return new FinalGrade(student.Id, policy.Calculate(rubric, own), policy.Title);
    }
}
