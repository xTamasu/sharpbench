// Program.cs — entry point for the ASP.NET Core API.
// Configures DI, JWT auth, CORS, controllers, and middleware pipeline.

using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Application.Validators;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Infrastructure.Services;
using TaskManager.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TaskManagerApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TaskManagerClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(TaskManager.Application.Validators.RegisterRequestValidator).Assembly);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Run EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Middleware pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
