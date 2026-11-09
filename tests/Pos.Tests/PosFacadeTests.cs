using Pos.Application;
using Pos.Application.Views;
using Pos.Domain.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Grading;
using Pos.Infrastructure.Persistence;
using Pos.Tests.Fakes;

namespace Pos.Tests;

/// <summary>Сценарии через фасад с настоящими in-memory адаптерами.</summary>
public class PosFacadeTests
{
    private readonly FakeExporter exporter = new();
    private readonly PosFacade pos;
    private readonly IReadOnlyList<CriterionView> criteria;
    private readonly StudentView anna;

    public PosFacadeTests()
    {
        var clock = new FixedTimeProvider(TestData.Now);
        pos = new PosFacade(
            new InMemoryStudentRepository(),
            new InMemoryRubricRepository(),
            new InMemoryEvaluationRepository(),
            new EvaluationFactory(clock),
            new FinalGradeCalculator(),
            new GradingPolicyCatalog(new WeightedGradingPolicy(), new EvaluatedOnlyGradingPolicy()),
            new ExporterCatalog(exporter),
            clock);

        criteria = pos.SetupRubric(new[]
        {
            new CriterionDraft("ДЗ", 10, 0.5m),
            new CriterionDraft("Экзамен", 10, 0.5m),
        });
        anna = pos.AddStudent("Анна");
    }

    [Fact]
    public void Grade_ThenFinalGrade_IsCalculatedFromEvaluations()
    {
        pos.Grade(anna.Id, criteria[0].Id, 8);
        pos.Grade(anna.Id, criteria[1].Id, 6);

        Assert.Equal(7m, pos.GetFinalGrade(anna.Id).Grade);
    }

    [Fact]
    public void Grade_OutOfRange_ThrowsDomainException_AndNothingIsSaved()
    {
        Assert.Throws<DomainException>(() => pos.Grade(anna.Id, criteria[0].Id, 12));
        Assert.Empty(pos.ListEvaluations());
    }

    [Fact]
    public void Grade_UnknownCriterion_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => pos.Grade(anna.Id, Guid.NewGuid(), 5));
    }

    [Fact]
    public void Grade_Twice_ForSameCriterion_Throws()
    {
        pos.Grade(anna.Id, criteria[0].Id, 8);

        Assert.Throws<PosOperationException>(() => pos.Grade(anna.Id, criteria[0].Id, 9));
    }

    [Fact]
    public void Grade_UnknownStudent_Throws()
    {
        Assert.Throws<PosOperationException>(() => pos.Grade(Guid.NewGuid(), criteria[0].Id, 5));
    }

    [Fact]
    public void Correct_And_Cancel_KeepFinalGradeConsistent()
    {
        var homework = pos.Grade(anna.Id, criteria[0].Id, 4);
        pos.Grade(anna.Id, criteria[1].Id, 10);
        Assert.Equal(7m, pos.GetFinalGrade(anna.Id).Grade);

        pos.CorrectEvaluation(homework.Id, 10);
        Assert.Equal(10m, pos.GetFinalGrade(anna.Id).Grade);

        pos.CancelEvaluation(homework.Id);
        Assert.Equal(5m, pos.GetFinalGrade(anna.Id).Grade);
    }

    [Fact]
    public void RemoveCriterion_RemovesItsEvaluations()
    {
        pos.Grade(anna.Id, criteria[0].Id, 8);
        pos.Grade(anna.Id, criteria[1].Id, 6);

        pos.RemoveCriterion(criteria[1].Id);

        var evaluation = Assert.Single(pos.ListEvaluations());
        Assert.Equal(criteria[0].Id, evaluation.CriterionId);
        Assert.Equal(8m, pos.GetFinalGrade(anna.Id).Grade);
    }

    [Fact]
    public void RemoveStudent_RemovesTheirEvaluations()
    {
        pos.Grade(anna.Id, criteria[0].Id, 8);

        pos.RemoveStudent(anna.Id);

        Assert.Empty(pos.ListStudents());
        Assert.Empty(pos.ListEvaluations());
    }

    [Fact]
    public void SetupRubric_AfterGrading_Throws()
    {
        pos.Grade(anna.Id, criteria[0].Id, 8);

        Assert.Throws<PosOperationException>(() =>
            pos.SetupRubric(new[] { new CriterionDraft("Новый", 10, 1) }));
    }

    [Fact]
    public void ChangeGradingPolicy_SwitchesStrategy()
    {
        pos.Grade(anna.Id, criteria[0].Id, 8);
        Assert.Equal(4m, pos.GetFinalGrade(anna.Id).Grade);

        pos.ChangeGradingPolicy("current");

        Assert.Equal(8m, pos.GetFinalGrade(anna.Id).Grade);
        Assert.Contains(pos.ListGradingPolicies(), p => p.Code == "current" && p.IsCurrent);
    }

    [Fact]
    public void ChangeGradingPolicy_Unknown_Throws()
    {
        Assert.Throws<PosOperationException>(() => pos.ChangeGradingPolicy("magic"));
    }

    [Fact]
    public void Export_PassesCurrentFinalGradesToExporter()
    {
        pos.Grade(anna.Id, criteria[0].Id, 10);

        var path = pos.ExportFinalGrades("fake", "out");

        Assert.Equal("out/fake", path);
        var row = Assert.Single(exporter.LastExported!);
        Assert.Equal("Анна", row.StudentName);
        Assert.Equal(5m, row.Grade);
    }
}
