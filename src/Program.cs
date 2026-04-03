using System.Diagnostics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure OpenTelemetry
var serviceName = "ObservabilityDemo";
var serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317");
        }));

// Add HttpClient for potential external calls
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Map the root endpoint
app.MapGet("/", async (IConfiguration config, ILogger<Program> logger) =>
{
    var random = new Random();
    var activity = Activity.Current;
    
    // Get configuration values
    var minDelay = config.GetValue<int>("ApiSettings:MinDelayMs", 0);
    var maxDelay = config.GetValue<int>("ApiSettings:MaxDelayMs", 1000);
    var errorRate = config.GetValue<double>("ApiSettings:ErrorRate", 0.05);
    
    // Add custom attributes to the trace
    activity?.SetTag("dice.min_delay", minDelay);
    activity?.SetTag("dice.max_delay", maxDelay);
    activity?.SetTag("dice.error_rate", errorRate);
    
    // Simulate random delay
    var delay = random.Next(minDelay, maxDelay + 1);
    activity?.SetTag("dice.delay_ms", delay);
    
    if (delay > 0)
    {
        await Task.Delay(delay);
    }
    
    // Simulate random errors
    var shouldError = random.NextDouble() < errorRate;
    
    if (shouldError)
    {
        activity?.SetTag("dice.error", true);
        logger.LogError("Simulated error occurred while rolling the dice");
        return Results.Problem(
            title: "Dice Roll Failed",
            detail: "The dice fell off the table!",
            statusCode: 500
        );
    }
    
    // Generate random number between 1 and 6
    var diceRoll = random.Next(1, 7);
    
    activity?.SetTag("dice.result", diceRoll);
    logger.LogInformation("Dice rolled: {DiceRoll}", diceRoll);
    
    return Results.Ok(new
    {
        Roll = diceRoll,
        Timestamp = DateTime.UtcNow,
        DelayMs = delay
    });
});

app.Run();
