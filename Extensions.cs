using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Specky7;
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

    internal static void Execute<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable) action.Invoke(item);
    }

    internal static void ScanTypeAndInject(this IServiceCollection serviceCollection, Type implementationType)
    {
        var specks = ((SpeckAttribute[])implementationType.GetCustomAttributes(typeof(SpeckAttribute), false)).AsSpan();
        foreach (var speck in specks)
        {
            var serviceType = speck.ServiceType ?? implementationType;
            try
            {
                serviceCollection.AddSpeck(serviceType, implementationType, speck.ServiceLifetime);
            }
            catch (TypeAccessException ex)
            {
                if (implementationType.IsInterface)
                {
                    serviceCollection.ScanPropertiesAndInject(implementationType);
                    serviceCollection.ScanFieldsAndInject(implementationType);
                    serviceCollection.ScanMethodsAndInject(implementationType);
                    continue;
                }
                throw new TypeAccessException($"Specky could not inject service type {serviceType.Name} with implementation type {implementationType.Name} for an unknown reason.\n{speck.ServiceType?.Name ?? "null"}.{implementationType.Name}", ex);
            }
        }
    }

    internal static void ScanPropertiesAndInject(this IServiceCollection serviceCollection, Type type)
    {
        var propertyInfos = type.GetProperties().AsSpan();
        foreach (var propertyInfo in propertyInfos)
        {
            var specks = ((SpeckAttribute[])propertyInfo.GetCustomAttributes(typeof(SpeckAttribute), false)).AsSpan();
            foreach (var speck in specks)
            {
                try
                {
                    serviceCollection.AddSpeck(speck.ServiceType ?? propertyInfo.PropertyType, propertyInfo.PropertyType, speck.ServiceLifetime);
                }
                catch (TypeAccessException ex)
                {
                    throw new TypeAccessException($"{speck.ServiceType?.Name ?? "null"}.{type.Name}.{propertyInfo.Name}.{propertyInfo.PropertyType.Name}", ex);
                }
            }
        }
    }

    internal static void ScanMethodsAndInject(this IServiceCollection serviceCollection, Type type)
    {
        foreach (var methodInfo in type.GetMethods().AsSpan())
        {
            foreach (var speck in ((SpeckAttribute[])methodInfo.GetCustomAttributes(typeof(SpeckAttribute), false)).AsSpan())
            {
                var serviceType = speck.ServiceType ?? methodInfo.ReturnType;
                var implementationType = methodInfo.ReturnType;
                var serviceLifetime = speck.ServiceLifetime;

                try
                {
                    serviceCollection.AddSpeck(serviceType, implementationType, serviceLifetime);
                }
                catch (TypeAccessException ex)
                {
                    if (methodInfo.ReturnType == typeof(void) && speck is SpeckyConfigurationAttribute)
                    {
                        throw new TypeAccessException($"Specky configuration methods cannot return {typeof(void).Name}. The {nameof(methodInfo.ReturnType)} must be the {nameof(Type)} you want Specky to inject.\n{speck.ServiceType?.Name ?? typeof(void).Name}.{type.Name}.{methodInfo.Name}.{nameof(methodInfo.ReturnType.Name)}", ex);
                    }
                    throw;
                }
            }
        }
    }

    internal static void ScanFieldsAndInject(this IServiceCollection serviceCollection, Type type)
    {
        foreach (var fieldInfo in type.GetFields().AsSpan())
        {
            foreach (var speck in ((SpeckAttribute[])fieldInfo.GetCustomAttributes(typeof(SpeckAttribute), false)).AsSpan())
            {
                var serviceType = speck.ServiceType ?? fieldInfo.FieldType;
                var implementationType = fieldInfo.FieldType;
                var serviceLifetime = speck.ServiceLifetime;

                try
                {
                    serviceCollection.AddSpeck(serviceType, implementationType, serviceLifetime);
                }
                catch (TypeAccessException ex)
                {
                    throw new TypeAccessException($"Specky could not inject service type {serviceType.Name} with implementation type {implementationType.Name} for an unknown reason.\n{speck.ServiceType?.Name ?? "null"}.{type.Name}.{fieldInfo.Name}.{nameof(fieldInfo.FieldType.Name)}", ex);
                }
            }
        }
    }

    internal static void AddSpeck(this IServiceCollection serviceCollection, Type serviceType, Type implementationType, ServiceLifetime serviceLifetime)
    {
        if (!implementationType.IsAssignableTo(serviceType))
        {
            throw new TypeAccessException($"Specky cannot inject {implementationType.Name} type because it cannot be assigned to {serviceType.Name}.\n{serviceType.Name}.{implementationType.Name}");
        }
        if (implementationType.IsInterface)
        {
            throw new TypeAccessException($"Specky cannot inject {implementationType.Name} because it is an interface.\n{serviceType.Name}.{implementationType.Name}");
        }
        var serviceDescriptor = new ServiceDescriptor(serviceType, implementationType, serviceLifetime);
        serviceCollection.Add(serviceDescriptor);
    }
}
