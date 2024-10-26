using Serilog;
using Serilog.Events;

namespace DBI.Heaven.Application.Logging;

static class SerilogAspNetExtensions
{
#if DEBUG
    const LogEventLevel DefaultLoggingLevel = LogEventLevel.Debug;
#else
    const LogEventLevel DefaultLoggingLevel = LogEventLevel.Information;
#endif

    const LogEventLevel InfrastructureLoggingLevel = LogEventLevel.Information;
    const string ConsoleTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} ({SourceContext}){NewLine}{Exception}";
    const string FileTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}";

    public static LoggerConfiguration ConfigureSerilog(this LoggerConfiguration configuration)
    {
        string thisAssemblyPath = System.AppContext.BaseDirectory;
        string? thisAssemblyDirectory = Path.GetDirectoryName(thisAssemblyPath);
        string logFile = Path.Join(thisAssemblyDirectory, "log", "Heaven.log");

        return configuration.WriteTo.Console(outputTemplate: ConsoleTemplate)
            .WriteTo.File(logFile, outputTemplate: FileTemplate, shared: true)
            .Enrich.WithProperty("SourceContext", "Bootstrap")
            .MinimumLevel.Is(DefaultLoggingLevel)
            .MinimumLevel.Override("System.Net.Http.HttpClient", InfrastructureLoggingLevel)
            .MinimumLevel.Override("Microsoft.Extensions.Http", InfrastructureLoggingLevel)
            .MinimumLevel.Override("Microsoft.AspNetCore", InfrastructureLoggingLevel)
            .MinimumLevel.Override("Microsoft.Identity", InfrastructureLoggingLevel)
            .MinimumLevel.Override("Microsoft.IdentityModel", InfrastructureLoggingLevel);
    }
}
