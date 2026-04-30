// Global exception handling middleware: maps domain exceptions to HTTP status codes.
using System.Net;
using System.Text.Json;

namespace TaskManager.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            // 403 Forbidden for ownership/authorization violations
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, exception.Message),
            // 404 Not Found for missing entities
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            // 409 Conflict for duplicate registrations
            InvalidOperationException => (HttpStatusCode.Conflict, exception.Message),
            // 400 Bad Request for validation failures
            FluentValidation.ValidationException validationEx =>
                (HttpStatusCode.BadRequest, string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))),
            // 500 for everything else
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(new { error = message, statusCode = (int)statusCode });
        await context.Response.WriteAsync(response);
    }
}
