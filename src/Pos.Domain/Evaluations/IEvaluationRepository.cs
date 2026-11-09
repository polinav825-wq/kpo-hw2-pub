using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Domain.Evaluations;

/// <summary>Порт хранилища оценок.</summary>
public interface IEvaluationRepository
{
    void Add(Evaluation evaluation);

    void Update(Evaluation evaluation);

    Evaluation? Find(EvaluationId id);

    Evaluation? Find(StudentId studentId, CriterionId criterionId);

    IReadOnlyList<Evaluation> GetByStudent(StudentId studentId);

    IReadOnlyList<Evaluation> GetAll();

    void Remove(EvaluationId id);

    void RemoveByStudent(StudentId studentId);

    void RemoveByCriterion(CriterionId criterionId);
}
