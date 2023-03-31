using Api.Domain;
using Api.Repositories;
using Api.Requests;
using EnsureThat;
using FluentValidation;
using MediatR;

namespace Api.Handlers
{
    public class UpdateCounterRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateCounterRequestValidator()
        {
            RuleFor(request => request.Name).NotEmpty().Length(1, 32);
        }
    }

    public class UpdateCounterRequest : IRequest<UpdateCounterResponse>
    {
        public string Name { get; set; } = "Untitled";

        public int Value { get; set; } = 0;
    }

    public class UpdateCounterResponse
    {
        public Ticker Counter { get; set; }

        public bool Status { get; set; }
    }

    public class UpdateCounterHandler : IRequestHandler<UpdateCounterRequest, UpdateCounterResponse>
    {
        private IRepository Repository { get; }

        public UpdateCounterHandler(IRepository repository)
        {
            Repository = EnsureArg.IsNotNull(repository);
        }

        public async Task<UpdateCounterResponse> Handle(UpdateCounterRequest request, CancellationToken token)
        {
            var result = new UpdateCounterResponse();

            result.Status = await Repository.UpdateCounter(request.Name, request.Value);

            var counter = await Repository.GetCounter(request.Name);

            if (counter != null)
            {
                result.Counter = new Ticker
                {
                    Name = request.Name,
                    Value = counter.Value
                };
            }

            return result;
        }
    }
}