using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
