using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Specky7;
public static class Extensions
{
    internal static readonly SpeckyOptions SpeckyOptions = new();
    internal static readonly HashSet<SpeckAttribute> SpeckyInitAttributes = new();

    public static IApplicationBuilder UseSpeckyPostSpecks(this IApplicationBuilder app)
    {
        foreach (var speckyInitAttribute in SpeckyInitAttributes)
        {
            _ = app.ApplicationServices.GetService(speckyInitAttribute.ServiceType!);
        }
        return app;
    }
    public static IServiceCollection AddSpecks<T>(this IServiceCollection serviceCollection)
        => serviceCollection.AddSpecks(opt => opt.AddAssembly<T>());

    public static IServiceCollection AddSpecks<T>(this IServiceCollection serviceCollection, Action<SpeckyOptions> options)
    {
        return serviceCollection.AddSpecks(opts =>
        {
            opts.AddAssembly<T>();
            options(opts);
        });
    }

    public static IServiceCollection AddSpecks(this IServiceCollection serviceCollection, Action<SpeckyOptions> options)
    {
        SpeckyOptions.Clear();
        var assembly = Assembly.GetEntryAssembly();
        options(SpeckyOptions);
        if (SpeckyOptions.Assemblies.Count == 0)
        {
            if (assembly == null) throw new TypeAccessException($"No assembly was found or registered for Specky to scan.\n{nameof(AddSpecks)}");
            SpeckyOptions.AddAssemblies(assembly);
        }

        if (SpeckyOptions.Configurations.Count > 0)
        {
            InjectInterfaceConfigurationsOnly(serviceCollection);
        }

        if (SpeckyOptions.UseConfigurationsOnly && SpeckyOptions.Configurations.Count == 0)
        {
            var speckyConfigurationTypes = SpeckyOptions
                .Assemblies
                .SelectMany(assembly => assembly
                .GetTypes()
                .Where(type => type.GetCustomAttributes(typeof(SpeckyConfigurationAttribute), false).Length > 0))
                .ToArray();

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
        foreach (var iface in SpeckyOptions.Configurations)
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
        serviceCollection.ScanPropertiesFromConfigurationAndInject(iface);
        serviceCollection.ScanFieldsAndInject(iface);
        serviceCollection.ScanMethodsAndInject(iface);
    }

    //Primary - called first when needing to locate all specks and attempt injecting all.
    internal static void ScanTypeAndInject(this IServiceCollection serviceCollection, Type implementationType)
    {
        var specks = (SpeckAttribute[])implementationType.GetCustomAttributes(typeof(SpeckAttribute), false);
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

            if (speck.IsPostInit) SpeckyInitAttributes.Add(speck);
        }
    }

    internal static void ScanPropertiesFromConfigurationAndInject(this IServiceCollection serviceCollection, Type type)
    {
        var propertyInfos = type.GetProperties();
        foreach (var propertyInfo in propertyInfos)
        {
            var specks = (SpeckAttribute[])propertyInfo.GetCustomAttributes(typeof(SpeckAttribute), false);
            foreach (var speck in specks)
            {
                var serviceType = speck.ServiceType ?? propertyInfo.PropertyType;
                try
                {
                    serviceCollection.AddSpeck(serviceType, propertyInfo.PropertyType, speck.ServiceLifetime);
                }
                catch (TypeAccessException ex)
                {
                    throw new TypeAccessException($"{speck.ServiceType?.Name ?? "null"}.{type.Name}.{propertyInfo.Name}.{propertyInfo.PropertyType.Name}", ex);
                }
                SpeckyOptions.ConfigurationAddedServiceTypes.Add(serviceType);
            }
        }
    }

    internal static void ScanMethodsAndInject(this IServiceCollection serviceCollection, Type type)
    {
        foreach (var methodInfo in type.GetMethods())
        {
            foreach (var speck in (SpeckAttribute[])methodInfo.GetCustomAttributes(typeof(SpeckAttribute), false))
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
                    if (methodInfo.ReturnType == typeof(void))
                    {
                        throw new TypeAccessException($"Specky configuration methods cannot return {typeof(void).Name}. The {nameof(methodInfo.ReturnType)} must be the {nameof(Type)} you want Specky to inject.\n{speck.ServiceType?.Name ?? typeof(void).Name}.{type.Name}.{methodInfo.Name}.{nameof(methodInfo.ReturnType.Name)}", ex);
                    }
                    throw;
                }
                SpeckyOptions.ConfigurationAddedServiceTypes.Add(serviceType);
            }
        }
    }

    internal static void ScanFieldsAndInject(this IServiceCollection serviceCollection, Type type)
    {
        foreach (var fieldInfo in type.GetFields())
        {
            foreach (var speck in (SpeckAttribute[])fieldInfo.GetCustomAttributes(typeof(SpeckAttribute), false))
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
                SpeckyOptions.ConfigurationAddedServiceTypes.Add(serviceType);
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
        if (SpeckyOptions.ConfigurationAddedServiceTypes.Contains(serviceType)) return;
        var serviceDescriptor = new ServiceDescriptor(serviceType, implementationType, serviceLifetime);
        serviceCollection.Add(serviceDescriptor);
    }
}
