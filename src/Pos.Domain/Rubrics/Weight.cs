using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

/// <summary>
/// Вклад критерия в итог (Value Object). Должен быть положительным;
/// веса не обязаны давать в сумме 1 — расчёт итога нормирует их сам.
/// </summary>
public sealed record Weight
{
    public Weight(decimal value)
    {
        if (value <= 0)
        {
            throw new DomainException($"Вес критерия должен быть положительным, получено {value}.");
        }

        Value = value;
    }

    public decimal Value { get; }
    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
