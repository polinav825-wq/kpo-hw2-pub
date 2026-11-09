using Pos.Application.Views;
using Pos.Domain.Evaluations;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Application;

/// <summary>
/// Facade (структурный GoF): единая точка входа во все сценарии «ПОС».
/// Консоль (и любой другой UI) вызывает только её и не знает о репозиториях, фабрике и расчёте итога.
/// </summary>
public sealed class PosFacade
{
    private readonly IStudentRepository students;
    private readonly IRubricRepository rubrics;
    private readonly IEvaluationRepository evaluations;
    private readonly EvaluationFactory evaluationFactory;
    private readonly FinalGradeCalculator calculator;
    private readonly GradingPolicyCatalog policies;
    private readonly ExporterCatalog exporters;
    private readonly TimeProvider clock;
    private IGradingPolicy currentPolicy;

    public PosFacade(
        IStudentRepository students,
        IRubricRepository rubrics,
        IEvaluationRepository evaluations,
        EvaluationFactory evaluationFactory,
        FinalGradeCalculator calculator,
        GradingPolicyCatalog policies,
        ExporterCatalog exporters,
        TimeProvider clock)
    {
        this.students = students ?? throw new ArgumentNullException(nameof(students));
        this.rubrics = rubrics ?? throw new ArgumentNullException(nameof(rubrics));
        this.evaluations = evaluations ?? throw new ArgumentNullException(nameof(evaluations));
        this.evaluationFactory = evaluationFactory ?? throw new ArgumentNullException(nameof(evaluationFactory));
        this.calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        this.policies = policies ?? throw new ArgumentNullException(nameof(policies));
        this.exporters = exporters ?? throw new ArgumentNullException(nameof(exporters));
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
        currentPolicy = policies.Default;
    }

    public bool IsEmpty => students.GetAll().Count == 0 && rubrics.Get().IsEmpty;


    public StudentView AddStudent(string name)
    {
        var student = new Student(new StudentName(name));
        students.Add(student);
        return ToView(student);
    }

    public IReadOnlyList<StudentView> ListStudents() =>
        students.GetAll().Select(x => ToView(x)).ToList();

    /// <summary>Удаляет студента вместе с его оценками.</summary>
    public void RemoveStudent(Guid studentId)
    {
        var student = GetStudent(studentId);
        evaluations.RemoveByStudent(student.Id);
        students.Remove(student.Id);
    }


    /// <summary>Задать рубрикатор целиком (через Builder). Возможно, пока не выставлено ни одной оценки.</summary>
    public IReadOnlyList<CriterionView> SetupRubric(IEnumerable<CriterionDraft> drafts)
    {
        ArgumentNullException.ThrowIfNull(drafts);
        if (evaluations.GetAll().Count > 0)
        {
            throw new PosOperationException("Нельзя заменить рубрикатор: по нему уже выставлены оценки.");
        }

        var builder = new RubricBuilder();
        foreach (var draft in drafts)
        {
            builder.WithCriterion(draft.Name, draft.MaxScore, draft.Weight);
        }

        var rubric = builder.Build();
        rubrics.Save(rubric);
        return ListCriteria();
    }

    public CriterionView AddCriterion(string name, decimal maxScore, decimal weight)
    {
        var rubric = rubrics.Get();
        var criterion = rubric.AddCriterion(name, maxScore, weight);
        rubrics.Save(rubric);
        return ToView(criterion);
    }

    public IReadOnlyList<CriterionView> ListCriteria() =>
        rubrics.Get().Criteria.Select(x => ToView(x)).ToList();

    /// <summary>Удаляет критерий и все оценки по нему, чтобы итог остался согласован с рубрикатором.</summary>
    public void RemoveCriterion(Guid criterionId)
    {
        var rubric = rubrics.Get();
        var id = new CriterionId(criterionId);
        rubric.RemoveCriterion(id);
        evaluations.RemoveByCriterion(id);
        rubrics.Save(rubric);
    }


    public EvaluationView Grade(Guid studentId, Guid criterionId, decimal score, string? comment = null)
    {
        var student = GetStudent(studentId);
        var rubric = rubrics.Get();
        var criterion = new CriterionId(criterionId);

        if (evaluations.Find(student.Id, criterion) is not null)
        {
            throw new PosOperationException("Оценка по этому критерию уже выставлена — используйте исправление.");
        }

        var evaluation = evaluationFactory.Create(student, rubric, criterion, score, comment);
        evaluations.Add(evaluation);
        return ToView(evaluation);
    }

    public EvaluationView CorrectEvaluation(Guid evaluationId, decimal newScore, string? comment = null)
    {
        var evaluation = GetEvaluation(evaluationId);
        evaluation.Correct(newScore, comment, clock.GetUtcNow());
        evaluations.Update(evaluation);
        return ToView(evaluation);
    }

    public void CancelEvaluation(Guid evaluationId)
    {
        var evaluation = GetEvaluation(evaluationId);
        evaluations.Remove(evaluation.Id);
    }

    public IReadOnlyList<EvaluationView> ListEvaluations(Guid? studentId = null)
    {
        var source = studentId is null
            ? evaluations.GetAll()
            : evaluations.GetByStudent(GetStudent(studentId.Value).Id);
        return source.Select(x => ToView(x)).ToList();
    }


    public IReadOnlyList<GradingPolicyView> ListGradingPolicies() =>
        policies.All.Select(p => new GradingPolicyView(p.Code, p.Title, ReferenceEquals(p, currentPolicy))).ToList();

    public GradingPolicyView ChangeGradingPolicy(string code)
    {
        currentPolicy = policies.Get(code);
        return new GradingPolicyView(currentPolicy.Code, currentPolicy.Title, true);
    }

    public FinalGradeView GetFinalGrade(Guid studentId) =>
        CalculateFinalGrade(GetStudent(studentId), rubrics.Get());

    public IReadOnlyList<FinalGradeView> GetFinalGrades()
    {
        var rubric = rubrics.Get();
        return students.GetAll().Select(s => CalculateFinalGrade(s, rubric)).ToList();
    }


    public IReadOnlyList<string> ExportFormats => exporters.Formats;

    /// <returns>Путь к файлу с выгрузкой.</returns>
    public string ExportFinalGrades(string format, string directory)
    {
        var exporter = exporters.Get(format);
        return exporter.Export(GetFinalGrades(), directory);
    }


    private FinalGradeView CalculateFinalGrade(Student student, Rubric rubric)
    {
        var own = evaluations.GetByStudent(student.Id);
        var grade = calculator.Calculate(student, rubric, own, currentPolicy);
        return new FinalGradeView(
            student.Id.Value,
            student.Name.Value,
            grade.Value,
            own.Count,
            rubric.Criteria.Count,
            grade.PolicyTitle);
    }

    private Student GetStudent(Guid id) =>
        students.Find(new StudentId(id)) ?? throw new PosOperationException("Студент не найден.");

    private Evaluation GetEvaluation(Guid id) =>
        evaluations.Find(new EvaluationId(id)) ?? throw new PosOperationException("Оценка не найдена.");

    private static StudentView ToView(Student student) => new(student.Id.Value, student.Name.Value);

    private static CriterionView ToView(Criterion criterion) =>
        new(criterion.Id.Value, criterion.Name.Value, criterion.MaxScore, criterion.Weight.Value);

    private EvaluationView ToView(Evaluation evaluation)
    {
        var studentName = students.Find(evaluation.StudentId)?.Name.Value ?? "—";
        var rubric = rubrics.Get();
        var criterionName = rubric.Contains(evaluation.CriterionId)
            ? rubric.GetCriterion(evaluation.CriterionId).Name.Value
            : "—";

        return new EvaluationView(
            evaluation.Id.Value,
            evaluation.StudentId.Value,
            studentName,
            evaluation.CriterionId.Value,
            criterionName,
            evaluation.Score.Value,
            evaluation.Score.Max,
            evaluation.GradedAt,
            evaluation.Comment);
    }
}
