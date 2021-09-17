using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ServiceStatus.Core.Constants;
using ServiceStatus.Core.Models;

namespace ServiceStatus.Core
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class WebContentServiceStatusCheck : ServiceStatusCheck
    {
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly Uri _uri;
        protected readonly HttpMethod _httpMethod;

        public WebContentServiceStatusCheck(ILogger<WebContentServiceStatusCheck> logger, IHttpClientFactory httpClientFactory, Uri uri) : this(logger, httpClientFactory, uri, HttpMethod.Get) { }

        public WebContentServiceStatusCheck(ILogger<WebContentServiceStatusCheck> logger, IHttpClientFactory httpClientFactory, Uri uri, HttpMethod httpMethod) : base(logger)
        {
            _httpClientFactory = httpClientFactory;
            _uri = uri;
            _httpMethod = httpMethod ?? HttpMethod.Get;
        }

        public override async Task<StatusCheckDetail> ExecuteStatusCheckAsync()
        {
            var timer = Stopwatch.StartNew();

            try
            {
                // Generate a new HTTP request message
                var request = new HttpRequestMessage(_httpMethod, _uri);

                // Retrieve the response from the web service
                HttpResponseMessage response = await _httpClientFactory.CreateClient().SendAsync(request, new CancellationTokenSource(5000).Token).ConfigureAwait(false);

                // Evaluate that the response is good
                return await EvaluateResponse(response).ConfigureAwait(false)
                    ? new StatusCheckDetail(StatusTypes.OK, timer.ElapsedMilliseconds)
                    : new StatusCheckDetail("Content check failed", timer.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, $"Unable to check web content at {_uri}. Reason: {e.Message}");

                // Run TCP check to see if the service is responsive
                StatusCheckDetail tcpCheck = await TcpConnectionCheckAsync(_uri.GetComponents(UriComponents.Host, UriFormat.Unescaped), int.Parse(_uri.GetComponents(UriComponents.StrongPort, UriFormat.Unescaped)), timer).ConfigureAwait(false);

                return tcpCheck.Value != StatusTypes.OK ? tcpCheck : new StatusCheckDetail(e.Message, timer.ElapsedMilliseconds);
            }
        }

        public abstract Task<bool> EvaluateResponse(HttpResponseMessage response);
    }
}
