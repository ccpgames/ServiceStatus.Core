using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using ServiceStatus.Core.Models;

namespace ServiceStatus.Core.AspNet.Hybrid
{
    internal static class HybridHealthCheckUtilities
    {
        public static async Task<HealthCheckResult> CheckHealthAsync(ServiceStatusCheck check, HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                StatusCheckDetail result = await check.ExecuteStatusCheckAsync().ConfigureAwait(false);
                return new HealthCheckResult(result.Value == "OK" ? HealthStatus.Healthy : HealthStatus.Unhealthy, result.Value);
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, exception: ex);
            }
        }
    }
}
