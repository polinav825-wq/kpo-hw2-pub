using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

/// <summary>Название критерия (Value Object).</summary>
public sealed record CriterionName
{
    public const int MaxLength = 100;
    public CriterionName(string value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length == 0)
        {
            throw new DomainException("Название критерия не может быть пустым.");
        }

        if (normalized.Length > MaxLength)
        {
            throw new DomainException($"Название критерия длиннее {MaxLength} символов.");
        }

        Value = normalized;
    }

    public string Value { get; }
    public bool SameAs(CriterionName other) =>
        string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => Value;
}
