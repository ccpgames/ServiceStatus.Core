using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace ServiceStatus.Core.AspNet.Hybrid
{
    public abstract class HybridResponsibilityServiceStatusCheck : ResponsibilityServiceStatusCheck, IHealthCheck
    {
        public HybridResponsibilityServiceStatusCheck(ILogger<ResponsibilityServiceStatusCheck> logger, IHttpClientFactory httpClientFactory, Uri uri, string[] responsibilities) : base(logger, httpClientFactory, uri, responsibilities)
        {
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return HybridHealthCheckUtilities.CheckHealthAsync(this, context, cancellationToken);
        }
    }
}
