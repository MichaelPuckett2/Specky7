namespace Specky6;

[AttributeUsage(AttributeTargets.Class)]
public class SpeckSingletonAsAttribute : Attribute
{
    public SpeckSingletonAsAttribute(Type type) => Type = type;
    public Type Type { get; }
}
