using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StaticLogger;
using System.Collections.Generic;

namespace Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger _staticLogger = Logger.CreateLogger("Sample Logger");

        private readonly ILogger<ValuesController> _logger;

        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            using (_logger.BeginScope("Correlation {CorrelationID}", 12345))
            {
                _staticLogger.LogInformation("Log from static logger");
                _logger.LogInformation("Hello there");
            }
            return new string[] { "value1", "value2" };
        }
    }
}
