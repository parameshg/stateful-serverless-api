using Counter.Domain;
using Counter.Repositories;
using EnsureThat;
using FluentValidation;
using MediatR;

namespace Counter.Handlers;

public class DecrementCounterRequestValidator : AbstractValidator<DecrementCounterRequest>
{
    public DecrementCounterRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().Length(1, 32);
    }
}

public class DecrementCounterRequest : IRequest<DecrementCounterResponse>
{
    public string Name { get; set;}
}

public class DecrementCounterResponse
{
    public int Value { get; set; }

    public bool Status { get; set; }
}

public class DecrementCounterHandler : IRequestHandler<DecrementCounterRequest, DecrementCounterResponse>
{
    private IRepository Repository { get; }

    public DecrementCounterHandler(IRepository repository)
    {
        Repository = EnsureArg.IsNotNull(repository);
    }

    public async Task<DecrementCounterResponse> Handle(DecrementCounterRequest request, CancellationToken token)
    {
        var result = new DecrementCounterResponse();

        var counter = await Repository.GetCounter(request.Name);

        counter.Value--;

        result.Status = await Repository.UpdateCounter(request.Name, counter.Value);

        counter = await Repository.GetCounter(request.Name);

        if (counter != null)
        {
            result.Value = counter.Value;
        }

        return result;
    }
}