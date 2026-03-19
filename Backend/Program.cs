using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// Load .env file from the solution root (one level above the Backend folder)
var envFile = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
Console.WriteLine($"DIAG: Working dir={Directory.GetCurrentDirectory()}");
Console.WriteLine($"DIAG: Raw env ConnectionStrings__Default={(Environment.GetEnvironmentVariable("ConnectionStrings__Default") ?? "(not set)")}");
if (File.Exists(envFile))
{
    foreach (var line in File.ReadAllLines(envFile))
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;
        var idx = trimmed.IndexOf('=');
        if (idx <= 0) continue;
        var envKey = trimmed[..idx].Trim();
        var value = trimmed[(idx + 1)..].Trim();
        Environment.SetEnvironmentVariable(envKey, value);
    }
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Identity
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT
var jwtConfig = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);

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
        ValidIssuer = jwtConfig["Issuer"],
        ValidAudience = jwtConfig["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero,
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<TMDBService>();

// AI / Search services — keyed off .env variables
builder.Services.AddSingleton<ChatService>(_ =>
    new ChatService(Environment.GetEnvironmentVariable("GEMINI_API_KEY")!));

builder.Services.AddSingleton<EmbeddedService>(_ =>
    new EmbeddedService(
        Environment.GetEnvironmentVariable("AZURE_VISION_ENDPOINT")!,
        Environment.GetEnvironmentVariable("AZURE_VISION_KEY_1")!));

builder.Services.AddSingleton<SearchService>(_ =>
{
    // AZURE_SEARCH_URL format: https://{serviceName}.search.windows.net
    var searchUrl = Environment.GetEnvironmentVariable("AZURE_SEARCH_URL")!;
    var serviceName = new Uri(searchUrl).Host.Replace(".search.windows.net", "");
    return new SearchService(
        serviceName,
        Environment.GetEnvironmentVariable("AZURE_SEARCH_INDEX_NAME")!,
        Environment.GetEnvironmentVariable("AZURE_SEARCH_API_KEY")!);
});

var configuredCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

var envCorsOrigins = (Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

var explicitAllowedOrigins = configuredCorsOrigins
    .Concat(envCorsOrigins)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
    options.AddPolicy("AllowStaticWebsite", policy => {
        policy.SetIsOriginAllowed(origin =>
              {
                  if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
                  var host = uri.Host;
                  // Allow any localhost port for development
                  if (host == "localhost" || host == "127.0.0.1") return true;
                  // Allow Azure Static Web Apps (both URL formats)
                  if (host.Equals("azurestaticapps.net", StringComparison.OrdinalIgnoreCase)) return true;
                  if (host.EndsWith(".azurestaticapps.net", StringComparison.OrdinalIgnoreCase)) return true;
                  // Allow explicitly configured production frontend origins
                  if (explicitAllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase)) return true;
                  return false;
              })
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// TEMP: log connection string source for debugging
var connStr = builder.Configuration.GetConnectionString("Default") ?? "(null)";
var masked = connStr.Length > 30 ? connStr[..30] + "..." : connStr;
app.Logger.LogWarning("DIAG: ConnectionStrings:Default starts with: {ConnStr}", masked);

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors("AllowStaticWebsite");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Diagnostic: log mapped controller endpoints to help debug missing routes in prod
try
{
    var provider = app.Services.GetRequiredService<Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider>();
    foreach (var ad in provider.ActionDescriptors.Items)
    {
        var cad = ad as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        app.Logger.LogInformation("Mapped endpoint: {Name} — {Route}",
            ad.DisplayName,
            cad?.AttributeRouteInfo?.Template ?? "(conventional)");
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Failed to enumerate action descriptors for diagnostic logging.");
}

var runStartupTasks = builder.Configuration.GetValue("StartupTasks:RunOnStartup", app.Environment.IsDevelopment());

if (runStartupTasks)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        var seederLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        var searchService = scope.ServiceProvider.GetRequiredService<SearchService>();
        await searchService.EnsureIndexAsync();

        await DbSeeder.SeedGenresAsync(scope.ServiceProvider, seederLogger);
        await DbSeeder.SeedMoviesAsync(scope.ServiceProvider, seederLogger);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Startup tasks failed. API will continue to run, but migrations/indexing/seeding did not complete.");
    }
}
else
{
    app.Logger.LogInformation("Startup tasks are disabled (StartupTasks:RunOnStartup=false).");
}

app.Run();
