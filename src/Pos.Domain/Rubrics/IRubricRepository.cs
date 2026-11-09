namespace Pos.Domain.Rubrics;

/// <summary>Порт хранилища рубрикатора курса.</summary>
public interface IRubricRepository
{
    Rubric Get();
    void Save(Rubric rubric);
}
