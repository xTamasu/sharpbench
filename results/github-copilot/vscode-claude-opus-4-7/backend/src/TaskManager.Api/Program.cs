// Program.cs
// Application bootstrap. Wires up DI, JWT auth, controllers, Swagger,
// the global exception middleware, runs EF Core migrations, and seeds the demo user.
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskManager.Api.Auth;
using TaskManager.Api.Middleware;
using TaskManager.Application;
using TaskManager.Application.Common;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// ----- Configuration & options -----
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

// ----- DI -----
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUserAccessor>();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskManager API", Version = "v1" });
    // Enable "Authorize" button in Swagger UI.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

// ----- Authentication -----
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false; // Behind nginx in dev/Docker; HTTPS terminated upstream in prod.
        o.SaveToken = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ----- Apply migrations & seed (with retry, since the DB may still be starting) -----
await ApplyMigrationsWithRetryAsync(app);

// ----- Pipeline -----
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.Run();

// Local helper kept inside Program.cs to avoid an extra one-off type.
static async Task ApplyMigrationsWithRetryAsync(WebApplication app)
{
    // Even though docker-compose's healthcheck waits for pg_isready, we still
    // retry briefly in case Postgres accepts connections before the role/db exist.
    const int maxAttempts = 15;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await DbSeeder.SeedAsync(db, hasher);
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            app.Logger.LogWarning(ex, "Database not ready yet (attempt {Attempt}/{Max}). Retrying...", attempt, maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}

// Required so WebApplicationFactory<Program> works in integration tests if added later.
public partial class Program { }
