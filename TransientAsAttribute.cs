namespace Specky6;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TransientAsAttribute : Attribute
{
    public TransientAsAttribute(Type type) => Type = type;
    public Type Type { get; }
}
