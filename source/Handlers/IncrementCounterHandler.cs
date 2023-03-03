using Counter.Repositories;
using Counter.Requests;
using EnsureThat;
using FluentValidation;
using MediatR;

namespace Counter.Handlers;

public class IncrementCounterRequestValidator : AbstractValidator<IncrementCounterRequest>
{
    public IncrementCounterRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().Length(1, 32);
    }
}

public class IncrementCounterRequest : IRequest<IncrementCounterResponse>
{
    public string Name { get; set;}
}

public class IncrementCounterResponse
{
    public int Value { get; set; }

    public bool Status { get; set; }
}

public class IncrementCounterHandler : IRequestHandler<IncrementCounterRequest, IncrementCounterResponse>
{
    private IRepository Repository { get; }

    public IncrementCounterHandler(IRepository repository)
    {
        Repository = EnsureArg.IsNotNull(repository);
    }

    public async Task<IncrementCounterResponse> Handle(IncrementCounterRequest request, CancellationToken token)
    {
        var result = new IncrementCounterResponse();

        var counter = await Repository.GetCounter(request.Name);

        counter.Value++;

        result.Status = await Repository.UpdateCounter(request.Name, counter.Value);

        counter = await Repository.GetCounter(request.Name);

        if (counter != null)
        {
            result.Value = counter.Value;
        }

        return result;
    }
}