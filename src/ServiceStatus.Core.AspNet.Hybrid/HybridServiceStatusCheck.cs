using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using ServiceStatus.Core.Models;

namespace ServiceStatus.Core.AspNet.Hybrid
{
    public abstract class HybridServiceStatusCheck : ServiceStatusCheck, IHealthCheck
    {
        protected HybridServiceStatusCheck(ILogger<HybridServiceStatusCheck> logger) : base(logger)
        {
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return HybridHealthCheck.CheckHealthAsync(this, context, cancellationToken);
        }
    }

    internal static class HybridHealthCheck
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
