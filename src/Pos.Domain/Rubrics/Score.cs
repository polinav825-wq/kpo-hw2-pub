using System.Globalization;
using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

/// <summary>
/// Балл по критерию (Value Object). Хранит и значение, и максимум критерия,
/// поэтому «12 из 10» или отрицательный балл просто нельзя создать.
/// </summary>
public sealed record Score
{
    public Score(decimal value, decimal max)
    {
        if (max <= 0)
        {
            throw new DomainException($"Максимальный балл должен быть положительным, получено {max}.");
        }
        if (value < 0 || value > max)
        {
            throw new DomainException(string.Format(
                CultureInfo.InvariantCulture,
                "Балл {0} вне диапазона критерия [0; {1}].",
                value,
                max));
        }

        Value = value;
        Max = max;
    }

    public decimal Value { get; }
    public decimal Max { get; }

    /// <summary>Доля от максимума, 0..1.</summary>
    public decimal Ratio => Value / Max;
    /// <summary>Тот же критерий, другой балл — с той же проверкой диапазона.</summary>
    public Score WithValue(decimal newValue) => new(newValue, Max);
    public override string ToString() =>
        string.Format(CultureInfo.InvariantCulture, "{0}/{1}", Value, Max);
}
