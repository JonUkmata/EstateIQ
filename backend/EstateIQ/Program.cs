using System.Reflection;
using AutoMapper;
using EstateIQ.Data;
using EstateIQ.Constants;
using EstateIQ.Interfaces;
using EstateIQ.Mappings;
using EstateIQ.Models;
using EstateIQ.Repositories;
using EstateIQ.Services;
using EstateIQ.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;

EnvironmentFileLoader.Load(Directory.GetCurrentDirectory());

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    if (builder.Environment.IsEnvironment("Testing"))
    {
        connectionString = "Server=(localdb)\\mssqllocaldb;Database=EstateIQTests;Trusted_Connection=True;TrustServerCertificate=True";
    }
    else
    {
        throw new InvalidOperationException(
            "Connection string 'DefaultConnection' was not found. Set ConnectionStrings__DefaultConnection in backend/EstateIQ/.env or as an environment variable.");
    }
}

var redisConnectionString = builder.Configuration["Redis:ConnectionString"];

if (string.IsNullOrWhiteSpace(redisConnectionString))
{
    if (builder.Environment.IsEnvironment("Testing"))
    {
        redisConnectionString = "localhost:6379";
    }
    else
    {
        throw new InvalidOperationException(
            "Redis connection string was not found. Set Redis__ConnectionString in backend/EstateIQ/.env or as an environment variable.");
    }
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddControllers();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings are not configured.");
ValidateJwtSettings(jwtSettings);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPropertyTypeRepository, PropertyTypeRepository>();
builder.Services.AddScoped<IPropertyTypeService, PropertyTypeService>();
builder.Services.AddScoped<IPropertyStatusRepository, PropertyStatusRepository>();
builder.Services.AddScoped<IPropertyStatusService, PropertyStatusService>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IAgentRepository, AgentRepository>();
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IAgentCompanyRepository, AgentCompanyRepository>();
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var options = ConfigurationOptions.Parse(redisConnectionString);
    options.AbortOnConnectFail = false;

    return ConnectionMultiplexer.Connect(options);
});
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);

    if (File.Exists(xmlFilePath))
    {
        options.IncludeXmlComments(xmlFilePath, includeControllerXmlComments: true);
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT bearer token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

var app = builder.Build();
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await PropertyTypeSeeder.SeedRequiredPropertyTypesAsync(dbContext);
    await PropertyStatusSeeder.SeedRequiredPropertyStatusesAsync(dbContext);
    await CompanySeeder.SeedRequiredCompaniesAsync(dbContext);
    await AgentCompanySeeder.SeedRequiredAgentsAndRelationshipsAsync(dbContext);
    await PropertySeeder.SeedRequiredPropertiesAsync(dbContext);
}

if (!app.Environment.IsEnvironment("Testing"))
{
    try
    {
        var redisMultiplexer = app.Services.GetRequiredService<IConnectionMultiplexer>();
        var redisPing = await redisMultiplexer.GetDatabase().PingAsync();
        startupLogger.LogInformation("Redis connection successful. Ping: {RedisPingMs} ms", redisPing.TotalMilliseconds);
    }
    catch (Exception exception)
    {
        startupLogger.LogWarning(exception, "Redis connection failed during startup. The API will continue to run, but Redis-backed operations may fail until Redis is available.");
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/test", () => "API is running")
    .WithName("GetApiTest");

app.MapGet("/api/test/protected", (ClaimsPrincipal user) => Results.Ok(new
{
    userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
}))
.RequireAuthorization()
.WithName("GetProtectedApiTest");

app.MapGet("/api/test/admin", () => "Admin access granted")
    .RequireAuthorization(policy => policy.RequireRole(Roles.Admin))
    .WithName("GetAdminAuthorizationTest");

app.MapGet("/api/test/db", async (AppDbContext dbContext) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync();
    return canConnect
        ? Results.Ok("Database connection successful")
        : Results.Problem("Database connection failed", statusCode: StatusCodes.Status503ServiceUnavailable);
})
.WithName("GetDatabaseConnectionTest");

app.MapGet("/api/test/redis", async (IRedisCacheService redisCacheService, ILoggerFactory loggerFactory) =>
{
    const string key = "test_key";
    const string expectedValue = "hello";

    var logger = loggerFactory.CreateLogger("RedisTest");

    await redisCacheService.SetStringAsync(key, expectedValue);
    var storedValue = await redisCacheService.GetStringAsync(key);

    if (!string.Equals(storedValue, expectedValue, StringComparison.Ordinal))
    {
        logger.LogError("Redis set/get verification failed for key {RedisKey}. Expected {ExpectedValue} but got {StoredValue}.", key, expectedValue, storedValue);
        return Results.Problem("Redis set/get verification failed", statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    logger.LogInformation("Redis set/get verification succeeded for key {RedisKey}.", key);

    return Results.Ok(new
    {
        key,
        value = storedValue,
        success = true
    });
})
.WithName("GetRedisConnectionTest");

app.Run();

static void ValidateJwtSettings(JwtSettings jwtSettings)
{
    if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
    {
        throw new InvalidOperationException("JWT issuer is not configured.");
    }

    if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
    {
        throw new InvalidOperationException("JWT audience is not configured.");
    }

    if (string.IsNullOrWhiteSpace(jwtSettings.Key))
    {
        throw new InvalidOperationException("JWT key is not configured.");
    }

    if (Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32)
    {
        throw new InvalidOperationException("JWT key must be at least 32 bytes long.");
    }
}

public partial class Program;
