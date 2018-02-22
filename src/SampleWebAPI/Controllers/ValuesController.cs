using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Splunk.SampleWebAPI.Controllers
{
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
            logger.Log(LogLevel.Trace, new EventId(-1, "Values Controller"), new { route = "Get" }, exception, 
                       (argState, argException) => { 
                return string.Format("{0} {1}", 
                                     argState != null ? argState.ToString() : string.Empty,
                                     argException != null ? argException.ToString() : string.Empty);
            });
            return new string[] { "4", "2" };
        }
    }
}