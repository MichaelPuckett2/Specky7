using System.Reflection;

namespace Specky7;

public class SpeckyOptions
{
    internal HashSet<Type> InterfaceTypes { get; } = new();
    internal HashSet<string> Options { get; } = new();
    internal HashSet<Assembly> Assemblies { get; } = new();
    public bool UseConfigurations { get; set; }
    public SpeckyOptions AddConfiguration<T>()
    {
        if (typeof(T).IsInterface)
        {
            if (typeof(T).GetCustomAttributes(typeof(SpeckyConfigurationAttribute), false).Length == 0)
            {
                throw new TypeAccessException($"{typeof(T).Name} must have the {nameof(SpeckyConfigurationAttribute)} to be used as a speck configuration interface.\n{nameof(AddConfiguration)}<{typeof(T).Name}>");
            }
            if (InterfaceTypes.Contains(typeof(T)))
            {
                throw new TypeAccessException($"{typeof(T).Name} was already added to the configuration interfaces.\n{nameof(AddConfiguration)}<{typeof(T).Name}");
            }
            InterfaceTypes.Add(typeof(T));
            UseConfigurations = true;
            return this;
        }
        throw new TypeAccessException($"{typeof(T).Name} must be an interface to be added as a speck configuration.\n{nameof(AddConfiguration)}<{typeof(T).Name}");
    }
    public SpeckyOptions AddOption(string option)
    {
        if (Options.Contains(option))
        {
            throw new ArgumentException($"{option} was already added to the configuration options.\n{nameof(AddOption)}");
        }
        Options.Add(option);
        return this;
    }
    public SpeckyOptions AddAssembly<T>() => AddAssemblies(new[] {  typeof(T).Assembly });
    public SpeckyOptions AddAssemblies(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies.AsSpan())
        {
            if (Assemblies.Contains(assembly))
            {
                throw new ArgumentException($"{assembly.GetName()} was already added to the configuration assemblies.\n{nameof(AddAssemblies)}");
            }
            Assemblies.Add(assembly);
        }
        return this;
    }
    public void Clear()
    {
        InterfaceTypes.Clear();
        Options.Clear();
        Assemblies.Clear();
        UseConfigurations = false;
    }

    internal void AddConfigurations(Span<Type> speckyConfigurationTypes)
    {
        foreach (var type in speckyConfigurationTypes) InterfaceTypes.Add(type);
    }
}