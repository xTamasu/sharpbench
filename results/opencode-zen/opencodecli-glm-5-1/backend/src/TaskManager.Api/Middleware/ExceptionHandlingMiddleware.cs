// Global exception handling middleware that maps domain exceptions to HTTP status codes
using System.Net;
using System.Text.Json;
using TaskManager.Application.Exceptions;

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException _ => (HttpStatusCode.NotFound, "Not Found"),
            ForbiddenException _ => (HttpStatusCode.Forbidden, "Forbidden"),
            UnauthorizedAccessException _ => (HttpStatusCode.Forbidden, "Forbidden"),
            KeyNotFoundException _ => (HttpStatusCode.NotFound, "Not Found"),
            BadRequestException _ => (HttpStatusCode.BadRequest, "Bad Request"),
            FluentValidation.ValidationException validationEx =>
                (HttpStatusCode.BadRequest, string.Join(", ", validationEx.Errors.Select(e => e.ErrorMessage))),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");
        else
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new { status = (int)statusCode, title, detail = exception.Message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}