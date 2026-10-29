using Cobranca.Cidada.Domain.Exceptions;
using MediatR;

namespace Cobranca.Cidada.Application.Behaviors;

public class RetryOnConcurrencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int MaxAttempts = 3;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt < MaxAttempts; attempt++)
        {
            try
            {
                return await next(cancellationToken);
            }
            catch (ConcurrencyConflictException)
            {
            }
        }

        return await next(cancellationToken);
    }
}
