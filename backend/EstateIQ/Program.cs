using System.Reflection;
using AutoMapper;
using EstateIQ.Data;
using EstateIQ.Interfaces;
using EstateIQ.Mappings;
using EstateIQ.Repositories;
using EstateIQ.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

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
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPropertyTypeRepository, PropertyTypeRepository>();
builder.Services.AddScoped<IPropertyStatusRepository, PropertyStatusRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IAgentRepository, AgentRepository>();
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

app.MapControllers();

app.MapGet("/api/test", () => "API is running")
    .WithName("GetApiTest");

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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public partial class Program;
