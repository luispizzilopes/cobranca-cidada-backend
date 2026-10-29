using Cobranca.Cidada.Application.Interfaces.Caching;
using Cobranca.Cidada.Domain.Interfaces.Caching;
using MediatR;
using Microsoft.Extensions.Hosting;

namespace Cobranca.Cidada.Application.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cacheService;
    private readonly IHostEnvironment _env;

    public CachingBehavior(ICacheService cacheService, IHostEnvironment env)
    {
        _cacheService = cacheService;
        _env = env;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheableQuery || !_env.IsProduction())
            return await next(cancellationToken);

        var cached = await _cacheService.GetAsync<TResponse>(cacheableQuery.CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var response = await next(cancellationToken);

        await _cacheService.SetAsync(cacheableQuery.CacheKey, response, cacheableQuery.Expiration, cancellationToken);

        return response;
    }
}
