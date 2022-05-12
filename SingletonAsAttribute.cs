namespace Specky6;

public class SingletonAsAttribute : SpeckAttribute
{
    public SingletonAsAttribute(Type type) => Type = type;
    public Type Type { get; }
}
