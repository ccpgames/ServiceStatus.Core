using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ServiceStatus.Core.AspNet
{
    public class VersionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<ServiceStatusSettings> _settings;

        public VersionMiddleware(RequestDelegate next, IOptions<ServiceStatusSettings> settings)
        {
            _next = next;
            _settings = settings;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(_settings.Value?.Version ?? "0.0.0.0");
        }
    }
}