using Pos.Domain.Common;
using Pos.Domain.Rubrics;

namespace Pos.Tests;

public class RubricTests
{
    [Fact]
    public void Builder_BuildsAllCriteria()
    {
        var rubric = TestData.StandardRubric();

        Assert.Equal(3, rubric.Criteria.Count);
        Assert.Equal(1.0m, rubric.TotalWeight);
    }

    [Fact]
    public void Builder_Empty_Throws()
    {
        Assert.Throws<DomainException>(() => new RubricBuilder().Build());
    }

    [Fact]
    public void Builder_WithInvalidCriterion_Throws()
    {
        var builder = new RubricBuilder()
            .WithCriterion("ДЗ", 10, 0.5m)
            .WithCriterion("Экзамен", 0, 0.5m);

        Assert.Throws<DomainException>(() => builder.Build());
    }

    [Fact]
    public void AddCriterion_DuplicateNameIgnoringCase_Throws()
    {
        var rubric = TestData.StandardRubric();

        Assert.Throws<DomainException>(() => rubric.AddCriterion("  дз ", 5, 0.1m));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(10, 0)]
    public void AddCriterion_InvalidMaxOrWeight_Throws(int maxScore, int weight)
    {
        var rubric = new Rubric();

        Assert.Throws<DomainException>(() => rubric.AddCriterion("Критерий", maxScore, weight));
        Assert.True(rubric.IsEmpty);
    }

    [Fact]
    public void GetCriterion_Unknown_Throws()
    {
        var rubric = TestData.StandardRubric();

        Assert.Throws<DomainException>(() => rubric.GetCriterion(CriterionId.New()));
    }

    [Fact]
    public void RemoveCriterion_RemovesIt()
    {
        var rubric = TestData.StandardRubric();
        var exam = rubric.Criteria[2];

        rubric.RemoveCriterion(exam.Id);

        Assert.False(rubric.Contains(exam.Id));
        Assert.Equal(2, rubric.Criteria.Count);
    }
}
