namespace Specky6;

public class ScopedAsAttribute : SpeckAttribute
{
    public ScopedAsAttribute(Type type) => Type = type;
    public Type Type { get; }
}
