using Pos.Domain.Common;

namespace Pos.Domain.Students;

/// <summary>ФИО студента (Value Object): непустое, без лишних пробелов, разумной длины.</summary>
public sealed record StudentName
{
    public const int MaxLength = 100;
    public StudentName(string value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length == 0)
        {
            throw new DomainException("Имя студента не может быть пустым.");
        }

        if (normalized.Length > MaxLength)
        {
            throw new DomainException($"Имя студента длиннее {MaxLength} символов.");
        }

        Value = normalized;
    }

    public string Value { get; }
    public override string ToString() => Value;
}
