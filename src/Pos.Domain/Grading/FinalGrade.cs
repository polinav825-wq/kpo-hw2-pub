using System.Globalization;
using Pos.Domain.Common;
using Pos.Domain.Students;

namespace Pos.Domain.Grading;

/// <summary>Итог по студенту по 10-балльной шкале (Value Object). Не хранится — всегда вычисляется.</summary>
public sealed record FinalGrade
{
    public const decimal MaxValue = 10m;

    public FinalGrade(StudentId studentId, decimal value, string policyTitle)
    {
        if (value < 0 || value > MaxValue)
        {
            throw new DomainException(string.Format(
                CultureInfo.InvariantCulture,
                "Итог {0} вне шкалы [0; {1}].",
                value,
                MaxValue));
        }

        StudentId = studentId;
        Value = value;
        PolicyTitle = policyTitle;
    }

    public StudentId StudentId { get; }
    public decimal Value { get; }
    public string PolicyTitle { get; }
}
