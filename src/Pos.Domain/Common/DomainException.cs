namespace Pos.Domain.Common;

/// <summary>
/// Нарушение правила предметной области: балл вне критерия, критерия нет в рубрикаторе и т. п.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
