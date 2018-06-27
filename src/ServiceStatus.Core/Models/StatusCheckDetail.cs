using Newtonsoft.Json;

namespace ServiceStatus.Core.Models
{
    public class StatusCheckDetail
    {
        /// <summary>
        /// Gets the value of the service check
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; }

        /// <summary>
        /// Gets the response time of the service check in milliseconds
        /// </summary>
        [JsonProperty(PropertyName = "responseTime")]
        public long ResponseTime { get; }

        /// <summary>
        /// Initialize a new instance of StatusCheck class.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="responseTime">Response time in milliseconds</param>
        public StatusCheckDetail(string value, long responseTime)
        {
            Value = value;
            ResponseTime = responseTime;
        }
    }
}
