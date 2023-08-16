using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentry;

namespace Api.Controllers
{
    [Route("test")]
    public class TestController : ControllerBase
    {
        [HttpGet("")]
        public object Index()
        {
            return new
            {
                timestamp = DateTime.Now,
                name = "Test Api",
                description = "This api allows you to test various features."
            };
        }

        [HttpGet("sentry")]
        public object TestSentry([FromQuery] bool error = false)
        {
            SentrySdk.CaptureMessage("Hello Sentry");

            if (error)
            {
                throw new NullReferenceException("Simulated Application Exception");
            }

            return new
            {
                timestamp = DateTime.Now,
                enabled = SentrySdk.IsEnabled
            };
        }

        [Authorize]
        [HttpGet("user")]
        public string? GetUser()
        {
            return User?.Identity?.Name;
        }
    }
}