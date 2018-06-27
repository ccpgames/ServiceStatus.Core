using Microsoft.Extensions.Options;

namespace ServiceStatus.Core
{
    public class ServiceStatusSettings : IOptions<ServiceStatusSettings>
    {
        public string Version { get; set; } = "0.0.0.0";
        public string Branch { get; set; }
        public ServiceStatusSettings Value => this;
    }
}
