using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EcommerceCatalog.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that automatically logs every command and query.
/// Registered once in Program.cs — no logging needed in any controller or handler.
/// </summary>
public class LoggingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        // ── Log the incoming request ──────────────────────────────────────────
        _logger.LogInformation(
            "Handling {RequestName} {@Request}",
            requestName,
            request);

        try
        {
            var response = await next();
            stopwatch.Stop();

            // ── Log successful completion with how long it took ───────────────
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // ── Log the failure with full exception details ───────────────────
            // This is the ONLY place errors need to be logged from handlers
            _logger.LogError(ex,
                "Error handling {RequestName} after {ElapsedMilliseconds}ms. Request data: {@Request}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                request);

            // Re-throw so GlobalExceptionMiddleware can return correct HTTP response
            throw;
        }
    }
}
