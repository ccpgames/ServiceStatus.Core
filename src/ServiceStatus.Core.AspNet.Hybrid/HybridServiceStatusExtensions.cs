using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ServiceStatus.Core.Abstractions;
using ServiceStatus.Core.AspNet;
using System;
using System.Linq;
using System.Reflection;

namespace ServiceStatus.Core.AspNet.Hybrid
{

    public static class ServiceStatusMiddlewareExtensions
    {
        public static IServiceCollection UseHybridServiceStatus(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.UseServiceStatus(assemblies);

            IHealthChecksBuilder healthChecksBuilder = services.AddHealthChecks();

            foreach (ServiceDescriptor descriptor in services.Where(d => d.ServiceType == typeof(IServiceStatusCheck)).ToList())
            {
                if (descriptor.ServiceType == typeof(IServiceStatusCheck))
                {
                    PropertyInfo nameProperty = descriptor.ImplementationType.GetProperty("HealthName");
                    if (nameProperty == null || nameProperty.GetMethod == null || !nameProperty.GetMethod.IsStatic)
                    {
                        throw new ApplicationException($"{descriptor.ImplementationType.Name} does not have a public static property HealthName with a getter");
                    }

                    healthChecksBuilder.Add(new HealthCheckRegistration((string)nameProperty.GetValue(null), (IServiceProvider s) => (IHealthCheck)s.GetServices<IServiceStatusCheck>().First(x => x.GetType() == descriptor.ImplementationType), null, null));
                }
            }

            return services;
        }

        public static IApplicationBuilder UseHybridServiceStatus(this IApplicationBuilder builder)
        {
            builder.UseServiceStatus();
            builder.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
            {
                ResponseWriter = async (HttpContext context, HealthReport report) =>
                {
                    context.Response.StatusCode = report.Status == HealthStatus.Healthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;
                    context.Response.ContentType = "application/json";
                    JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
                    serializerSettings.Converters.Add(new StringEnumConverter());
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(report, serializerSettings));
                }
            });

            return builder;
        }
    }
}