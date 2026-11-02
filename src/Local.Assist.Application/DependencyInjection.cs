using Local.Assist.Application.Abstractions.Behaviors;
using Local.Assist.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Local.Assist.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services.AddCustomTools();
    }

    private static IServiceCollection AddCustomTools(this IServiceCollection services)
    {
        services.AddSingleton<CustomToolInvoker>();

        services.Scan(scan => scan
            .FromAssemblyOf<ICustomTool>()
            .AddClasses(classes => classes.AssignableTo(typeof(ICustomToolHandler<>)), publicOnly: false)
            .AsSelf()
            .WithScopedLifetime());

        var handlerTypes = services
            .Where(descriptor => descriptor.ServiceType == descriptor.ImplementationType)
            .Select(descriptor => descriptor.ImplementationType!)
            .Where(type => type is not null && type.GetInterfaces().Any(contract =>
                contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(ICustomToolHandler<>)))
            .Distinct()
            .ToArray();

        foreach (var handlerType in handlerTypes)
        {
            var handlerContracts = handlerType
                .GetInterfaces()
                .Where(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(ICustomToolHandler<>));

            foreach (var handlerContract in handlerContracts)
            {
                var customToolType = handlerContract.GenericTypeArguments[0];
                services.AddKeyedScoped(handlerContract, customToolType.Name, handlerType);
            }
        }

        return services;
    }
}
