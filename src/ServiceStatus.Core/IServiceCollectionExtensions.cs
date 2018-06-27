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
            IEnumerable<ServiceDescriptor> serviceStatusImplementations =
                from assembly in assemblies
                from type in assembly.GetExportedTypes()
                where type.GetInterfaces().Contains(typeof(IServiceStatusCheck))
                select new ServiceDescriptor(typeof(IServiceStatusCheck), type, ServiceLifetime.Scoped);

            IEnumerable<ServiceDescriptor> configurationStatusImplementations =
                from assembly in assemblies
                from type in assembly.GetExportedTypes()
                let interfaces = type.GetInterfaces()
                where interfaces.Contains(typeof(IConfigurationStatusCheck)) && !interfaces.Contains(typeof(IServiceStatusCheck))
                select new ServiceDescriptor(typeof(IConfigurationStatusCheck), type, ServiceLifetime.Scoped);

            foreach (ServiceDescriptor serviceStatus in serviceStatusImplementations.Union(configurationStatusImplementations))
            {
                services.Add(serviceStatus);
            }

            return services;
        }
    }
}
