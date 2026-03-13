using System.Text;
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

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

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
//builder.Services.AddScoped<SearchService>();
// builder.Services.AddScoped<EmbeddedService>(sp =>
// {
//     var config = sp.GetRequiredService<IConfiguration>();
//     var endpoint = config["EmbeddedService:Endpoint"] 
//         ?? throw new InvalidOperationException("EmbeddedService:Endpoint is not configured.");
//     var apiKey = config["EmbeddedService:ApiKey"] 
//         ?? throw new InvalidOperationException("EmbeddedService:ApiKey is not configured.");
//     return new EmbeddedService(endpoint, apiKey);
// });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
    options.AddPolicy("AllowStaticWebsite", policy => {
    // TODO: Update this to the actual production frontend URL before launch
        policy.WithOrigins("https://white-desert-08300781e.6.azurestaticapps.net")
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    var seederLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DbSeeder.SeedGenresAsync(scope.ServiceProvider, seederLogger);
    await DbSeeder.SeedMoviesAsync(scope.ServiceProvider, seederLogger);
}

app.Run();
