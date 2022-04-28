namespace Specky6;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ScopedAsAttribute : Attribute
{
    public ScopedAsAttribute(Type type) => Type = type;
    public Type Type { get; }
}
