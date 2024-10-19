using DBI.Heaven.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

Log.Logger = new LoggerConfiguration().ConfigureSerilog().CreateBootstrapLogger();

try
{
    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog(opt => { opt.ConfigureSerilog().ReadFrom.Configuration(builder.Configuration); });

    IHost app = builder.Build();

    ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Hello world!");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
