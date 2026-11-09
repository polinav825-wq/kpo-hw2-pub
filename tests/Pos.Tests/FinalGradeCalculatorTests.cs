using Pos.Domain.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;

namespace Pos.Tests;

public class FinalGradeCalculatorTests
{
    private readonly FinalGradeCalculator calculator = new();
    private readonly Rubric rubric = TestData.StandardRubric();
    private readonly EvaluationFactory factory = TestData.Factory();

    [Fact]
    public void Weighted_AllCriteria_UsesWeights()
    {
        var student = TestData.NewStudent();
        var evaluations = new[]
        {
            factory.Create(student, rubric, rubric.Criteria[0].Id, 9),  // 0.4 * 0.9
            factory.Create(student, rubric, rubric.Criteria[1].Id, 8),  // 0.3 * 0.8
            factory.Create(student, rubric, rubric.Criteria[2].Id, 85), // 0.3 * 0.85
        };

        var grade = calculator.Calculate(student, rubric, evaluations, new WeightedGradingPolicy());

        Assert.Equal(8.55m, grade.Value);
    }

    [Fact]
    public void Weighted_MissingCriterion_CountsAsZero()
    {
        var student = TestData.NewStudent();
        var evaluations = new[] { factory.Create(student, rubric, rubric.Criteria[0].Id, 10) };

        var grade = calculator.Calculate(student, rubric, evaluations, new WeightedGradingPolicy());

        Assert.Equal(4m, grade.Value);
    }

    [Fact]
    public void EvaluatedOnly_IgnoresMissingCriteria()
    {
        var student = TestData.NewStudent();
        var evaluations = new[] { factory.Create(student, rubric, rubric.Criteria[0].Id, 10) };

        var grade = calculator.Calculate(student, rubric, evaluations, new EvaluatedOnlyGradingPolicy());

        Assert.Equal(10m, grade.Value);
    }

    [Fact]
    public void NoEvaluations_GradeIsZero()
    {
        var student = TestData.NewStudent();

        Assert.Equal(0m, calculator.Calculate(student, rubric, Array.Empty<Evaluation>(), new WeightedGradingPolicy()).Value);
        Assert.Equal(0m, calculator.Calculate(student, rubric, Array.Empty<Evaluation>(), new EvaluatedOnlyGradingPolicy()).Value);
    }

    [Fact]
    public void OtherStudentsEvaluations_AreIgnored()
    {
        var anna = TestData.NewStudent("Анна");
        var boris = TestData.NewStudent("Борис");
        var evaluations = new[]
        {
            factory.Create(anna, rubric, rubric.Criteria[0].Id, 10),
            factory.Create(boris, rubric, rubric.Criteria[1].Id, 10),
        };

        var grade = calculator.Calculate(anna, rubric, evaluations, new WeightedGradingPolicy());

        Assert.Equal(4m, grade.Value);
    }

    [Fact]
    public void EvaluationOutsideRubric_Throws()
    {
        var student = TestData.NewStudent();
        var otherRubric = TestData.StandardRubric();
        var foreign = factory.Create(student, otherRubric, otherRubric.Criteria[0].Id, 5);

        Assert.Throws<DomainException>(() =>
            calculator.Calculate(student, rubric, new[] { foreign }, new WeightedGradingPolicy()));
    }

    [Theory]
    [InlineData(0, 3.0)]
    [InlineData(1, 3.3)]
    [InlineData(2, 3.33)]
    public void RoundingDecorator_RoundsInnerPolicy(int decimals, double expected)
    {
        var thirds = new RubricBuilder().WithCriterion("Работа", 3, 1).Build();
        var student = TestData.NewStudent();
        var evaluations = new[] { factory.Create(student, thirds, thirds.Criteria[0].Id, 1) }; // 10 * 1/3

        var policy = new RoundingGradingPolicy(new WeightedGradingPolicy(), decimals);
        var grade = calculator.Calculate(student, thirds, evaluations, policy);

        Assert.Equal((decimal)expected, grade.Value);
        Assert.Equal("weighted", policy.Code);
    }

    [Fact]
    public void RoundingDecorator_HalfRoundsUp()
    {
        var rubric = new RubricBuilder().WithCriterion("Работа", 20, 1).Build();
        var student = TestData.NewStudent();
        var evaluations = new[] { factory.Create(student, rubric, rubric.Criteria[0].Id, 15) }; // 7.5

        var grade = calculator.Calculate(student, rubric, evaluations, new RoundingGradingPolicy(new WeightedGradingPolicy(), 0));

        Assert.Equal(8m, grade.Value);
    }
}
