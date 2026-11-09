namespace Pos.Domain.Students;

/// <summary>Порт хранилища студентов. Реализация — снаружи домена (Infrastructure).</summary>
public interface IStudentRepository
{
    void Add(Student student);
    Student? Find(StudentId id);
    IReadOnlyList<Student> GetAll();
    void Remove(StudentId id);
}
