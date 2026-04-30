// ExceptionHandlingMiddleware.cs
// Translates domain exceptions into RFC-7807 problem-details responses
// with the appropriate HTTP status code.
using System.Text.Json;
using TaskManager.Application.Common;

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

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            ValidationFailedException     => (StatusCodes.Status400BadRequest,  "Validation failed"),
            AuthenticationFailedException => (StatusCodes.Status401Unauthorized,"Authentication failed"),
            UnauthorizedAccessException   => (StatusCodes.Status403Forbidden,   "Forbidden"),
            ForbiddenException            => (StatusCodes.Status403Forbidden,   "Forbidden"),
            KeyNotFoundException          => (StatusCodes.Status404NotFound,    "Not found"),
            NotFoundException             => (StatusCodes.Status404NotFound,    "Not found"),
            ConflictException             => (StatusCodes.Status409Conflict,    "Conflict"),
            _                             => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (status >= 500) _logger.LogError(ex, "Unhandled exception");
        else               _logger.LogWarning(ex, "Handled exception: {Title}", title);

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var payload = JsonSerializer.Serialize(new
        {
            type = $"https://httpstatuses.io/{status}",
            title,
            status,
            detail = ex.Message
        });
        await context.Response.WriteAsync(payload);
    }
}
