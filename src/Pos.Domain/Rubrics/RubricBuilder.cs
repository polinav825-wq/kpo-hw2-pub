using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

/// <summary>
/// Builder (порождающий GoF): собирает рубрикатор целиком и отдаёт его только полностью корректным.
/// Если хоть один критерий плохой или рубрикатор пуст — наружу не попадает «полусобранный» объект.
/// </summary>
public sealed class RubricBuilder
{
    private readonly List<(string Name, decimal MaxScore, decimal Weight)> drafts = new();
    public RubricBuilder WithCriterion(string name, decimal maxScore, decimal weight)
    {
        drafts.Add((name, maxScore, weight));
        return this;
    }

    public Rubric Build()
    {
        if (drafts.Count == 0)
        {
            throw new DomainException("Рубрикатор должен содержать хотя бы один критерий.");
        }

        var rubric = new Rubric();
        foreach (var (name, maxScore, weight) in drafts)
        {
            rubric.AddCriterion(name, maxScore, weight);
        }

        return rubric;
    }
}
