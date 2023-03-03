using Counter.Repositories;
using EnsureThat;
using FluentValidation;
using MediatR;

namespace Counter.Handlers;

public class GetCounterRequestValidator : AbstractValidator<GetCounterRequest>
{
    public GetCounterRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().Length(1, 32);
    }
}

public class GetCounterRequest : IRequest<GetCounterResponse>
{
    public string Name { get; set; } = "Untitled";
}

public class GetCounterResponse
{
    public int Value { get; set; }

    public bool Status { get; set; }
}

public class GetCounterHandler : IRequestHandler<GetCounterRequest, GetCounterResponse>
{
    private IRepository Repository { get; }

    public GetCounterHandler(IRepository repository)
    {
        Repository = EnsureArg.IsNotNull(repository);
    }

    public async Task<GetCounterResponse> Handle(GetCounterRequest request, CancellationToken token)
    {
        var result = new GetCounterResponse();

        var counter = await Repository.GetCounter(request.Name);

        if (counter != null)
        {
            result.Value = counter.Value;

            result.Status = true;
        }

        return result;
    }
}