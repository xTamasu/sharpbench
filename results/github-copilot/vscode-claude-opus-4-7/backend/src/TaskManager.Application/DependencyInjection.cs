// DependencyInjection.cs
// Registers Application-layer services and FluentValidation validators into DI.
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Services;

namespace TaskManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ICommentService, CommentService>();

        // Auto-register every AbstractValidator<> in this assembly.
        services.AddValidatorsFromAssemblyContaining<DependencyInjection>();
        return services;
    }
}
