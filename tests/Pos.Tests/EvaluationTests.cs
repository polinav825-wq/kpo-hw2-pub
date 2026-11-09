using Pos.Domain.Common;
using Pos.Domain.Rubrics;

namespace Pos.Tests;

public class EvaluationTests
{
    [Fact]
    public void Factory_ValidScore_CreatesEvaluation()
    {
        var rubric = TestData.StandardRubric();
        var student = TestData.NewStudent();
        var homework = rubric.Criteria[0];
        var evaluation = TestData.Factory().Create(student, rubric, homework.Id, 9, "  молодец ");

        Assert.Equal(student.Id, evaluation.StudentId);
        Assert.Equal(homework.Id, evaluation.CriterionId);
        Assert.Equal(new Score(9, 10), evaluation.Score);
        Assert.Equal(TestData.Now, evaluation.GradedAt);
        Assert.Equal("молодец", evaluation.Comment);
    }

    [Fact]
    public void Factory_ScoreAboveCriterionMax_Throws()
    {
        var rubric = TestData.StandardRubric();

        Assert.Throws<DomainException>(() =>
            TestData.Factory().Create(TestData.NewStudent(), rubric, rubric.Criteria[0].Id, 12));
    }

    [Fact]
    public void Factory_CriterionNotInRubric_Throws()
    {
        var rubric = TestData.StandardRubric();

        Assert.Throws<DomainException>(() =>
            TestData.Factory().Create(TestData.NewStudent(), rubric, CriterionId.New(), 5));
    }

    [Fact]
    public void Factory_CriterionFromAnotherRubric_Throws()
    {
        var rubric = TestData.StandardRubric();
        var otherRubric = TestData.StandardRubric();

        Assert.Throws<DomainException>(() =>
            TestData.Factory().Create(TestData.NewStudent(), rubric, otherRubric.Criteria[0].Id, 5));
    }

    [Fact]
    public void Correct_ValidValue_UpdatesScoreAndKeepsComment()
    {
        var rubric = TestData.StandardRubric();
        var evaluation = TestData.Factory().Create(TestData.NewStudent(), rubric, rubric.Criteria[2].Id, 60, "экзамен");
        var later = TestData.Now.AddDays(1);

        evaluation.Correct(75, null, later);

        Assert.Equal(new Score(75, 100), evaluation.Score);
        Assert.Equal("экзамен", evaluation.Comment);
        Assert.Equal(later, evaluation.GradedAt);
    }

    [Fact]
    public void Correct_OutOfRange_ThrowsAndKeepsOldScore()
    {
        var rubric = TestData.StandardRubric();
        var evaluation = TestData.Factory().Create(TestData.NewStudent(), rubric, rubric.Criteria[0].Id, 7);

        Assert.Throws<DomainException>(() => evaluation.Correct(11, "ошибка", TestData.Now));
        Assert.Equal(new Score(7, 10), evaluation.Score);
        Assert.Null(evaluation.Comment);
    }
}
