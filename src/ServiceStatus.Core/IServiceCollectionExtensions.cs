using ServiceStatus.Core.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection UseServiceStatus(this IServiceCollection services, params Assembly[] assemblies)
        {
            IEnumerable<ServiceDescriptor> serviceStatusImplementations = assemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(type => type.GetInterfaces().Contains(typeof(IServiceStatusCheck)))
                .Select(type => new ServiceDescriptor(typeof(IServiceStatusCheck), type, ServiceLifetime.Scoped));

            IEnumerable<ServiceDescriptor> configurationStatusImplementations = assemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(type => type.GetInterfaces().Contains(typeof(IConfigurationStatusCheck)) && !type.GetInterfaces().Contains(typeof(IServiceStatusCheck)))
                .Select(type => new ServiceDescriptor(typeof(IServiceStatusCheck), type, ServiceLifetime.Scoped));

            foreach (ServiceDescriptor serviceStatus in serviceStatusImplementations.Union(configurationStatusImplementations))
            {
                services.Add(serviceStatus);
            }

            return services;
        }
    }
}
