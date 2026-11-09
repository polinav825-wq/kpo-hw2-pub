namespace Pos.Domain.Students;

/// <summary>
/// Студент — кого оценивают (Entity): у него есть идентичность, имя может меняться.
/// </summary>
public sealed class Student
{
    public Student(StudentName name)
        : this(StudentId.New(), name)
    {
    }

    public Student(StudentId id, StudentName name)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public StudentId Id { get; }

    public StudentName Name { get; private set; }

    public void Rename(StudentName newName)
    {
        Name = newName ?? throw new ArgumentNullException(nameof(newName));
    }
}
