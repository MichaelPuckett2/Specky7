namespace Specky6;

[AttributeUsage(AttributeTargets.Class)]
public class SpeckTransientAsAttribute : Attribute
{
    public SpeckTransientAsAttribute(Type type) => Type = type;
    public Type Type { get; }
}
