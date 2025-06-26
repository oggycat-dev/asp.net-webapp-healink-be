using MediatR;
using Microsoft.Extensions.Logging;

namespace Healink.Application.Common.Behaviors;

/// <summary>
/// Unhandled exception behavior for MediatR pipeline
/// </summary>
public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;

    public UnhandledExceptionBehavior(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(ex, "Healink Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);

            throw;
        }
    }
} 