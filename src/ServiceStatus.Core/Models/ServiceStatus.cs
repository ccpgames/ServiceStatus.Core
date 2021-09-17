using System.Diagnostics;

using Newtonsoft.Json;

using ServiceStatus.Core.Constants;

namespace ServiceStatus.Core.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ServiceStatus
    {
        /// <summary>
        /// Gets or sets service version
        /// </summary>
        [JsonProperty(PropertyName = "version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the branch the service was built from
        /// </summary>
        [JsonProperty(PropertyName = "branch", NullValueHandling = NullValueHandling.Ignore)]
        public string Branch { get; set; }

        /// <summary>
        /// Gets or sets the server responding to the request
        /// </summary>
        [JsonProperty(PropertyName = "server", NullValueHandling = NullValueHandling.Ignore)]
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the status of the service (OK is normal)
        /// </summary>
        [JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; } = StatusTypes.Unknown;

        /// <summary>
        /// Gets the string to be displayed in the debugger
        /// </summary>
        private string DebuggerDisplay => $"Status = {Status}, Server = {Server}, Version = {Version}, Branch = {Branch}";
    }
}
