using System.Diagnostics;
using DBI.Heaven.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

Log.Logger = new LoggerConfiguration().ConfigureSerilog().CreateBootstrapLogger();


try
{
    if (AnotherInstanceIsRunning())
    {
        Log.Logger.Warning("Another instance of Heaven has been detected, this instance will exit.");
        return;
    }

    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog(opt => { opt.ConfigureSerilog().ReadFrom.Configuration(builder.Configuration); });

    IHost app = builder.Build();

    ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();


    logger.LogInformation("Hello!");
    await app.RunAsync();
    Log.Logger.Information("Bye!");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

return;

bool AnotherInstanceIsRunning()
{
    Process currentProcess = Process.GetCurrentProcess();
    string currentProcessName = currentProcess.ProcessName;
    Process[] processes = Process.GetProcessesByName(currentProcessName).Where(p => p.Id != currentProcess.Id).ToArray();
    return processes.Length > 0;
}
