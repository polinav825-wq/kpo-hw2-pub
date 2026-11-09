using Pos.Application;
using Pos.Application.Views;
using Pos.Domain.Common;

namespace Pos.ConsoleApp;

/// <summary>
/// Сквозной сценарий из ТЗ: студенты и критерии → оценки → итог → экспорт,
/// плюс демонстрация того, что правила домена нарушить нельзя.
/// </summary>
public sealed class DemoScenario
{
    private readonly PosFacade pos;
    private readonly TextWriter output;

    public DemoScenario(PosFacade pos, TextWriter output)
    {
        this.pos = pos ?? throw new ArgumentNullException(nameof(pos));
        this.output = output ?? throw new ArgumentNullException(nameof(output));
    }

    public void Run(string exportDirectory = "export")
    {
        if (!pos.IsEmpty)
        {
            output.WriteLine("  Демо запускается только на пустом курсе (без студентов и критериев).");
            return;
        }

        Section("1. Рубрикатор (собран Builder'ом целиком)");
        var criteria = pos.SetupRubric(new[]
        {
            new CriterionDraft("Домашние задания", 10, 0.4m),
            new CriterionDraft("Контрольная", 10, 0.3m),
            new CriterionDraft("Экзамен", 100, 0.3m),
        });
        foreach (var criterion in criteria)
        {
            output.WriteLine($"  - {Display.Of(criterion)}");
        }

        var homework = criteria[0];
        var test = criteria[1];
        var exam = criteria[2];

        Section("2. Студенты");
        var anna = pos.AddStudent("Анна Смирнова");
        var boris = pos.AddStudent("Борис Кузнецов");
        var vera = pos.AddStudent("Вера Павлова");
        foreach (var student in pos.ListStudents())
        {
            output.WriteLine($"  - {Display.Of(student)}");
        }

        Section("3. Оценки");
        Show(pos.Grade(anna.Id, homework.Id, 9, "Все ДЗ вовремя"));
        Show(pos.Grade(anna.Id, test.Id, 8));
        Show(pos.Grade(anna.Id, exam.Id, 85));
        Show(pos.Grade(boris.Id, homework.Id, 6));
        Show(pos.Grade(boris.Id, test.Id, 7.5m));
        Show(pos.Grade(vera.Id, homework.Id, 10));

        Section("4. Правила домена нарушить нельзя");
        Attempt("12 баллов из 10", () => pos.Grade(vera.Id, test.Id, 12));
        Attempt("отрицательный балл", () => pos.Grade(vera.Id, test.Id, -1));
        Attempt("критерий не из рубрикатора", () => pos.Grade(vera.Id, Guid.NewGuid(), 5));
        Attempt("вторая оценка по тому же критерию", () => pos.Grade(anna.Id, homework.Id, 5));
        Attempt("критерий с нулевым весом", () => pos.AddCriterion("Активность", 5, 0));
        Attempt("критерий-дубликат", () => pos.AddCriterion("экзамен", 100, 0.1m));

        Section("5. Исправление и отмена");
        var borisTest = pos.ListEvaluations(boris.Id).Single(e => e.CriterionId == test.Id);
        Show(pos.CorrectEvaluation(borisTest.Id, 9, "Апелляция удовлетворена"));
        Attempt("исправить на 11 из 10", () => pos.CorrectEvaluation(borisTest.Id, 11));
        var veraHomework = pos.ListEvaluations(vera.Id).Single();
        pos.CancelEvaluation(veraHomework.Id);
        output.WriteLine($"  Отменено: {Display.Of(veraHomework)}");
        Show(pos.Grade(vera.Id, homework.Id, 7));

        Section("6. Итог (Strategy + Decorator)");
        PrintFinalGrades();
        pos.ChangeGradingPolicy("current");
        PrintFinalGrades();
        pos.ChangeGradingPolicy("weighted");

        Section("7. Экспорт итога (Template Method)");
        foreach (var format in pos.ExportFormats)
        {
            var path = pos.ExportFinalGrades(format, exportDirectory);
            output.WriteLine($"  {format}: {Path.GetFullPath(path)}");
        }

        output.WriteLine();
        output.WriteLine("Демо завершено.");
    }

    private void PrintFinalGrades()
    {
        var grades = pos.GetFinalGrades();
        output.WriteLine($"  {grades[0].Policy}:");
        foreach (var grade in grades)
        {
            output.WriteLine($"    {Display.Of(grade)}");
        }
    }

    private void Section(string title)
    {
        output.WriteLine();
        output.WriteLine(title);
    }

    private void Show(EvaluationView evaluation) => output.WriteLine($"  [OK] {Display.Of(evaluation)}");

    private void Attempt(string caseName, Action action)
    {
        try
        {
            action();
            output.WriteLine($"  [?] {caseName}: неожиданно прошло");
        }
        catch (DomainException exception)
        {
            output.WriteLine($"  [ОТКЛОНЕНО] {caseName}: {exception.Message}");
        }
        catch (PosOperationException exception)
        {
            output.WriteLine($"  [ОТКЛОНЕНО] {caseName}: {exception.Message}");
        }
    }
}
