# SplunkLogger
This is a C# .Net Core 2 Splunk ILogger implementation developed at **VTEX** to send log to **Splunk**

| Package Name                   | Release |
|--------------------------------|-----------------|
| `SplunkLogger`         | [![NuGet](https://img.shields.io/nuget/v/SplunkLogger.svg)](https://www.nuget.org/packages/SplunkLogger/) |

It was developed to be integrated to .Net Core 2 logging abstractions.

### Features

* ILoggers **HEC** (*Raw* and *Json*) and **Socket** (*TCP* and *UDP*)
* Batch Manager class (Improve **Splunk** *HEC* performance sending data as batch)

## Usage

Add *SplunkLogger* nuget library
```powershell
PM> Install-Package SplunkLogger
```

Initialize a new *SplunkLoggerConfiguration* and the logger provider at **Configure** method at **Startup** class:

```csharp
static readonly ILoggerFormatter formatter = null;

public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    var splunkConfiguration = new SplunkLoggerConfiguration()
    {
        HecConfiguration = new HECConfiguration()
        {
            SplunkCollectorUrl = "https://localhost:8088/services/collector",
            Token = "753c5a9c-fb59-4da0-9064-947f99dc20ba"
        }
    };
    loggerFactory.AddHECRawSplunkLogger(splunkConfiguration, formatter);
    app.UseMvc();
}
```

The case above is using a **Splunk** *HEC Raw* logger without any custom formatter as log provider.

Now, in your controller you can log normally, for instance:

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

-------------------------------------------

# VTEXSplunkLogger
For us at **VTEX** we need more customized log entries at Splunk and also we need easier ways to call for log registration during the code. For that, we created this *VTEXSplunkLogger* library.

This project contains all **VTEX** extra classes designed to ease logging. All classes changes .Net Core 2 log abstraction to a customized log entry at **Splunk**

| Package Name                   | Release |
|--------------------------------|-----------------|
| `VTEXSplunkLogger` | [![MyGet](https://img.shields.io/myget/vtexlab/v/VTEXSplunkLogger.svg)](https://www.myget.org/feed/vtexlab/package/nuget/VTEXSplunkLogger) |

### Features

* ILoggerExtensions (*Allow easy log creation*)
* LoggerFactoryExtensions (*Simplify loggerFactory add provider method call*)
* VTEXSplunkLoggerFormatter (*Custom `ILoggerFormatter` which creates VTEX's custom text to Splunk*)


## Usage

Add *VTEXSplunkLogger* nuget library
```powershell
PM> Install-Package VTEXSplunkLogger -Source https://www.myget.org/F/vtexlab/api/v3/index.json
```

Initialize a new *SplunkLoggerConfiguration* and the logger provider at **Configure** method at **Startup** class:

```csharp
static readonly ILoggerFormatter formatter = new VTEXSplunkLoggerFormatter();

public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    //TODO: Set your project name
    ILoggerExtensions.SetApplication("ProjectX"); 
    
    var splunkConfiguration = new SplunkLoggerConfiguration()
    {
        HecConfiguration = new HECConfiguration()
        {
            SplunkCollectorUrl = "https://localhost:8088/services/collector",
            Token = "753c5a9c-fb59-4da0-9064-947f99dc20ba"
        }
    };
    loggerFactory.AddHECRawSplunkLogger(splunkConfiguration, formatter);
    app.UseMvc();
}
```

```csharp
using Vtex;
// other usings ..

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
        logger.DefineVTEXLog(LogLevel.Critical, 
                             "Values Controller", 
                             "api/values", 
                             string.Empty, 
                             new NotImplementedException(), 
                             new Tuple<string, string>("method", "GET"));
        return new string[] { "value1", "value2" };
    }
}
```
