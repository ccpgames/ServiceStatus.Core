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
    public abstract class HybridWebContentServiceStatusCheck : WebContentServiceStatusCheck, IHealthCheck
    {
        public HybridWebContentServiceStatusCheck(ILogger<WebContentServiceStatusCheck> logger, IHttpClientFactory httpClientFactory, Uri uri) : base(logger, httpClientFactory, uri)
        {
        }

        public HybridWebContentServiceStatusCheck(ILogger<WebContentServiceStatusCheck> logger, IHttpClientFactory httpClientFactory, Uri uri, HttpMethod httpMethod) : base(logger, httpClientFactory, uri, httpMethod)
        {
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return HybridHealthCheckUtilities.CheckHealthAsync(this, context, cancellationToken);
        }
    }
}
