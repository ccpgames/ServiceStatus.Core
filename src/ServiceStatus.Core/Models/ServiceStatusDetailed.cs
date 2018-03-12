using ServiceStatus.Core.Abstractions;
using ServiceStatus.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceStatus.Core.Models
{
    public class ServiceStatusDetailed : ServiceStatus
    {
        /// <summary>
        /// Gets the service status details
        /// </summary>
        public List<ServiceStatusDetailedEntry> Details { get; } = new List<ServiceStatusDetailedEntry>();

        /// <summary>
        /// Gets the service status responsibilities
        /// </summary>
        public Dictionary<string, string> Responsibilities { get; } = new Dictionary<string, string>() { { ResponsibilityTypes.Core, StatusTypes.OK } };

        public ServiceStatusDetailed() { }

        /// <summary>
        /// Initializes a new instance of the class
        /// </summary>
        /// <param name="checks">List of responsibilities</param>
        public ServiceStatusDetailed(Dictionary<IConfigurationStatusCheck, StatusCheckDetail> checks)
        {
            foreach (var check in checks)
            {
                Details.Add(new ServiceStatusDetailedEntry(
                    check.Key.Name,
                    check.Value.Value,
                    check.Value.ResponseTime));
            }
        }

        /// <summary>
        /// Initialize a new instance of the class
        /// </summary>
        /// <param name="checks"></param>
        /// <param name="responsibilityFilter">Filter checks by responsibility</param>
        public ServiceStatusDetailed(Dictionary<IServiceStatusCheck, StatusCheckDetail> checks, string responsibilityFilter = null)
        {
            // We have received a filtering request, empty the responsibilities
            if (!string.IsNullOrEmpty(responsibilityFilter))
                Responsibilities = new Dictionary<string, string>();

            foreach (var check in checks)
            {
                // Retrieve lists of keys for required and optional responsiblities
                IEnumerable<string> requiredFor = check.Key.Responsibilities.Where(x => x.Value == ServiceStatusRequirement.Required).Select(x => x.Key);
                IEnumerable<string> optionalFor = check.Key.Responsibilities.Where(x => x.Value == ServiceStatusRequirement.Optional).Select(x => x.Key);

                // Add the Add the service status detialed entry
                Details.Add(new ServiceStatusDetailedEntry(
                    check.Key.Name,
                    check.Value.Value,
                    check.Value.ResponseTime,
                    (string.IsNullOrEmpty(responsibilityFilter) && requiredFor.Any() ? requiredFor.ToArray() : null),
                    (string.IsNullOrEmpty(responsibilityFilter) && optionalFor.Any() ? optionalFor.ToArray() : null)));
            }

            // Gather responsibilities
            IEnumerable<string> responsibilities = !string.IsNullOrEmpty(responsibilityFilter) ?
                checks.Keys.SelectMany(statusCheck => statusCheck.Responsibilities.Where(x => x.Key.Equals(responsibilityFilter, StringComparison.OrdinalIgnoreCase)).Select(y => y.Key)).Distinct() :
                checks.Keys.SelectMany(statusCheck => statusCheck.Responsibilities.Select(x => x.Key)).Distinct();

            foreach (string responsibility in responsibilities)
            {
                var applicableServiceStatusChecks = checks.Keys
                    .Where(statusCheck => statusCheck.Responsibilities.Any(checkResponsibility => checkResponsibility.Key.Equals(responsibility, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                bool anyWantedFailures = applicableServiceStatusChecks
                    .Where(statusCheck => statusCheck.Responsibilities.Single(x => x.Key.Equals(responsibility, StringComparison.OrdinalIgnoreCase)).Value == ServiceStatusRequirement.Optional)
                    .Any(statusCheck => checks[statusCheck].Value != StatusTypes.OK);

                bool anyNeededFailures = applicableServiceStatusChecks
                    .Where(statusCheck => statusCheck.Responsibilities.Single(x => x.Key.Equals(responsibility, StringComparison.OrdinalIgnoreCase)).Value == ServiceStatusRequirement.Required)
                    .Any(statusCheck => checks[statusCheck].Value != StatusTypes.OK);

                string result = anyNeededFailures ? StatusTypes.Offline : anyWantedFailures ? StatusTypes.Degraded : StatusTypes.OK;

                if (!Responsibilities.ContainsKey(responsibility))
                {
                    Responsibilities.Add(responsibility, result);
                }
                else
                {
                    Responsibilities[responsibility] = result;
                }
            }
        }

        /// <summary>
        /// Validates the over all final status, in regards to any responsibilities and checks
        /// </summary>
        /// <param name="requiredResponsibilities">A list of responsibilities that will result in final status of ERROR if any are not returning OK</param>
        public void ValidateStatus(string[] requiredResponsibilities)
        {
            if (requiredResponsibilities != null)
            {
                Status = Responsibilities.Any(x => requiredResponsibilities.Any(y => x.Key == y) && x.Value != StatusTypes.OK) ? StatusTypes.Error : Details.Any(x => x.Value != StatusTypes.OK) ? StatusTypes.Degraded : StatusTypes.OK;
            }
            else
            {
                Status = Details.Any(x => x.Value != StatusTypes.OK) ? StatusTypes.Degraded : StatusTypes.OK;
            }
        }

        /// <summary>
        /// Validates the over all final status, in regards to any responsibilities and checks
        /// </summary>
        /// <param name="requiredResponsibility">Responsibility that will result in final status of ERROR if it is not returning OK</param>
        public void ValidateStatus(string requiredResponsibility) => ValidateStatus(new[] { requiredResponsibility });

        /// <summary>
        /// Validates the over all final status in regards to checks
        /// </summary>
        public void ValidateStatus() => ValidateStatus(requiredResponsibilities: null);
    }
}
