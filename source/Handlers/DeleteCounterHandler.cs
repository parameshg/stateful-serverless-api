using Counter.Repositories;
using Counter.Requests;
using EnsureThat;
using FluentValidation;
using MediatR;

namespace Counter.Handlers;

public class DeleteCounterRequestValidator : AbstractValidator<DeleteCounterRequest>
{
    public DeleteCounterRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().Length(1, 32);
    }
}

public class DeleteCounterRequest : IRequest<DeleteCounterResponse>
{
    public string Name { get; set;}
}

public class DeleteCounterResponse
{
    public bool Status { get; set; }
}

public class DeleteCounterHandler : IRequestHandler<DeleteCounterRequest, DeleteCounterResponse>
{
    private IRepository Repository { get; }

    public DeleteCounterHandler(IRepository repository)
    {
        Repository = EnsureArg.IsNotNull(repository);
    }

    public async Task<DeleteCounterResponse> Handle(DeleteCounterRequest request, CancellationToken token)
    {
        var result = new DeleteCounterResponse();

        result.Status = await Repository.DeleteCounter(request.Name);

        return result;
    }
}