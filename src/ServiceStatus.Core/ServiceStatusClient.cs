using ServiceStatus.Core.Abstractions;
using ServiceStatus.Core.Constants;
using ServiceStatus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceStatus.Core
{
    public class ServiceStatusClient
    {

        /// <summary>
        /// Processes the service status object
        /// </summary>
        /// <param name="checks">Dependency checks</param>
        /// <param name="responsibility">Optional responsibility filtering</param>
        /// <returns>ServiceStatusDetailed object, or null if nothing is found</returns>
        private ServiceStatusDetailed ProcessServiceStatus(Dictionary<IServiceStatusCheck, StatusCheckDetail> checks, string responsibility = null)
        {
            var status = new ServiceStatusDetailed(checks, responsibility);

            if (!string.IsNullOrEmpty(responsibility))
            {
                // Retrieve the responsibilities matching the requested filter
                KeyValuePair<string, string> result = status.Responsibilities.Where(x => x.Key.Equals(responsibility, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();

                // If there is nothing found, return null
                if (!string.IsNullOrEmpty(result.Value))
                    return null;

                // fill the responsibilities of our new status with the values provided
                status.Status = result.Value;
            }
            else
            {
                status.Status = status.Responsibilities[ResponsibilityTypes.Core] != StatusTypes.OK ? StatusTypes.Error : checks.Values.Any(x => x.Value != StatusTypes.OK) ? StatusTypes.Degraded : StatusTypes.OK;
            }

            return status;
        }

        /// <summary>
        /// Process the service status object
        /// </summary>
        /// <param name="checks">Dependency checks</param>
        /// <returns>ServiceStatusDetailed object</returns>
        private ServiceStatusDetailed ProcessServiceStatus(Dictionary<IConfigurationStatusCheck, StatusCheckDetail> checks)
        {
            var status = new ServiceStatusDetailed(checks);

            status.Status = status.Responsibilities[ResponsibilityTypes.Core] != StatusTypes.OK ? StatusTypes.Error : checks.Values.Any(x => x.Value != StatusTypes.OK) ? StatusTypes.Degraded : StatusTypes.OK;

            return status;
        }
    }
}
