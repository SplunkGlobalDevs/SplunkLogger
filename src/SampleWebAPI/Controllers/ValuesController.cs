using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Vtex.SampleWebAPI.Controllers
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
            logger.Log(LogLevel.Critical,
                       new EventId(-1, "Values Controller"),
                       new { route = "Get" },
                       exception, null);
            throw exception;
        }
    }
}