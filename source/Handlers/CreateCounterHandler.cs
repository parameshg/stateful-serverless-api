using Counter.Domain;
using Counter.Repositories;
using Counter.Requests;
using EnsureThat;
using FluentValidation;
using MediatR;

namespace Counter.Handlers;

public class CreateCounterRequestValidator : AbstractValidator<CreateRequest>
{
    public CreateCounterRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().Length(1, 32);

        RuleFor(request => request.Value).InclusiveBetween(0, 65535);
    }
}

public class CreateCounterRequest : IRequest<CreateCounterResponse>
{
    public string Name { get; set; } = "Untitled";

    public int Value { get; set; } = 0;
}

public class CreateCounterResponse
{
    public Ticker Counter { get; set; }

    public bool Status { get; set; }
}

public class CreateCounterHandler : IRequestHandler<CreateCounterRequest, CreateCounterResponse>
{
    private IRepository Repository { get; }

    public CreateCounterHandler(IRepository repository)
    {
        Repository = EnsureArg.IsNotNull(repository);
    }

    public async Task<CreateCounterResponse> Handle(CreateCounterRequest request, CancellationToken token)
    {
        var result = new CreateCounterResponse();

        result.Status = await Repository.CreateCounter(request.Name, request.Value);

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