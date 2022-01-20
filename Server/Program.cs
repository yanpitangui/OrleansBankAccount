using Contracts;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Serilog;
using Orleans;
using Orleans.Hosting;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;
using Server;
using Server.Grains;

var builder = WebApplication.CreateBuilder(args);

// remove default logging providers
builder.Logging.ClearProviders();
// Serilog configuration        
var logger = ConfigureBaseLogging()
    .CreateLogger();

// Register Serilog
builder.Logging.AddSerilog(logger);


// Register orleans
builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder
        .UseLocalhostClustering()
        .AddMemoryGrainStorageAsDefault()
        .UseTransactions();

    siloBuilder.ConfigureApplicationParts(parts =>
    {
        parts.AddApplicationPart(typeof(AccountGrain).Assembly).WithReferences();
    });

    siloBuilder.UseDashboard(options =>
    {
        options.Port = 9000;
    });
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ErrorHandlingMiddleware>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware(typeof(ErrorHandlingMiddleware));
await app.RunAsync();

LoggerConfiguration ConfigureBaseLogging()
{
    var loggerConfiguration = new LoggerConfiguration();
    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing") return loggerConfiguration.MinimumLevel.Fatal();
    loggerConfiguration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Orleans", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Infrastructure", LogEventLevel.Warning)
        .Destructure.AsScalar<JObject>()
        .Destructure.AsScalar<JArray>()
        .WriteTo.Async(a => a.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code))
        .Enrich.WithExceptionDetails()
        .Enrich.FromLogContext();

    return loggerConfiguration;
}