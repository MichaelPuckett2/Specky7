namespace Specky6;

[AttributeUsage(AttributeTargets.Class)]
public class TransientAsAttribute : Attribute
{
    public TransientAsAttribute(Type type) => Type = type;
    public Type Type { get; }
}
