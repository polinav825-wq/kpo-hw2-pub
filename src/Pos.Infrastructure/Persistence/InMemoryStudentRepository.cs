using Pos.Domain.Students;

namespace Pos.Infrastructure.Persistence;

/// <summary>Адаптер порта IStudentRepository: хранение в памяти, порядок добавления сохраняется.</summary>
public sealed class InMemoryStudentRepository : IStudentRepository
{
    private readonly List<Student> items = new();
    public void Add(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);
        if (items.Any(s => s.Id == student.Id))
        {
            throw new InvalidOperationException("Студент с таким идентификатором уже сохранён.");
        }

        items.Add(student);
    }
    public Student? Find(StudentId id) => items.FirstOrDefault(s => s.Id == id);
    public IReadOnlyList<Student> GetAll() => items.ToList();
    public void Remove(StudentId id) => items.RemoveAll(s => s.Id == id);
}
