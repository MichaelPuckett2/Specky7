namespace Specky6.Exceptions;
public  class SpeckAttributeUnknownException : Exception
{
    public SpeckAttributeUnknownException(Type type) : base($"{type.Name} is an unknown Speck Attribute.") { }
}
