namespace Specky6;

public class TransientAsAttribute : SpeckAttribute
{
    public TransientAsAttribute(Type type) => Type = type;
    public Type Type { get; }
}
