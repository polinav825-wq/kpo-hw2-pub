namespace Pos.Domain.Evaluations;

/// <summary>Идентификатор оценки (Value Object).</summary>
public readonly record struct EvaluationId(Guid Value)
{
    public static EvaluationId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
