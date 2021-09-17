using System.Collections.Generic;

using ServiceStatus.Core.Models;

namespace ServiceStatus.Core.Abstractions
{
    public interface IServiceStatusCheck : IConfigurationStatusCheck
    {
        Dictionary<string, ServiceStatusRequirement> Responsibilities { get; }
    }
}
