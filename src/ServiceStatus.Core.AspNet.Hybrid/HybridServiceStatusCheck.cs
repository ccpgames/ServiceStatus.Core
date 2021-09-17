using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace ServiceStatus.Core.AspNet.Hybrid
{
    public abstract class HybridServiceStatusCheck : ServiceStatusCheck, IHealthCheck
    {
        public HybridServiceStatusCheck(ILogger<HybridServiceStatusCheck> logger) : base(logger)
        {
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return HybridHealthCheckUtilities.CheckHealthAsync(this, context, cancellationToken);
        }
    }
}
