namespace Pos.Domain.Rubrics;

/// <summary>Идентификатор критерия (Value Object).</summary>
public readonly record struct CriterionId(Guid Value)
{
    public static CriterionId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
