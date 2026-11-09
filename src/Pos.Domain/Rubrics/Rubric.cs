using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

/// <summary>
/// Рубрикатор — корень агрегата с критериями оценивания.
/// Инварианты: названия критериев уникальны, у каждого положительные максимум и вес.
/// </summary>
public sealed class Rubric
{
    private readonly List<Criterion> criteria = new();
    public IReadOnlyList<Criterion> Criteria => criteria;
    public decimal TotalWeight => criteria.Sum(c => c.Weight.Value);
    public bool IsEmpty => criteria.Count == 0;
    public Criterion AddCriterion(string name, decimal maxScore, decimal weight)
    {
        var criterionName = new CriterionName(name);
        if (criteria.Any(c => c.Name.SameAs(criterionName)))
        {
            throw new DomainException($"Критерий «{criterionName}» уже есть в рубрикаторе.");
        }
        var criterion = new Criterion(CriterionId.New(), criterionName, maxScore, new Weight(weight));
        criteria.Add(criterion);
        return criterion;
    }

    public void RemoveCriterion(CriterionId id)
    {
        criteria.Remove(GetCriterion(id));
    }
    public bool Contains(CriterionId id) => criteria.Any(c => c.Id == id);

    /// <summary>Критерий из рубрикатора; если его нет — DomainException.</summary>
    public Criterion GetCriterion(CriterionId id) =>
        criteria.FirstOrDefault(c => c.Id == id)
        ?? throw new DomainException("Такого критерия нет в рубрикаторе.");
}
