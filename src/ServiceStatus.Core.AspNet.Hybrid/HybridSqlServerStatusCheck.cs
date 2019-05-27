using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
