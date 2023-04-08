using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet("")]
        public object Get()
        {
            return new
            {
                timestamp = DateTime.Now,
                name = "stateful-serverless-api",
                description = "working prototype for a stateful serverless aspnet api on aws using lamdba and dynamodb provisioned by terraform",
                url = "https://github.com/parameshg/stateful-serverless-api"
            };
        }
    }
}