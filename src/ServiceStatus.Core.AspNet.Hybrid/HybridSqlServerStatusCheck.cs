using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace ServiceStatus.Core.AspNet.Hybrid
{
    public abstract class HybridSqlServerStatusCheck : SqlServerStatusCheck, IHealthCheck
    {
        protected HybridSqlServerStatusCheck(ILogger<SqlServerStatusCheck> logger) : base(logger)
        {
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return HybridHealthCheckUtilities.CheckHealthAsync(this, context, cancellationToken);
        }
    }
}
