using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Grading;

/// <summary>
/// Strategy (поведенческий GoF): правило, по которому из оценок и рубрикатора получается итог 0..10.
/// </summary>
public interface IGradingPolicy
{
    /// <summary>Короткий код для выбора в меню, например "weighted".</summary>
    string Code { get; }
    string Title { get; }
    /// <param name="evaluations">Оценки одного студента, не больше одной на критерий.</param>
    decimal Calculate(Rubric rubric, IReadOnlyCollection<Evaluation> evaluations);
}
