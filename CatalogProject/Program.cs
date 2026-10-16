using CatalogProject;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Json;

DotNetEnv.Env.Load();

var  myAllowSpecificOrigins = "_myAllowSpecificOrigins";

try
{
    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.FromLogContext()
        .Enrich.WithProperty("service.name", builder.Environment.ApplicationName)
        .WriteTo.Console(new JsonFormatter())
        .CreateLogger();
    
    builder.Host.UseSerilog();

    var allowedOriginsRaw = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")
                            ?? throw new InvalidOperationException("CorsAllowedOrigins not found.");
    var allowedOrigins = allowedOriginsRaw.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: myAllowSpecificOrigins,
            policy =>
            {
                if (allowedOrigins != null)
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
            });
    });

    builder.Services.AddControllers();
    builder.Services.AddSwaggerGen();

    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                           ?? throw new InvalidOperationException("Connection string not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));

    builder.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddConsole();
        loggingBuilder.AddDebug();
    });

    var app = builder.Build();

    app.UseCors(myAllowSpecificOrigins);

    app.MapControllers();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "The application has been stopped due to an exception.");
}
finally
{
    Log.CloseAndFlush();
}