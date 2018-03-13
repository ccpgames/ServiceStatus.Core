# ServiceStatus.Core

With this package you can easily make a consistent service status JSON, with the ability to query other services to see connectivity and health.

## Usage example

Getting started with ServiceStatus.Core is kept simple by using the DependencyInjection package. In your startup simply add the following code:

```cs
public void ConfigureServices(IServiceCollection services)
{
	...

	// Enable ServiceStatus.Core, the assembly referenced should be the one
	// where StatusChecks are implemented. Alternately use typeof(T).Assembly
	services.UseServiceStatus(GetType().Assembly);

	...
}
```

Once completed you need to add some StatusCheck classes, depending on what you want to look up. Lets go with a web service that is a dependency for your service. 

Assuming this service does not have an identical service status / ServiceStatus.Core implemented, we will create a simple abstraction for checks to see if a web service responds with HttpStatusCode 200 OK.

If your service is running ServiceStatus.Core, you may use `ResponsibilityServiceStatusCheck` instead.

```cs
public abstract class IsSuccessStatusCodeServiceStatusCheck : WebContentServiceStatusCheck
{
    public IsSuccessStatusCodeServiceStatusCheck(ILogger logger, HttpClient httpClient, Uri uri) : base(logger, httpClient, uri) { }

    /// <summary>
    /// Evaluate the response from a HTTP service
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public override async Task<bool> EvaluateResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            // We successfully contacted the service
            return true;
        }

        // Service could not be reached
        return false;
    }
}
```

Once the `IsSuccessStatusCodeServiceStatusCheck` has been implemented, you can now create a service check like so...

```cs
public class LoginServiceServiceCheck : IsSuccessStatusCodeServiceStatusCheck
{
    private static readonly Dictionary<string, ServiceStatusRequirement> _responsibilities = new Dictionary<string, ServiceStatusRequirement>
    {
        { ResponsibilityTypes.Core, ServiceStatusRequirement.Required }
    };

    private static readonly TimeSpan _cacheDuration = null;
    private static readonly string[] _responsibilitiesToCheck = new[] { ResponsibilityTypes.Core };

	// We retrieve the service Uri from IOptions
	// LoginServiceSettings.ServiceUri in this case
    public LoginServiceServiceCheck(ILogger logger, HttpClient httpClient, IOptions<LoginServiceSettings> settings) : base(logger, httpClient, ServiceStatusUri(settings.Value.ServiceUri), _responsibilitiesToCheck)
    {
    }

    private static Uri ServiceStatusUri(Uri uri)
    {
        // Path to where the service should respond with 200 when there are no problems
        var builder = new UriBuilder(uri) { Path = "AlwaysOKIfServiceIsUp" };
        return builder.Uri;
    }

	// Responsibilities this check belongs to
    public override Dictionary<string, ServiceStatusRequirement> Responsibilities => _responsibilities;

	// Name of the check
    public override string Name => "LoginService";

	// Cache duration is not used in this example
	// but could be added to the evaluation
    public override TimeSpan? CacheDuration => _cacheDuration;

	// Is the check enabled
	public override bool IsEnabled() => true;
}
```

Now you're able to query your service to see if it's at least up and answering 200 OK at a specific path. So to display your service status, we need to create a controller.

```cs
public class ServiceStatusController : Controller
{
    private readonly ILogger _logger;
    private readonly IEnumerable<IServiceStatusCheck> _serviceStatusChecks;
    private readonly IEnumerable<IConfigurationStatusCheck> _configurationStatusChecks;

    public ServiceStatusController(ILogger<ServiceStatusController> logger, IEnumerable<IServiceStatusCheck> serviceStatusChecks, IEnumerable<IConfigurationStatusCheck> configurationStatusChecks)
    {
        _logger = logger;
        _serviceStatusChecks = serviceStatusChecks;
        _configurationStatusChecks = configurationStatusChecks;
    }

    [HttpGet]
    public async Task<IActionResult> GetServiceStatusChecks(string responsibility = null)
    {
        // Create a queryable list of service status checks
        IQueryable<IServiceStatusCheck> checksToMake = _serviceStatusChecks.AsQueryable();

        // If the responsibility string has been set
        // filter to only the responsibilities that are requested
        if (!string.IsNullOrEmpty(responsibility))
            checksToMake = checksToMake.Where(x => x.Responsibilities.Any(y => y.Key.Equals(responsibility, StringComparison.OrdinalIgnoreCase)));

        // No checks to make
        if (checksToMake.Count() == 0)
            return NotFound();

        // Prepare a list of tasks to run through
        var checkTasks = checksToMake.ToDictionary(x => x, x => DoServiceCheck(x));

        await Task.WhenAll(checkTasks.Values);

        // Get the result of service checks in a new dictionary
        var checks = checkTasks.ToDictionary(x => x.Key, x => x.Value.Result);

        // Initialize the status object that we will be returning
        var status = new ServiceStatusDetailed(checks)
        {
            // Set version of your service
            Version = "1.0.0-QA"
        };

        // Validate the status of this service, 
        // where no CORE responsibilities are allowed to fail
        status.ValidateStatus(ResponsibilityTypes.Core);

        return Ok(status);
    }

    /// <summary>
    /// Execute a status check and handle results
    /// </summary>
    /// <param name="statusCheck"></param>
    /// <returns></returns>
    private Task<StatusCheckDetail> DoServiceCheck(IConfigurationStatusCheck statusCheck)
    {
        // Start a new timer
        var timer = Stopwatch.StartNew();

        // Create a fetch task, which will be the execution of the status check
        Task<StatusCheckDetail> fetchTask = statusCheck.ExecuteStatusCheckAsync();

        return fetchTask.ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                return new StatusCheckDetail($"Exception: {task.Exception.Message}", timer.ElapsedMilliseconds);
            }

            return task.Result;
        });
    }
}
```