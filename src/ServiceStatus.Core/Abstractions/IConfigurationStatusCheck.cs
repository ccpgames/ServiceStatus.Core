using ServiceStatus.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStatus.Core.Abstractions
{
    public interface IConfigurationStatusCheck
    {
        /// <summary>
        /// Name of the status check element
        /// </summary>
        string Name { get; }

        /// <summary>
        /// How long should the result be cached
        /// </summary>
        TimeSpan? CacheDuration { get; }

        /// <summary>
        /// Execute the status check
        /// </summary>
        /// <returns></returns>
        Task<StatusCheckDetail> ExecuteStatusCheckAsync();

        bool IsEnabled();
    }
}
