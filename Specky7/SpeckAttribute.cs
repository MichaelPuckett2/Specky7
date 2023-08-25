using Microsoft.Extensions.DependencyInjection;

namespace Specky7;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true)]
public class SpeckAttribute : Attribute
{
    public SpeckAttribute(ServiceLifetime serviceLifetime = ServiceLifetime.Singleton, Type? serviceType = null)
    {
        ServiceLifetime = serviceLifetime;
        ServiceType = serviceType;
    }
    public ServiceLifetime ServiceLifetime { get; init; }
    public Type? ServiceType { get; init; }
}

public class SpeckAttribute<T> : SpeckAttribute where T : class
{

    public SpeckAttribute(ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) : base(serviceLifetime, typeof(T)) { }
}

public class SingletonAttribute : SpeckAttribute { }
public class SingletonAttribute<T> : SpeckAttribute<T> where T : class { }

public class ScopedAttribute : SpeckAttribute
{
    public ScopedAttribute() : base(ServiceLifetime.Scoped) { }
}
public class ScopedAttribute<T> : SpeckAttribute<T> where T : class
{
    public ScopedAttribute() : base(ServiceLifetime.Scoped) { }
}

public class TransientAttribute : SpeckAttribute
{
    public TransientAttribute() : base(ServiceLifetime.Transient) { }
}
public class TransientAttribute<T> : SpeckAttribute<T> where T : class
{
    public TransientAttribute() : base(ServiceLifetime.Transient) { }
}
