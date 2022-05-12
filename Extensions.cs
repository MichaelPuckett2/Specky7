using Microsoft.Extensions.DependencyInjection;

using Specky6.Exceptions;

using System.Reflection;

namespace Specky6;
public static class Extensions
{
    /// <summary>
    /// Scans the calling assembly and injects specks into the IServiceCollection.
    /// </summary>
    /// <param name="serviceCollection">The IServiceCollection in use.</param>
    /// <param name="assemblies">The assemblies to scan.</param>
    /// <returns>The same IServiceCollection in use.</returns>
    public static IServiceCollection AddSpecks(this IServiceCollection serviceCollection)
    {
        Assembly.GetCallingAssembly()
            .GetTypes()
            .Execute(type => ScanTypeAndInject(serviceCollection, type));
        return serviceCollection;
    }

    /// <summary>
    /// Scans the requested assemblies and injects specks into the IServiceCollection.
    /// </summary>
    /// <param name="serviceCollection">The IServiceCollection in use.</param>
    /// <param name="assemblies">The assemblies to scan.</param>
    /// <returns>The same IServiceCollection in use.</returns>
    public static IServiceCollection AddSpecks(this IServiceCollection serviceCollection, IEnumerable<Assembly> assemblies)
    {
        assemblies
            .SelectMany(x => x.GetTypes())
            .Execute(type => ScanTypeAndInject(serviceCollection, type));
        return serviceCollection;
    }

    internal static IServiceCollection WithTypesAs<T>(this IServiceCollection serviceCollection, Type type, Action<T> action) where T : Attribute
    {
        var attributes = type.GetCustomAttributes().OfType<T>();
        foreach (var attribute in attributes) action.Invoke(attribute);
        return serviceCollection;
    }

    internal static void Execute<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable) action.Invoke(item);
    }

    internal static void ScanTypeAndInject(IServiceCollection serviceCollection, Type type)
    {
        serviceCollection.WithTypesAs<SingletonAttribute>(type, x => serviceCollection.AddSingleton(type));
        serviceCollection.WithTypesAs<SingletonAsAttribute>(type, x => serviceCollection.AddSingleton(x.Type, type));

        serviceCollection.WithTypesAs<TransientAttribute>(type, x => serviceCollection.AddTransient(type));
        serviceCollection.WithTypesAs<TransientAsAttribute>(type, x => serviceCollection.AddTransient(x.Type, type));

        serviceCollection.WithTypesAs<ScopedAttribute>(type, x => serviceCollection.AddScoped(type));
        serviceCollection.WithTypesAs<ScopedAsAttribute>(type, x => serviceCollection.AddScoped(x.Type, type));

        if (type.IsInterface)
        {
            serviceCollection.WithTypesAs(type, (Action<SpeckyConfiguration>)(speckyConfigurationAttribute =>
            {
                ScanPropertiesAndInject(serviceCollection, type);
                ScanFieldsAndInject(serviceCollection, type);
                ScanMethodsAndInject(serviceCollection, type);
            }));
        }
    }

    private static void ScanMethodsAndInject(IServiceCollection serviceCollection, Type type)
    {
        type.GetMethods().Where(x => x.ReturnType != typeof(void)).ToList().ForEach(methodInfo =>
        {
            methodInfo.GetCustomAttributes<SpeckAttribute>().ToList().ForEach(speckAttribute =>
            {
                _ = speckAttribute switch
                {
                    SingletonAttribute singletonAttribute => serviceCollection.AddSingleton(methodInfo.ReturnType),
                    SingletonAsAttribute singletonAsAttribute => serviceCollection.AddSingleton(singletonAsAttribute.Type, methodInfo.ReturnType),
                    TransientAttribute transientAttribute => serviceCollection.AddSingleton(methodInfo.ReturnType),
                    TransientAsAttribute transientAsAttribute => serviceCollection.AddSingleton(transientAsAttribute.Type, methodInfo.ReturnType),
                    ScopedAttribute scopedAttribute => serviceCollection.AddSingleton(methodInfo.ReturnType),
                    ScopedAsAttribute scopedAsAttribute => serviceCollection.AddSingleton(scopedAsAttribute.Type, methodInfo.ReturnType),
                    _ => throw new SpeckAttributeUnknownException(speckAttribute.GetType())
                };
            });
        });
    }

    private static void ScanFieldsAndInject(IServiceCollection serviceCollection, Type type)
    {
        type.GetFields().ToList().ForEach(fieldInfo =>
        {
            fieldInfo.GetCustomAttributes<SpeckAttribute>().ToList().ForEach(speckAttribute =>
            {
                _ = speckAttribute switch
                {
                    SingletonAttribute singletonAttribute => serviceCollection.AddSingleton(fieldInfo.FieldType),
                    SingletonAsAttribute singletonAsAttribute => serviceCollection.AddSingleton(singletonAsAttribute.Type, fieldInfo.FieldType),
                    TransientAttribute transientAttribute => serviceCollection.AddSingleton(fieldInfo.FieldType),
                    TransientAsAttribute transientAsAttribute => serviceCollection.AddSingleton(transientAsAttribute.Type, fieldInfo.FieldType),
                    ScopedAttribute scopedAttribute => serviceCollection.AddSingleton(fieldInfo.FieldType),
                    ScopedAsAttribute scopedAsAttribute => serviceCollection.AddSingleton(scopedAsAttribute.Type, fieldInfo.FieldType),
                    _ => throw new SpeckAttributeUnknownException(speckAttribute.GetType())
                };
            });
        });
    }

    private static void ScanPropertiesAndInject(IServiceCollection serviceCollection, Type type)
    {
        type.GetProperties().ToList().ForEach(propertyInfo =>
        {
            propertyInfo.GetCustomAttributes<SpeckAttribute>().ToList().ForEach(speckAttribute =>
            {
                _ = speckAttribute switch
                {
                    SingletonAttribute singletonAttribute => serviceCollection.AddSingleton(propertyInfo.PropertyType),
                    SingletonAsAttribute singletonAsAttribute => serviceCollection.AddSingleton(singletonAsAttribute.Type, propertyInfo.PropertyType),
                    TransientAttribute transientAttribute => serviceCollection.AddSingleton(propertyInfo.PropertyType),
                    TransientAsAttribute transientAsAttribute => serviceCollection.AddSingleton(transientAsAttribute.Type, propertyInfo.PropertyType),
                    ScopedAttribute scopedAttribute => serviceCollection.AddSingleton(propertyInfo.PropertyType),
                    ScopedAsAttribute scopedAsAttribute => serviceCollection.AddSingleton(scopedAsAttribute.Type, propertyInfo.PropertyType),
                    _ => throw new SpeckAttributeUnknownException(speckAttribute.GetType())
                };
            });
        });
    }
}
