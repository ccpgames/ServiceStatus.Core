using Microsoft.Extensions.Logging;
using ServiceStatus.Core.Constants;
using ServiceStatus.Core.Models;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStatus.Core
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class WebContentServiceStatusCheck : ServiceStatusCheck
    {
        protected readonly HttpClient _httpClient;
        protected readonly Uri _uri;
        protected readonly HttpMethod _httpMethod;

        public WebContentServiceStatusCheck(ILogger logger, HttpClient httpClient, Uri uri) : this(logger, httpClient, uri, HttpMethod.Get) { }

        public WebContentServiceStatusCheck(ILogger logger, HttpClient httpClient, Uri uri, HttpMethod httpMethod) : base (logger) 
        {
            _httpClient = httpClient ?? new HttpClient();
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
                HttpResponseMessage response = await _httpClient.SendAsync(request, new CancellationTokenSource(5000).Token).ConfigureAwait(false);

                // Evaluate that the response is good
                if (await EvaluateResponse(response).ConfigureAwait(false))
                    return new StatusCheckDetail(StatusTypes.OK, timer.ElapsedMilliseconds);

                return new StatusCheckDetail("Content check failed", timer.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, $"Unable to check web content at {_uri}. Reason: {e.Message}");

                // Run TCP check to see if the service is responsive
                StatusCheckDetail tcpCheck = await TcpConnectionCheckAsync(_uri.GetComponents(UriComponents.Host, UriFormat.Unescaped), int.Parse(_uri.GetComponents(UriComponents.StrongPort, UriFormat.Unescaped)), timer).ConfigureAwait(false);

                if (tcpCheck.Value != StatusTypes.OK)
                    return tcpCheck;

                return new StatusCheckDetail(e.Message, timer.ElapsedMilliseconds);
            }
        }

        public abstract Task<bool> EvaluateResponse(HttpResponseMessage response);
    }
}
