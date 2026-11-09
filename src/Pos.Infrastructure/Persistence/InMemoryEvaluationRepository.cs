using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Infrastructure.Persistence;

/// <summary>Адаптер порта IEvaluationRepository: хранение в памяти.</summary>
public sealed class InMemoryEvaluationRepository : IEvaluationRepository
{
    private readonly List<Evaluation> items = new();
    public void Add(Evaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);
        if (items.Any(e => e.Id == evaluation.Id))
        {
            throw new InvalidOperationException("Оценка с таким идентификатором уже сохранена.");
        }

        items.Add(evaluation);
    }
    public void Update(Evaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);
        var index = items.FindIndex(e => e.Id == evaluation.Id);
        if (index < 0)
        {
            throw new InvalidOperationException("Оценка не найдена в хранилище.");
        }

        items[index] = evaluation;
    }
    public Evaluation? Find(EvaluationId id) => items.FirstOrDefault(e => e.Id == id);
    public Evaluation? Find(StudentId studentId, CriterionId criterionId) =>
        items.FirstOrDefault(e => e.StudentId == studentId && e.CriterionId == criterionId);
    public IReadOnlyList<Evaluation> GetByStudent(StudentId studentId) =>
        items.Where(e => e.StudentId == studentId).ToList();
    public IReadOnlyList<Evaluation> GetAll() => items.ToList();
    public void Remove(EvaluationId id) => items.RemoveAll(e => e.Id == id);
    public void RemoveByStudent(StudentId studentId) => items.RemoveAll(e => e.StudentId == studentId);
    public void RemoveByCriterion(CriterionId criterionId) => items.RemoveAll(e => e.CriterionId == criterionId);
}
