using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Specky6;
public static class Extensions
{
    public static IServiceCollection AddSpecks(this IServiceCollection serviceCollection, IEnumerable<Assembly>? assemblies = null)
    {
        assemblies ??= new [] { Assembly.GetCallingAssembly() };

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                serviceCollection.ForSpeck<SpeckSingletonAttribute>(type, x => serviceCollection.AddSingleton(type));
                serviceCollection.ForSpeck<SpeckSingletonAsAttribute>(type, x => serviceCollection.AddSingleton(x.Type, type));

                serviceCollection.ForSpeck<SpeckTransientAttribute>(type, x => serviceCollection.AddSingleton(type));
                serviceCollection.ForSpeck<SpeckTransientAsAttribute>(type, x => serviceCollection.AddSingleton(x.Type, type));

                serviceCollection.ForSpeck<SpeckScopedAttribute>(type, x => serviceCollection.AddSingleton(type));
                serviceCollection.ForSpeck<SpeckScopedAsAttribute>(type, x => serviceCollection.AddSingleton(x.Type, type));
            }
        }

        return serviceCollection;
    }

    private static IServiceCollection ForSpeck<T>(this IServiceCollection serviceCollection, Type type, Action<T> action) where T : Attribute
    {
        var attributes = type.GetCustomAttributes().Where(x => x is T).Select(x => (T)x);
        foreach (var attribute in attributes) action.Invoke(attribute);
        return serviceCollection;
    }
}
