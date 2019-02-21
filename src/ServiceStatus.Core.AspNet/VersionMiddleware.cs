using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServiceStatus.Core.Abstractions;
using ServiceStatus.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}