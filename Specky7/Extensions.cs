using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace Specky7;
public static class Extensions
{
    internal static readonly SpeckyOptions SpeckyOptions = new();

    public static IServiceCollection AddSpecks(this IServiceCollection serviceCollection)
        => serviceCollection.AddSpecks(opt => { });

    public static IServiceCollection AddSpecks(this IServiceCollection serviceCollection, Action<SpeckyOptions> options)
    {
        SpeckyOptions.Clear();
        options(SpeckyOptions);
        if (SpeckyOptions.Assemblies.Count == 0)
        {
            SpeckyOptions.AddAssemblies(Assembly.GetCallingAssembly());
        }

        if (SpeckyOptions.InterfaceTypes.Count > 0)
        {
            return InjectInterfaceConfigurationsOnly(serviceCollection);
        }

        if (SpeckyOptions.UseConfigurations)
        {
            var speckyConfigurationTypes = SpeckyOptions
                .Assemblies
                .SelectMany(assembly => assembly
                .GetTypes()
                .Where(type => type.GetCustomAttributes(typeof(SpeckyConfigurationAttribute), false).Length > 0))
                .ToArray().AsSpan();

            if (speckyConfigurationTypes.Length == 0)
            {
                throw new TypeAccessException($"Specky was expected to inject with configurations but none was found."); ;
            }
            SpeckyOptions.AddConfigurations(speckyConfigurationTypes);
            return InjectInterfaceConfigurationsOnly(serviceCollection);
        }

        var speckTypes = SpeckyOptions.Assemblies.SelectMany(assembly => assembly.GetTypes());
        foreach (var implementationType in speckTypes)
        {
            serviceCollection.ScanTypeAndInject(implementationType);
        }
        return serviceCollection;
    }

    private static IServiceCollection InjectInterfaceConfigurationsOnly(IServiceCollection serviceCollection)
    {
        foreach (var iface in SpeckyOptions.InterfaceTypes)
        {
            var speckyConfigurationAttribute = iface.GetCustomAttribute<SpeckyConfigurationAttribute>();
            if (speckyConfigurationAttribute == null) continue;

            if (SpeckyOptions.Options.Count > 0)
            {
                InjectInterfaceConfigurationsWithOptionsOnly(serviceCollection, iface, speckyConfigurationAttribute);
                continue;
            }
            ScanAndInjectInterace(serviceCollection, iface);
        }
        return serviceCollection;
    }

    private static void InjectInterfaceConfigurationsWithOptionsOnly(IServiceCollection serviceCollection, Type iface, SpeckyConfigurationAttribute speckyConfigurationAttribute)
    {
        if (SpeckyOptions.Options.Contains(speckyConfigurationAttribute.Option))
        {
            ScanAndInjectInterace(serviceCollection, iface);
        }
    }

    private static void ScanAndInjectInterace(IServiceCollection serviceCollection, Type iface)
    {
        serviceCollection.ScanPropertiesAndInject(iface);
        serviceCollection.ScanFieldsAndInject(iface);
        serviceCollection.ScanMethodsAndInject(iface);
    }

    //Primary - called first when needing to locate all specks and attempt injecting all.
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
