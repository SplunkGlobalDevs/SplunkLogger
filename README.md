# SplunkLogger

This is a C# .Net Core 2 ILogger implementation developed by **VTEX** developer [Caldas](https://github.com/Caldas) to send data to Splunk.

### Features

* Multiples ILoggers to send data via **Http** or **Socket**
  * **Http** loggers available to send data via **Raw** or **Json** routes
  * **Socket** loggers available to send data via **TCP** or **UDP**
* Send **Http** events as batch (Improve **Splunk** *HEC* performance sending data as batch)
* **ILoggerFormatter** that enable you to handle and formart your logs before send it to Splunk

### NuGet Package Status

| Package Name                   | Release |
|--------------------------------|-----------------|
| `SplunkLogger`         | [![NuGet](https://img.shields.io/nuget/v/SplunkLogger.svg)](https://www.nuget.org/packages/SplunkLogger/) |

## Usage

Add *SplunkLogger* nuget library
```powershell
PM> Install-Package SplunkLogger
```

### Configure Logger

Let's say for instance that you are creating a WebAPI project, so the first step is to configure one of the Splunk loggers options:
```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    var splunkLoggerConfiguration = GetSplunkLoggerConfiguration(app);
    
    //Append Http Raw logger 
    //loggerFactory.AddHECRawSplunkLogger(splunkLoggerConfiguration);
    
    //Append Http Json logger
    loggerFactory.AddHECJsonSplunkLogger(splunkLoggerConfiguration);
    
    //Append Socket TCP logger
    //loggerFactory.AddTcpSplunkLogger(splunkLoggerConfiguration);
    
    //Append Socket UDP logger
    //loggerFactory.AddUdpSplunkLogger(splunkLoggerConfiguration);
}
```

As you can see, no matter what is your option you always must delivery a *SplunkLoggerConfiguration* when adding a ILogger to the logger factory. You can provide it via config or as hard code:

#### Get Configuration From Json File

You can provide the configuration from json file using .Net Core 2 configuration binding feature. 

For instance at [SampleWebAPI project](https://github.com/vtex/SplunkLogger/tree/master/src/SampleWebAPI) we use the *appsettings.json* file.

```json
{
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Trace",
      "System": "Debug",
      "Microsoft": "Debug",
      "Splunk": "Trace"
    }
  },
  "Splunk": {
    "HecConfiguration": {
      "BatchIntervalInMilliseconds": 5000,
      "BatchSizeCount": 10,
      "ChannelIdType": "None",
      "DefaultTimeoutInMiliseconds": 10000,
      "SplunkCollectorUrl": "https://localhost:8088/services/collector/",
      "Token": "753c5a9c-fb59-4da0-9064-947f99dc20ba",
      "UseAuthTokenAsQueryString": false
    },
    "SocketConfiguration": {
      "HostName": "localhost",
      "Port": 4242
    }
  }
}
```

If you intend to send data via **Http** you should set **HecConfiguration** section and if you choose to send data via socket you must set **SocketConfiguration** section.

Now we need to configure SplunkLoggerConfiguration at dependency injection and indicate to use **Splunk** section from configuration file.
```csharp
/// <summary>
/// This method gets called by the runtime. Use this method to add services to the container.
/// </summary>
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<SplunkLoggerConfiguration>(Configuration.GetSection("Splunk"));
    services.AddMvc();
}
```

Now that `SplunkLoggerConfiguration` class is configured we can retrieve it by requesting `IOptions<SplunkLoggerConfiguration>` at dependency injection service, like:
```csharp
/// <summary>
/// Demonstrate how can you provide configuration to your splunk logger addapter(s) 
/// </summary>
SplunkLoggerConfiguration GetSplunkLoggerConfiguration(IApplicationBuilder app)
{
    SplunkLoggerConfiguration result = null;
    var splunkLoggerConfigurationOption = app.ApplicationServices.GetService<IOptions<SplunkLoggerConfiguration>>();
    if(splunkLoggerConfigurationOption != null && splunkLoggerConfigurationOption.Value != null)
        result = app.ApplicationServices.GetService<IOptions<SplunkLoggerConfiguration>>().Value;
    return result;
}
```

#### Get Static Configuration

If you don't want to use the configuration file, you can provide a hard coded configuration instance:

```csharp
/// <summary>
/// Demonstrate how can you provide configuration to your splunk logger addapter(s) 
/// </summary>
SplunkLoggerConfiguration GetSplunkLoggerConfiguration(IApplicationBuilder app)
{
    SplunkLoggerConfiguration result = new SplunkLoggerConfiguration()
    {
        HecConfiguration = new HECConfiguration()
        {
            SplunkCollectorUrl = "https://localhost:8088/services/collector",
            BatchIntervalInMilliseconds = 5000,
            BatchSizeCount = 100,
            ChannelIdType = HECConfiguration.ChannelIdOption.None,
            Token = "753c5a9c-fb59-4da0-9064-947f99dc20ba"
        },
        SocketConfiguration = new SocketConfiguration()
        {
            HostName = "localhost",
            Port = 8111
        }
    };
    return result;
}
```

Again, if you intend to use send data via **Http** you should set **HecConfiguration** property and if you choose to send data via socket you must set **SocketConfiguration** property.
 
### Logging 

Now that everything is configured and ILoggerFactory already have the desired ILogger instance added you can log your messages.

Here is a sample of how you can log messages, in this case I'm logging a `NotImplementedException` at [SampleWebAPI project](https://github.com/vtex/SplunkLogger/tree/master/src/SampleWebAPI) `ValuesController`.

```csharp
[Route("api/[controller]")]
public class ValuesController : Controller
{
    readonly ILogger logger;

    public ValuesController(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger<ValuesController>();
    }

    // GET api/values
    [HttpGet]
    public IEnumerable<string> Get()
    {
         var exception = new NotImplementedException();
         var message = "An error has ocurried route=Get";
         var eventId = new EventId(-1, "Values Controller");

         //You can log like this
         logger.Log(LogLevel.Trace, eventId, message, exception);
         //Or like this
         //logger.LogTrace(eventId, exception, message);

         return new string[] { "4", "2" };
    }
}
```

## More Information

You can read more about the projects and it's details (like send http events as batch) at [Wiki page](https://github.com/vtex/SplunkLogger/wiki)

## Project Sponsored By

**[VTEX](https://www.vtex.com)** 
