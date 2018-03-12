# ServiceStatus.Core

With this package you can easily make a consistent service status JSON, with the ability to query other services to see connectivity and health.

## Usage example

Getting started with ServiceStatus.Core is kept simple by using the DependencyInjection package. In your startup simply add the following code:

```cs
public void ConfigureServices(IServiceCollection services)
{
	// Enable ServiceStatus.Core, the assembly referenced should be the one
	// where StatusChecks are implemented. Alternately use typeof(T).Assembly
	services.UseServiceStatus(GetType().Assembly);
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
            return true;
        }

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

    public LoginServiceServiceCheck(ILogger logger, HttpClient httpClient, IOptions<LoginServiceSettings> settings) : base(logger, httpClient, ServiceStatusUri(settings.Value.ServiceUri), _responsibilitiesToCheck)
    {
    }

    private static Uri ServiceStatusUri(Uri uri)
    {
        // Path to where the service should respond with 200 when there are no problems
        var builder = new UriBuilder(uri) { Path = "version" };
        return builder.Uri;
    }

    public override Dictionary<string, ServiceStatusRequirement> Responsibilities => _responsibilities;

    public override string Name => "LoginService";

    public override TimeSpan? CacheDuration => _cacheDuration;

    public override bool IsEnabled() => true;
}
```