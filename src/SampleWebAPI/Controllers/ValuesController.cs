using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace VTEX.SampleWebAPI.Controllers
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
            logger.DefineVTEXLog(LogLevel.Critical, 
                                 "Values Controller", 
                                 "api/values", 
                                 string.Empty, 
                                 new NotImplementedException(), 
                                 new Tuple<string, string>("method", "GET"));
            return new string[] { "value1", "value2" };
        }
    }
}