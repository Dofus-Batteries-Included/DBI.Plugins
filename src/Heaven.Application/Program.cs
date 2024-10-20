using System.Diagnostics;
using DBI.Heaven.Application.HellInterop.Services;
using DBI.Heaven.Application.Logging;
using DBI.Heaven.Application.Plugins;
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
        Log.Logger.Warning("Exiting...");
        return;
    }


    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog(opt => { opt.ConfigureSerilog().ReadFrom.Configuration(builder.Configuration); });

    string socketDir = builder.Configuration.GetValue<string>("GRPC_SOCKET_DIR") ?? ".";
    string socketNamePrefix = builder.Configuration.GetValue<string>("GRPC_SOCKET_NAME_PREFIX") ?? "";
    string socketPath = Path.Combine(socketDir, $"{socketNamePrefix}{Environment.ProcessId}");
    builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.ListenUnixSocket(socketPath, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; }); });

    builder.Services.AddGrpc();

    builder.Services.ConfigurePlugins();

    WebApplication app = builder.Build();

    app.MapGrpcService<PingHellService>();
    app.MapGrpcService<PluginsHellService>();

    ILogger logger = app.Services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Hello!");
    await app.StartAsync();
    logger.LogInformation("Ready!");
    await app.WaitForShutdownAsync();
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
