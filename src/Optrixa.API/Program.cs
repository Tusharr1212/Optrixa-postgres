using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Optrixa.Application;
using Optrixa.Infrastructure;
using Optrixa.Infrastructure.Persistence;
using Optrixa.API.Middleware;
using Serilog;
using System.Text;
using Optrixa.API;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Port binding — Render sets PORT env variable ───────────────
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://+:{port}");

// ── Serilog ────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// ── Controllers + Swagger ──────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Optrixa API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization. Enter: Bearer {token}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── Application + Infrastructure ──────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── JWT Authentication ─────────────────────────────────────────
var tokenSettings = builder.Configuration.GetSection("TokenSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = tokenSettings["Issuer"],
        ValidAudience = tokenSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(tokenSettings["SecretKey"]!))
    };
});

// ── CORS ───────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                // Add your Vercel URL after deployment
                "https://optrixa-ui.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// ── Auto migrate + seed on startup ────────────────────────────
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        // Always run migrations on startup
        db.Database.Migrate();

        // Seed default data
        await DbSeeder.SeedAsync(scope.ServiceProvider);

        Log.Information("Database migrated and seeded successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during database migration/seeding.");
    }
}

// ── Middleware Pipeline ────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Optrixa API v1");
    c.RoutePrefix = "swagger";
});

// Health check endpoint — Render pings this
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("ReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();