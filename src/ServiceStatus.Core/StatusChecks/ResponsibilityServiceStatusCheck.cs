using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServiceStatus.Core.Constants;
using ServiceStatus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceStatus.Core
{
    public abstract class ResponsibilityServiceStatusCheck : WebContentServiceStatusCheck
    {
        private readonly string[] _responsibilities;

        public ResponsibilityServiceStatusCheck(ILogger<ResponsibilityServiceStatusCheck> logger, IHttpClientFactory httpClientFactory, Uri uri, string[] responsibilities) : base(logger, httpClientFactory, uri)
        {
            _responsibilities = responsibilities;
        }

        /// <summary>
        /// Evaluate the response from a HTTP service
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public override async Task<bool> EvaluateResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                ServiceStatusDetailed serviceStatus = JsonConvert.DeserializeObject<ServiceStatusDetailed>(result);

                if (serviceStatus?.Responsibilities != null)
                {
                    return _responsibilities.All(x =>
                    {
                        KeyValuePair<string, string> responsibility = serviceStatus.Responsibilities.FirstOrDefault(y => string.Compare(y.Key, x, true) == 0);
                        return string.Compare(responsibility.Value, StatusTypes.OK, true) == 0;
                    });
                }
            }

            return false;
        }
    }
}
