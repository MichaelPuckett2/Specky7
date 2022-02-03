namespace Specky6;

[AttributeUsage(AttributeTargets.Class)]
public class SpeckScopedAsAttribute : Attribute
{
    public SpeckScopedAsAttribute(Type type) => Type = type;
    public Type Type { get; }
}
