using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;

namespace Test.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly TelemetryClient _telemetryClient;

        public ValuesController(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("slow")]
        public ActionResult<IEnumerable<string>> Slow()
        {
            Thread.Sleep(TimeSpan.FromSeconds(5));

            _telemetryClient.TrackTrace("Statement 1");
            _telemetryClient.TrackTrace("Statement 2");

            return new string[] { "slow", "response" };
        }

        [HttpGet("exception")]
        public ActionResult<IEnumerable<string>> Exception()
        {
            _telemetryClient.TrackTrace("This will throw an exception");

            throw new Exception("Error");
        }

        [HttpGet("badrequest")]
        public ActionResult BadRequest()
        {
            return StatusCode(400);
        }

        [HttpGet("servererror")]
        public ActionResult ServerError()
        {
            return StatusCode(500);
        }

        [HttpGet("test-event")]
        public ActionResult TestEvent()
        {
            var eventTelemetry = new EventTelemetry("test-event");
            _telemetryClient.TrackEvent(eventTelemetry);

            var anotherEvent = new EventTelemetry("another-event");
            _telemetryClient.TrackEvent(anotherEvent);

            return StatusCode(200);
        }

        [HttpGet("long-dependency")]
        public ActionResult LongDependency()
        {
            var telemetry = new DependencyTelemetry {Type = "slow", Target = "slow-runner"};
            telemetry.Start();

            Thread.Sleep(5000);

            telemetry.Stop();
            _telemetryClient.TrackDependency(telemetry);

            return StatusCode(200);
        }

        [HttpGet("background-task")]
        public ActionResult BackGroundTask()
        {
           Task.Factory.StartNew(BackGroundTaskRunner);
           return StatusCode(200);
        }
        
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private void BackGroundTaskRunner()
        {
           var telemetry = new DependencyTelemetry {Type = "slow", Target = "slow-runner"};
           telemetry.Start();

           Thread.Sleep(5000);

           telemetry.Stop();
           _telemetryClient.TrackDependency(telemetry);
        }
    }
}
