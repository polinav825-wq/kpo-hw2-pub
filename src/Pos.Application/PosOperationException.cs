namespace Pos.Application;

/// <summary>Нарушение правила сценария: объект не найден, оценка уже выставлена и т. п.</summary>
public sealed class PosOperationException : Exception
{
    public PosOperationException(string message)
        : base(message)
    {
    }
}
