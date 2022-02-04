namespace Specky6;

[AttributeUsage(AttributeTargets.Class)]
public class SingletonAsAttribute : Attribute
{
    public SingletonAsAttribute(Type type) => Type = type;
    public Type Type { get; }
}
