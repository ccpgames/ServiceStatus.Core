using ServiceStatus.Core.AspNet;

namespace Microsoft.AspNetCore.Builder
{
    public static class ServiceStatusMiddlewareExtensions
    {
        public static IApplicationBuilder UseServiceStatus(this IApplicationBuilder builder)
        {
            return builder
                .Map("/version", app => app.UseMiddleware<VersionMiddleware>())
                .Map("/servicestatus", app => app.UseMiddleware<ServiceStatusMiddleware>())
                .Map("/servicestatusdetailed", app => app.UseMiddleware<ServiceStatusMiddleware>());
        }
    }
}
