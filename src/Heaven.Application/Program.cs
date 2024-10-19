using System.Diagnostics;
using DBI.Heaven.HellInterop.Services;
using DBI.Heaven.Logging;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

Log.Logger = new LoggerConfiguration().ConfigureSerilog().CreateBootstrapLogger();

try
{
    Process? alreadyRunningMainInstance = FindAlreadyRunningMainInstance();
    if (alreadyRunningMainInstance != null)
    {
        await Console.Error.WriteLineAsync($"main:{alreadyRunningMainInstance.Id}");

        Log.Logger.Warning("Another instance of Heaven has been detected: {Process}.", alreadyRunningMainInstance);
        Log.Logger.Warning("Exitting...");
        return;
    }

    Process currentProcess = Process.GetCurrentProcess();
    string socketPath = Path.Combine(Path.GetTempPath(), $"dbi_server_socket_{currentProcess.Id}.tmp");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog(opt => { opt.ConfigureSerilog().ReadFrom.Configuration(builder.Configuration); });

    builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.ListenUnixSocket(socketPath, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; }); });

    builder.Services.AddGrpc();

    WebApplication app = builder.Build();

    app.MapGrpcService<PingHellService>();

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

Process? FindAlreadyRunningMainInstance()
{
    Process currentProcess = Process.GetCurrentProcess();
    string currentProcessName = currentProcess.ProcessName;
    IEnumerable<Process> processes = Process.GetProcessesByName(currentProcessName).Where(p => p.Id != currentProcess.Id);
    return processes.FirstOrDefault();
}
