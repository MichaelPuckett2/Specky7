namespace Specky6;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true)]
public abstract class SpeckAttribute : Attribute { }
