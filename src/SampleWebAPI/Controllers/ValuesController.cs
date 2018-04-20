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
            var message = "An error has ocurried route=Get";
            var eventId = new EventId(-1, "Values Controller");

            //You can log like this
            logger.Log(LogLevel.Trace, eventId, message, exception);
            //Or like this
            logger.LogTrace(eventId, exception, message);

            return new string[] { "4", "2" };
        }
    }
}