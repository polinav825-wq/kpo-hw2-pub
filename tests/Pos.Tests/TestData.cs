using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Tests.Fakes;

namespace Pos.Tests;

internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);

    /// <summary>ДЗ (10, вес 0.4), Контрольная (10, вес 0.3), Экзамен (100, вес 0.3).</summary>
    public static Rubric StandardRubric() => new RubricBuilder()
        .WithCriterion("ДЗ", 10, 0.4m)
        .WithCriterion("Контрольная", 10, 0.3m)
        .WithCriterion("Экзамен", 100, 0.3m)
        .Build();

    public static Student NewStudent(string name = "Анна") => new(new StudentName(name));
    public static EvaluationFactory Factory() => new(new FixedTimeProvider(Now));
}
