using ServiceStatus.Core.Models;
using System.Collections.Generic;

namespace ServiceStatus.Core.Abstractions
{
    public interface IServiceStatusCheck : IConfigurationStatusCheck
    {
        Dictionary<string, ServiceStatusRequirement> Responsibilities { get; }
    }
}
