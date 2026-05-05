// GlobalExceptionHandlerMiddleware — catches unhandled exceptions and maps them to proper HTTP status codes.
// UnauthorizedAccessException → 403, KeyNotFoundException → 404, others → 500.

using System.Net;
using System.Text.Json;

namespace TaskManager.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred");

        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, errorMessage) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        response.StatusCode = (int)statusCode;

        var errorResponse = new
        {
            status = (int)statusCode,
            error = errorMessage
        };

        var json = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(json);
    }
}
