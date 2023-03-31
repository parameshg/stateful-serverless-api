using Api.Domain;
using Api.Handlers;
using Api.Requests;
using EnsureThat;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("")]
    public class HomeController : ControllerBase
    {
        private IMediator Mediator { get; }

        public HomeController(IMediator mediator)
        {
            Mediator = EnsureArg.IsNotNull(mediator);
        }

        [HttpGet("")]
        public object Get()
        {
            return new
            {
                timestamp = DateTime.Now,
                name = "Counter Api - Number as a Service",
                description = "This api allows you to create simple numeric counters. You can create a counter, update the value of a counter, increment/decrement a counter and delete a counter. All counters are accesible if you know the key and there are no private counters.",
            };
        }

        [HttpGet("integer/{name}")]
        public async Task<int> Get(ViewRequest request)
        {
            var result = 0;

            if (ModelState.IsValid)
            {
                var response = await Mediator.Send(new GetCounterRequest { Name = request.Name });

                if (response != null && response.Status)
                {
                    result = response.Value;
                }
            }

            return result;
        }

        [HttpGet("integer/{name}/up")]
        public async Task<int> Increment(IncrementRequest request)
        {
            var result = 0;

            if (ModelState.IsValid)
            {
                var response = await Mediator.Send(new IncrementCounterRequest { Name = request.Name });

                if (response != null && response.Status)
                {
                    result = response.Value;
                }
            }

            return result;
        }

        [HttpGet("integer/{name}/down")]
        public async Task<int> Decrement(DecrementRequest request)
        {
            var result = 0;

            if (ModelState.IsValid)
            {
                var response = await Mediator.Send(new DecrementCounterRequest { Name = request.Name });

                if (response != null && response.Status)
                {
                    result = response.Value;
                }
            }

            return result;
        }

        [HttpPost("integer")]
        public async Task<Ticker> Post([FromBody] CreateRequest request)
        {
            Ticker result = null;

            if (ModelState.IsValid)
            {
                var response = await Mediator.Send(new CreateCounterRequest { Name = request.Name, Value = request.Value });

                if (response != null && response.Status)
                {
                    result = response.Counter;
                }
            }

            return result;
        }

        [HttpPut("integer")]
        public async Task<Ticker> Put([FromBody] UpdateRequest request)
        {
            Ticker result = null;

            if (ModelState.IsValid)
            {
                var response = await Mediator.Send(new UpdateCounterRequest { Name = request.Name, Value = request.Value });

                if (response != null && response.Status)
                {
                    result = response.Counter;
                }
            }

            return result;
        }

        [HttpDelete("integer/{name}")]
        public async Task<bool> Delete(DeleteRequest request)
        {
            var result = false;

            if (ModelState.IsValid)
            {
                var response = await Mediator.Send(new DeleteCounterRequest { Name = request.Name });

                if (response != null)
                {
                    result = response.Status;
                }
            }

            return result;
        }
    }
}