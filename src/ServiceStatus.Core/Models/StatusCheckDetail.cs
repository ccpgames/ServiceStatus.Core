namespace ServiceStatus.Core.Models
{
    public class StatusCheckDetail
    {
        /// <summary>
        /// Gets the value of the service check
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the response time of the service check in milliseconds
        /// </summary>
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
