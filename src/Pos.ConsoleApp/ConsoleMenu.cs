using Pos.Application;
using Pos.Domain.Common;

namespace Pos.ConsoleApp;

/// <summary>
/// Консольный адаптер (входной порт): переводит команды пользователя в вызовы фасада.
/// Никакой бизнес-логики здесь нет — только ввод, вывод и обработка ошибок.
/// </summary>
public sealed class ConsoleMenu
{
    private readonly PosFacade pos;
    private readonly DemoScenario demo;
    private readonly ConsoleInput input;
    private readonly TextWriter output;

    public ConsoleMenu(PosFacade pos, DemoScenario demo, ConsoleInput input, TextWriter output)
    {
        this.pos = pos ?? throw new ArgumentNullException(nameof(pos));
        this.demo = demo ?? throw new ArgumentNullException(nameof(demo));
        this.input = input ?? throw new ArgumentNullException(nameof(input));
        this.output = output ?? throw new ArgumentNullException(nameof(output));
    }

    public void Run()
    {
        output.WriteLine("ПОС — Преподавательская Оценка Студентов");

        while (true)
        {
            PrintMenu();
            string? choice;
            try
            {
                choice = input.Ask("> ");
            }
            catch (EndOfInputException)
            {
                return;
            }

            if (choice is null || choice.Trim() == "0")
            {
                output.WriteLine("До свидания!");
                return;
            }

            try
            {
                Execute(choice.Trim());
            }
            catch (EndOfInputException)
            {
                return;
            }
            catch (DomainException exception)
            {
                output.WriteLine($"  [ПРАВИЛО ДОМЕНА] {exception.Message}");
            }
            catch (PosOperationException exception)
            {
                output.WriteLine($"  [ОШИБКА] {exception.Message}");
            }
            catch (IOException exception)
            {
                output.WriteLine($"  [ФАЙЛ] {exception.Message}");
            }
        }
    }

    private void PrintMenu()
    {
        output.WriteLine();
        output.WriteLine(" 1. Демо-сценарий (на пустом курсе)");
        output.WriteLine(" 2. Студенты: список        3. Добавить студента       4. Удалить студента");
        output.WriteLine(" 5. Критерии: список        6. Добавить критерий       7. Удалить критерий");
        output.WriteLine(" 8. Выставить оценку        9. Оценки: список         10. Исправить оценку");
        output.WriteLine("11. Отменить оценку        12. Итоги                  13. Правило итога");
        output.WriteLine("14. Экспорт итогов          0. Выход");
    }

    private void Execute(string choice)
    {
        switch (choice)
        {
            case "1": demo.Run(); break;
            case "2": ListStudents(); break;
            case "3": AddStudent(); break;
            case "4": RemoveStudent(); break;
            case "5": ListCriteria(); break;
            case "6": AddCriterion(); break;
            case "7": RemoveCriterion(); break;
            case "8": Grade(); break;
            case "9": ListEvaluations(); break;
            case "10": CorrectEvaluation(); break;
            case "11": CancelEvaluation(); break;
            case "12": ShowFinalGrades(); break;
            case "13": ChangePolicy(); break;
            case "14": Export(); break;
            default: output.WriteLine("  Нет такого пункта меню."); break;
        }
    }

    private void ListStudents()
    {
        var students = pos.ListStudents();
        output.WriteLine(students.Count == 0 ? "  Студентов пока нет." : "Студенты:");
        foreach (var student in students)
        {
            output.WriteLine($"  - {Display.Of(student)}");
        }
    }

    private void AddStudent()
    {
        var student = pos.AddStudent(input.AskText("ФИО студента: "));
        output.WriteLine($"  Добавлен: {Display.Of(student)}");
    }

    private void RemoveStudent()
    {
        var student = input.Choose("Кого удалить?", pos.ListStudents(), x => Display.Of(x));
        if (student is null)
        {
            return;
        }

        pos.RemoveStudent(student.Id);
        output.WriteLine($"  Удалён вместе с оценками: {Display.Of(student)}");
    }

    private void ListCriteria()
    {
        var criteria = pos.ListCriteria();
        output.WriteLine(criteria.Count == 0 ? "  Рубрикатор пуст." : "Рубрикатор:");
        foreach (var criterion in criteria)
        {
            output.WriteLine($"  - {Display.Of(criterion)}");
        }
    }

    private void AddCriterion()
    {
        var name = input.AskText("Название критерия: ");
        var maxScore = input.AskDecimal("Максимальный балл: ");
        var weight = input.AskDecimal("Вес (вклад в итог): ");
        var criterion = pos.AddCriterion(name, maxScore, weight);
        output.WriteLine($"  Добавлен: {Display.Of(criterion)}");
    }

    private void RemoveCriterion()
    {
        var criterion = input.Choose("Какой критерий удалить?", pos.ListCriteria(), x => Display.Of(x));
        if (criterion is null)
        {
            return;
        }

        pos.RemoveCriterion(criterion.Id);
        output.WriteLine($"  Удалён вместе с оценками по нему: {criterion.Name}");
    }

    private void Grade()
    {
        var student = input.Choose("Студент:", pos.ListStudents(), x => Display.Of(x));
        if (student is null)
        {
            return;
        }

        var criterion = input.Choose("Критерий:", pos.ListCriteria(), x => Display.Of(x));
        if (criterion is null)
        {
            return;
        }

        var score = input.AskDecimal($"Балл (0..{Display.Number(criterion.MaxScore)}): ");
        var comment = input.Ask("Комментарий (необязательно): ");
        var evaluation = pos.Grade(student.Id, criterion.Id, score, comment);
        output.WriteLine($"  Выставлено: {Display.Of(evaluation)}");
    }

    private void ListEvaluations()
    {
        var evaluations = pos.ListEvaluations();
        output.WriteLine(evaluations.Count == 0 ? "  Оценок пока нет." : "Оценки:");
        foreach (var evaluation in evaluations)
        {
            output.WriteLine($"  - {Display.Of(evaluation)}");
        }
    }

    private void CorrectEvaluation()
    {
        var evaluation = input.Choose("Какую оценку исправить?", pos.ListEvaluations(), x => Display.Of(x));
        if (evaluation is null)
        {
            return;
        }

        var score = input.AskDecimal($"Новый балл (0..{Display.Number(evaluation.MaxScore)}): ");
        var comment = input.Ask("Комментарий (Enter — оставить прежний): ");
        var corrected = pos.CorrectEvaluation(evaluation.Id, score, comment);
        output.WriteLine($"  Исправлено: {Display.Of(corrected)}");
    }

    private void CancelEvaluation()
    {
        var evaluation = input.Choose("Какую оценку отменить?", pos.ListEvaluations(), x => Display.Of(x));
        if (evaluation is null)
        {
            return;
        }

        pos.CancelEvaluation(evaluation.Id);
        output.WriteLine($"  Отменено: {Display.Of(evaluation)}");
    }

    private void ShowFinalGrades()
    {
        var grades = pos.GetFinalGrades();
        if (grades.Count == 0)
        {
            output.WriteLine("  Студентов пока нет.");
            return;
        }

        output.WriteLine($"Итоги — {grades[0].Policy}:");
        foreach (var grade in grades)
        {
            output.WriteLine($"  {Display.Of(grade)}");
        }
    }

    private void ChangePolicy()
    {
        var policy = input.Choose(
            "Правило итога:",
            pos.ListGradingPolicies(),
            p => p.IsCurrent ? $"{p.Title}  ← сейчас" : p.Title);
        if (policy is null)
        {
            return;
        }

        var current = pos.ChangeGradingPolicy(policy.Code);
        output.WriteLine($"  Теперь итог считается так: {current.Title}");
    }

    private void Export()
    {
        var format = input.AskText($"Формат ({string.Join("/", pos.ExportFormats)}): ");
        var directory = input.Ask("Папка (Enter — ./export): ");
        var path = pos.ExportFinalGrades(format, string.IsNullOrWhiteSpace(directory) ? "export" : directory.Trim());
        output.WriteLine($"  Итоги выгружены: {Path.GetFullPath(path)}");
    }
}
