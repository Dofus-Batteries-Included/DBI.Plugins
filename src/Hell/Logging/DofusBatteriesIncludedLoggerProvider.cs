using BepInEx.Logging;
using Microsoft.Extensions.Logging;

namespace DBI.Hell.Logging;

class DofusBatteriesIncludedLoggerProvider : ILoggerProvider
{
    readonly ManualLogSource _log = Logger.CreateLogSource("DBI");

    public ILogger CreateLogger(string categoryName) => new DofusBatteriesIncludedLogger(_log, categoryName);
    public void Dispose() => GC.SuppressFinalize(this);
}
