namespace Pos.Infrastructure.DependencyInjection;

public enum Lifetime
{
    /// <summary>Новый объект при каждом Resolve.</summary>
    Transient,

    /// <summary>Один объект на весь контейнер.</summary>
    Singleton,
}

/// <summary>
/// DI-контейнер из ДЗ1: регистрирует «абстракция-реализация» и создаёт объекты
/// рекурсивно подставляя зависимости в самый длинный публичный конструктор
/// </summary>
public sealed class DIContainer
{
    private readonly Dictionary<Type, Registration> _services = new();

    public void Register<TInterface, TImplementation>(Lifetime lifetime = Lifetime.Transient)
        where TImplementation : class, TInterface
    {
        _services[typeof(TInterface)] = new Registration(typeof(TImplementation), lifetime);
    }

    public void RegisterSingleton<TInterface, TImplementation>()
        where TImplementation : class, TInterface
    {
        Register<TInterface, TImplementation>(Lifetime.Singleton);
    }

    /// <summary>Зарегистрировать готовый объект (Console.Out, каталоги, fake в тестах).</summary>
    public void RegisterInstance<TInterface>(TInterface instance)
        where TInterface : class
    {
        ArgumentNullException.ThrowIfNull(instance);
        _services[typeof(TInterface)] = new Registration(instance.GetType(), Lifetime.Singleton)
        {
            Instance = instance,
        };
    }

    public T Resolve<T>() => (T)Resolve(typeof(T), new HashSet<Type>());
    private object Resolve(Type type, HashSet<Type> chain)
    {
        if (_services.TryGetValue(type, out var registration))
        {
            if (registration.Instance is not null)
            {
                return registration.Instance;
            }
            var created = Create(registration.ImplementationType, chain);
            if (registration.Lifetime == Lifetime.Singleton)
            {
                registration.Instance = created;
            }
            return created;
        }

        if (type.IsInterface || type.IsAbstract)
        {
            throw new InvalidOperationException($"Тип {type.Name} не зарегистрирован в контейнере!");
        }

        return Create(type, chain);
    }

    private object Create(Type type, HashSet<Type> chain)
    {
        if (type.IsPrimitive || type == typeof(string))
        {
            throw new InvalidOperationException(
                $"Контейнер не умеет создавать {type.Name} сам — зарегистрируйте значение через RegisterInstance.");
        }
        if (!chain.Add(type))
        {
            throw new InvalidOperationException($"Циклическая зависимость при создании {type.Name}.");
        }
        var constructor = type.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault()
            ?? throw new InvalidOperationException($"У типа {type.Name} нет публичного конструктора.");
        var args = constructor.GetParameters()
            .Select(p => Resolve(p.ParameterType, chain))
            .ToArray();

        chain.Remove(type);
        return constructor.Invoke(args);
    }

    private sealed class Registration
    {
        public Registration(Type implementationType, Lifetime lifetime)
        {
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }
        public Type ImplementationType { get; }
        public Lifetime Lifetime { get; }
        public object? Instance { get; set; }
    }
}
