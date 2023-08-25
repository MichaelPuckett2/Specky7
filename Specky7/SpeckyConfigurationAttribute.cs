namespace Specky7;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
public class SpeckyConfigurationAttribute : Attribute 
{
    public SpeckyConfigurationAttribute() : this(string.Empty) { }
    public SpeckyConfigurationAttribute(string option) => Option = option;
    public string Option { get; init; }
}