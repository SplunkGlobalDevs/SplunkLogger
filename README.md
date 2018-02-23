# SplunkLogger
This is a C# .Net Core 2 ILogger implementation developed by **VTEX** developer (@[Caldas](https://github.com/Caldas)) to send data to Splunk.

### Features

* Multiples ILoggers to send data via **Http** or **Socket**
  * **Http** loggers available to send data via **Raw** or **Json** routes
  * **Socket** loggers available to send data via **TCP** or **UDP**
* Send **Http** events as batch (Improve **Splunk** *HEC* performance sending data as batch)

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

Let's say for instance that your are creating a WebAPI project, so the first step is to configure one of the Splunk loggers options
```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    var splunkLoggerConfiguration = GetSplunkLoggerConfiguration(app);
    
    //Append Http Raw logger 
    //loggerFactory.AddHECRawSplunkLogger(splunkLoggerConfiguration);
    
    //Append Http Json logger
    loggerFactory.AddHECJsonSplunkLogger(splunkConfiguration);
    
    //Append Socket TCP logger
    //loggerFactory.AddTcpSplunkLogger(splunkConfiguration);
    
    //Append Socket UDP logger
    //loggerFactory.AddUdpSplunkLogger(splunkConfiguration);
}
```

As you can see, no matter what is your option you always must delivery a *SplunkLoggerConfiguration* that can be provided by two ways:

#### Get Configuration From Json File

First, and most beautiful one. 

You can provide the configuration from json file. In this case I will be using *appsettings.json* as source and loading it using .Net Core 2 configuration binding feature.

So first, let's check the json file. 
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

If you intend to use send data via **Http** you should set **HecConfiguration** section and if you choose to send data via socket you must set **SocketConfiguration** section.

Now we need to configure SplunkLoggerConfiguration and indicate to use **Splunk** section from configuration file.
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

You can also provide a hard coded configuration instance:

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

If you intend to use send data via **Http** you should set **HecConfiguration** property and if you choose to send data via socket you must set **SocketConfiguration** property.
 
### Sample Logging 

At controller you must receive `ILoggerFactory loggerFactory` and extract `ILogger`from it and use this `ILogger` instance to log any desired event, as sample below:

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
        logger.LogCritical(new EventId(-1, "Values Controller"), new NotImplementedException(), "Error on GET api/values route");
        return new string[] { "value1", "value2" };
    }
}
```

### Sending Http Events As Batchs

**To Be Definied**
