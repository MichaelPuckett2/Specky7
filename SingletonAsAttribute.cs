namespace Specky6;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SingletonAsAttribute : Attribute
{
    public SingletonAsAttribute(Type type) => Type = type;
    public Type Type { get; }
}
