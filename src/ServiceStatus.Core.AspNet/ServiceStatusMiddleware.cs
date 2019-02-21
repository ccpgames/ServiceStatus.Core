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
    public class ServiceStatusMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<ServiceStatusSettings> _settings;

        public ServiceStatusMiddleware(RequestDelegate next, IOptions<ServiceStatusSettings> settings)
        {
            _next = next;
            _settings = settings;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await HandleServiceStatusRequest(context, context.RequestServices.GetServices<IServiceStatusCheck>());
        }

        private async Task HandleServiceStatusRequest(HttpContext context, IEnumerable<IServiceStatusCheck> serviceStatusChecks)
        {
            string responsibility = context.Request.Query["responsibility"].ToString() ?? string.Empty;

            // Create a queryable list of service status checks
            IQueryable<IServiceStatusCheck> checksToMake = serviceStatusChecks.AsQueryable();

            // If the responsibility string has been set
            // filter to only the responsibilities that are requested
            if (!string.IsNullOrEmpty(responsibility))
            {
                checksToMake = checksToMake.Where(x => x.Responsibilities.Any(y => y.Key.Equals(responsibility, StringComparison.OrdinalIgnoreCase)));
            }

            // No checks to make
            if (checksToMake.Count() == 0)
            {
                context.Response.StatusCode = 404;
            }

            // Prepare a list of tasks to run through
            var checkTasks = checksToMake.ToDictionary(x => x, x => DoServiceCheck(x));

            await Task.WhenAll(checkTasks.Values);

            // Get the result of service checks in a new dictionary
            var checks = checkTasks.ToDictionary(x => x.Key, x => x.Value.Result);

            // Initialize the status object that we will be returning
            var status = new ServiceStatusDetailed(checks)
            {
                // Set version of your service
                Version = _settings.Value?.Version ?? "0.0.0.0",
                Branch = _settings.Value?.Branch ?? null
            };

            // Validate the status of this service, 
            // where no CORE responsibilities are allowed to fail
            status.ValidateStatus(ResponsibilityTypes.Core);

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(status));
        }

        /// <summary>
        /// Execute a status check and handle results
        /// </summary>
        /// <param name="statusCheck"></param>
        /// <returns></returns>
        private Task<StatusCheckDetail> DoServiceCheck(IConfigurationStatusCheck statusCheck)
        {
            // Start a new timer
            var timer = Stopwatch.StartNew();

            // Create a fetch task, which will be the execution of the status check
            Task<StatusCheckDetail> fetchTask = statusCheck.ExecuteStatusCheckAsync();

            return fetchTask.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return new StatusCheckDetail($"Exception: {task.Exception.Message}", timer.ElapsedMilliseconds);
                }

                return task.Result;
            });
        }

    }
}