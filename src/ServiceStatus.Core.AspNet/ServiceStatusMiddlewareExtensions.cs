using ServiceStatus.Core.AspNet;

namespace Microsoft.AspNetCore.Builder
{
    public static class ServiceStatusMiddlewareExtensions
    {
        public static IApplicationBuilder UseServiceStatus(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ServiceStatusMiddleware>();
        }
    }
}
