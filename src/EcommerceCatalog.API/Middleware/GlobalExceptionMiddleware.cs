using FluentValidation;
using System.Net;
using System.Text.Json;

namespace EcommerceCatalog.API.Middleware;

/// <summary>
/// Catches all unhandled exceptions and returns a consistent JSON error response.
/// Logging here is intentionally minimal — LoggingBehaviour already logged
/// the full exception with request data. Here we just log the HTTP outcome.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            // FluentValidation failures — 400 Bad Request
            _logger.LogWarning(
                "Validation failed for {Path}: {@Errors}",
                context.Request.Path,
                ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            await WriteErrorResponse(context, HttpStatusCode.BadRequest, "Validation failed.", errors);
        }
        catch (KeyNotFoundException ex)
        {
            // Resource not found — 404
            _logger.LogWarning(
                "Resource not found at {Path}: {Message}",
                context.Request.Path,
                ex.Message);

            await WriteErrorResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ArgumentException ex)
        {
            // Business rule violation — 400
            _logger.LogWarning(
                "Bad request at {Path}: {Message}",
                context.Request.Path,
                ex.Message);

            await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            // Unexpected error — 500
            // Note: LoggingBehaviour already logged the full exception with stack trace
            // Here we just log that a 500 was returned so we can correlate HTTP logs
            _logger.LogError(ex,
                "Unhandled exception at {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorResponse(context, HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteErrorResponse(
        HttpContext context,
        HttpStatusCode statusCode,
        string message,
        object? errors = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(new
        {
            status = (int)statusCode,
            error = message,
            errors,
            timestamp = DateTime.UtcNow
        }, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });

        await context.Response.WriteAsync(response);
    }
}
