using CatalogProject;
using CatalogProject.Metrics;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Prometheus.SystemMetrics;

DotNetEnv.Env.Load();

var  myAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSystemMetrics(); 

var allowedOriginsRaw = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")
                     ?? throw new InvalidOperationException("CorsAllowedOrigins not found.");
var metricsPortString = Environment.GetEnvironmentVariable("METRICS_PORT");
if (!int.TryParse(metricsPortString, out var metricsPort))
    throw new InvalidOperationException("Port for metrics not found.");
var allowedOrigins =  allowedOriginsRaw.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy  =>
        {
            if (allowedOrigins != null)
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
        });
});

builder.Services.AddMetricServer(options =>
{
    options.Port = (ushort)metricsPort; 
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<BookMetrics>();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

var app = builder.Build();

app.UseCors(myAllowSpecificOrigins);

app.UseHttpMetrics();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();